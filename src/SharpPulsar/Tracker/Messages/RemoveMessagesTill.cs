using SharpPulsar.API;

namespace SharpPulsar.Tracker.Messages
{
    public record RemoveMessagesTill
    {
        public RemoveMessagesTill(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
