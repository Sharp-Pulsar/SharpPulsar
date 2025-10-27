
using System.Collections.Generic;
using System.Linq;
using Pulsar.Proto;
using SharpPulsar.API;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class LastMessageIdResponse
    {
        public LastMessageIdResponse(long ledgerId, long entryId, int partition, int batchIndex, int batchSize, List<long> ackSets, MessageIdData deletePosition)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
            Partition = partition;
            BatchIndex = batchIndex;
            BatchSize = batchSize;
            AckSets = ackSets;
            if (deletePosition != null)
                MarkDeletePosition = new MarkDeletePosition((long)deletePosition.LedgerId, (long)deletePosition.EntryId, deletePosition.Partition, deletePosition.BatchIndex, deletePosition.BatchSize, deletePosition.AckSet.ToList());
        }

        public long LedgerId { get; }
        public long EntryId { get; }
        public int Partition { get; }
        public int BatchIndex { get; }
        public int BatchSize { get; }
        public List<long>  AckSets { get; }
        public MarkDeletePosition MarkDeletePosition { get; }

    }

    public record GetLastMessageIdResponse(IMessageId LastMessageId, IMessageIdAdv MarkDeletePosition);
   
    public record MarkDeletePosition(long LedgerId, long EntryId, int Partition, int BatchIndex, int BatchSize, List<long> AckSets);
    
}
