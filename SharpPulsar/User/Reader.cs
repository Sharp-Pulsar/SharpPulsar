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
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SharpPulsar.User
{
    public class Reader<T> : IReader<T>
    {
        private readonly ISchema<T> _schema;
        private readonly ReaderConfigurationData<T> _conf;
        private readonly IActorRef _readerActor;
        private readonly IActorRef _stateActor;
        private readonly ConsumerQueueCollections<T> _queue;

        public Reader(IActorRef stateActor, IActorRef consumer, ConsumerQueueCollections<T> queue, ISchema<T> schema, ReaderConfigurationData<T> conf)
        {
            _stateActor = stateActor;
            _readerActor = consumer;
            _queue = queue;
            _schema = schema;
            _conf = conf;
        }
        public string Topic => TopicAsync().GetAwaiter().GetResult();
        public async ValueTask<string> TopicAsync() => await _readerActor.AskFor<string>(GetTopic.Instance).ConfigureAwait(false);

        public bool Connected => ConnectedAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> ConnectedAsync() => await _readerActor.AskFor<bool>(IsConnected.Instance).ConfigureAwait(false);


        public void Stop()
        {
            _readerActor.GracefulStop(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }

        public bool HasMessageAvailable() => HasMessageAvailableAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> HasMessageAvailableAsync()
        {
            _readerActor.Tell(Messages.Consumer.HasMessageAvailable.Instance);
            var b = _queue.HasMessageAvailable.Take();
            return await Task.FromResult(b).ConfigureAwait(false);
        }

        public bool HasReachedEndOfTopic() => HasReachedEndOfTopicAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> HasReachedEndOfTopicAsync()
        {
            _readerActor.Tell(Messages.Consumer.HasReachedEndOfTopic.Instance);
            var b = _queue.HasReachedEndOfTopic.Take();
            return await Task.FromResult(b).ConfigureAwait(false);
        }
        public IMessage<T> ReadNext() => ReadNextAsync().GetAwaiter().GetResult();
        public async ValueTask<IMessage<T>> ReadNextAsync()
        {
            await VerifyConsumerState().ConfigureAwait(false);
            if (_queue.IncomingMessages.TryReceive(out var m))
            {
                _readerActor.Tell(new AcknowledgeCumulativeMessage<T>(m));
            }
            return m;
        }
        public IMessage<T> ReadNext(int timeout, TimeUnit unit) => ReadNextAsync(timeout, unit).GetAwaiter().GetResult();
        public async ValueTask<IMessage<T>> ReadNextAsync(int timeout, TimeUnit unit)
        {
            await VerifyConsumerState().ConfigureAwait(false);
            if (_conf.ReceiverQueueSize == 0)
            {
                throw new PulsarClientException.InvalidConfigurationException("Can't use receive with timeout, if the queue size is 0");
            }
            try
            {
                var message = await _queue.IncomingMessages.ReceiveAsync(timeout: TimeSpan.FromMilliseconds(timeout));
                if (message != null)
                {
                    _readerActor.Tell(new AcknowledgeCumulativeMessage<T>(message));
                    _readerActor.Tell(new MessageProcessed<T>(message));
                    return message;
                }
            }
            catch { }
            return null;
        }
        private async ValueTask VerifyConsumerState()
        {
            var result = await _stateActor.AskFor<HandlerStateResponse>(GetHandlerState.Instance).ConfigureAwait(false);
            var state = result.State;

            var retry = 10;
            while (state == HandlerState.State.Uninitialized && retry > 0)
            {
                result = await _stateActor.AskFor<HandlerStateResponse>(GetHandlerState.Instance).ConfigureAwait(false);
                state = result.State;
                retry--;
                if (state == HandlerState.State.Uninitialized || state == HandlerState.State.Failed)
                    await Task.Delay(TimeSpan.FromSeconds(5));
            }
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
            => SeekAsync(messageId).GetAwaiter().GetResult();
        public async ValueTask SeekAsync(IMessageId messageId)
        {
            var result = await _stateActor.AskFor<HandlerStateResponse>(GetHandlerState.Instance).ConfigureAwait(false);
            var state = result.State;
            if (state == HandlerState.State.Closing || state == HandlerState.State.Closed)
            {
                throw new PulsarClientException.AlreadyClosedException($"The consumer was already closed when seeking the subscription of the topic {Topic} to the message {messageId}");

            }

            if (!await ConnectedAsync().ConfigureAwait(false))
            {
                throw new PulsarClientException($"The client is not connected to the broker when seeking the subscription of the topic {Topic} to the message {messageId}");

            }
            _readerActor.Tell(new SeekMessageId(messageId));
            if (_queue.SeekException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }
        public void Seek(long timestamp) 
            => SeekAsync(timestamp).GetAwaiter().GetResult();
        public async ValueTask SeekAsync(long timestamp)
        {
            var result = await _stateActor.AskFor<HandlerStateResponse>(GetHandlerState.Instance).ConfigureAwait(false);
            var state = result.State;
            if (state == HandlerState.State.Closing || state == HandlerState.State.Closed)
            {
                throw new Exception($"The reader was already closed when seeking the subscription of the topic {Topic} to the timestamp {timestamp:D}");

            }

            if (!await ConnectedAsync().ConfigureAwait(false))
            {
                throw new Exception($"The client is not connected to the broker when seeking the subscription of the topic {Topic} to the timestamp {timestamp:D}");
            }
            _readerActor.Tell(new SeekTimestamp(timestamp));
            if (_queue.SeekException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

    }
}
