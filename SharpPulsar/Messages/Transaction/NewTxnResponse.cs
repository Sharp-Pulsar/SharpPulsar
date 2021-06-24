using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages.Transaction
{
    public sealed class NewTxnResponse
    {
        public CommandNewTxnResponse Response { get; }
        public NewTxnResponse(CommandNewTxnResponse response)
        {
            Response = response;
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

        public ServerError Error { get; }
        public string Message { get; }
        public EndTxnResponse(long requestId, long leastBits, long mostBits, ServerError error, string message)
        {
            RequestId = requestId;
            LeastBits = leastBits;
            MostBits = mostBits;
            Error = error;
            Message = message;
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
