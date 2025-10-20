using System.Threading.Tasks;
using Akka.Actor;
using Pulsar.Proto;
using SharpPulsar.API;
namespace SharpPulsar.Messages.Transaction
{
    public record RegisterTransactionMetaStoreHandler
    {
        public long TransactionCoordinatorId { get; }
        public IActorRef Coordinator { get; }
        public RegisterTransactionMetaStoreHandler(long cid, IActorRef coord)
        {
            TransactionCoordinatorId = cid;
            Coordinator = coord;
        }
    }
    public record RemoveTopicListWatcher
    {
        public long WatcherId { get; }
        public RemoveTopicListWatcher(long watcherid)
        {
            WatcherId = watcherid;
        }
    }
    public record RegisterTopicListWatcher
    {
        public long WatcherId { get; }
        public IActorRef Watcher { get; }
        public RegisterTopicListWatcher(long watcherid, IActorRef watcher)
        {
            WatcherId = watcherid;
            Watcher = watcher;
        }
    }
    public record RegisterProducedTopic
    {
        public string Topic { get; }
        public RegisterProducedTopic(string topic)
        {
            Topic = topic;
        }
    }
    public record RegisterProducedTopicResponse
    {
        public ServerError? Error { get; }
        public RegisterProducedTopicResponse(ServerError? error)
        {
            Error = error;
        }
    }
    public record RegisterCumulativeAckConsumer
    {
        public IActorRef Consumer { get; }
        public RegisterCumulativeAckConsumer(IActorRef consumer)
        {
            Consumer = consumer;
        }
    }
    public record NextSequenceId
    {
        public static NextSequenceId Instance = new NextSequenceId();
    }
    public record GetTxnIdBits
    {
        public static GetTxnIdBits Instance = new GetTxnIdBits();
    }
    public record GetTxnIdBitsResponse
    {
        public long MostBits { get; }
        public long LeastBits { get; }
        public GetTxnIdBitsResponse(long mostBits, long leastBits)
        {
            MostBits = mostBits;
            LeastBits = leastBits;
        }
    }
    public record StartTransactionCoordinatorClient
    {
        public IActorRef Client { get; }
        public StartTransactionCoordinatorClient(IActorRef client)
        {
            Client = client;
        }
    }
    public record RegisterSendOp
    {
        public IMessageId MessageId { get; }
        public RegisterSendOp(IMessageId messageId)
        {
            MessageId = messageId;

        }
    }
    public record RegisterAckOp
    {
        public TaskCompletionSource<Task> Task { get; }
        public RegisterAckOp(TaskCompletionSource<Task> task)
        {
           Task = task;
        }
    }
}
