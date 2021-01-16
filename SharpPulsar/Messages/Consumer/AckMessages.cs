using SharpPulsar.Api;

namespace SharpPulsar.Messages.Consumer
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
