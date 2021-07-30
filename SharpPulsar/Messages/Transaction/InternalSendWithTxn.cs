
using Akka.Actor;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Transaction
{
    public class InternalSendWithTxn<T>
    {
        public IMessage<T> Message { get; }
        public IActorRef Txn { get; }
        public InternalSendWithTxn(IMessage<T> message, IActorRef txn)
        {
            Message = message;
            Txn = txn;
        }
    }
    public class InternalSend<T>
    {
        public IMessage<T> Message { get; }
        public InternalSend(IMessage<T> message)
        {
            Message = message;
        }
    }
    public class InternalSendResponse
    {
        public IMessageId MessageId { get; }
        public InternalSendResponse(IMessageId messageId)
        {
            MessageId = messageId;
        }
    }
}
