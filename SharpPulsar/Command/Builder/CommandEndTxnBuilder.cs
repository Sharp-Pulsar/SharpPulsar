using SharpPulsar.Common.PulsarApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Command.Builder
{
    public class CommandEndTxnBuilder
    {
        private CommandEndTxn _txn;
        public CommandEndTxnBuilder()
        {
            _txn = new CommandEndTxn();
        }
        private CommandEndTxnBuilder(CommandEndTxn txn)
        {
            _txn = txn;
        }
        public CommandEndTxnBuilder SetRequestId(long requestId)
        {
            _txn.RequestId = (ulong)requestId;
            return new CommandEndTxnBuilder(_txn);
        }
        public CommandEndTxnBuilder SetTxnAction(TxnAction action)
        {
            _txn.TxnAction = action;
            return new CommandEndTxnBuilder(_txn);
        }
        public CommandEndTxnBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            _txn.TxnidLeastBits = (ulong)txnidLeastBits;
            return new CommandEndTxnBuilder(_txn);
        }
        public CommandEndTxnBuilder SetTxnidMostBits(long txnidMostBits)
        {
            _txn.TxnidMostBits = (ulong)txnidMostBits;
            return new CommandEndTxnBuilder(_txn);
        }
        public CommandEndTxn Build()
        {
            return _txn;
        }
    }
}
