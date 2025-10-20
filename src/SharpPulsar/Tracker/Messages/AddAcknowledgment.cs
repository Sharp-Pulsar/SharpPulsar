using System.Collections.Generic;

namespace SharpPulsar.Tracker.Messages
{
    public record AddAcknowledgment
    {
        public AddAcknowledgment(IMessageId messageId, CommandAck.AckType ackType, IDictionary<string, long> properties)
        {
            MessageId = messageId;
            AckType = ackType;
            Properties = properties;
        }
        public IMessageId MessageId { get; } 
        public CommandAck.AckType AckType { get; } 
        public IDictionary<string, long> Properties { get; }
    }
}
