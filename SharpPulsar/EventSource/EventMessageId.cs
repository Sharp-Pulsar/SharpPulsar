
namespace SharpPulsar.EventSource
{
    public sealed class EventMessageId
    {
        public EventMessageId(long ledgerId, long entryId, long index)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
            Index = index;
        }

        public long LedgerId { get; }
        public long EntryId { get; }
        public long Index { get; }
    }
}
