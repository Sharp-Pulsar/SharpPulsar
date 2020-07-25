
using SharpPulsar.Akka.InternalCommands.Consumer;

namespace SharpPulsar.Akka.EventSource.Messages
{
    /// <summary>
    /// Same type of query as <see cref="EventTopics"/> but the stream
    /// is completed immediately when it reaches the end of the "result set". Event topics
    /// that are created after the query is completed are not included in the stream.
    /// </summary>
    public sealed class CurrentEventTopics : IEventSourceMessage
    {
        public CurrentEventTopics(string tenant, string ns, SourceType source)
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
