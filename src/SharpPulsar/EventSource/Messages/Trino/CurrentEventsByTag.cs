
using System.Collections.Generic;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Trino;

namespace SharpPulsar.EventSource.Messages.Presto
{
    /// <summary>
    /// Same type of query as <see cref="EventsByTag"/> but the event stream
    /// is completed immediately when it reaches the end of the "result set". Events that are
    /// stored after the query is completed are not included in the event stream.
    /// </summary>
    public sealed class CurrentEventsByTag: ITrinoEventSourceMessage
    {
        public CurrentEventsByTag(string tenant, string ns, string topic, HashSet<string> columns, long fromMessageId, long toMessageId, Tag tag, ClientOptions options, string adminUrl)
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

