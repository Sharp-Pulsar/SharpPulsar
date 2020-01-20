using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandSendBuilder
    {
        private readonly CommandSend _send;
        public CommandSendBuilder()
        {
            _send = new CommandSend();
        }
        private CommandSendBuilder(CommandSend send)
        {
            _send = send;
        }
        public CommandSendBuilder SetHighestSequenceId(long highestSequenceId)
        {
            _send.HighestSequenceId = (ulong)highestSequenceId;
            return new CommandSendBuilder(_send);
        }
        public CommandSendBuilder SetNumberOfMessages(int numMessages) 
        {
            if(numMessages > 1)
                _send.NumMessages = numMessages;
            return new CommandSendBuilder(_send);
        }
        public CommandSendBuilder SetProducerId(long producerid)
        {
            _send.ProducerId = (ulong)producerid;
            return new CommandSendBuilder(_send);
        }
        public CommandSendBuilder SetSequenceId(long sequenceId)
        {
            _send.SequenceId = (ulong)sequenceId;
            return new CommandSendBuilder(_send);
        }
        public CommandSendBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            if(txnidLeastBits > 0)
                _send.TxnidLeastBits = (ulong)txnidLeastBits;
            return new CommandSendBuilder(_send);
        }
        public CommandSendBuilder TxnidMostBits(long txnidMostBits)
        {
            if(txnidMostBits > 0)
                _send.TxnidMostBits = (ulong)txnidMostBits;
            return new CommandSendBuilder(_send);
        }
        public CommandSend Build()
        {
            return _send;
        }
    }
}
