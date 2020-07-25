using System.Collections.Generic;

namespace SharpPulsar.Akka.EventSource.Messages
{
    public class EventEnvelope
    {
        public EventEnvelope(Dictionary<string, object> @event, Dictionary<string, object> metadata, long sequenceId, string topic)
        {
            Event = @event;
            Metadata = metadata;
            SequenceId = sequenceId;
            Topic = topic;
        }
        public string Topic { get; }
        public Dictionary<string, object> Event { get; }
        public Dictionary<string, object> Metadata { get; }
        public long SequenceId { get; }
    }

}
