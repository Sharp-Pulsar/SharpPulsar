
using SharpPulsar.Interfaces;

namespace SharpPulsar.Tracker.Messages
{
    public readonly record struct IsDuplicate
    {
        public IsDuplicate(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
