
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages
{
    public readonly record struct NegativeAck
    {
        public NegativeAck(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
