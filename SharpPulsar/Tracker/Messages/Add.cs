using SharpPulsar.Api;

namespace SharpPulsar.Tracker.Messages
{
    public sealed class Add
    {
        public Add(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
