using SharpPulsar.Akka.Consumer;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public class AckMessages
    {
        public AckMessages(MessageIdReceived messageId)
        {
            MessageId = messageId;
        }

        public MessageIdReceived MessageId { get; }
    }
}
