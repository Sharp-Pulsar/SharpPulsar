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
        public TopicManager(ClientConfigurationData configuration, IActorRef network, IActorRef pulsarManager)
        {
            var producer = Context.ActorOf(ProducerManager.Prop(configuration, network, pulsarManager), "ProducerManager");
            Context.ActorOf(ReaderManager.Prop(configuration, network, pulsarManager), "ReaderManager");
            Context.ActorOf(ProducerBroadcastGroup.Prop(producer, pulsarManager), "ProducerBroadcastManager");
            
            Receive<NewProducer>(cmd =>
            {
                Context.Child("ProducerManager").Forward(cmd);
            });
            Receive<NewProducerBroadcastGroup>(cmd =>
            {
                Context.Child("ProducerBroadcastManager").Tell(cmd);
            });
            Receive<NewReader>(cmd =>
            {
                Context.Child("ReaderManager").Forward(cmd);
            });
        }

        public static Props Prop(ClientConfigurationData configuration, IActorRef network, IActorRef pulsarManager)
        {
            return Props.Create(() => new TopicManager(configuration, network, pulsarManager));
        }
    }
}
