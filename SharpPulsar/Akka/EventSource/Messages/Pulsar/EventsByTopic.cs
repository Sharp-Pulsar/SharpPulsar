
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.EventSource.Messages.Pulsar
{
    /// <summary>
    /// <see cref="EventsByTopic"/> is used for retrieving events for a specific topics 
    /// <para>
    /// You can retrieve a subset of all events by specifying <paramref name="fromSequenceId"/> and <paramref name="toSequenceId"/>
    /// or use `0L` and <see cref="long.MaxValue"/> respectively to retrieve all events. Note that
    /// the corresponding sequence id of each event is provided in the
    /// <see cref="EventEnvelope"/>, which makes it possible to resume the
    /// stream at a later point from a given sequence id.
    /// </para>
    /// The returned event stream is ordered by ledgerId and entryId.
    /// <para>
    /// The stream is not completed when it reaches the end of the currently stored events,
    /// but it continues to push new events when new events are persisted.
    /// Corresponding query that is completed when it reaches the end of the currently
    /// stored events is provided by <see cref="CurrentEventsByTopic"/>.
    /// </para>
    /// </summary>
    public sealed class EventsByTopic : IPulsarEventSourceMessage
    {
        public EventsByTopic(string tenant, string ns, string topic, long fromSequenceId, long toSequenceId, SourceType source, string adminUrl, ReaderConfigurationData configuration, ClientConfigurationData clientConfiguration)
        {
            Tenant = tenant;
            Namespace = ns;
            Topic = topic;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
            Source = source;
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
        public ReaderConfigurationData Configuration { get; }
        public ClientConfigurationData ClientConfiguration { get; }
    }
}
