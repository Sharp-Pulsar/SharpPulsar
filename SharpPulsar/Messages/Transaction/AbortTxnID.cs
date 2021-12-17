using SharpPulsar.Transaction;

namespace SharpPulsar.Messages.Transaction
{
    public sealed class AbortTxnID
    {
        public TxnID TxnID { get; }

        public AbortTxnID(TxnID txnID)
        {
            TxnID = txnID;
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
        public CommitTxnID(TxnID txnID)
        {
            TxnID = txnID;
        }
    }
    public sealed class Commit
    {
        public static Commit Instance = new Commit();
    }
}
