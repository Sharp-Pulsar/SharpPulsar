using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class MessageProcessed<T>
    {
        public IMessage<T> Message { get; }
        public MessageProcessed(IMessage<T> message)
        {
            Message = message;
        }
    }
}
