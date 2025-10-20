namespace SharpPulsar.Tracker.Messages
{
    public record IsDuplicate
    {
        public IsDuplicate(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
