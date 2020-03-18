using Akka.Actor;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Akka.Producer;
using SharpPulsar.Akka.Reader;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka
{
    public class PulsarManager:ReceiveActor, IWithUnboundedStash
    {
        private IActorRef _network;
        private ClientConfigurationData _config;
        public PulsarManager(ClientConfigurationData conf)
        {
            _config = conf;
            Become(NetworkSetup);
        }

        private void Ready()
        {
            Receive<NewProducer>(cmd =>
            {
                Context.Child("ProducerManager").Tell(cmd);
            });
            Receive<NewConsumer>(cmd =>
            {
                Context.Child("ConsumerManager").Tell(cmd);
            });
            Receive<NewReader>(cmd =>
            {
                Context.Child("ReaderManager").Tell(cmd);
            });
            Receive<UpdateService>(u =>
            {
                foreach (var c in Context.GetChildren())
                {
                    //c.Tell(u);
                }
            });
        }
        
        private void NetworkSetup()
        {
            _network = Context.ActorOf(NetworkManager.Prop(Self, _config), "NetworkManager");
            Receive<ConnectedServerInfo>(s =>
            {
                Context.ActorOf(ProducerManager.Prop(_config, _network), "ProducerManager");
                Context.ActorOf(ConsumerManager.Prop(_config, _network), "ConsumerManager");
                Context.ActorOf(ReaderManager.Prop(_config, _network), "ReaderManager");
                Become(Ready);
                Stash.UnstashAll();
            });
            ReceiveAny(c=> Stash.Stash());
        }
        public static Props Prop(ClientConfigurationData conf)
        {
            return Props.Create(()=> new PulsarManager(conf));
        }

        public IStash Stash { get; set; }
    }
}
