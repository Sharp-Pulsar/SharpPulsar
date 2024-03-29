﻿using SharpPulsar.Protocol.Proto;
using SharpPulsar.TransactionImpl;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpPulsar.Messages.Transaction
{
    public readonly record struct AddPublishPartitionToTxn
    {
        public TxnID TxnID { get; }
        public ImmutableList<string> Topics { get; }

        public AddPublishPartitionToTxn(TxnID txnID, IList<string> topics)
        {
            TxnID = txnID;
            Topics = topics.ToImmutableList();
        }
    }
    public readonly record struct SubscriptionToTxn
    {
        public TxnID TxnID { get; }
        public string Topic { get; }
        public string Subscription { get; }
        public SubscriptionToTxn(TxnID txnID, string topic, string subscription)
        {
            TxnID = txnID;
            Topic = topic;
            Subscription = subscription;
        }
    }
    public readonly record struct AddSubscriptionToTxn
    {
        public TxnID TxnID { get; }
        public ImmutableList<Subscription> Subscriptions { get; }
        public AddSubscriptionToTxn(TxnID txnID, IList<Subscription> subscriptions)
        {
            TxnID = txnID;
            Subscriptions = subscriptions.ToImmutableList();
        }
    }
}
