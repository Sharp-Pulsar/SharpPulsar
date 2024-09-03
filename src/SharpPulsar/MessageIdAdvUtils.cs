using System.Collections;
using SharpPulsar.Interfaces;


namespace SharpPulsar
{

    public class MessageIdAdvUtils
    {

        internal static int HashCode(IMessageIdAdv msgId)
        {
            return (int)(31 * (msgId.LedgerId + 31 * msgId.EntryId) + (31 * (long)msgId.PartitionIndex) + msgId.BatchIndex);
        }

        internal static bool Equals(IMessageIdAdv lhs, object o)
        {
            if (!(o is IMessageIdAdv))
            {
                return false;
            }
            
            IMessageIdAdv rhs = (IMessageIdAdv)o;
            return lhs.LedgerId == rhs.LedgerId && lhs.EntryId == rhs.EntryId && lhs.PartitionIndex == rhs.PartitionIndex 
                && lhs.BatchIndex == rhs.BatchIndex;
        }

        /// <summary>
        /// Acknowledge batch message.
        /// </summary>
        /// <param name="msgId">     the message id </param>
        /// <param name="individual"> whether to acknowledge the batch message individually </param>
        /// <returns> true if the batch message is fully acknowledged </returns>
        internal static bool Acknowledge(IMessageIdAdv msgId, bool individual)
        {
            if (!IsBatch(msgId))
            {
                return true;
            }
            //var ackSet = BitSet.Create();
            BitArray ackSet = msgId.AckSet;
            if (ackSet == null)
            {
                // The internal MessageId implementation should never reach here. If users have implemented their own
                // MessageId and getAckSet() is not override, return false to avoid acknowledge current entry.
                return false;
            }
            int batchIndex = msgId.BatchIndex;
            lock (ackSet)
            {
                if (individual)
                {
                    ackSet.Set(batchIndex, false);
                }
                // ?????????????????????????????????????????
                /*else
                {
                    ackSet.Clear(0, batchIndex + 1);
                }
                return ackSet.IsEmpty();*/
                return true;
            }
        }

        internal static bool IsBatch(IMessageIdAdv msgId)
        {
            return msgId.BatchIndex >= 0 && msgId.BatchSize > 0;
        }

        internal static IMessageIdAdv DiscardBatch(IMessageId messageId)
        {
            if (messageId is ChunkMessageId)
            {
                return (IMessageIdAdv)messageId;
            }
            IMessageIdAdv msgId = (IMessageIdAdv)messageId;
            return new MessageId(msgId.LedgerId, msgId.EntryId, msgId.PartitionIndex);
        }

        internal static IMessageIdAdv prevMessageId(IMessageIdAdv msgId)
        {
            return new MessageId(msgId.LedgerId, msgId.EntryId - 1, msgId.PartitionIndex);
        }
    }


}
