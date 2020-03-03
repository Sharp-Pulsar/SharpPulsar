using System;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.Network;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Reader
{
    public class ReaderManager:ReceiveActor, IWithUnboundedStash
    {
        private IActorRef _network;
        private ClientConfigurationData _config;
        public ReaderManager(ClientConfigurationData configuration)
        {
            _config = configuration;
            Become(() => Init(configuration));
        }
        public static Props Prop(ClientConfigurationData clientConfiguration)
        {
            return Props.Create(() => new ReaderManager(clientConfiguration));
        }
        private void Open()
        {
            Receive<NewReader>(NewReader);
            Receive<TcpClosed>(_ =>
            {
                Become(Connecting);
            });
            Stash.UnstashAll();
        }

        private void NewReader(NewReader reader)
        {
            var schema = reader.ReaderConfiguration.Schema;
            var clientConfig = reader.Configuration;
            var readerConfig = reader.ReaderConfiguration;
            Context.ActorOf(Reader.Prop(clientConfig, readerConfig, _network));
        }
        private void Connecting()
        {
            _network.Tell(new TcpReconnect());
            Receive<TcpSuccess>(s =>
            {
                Console.WriteLine($"Pulsar handshake completed with {s.Name}");
                Become(Open);
            });
            ReceiveAny(m =>
            {
                Stash.Stash();
            });
        }
        private void Init(ClientConfigurationData configuration)
        {
            _network = Context.ActorOf(NetworkManager.Prop(Self, configuration), "NetworkManager");
            Receive<TcpSuccess>(s =>
            {
                Console.WriteLine($"Pulsar handshake completed with {s.Name}");
                Become(Open);
            });
            ReceiveAny(_ => Stash.Stash());
        }
        public IStash Stash { get; set; }
    }
}
