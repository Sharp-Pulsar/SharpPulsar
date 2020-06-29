using SharpPulsar.Akka.Consumer;
using SharpPulsar.Api;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public class AckMessages
    {
        public AckMessages(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
}
