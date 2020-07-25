
using SharpPulsar.Akka.InternalCommands.Consumer;

namespace SharpPulsar.Akka.EventSource.Messages
{
    /// <summary>
    /// <see cref="EventsByTag"/> is used for retrieving events that were marked with
    /// a given tag, e.g. all events of an Aggregate Root type.
    /// To tag events you create an a message with tag key and value as message property.
    /// Connection is made for each topic in the namespace
    /// The query is not completed when it reaches the end of the currently stored events,
    /// but it continues to push new events when new events are persisted.
    /// Corresponding query that is completed when it reaches the end of the currently
    /// stored events is provided by <see cref="CurrentEventsByTag"/>.
    /// </summary>
    public sealed class EventsByTag
    {
        public EventsByTag(string tenant, string ns, long fromSequenceId, long toSequenceId, Tag tag)
        {
            Tenant = tenant;
            Namespace = ns;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
            Tag = tag;
        }
        public Tag Tag { get; }
        public string Tenant { get; }
        public string Namespace { get; }
        public long FromSequenceId { get; } //Compute ledgerId and entryId for this 
        public long ToSequenceId { get; } //Compute ledgerId and entryId for this 
    }
}
