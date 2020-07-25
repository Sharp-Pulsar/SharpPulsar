
namespace SharpPulsar.Akka.EventSource.Messages
{
    /// <summary>
    /// Same type of query as <see cref="EventsByTopic"/> but the event stream
    /// is completed immediately when it reaches the end of the "result set". Events that are
    /// stored after the query is completed are not included in the event stream.
    /// </summary>
    public sealed class CurrentEventsByTopic
    {
        public CurrentEventsByTopic(string tenant, string ns, string topic, long fromSequenceId, long toSequenceId)
        {
            Tenant = tenant;
            Namespace = ns;
            Topic = topic;
            FromSequenceId = fromSequenceId;
            ToSequenceId = toSequenceId;
        }

        public string Tenant { get; }
        public string Namespace { get; }
        public string Topic { get; }
        public long FromSequenceId { get; } //Compute ledgerId and entryId for this 
        public long ToSequenceId { get; } //Compute ledgerId and entryId for this 
    }
}
