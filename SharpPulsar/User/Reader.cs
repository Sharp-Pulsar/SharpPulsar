using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SharpPulsar.User
{
    public class Reader<T> : IReader<T>
    {
        private readonly ISchema<T> _schema;
        private readonly ReaderConfigurationData<T> _conf;
        private readonly IActorRef _readerActor;
        private readonly IActorRef _stateActor;

        public Reader(IActorRef stateActor, IActorRef consumer, ISchema<T> schema, ReaderConfigurationData<T> conf)
        {
            _stateActor = stateActor;
            _readerActor = consumer;
            _schema = schema;
            _conf = conf;
        }
        public string Topic => TopicAsync().GetAwaiter().GetResult();
        public async ValueTask<string> TopicAsync() => await _readerActor.Ask<string>(GetTopic.Instance).ConfigureAwait(false);

        public bool Connected => ConnectedAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> ConnectedAsync() => await _readerActor.Ask<bool>(IsConnected.Instance).ConfigureAwait(false);


        public void Stop()
        {
            _readerActor.GracefulStop(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }

        public bool HasMessageAvailable() => HasMessageAvailableAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> HasMessageAvailableAsync()
        {
            var response = await _readerActor.Ask<AskResponse>(Messages.Consumer.HasMessageAvailable.Instance).ConfigureAwait(false);
            if (response.Failed)
                throw response.Exception;

            return response.ConvertTo<bool>();
        }

        public bool HasReachedEndOfTopic() => HasReachedEndOfTopicAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> HasReachedEndOfTopicAsync()
        {
            var response = await _readerActor.Ask<AskResponse>(Messages.Consumer.HasReachedEndOfTopic.Instance).ConfigureAwait(false);
            return response.ConvertTo<bool>();
        }
        public IMessage<T> ReadNext() => ReadNextAsync().GetAwaiter().GetResult();
        public async ValueTask<IMessage<T>> ReadNextAsync()
        {
            return await GetMessage().ConfigureAwait(false);
        }
        public IMessage<T> ReadNext(TimeSpan timeSpan) => ReadNextAsync(timeSpan).GetAwaiter().GetResult();
        public async ValueTask<IMessage<T>> ReadNextAsync(TimeSpan timeSpan)
        {
            if (_conf.ReceiverQueueSize == 0)
            {
                throw new PulsarClientException.InvalidConfigurationException("Can't use receive with timeout, if the queue size is 0");
            }
            return await GetMessage(timeSpan).ConfigureAwait(false);
        }
        public void Seek(IMessageId messageId) 
            => SeekAsync(messageId).GetAwaiter().GetResult();
        public async ValueTask SeekAsync(IMessageId messageId)
        {
            var askForState = await _stateActor.Ask<AskResponse>(GetHandlerState.Instance).ConfigureAwait(false);
            var state = askForState.ConvertTo<HandlerState.State>();
            if (state == HandlerState.State.Closing || state == HandlerState.State.Closed)
            {
                throw new PulsarClientException.AlreadyClosedException($"The consumer was already closed when seeking the subscription of the topic {Topic} to the message {messageId}");

            }

            if (!await ConnectedAsync().ConfigureAwait(false))
            {
                throw new PulsarClientException($"The client is not connected to the broker when seeking the subscription of the topic {Topic} to the message {messageId}");

            }
            var response = await _readerActor.Ask<AskResponse>(new SeekMessageId(messageId)).ConfigureAwait(false);
            if (response.Failed)
                  throw response.Exception;
        }
        public void Seek(long timestamp) 
            => SeekAsync(timestamp).GetAwaiter().GetResult();
        public async ValueTask SeekAsync(long timestamp)
        {
            var askForState = await _stateActor.Ask<AskResponse>(GetHandlerState.Instance).ConfigureAwait(false);
            var state = askForState.ConvertTo<HandlerState.State>();
            if (state == HandlerState.State.Closing || state == HandlerState.State.Closed)
            {
                throw new Exception($"The reader was already closed when seeking the subscription of the topic {Topic} to the timestamp {timestamp:D}");

            }

            if (!await ConnectedAsync().ConfigureAwait(false))
            {
                throw new Exception($"The client is not connected to the broker when seeking the subscription of the topic {Topic} to the timestamp {timestamp:D}");
            }
            var response = await _readerActor.Ask<AskResponse>(new SeekTimestamp(timestamp)).ConfigureAwait(false);
            if (response.Failed)
                throw response.Exception;
        }
        public void Close() => CloseAsync().ConfigureAwait(false);
        public async ValueTask CloseAsync()
        {
            await _readerActor.GracefulStop(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }
        private async ValueTask<IMessage<T>> GetMessage()
        {
            var response = await _readerActor.Ask<AskResponse>(Messages.Consumer.Receive.Instance).ConfigureAwait(false);
            while (true)
            {
                if (response.Failed)
                    throw response.Exception;

                if (response.Data != null)
                {
                    var message = response.ConvertTo<IMessage<T>>();
                    _readerActor.Tell(new AcknowledgeCumulativeMessage<T>(message));
                    _readerActor.Tell(new MessageProcessed<T>(message));
                    return message;
                }
                else
                {
                    await Task.Delay(100);
                    response = await _readerActor.Ask<AskResponse>(Messages.Consumer.Receive.Instance).ConfigureAwait(false);
                }
            }

        }

        private async ValueTask<IMessage<T>> GetMessage(TimeSpan time)
        {
            IMessage<T> message = null; 
            var s = new Stopwatch();
            s.Start();
            while (s.Elapsed < time)
            {
                var response = await _readerActor.Ask<AskResponse>(Messages.Consumer.Receive.Instance).ConfigureAwait(false);
                if (response.Failed)
                    throw response.Exception;

                if (response.Data != null)
                {
                    message = response.ConvertTo<IMessage<T>>();
                    _readerActor.Tell(new AcknowledgeCumulativeMessage<T>(message));
                    _readerActor.Tell(new MessageProcessed<T>(message));
                    break;
                }
            }
            s.Stop();
            return message; 
        }
    }
}
