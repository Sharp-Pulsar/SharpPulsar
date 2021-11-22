
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Configuration;

namespace SharpPulsar.EventSource.Messages.Pulsar
{
    
    public sealed class EventsByTopic<T> : IPulsarEventSourceMessage<T>
    {
        public EventsByTopic(string tenant, string ns, string topic, long fromMessageId, long toMessageId, string adminUrl, ReaderConfigurationData<T> configuration, ClientConfigurationData clientConfiguration)
        {
            Tenant = tenant;
            Namespace = ns;
            Topic = topic;
            FromMessageId = fromMessageId;
            ToMessageId = toMessageId;
            Source = SourceType.Pulsar;
            AdminUrl = adminUrl;
            Configuration = configuration;
            ClientConfiguration = clientConfiguration;
        }

        public string Tenant { get; }
        public string Namespace { get; }
        public string Topic { get; }
        public long FromMessageId { get; } //Compute ledgerId and entryId for this 
        public long ToMessageId { get; } //Compute ledgerId and entryId for this 
        public SourceType Source { get; }
        public string AdminUrl { get; }
        public ReaderConfigurationData<T> Configuration { get; }
        public ClientConfigurationData ClientConfiguration { get; }
    }
}
