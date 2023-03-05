using System.Collections.Generic;
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Tracker.Messages
{
    public readonly record struct AddAcknowledgment
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
