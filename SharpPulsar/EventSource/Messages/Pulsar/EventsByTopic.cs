﻿
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Configuration;

namespace SharpPulsar.EventSource.Messages.Pulsar
{
    
    public sealed class EventsByTopic<T> : IPulsarEventSourceMessage<T>
    {
        public EventsByTopic(string tenant, string ns, string topic, long fromSequenceId, long toSequenceId, string adminUrl, ReaderConfigurationData<T> configuration, ClientConfigurationData clientConfiguration)
        {
            Tenant = tenant;
            Namespace = ns;
            Topic = topic;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
            Source = SourceType.Pulsar;
            AdminUrl = adminUrl;
            Configuration = configuration;
            ClientConfiguration = clientConfiguration;
        }

        public string Tenant { get; }
        public string Namespace { get; }
        public string Topic { get; }
        public long FromSequenceId { get; } //Compute ledgerId and entryId for this 
        public long ToSequenceId { get; } //Compute ledgerId and entryId for this 
        public SourceType Source { get; }
        public string AdminUrl { get; }
        public ReaderConfigurationData<T> Configuration { get; }
        public ClientConfigurationData ClientConfiguration { get; }
    }
}
