
namespace SharpPulsar.Messages
{
    public sealed class AckReceived
    {//equenceId, highestSequenceId, ledgerId, entryId
        public ClientCnx ClientCnx { get; }
        public long SequenceId { get; }
        public long HighestSequenceId { get; }
        public long LedgerId { get; }
        public long EntryId { get; }
        public AckReceived(ClientCnx client, long seq, long highseq, long ledger, long entry)
        {
            ClientCnx = client;
            SequenceId = seq;
            HighestSequenceId = highseq;
            LedgerId = ledger;
            EntryId = entry;
        }
    }
}
