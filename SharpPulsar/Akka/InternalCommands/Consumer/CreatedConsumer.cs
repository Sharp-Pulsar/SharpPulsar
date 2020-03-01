using Akka.Actor;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public class CreatedConsumer
    {
        public CreatedConsumer(IActorRef consumer, string topic)
        {
            Consumer = consumer;
            Topic = topic;
        }

        public IActorRef Consumer { get; }
        public string Topic { get; }
    }
}
