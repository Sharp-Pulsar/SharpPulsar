namespace SharpPulsar.Messages
{
    public record NegativeAck
    {
        public NegativeAck(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
