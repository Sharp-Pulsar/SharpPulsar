
using System.Collections.Generic;
using System.Collections.Immutable;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Sql.Client;

namespace SharpPulsar.EventSource.Messages.Presto
{
    /// <summary>
    /// Same type of query as <see cref="EventsByTag"/> but the event stream
    /// is completed immediately when it reaches the end of the "result set". Events that are
    /// stored after the query is completed are not included in the event stream.
    /// </summary>
    public sealed class CurrentEventsByTag
    {
        public CurrentEventsByTag(string tenant, string ns, string topic, HashSet<string> columns, long fromSequenceId, long toSequenceId, Tag tag, ClientOptions options, string adminUrl)
        {
            Tenant = tenant;
            Namespace = ns;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
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
        public long FromSequenceId { get; } //Compute ledgerId and entryId for this 
        public long ToSequenceId { get; } //Compute ledgerId and entryId for this 
        public SourceType Source { get; }
        public ClientOptions Options { get; }
        public HashSet<string> Columns { get; }
        public string AdminUrl { get; }
    }
}

