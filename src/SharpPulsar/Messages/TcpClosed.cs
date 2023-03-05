
namespace SharpPulsar.Messages
{
    public readonly record struct TcpClosed
    {
    }

    public readonly record struct ProducerClosed
    {
        public ProducerClosed(long producerId)
        {
            ProducerId = producerId;
        }

        public long ProducerId { get; }
    }
    public readonly record struct ConsumerClosed
    {
        public ConsumerClosed(long consumerId)
        {
            ConsumerId = consumerId;
        }
        public long ConsumerId { get; }
    }
}
