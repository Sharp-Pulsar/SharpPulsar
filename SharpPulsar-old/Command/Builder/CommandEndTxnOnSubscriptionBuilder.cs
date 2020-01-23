using SharpPulsar.Common.PulsarApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Command.Builder
{
    public class CommandEndTxnOnSubscriptionBuilder
    {
        private CommandEndTxnOnSubscription _subscription;
        public CommandEndTxnOnSubscriptionBuilder()
        {
            _subscription = new CommandEndTxnOnSubscription();
        }
        
        public CommandEndTxnOnSubscriptionBuilder SetRequestId(long requestId)
        {
            _subscription.RequestId = (ulong)requestId;
            return this;
        }
        public CommandEndTxnOnSubscriptionBuilder SetSubscription(Subscription subscription)
        {
            _subscription.Subscription= subscription;
            return this;
        }
        public CommandEndTxnOnSubscriptionBuilder SetTxnAction(TxnAction action)
        {
            _subscription.TxnAction = action;
            return this;
        }
        public CommandEndTxnOnSubscriptionBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            _subscription.TxnidLeastBits = (ulong)txnidLeastBits;
            return this;
        }
        public CommandEndTxnOnSubscriptionBuilder SetTxnidMostBits(long txnidMostBits)
        {
            _subscription.TxnidMostBits = (ulong)txnidMostBits;
            return this;
        }
        public CommandEndTxnOnSubscription Build()
        {
            return _subscription;
        }
    }
}
