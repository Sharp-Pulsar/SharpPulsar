using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandSendReceiptBuilder
    {
        private readonly CommandSendReceipt _receipt;
        public CommandSendReceiptBuilder()
        {
            _receipt = new CommandSendReceipt();
        }
        
        public  CommandSendReceiptBuilder SetProducerId(long producerId)
        {
            _receipt.ProducerId = (ulong)producerId;
            return this;
        }
        public  CommandSendReceiptBuilder SetSequenceId(long sequenceId)
        {
            _receipt.SequenceId = (ulong)sequenceId;
            return this;
        }
        public  CommandSendReceiptBuilder SetHighestSequenceId(long highestId)
        {
            _receipt.HighestSequenceId = (ulong)highestId;
            return this;
        }
        public  CommandSendReceiptBuilder SetMessageId(long ledgerId, long entryId)
        {
            MessageIdDataBuilder messageIdBuilder = new MessageIdDataBuilder()
                .SetLedgerId(ledgerId)
                .SetEntryId(entryId);

            _receipt.MessageId = messageIdBuilder.Build();
            return this;
        }

        public  CommandSendReceipt Build()
        {
            return _receipt;
        }
    }
}
