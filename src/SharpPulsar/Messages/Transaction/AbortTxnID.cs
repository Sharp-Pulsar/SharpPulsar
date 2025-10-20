namespace SharpPulsar.Messages.Transaction
{
    public record AbortTxnID
    {
        public TxnID TxnID { get; }

        public AbortTxnID(TxnID txnID)
        {
            TxnID = txnID;
        }
    }
    public record Abort
    {
        public static Abort Instance = new Abort();
    }
    public record RegisterAckedTopic
    {
        public string Topic { get; }
        public string Subscription { get; }
        public RegisterAckedTopic(string topic, string subscription)
        {
            Topic = topic;
            Subscription = subscription;
        }
    }
    public record CommitTxnID
    {
        public TxnID TxnID { get; }
        public CommitTxnID(TxnID txnID)
        {
            TxnID = txnID;
        }
    }
    public record Commit
    {
        public static Commit Instance = new Commit();
    }

    public record TransState
    {
        public static TransactionState Instance = new TransactionState();
    }
}
