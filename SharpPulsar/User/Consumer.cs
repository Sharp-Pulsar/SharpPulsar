using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Stats.Consumer.Api;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
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
        private readonly TimeSpan _operationTimeout;

        public Consumer(IActorRef stateActor, IActorRef consumer, ISchema<T> schema, ConsumerConfigurationData<T> conf, TimeSpan operationTimeout)
        {
            _stateActor = stateActor;
            _consumerActor = consumer;
            _schema = schema;
            _conf = conf;
            _operationTimeout = operationTimeout;
        }
        public string Topic => TopicAsync().GetAwaiter().GetResult();
        public async ValueTask<string> TopicAsync() 
            => await _consumerActor.Ask<string>(GetTopic.Instance, TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        public string Subscription => SubscriptionAsync().GetAwaiter().GetResult();
        public async ValueTask<string> SubscriptionAsync() 
            => await _consumerActor.Ask<string>(GetSubscription.Instance, TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        public IConsumerStats Stats => StatsAsync().GetAwaiter().GetResult();
        public async ValueTask<IConsumerStats> StatsAsync() 
            => await _consumerActor.Ask<IConsumerStats>(GetStats.Instance, TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        public IMessageId LastMessageId => LastMessageIdAsync().GetAwaiter().GetResult();
        public async ValueTask<IMessageId> LastMessageIdAsync() 
            => await _consumerActor.Ask<IMessageId>(GetLastMessageId.Instance, TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        public bool Connected => ConnectedAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> ConnectedAsync() 
            => await _consumerActor.Ask<bool>(IsConnected.Instance, TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        public string ConsumerName => ConsumerNameAsync().GetAwaiter().GetResult();
        public async ValueTask<string> ConsumerNameAsync() 
            => await _consumerActor.Ask<string>(GetConsumerName.Instance, TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        public long LastDisconnectedTimestamp => LastDisconnectedTimestampAsync().GetAwaiter().GetResult();
        public async ValueTask<long> LastDisconnectedTimestampAsync() 
            => await _consumerActor.Ask<long>(GetLastDisconnectedTimestamp.Instance, TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        public void Acknowledge(IMessage<T> message) => AcknowledgeAsync(message).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IMessage<T> message)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeMessage<T>(message), TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public int NumMessagesInQueue()
        {
            return NumMessagesInQueueAsync().GetAwaiter().GetResult();
        }
        public async ValueTask<int> NumMessagesInQueueAsync()
        {
            var askFormesageCount = await _consumerActor.Ask<AskResponse>(GetIncomingMessageCount.Instance, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            var mesageCount = askFormesageCount.ConvertTo<long>();
            return (int)mesageCount;
        }
        public void Acknowledge(IMessageId messageId) => AcknowledgeAsync(messageId).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IMessageId messageId)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeMessageId(messageId), TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public IActorRef ConsumerActor => _consumerActor;
        public void Acknowledge(IMessages<T> messages) => AcknowledgeAsync(messages).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IMessages<T> messages)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeMessages<T>(messages), TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public void Acknowledge(IList<IMessageId> messageIds) => AcknowledgeAsync(messageIds).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IList<IMessageId> messageIds)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeMessageIds(messageIds), TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public void Acknowledge(IMessageId messageId, Transaction txn)
            => AcknowledgeAsync(messageId, txn).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeAsync(IMessageId messageId, Transaction txn)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeWithTxn(messageId, txn.Txn), TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public void AcknowledgeCumulative(IMessage<T> message) 
            => AcknowledgeCumulativeAsync(message).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeCumulativeAsync(IMessage<T> message)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeCumulativeMessage<T>(message), TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }
        public void AcknowledgeCumulative(IMessageId messageid)
            => AcknowledgeCumulativeAsync(messageid).GetAwaiter().GetResult();
        public async ValueTask AcknowledgeCumulativeAsync(IMessageId messageId)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeCumulativeMessageId(messageId), TimeSpan.FromSeconds(5))
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
            var ask = await _consumerActor.Ask<AskResponse>(new AcknowledgeCumulativeTxn(messageId, txn.Txn), TimeSpan.FromSeconds(5))
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
            await _consumerActor.GracefulStop(_operationTimeout).ConfigureAwait(false);
        }
        public bool HasReachedEndOfTopic() 
            => HasReachedEndOfTopicAsync().GetAwaiter().GetResult();
        public async ValueTask<bool> HasReachedEndOfTopicAsync()
        {
            var ask = await _consumerActor.Ask<AskResponse>(Messages.Consumer.HasReachedEndOfTopic.Instance, TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);

            return ask.ConvertTo<bool>();
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
            var ask = await _consumerActor.Ask<AskResponse>(new NegativeAcknowledgeMessage<T>(message), TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }

        public void NegativeAcknowledge(IMessages<T> messages) 
            => NegativeAcknowledgeAsync(messages).GetAwaiter().GetResult();

        public async ValueTask NegativeAcknowledgeAsync(IMessages<T> messages)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new NegativeAcknowledgeMessages<T>(messages), TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }

        public void Pause()
        {
            _consumerActor.Tell(Messages.Consumer.Pause.Instance);
        }
        public IMessage<T> Receive()
        {
            return ReceiveAsync().GetAwaiter().GetResult();
        }

        public async ValueTask<IMessage<T>> ReceiveAsync()
        {
            var response = await _consumerActor.Ask<AskResponse>(Messages.Consumer.Receive.Instance, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            if (response.Failed)
                throw response.Exception;

            if (response.Data != null)
            {
                var message = response.ConvertTo<IMessage<T>>();
                return message;
            }                

            return null;
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
            var response = await _consumerActor.Ask<AskResponse>(Messages.Consumer.BatchReceive.Instance, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            if (response.Failed)
                throw response.Exception;

            if (response.Data != null)
                return response.ConvertTo<IMessages<T>>();

            return null;
        }

        
        
        public void ReconsumeLater(IMessage<T> message, TimeSpan delayTimeInMs) 
            => ReconsumeLaterAsync(message, delayTimeInMs).GetAwaiter().GetResult();
        public async ValueTask ReconsumeLaterAsync(IMessage<T> message, TimeSpan delayTimeInMs)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new ReconsumeLaterMessage<T>(message, delayTimeInMs), TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }

        public void ReconsumeLater(IMessages<T> messages, TimeSpan delayTimeInMs)
            => ReconsumeLaterAsync(messages, delayTimeInMs).GetAwaiter().GetResult();
        public async ValueTask ReconsumeLaterAsync(IMessages<T> messages, TimeSpan delayTimeInMs)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new ReconsumeLaterMessages<T>(messages, delayTimeInMs), TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }

        public void ReconsumeLaterCumulative(IMessage<T> message, TimeSpan delayTimeInMs)
            => ReconsumeLaterCumulativeAsync(message, delayTimeInMs).GetAwaiter().GetResult();
        public async ValueTask ReconsumeLaterCumulativeAsync(IMessage<T> message, TimeSpan delayTimeInMs)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new ReconsumeLaterCumulative<T>(message, delayTimeInMs), TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);
            if (ask.Failed)
                throw ask.Exception;
        }

        public void RedeliverUnacknowledgedMessages()
            => RedeliverUnacknowledgedMessagesAsync().ConfigureAwait(false);
        public async ValueTask RedeliverUnacknowledgedMessagesAsync()
        {
            var ask = await _consumerActor.Ask<AskResponse>(Messages.Consumer.RedeliverUnacknowledgedMessages.Instance, TimeSpan.FromSeconds(5))
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
            var askForState = await _stateActor.Ask<AskResponse>(GetHandlerState.Instance, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            var state = askForState.ConvertTo<HandlerState.State>();
            if (state == HandlerState.State.Closing || state == HandlerState.State.Closed)
            {
                throw new PulsarClientException.AlreadyClosedException($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {Topic} to the message {messageId}");

            }

            if (!await ConnectedAsync())
            {
                throw new PulsarClientException($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {Topic} to the message {messageId}");

            }

            var ask = await _consumerActor.Ask<AskResponse>(new SeekMessageId(messageId), TimeSpan.FromSeconds(5))
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
            var askForState = await _stateActor.Ask<AskResponse>(GetHandlerState.Instance, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            var state = askForState.ConvertTo<HandlerState.State>();
            if (state == HandlerState.State.Closing || state == HandlerState.State.Closed)
            {
                throw new Exception($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {Topic} to the timestamp {timestamp:D}");

            }

            if (!await ConnectedAsync())
            {
                throw new Exception($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {Topic} to the timestamp {timestamp:D}");
            }

            var ask = await _consumerActor.Ask<AskResponse>(new SeekTimestamp(timestamp), TimeSpan.FromSeconds(5))
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
                    AskResponse response = null;
                    try
                    {
                        response = await _consumerActor.Ask<AskResponse>(Messages.Consumer.Receive.Instance, TimeSpan.FromMilliseconds(receiveTimeout)).ConfigureAwait(false);

                    }
                    catch (AskTimeoutException)
                    { }
                    catch
                    {
                        throw;
                    }
                    if(response != null && response.Data is IMessage<T> message)
                        yield return ProcessMessage(message, autoAck, customHander);
                }
            }
            else if (takeCount > 0)//end at takeCount
            {
                for (var i = 0; i < takeCount; i++)
                {
                    AskResponse response = null;
                    try
                    {
                        response = await _consumerActor.Ask<AskResponse>(Messages.Consumer.Receive.Instance, TimeSpan.FromMilliseconds(receiveTimeout)).ConfigureAwait(false);

                    }
                    catch (AskTimeoutException)
                    { }
                    catch
                    {
                        throw;
                    }

                    if (response != null && response.Data is IMessage<T> message)
                        yield return ProcessMessage(message, autoAck, customHander);                    
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
                while(true)
                {
                    AskResponse response = null;
                    try
                    {
                        response = await _consumerActor.Ask<AskResponse>(Messages.Consumer.Receive.Instance, TimeSpan.FromMilliseconds(receiveTimeout)).ConfigureAwait(false);
                    }
                    catch (AskTimeoutException)
                    { 
                    }
                    catch
                    {
                        throw;
                    }

                    if (response != null && response.Data is IMessage<T> message)
                        yield return ProcessMessage(message, autoAck, customHander);
                    else
                        break;
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
        public void NegativeAcknowledge(IMessageId messageId)
            => NegativeAcknowledgeAsync(messageId).GetAwaiter().GetResult();

        public async ValueTask NegativeAcknowledgeAsync(IMessageId messageId)
        {
            var ask = await _consumerActor.Ask<AskResponse>(new NegativeAcknowledgeMessageId(messageId), TimeSpan.FromSeconds(5))
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
