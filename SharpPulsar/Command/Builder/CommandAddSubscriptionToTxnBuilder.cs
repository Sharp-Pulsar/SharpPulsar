using SharpPulsar.Common.PulsarApi;
using System.Collections.Generic;

namespace SharpPulsar.Command.Builder
{
    public class CommandAddSubscriptionToTxnBuilder
    {
        private readonly CommandAddSubscriptionToTxn _txn;
        public CommandAddSubscriptionToTxnBuilder()
        {
            _txn = new CommandAddSubscriptionToTxn();
        }
        private CommandAddSubscriptionToTxnBuilder(CommandAddSubscriptionToTxn txn)
        {
            _txn = txn;
        }
        public CommandAddSubscriptionToTxnBuilder SetRequestId(long requestId)
        {
            _txn.RequestId = (ulong)requestId;
            return new CommandAddSubscriptionToTxnBuilder(_txn);
        }
        public CommandAddSubscriptionToTxnBuilder AddSubscriptions(IList<Subscription> subscription)
        {
            _txn.Subscriptions.AddRange(subscription);
            return new CommandAddSubscriptionToTxnBuilder(_txn);
        }
        public CommandAddSubscriptionToTxnBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            _txn.TxnidLeastBits = (ulong)txnidLeastBits;
            return new CommandAddSubscriptionToTxnBuilder(_txn);
        }
        public CommandAddSubscriptionToTxnBuilder SetTxnidMostBits(long txnidMostBits)
        {
            _txn.TxnidMostBits = (ulong)txnidMostBits;
            return new CommandAddSubscriptionToTxnBuilder(_txn);
        }
        public CommandAddSubscriptionToTxn Build()
        {
            return _txn;
        }
    }
}
