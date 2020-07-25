
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.EventSource.Messages.Pulsar
{
    /// <summary>
    /// Same type of query as <see cref="EventsByTag"/> but the event stream
    /// is completed immediately when it reaches the end of the "result set". Events that are
    /// stored after the query is completed are not included in the event stream.
    /// </summary>
    public sealed class CurrentEventsByTag:IPulsarEventSourceMessage
    {
        public CurrentEventsByTag(string tenant, string ns, string topic, long fromSequenceId, long toSequenceId, Tag tag, SourceType source, string adminUrl, ReaderConfigurationData configuration)
        {
            Tenant = tenant;
            Namespace = ns;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
            Tag = tag;
            Source = source;
            AdminUrl = adminUrl;
            Configuration = configuration;
            Topic = topic;
        }
        public Tag Tag { get; }
        public string Tenant { get; }
        public string Namespace { get; }
        public long FromSequenceId { get; } //Compute ledgerId and entryId for this 
        public long ToSequenceId { get; } //Compute ledgerId and entryId for this 
        public SourceType Source { get; }
        public string AdminUrl { get; }
        public ReaderConfigurationData Configuration { get; }
        public string Topic { get; }
    }
    
}

