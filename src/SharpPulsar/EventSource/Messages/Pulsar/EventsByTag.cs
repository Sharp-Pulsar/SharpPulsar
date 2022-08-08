
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Configuration;

namespace SharpPulsar.EventSource.Messages.Pulsar
{
    
    public sealed class EventsByTag<T> : IPulsarEventSourceMessage<T>
    {
        public EventsByTag(string tenant, string ns, string topic, long fromMessageId, long toMessageId, Tag tag, string adminUrl, ReaderConfigurationData<T> configuration, ClientConfigurationData clientConfiguration)
        {
            Tenant = tenant;
            Namespace = ns;
            FromMessageId = fromMessageId;
            ToMessageId = toMessageId;
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
        public long FromMessageId { get; } //Compute ledgerId and entryId for this 
        public long ToMessageId { get; } //Compute ledgerId and entryId for this 
        public SourceType Source { get; }
        public string AdminUrl { get; }
        public ReaderConfigurationData<T> Configuration { get; }
        public ClientConfigurationData ClientConfiguration { get; }
        public string Topic { get; }
    }
}
