using SharpPulsar.Interfaces;

namespace SharpPulsar.Tracker.Messages
{
    public sealed class Add<T>
    {
        public Add(IMessageId messageId)
        {
            MessageId = messageId;
        }
        public Add(IMessage<T> message)
        {
            Message = message;
        }
        public Add(IMessageId messageId, int redeliveryCount)
        {
            MessageId = messageId;
            RedeliveryCount = redeliveryCount;
        }
        public IMessageId MessageId { get; }
        public IMessage<T> Message { get; }
        public int RedeliveryCount { get; } 
    }
}