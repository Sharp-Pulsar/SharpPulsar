using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandEndTxnOnPartitionBuilder
    {
        private CommandEndTxnOnPartition _partition;
        public CommandEndTxnOnPartitionBuilder()
        {
            _partition = new CommandEndTxnOnPartition();
        }
        
        public CommandEndTxnOnPartitionBuilder SetRequestId(long requestId)
        {
            _partition.RequestId = (ulong)requestId;
            return this;
        }
        public CommandEndTxnOnPartitionBuilder SetTopic(string topic)
        {
            _partition.Topic = topic;
            return this;
        }
        public CommandEndTxnOnPartitionBuilder SetTxnAction(TxnAction action)
        {
            _partition.TxnAction = action;
            return this;
        }
        public CommandEndTxnOnPartitionBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            _partition.TxnidLeastBits = (ulong)txnidLeastBits;
            return this;
        }
        public CommandEndTxnOnPartitionBuilder SetTxnidMostBits(long txnidMostBits)
        {
            _partition.TxnidMostBits = (ulong)txnidMostBits;
            return this;
        }
        public CommandEndTxnOnPartition Build()
        {
            return _partition;
        }
    }
}
