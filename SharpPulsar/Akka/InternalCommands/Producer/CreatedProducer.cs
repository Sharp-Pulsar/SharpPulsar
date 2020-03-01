using Akka.Actor;

namespace SharpPulsar.Akka.InternalCommands.Producer
{
    public class CreatedProducer
    {
        public CreatedProducer(IActorRef producer, string topic)
        {
            Producer = producer;
            Topic = topic;
        }

        public IActorRef Producer { get; }
        public string Topic { get; }
    }
}
