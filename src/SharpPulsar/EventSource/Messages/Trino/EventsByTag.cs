using System.Collections.Generic;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Trino;

namespace SharpPulsar.EventSource.Messages.Presto
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
    public sealed class EventsByTag : ITrinoEventSourceMessage
    {
        public EventsByTag(string tenant, string ns, string topic, HashSet<string> columns, long fromMessageId, long toMessageId, Tag tag, ClientOptions options, string adminUrl)
        {
            Tenant = tenant;
            Namespace = ns;
            FromMessageId = fromMessageId;
            ToMessageId = toMessageId;
            Tag = tag;
            Source = SourceType.Presto;
            Topic = topic;
            Columns = columns;
            Options = options;
            AdminUrl = adminUrl;
        }
        public Tag Tag { get; }
        public string Tenant { get; }
        public string Namespace { get; }
        public string Topic { get; }
        public long FromMessageId { get; } //Compute ledgerId and entryId for this 
        public long ToMessageId { get; } //Compute ledgerId and entryId for this 
        public SourceType Source { get; }
        public ClientOptions Options { get; }
        public HashSet<string> Columns { get; }
        public string AdminUrl { get; }
    }
}
