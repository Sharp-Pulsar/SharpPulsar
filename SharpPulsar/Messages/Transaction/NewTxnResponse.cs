using SharpPulsar.Exceptions;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages.Transaction
{
    public sealed class NewTxnResponse
    {
        public long RequestId { get; }
        public long MostSigBits { get; }
        public long LeastSigBits { get; }
        public TransactionCoordinatorClientException Error { get; }
        public NewTxnResponse(long requestid, long least, long most, TransactionCoordinatorClientException error)
        {
            RequestId = requestid;
            MostSigBits = most;
            LeastSigBits = least;
            Error = error;
        }
    }
    public sealed class NewTxn
    {
        public long TxnRequestTimeoutMs { get; }
        public NewTxn(long txnRequestTimeoutMs)
        {
            TxnRequestTimeoutMs = txnRequestTimeoutMs;
        }
    }
    public sealed class EndTxnResponse
    {
        public long LeastBits { get; }
        public long MostBits { get; }
        public long RequestId { get; }

        public TransactionCoordinatorClientException Error { get; }
        public EndTxnResponse(long requestId, long leastBits, long mostBits, TransactionCoordinatorClientException error)
        {
            RequestId = requestId;
            LeastBits = leastBits;
            MostBits = mostBits;
            Error = error;
        }
    }
    public sealed class AddPublishPartitionToTxnResponse
    {
        public CommandAddPartitionToTxnResponse Response { get; }
        public AddPublishPartitionToTxnResponse(CommandAddPartitionToTxnResponse response)
        {
            Response = response;
        }
    }
    public sealed class AddSubscriptionToTxnResponse
    {
        public CommandAddSubscriptionToTxnResponse Response { get; }
        public AddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse response)
        {
            Response = response;
        }
    }

}
