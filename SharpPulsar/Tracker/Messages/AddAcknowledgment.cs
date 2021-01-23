using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Tracker.Messages
{
    public sealed class AddAcknowledgment
    {
        public AddAcknowledgment(IMessageId messageId, CommandAck.AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        {
            MessageId = messageId;
            AckType = ackType;
            Properties = properties;
            Txn = txn;
        }
        public IActorRef Txn { get; }
        public IMessageId MessageId { get; } 
        public CommandAck.AckType AckType { get; } 
        public IDictionary<string, long> Properties { get; }
    }
}
