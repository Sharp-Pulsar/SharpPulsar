using Akka.Actor;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.Producer;
using SharpPulsar.Akka.Reader;
using SharpPulsar.Akka.Transaction;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka
{
    public class PulsarManager:ReceiveActor
    {
        private ClientConfigurationData _configuration;
        public PulsarManager(ClientConfigurationData conf)
        {
            _configuration = conf;
            Context.ActorOf(ProducerManager.Prop(), "ProducerManager");
            Context.ActorOf(ConsumerManager.Prop(), "ConsumerManager");
            Context.ActorOf(ReaderManager.Prop(), "ReaderManager");
            Context.ActorOf(TransactionManager.Prop(), "TransactionManager");
        }

        public static Props Prop(ClientConfigurationData conf)
        {
            return Props.Create(()=> new PulsarManager(conf));
        }
    }
}
