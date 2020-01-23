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
        
        public CommandNewTxnBuilder SetRequestId(long requestId)
        {
            _txn.RequestId = (ulong)requestId;
            return this;
        }
        public CommandNewTxnBuilder SetTcId(long tcId)
        {
            _txn.TcId = (ulong)tcId;
            return this;
        }
        public CommandNewTxnBuilder SetTxnTtlSeconds(long txnTtlSeconds)
        {
            _txn.TxnTtlSeconds = (ulong)txnTtlSeconds;
            return this;
        }
        public CommandNewTxn Build()
        {
            return _txn;
        }
    }
}
