namespace SharpPulsar.Messages
{
    public record AckReceived
    {
        public long SequenceId { get; }
        public long HighestSequenceId { get; }
        public long LedgerId { get; }
        public long EntryId { get; }
        public AckReceived(long seq, long highseq, long ledger, long entry)
        {
            SequenceId = seq;
            HighestSequenceId = highseq;
            LedgerId = ledger;
            EntryId = entry;
        }
    }
}
