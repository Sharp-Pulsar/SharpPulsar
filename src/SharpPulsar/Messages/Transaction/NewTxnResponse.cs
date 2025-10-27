using Pulsar.Proto;
using SharpPulsar.Shared.Exceptions;

namespace SharpPulsar.Messages.Transaction
{
    public record NewTxnResponse
    {
        public CommandNewTxnResponse Response { get; }
        public TransactionCoordinatorClientException Error { get; }
        public NewTxnResponse(CommandNewTxnResponse response, TransactionCoordinatorClientException error)
        {
            Response = response;
            Error = error;
        }
    }
    public record NewTxn
    {
        public long TxnRequestTimeoutMs { get; }
        public NewTxn(long txnRequestTimeoutMs)
        {
            TxnRequestTimeoutMs = txnRequestTimeoutMs;
        }
    }
    public record EndTxnResponse
    {
        public CommandEndTxnResponse Response { get; }
        public TransactionCoordinatorClientException Error{get;}
        public EndTxnResponse(CommandEndTxnResponse response) => (Response, Error) = (response, null);
        public EndTxnResponse(TransactionCoordinatorClientException error) => (Response, Error) = (null, error);
    }
    public record AddPublishPartitionToTxnResponse
    {
        public CommandAddPartitionToTxnResponse Response { get; }
        public AddPublishPartitionToTxnResponse(CommandAddPartitionToTxnResponse response)
        {
            Response = response;
        }
    }
    public record AddSubscriptionToTxnResponse
    {
        public CommandAddSubscriptionToTxnResponse Response { get; }
        public AddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse response)
        {
            Response = response;
        }
    }

}
