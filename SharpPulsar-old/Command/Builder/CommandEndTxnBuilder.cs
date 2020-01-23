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
        
        public CommandEndTxnBuilder SetRequestId(long requestId)
        {
            _txn.RequestId = (ulong)requestId;
            return this;
        }
        public CommandEndTxnBuilder SetTxnAction(TxnAction action)
        {
            _txn.TxnAction = action;
            return this;
        }
        public CommandEndTxnBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            _txn.TxnidLeastBits = (ulong)txnidLeastBits;
            return this;
        }
        public CommandEndTxnBuilder SetTxnidMostBits(long txnidMostBits)
        {
            _txn.TxnidMostBits = (ulong)txnidMostBits;
            return this;
        }
        public CommandEndTxn Build()
        {
            return _txn;
        }
    }
}
