
namespace SharpPulsar.Messages
{
    public class TcpClosed
    {
    }

    public sealed class ProducerClosed
    {
        public ProducerClosed(long producerId)
        {
            ProducerId = producerId;
        }

        public long ProducerId { get; }
    }
    public sealed class ConsumerClosed
    {
        public ConsumerClosed(long consumerId)
        {
            ConsumerId = consumerId;
        }
        public long ConsumerId { get; }
    }
}
