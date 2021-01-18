
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
    public sealed class GetTxnIdLeastBits
    {
        public static GetTxnIdLeastBits Instance = new GetTxnIdLeastBits();
    }
    public sealed class GetTxnIdMostBits
    {
        public static GetTxnIdMostBits Instance = new GetTxnIdMostBits();
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
