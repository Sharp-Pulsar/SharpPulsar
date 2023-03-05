using System.Collections.Generic;
using SharpPulsar.Trino.Message;

namespace SharpPulsar.EventSource.Messages
{
    /// <summary>
    /// implemented by EventEnvelope, EventStats & EventError
    /// </summary>
    public interface IEventEnvelope
    {

    }
    public class EventEnvelope:IEventEnvelope
    {
        public EventEnvelope(Dictionary<string, object> @event, long sequenceId, string topic)
        {
            Event = @event;
            SequenceId = sequenceId;
            Topic = topic;
        }
        public string Topic { get; }
        public Dictionary<string, object> Event { get; }
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
