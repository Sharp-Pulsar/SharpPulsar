using SharpPulsar.Api;

namespace SharpPulsar.Messages.Consumer
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
