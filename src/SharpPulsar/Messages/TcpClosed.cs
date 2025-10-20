
namespace SharpPulsar.Messages
{
    public record TcpClosed
    {
    }

    public record ProducerClosed
    {
        public ProducerClosed(long producerId)
        {
            ProducerId = producerId;
        }

        public long ProducerId { get; }
    }
    public record ConsumerClosed
    {
        public ConsumerClosed(long consumerId)
        {
            ConsumerId = consumerId;
        }
        public long ConsumerId { get; }
    }
}
