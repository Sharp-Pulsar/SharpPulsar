using SharpPulsar.Interfaces;

namespace SharpPulsar.Table.Messages
{
    public readonly record struct HandleMessage<T>
    {
        public IMessage<T> Message { get; }
        public HandleMessage(IMessage<T> message)
        {
            Message = message;
        }
    }
}
