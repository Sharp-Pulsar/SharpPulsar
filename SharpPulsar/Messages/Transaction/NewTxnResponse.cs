using SharpPulsar.Exceptions;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages.Transaction
{
    public sealed class NewTxnResponse
    {
        public CommandNewTxnResponse Response { get; }
        public TransactionCoordinatorClientException Error { get; }
        public NewTxnResponse(CommandNewTxnResponse response, TransactionCoordinatorClientException error)
        {
            Response = response;
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
        public CommandEndTxnResponse Response { get; }
        public TransactionCoordinatorClientException Error{get;}
        public EndTxnResponse(CommandEndTxnResponse response)
        {
            Response = response;
        }
        public EndTxnResponse(TransactionCoordinatorClientException error)
        {
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
