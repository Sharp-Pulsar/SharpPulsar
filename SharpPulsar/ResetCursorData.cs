
using SharpPulsar.Batch;

namespace SharpPulsar
{
    public class ResetCursorData
    {
        protected internal long LedgerId;
        protected internal long EntryId;
        protected internal int PartitionIndex = -1;
        protected internal bool IsExcluded = false;
        protected internal int BatchIndex = -1;

        public ResetCursorData(long ledgerId, long entryId)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
        }

        public ResetCursorData(long ledgerId, long entryId, bool isExcluded)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
            IsExcluded = isExcluded;
        }

        public ResetCursorData(MessageId messageId)
        {
            if (messageId is BatchMessageId batchMessageId)
            {
                LedgerId = batchMessageId.LedgerId;
                EntryId = batchMessageId.EntryId;
                BatchIndex = batchMessageId.BatchIndex;
            }
            else if (messageId is MessageId msgId)
            {
                LedgerId = msgId.LedgerId;
                EntryId = msgId.EntryId;
            }
            else if (messageId is TopicMessageId)
            {
                throw new System.ArgumentException("Not supported operation on partitioned-topic");
            }
        }

    }

}
