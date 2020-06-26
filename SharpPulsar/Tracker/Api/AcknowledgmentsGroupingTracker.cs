
using System.Collections.Generic;
using SharpPulsar.Batch;
using SharpPulsar.Impl;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Tracker.Api
{
    public interface AcknowledgmentsGroupingTracker
    {
        bool IsDuplicate(MessageId messageId);

        void AddAcknowledgment(MessageId msgId, CommandAck.AckType ackType, IDictionary<string, long> properties);

        void AddBatchIndexAcknowledgment(BatchMessageId msgId, int batchIndex, int batchSize, CommandAck.AckType ackType, IDictionary<string, long> properties);

        void Flush();

        void Close();

        void FlushAndClean();
	}
}
