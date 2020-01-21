using SharpPulsar.Common.PulsarApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Command.Builder
{
    public class CommandNewTxnBuilder
    {
        private readonly CommandNewTxn _txn;
        public CommandNewTxnBuilder()
        {
            _txn = new CommandNewTxn();
        }
        private CommandNewTxnBuilder(CommandNewTxn txn)
        {
            _txn = txn;
        }
        public CommandNewTxnBuilder SetRequestId(long requestId)
        {
            _txn.RequestId = (ulong)requestId;
            return new CommandNewTxnBuilder(_txn);
        }
        public CommandNewTxnBuilder SetTcId(long tcId)
        {
            _txn.TcId = (ulong)tcId;
            return new CommandNewTxnBuilder(_txn);
        }
        public CommandNewTxnBuilder SetTxnTtlSeconds(long txnTtlSeconds)
        {
            _txn.TxnTtlSeconds = (ulong)txnTtlSeconds;
            return new CommandNewTxnBuilder(_txn);
        }
        public CommandNewTxn Build()
        {
            return _txn;
        }
    }
}
