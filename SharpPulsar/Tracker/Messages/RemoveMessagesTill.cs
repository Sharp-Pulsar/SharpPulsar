using SharpPulsar.Api;

namespace SharpPulsar.Tracker.Messages
{
    public sealed class RemoveMessagesTill
    {
        public RemoveMessagesTill(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
