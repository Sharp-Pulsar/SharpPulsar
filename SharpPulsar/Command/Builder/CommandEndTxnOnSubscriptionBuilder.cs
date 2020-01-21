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
        private CommandEndTxnOnSubscriptionBuilder(CommandEndTxnOnSubscription subscription)
        {
            _subscription = subscription;
        }
        public CommandEndTxnOnSubscriptionBuilder SetRequestId(long requestId)
        {
            _subscription.RequestId = (ulong)requestId;
            return new CommandEndTxnOnSubscriptionBuilder(_subscription);
        }
        public CommandEndTxnOnSubscriptionBuilder SetSubscription(Subscription subscription)
        {
            _subscription.Subscription= subscription;
            return new CommandEndTxnOnSubscriptionBuilder(_subscription);
        }
        public CommandEndTxnOnSubscriptionBuilder SetTxnAction(TxnAction action)
        {
            _subscription.TxnAction = action;
            return new CommandEndTxnOnSubscriptionBuilder(_subscription);
        }
        public CommandEndTxnOnSubscriptionBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            _subscription.TxnidLeastBits = (ulong)txnidLeastBits;
            return new CommandEndTxnOnSubscriptionBuilder(_subscription);
        }
        public CommandEndTxnOnSubscriptionBuilder SetTxnidMostBits(long txnidMostBits)
        {
            _subscription.TxnidMostBits = (ulong)txnidMostBits;
            return new CommandEndTxnOnSubscriptionBuilder(_subscription);
        }
        public CommandEndTxnOnSubscription Build()
        {
            return _subscription;
        }
    }
}
