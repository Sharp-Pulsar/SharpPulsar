using Akka.Actor;
using SharpPulsar.Impl;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public class ConsumedMessage
    {
        public ConsumedMessage(IActorRef consumer, Message message)
        {
            Consumer = consumer;
            Message = message;
        }

        public IActorRef Consumer { get; }
        public Message Message { get; }
    }
    public class EventMessage
    {
        public EventMessage(Message message, long sequenceId, long ledgerId, long entry)
        {
            Message = message;
            SequenceId = sequenceId;
            LedgerId = ledgerId;
            Entry = entry;
        }
        public long SequenceId { get; }
        public long LedgerId { get; }
        public long Entry { get; }
        public Message Message { get; }
    }
}
