using System.Collections.Immutable;
using SharpPulsar.Akka.Consumer;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public class AckMultiMessage
    {
        public AckMultiMessage(ImmutableList<MessageIdReceived> messageIds)
        {
            MessageIds = messageIds;
        }

        public ImmutableList<MessageIdReceived> MessageIds { get; }
    }
}
