using SharpPulsar.Api;

namespace SharpPulsar.Tracker.Messages
{
    public sealed class Remove
    {
        public Remove(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
