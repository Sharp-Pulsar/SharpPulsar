using Akka.Actor;

namespace SharpPulsar.Akka.InternalCommands.Producer
{
    public class CreatedProducer
    {
        public CreatedProducer(IActorRef producer, string topic, string name, bool isgroup = false)
        {
            Producer = producer;
            Topic = topic;
            Name = name;
            IsGroup = isgroup;
        }

        public IActorRef Producer { get; }
        public string Topic { get; }
        public string Name { get; }
        public bool IsGroup { get; }
    }
}
