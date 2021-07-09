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
        private readonly ConsumerInterceptors<T> _interceptors;
        private readonly CancellationTokenSource _tokenSource;
        private IMessageListener<T> _listener;

        public Consumer(IActorRef stateActor, IActorRef consumer, ISchema<T> schema, ConsumerConfigurationData<T> conf, ConsumerInterceptors<T> interceptors)
        {
            _stateActor = stateActor;
            _listener = conf.MessageListener;
            _tokenSource = new CancellationTokenSource();
            _interceptors = interceptors;
            _consumerActor = consumer;
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
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeMessage<T>(message))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public int NumMessagesInQueue()
            => _queue.IncomingMessages.Count;
        public void Acknowledge(IMessageId messageId) => AcknowledgeAsync(messageId).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IMessageId messageId)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeMessageId(messageId))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public IActorRef ConsumerActor => _consumerActor;
        public void Acknowledge(IMessages<T> messages) => AcknowledgeAsync(messages).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IMessages<T> messages)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeMessages<T>(messages))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public void Acknowledge(IList<IMessageId> messageIds) => AcknowledgeAsync(messageIds).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IList<IMessageId> messageIds)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeMessageIds(messageIds))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public void Acknowledge(IMessageId messageId, Transaction txn)
            => AcknowledgeAsync(messageId, txn).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IMessageId messageId, Transaction txn)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeWithTxn(messageId, txn.Txn))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public void AcknowledgeCumulative(IMessage<T> message) 
            => AcknowledgeCumulativeAsync(message).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeCumulativeAsync(IMessage<T> message)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeCumulativeMessage<T>(message))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public void AcknowledgeCumulative(IMessageId messageid)
            => AcknowledgeCumulativeAsync(messageid).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeCumulativeAsync(IMessageId messageId)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeCumulativeMessageId(messageId))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public void AcknowledgeCumulative(IMessageId messageid, Transaction txn)
            => AcknowledgeCumulativeAsync(messageid, txn).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeCumulativeAsync(IMessageId messageId, Transaction txn)
        {
            if (!IsCumulativeAcknowledgementAllowed(_conf.SubscriptionType))
            {
                throw new PulsarClientException.InvalidConfigurationException("Cannot use cumulative acks on a non-exclusive/non-failover subscription");
            }
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeCumulativeTxn(messageId, txn.Txn))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
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
            var ask = await _consumerActor.Ask<AskResponse>(Messages.Consumer.HasReachedEndOfTopic.Instance)
                .ConfigureAwait(false);

            return ask.GetData<bool>();
        }
        /// <summary>
        /// Negatively Acknowledge a message so that it can be redelivered
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="PulsarClientException"></exception>
        public void NegativeAcknowledge(IMessage<T> message) 
            => NegativeAcknowledgeAsync(message).GetAwaiter().GetResult();
        public async ValueTask NegativeAcknowledgeAsync(IMessage<T> message)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new NegativeAcknowledgeMessage<T>(message))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }

        public void NegativeAcknowledge(IMessages<T> messages) 
            => NegativeAcknowledgeAsync(messages).GetAwaiter().GetResult();

        public async ValueTask NegativeAcknowledgeAsync(IMessages<T> messages)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new NegativeAcknowledgeMessages<T>(messages))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
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
            var message = GetMessage();
            if (message == null && timeouts != null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(timeouts.Value.TotalMilliseconds));
                return GetMessage();
            }
            return message;
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
                var message = GetMessage();
                while (message != null && messages.CanAdd(message))
                {
                    messages.Add(message);
                    message = GetMessage();
                }
            }
            return messages;
        }

        private async ValueTask VerifyConsumerState()
        {
            var askForState = await _stateActor.Ask<AskResponse>(GetHandlerState.Instance).ConfigureAwait(false);
            var state = askForState.GetData<HandlerState.State>();

            var retry = 10;
            while(state == HandlerState.State.Uninitialized && retry > 0)
            {
                askForState = await _stateActor.Ask<AskResponse>(GetHandlerState.Instance).ConfigureAwait(false);
                state = askForState.GetData<HandlerState.State>();
                retry--;
                if (state == HandlerState.State.Uninitialized || state == HandlerState.State.Failed)
                    await Task.Delay(TimeSpan.FromSeconds(5))
                .ConfigureAwait(false); 
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
            var askFormesageSize = await _consumerActor.Ask<AskResponse>(GetIncomingMessageSize.Instance).ConfigureAwait(false);
            var mesageSize = askFormesageSize.GetData<long>();
            if (_conf.BatchReceivePolicy.MaxNumMessages <= 0 && _conf.BatchReceivePolicy.MaxNumBytes <= 0)
            {
                return false;
            }
            return (_conf.BatchReceivePolicy.MaxNumMessages > 0 && _queue.IncomingMessages.Count >= _conf.BatchReceivePolicy.MaxNumMessages) || (_conf.BatchReceivePolicy.MaxNumBytes > 0 && mesageSize >= _conf.BatchReceivePolicy.MaxNumBytes);
        }
        public void ReconsumeLater(IMessage<T> message, TimeSpan delayTimeInMs) 
            => ReconsumeLaterAsync(message, delayTimeInMs).GetAwaiter().GetResult();
        public async ValueTask ReconsumeLaterAsync(IMessage<T> message, TimeSpan delayTimeInMs)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new ReconsumeLaterMessage<T>(message, delayTimeInMs))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }

        public void ReconsumeLater(IMessages<T> messages, TimeSpan delayTimeInMs)
            => ReconsumeLaterAsync(messages, delayTimeInMs).GetAwaiter().GetResult();
        public async ValueTask ReconsumeLaterAsync(IMessages<T> messages, TimeSpan delayTimeInMs)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new ReconsumeLaterMessages<T>(messages, delayTimeInMs))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }

        public void ReconsumeLaterCumulative(IMessage<T> message, TimeSpan delayTimeInMs)
            => ReconsumeLaterCumulativeAsync(message, delayTimeInMs).GetAwaiter().GetResult();
        public async ValueTask ReconsumeLaterCumulativeAsync(IMessage<T> message, TimeSpan delayTimeInMs)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new ReconsumeLaterCumulative<T>(message, delayTimeInMs))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }

        public void RedeliverUnacknowledgedMessages()
            => RedeliverUnacknowledgedMessagesAsync().ConfigureAwait(false);
        public async ValueTask RedeliverUnacknowledgedMessagesAsync()
        {
            var ask = await _consumerActor.Ask<AskResponse>(Messages.Consumer.RedeliverUnacknowledgedMessages.Instance)
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }

        public void Resume()
        {
            _consumerActor.Tell(Messages.Consumer.Resume.Instance);
        }
        public void Seek(IMessageId messageId)
        {
            SeekAsync(messageId).ConfigureAwait(false);
        }
        public async ValueTask SeekAsync(IMessageId messageId)
        {
            var askForState = await _stateActor.Ask<AskResponse>(GetHandlerState.Instance).ConfigureAwait(false);
            var state = askForState.GetData<HandlerState.State>();
            if (state == HandlerState.State.Closing || state == HandlerState.State.Closed)
            {
                throw new PulsarClientException.AlreadyClosedException($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {Topic} to the message {messageId}");

            }

            if (!await ConnectedAsync())
            {
                throw new PulsarClientException($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {Topic} to the message {messageId}");

            }

            var ask = await _consumerActor.Ask<AskResponse>(new SeekMessageId(messageId))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public void Seek(long timestamp)
        {
            SeekAsync(timestamp).ConfigureAwait(false);
        }
        public async ValueTask SeekAsync(long timestamp)
        {
            var askForState = await _stateActor.Ask<AskResponse>(GetHandlerState.Instance).ConfigureAwait(false);
            var state = askForState.GetData<HandlerState.State>();
            if (state == HandlerState.State.Closing || state == HandlerState.State.Closed)
            {
                throw new Exception($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {Topic} to the timestamp {timestamp:D}");

            }

            if (!await ConnectedAsync())
            {
                throw new Exception($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {Topic} to the timestamp {timestamp:D}");
            }

            var ask = await _consumerActor.Ask<AskResponse>(new SeekTimestamp(timestamp))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
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
                while(true)
                {
                    while(_queue.IncomingMessages.TryReceive(out var message))
                    { 
                       yield return ProcessMessage(message, autoAck, customHander);
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(receiveTimeout));
                }
            }
            else if (takeCount > 0)//end at takeCount
            {
                for (var i = 0; i < takeCount; i++)
                {
                    if (_queue.IncomingMessages.TryReceive(out var message))
                    {
                        yield return ProcessMessage(message, autoAck, customHander);
                    }
                    else
                    {
                        //we need to go back since no message was received within the timeout
                        i--;
                        await Task.Delay(TimeSpan.FromMilliseconds(receiveTimeout));
                    }
                }
            }
            else
            {
                //drain the current messages
                while (_queue.IncomingMessages.TryReceive(out var message))
                {
                    yield return ProcessMessage(message, autoAck, customHander);
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
        private IMessage<T> GetMessage()
        {
            if (_queue.IncomingMessages.TryReceive(out var message))
            {
                _consumerActor.Tell(new MessageProcessed<T>(message));
                var inteceptedMessage = BeforeConsume(message);
                return inteceptedMessage;
            }
            return null;
        }
        public void Unsubscribe()
        {
            _consumerActor.Tell(Messages.Consumer.Unsubscribe.Instance);
        }
        public void NegativeAcknowledge(IMessageId messageId)
            => NegativeAcknowledgeAsync(messageId).GetAwaiter().GetResult();

        public async ValueTask NegativeAcknowledgeAsync(IMessageId messageId)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new NegativeAcknowledgeMessageId(messageId))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }

        public void Seek(Func<string, object> function) 
            => SeekAsync(function).GetAwaiter().GetHashCode();

        public async ValueTask SeekAsync(Func<string, object> function)
        {
            if (function == null)
            {
                throw new PulsarClientException("Function must be set");
            }
            var topic = await TopicAsync().ConfigureAwait(false);
            var seekPosition = function(topic);
            if (seekPosition == null)
            {
                return;
            }
            if (seekPosition is MessageId msgId)
            {
                await SeekAsync(msgId).ConfigureAwait(false);
            }
            else if (seekPosition is long timeStamp)
            {
                await SeekAsync(timeStamp).ConfigureAwait(false);
            }
            throw new PulsarClientException("Only support seek by messageId or timestamp");

            throw new NotImplementedException();
        }
    }
}
