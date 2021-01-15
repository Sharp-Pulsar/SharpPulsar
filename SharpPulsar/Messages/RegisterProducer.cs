using Akka.Actor;

namespace SharpPulsar.Messages
{
    public sealed class RegisterProducer
    {
        public long Id { get; }
        public IActorRef Producer { get; }
        public RegisterProducer(long id, IActorRef producer)
        {
            Id = id;
            Producer = producer;
        }
    }
    public sealed class RegisterConsumer
    {
        public long Id { get; }
        public IActorRef Consumer { get; }
        public RegisterConsumer(long id, IActorRef consumer)
        {
            Id = id;
            Consumer = consumer;
        }
    }
}
