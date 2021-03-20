using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Queues;
using SharpPulsar.Stats.Consumer.Api;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
using System.Threading.Tasks.Dataflow;
using System.Runtime.CompilerServices;

namespace SharpPulsar.User
{
    public class Consumer<T> : IConsumer<T>
    {
        //Either of three: Pattern, Multi, Single topic consumer
        private readonly ISchema<T> _schema;
        private readonly ConsumerConfigurationData<T> _conf;
        private readonly IActorRef _consumerActor;
        private readonly IActorRef _stateActor;
        private readonly ConsumerQueueCollections<T> _queue;
        private readonly ConsumerInterceptors<T> _interceptors;
        private readonly CancellationTokenSource _tokenSource;
        private IMessageListener<T> _listener;

        public Consumer(IActorRef stateActor, IActorRef consumer, ConsumerQueueCollections<T> queue, ISchema<T> schema, ConsumerConfigurationData<T> conf, ConsumerInterceptors<T> interceptors)
        {
            _stateActor = stateActor;
            _listener = conf.MessageListener;
            _tokenSource = new CancellationTokenSource();
            _interceptors = interceptors;
            _consumerActor = consumer;
            _queue = queue;
            _schema = schema;
            _conf = conf;
        }
        public string Topic => TopicAsync().GetAwaiter().GetResult();
        public async ValueTask<string> TopicAsync() 
            => await _consumerActor.Ask<string>(GetTopic.Instance).ConfigureAwait(false);

        public string Subscription => SubscriptionAsync().GetAwaiter().GetResult();
        public async ValueTask<string> SubscriptionAsync() 
            => await _consumerActor.Ask<string>(GetSubscription.Instance).ConfigureAwait(false);

        public IConsumerStats Stats => StatsAsync().GetAwaiter().GetResult();
        public async ValueTask<IConsumerStats> StatsAsync() 
            => await _consumerActor.Ask<IConsumerStats>(GetStats.Instance).ConfigureAwait(false);

        public IMessageId LastMessageId => LastMessageIdAsync().GetAwaiter().GetResult();
        public async ValueTask<IMessageId> LastMessageIdAsync() 
            => await _consumerActor.Ask<IMessageId>(GetLastMessageId.Instance).ConfigureAwait(false);

        public bool Connected => ConnectedAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> ConnectedAsync() 
            => await _consumerActor.Ask<bool>(IsConnected.Instance).ConfigureAwait(false);

        public string ConsumerName => ConsumerNameAsync().GetAwaiter().GetResult();
        public async ValueTask<string> ConsumerNameAsync() 
            => await _consumerActor.Ask<string>(GetConsumerName.Instance).ConfigureAwait(false);

        public long LastDisconnectedTimestamp => LastDisconnectedTimestampAsync().GetAwaiter().GetResult();
        public async ValueTask<long> LastDisconnectedTimestampAsync() 
            => await _consumerActor.Ask<long>(GetLastDisconnectedTimestamp.Instance).ConfigureAwait(false);

