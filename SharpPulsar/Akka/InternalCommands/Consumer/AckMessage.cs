using SharpPulsar.Akka.Consumer;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public class AckMessage
    {
        public AckMessage(MessageIdReceived messageId)
        {
            MessageId = messageId;
        }

        public MessageIdReceived MessageId { get; }
    }
}
