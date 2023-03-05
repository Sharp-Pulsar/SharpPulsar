using SharpPulsar.Interfaces;

namespace SharpPulsar.Tracker.Messages
{
    public readonly record struct RemoveMessagesTill
    {
        public RemoveMessagesTill(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
