using Akka.Actor;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Reader;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Queues;
using System;

namespace SharpPulsar.User
{
    public class Reader<T> : IReader<T>
    {
        private readonly ISchema<T> _schema;
        private readonly ReaderConfigurationData<T> _conf;
        private readonly IActorRef _readerActor;
        private readonly ConsumerQueueCollections<T> _queue;

        public Reader(IActorRef consumer, ConsumerQueueCollections<T> queue, ISchema<T> schema, ReaderConfigurationData<T> conf)
        {
            _readerActor = consumer;
            _queue = queue;
            _schema = schema;
            _conf = conf;
        }
        public string Topic => _readerActor.AskFor<string>(GetTopic.Instance);

        public bool Connected => _readerActor.AskFor<bool>(IsConnected.Instance);


        public void Stop()
        {
            _readerActor.GracefulStop(TimeSpan.FromSeconds(5));
        }

        public bool HasMessageAvailable()
        {
            _readerActor.Tell(Messages.Consumer.HasMessageAvailable.Instance);
            return _queue.HasMessageAvailable.Take();
        }

        public bool HasReachedEndOfTopic()
        {
            _readerActor.Tell(Messages.Consumer.HasReachedEndOfTopic.Instance);
            return _queue.HasReachedEndOfTopic.Take();
        }

        public IMessage<T> ReadNext()
        {
            VerifyConsumerState();
            if (_queue.IncomingMessages.TryTake(out var m))
            {
                _readerActor.Tell(new AcknowledgeCumulativeMessage<T>(m));
                return m;
            }                

            return null;
        }
        public IMessage<T> ReadNext(int timeout, TimeUnit unit)
        {
            VerifyConsumerState();
            if (_conf.ReceiverQueueSize == 0)
            {
                throw new PulsarClientException.InvalidConfigurationException("Can't use receive with timeout, if the queue size is 0");
            }
            if (_queue.IncomingMessages.TryTake(out var message, (int)unit.ToMilliseconds(timeout)))
            {
                _readerActor.Tell(new AcknowledgeCumulativeMessage<T>(message));
                _readerActor.Tell(new MessageProcessed<T>(message));
                return message;
            }
            return null;
        }
        protected internal void VerifyConsumerState()
        {
            var state = _readerActor.AskFor<HandlerStateResponse>(GetHandlerState.Instance).State;
            switch (state)
            {
                case HandlerState.State.Ready:
                case HandlerState.State.Connecting:
                    break; // Ok
                    goto case HandlerState.State.Closing;
                case HandlerState.State.Closing:
                case HandlerState.State.Closed:
                    throw new PulsarClientException.AlreadyClosedException("Consumer already closed");
                case HandlerState.State.Terminated:
                    throw new PulsarClientException.AlreadyClosedException("Topic was terminated");
                case HandlerState.State.Failed:
                case HandlerState.State.Uninitialized:
                    throw new PulsarClientException.NotConnectedException();
                default:
                    break;
            }
        }
        public void Seek(IMessageId messageId)
        {
            _readerActor.Tell(new SeekMessageId(messageId));
            if (_queue.SeekException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void Seek(long timestamp)
        {
            _readerActor.Tell(new SeekTimestamp(timestamp));
            if (_queue.SeekException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

    }
}
