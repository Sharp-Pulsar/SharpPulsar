using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Requests
{
    public sealed class ReceivedMessage<T>
    {
        public IMessage<T> Message { get; }
        public ReceivedMessage(IMessage<T> message)
        {
            Message = message;
        }
    }
    public sealed class ReceivedBatchMessage<T>
    {
        public IMessages<T> Messages { get; }
        public ReceivedBatchMessage(IMessages<T> messages)
        {
            Messages = messages;
        }
    }
}
