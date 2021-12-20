using System;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Interfaces;

namespace SharpPulsar
{
    internal class SendCallback<T> : ISendCallback<T>
    {
        private readonly IActorRef _outerInstance;

        private TaskCompletionSource<Message<T>> _future;

        private Message<T> _interceptorMessage;
        private ISendCallback<T> _nextCallback;
        private Message<T> _nextMsg;
        private long _createdAt;
        public SendCallback(IActorRef outerInstance, TaskCompletionSource<Message<T>> future, Message<T> interceptorMessage)
        {
            _outerInstance = outerInstance;
            _future = future;
            _interceptorMessage = interceptorMessage;
            _nextCallback = null;
            _nextMsg = null;
            _createdAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public ISendCallback<T> NextSendCallback => _nextCallback;

        public Message<T> NextMessage => _nextMsg;

        public TaskCompletionSource<Message<T>> Future => _future;
        public void SendComplete(Exception e)
        {
            try
            {
                if (e != null)
                {
                    outerInstance.stats.incrementSendFailed();
                    onSendAcknowledgement(_interceptorMessage, null, e);
                    _future.TrySetException(e);
                }
                else
                {
                    onSendAcknowledgement(interceptorMessage, interceptorMessage.getMessageId(), null);
                    _future.TrySetResult(_interceptorMessage);
                    outerInstance.stats.incrementNumAcksReceived(System.nanoTime() - CreatedAt);
                }
            }
            finally
            {
                
            }

            while (_nextCallback != null)
            {
                var sendCallback = _nextCallback;

                var msg = _nextMsg;
                // Retain the buffer used by interceptors callback to get message. Buffer will release after complete interceptors.
                try
                {
                    if (e != null)
                    {
                        outerInstance.stats.incrementSendFailed();
                        onSendAcknowledgement(msg, null, e);
                        sendCallback.Future.TrySetException(e);
                    }
                    else
                    {
                        onSendAcknowledgement(msg, msg.MessageId, null);
                        sendCallback.Future.TrySetResult(msg);
                        outerInstance.stats.incrementNumAcksReceived(System.nanoTime() - CreatedAt);
                    }
                    _nextMsg = _nextCallback.NextMessage;
                    _nextCallback = _nextCallback.NextSendCallback;
                }
                finally
                {
                    
                }
            }
        }

        public void AddCallback(Message<T> msg, ISendCallback<T> scb)
        {
            _nextMsg = msg;
            _nextCallback = scb;
        }
    }
}
