
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class LastMessageIdResponse
    {
        public LastMessageIdResponse(long ledgerId, long entryId, int partition, int batchIndex, int batchSize, long[] ackSets, MessageIdData deletePosition)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
            Partition = partition;
            BatchIndex = batchIndex;
            BatchSize = batchSize;
            AckSets = ackSets;
            if(deletePosition != null)
                MarkDeletePosition = new MarkDeletePosition((long)deletePosition.ledgerId, (long)deletePosition.entryId, deletePosition.Partition, deletePosition.BatchIndex, deletePosition.BatchSize, deletePosition.AckSets);
        }

        public long LedgerId { get; }
        public long EntryId { get; }
        public int Partition { get; }
        public int BatchIndex { get; }
        public int BatchSize { get; }
        public long[] AckSets { get; }
        public MarkDeletePosition MarkDeletePosition { get; }

    }
    public sealed class GetLastMessageIdResponse
    {
        public IMessageId LastMessageId { get; }
        public MessageId MarkDeletePosition { get; }
        public GetLastMessageIdResponse(IMessageId last, MessageId mark)
        {
            LastMessageId = last;
            MarkDeletePosition = mark;
        }
    }
    public sealed class MarkDeletePosition
    {
        public MarkDeletePosition(long ledgerId, long entryId, int partition, int batchIndex, int batchSize, long[] ackSets)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
            Partition = partition;
            BatchIndex = batchIndex;
            BatchSize = batchSize;
            AckSets = ackSets;
        }

        public long LedgerId { get; }
        public long EntryId { get; }
        public int Partition { get; }
        public int BatchIndex { get; }
        public int BatchSize { get; }
        public long[] AckSets { get; }

    }
}
