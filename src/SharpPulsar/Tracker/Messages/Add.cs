
using SharpPulsar.Interfaces;

namespace SharpPulsar.Tracker.Messages
{
    public readonly record struct Add<T>
    {
        public Add(IMessage<T> message)
        {
            Message = message;
        }
        public IMessage<T> Message { get; }
    }
    public readonly record struct Add
    {
        public Add(IMessageId messageId) => (MessageId, RedeliveryCount) = (messageId, 0);
        public Add(IMessageId messageId, int redeliveryCount) => (MessageId, RedeliveryCount) = (messageId, redeliveryCount);
        public IMessageId MessageId { get; }
        public int RedeliveryCount { get; }
    }
}