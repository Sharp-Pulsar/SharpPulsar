namespace SharpPulsar.Table.Messages
{
    public record HandleMessage<T>
    {
        public IMessage<T> Message { get; }
        public HandleMessage(IMessage<T> message)
        {
            Message = message;
        }
    }
}
