using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Batch;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Tracker.Messages
{
    public sealed class AddBatchIndexAcknowledgment
    {
        public AddBatchIndexAcknowledgment(BatchMessageId messageId, int batchIndex, int batchSize, CommandAck.AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        {
            MessageId = messageId;
            BatchIndex = batchIndex;
            BatchSize = batchSize;
            AckType = ackType;
            Properties = properties;
            Txn = txn;
        }
        public IActorRef Txn { get; }
        public BatchMessageId MessageId { get; } 
        public int BatchIndex { get; } 
        public int BatchSize { get; } 
        public CommandAck.AckType AckType { get; } 
        public IDictionary<string, long> Properties { get; }
    }
}
