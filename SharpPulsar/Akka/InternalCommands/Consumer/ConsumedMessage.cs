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
    public class EventMessage:IEventMessage
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
    /// <summary>
    /// We use this to move the cursor/count forward at the client
    /// This could be used to filter out messages without certain properties
    /// </summary>
    public sealed class NotTagged:IEventMessage
    {
        public NotTagged(Message message, long sequenceId, string topic, long ledgerId, long entryId)
        {
            Message = message;
            SequenceId = sequenceId;
            Topic = topic;
            LedgerId = ledgerId;
            EntryId = entryId;
        }

        public long SequenceId { get; }
        public string Topic { get; }
        public Message Message { get;}
        public long LedgerId { get; }
        public long EntryId { get; }
    }
    public interface IEventMessage
    {

    }
}
