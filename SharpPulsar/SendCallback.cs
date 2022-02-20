using System;
using System.Threading.Tasks;
using SharpPulsar.Interfaces;

namespace SharpPulsar
{
    internal class SendCallback<T> : ISendCallback<T>
    {
        private readonly ProducerActor<T> _outerInstance;

        private readonly TaskCompletionSource<Message<T>> _future;

        private readonly Message<T> _interceptorMessage;
        private ISendCallback<T> _nextCallback;
        private Message<T> _nextMsg;
        private readonly long _createdAt;
        public SendCallback(ProducerActor<T> outerInstance, TaskCompletionSource<Message<T>> future, Message<T> interceptorMessage)
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
                    _outerInstance._stats.IncrementSendFailed();
                    _outerInstance.OnSendAcknowledgement(_interceptorMessage, null, e);
                    _future.TrySetException(e);
                }
                else
                {
                    _outerInstance.OnSendAcknowledgement(_interceptorMessage, _interceptorMessage.MessageId, null);
                    _future.TrySetResult(_interceptorMessage);
                    _outerInstance._stats.IncrementNumAcksReceived(DateTimeOffset.Now.ToUnixTimeMilliseconds() - _createdAt);
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
                        _outerInstance._stats.IncrementSendFailed();
                        _outerInstance.OnSendAcknowledgement(msg, null, e);
                        sendCallback.Future.TrySetException(e);
                    }
                    else
                    {
                        _outerInstance.OnSendAcknowledgement(msg, msg.MessageId, null);
                        sendCallback.Future.TrySetResult(msg);
                        _outerInstance._stats.IncrementNumAcksReceived(DateTimeOffset.Now.ToUnixTimeMilliseconds() - _createdAt);
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
