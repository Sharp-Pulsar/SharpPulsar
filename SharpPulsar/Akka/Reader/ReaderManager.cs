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
        public ReaderManager(ClientConfigurationData configuration, IActorRef network)
        {
            _network = network;
            _config = configuration;
            Become(() => Init(configuration));
        }
        public static Props Prop(ClientConfigurationData clientConfiguration, IActorRef network)
        {
            return Props.Create(() => new ReaderManager(clientConfiguration, network));
        }
        private void Open()
        {
            Receive<NewReader>(NewReader);
            Stash.UnstashAll();
        }

        private void NewReader(NewReader reader)
        {
            var schema = reader.ReaderConfiguration.Schema;
            var clientConfig = reader.Configuration;
            var readerConfig = reader.ReaderConfiguration;
            Context.ActorOf(Reader.Prop(clientConfig, readerConfig, _network));
        }
        
        private void Init(ClientConfigurationData configuration)
        {
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
