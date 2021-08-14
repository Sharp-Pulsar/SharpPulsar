using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class ReceivedMessage<T>
    {
        public IMessage<T> Message { get; }
        public ReceivedMessage(IMessage<T> message)
        {
            Message = message;
        }
    }
}
