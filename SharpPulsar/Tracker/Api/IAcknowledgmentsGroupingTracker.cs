
using System.Collections.Generic;
using SharpPulsar.Batch;
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Tracker.Api
{
    public interface IAcknowledgmentsGroupingTracker
    {
        bool IsDuplicate(IMessageId messageId);

        void AddAcknowledgment(IMessageId msgId, CommandAck.AckType ackType, IDictionary<string, long> properties);

        void AddBatchIndexAcknowledgment(BatchMessageId msgId, int batchIndex, int batchSize, CommandAck.AckType ackType, IDictionary<string, long> properties);

        void Flush();

        void Close();

        void FlushAndClean();
	}
}
