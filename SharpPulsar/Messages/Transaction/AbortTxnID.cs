
using SharpPulsar.Interfaces;
using SharpPulsar.Transaction;
using System.Collections.Generic;

namespace SharpPulsar.Messages.Transaction
{
    public sealed class AbortTxnID
    {
        public TxnID TxnID { get; }
        public IList<IMessageId> MessageIds { get; }
        public AbortTxnID(TxnID txnID, IList<IMessageId> messageIds)
        {
            TxnID = txnID;
            MessageIds = messageIds;
        }
    }
    public sealed class Abort
    {
        public static Abort Instance = new Abort();
    }
    public sealed class RegisterAckedTopic
    {
        public string Topic { get; }
        public string Subscription { get; }
        public RegisterAckedTopic(string topic, string subscription)
        {
            Topic = topic;
            Subscription = subscription;
        }
    }
    public sealed class CommitTxnID
    {
        public TxnID TxnID { get; }
        public IList<IMessageId> MessageIds { get; }
        public CommitTxnID(TxnID txnID, IList<IMessageId> messageIds)
        {
            TxnID = txnID;
            MessageIds = messageIds;
        }
    }
    public sealed class Commit
    {
        public static Commit Instance = new Commit();
    }
}
