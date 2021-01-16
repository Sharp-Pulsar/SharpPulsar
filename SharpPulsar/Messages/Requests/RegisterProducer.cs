using Akka.Actor;

namespace SharpPulsar.Messages.Requests
{
    public sealed class RegisterProducer
    {
        public long ProducerId { get; }
        public IActorRef Producer { get; }
        public RegisterProducer(long producerId, IActorRef producer)
        {
            Producer = producer;
            ProducerId = producerId;
        }
    }
    public sealed class RemoveProducer
    {
        public long ProducerId { get; }
        public RemoveProducer(long producerId)
        {
            ProducerId = producerId;
        }
    }
    public sealed class RegisterConsumer
    {
        public long ConsumerId { get; }
        public IActorRef Consumer { get; }
        public RegisterConsumer(long consumerId, IActorRef consumer)
        {
            ConsumerId = consumerId;
            Consumer = consumer;
        }
    }
    public sealed class RemoveConsumer
    {
        public long ConsumerId { get; }
        public RemoveConsumer(long consumerId)
        {
            ConsumerId = consumerId;
        }
    }
}
