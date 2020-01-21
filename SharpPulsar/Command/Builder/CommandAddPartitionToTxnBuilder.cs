using SharpPulsar.Common.PulsarApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Command.Builder
{
    public class CommandAddPartitionToTxnBuilder
    {
        private CommandAddPartitionToTxn _txn;
        public CommandAddPartitionToTxnBuilder()
        {
            _txn = new CommandAddPartitionToTxn();
        }
        private CommandAddPartitionToTxnBuilder(CommandAddPartitionToTxn txn)
        {
            _txn = txn;
        }
        public CommandAddPartitionToTxnBuilder SetPartitions(IList<string> parts)
        {
            _txn.Partitions.AddRange(parts);
            return new CommandAddPartitionToTxnBuilder(_txn);
        }
        public CommandAddPartitionToTxnBuilder SetRequestId(long requestId)
        {
            _txn.RequestId = (ulong)requestId;
            return new CommandAddPartitionToTxnBuilder(_txn);
        }
        public CommandAddPartitionToTxnBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            _txn.TxnidLeastBits = (ulong)txnidLeastBits;
            return new CommandAddPartitionToTxnBuilder(_txn);
        }
        public CommandAddPartitionToTxnBuilder SetTxnidMostBits(long txnidMostBits)
        {
            _txn.TxnidMostBits = (ulong)txnidMostBits;
            return new CommandAddPartitionToTxnBuilder(_txn);
        }
        public CommandAddPartitionToTxn Build()
        {
            return _txn;
        }
    }
}
