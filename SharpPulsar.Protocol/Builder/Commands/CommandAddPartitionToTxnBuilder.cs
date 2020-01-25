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
       
        public CommandAddPartitionToTxnBuilder SetPartitions(IList<string> parts)
        {
            _txn.Partitions.AddRange(parts);
            return this;
        }
        public CommandAddPartitionToTxnBuilder SetRequestId(long requestId)
        {
            _txn.RequestId = (ulong)requestId;
            return this;
        }
        public CommandAddPartitionToTxnBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            _txn.TxnidLeastBits = (ulong)txnidLeastBits;
            return this;
        }
        public CommandAddPartitionToTxnBuilder SetTxnidMostBits(long txnidMostBits)
        {
            _txn.TxnidMostBits = (ulong)txnidMostBits;
            return this;
        }
        public CommandAddPartitionToTxn Build()
        {
            return _txn;
        }
    }
}
