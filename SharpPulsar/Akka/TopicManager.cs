using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Producer;
using SharpPulsar.Akka.Reader;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka
{
    public class TopicManager: ReceiveActor
    {
        public TopicManager(ClientConfigurationData configuration, IActorRef network)
        {
            Context.ActorOf(ProducerManager.Prop(configuration, network), "ProducerManager");
            Context.ActorOf(ReaderManager.Prop(configuration, network), "ReaderManager");
            Receive<NewProducer>(cmd =>
            {
                Context.Child("ProducerManager").Tell(cmd);
            });
            
            Receive<NewReader>(cmd =>
            {
                Context.Child("ReaderManager").Tell(cmd);
            });
        }

        public static Props Prop(ClientConfigurationData configuration, IActorRef network)
        {
            return Props.Create(() => new TopicManager(configuration, network));
        }
    }
}
