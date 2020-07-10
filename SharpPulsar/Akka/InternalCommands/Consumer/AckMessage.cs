using SharpPulsar.Api;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public class AckMessage
    {
        public AckMessage(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
