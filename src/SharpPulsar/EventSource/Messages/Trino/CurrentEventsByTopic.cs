
using System.Collections.Generic;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Trino;

namespace SharpPulsar.EventSource.Messages.Presto
{
    /// <summary>
    /// Same type of query as <see cref="EventsByTopic"/> but the event query
    /// is completed immediately when it reaches the end of the "result set". Events that are
    /// stored after the query is completed are not included in the event stream.
    /// </summary>
    public sealed class CurrentEventsByTopic : ITrinoEventSourceMessage
    {
        public CurrentEventsByTopic(string tenant, string ns, string topic, HashSet<string> columns, long fromMessageId, long toMessageId, string adminUrl, ClientOptions options)
        {
            Tenant = tenant;
            Namespace = ns;
            Topic = topic;
            Columns = columns;
            FromMessageId = fromMessageId;
            ToMessageId = toMessageId;
            Source = SourceType.Presto;
            AdminUrl = adminUrl;
            Options = options;
        }

        public string Tenant { get; }
        public string Namespace { get; }
        public string Topic { get; }
        public string AdminUrl { get; }
        public long FromMessageId { get; } //Compute ledgerId and entryId for this 
        public long ToMessageId { get; } //Compute ledgerId and entryId for this 
        public SourceType Source { get; }
        public ClientOptions Options { get; }
        public HashSet<string> Columns { get; }
    }
}
