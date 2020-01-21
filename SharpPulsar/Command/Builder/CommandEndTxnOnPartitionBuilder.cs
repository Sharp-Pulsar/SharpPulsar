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
        private CommandEndTxnOnPartitionBuilder(CommandEndTxnOnPartition part)
        {
            _partition = part;
        }
        public CommandEndTxnOnPartitionBuilder SetRequestId(long requestId)
        {
            _partition.RequestId = (ulong)requestId;
            return new CommandEndTxnOnPartitionBuilder(_partition);
        }
        public CommandEndTxnOnPartitionBuilder SetTopic(string topic)
        {
            _partition.Topic = topic;
            return new CommandEndTxnOnPartitionBuilder(_partition);
        }
        public CommandEndTxnOnPartitionBuilder SetTxnAction(TxnAction action)
        {
            _partition.TxnAction = action;
            return new CommandEndTxnOnPartitionBuilder(_partition);
        }
        public CommandEndTxnOnPartitionBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            _partition.TxnidLeastBits = (ulong)txnidLeastBits;
            return new CommandEndTxnOnPartitionBuilder(_partition);
        }
        public CommandEndTxnOnPartitionBuilder SetTxnidMostBits(long txnidMostBits)
        {
            _partition.TxnidMostBits = (ulong)txnidMostBits;
            return new CommandEndTxnOnPartitionBuilder(_partition);
        }
        public CommandEndTxnOnPartition Build()
        {
            return _partition;
        }
    }
}
