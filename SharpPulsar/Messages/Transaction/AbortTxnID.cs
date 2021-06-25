
using Akka.Actor;
using SharpPulsar.Transaction;

namespace SharpPulsar.Messages.Transaction
{
    public sealed class AbortTxnID
    {
        public TxnID TxnID { get; }
        public IActorRef ReplyTo { get; }
        public AbortTxnID(TxnID txnID, IActorRef replyto)
        {
            TxnID = txnID;
            ReplyTo = replyto;
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
        public IActorRef ReplyTo { get; }
        public CommitTxnID(TxnID txnID, IActorRef replyto)
        {
            TxnID = txnID;
            ReplyTo = replyto;
        }
    }
    public sealed class Commit
    {
        public static Commit Instance = new Commit();
    }
}
