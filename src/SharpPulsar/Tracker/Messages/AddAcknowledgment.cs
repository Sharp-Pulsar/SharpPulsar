using System.Collections.Generic;
using Pulsar.Proto;
using SharpPulsar.API;

namespace SharpPulsar.Tracker.Messages
{
    public record AddAcknowledgment
    {
        public AddAcknowledgment(IMessageId messageId, CommandAck.Types.AckType ackType, IDictionary<string, long> properties)
        {
            MessageId = messageId;
            AckType = ackType;
            Properties = properties;
        }
        public IMessageId MessageId { get; } 
        public CommandAck.Types.AckType AckType { get; } 
        public IDictionary<string, long> Properties { get; }
    }
}
