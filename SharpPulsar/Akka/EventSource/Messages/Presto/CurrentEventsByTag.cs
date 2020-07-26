
using System.Collections.Immutable;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.Sql.Client;

namespace SharpPulsar.Akka.EventSource.Messages.Presto
{
    /// <summary>
    /// Same type of query as <see cref="EventsByTag"/> but the event stream
    /// is completed immediately when it reaches the end of the "result set". Events that are
    /// stored after the query is completed are not included in the event stream.
    /// </summary>
    public sealed class CurrentEventsByTag: IPrestoEventSourceMessage
    {
        public CurrentEventsByTag(string tenant, string ns, string topic, ImmutableHashSet<string> columns, long fromSequenceId, long toSequenceId, Tag tag, SourceType source,  ClientOptions options, string adminUrl)
        {
            Tenant = tenant;
            Namespace = ns;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
            Tag = tag;
            Source = source;
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
        public ImmutableHashSet<string> Columns { get; }
        public string AdminUrl { get; }
    }
}

