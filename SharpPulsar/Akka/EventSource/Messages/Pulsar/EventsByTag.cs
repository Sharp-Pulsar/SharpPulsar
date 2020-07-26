﻿
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.EventSource.Messages.Pulsar
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
    public sealed class EventsByTag : IPulsarEventSourceMessage
    {
        public EventsByTag(string tenant, string ns, string topic, long fromSequenceId, long toSequenceId, Tag tag, SourceType source, string adminUrl, ReaderConfigurationData configuration, ClientConfigurationData clientConfiguration)
        {
            Tenant = tenant;
            Namespace = ns;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
            Tag = tag;
            Source = source;
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
