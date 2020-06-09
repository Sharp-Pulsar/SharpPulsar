
namespace SharpPulsar.Akka.InternalCommands
{
    public class SentReceipt
    {
        public SentReceipt(long producerId, long sequenceId, long highestSequenceId, long entryId, long ledgerId, int batchIndex, int partition)
        {
            ProducerId = producerId;
            SequenceId = sequenceId;
            EntryId = entryId;
            LedgerId = ledgerId;
            BatchIndex = batchIndex;
            Partition = partition;
            HighestSequenceId = highestSequenceId;
        }

        public long ProducerId { get; }
        public long SequenceId { get; }
        public long HighestSequenceId { get; }
        public long EntryId { get; }
        public long LedgerId { get; }
        public int BatchIndex { get; }
        public int Partition { get; }

    }
}
