
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.EventSource.Messages.Pulsar
{
    
    public sealed class EventsByTag : IPulsarEventSourceMessage
    {
        public EventsByTag(string tenant, string ns, string topic, long fromSequenceId, long toSequenceId, Tag tag, string adminUrl, ReaderConfigurationData configuration, ClientConfigurationData clientConfiguration)
        {
            Tenant = tenant;
            Namespace = ns;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
            Tag = tag;
            Source = SourceType.Pulsar;
            AdminUrl = adminUrl;
            Configuration = configuration;
            ClientConfiguration = clientConfiguration;
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
        public ClientConfigurationData ClientConfiguration { get; }
        public string Topic { get; }
    }
}
