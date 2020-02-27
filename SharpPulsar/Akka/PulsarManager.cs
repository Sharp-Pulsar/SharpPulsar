using Akka.Actor;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.Producer;
using SharpPulsar.Akka.Reader;
using SharpPulsar.Akka.Transaction;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka
{
    public class PulsarManager:ReceiveActor
    {
        public PulsarManager(ClientConfigurationData conf)
        {
            Context.ActorOf(ProducerManager.Prop(conf), "ProducerManager");
            Context.ActorOf(ConsumerManager.Prop(), "ConsumerManager");
            Context.ActorOf(ReaderManager.Prop(), "ReaderManager");
            Context.ActorOf(TransactionManager.Prop(), "TransactionManager");
            Receive<NewProducer>(cmd =>
            {
                Context.Child("ProducerManager").Tell(cmd);
            });
            Receive<UpdateService>(u =>
            {
                foreach (var c in Context.GetChildren())
                {
                    c.Tell(u);
                }
            });
        }

        public static Props Prop(ClientConfigurationData conf)
        {
            return Props.Create(()=> new PulsarManager(conf));
        }
    }
}
