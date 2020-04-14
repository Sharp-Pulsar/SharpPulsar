using Akka.Actor;

namespace SharpPulsar.Akka.InternalCommands.Producer
{
    public class CreatedProducer
    {
        public CreatedProducer(IActorRef producer, string topic, string name)
        {
            Producer = producer;
            Topic = topic;
            Name = name;
        }

        public IActorRef Producer { get; }
        public string Topic { get; }
        public string Name { get; }
    }
}
