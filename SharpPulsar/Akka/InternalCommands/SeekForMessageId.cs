
namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class SeekForMessageId
    {
        public SeekForMessageId(long ledgerId, long entryId)
        {
            LedgerId = ledgerId;
            EntryId = entryId;
        }

        public long LedgerId { get; }
        public long EntryId { get; }
    }
    public sealed class TimestampSeek
    {
        public TimestampSeek(long timestamp)
        {
            Timestamp = timestamp;
        }

        public long Timestamp { get; }
    }
}
