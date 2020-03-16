
using System.Collections.Immutable;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class RedeliverMessages
    {
        public RedeliverMessages(ImmutableHashSet<Unacked> messages)
        {
            Messages = messages;
        }

        public ImmutableHashSet<Unacked> Messages { get; }
    }

    public sealed class Unacked
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
