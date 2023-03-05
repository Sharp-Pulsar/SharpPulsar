using SharpPulsar.TransactionImpl;

namespace SharpPulsar.Messages.Transaction
{
    public readonly record struct AbortTxnID
    {
        public TxnID TxnID { get; }

        public AbortTxnID(TxnID txnID)
        {
            TxnID = txnID;
        }
    }
    public readonly record struct Abort
    {
        public static Abort Instance = new Abort();
    }
    public readonly record struct RegisterAckedTopic
    {
        public string Topic { get; }
        public string Subscription { get; }
        public RegisterAckedTopic(string topic, string subscription)
        {
            Topic = topic;
            Subscription = subscription;
        }
    }
    public readonly record struct CommitTxnID
    {
        public TxnID TxnID { get; }
        public CommitTxnID(TxnID txnID)
        {
            TxnID = txnID;
        }
    }
    public readonly record struct Commit
    {
        public static Commit Instance = new Commit();
    }

    public readonly record struct TransState
    {
        public static TransactionState Instance = new TransactionState();
    }
}
