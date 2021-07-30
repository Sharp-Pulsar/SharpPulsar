
using Akka.Actor;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Transaction
{
    public sealed class RegisterTransactionMetaStoreHandler
    {
        public long TransactionCoordinatorId { get; }
        public IActorRef Coordinator { get; }
        public RegisterTransactionMetaStoreHandler(long cid, IActorRef coord)
        {
            TransactionCoordinatorId = cid;
            Coordinator = coord;
        }
    }
    public sealed class RegisterProducedTopic
    {
        public string Topic { get; }
        public RegisterProducedTopic(string topic)
        {
            Topic = topic;
        }
    }
    public sealed class RegisterCumulativeAckConsumer
    {
        public IActorRef Consumer { get; }
        public RegisterCumulativeAckConsumer(IActorRef consumer)
        {
            Consumer = consumer;
        }
    }
    public sealed class NextSequenceId
    {
        public static NextSequenceId Instance = new NextSequenceId();
    }
    public sealed class GetTxnIdBits
    {
        public static GetTxnIdBits Instance = new GetTxnIdBits();
    }
    public sealed class GetTxnIdBitsResponse
    {
        public long MostBits { get; }
        public long LeastBits { get; }
        public GetTxnIdBitsResponse(long mostBits, long leastBits)
        {
            MostBits = mostBits;
            LeastBits = leastBits;
        }
    }
    public sealed class StartTransactionCoordinatorClient
    {
        public IActorRef Client { get; }
        public StartTransactionCoordinatorClient(IActorRef client)
        {
            Client = client;
        }
    }
    public sealed class RegisterSendOp
    {
        public IMessageId MessageId { get; }
        public RegisterSendOp(IMessageId messageId)
        {
            MessageId = messageId;

        }
    }
}
