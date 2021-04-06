using SharpPulsar.Sql.Message;
using System.Collections.Generic;

namespace SharpPulsar.EventSource.Messages
{
    public interface IEventEnvelope
    {

    }
    public class EventEnvelope:IEventEnvelope
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

    public class EventStats : IEventEnvelope
    {
        public EventStats(StatsResponse stats)
        {
            Stats = stats;
        }

        public StatsResponse Stats { get; }
    }

    public class EventError: IEventEnvelope
    {
        public EventError(ErrorResponse error)
        {
            Error = error;
        }

        public ErrorResponse Error { get; }
    }
}
