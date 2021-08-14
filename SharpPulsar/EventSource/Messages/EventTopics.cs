
namespace SharpPulsar.EventSource.Messages
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
    public sealed class EventTopics:IEventTopics
    {
        public EventTopics(string tenant, string ns, string adminUri)
        {
            Tenant = tenant;
            Namespace = ns;
            AdminUri = adminUri;
        }

        public string Tenant { get; }
        public string Namespace { get; }
        public string AdminUri { get; }
    }
    public sealed class CurrentEventTopics:IEventTopics
    {
        public CurrentEventTopics(string tenant, string ns, string adminUri)
        {
            Tenant = tenant;
            Namespace = ns;
            AdminUri = adminUri;
        }

        public string Tenant { get; }
        public string Namespace { get; }
        public string AdminUri { get; }
    }

    public interface IEventTopics
    {
        public string Tenant { get; }
        public string Namespace { get; }
        public string AdminUri { get; }
    }
}
