using System;
using System.Collections;

namespace SharpPulsar.API
{
    /// <summary>
    /// The <seealso cref="MessageIdAdv"/> interface provided for advanced users.
    /// <para>
    /// All built-in MessageId implementations should be able to be cast to MessageIdAdv.
    /// </para>
    /// </summary>
    public interface IMessageIdAdv : IMessageId
    {

        /// <summary>
        /// Get the ledger ID.
        /// </summary>
        /// <returns> the ledger ID </returns>
        long LedgerId { get; }

        /// <summary>
        /// Get the entry ID.
        /// </summary>
        /// <returns> the entry ID </returns>
        long EntryId { get; }

        /// <summary>
        /// Get the partition index.
        /// </summary>
        /// <returns> -1 if the message is from a non-partitioned topic, otherwise the non-negative partition index </returns>
        int PartitionIndex
        {
            get
            {
                return -1;
            }
        }

        /// <summary>
        /// Get the batch index.
        /// </summary>
        /// <returns> -1 if the message is not in a batch </returns>
        int BatchIndex
        {
            get
            {
                return -1;
            }
        }

        /// <summary>
        /// Get the batch size.
        /// </summary>
        /// <returns> 0 if the message is not in a batch </returns>
        int BatchSize
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the BitSet that indicates which messages in the batch.
        /// 
        /// @implNote The message IDs of a batch should share a BitSet. For example, given 3 messages in the same batch whose
        /// size is 3, all message IDs of them should return "111" (i.e. a BitSet whose size is 3 and all bits are 1). If the
        /// 1st message has been acknowledged, the returned BitSet should become "011" (i.e. the 1st bit become 0).
        /// If the caller performs any read or write operations on the return value of this method, they should do so with
        /// lock protection.
        /// </summary>
        /// <returns> null if the message is a non-batched message </returns>
        BitArray AckSet
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Get the message ID of the first chunk if the current message ID represents the position of a chunked message.
        /// 
        /// @implNote A chunked message is distributed across different BookKeeper entries. The message ID of a chunked
        /// message is composed of two message IDs that represent positions of the first and the last chunk. The message ID
        /// itself represents the position of the last chunk.
        /// </summary>
        /// <returns> null if the message is not a chunked message </returns>
        IMessageIdAdv FirstChunkMessageId
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The default implementation of <seealso cref="Comparable.compareTo(object)"/>.
        /// </summary>
        int CmpareTo(IMessageId o)
        {
            if (o is not IMessageIdAdv)
            {
                throw new NotSupportedException("Unknown MessageId type: " + (o != null ? o.GetType().Name : "null"));
            }
            var other = (IMessageIdAdv)o;
            var result = LedgerId.CompareTo(other.LedgerId);
            if (result != 0)
            {
                return result;
            }
            result = EntryId.CompareTo(other.EntryId);
            if (result != 0)
            {
                return result;
            }
            // TODO: Correct the following compare logics, see https://github.com/apache/pulsar/pull/18981
            result = PartitionIndex.CompareTo(other.PartitionIndex);
            if (result != 0)
            {
                return result;
            }
            return BatchIndex.CompareTo(other.BatchIndex);
        }
    }

}
