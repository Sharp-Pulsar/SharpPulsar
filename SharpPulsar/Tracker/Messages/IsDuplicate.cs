using SharpPulsar.Api;

namespace SharpPulsar.Tracker.Messages
{
    public sealed class IsDuplicate
    {
        public IsDuplicate(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
