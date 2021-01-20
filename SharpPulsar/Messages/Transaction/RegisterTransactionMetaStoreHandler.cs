
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
        public static StartTransactionCoordinatorClient Instance = new StartTransactionCoordinatorClient();
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
