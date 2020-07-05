using SharpPulsar.Api;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class NegativeAck
    {
        public NegativeAck(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
