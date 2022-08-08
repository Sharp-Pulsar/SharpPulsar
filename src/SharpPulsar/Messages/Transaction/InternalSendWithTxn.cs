
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Transaction
{
    public class InternalSendWithTxn<T>
    {
        public IMessage<T> Message { get; }
        public IActorRef Txn { get; }
        public TaskCompletionSource<Message<T>> Callback { get; }
        public InternalSendWithTxn(IMessage<T> message, IActorRef txn, TaskCompletionSource<Message<T>> callback)
        {
            Message = message;
            Txn = txn;
            Callback = callback;
        }
    }
    public class InternalSend<T>
    {
        public IMessage<T> Message { get; }
        public TaskCompletionSource<Message<T>> Callback { get; }
        public InternalSend(IMessage<T> message, TaskCompletionSource<Message<T>> callback)
        {
            Message = message;
            Callback = callback;
        }
    }
}
