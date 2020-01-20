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
        private CommandSendReceiptBuilder(CommandSendReceipt receipt)
        {
            _receipt = receipt;
        }
        public  CommandSendReceiptBuilder SetProducerId(long producerId)
        {
            _receipt.ProducerId = (ulong)producerId;
            return new CommandSendReceiptBuilder(_receipt);
        }
        public  CommandSendReceiptBuilder SetSequenceId(long sequenceId)
        {
            _receipt.SequenceId = (ulong)sequenceId;
            return new CommandSendReceiptBuilder(_receipt);
        }
        public  CommandSendReceiptBuilder SetHighestSequenceId(long highestId)
        {
            _receipt.HighestSequenceId = (ulong)highestId;
            return new CommandSendReceiptBuilder(_receipt);
        }
        public  CommandSendReceiptBuilder SetMessageId(long ledgerId, long entryId)
        {
            MessageIdDataBuilder messageIdBuilder = new MessageIdDataBuilder()
                .SetLedgerId(ledgerId)
                .SetEntryId(entryId);

            _receipt.MessageId = messageIdBuilder.Build();
            return new CommandSendReceiptBuilder(_receipt);
        }

        public  CommandSendReceipt Build()
        {
            return _receipt;
        }
    }
}
