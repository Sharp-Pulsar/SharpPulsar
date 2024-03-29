﻿
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Transaction
{
    public readonly record struct InternalSendWithTxn<T>
    {
        public IMessage<T> Message { get; }
        public IActorRef Txn { get; }
        public TaskCompletionSource<IMessageId> Callback { get; }
        public InternalSendWithTxn(IMessage<T> message, IActorRef txn, TaskCompletionSource<IMessageId> callback)
        {
            Message = message;
            Txn = txn;
            Callback = callback;
        }
    }
    public readonly record struct InternalSend<T>
    {
        public IMessage<T> Message { get; }
        public TaskCompletionSource<IMessageId> Callback { get; }
        public InternalSend(IMessage<T> message, TaskCompletionSource<IMessageId> callback)
        {
            Message = message;
            Callback = callback;
        }
    }
}
