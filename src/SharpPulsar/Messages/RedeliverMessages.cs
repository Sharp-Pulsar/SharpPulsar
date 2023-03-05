
using System.Collections.Immutable;

namespace SharpPulsar.Messages
{
    public readonly record struct RedeliverMessages
    {
        public RedeliverMessages(ImmutableHashSet<Unacked> messages)
        {
            Messages = messages;
        }

        public ImmutableHashSet<Unacked> Messages { get; }
    }

    public readonly record struct Unacked
    {
        public Unacked(long ledgerId, long entryId, int partitionIndex)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
            PartitionIndex = partitionIndex;
        }

        public long LedgerId { get; }
        public long EntryId { get; }
        public int PartitionIndex { get; }
    }
}
