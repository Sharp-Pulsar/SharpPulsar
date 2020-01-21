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
        
        public CommandSendBuilder SetHighestSequenceId(long highestSequenceId)
        {
            _send.HighestSequenceId = (ulong)highestSequenceId;
            return this;
        }
        public CommandSendBuilder SetNumberOfMessages(int numMessages) 
        {
            if(numMessages > 1)
                _send.NumMessages = numMessages;
            return this;
        }
        public CommandSendBuilder SetProducerId(long producerid)
        {
            _send.ProducerId = (ulong)producerid;
            return this;
        }
        public CommandSendBuilder SetSequenceId(long sequenceId)
        {
            _send.SequenceId = (ulong)sequenceId;
            return this;
        }
        public CommandSendBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            if(txnidLeastBits > 0)
                _send.TxnidLeastBits = (ulong)txnidLeastBits;
            return this;
        }
        public CommandSendBuilder TxnidMostBits(long txnidMostBits)
        {
            if(txnidMostBits > 0)
                _send.TxnidMostBits = (ulong)txnidMostBits;
            return this;
        }
        public CommandSend Build()
        {
            return _send;
        }
    }
}
