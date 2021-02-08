
using Akka.Actor;
using SharpPulsar.Interfaces;
using System;

namespace SharpPulsar.Messages.Transaction
{
    public class InternalSendWithTxn<T>
    {
        public IMessage<T> Message { get; }
        public IActorRef Txn { get; }
        public bool IsDeadLetter { get; }
        public InternalSendWithTxn(IMessage<T> message, IActorRef txn, bool isDeadLetter = false)
        {
            Message = message;
            Txn = txn;
            IsDeadLetter  = isDeadLetter;
        }
    }
    public class InternalSend<T>
    {
        public IMessage<T> Message { get; }
        public bool IsDeadLetter { get; }
        public InternalSend(IMessage<T> message, bool isDeadLetter = false)
        {
            Message = message;
            IsDeadLetter = isDeadLetter;
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
