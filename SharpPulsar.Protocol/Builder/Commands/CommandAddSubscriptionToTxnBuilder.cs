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
        
        public CommandAddSubscriptionToTxnBuilder SetRequestId(long requestId)
        {
            _txn.RequestId = (ulong)requestId;
            return this;
        }
        public CommandAddSubscriptionToTxnBuilder AddSubscriptions(IList<Subscription> subscription)
        {
            _txn.Subscriptions.AddRange(subscription);
            return this;
        }
        public CommandAddSubscriptionToTxnBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            _txn.TxnidLeastBits = (ulong)txnidLeastBits;
            return this;
        }
        public CommandAddSubscriptionToTxnBuilder SetTxnidMostBits(long txnidMostBits)
        {
            _txn.TxnidMostBits = (ulong)txnidMostBits;
            return this;
        }
        public CommandAddSubscriptionToTxn Build()
        {
            return _txn;
        }
    }
}
