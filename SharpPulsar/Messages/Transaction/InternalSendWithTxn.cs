
using Akka.Actor;
using SharpPulsar.Interfaces;
using System;

namespace SharpPulsar.Messages.Transaction
{
    public class InternalSendWithTxn<T>
    {
        public IMessage<T> Message { get; }
        public IActorRef Txn { get; }
        public Type Type { get; }
        public InternalSendWithTxn(IMessage<T> message, IActorRef txn, Type type)
        {
            Message = message;
            Txn = txn;
            Type = type;
        }
    }
    public class InternalSend<T>
    {
        public IMessage<T> Message { get; }
        public Type Type { get; }
        public InternalSend(IMessage<T> message, Type type)
        {
            Message = message;
            Type = type;
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
