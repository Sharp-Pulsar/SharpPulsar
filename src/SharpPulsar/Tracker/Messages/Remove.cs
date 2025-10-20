namespace SharpPulsar.Tracker.Messages
{
    public record Remove
    {
        public Remove(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
   
}
