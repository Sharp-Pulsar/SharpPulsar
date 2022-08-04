using SharpPulsar.Interfaces;

namespace SharpPulsar.Table.Messages
{
    public sealed class HandleMessage<T>
    {
        public IMessage<T> Message { get; }
        public HandleMessage(IMessage<T> message)
        {
            Message = message;
        }
    }
}
