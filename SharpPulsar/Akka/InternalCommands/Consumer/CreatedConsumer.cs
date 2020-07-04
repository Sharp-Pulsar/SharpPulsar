using Akka.Actor;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public class CreatedConsumer
    {
        public CreatedConsumer(IActorRef consumer, string topic, string consumerName)
        {
            Consumer = consumer;
            Topic = topic;
            ConsumerName = consumerName;
        }

        public string ConsumerName { get; }
        public IActorRef Consumer { get; }
        public string Topic { get; }
    }
}
