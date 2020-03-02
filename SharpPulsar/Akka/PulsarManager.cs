using Akka.Actor;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Producer;
using SharpPulsar.Akka.Reader;
using SharpPulsar.Akka.Transaction;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka
{
    public class PulsarManager:ReceiveActor
    {
        public PulsarManager(ClientConfigurationData conf)
        {
            Context.ActorOf(ProducerManager.Prop(conf), "ProducerManager");
            Context.ActorOf(ConsumerManager.Prop(conf), "ConsumerManager");
            Context.ActorOf(ReaderManager.Prop(conf), "ReaderManager");
            Context.ActorOf(TransactionManager.Prop(), "TransactionManager");
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
