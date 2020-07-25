
using SharpPulsar.Akka.InternalCommands.Consumer;

namespace SharpPulsar.Akka.EventSource.Messages
{
    /// <summary>
    /// Same type of query as <see cref="EventsByTag"/> but the event stream
    /// is completed immediately when it reaches the end of the "result set". Events that are
    /// stored after the query is completed are not included in the event stream.
    /// </summary>
    public sealed class CurrentEventsByTag:IEventSourceMessage
    {
        public CurrentEventsByTag(string tenant, string ns, long fromSequenceId, long toSequenceId, Tag tag, SourceType source)
        {
            Tenant = tenant;
            Namespace = ns;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
            Tag = tag;
            Source = source;
        }
        public Tag Tag { get; }
        public string Tenant { get; }
        public string Namespace { get; }
        public long FromSequenceId { get; } //Compute ledgerId and entryId for this 
        public long ToSequenceId { get; } //Compute ledgerId and entryId for this 
        public SourceType Source { get; }
    }
}

