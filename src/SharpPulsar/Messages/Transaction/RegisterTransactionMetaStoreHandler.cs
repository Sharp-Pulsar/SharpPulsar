using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages.Transaction
{
    public readonly record struct RegisterTransactionMetaStoreHandler
    {
        public long TransactionCoordinatorId { get; }
        public IActorRef Coordinator { get; }
        public RegisterTransactionMetaStoreHandler(long cid, IActorRef coord)
        {
            TransactionCoordinatorId = cid;
            Coordinator = coord;
        }
    }
    public readonly record struct RemoveTopicListWatcher
    {
        public long WatcherId { get; }
        public RemoveTopicListWatcher(long watcherid)
        {
            WatcherId = watcherid;
        }
    }
    public readonly record struct RegisterTopicListWatcher
    {
        public long WatcherId { get; }
        public IActorRef Watcher { get; }
        public RegisterTopicListWatcher(long watcherid, IActorRef watcher)
        {
            WatcherId = watcherid;
            Watcher = watcher;
        }
    }
    public readonly record struct RegisterProducedTopic
    {
        public string Topic { get; }
        public RegisterProducedTopic(string topic)
        {
            Topic = topic;
        }
    }
    public readonly record struct RegisterProducedTopicResponse
    {
        public ServerError? Error { get; }
        public RegisterProducedTopicResponse(ServerError? error)
        {
            Error = error;
        }
    }
    public readonly record struct RegisterCumulativeAckConsumer
    {
        public IActorRef Consumer { get; }
        public RegisterCumulativeAckConsumer(IActorRef consumer)
        {
            Consumer = consumer;
        }
    }
    public readonly record struct NextSequenceId
    {
        public static NextSequenceId Instance = new NextSequenceId();
    }
    public readonly record struct GetTxnIdBits
    {
        public static GetTxnIdBits Instance = new GetTxnIdBits();
    }
    public readonly record struct GetTxnIdBitsResponse
    {
        public long MostBits { get; }
        public long LeastBits { get; }
        public GetTxnIdBitsResponse(long mostBits, long leastBits)
        {
            MostBits = mostBits;
            LeastBits = leastBits;
        }
    }
    public readonly record struct StartTransactionCoordinatorClient
    {
        public IActorRef Client { get; }
        public StartTransactionCoordinatorClient(IActorRef client)
        {
            Client = client;
        }
    }
    public readonly record struct RegisterSendOp
    {
        public IMessageId MessageId { get; }
        public RegisterSendOp(IMessageId messageId)
        {
            MessageId = messageId;

        }
    }
    public readonly record struct RegisterAckOp
    {
        public TaskCompletionSource<Task> Task { get; }
        public RegisterAckOp(TaskCompletionSource<Task> task)
        {
           Task = task;
        }
    }
}
