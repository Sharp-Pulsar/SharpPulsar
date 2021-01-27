
using SharpPulsar.Interfaces;
using SharpPulsar.Transaction;
using System.Collections.Generic;

namespace SharpPulsar.Messages.Transaction
{
    public sealed class Abort
    {
        public TxnID TxnID { get; }
        public IList<IMessageId> MessageIds { get; }
        public Abort(TxnID txnID, IList<IMessageId> messageIds)
        {
            TxnID = txnID;
            MessageIds = messageIds;
        }
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
    public sealed class Commit
    {
        public TxnID TxnID { get; }
        public IList<IMessageId> MessageIds { get; }
        public Commit(TxnID txnID, IList<IMessageId> messageIds)
        {
            TxnID = txnID;
            MessageIds = messageIds;
        }
    }
}
