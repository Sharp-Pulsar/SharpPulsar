using SharpPulsar.Interfaces;

namespace SharpPulsar.Tracker.Messages
{
    public sealed class Add<T>
    {
        public Add(IMessage<T> message)
        {
            Message = message;
        }
        public IMessage<T> Message { get; }
    }
    public sealed class Add
    {
        public Add(IMessageId messageId)
        {
            MessageId = messageId;
        }
        public Add(IMessageId messageId, int redeliveryCount)
        {
            MessageId = messageId;
            RedeliveryCount = redeliveryCount;
        }
        public IMessageId MessageId { get; }
        public int RedeliveryCount { get; }
    }
}