
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages
{
    public sealed class NegativeAck
    {
        public NegativeAck(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
