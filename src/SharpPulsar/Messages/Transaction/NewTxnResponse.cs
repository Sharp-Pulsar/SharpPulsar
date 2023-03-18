using SharpPulsar.Exceptions;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages.Transaction
{
    public readonly record struct NewTxnResponse
    {
        public CommandNewTxnResponse Response { get; }
        public TransactionCoordinatorClientException Error { get; }
        public NewTxnResponse(CommandNewTxnResponse response, TransactionCoordinatorClientException error)
        {
            Response = response;
            Error = error;
        }
    }
    public readonly record struct NewTxn
    {
        public long TxnRequestTimeoutMs { get; }
        public NewTxn(long txnRequestTimeoutMs)
        {
            TxnRequestTimeoutMs = txnRequestTimeoutMs;
        }
    }
    public readonly record struct EndTxnResponse
    {
        public CommandEndTxnResponse Response { get; }
        public TransactionCoordinatorClientException Error{get;}
        public EndTxnResponse(CommandEndTxnResponse response) => (Response, Error) = (response, null);
        public EndTxnResponse(TransactionCoordinatorClientException error) => (Response, Error) = (null, error);
    }
    public readonly record struct AddPublishPartitionToTxnResponse
    {
        public CommandAddPartitionToTxnResponse Response { get; }
        public AddPublishPartitionToTxnResponse(CommandAddPartitionToTxnResponse response)
        {
            Response = response;
        }
    }
    public readonly record struct AddSubscriptionToTxnResponse
    {
        public CommandAddSubscriptionToTxnResponse Response { get; }
        public AddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse response)
        {
            Response = response;
        }
    }

}
