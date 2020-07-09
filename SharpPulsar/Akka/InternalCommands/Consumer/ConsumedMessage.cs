using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Api;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public class ConsumedMessage
    {
        public ConsumedMessage(IActorRef consumer, IMessage message, IList<long> ackSets, string consumerName)
        {
            Consumer = consumer;
            Message = message;
            AckSets = ackSets;
            ConsumerName = consumerName;
        }
        public string ConsumerName { get; }
        public IActorRef Consumer { get; }
        public IMessage Message { get; }
        public IList<long> AckSets { get; }

    }

    public sealed class ConsumedMessages
    {
        public List<ConsumedMessage> Messages { get; }

        public ConsumedMessages()
        {
                Messages = new List<ConsumedMessage>();
        }
    }
    public class EventMessage:IEventMessage
    {
        public EventMessage(IMessage message, long sequenceId, long ledgerId, long entry)
        {
            Message = message;
            SequenceId = sequenceId;
            LedgerId = ledgerId;
            Entry = entry;
        }
        public long SequenceId { get; }
        public long LedgerId { get; }
        public long Entry { get; }
        public IMessage Message { get; }
    }
    /// <summary>
    /// We use this to move the cursor/count forward at the client
    /// This could be used to filter out messages without certain properties
    /// </summary>
    public sealed class NotTagged:IEventMessage
    {
        public NotTagged(IMessage message, long sequenceId, string topic, long ledgerId, long entryId)
        {
            Message = message;
            SequenceId = sequenceId;
            Topic = topic;
            LedgerId = ledgerId;
            EntryId = entryId;
        }

        public long SequenceId { get; }
        public string Topic { get; }
        public IMessage Message { get;}
        public long LedgerId { get; }
        public long EntryId { get; }
    }
    public interface IEventMessage
    {

    }
}
