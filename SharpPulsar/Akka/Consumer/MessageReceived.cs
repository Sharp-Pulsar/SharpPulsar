
using System.Buffers;

namespace SharpPulsar.Akka.Consumer
{
    public class MessageReceived
    {
        public MessageReceived(long consumerId, MessageIdReceived messageId, ReadOnlySequence<byte> data, int redeliveryCount)
        {
            ConsumerId = consumerId;
            MessageId = messageId;
            Data = data;
            RedeliveryCount = redeliveryCount;
        }

        public long ConsumerId { get; }
        public MessageIdReceived MessageId { get; }
        public ReadOnlySequence<byte> Data { get; }
        public int RedeliveryCount { get; }
    }

    public class MessageIdReceived
    {
        public MessageIdReceived(long ledgerId, long entryId, int batchIndex, int partition)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
            BatchIndex = batchIndex;
            Partition = partition;
        }

        public long LedgerId { get; }
        public long EntryId { get; }
        public int BatchIndex { get; }
        public int Partition { get; }
    }
}
