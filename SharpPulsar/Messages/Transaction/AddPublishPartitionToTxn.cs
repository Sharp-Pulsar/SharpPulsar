using SharpPulsar.Transaction;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace SharpPulsar.Messages.Transaction
{
    public sealed class AddPublishPartitionToTxn
    {
        public TxnID TxnID { get; }
        public ImmutableList<string> Topics { get; }
        public AddPublishPartitionToTxn(TxnID txnID, IList<string> topics)
        {
            TxnID = txnID;
            Topics = topics.ToImmutableList();
        }
    }
    public sealed class AddSubscriptionToTxn
    {
        public TxnID TxnID { get; }
        public string Topic { get; }
        public string Subscription { get; }
        public AddSubscriptionToTxn(TxnID txnID, string topic, string subscription)
        {
            TxnID = txnID;
            Topic = topic;
            Subscription = subscription;
        }
    }
}
