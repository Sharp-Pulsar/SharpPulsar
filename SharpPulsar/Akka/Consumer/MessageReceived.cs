
using System.Buffers;

namespace SharpPulsar.Messages.Consumer
{
    public class MessageReceived
    {
        public MessageReceived(long consumerId, MessageIdReceived messageId, ReadOnlySequence<byte> data, int redeliveryCount, ClientCnx clientCnx)
        {
            ConsumerId = consumerId;
            MessageId = messageId;
            Data = data;
            RedeliveryCount = redeliveryCount;
            ClientCnx = clientCnx;
        }

        public long ConsumerId { get; }
        public MessageIdReceived MessageId { get; }
        public ReadOnlySequence<byte> Data { get; }
        public int RedeliveryCount { get; }
        public ClientCnx ClientCnx { get; }
    }

    public class MessageIdReceived
    {
        public MessageIdReceived(long ledgerId, long entryId, int batchIndex, int partition, long[] ackSet)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
            BatchIndex = batchIndex;
            Partition = partition;
            AckSet = ackSet;
        }

        public long LedgerId { get; }
        public long EntryId { get; }
        public int BatchIndex { get; }
        public int Partition { get; }
        public long[] AckSet { get; }
    }
}
