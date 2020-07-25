
using System;
using SharpPulsar.Akka.InternalCommands.Consumer;

namespace SharpPulsar.Akka.EventSource.Messages
{

    /// <summary>
    /// <para>
    /// <see cref="EventTopics"/> is used for retrieving all `topics` of all
    /// event namespace.
    /// </para>
    /// The returned event stream is unordered and you can expect different order for multiple
    /// executions of the query.
    /// <para>
    /// The stream is not completed when it reaches the end of the currently used `namespace`,
    /// but it continues to push new `topics` when new event topics are created.
    /// Corresponding query that is completed when it reaches the end of the currently
    /// currently used `namespace` is provided by <see cref="CurrentEventTopics"/>.
    /// </para>
    /// A dedicated Actor will be created for each <see cref="EventTopics"/> request
    /// </summary>
    public sealed class EventTopics : IEventSourceMessage
    {
        public EventTopics(string tenant, string ns, SourceType source)
        {
            Tenant = tenant;
            Namespace = ns;
            Source = source;
        }

        public string Tenant { get; }
        public string Namespace { get; }
        public SourceType Source { get; }
    }
}