        public void Acknowledge(IMessage<T> message) => AcknowledgeAsync(message).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IMessage<T> message)
        {
            _consumerActor.Tell(new AcknowledgeMessage<T>(message));
            if (_queue.AcknowledgeException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }
        public int NumMessagesInQueue()
            => _queue.IncomingMessages.Count;
        public void Acknowledge(IMessageId messageId) => AcknowledgeAsync(messageId).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IMessageId messageId)
        {
            _consumerActor.Tell(new AcknowledgeMessageId(messageId));
            if (_queue.AcknowledgeException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }
        public IActorRef ConsumerActor => _consumerActor;
        public void Acknowledge(IMessages<T> messages) => AcknowledgeAsync(messages).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IMessages<T> messages)
        {
            _consumerActor.Tell(new AcknowledgeMessages<T>(messages));
            if (_queue.AcknowledgeException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }
        public void Acknowledge(IList<IMessageId> messageIds) => AcknowledgeAsync(messageIds).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IList<IMessageId> messageIds)
        {
            _consumerActor.Tell(new AcknowledgeMessageIds(messageIds));
            if (_queue.AcknowledgeException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }
        public void Acknowledge(IMessageId messageId, Transaction txn)
            => AcknowledgeAsync(messageId, txn).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IMessageId messageId, Transaction txn)
        {
            _consumerActor.Tell(new AcknowledgeWithTxn(messageId, txn.Txn));
            if (_queue.AcknowledgeException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }
        public void AcknowledgeCumulative(IMessage<T> message) 
            => AcknowledgeCumulativeAsync(message).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeCumulativeAsync(IMessage<T> message)
        {
            _consumerActor.Tell(new AcknowledgeCumulativeMessage<T>(message));
            if (_queue.AcknowledgeCumulativeException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }
        public void AcknowledgeCumulative(IMessageId messageid)
            => AcknowledgeCumulativeAsync(messageid).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeCumulativeAsync(IMessageId messageId)
        {
            _consumerActor.Tell(new AcknowledgeCumulativeMessageId(messageId));
            if (_queue.AcknowledgeCumulativeException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }
        public void AcknowledgeCumulative(IMessageId messageid, Transaction txn)
            => AcknowledgeCumulativeAsync(messageid, txn).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeCumulativeAsync(IMessageId messageId, Transaction txn)
        {
            if (!IsCumulativeAcknowledgementAllowed(_conf.SubscriptionType))
            {
                throw new PulsarClientException.InvalidConfigurationException("Cannot use cumulative acks on a non-exclusive/non-failover subscription");
            }
            _consumerActor.Tell(new AcknowledgeCumulativeTxn(messageId, txn.Txn));
            if (_queue.AcknowledgeCumulativeException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }

        private bool IsCumulativeAcknowledgementAllowed(SubType type)
        {
            return SubType.Shared != type && SubType.KeyShared != type;
        }
        public void Close() => CloseAsync().ConfigureAwait(false);
        public async ValueTask CloseAsync()
        {
            await _consumerActor.GracefulStop(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }
        public bool HasReachedEndOfTopic() 
            => HasReachedEndOfTopicAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> HasReachedEndOfTopicAsync()
        {
            _consumerActor.Tell(Messages.Consumer.HasReachedEndOfTopic.Instance);
            if (_queue.HasReachedEndOfTopic.TryTake(out var reached, 5000))
                return await Task.FromResult(reached);

            return false;
        }

        public void NegativeAcknowledge(IMessage<T> message) 
            => NegativeAcknowledgeAsync(message).GetAwaiter().GetResult();
        public async ValueTask NegativeAcknowledgeAsync(IMessage<T> message)
        {
            _consumerActor.Tell(new NegativeAcknowledgeMessage<T>(message));
            if (_queue.NegativeAcknowledgeException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }

        public void NegativeAcknowledge(IMessages<T> messages) 
            => NegativeAcknowledgeAsync(messages).GetAwaiter().GetResult();
        public async ValueTask NegativeAcknowledgeAsync(IMessages<T> messages)
        {
            _consumerActor.Tell(new NegativeAcknowledgeMessages<T>(messages));
            if (_queue.NegativeAcknowledgeException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }

        public void Pause()
        {
            _consumerActor.Tell(Messages.Consumer.Pause.Instance);
        }
        public IMessage<T> Receive(TimeSpan? timeout = null)
        {
            return ReceiveAsync(timeout).GetAwaiter().GetResult();
        }

        public async ValueTask<IMessage<T>> ReceiveAsync(TimeSpan? timeouts = null)
        {
            await VerifyConsumerState();
            if (_conf.ReceiverQueueSize == 0)
            {
                throw new PulsarClientException.InvalidConfigurationException("Can't use receive with timeout, if the queue size is 0");
            }
            if (_conf.MessageListener != null)
            {
                throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
            }

            if (_queue.IncomingMessages.TryReceive(out var message))
            {
                _consumerActor.Tell(new MessageProcessed<T>(message));
                var inteceptedMessage = BeforeConsume(message);
                return await Task.FromResult(inteceptedMessage);
            }
            else if(timeouts != null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(timeouts.Value.TotalMilliseconds));
                return await ReceiveAsync();
            }
            return null;
        }

        protected internal virtual IMessage<T> BeforeConsume(IMessage<T> message)
        {
            if (_interceptors != null)
            {
                return _interceptors.BeforeConsume(_consumerActor, message);
            }
            else
            {
                return message;
            }
        }
        /// <summary>
        /// batch receive messages
        /// </summary>crea
        /// <code>
        /// if(HasMessage("{consumerName}", out var count))
        /// {
        ///     var messages = BatchReceive("{consumerName}", count);
        /// }
        /// </code>
        /// <param name="consumerName"></param>
        /// <param name="batchSize"></param>
        /// <param name="receiveTimeout"></param> 
        /// <returns></returns>
        public IMessages<T> BatchReceive()
        {            
            return BatchReceiveAsync().GetAwaiter().GetResult();
        }
        public async ValueTask<IMessages<T>> BatchReceiveAsync()
        {
            VerifyBatchReceive();
            await VerifyConsumerState();
            var messages = new Messages<T>(_conf.BatchReceivePolicy.MaxNumMessages, _conf.BatchReceivePolicy.MaxNumBytes);

            if (await HasEnoughMessagesForBatchReceive())
            {
                while (_queue.IncomingMessages.TryReceive(out var message) && messages.CanAdd(message))
                {
                    try
                    {
                        _consumerActor.Tell(new MessageProcessed<T>(message));
                        IMessage<T> interceptMsg = BeforeConsume(message);
                        messages.Add(interceptMsg);
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            return messages;
        }

        private async Task VerifyConsumerState()
        {
            var result = await _stateActor.Ask<HandlerStateResponse>(GetHandlerState.Instance).ConfigureAwait(false);
            var state = result.State;

            var retry = 10;
            while(state == HandlerState.State.Uninitialized && retry > 0)
            {
                result = await _stateActor.Ask<HandlerStateResponse>(GetHandlerState.Instance).ConfigureAwait(false);
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
        private void VerifyBatchReceive()
        {
            if (_conf.MessageListener != null)
            {
                throw new PulsarClientException.InvalidConfigurationException("Cannot use receive() when a listener has been set");
            }
            if (_conf.ReceiverQueueSize == 0)
            {
                throw new PulsarClientException.InvalidConfigurationException("Can't use batch receive, if the queue size is 0");
            }
        }
        private async ValueTask<bool> HasEnoughMessagesForBatchReceive()
        {
            var mesageSize = await _consumerActor.Ask<long>(GetIncomingMessageSize.Instance).ConfigureAwait(false);
            if (_conf.BatchReceivePolicy.MaxNumMessages <= 0 && _conf.BatchReceivePolicy.MaxNumBytes <= 0)
            {
                return false;
            }
            return (_conf.BatchReceivePolicy.MaxNumMessages > 0 && _queue.IncomingMessages.Count >= _conf.BatchReceivePolicy.MaxNumMessages) || (_conf.BatchReceivePolicy.MaxNumBytes > 0 && mesageSize >= _conf.BatchReceivePolicy.MaxNumBytes);
        }
        public void ReconsumeLater(IMessage<T> message, long delayTimeInMs) 
            => ReconsumeLaterAsync(message, delayTimeInMs).GetAwaiter().GetResult();
        public async ValueTask ReconsumeLaterAsync(IMessage<T> message, long delayTimeInMs)
        {
            _consumerActor.Tell(new ReconsumeLaterMessage<T>(message, delayTimeInMs));
            if (_queue.ReconsumeLaterException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }

        public void ReconsumeLater(IMessages<T> messages, long delayTimeInMs)
            => ReconsumeLaterAsync(messages, delayTimeInMs).GetAwaiter().GetResult();
        public async ValueTask ReconsumeLaterAsync(IMessages<T> messages, long delayTimeInMs)
        {
            _consumerActor.Tell(new ReconsumeLaterMessages<T>(messages, delayTimeInMs));
            if (_queue.ReconsumeLaterException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }

        public void ReconsumeLaterCumulative(IMessage<T> message, long delayTimeInMs)
            => ReconsumeLaterCumulativeAsync(message, delayTimeInMs).GetAwaiter().GetResult();
        public async ValueTask ReconsumeLaterCumulativeAsync(IMessage<T> message, long delayTimeInMs)
        {
            _consumerActor.Tell(new ReconsumeLaterCumulative<T>(message, delayTimeInMs));
            if (_queue.ReconsumeLaterException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }

        public void RedeliverUnacknowledgedMessages()
            => RedeliverUnacknowledgedMessagesAsync().ConfigureAwait(false);
        public async ValueTask RedeliverUnacknowledgedMessagesAsync()
        {
            _consumerActor.Tell(Messages.Consumer.RedeliverUnacknowledgedMessages.Instance);
            if (_queue.RedeliverUnacknowledgedException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }

        public void Resume()
        {
            _consumerActor.Tell(Messages.Consumer.Resume.Instance);
        }
        public void Seek(IMessageId messageId)
        {
            SeekAsync(messageId).ConfigureAwait(false);
        }
        public async Task SeekAsync(IMessageId messageId)
        {
            var result = await _stateActor.Ask<HandlerStateResponse>(GetHandlerState.Instance).ConfigureAwait(false);
            var state = result.State;
            if (state == HandlerState.State.Closing || state == HandlerState.State.Closed)
            {
                throw new PulsarClientException.AlreadyClosedException($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {Topic} to the message {messageId}");

            }

            if (!await ConnectedAsync())
            {
                throw new PulsarClientException($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {Topic} to the message {messageId}");

            }

            _consumerActor.Tell(new SeekMessageId(messageId));
            if (_queue.SeekException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }
        public void Seek(long timestamp)
        {
            SeekAsync(timestamp).ConfigureAwait(false);
        }
        public async Task SeekAsync(long timestamp)
        {
            var result = await _stateActor.Ask<HandlerStateResponse>(GetHandlerState.Instance).ConfigureAwait(false);
            var state = result.State;
            if (state == HandlerState.State.Closing || state == HandlerState.State.Closed)
            {
                throw new Exception($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {Topic} to the timestamp {timestamp:D}");

            }

            if (!await ConnectedAsync())
            {
                throw new Exception($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {Topic} to the timestamp {timestamp:D}");
            }

            _consumerActor.Tell(new SeekTimestamp(timestamp));
            if (_queue.SeekException.TryTake(out var msg, 1000))
                if (msg?.Exception != null)
                    await Task.FromException(msg.Exception).ConfigureAwait(false);
        }

        /// <summary>
        /// Consume messages from queue. ConsumptionType has to be set to Queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="autoAck"></param>
        /// <param name="takeCount"></param>
        /// <param name="customProcess"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<T> ReceiveFunc(bool autoAck = true, int takeCount = -1, int receiveTimeout = 3000, Func<IMessage<T>, T> customHander = null, [EnumeratorCancellation]CancellationToken token = default)
        {
            //no end
            if (takeCount == -1)
            {
                for (var i = 0; i > takeCount; i++)
                {
                    IMessage<T> message;
                    try
                    {
                        message = message = await _queue.IncomingMessages.ReceiveAsync(timeout: TimeSpan.FromMilliseconds(receiveTimeout), cancellationToken: token);
                    }
                    catch
                    {
                        message = null;
                    }

                    if (message != null)
                    {
                        yield return ProcessMessage(message, autoAck, customHander); 
                    }
                }
            }
            else if (takeCount > 0)//end at takeCount
            {
                for (var i = 0; i < takeCount; i++)
                {
                    IMessage<T> message;
                    try
                    {
                        message = message = await _queue.IncomingMessages.ReceiveAsync(timeout: TimeSpan.FromMilliseconds(receiveTimeout), cancellationToken: token);
                    }
                    catch
                    {
                        message = null;
                    }
                    if (message != null)
                    {
                        yield return ProcessMessage(message, autoAck, customHander);
                    }
                    else
                    {
                        //we need to go back since no message was received within the timeout
                        i--;
                    }
                }
            }
            else
            {
                //drain the current messages
                while (true)
                {
                    IMessage<T> message;
                    try
                    {
                        message = message = await _queue.IncomingMessages.ReceiveAsync(timeout: TimeSpan.FromMilliseconds(receiveTimeout), cancellationToken: token);
                    }
                    catch
                    {
                        message = null;
                    }
                    if (message != null)
                    {
                        yield return ProcessMessage(message, autoAck, customHander);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private T ProcessMessage(IMessage<T> m, bool autoAck, Func<IMessage<T>, T> customHander = null)
        {
            var received = customHander == null ? m.Value : customHander(m);
            if (autoAck)
            {
                Acknowledge(m);
            }

            return received;
        }

        public void Unsubscribe()
        {
            _consumerActor.Tell(Messages.Consumer.Unsubscribe.Instance);
        }
    }
}
