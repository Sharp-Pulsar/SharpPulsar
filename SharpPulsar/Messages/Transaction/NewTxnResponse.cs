using BAMCIS.Util.Concurrent;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Transaction;

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
        public long Timeout { get; }
        public TimeUnit TimeUnit { get; }
        public NewTxn(long timeout, TimeUnit unit)
        {
            Timeout = timeout;
            TimeUnit = unit;
        }
    }
    public sealed class EndTxnResponse
    {
        public CommandEndTxnResponse Response { get; }
        public EndTxnResponse(CommandEndTxnResponse response)
        {
            Response = response;
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
