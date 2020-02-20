using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using Microsoft.Extensions.Logging;
using Pulsar.Common.Auth;
using SharpPulsar.Api;
using SharpPulsar.Common.Compression;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Schema;
using SharpPulsar.Extension;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Transaction;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Shared;
using SharpPulsar.Utility.Atomic;
using SharpPulsar.Utils;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Impl
{
    public class ConsumerImpl<T> : ConsumerBase<T>, IConnection
	{
		private const int MaxRedeliverUnacknowledged = 1000;

		internal readonly long ConsumerId;

		private static readonly ConcurrentDictionary<ConsumerImpl<T>, int> _availablePermits = new ConcurrentDictionary<ConsumerImpl<T>, int>();

        internal volatile IMessageId LastDequeuedMessage;
		private volatile IMessageId _lastMessageIdInBroker = MessageIdFields.Earliest;

		private readonly long _subscribeTimeout;
		internal int PartitionIndex;
		private readonly bool _hasParentConsumer;

		private readonly int _receiverQueueRefillThreshold;

		private readonly ReaderWriterLock _lock = new ReaderWriterLock();

		public UnAckedMessageTracker<T> UnAckedMessageTracker;
		private readonly IAcknowledgmentsGroupingTracker _acknowledgmentsGroupingTracker;
		private readonly NegativeAcksTracker<T> _negativeAcksTracker;

        internal readonly ConsumerStatsRecorder ConsumerStats;
		private readonly int _priorityLevel;
		private readonly SubscriptionMode _subscriptionMode;
		private volatile BatchMessageIdImpl _startMessageId;

		private readonly BatchMessageIdImpl _initialStartMessageId;
		private readonly long _startMessageRollbackDurationInSec;

		private volatile bool _hasReachedEndOfTopic;

		private readonly MessageCrypto _msgCrypto;

		private readonly IDictionary<string, string> _metadata;

		private readonly bool _readCompacted;
		private readonly bool _resetIncludeHead;

		private readonly SubscriptionInitialPosition _subscriptionInitialPosition;
		public  ConnectionHandler Handler;

		private readonly TopicName _topicName;
		public string TopicNameWithoutPartition;

		private readonly IDictionary<MessageIdImpl, IList<MessageImpl<T>>> _possibleSendToDeadLetterTopicMessages;

		private readonly DeadLetterPolicy _deadLetterPolicy;

		private IProducer<T> _deadLetterProducer;

        internal volatile bool Paused;

		private readonly bool _createTopicIfDoesNotExist;

		public enum SubscriptionMode
		{
			// Make the subscription to be backed by a durable cursor that will retain messages and persist the current
			// position
			Durable,

			// Lightweight subscription mode that doesn't have a durable cursor associated
			NonDurable
		}

		public static ConsumerImpl<T> NewConsumerImpl(PulsarClientImpl client, string topic, ConsumerConfigurationData<T> conf, ScheduledThreadPoolExecutor listenerExecutor, int partitionIndex, bool hasParentConsumer, TaskCompletionSource<IConsumer<T>> subscribeTask, SubscriptionMode subscriptionMode, IMessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist)
        {
            if (conf.ReceiverQueueSize == 0)
			{
				return new ZeroQueueConsumerImpl<T>(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, subscribeTask, subscriptionMode, startMessageId, schema, interceptors, createTopicIfDoesNotExist);
			}

            return new ConsumerImpl<T>(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, subscribeTask, subscriptionMode, startMessageId, 0, schema, interceptors, createTopicIfDoesNotExist);
        }

		public ConsumerImpl(PulsarClientImpl client, string topic, ConsumerConfigurationData<T> conf, ScheduledThreadPoolExecutor listenerExecutor, int partitionIndex, bool hasParentConsumer, TaskCompletionSource<IConsumer<T>> subscribeTask, SubscriptionMode subscriptionMode, IMessageId startMessageId, long startMessageRollbackDurationInSec, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist) : base(client, topic, conf, conf.ReceiverQueueSize, listenerExecutor, subscribeTask, schema, interceptors)
		{
			SetState(State.Uninitialized);
			ConsumerId = client.NewConsumerId();
			_subscriptionMode = subscriptionMode;
			_startMessageId = startMessageId != null ? new BatchMessageIdImpl((MessageIdImpl) startMessageId) : null;
			LastDequeuedMessage = startMessageId ?? MessageIdFields.Earliest;
			_initialStartMessageId = _startMessageId;
			_startMessageRollbackDurationInSec = startMessageRollbackDurationInSec;
			_availablePermits[this] =  0;
			_subscribeTimeout = DateTimeHelper.CurrentUnixTimeMillis() + client.Configuration.OperationTimeoutMs;
			PartitionIndex = partitionIndex;
			_hasParentConsumer = hasParentConsumer;
			_receiverQueueRefillThreshold = conf.ReceiverQueueSize / 2;
			_priorityLevel = conf.PriorityLevel;
			_readCompacted = conf.ReadCompacted;
			_subscriptionInitialPosition = conf.SubscriptionInitialPosition;
			_negativeAcksTracker = new NegativeAcksTracker<T>(this, conf);
			_resetIncludeHead = conf.ResetIncludeHead;
			_createTopicIfDoesNotExist = createTopicIfDoesNotExist;

			if (client.Configuration.StatsIntervalSeconds > 0)
			{
				ConsumerStats = new ConsumerStatsRecorderImpl<T>(client, conf, this);
			}
			else
			{
				ConsumerStats = ConsumerStatsDisabled.Instance;
			}

			if (conf.AckTimeoutMillis != 0)
			{
				if (conf.TickDurationMillis > 0)
				{
					UnAckedMessageTracker = new UnAckedMessageTracker<T>(client, this, conf.AckTimeoutMillis, Math.Min(conf.TickDurationMillis, conf.AckTimeoutMillis));
				}
				else
				{
					UnAckedMessageTracker = new UnAckedMessageTracker<T>(client, this, conf.AckTimeoutMillis);
				}
			}
			else
			{
				UnAckedMessageTracker = UnAckedMessageTracker<T>.UnackedMessageTrackerDisabled;
			}

			// Create msgCrypto if not created already
			_msgCrypto = conf.CryptoKeyReader == null ? new MessageCrypto($"[{topic}] [{Subscription}]", false) : null;

			_metadata = !Enumerable.Any(conf.Properties) ? new Dictionary<string, string>() : new Dictionary<string,string>(conf.Properties);

			Handler = new ConnectionHandler(this, new BackoffBuilder()
									.SetInitialTime(client.Configuration.InitialBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).SetMax(client.Configuration.MaxBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).SetMandatoryStop(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).Create(), this);

			_topicName = TopicName.Get(topic);
			if (_topicName.Persistent)
			{
				_acknowledgmentsGroupingTracker = new PersistentAcknowledgmentsGroupingTracker<T>(this, conf, client.EventLoopGroup);
			}
			else
			{
				_acknowledgmentsGroupingTracker = NonPersistentAcknowledgmentGroupingTracker.Of();
			}

			if (conf.DeadLetterPolicy != null)
			{
				_possibleSendToDeadLetterTopicMessages = new ConcurrentDictionary<MessageIdImpl, IList<MessageImpl<T>>>();
				if (string.IsNullOrWhiteSpace(conf.DeadLetterPolicy.DeadLetterTopic))
				{
					_deadLetterPolicy = new DeadLetterPolicy
                    {
						MaxRedeliverCount = conf.DeadLetterPolicy.MaxRedeliverCount,
						DeadLetterTopic = conf.DeadLetterPolicy.DeadLetterTopic
					};
				}
				else
				{
                    _deadLetterPolicy = new DeadLetterPolicy
                    {
                        MaxRedeliverCount = conf.DeadLetterPolicy.MaxRedeliverCount,
                        DeadLetterTopic = $"{topic}-{Subscription}-DLQ"
					};
				}
			}
			else
			{
				_deadLetterPolicy = null;
				_possibleSendToDeadLetterTopicMessages = null;
			}

			TopicNameWithoutPartition = _topicName.PartitionedTopicName;
			
			GrabCnx();
		}



		public override ValueTask UnsubscribeAsync()
		{
			if (GetState() == State.Closing || GetState() == State.Closed)
			{
				return new ValueTask(Task.FromException(new PulsarClientException.AlreadyClosedException("Consumer was already closed")));
			}

			var unsubscribeTask = new TaskCompletionSource<Task>();
			if (Connected)
			{
                SetState(State.Closing);
				var requestId = Client.NewRequestId();
				var unsubscribe = Commands.NewUnsubscribe(ConsumerId, requestId);
				var cnx = Cnx();
                cnx.SendRequestWithId(unsubscribe, requestId).AsTask().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Log.LogError("[{}][{}] Failed to unsubscribe: {}", Topic, Subscription, task.Exception.Message);
                        SetState(State.Ready);
                        unsubscribeTask.SetException(PulsarClientException.Wrap(task.Exception,
                            $"Failed to unsubscribe the subscription {_topicName.ToString()} of topic {Subscription}"));
                        return;
                    }

                    cnx.RemoveConsumer(ConsumerId);
                    UnAckedMessageTracker.Close();
                    _possibleSendToDeadLetterTopicMessages?.Clear();

                    Client.CleanupConsumer(this);
                    Log.LogInformation("[{}][{}] Successfully unsubscribed from topic", Topic, Subscription);
                    SetState(State.Closed);
                    unsubscribeTask.SetResult(null);
                });
            }
			else
			{
				unsubscribeTask.SetException(new PulsarClientException(
                    "The client is not connected to the broker when unsubscribing the " +
                    $"subscription {Subscription} of the topic {_topicName.ToString()}"));
			}
			return new ValueTask(unsubscribeTask.Task);
		}

		public override IMessage<T> InternalReceive()
		{
            try
			{
				var message = IncomingMessages.Take();
				MessageProcessed(message);
				return BeforeConsume(message);
			}
			catch (ThreadInterruptedException e)
			{
				ConsumerStats.IncrementNumReceiveFailed();
				throw PulsarClientException.Unwrap(e);
			}
		}

		public override ValueTask<IMessage<T>> InternalReceiveAsync()
		{

			var result = new TaskCompletionSource<IMessage<T>>();
			IMessage<T> message = null;
			try
			{
				_lock.AcquireWriterLock(3000);
				message = IncomingMessages.Poll(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
				if (message == null)
				{
					PendingReceives.Enqueue(result);
				}
			}
			catch (ThreadInterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				result.SetException(e);
			}
			finally
			{
				_lock.ReleaseWriterLock();
			}

			if (message != null)
			{
				MessageProcessed(message);
				result.SetResult(BeforeConsume(message));
			}

			return new ValueTask<IMessage<T>>(result.Task.Result);
		}

		public override IMessage<T> InternalReceive(int timeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
            try
			{
				var message = IncomingMessages.Poll(timeout, unit);
				if (message == null)
				{
					return null;
				}
				MessageProcessed(message);
				return BeforeConsume(message);
			}
			catch (ThreadInterruptedException e)
			{
				var state = GetState();
				if (state != State.Closing && state != State.Closed)
				{
					ConsumerStats.IncrementNumReceiveFailed();
					throw PulsarClientException.Unwrap(e);
				}

                return null;
            }
		}

		public override IMessages<T> InternalBatchReceive()
		{
			try
			{
				return InternalBatchReceiveAsync().Result;
			}
			catch (Exception e) when (e is ThreadInterruptedException)
			{
				var state = GetState();
				if (state != State.Closing && state != State.Closed)
				{
					ConsumerStats.IncrementNumBatchReceiveFailed();
					throw PulsarClientException.Unwrap(e);
				}

                return null;
            }
		}

		public override ValueTask<IMessages<T>> InternalBatchReceiveAsync()
		{
			var result = new TaskCompletionSource<IMessages<T>>();
			try
			{
				_lock.AcquireWriterLock(300);
				if (PendingBatchReceives == null)
				{
					PendingBatchReceives = new ConcurrentQueue<OpBatchReceive<T>>();
				}
				if (HasEnoughMessagesForBatchReceive())
				{
					var messages = NewMessagesImpl;
					var msgPeeked = IncomingMessages.Peek();
					while (msgPeeked != null && messages.CanAdd(msgPeeked))
					{
						var msg = IncomingMessages.Poll();
						if (msg != null)
						{
							MessageProcessed(msg);
							var interceptMsg = BeforeConsume(msg);
							messages.Add(interceptMsg);
						}
						msgPeeked = IncomingMessages.Peek();
					}
					result.SetResult(messages);
				}
				else
				{
					PendingBatchReceives.Enqueue(new OpBatchReceive<T>(result));
				}
			}
			finally
			{
				_lock.ReleaseWriterLock();
			}
			return new ValueTask<IMessages<T>>(result.Task.Result);
		}

		public bool MarkAckForBatchMessage(BatchMessageIdImpl batchMessageId, CommandAck.Types.AckType ackType, IDictionary<string, long> properties)
		{
			bool isAllMsgsAcked;
			isAllMsgsAcked = ackType == CommandAck.Types.AckType.Individual ? batchMessageId.AckIndividual() : batchMessageId.AckCumulative();
			var outstandingAcks = 0;
			if (Log.IsEnabled(LogLevel.Debug))
			{
				outstandingAcks = batchMessageId.OutstandingAcksInSameBatch;
			}

			var batchSize = batchMessageId.BatchSize;
			// all messages in this batch have been acked
			if (isAllMsgsAcked)
			{
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] [{}] can ack message to broker {}, acktype {}, cardinality {}, length {}", Subscription, ConsumerName, batchMessageId, ackType, outstandingAcks, batchSize);
				}
				return true;
			}

            if (CommandAck.Types.AckType.Cumulative == ackType && !batchMessageId.Acker.PrevBatchCumulativelyAcked)
            {
                SendAcknowledge(batchMessageId.PrevBatchMessageId(), CommandAck.Types.AckType.Cumulative, properties, null);
                batchMessageId.Acker.PrevBatchCumulativelyAcked = true;
            }
            else
            {
                OnAcknowledge(batchMessageId, null);
            }
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("[{}] [{}] cannot ack message to broker {}, acktype {}, pending acks - {}", Subscription, ConsumerName, batchMessageId, ackType, outstandingAcks);
            }
            return false;
		}

		public override TaskCompletionSource<Task> DoAcknowledge(IMessageId messageId, CommandAck.Types.AckType ackType, IDictionary<string, long> properties, TransactionImpl txnImpl)
		{
			var doTask = new TaskCompletionSource<Task>();
            if (messageId is MessageIdImpl)
            {
                if (GetState() != State.Ready && GetState() != State.Connecting)
                {
                    ConsumerStats.IncrementNumAcksFailed();
                    var exception = new PulsarClientException("Consumer not ready. State: " + GetState());
                    if (CommandAck.Types.AckType.Individual.Equals(ackType))
                    {
                        OnAcknowledge(messageId, exception);
                    }
                    else if (CommandAck.Types.AckType.Cumulative.Equals(ackType))
                    {
                        OnAcknowledgeCumulative(messageId, exception);
                    }
					doTask.SetException(exception);
                    return doTask;
                }
			}


			if (messageId is BatchMessageIdImpl id)
			{
				if (MarkAckForBatchMessage(id, ackType, properties))
				{
					// all messages in batch have been acked so broker can be acked via sendAcknowledge()
					if (Log.IsEnabled(LogLevel.Debug))
					{
						Log.LogDebug("[{}] [{}] acknowledging message - {}, acktype {}", Subscription, ConsumerName, messageId, ackType);
					}
				}
				else
				{
					// other messages in batch are still pending ack.
					doTask.SetResult(null);
                    return doTask;
                }
			}
			return SendAcknowledge(messageId, ackType, properties, txnImpl);
		}

		private TaskCompletionSource<Task> SendAcknowledge(IMessageId messageId, CommandAck.Types.AckType ackType, IDictionary<string, long> properties, TransactionImpl txnImpl)
		{
			var msgId = (MessageIdImpl) messageId;

			if (ackType == CommandAck.Types.AckType.Individual)
			{
				if (messageId is BatchMessageIdImpl batchMessageId)
				{
                    ConsumerStats.IncrementNumAcksSent(batchMessageId.BatchSize);
					UnAckedMessageTracker.Remove(new MessageIdImpl(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex));
                    _possibleSendToDeadLetterTopicMessages?.Remove(new MessageIdImpl(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex));
                }
				else
				{
					// increment counter by 1 for non-batch msg
					UnAckedMessageTracker.Remove(msgId);
                    _possibleSendToDeadLetterTopicMessages?.Remove(msgId);
                    ConsumerStats.IncrementNumAcksSent(1);
				}
				OnAcknowledge(messageId, null);
			}
			else if (ackType == CommandAck.Types.AckType.Cumulative)
			{
				OnAcknowledgeCumulative(messageId, null);
				ConsumerStats.IncrementNumAcksSent(UnAckedMessageTracker.RemoveMessagesTill(msgId));
			}

			_acknowledgmentsGroupingTracker.AddAcknowledgment(msgId, ackType, properties);

			// Consumer acknowledgment operation immediately succeeds. In any case, if we're not able to send ack to broker,
			// the messages will be re-delivered
			return new TaskCompletionSource<Task>();
		}

		public override void NegativeAcknowledge(IMessageId messageId)
		{
			_negativeAcksTracker.Add(messageId);

			// Ensure the message is not redelivered for ack-timeout, since we did receive an "ack"
			UnAckedMessageTracker.Remove(messageId);
		}

		public void ConnectionOpened(ClientCnx cnx)
		{
			ClientCnx = cnx;
			cnx.RegisterConsumer(ConsumerId, this);

			Log.LogInformation("[{}][{}] Subscribing to topic on cnx {}", Topic, Subscription, cnx.Ctx().Channel);

			var requestId = Client.NewRequestId();

			int currentSize;
			lock (this)
			{
				currentSize = IncomingMessages.size();
				_startMessageId = ClearReceiverQueue();
                _possibleSendToDeadLetterTopicMessages?.Clear();
            }

			var isDurable = _subscriptionMode == SubscriptionMode.Durable;
			MessageIdData startMessageIdData;
			if (isDurable)
			{
				// For regular durable subscriptions, the message id from where to restart will be determined by the broker.
				startMessageIdData = null;
			}
			else
			{
				// For non-durable we are going to restart from the next entry
				var builder = MessageIdData.NewBuilder();
				builder.SetLedgerId(_startMessageId.LedgerId);
				builder.SetEntryId(_startMessageId.EntryId);
				if (_startMessageId is BatchMessageIdImpl)
				{
					builder.SetBatchIndex(((BatchMessageIdImpl) _startMessageId).BatchIndex);
				}

				startMessageIdData = builder.Build();
				builder.Recycle();
			}

			var si = (SchemaInfo)Schema.SchemaInfo;
			if (si != null && (SchemaType.Bytes == si.Type || SchemaType.None == si.Type))
			{
				// don't set schema for Schema.BYTES
				si = null;
			}
			// startMessageRollbackDurationInSec should be consider only once when consumer connects to first time
			var startMessageRollbackDuration = (_startMessageRollbackDurationInSec > 0 && _startMessageId.Equals(_initialStartMessageId)) ? _startMessageRollbackDurationInSec : 0;
			var request = Commands.NewSubscribe(Topic, Subscription, ConsumerId, requestId, SubType, _priorityLevel, ConsumerName, isDurable, startMessageIdData, _metadata, _readCompacted, Conf.ReplicateSubscriptionState, CommandSubscribe.ValueOf(_subscriptionInitialPosition.Value), startMessageRollbackDuration, si, _createTopicIfDoesNotExist, Conf.KeySharedPolicy);
            startMessageIdData?.Recycle();

            cnx.SendRequestWithId(request, requestId).AsTask().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    ClientCnx.RemoveConsumer(ConsumerId);
                    if (GetState() == State.Closing || GetState() == State.Closed)
                    {
                        ClientCnx.Channel().CloseAsync();
                        return;
                    }
                    Log.LogWarning("[{}][{}] Failed to subscribe to topic on {}", Topic, Subscription, ClientCnx.Channel().RemoteAddress);
                    if (task.Exception != null && (task.Exception.InnerException is PulsarClientException exception && Handler.IsRetriableError(exception) && DateTimeHelper.CurrentUnixTimeMillis() < _subscribeTimeout))
                    {
                        ReconnectLater(exception);
                    }
                    else if (!SubscribeTask.Task.IsCompleted)
                    {
                        SetState(State.Failed);
                        CloseConsumerTasks();
                        SubscribeTask.SetException(PulsarClientException.Wrap(task.Exception,
                            $"Failed to subscribe the topic {_topicName.ToString()} with subscription " +
                            $"name {Subscription} when connecting to the broker"));
                        Client.CleanupConsumer(this);
                    }
                    else if (task.Exception is PulsarClientException.TopicDoesNotExistException)
                    {
                        SetState(State.Failed);
                        Client.CleanupConsumer(this);
                        Log.LogWarning("[{}][{}] Closed consumer because topic does not exist anymore {}", Topic, Subscription, ClientCnx.Channel().RemoteAddress);
                    }
                    else
                    {
                        ReconnectLater(task.Exception);
                    }
					return;
                }

                lock (this)
                {
                    if (ChangeToReadyState())
                    {
                        ConsumerIsReconnectedToBroker(ClientCnx, currentSize);
                    }
                    else
                    {
                        SetState(State.Closed);
                        ClientCnx.RemoveConsumer(ConsumerId);
                        ClientCnx.Channel().CloseAsync();
                        return;
                    }
                }

                ResetBackoff();
                var firstTimeConnect = SubscribeTask.TrySetResult(this);
                if (!(firstTimeConnect && _hasParentConsumer && isDurable) && Conf.ReceiverQueueSize != 0)
                {
                    SendFlowPermitsToBroker(ClientCnx, Conf.ReceiverQueueSize);
                }
            });
        }

		public void ConsumerIsReconnectedToBroker(ClientCnx cnx, int currentQueueSize)
		{
			Log.LogInformation("[{}][{}] Subscribed to topic on {} -- consumer: {}", Topic, Subscription, cnx.Channel().RemoteAddress, ConsumerId);

			_availablePermits[this] =  0;
		}

		/// <summary>
		/// Clear the internal receiver queue and returns the message id of what was the 1st message in the queue that was
		/// not seen by the application
		/// </summary>
		private BatchMessageIdImpl ClearReceiverQueue()
		{
			IList<IMessage<T>> currentMessageQueue = new List<IMessage<T>>(IncomingMessages.size());
			IncomingMessages.DrainTo(currentMessageQueue);
			IncomingMessagesSize[this] =  0;
			if (currentMessageQueue.Count > 0)
			{
				var nextMessageInQueue = (MessageIdImpl) currentMessageQueue[0].MessageId;
				BatchMessageIdImpl previousMessage;
				if (nextMessageInQueue is BatchMessageIdImpl impl)
				{
					// Get on the previous message within the current batch
					previousMessage = new BatchMessageIdImpl(impl.LedgerId, impl.EntryId, impl.PartitionIndex, impl.BatchIndex - 1);
				}
				else
				{
					// Get on previous message in previous entry
					previousMessage = new BatchMessageIdImpl(nextMessageInQueue.LedgerId, nextMessageInQueue.EntryId - 1, nextMessageInQueue.PartitionIndex, -1);
				}

				return previousMessage;
			}

            if (!LastDequeuedMessage.Equals(MessageIdFields.Earliest))
            {
                // If the queue was empty we need to restart from the message just after the last one that has been dequeued
                // in the past
                return new BatchMessageIdImpl((MessageIdImpl) LastDequeuedMessage);
            }
            // No message was received or dequeued by this consumer. Next message would still be the startMessageId
            return _startMessageId;
        }

		/// <summary>
		/// send the flow command to have the broker start pushing messages
		/// </summary>
		public void SendFlowPermitsToBroker(ClientCnx cnx, int numMessages)
		{
			if (cnx != null)
			{
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] [{}] Adding {} additional permits", Topic, Subscription, numMessages);
				}

				cnx.Ctx().WriteAndFlushAsync(Commands.NewFlow(ConsumerId, numMessages));
			}
		}

		public void ConnectionFailed(PulsarClientException exception)
		{
			if (DateTimeHelper.CurrentUnixTimeMillis() > _subscribeTimeout && SubscribeTask.TrySetException(exception))
			{
				SetState(State.Failed);
				Log.LogInformation("[{}] Consumer creation failed for consumer {}", Topic, ConsumerId);
				Client.CleanupConsumer(this);
			}
		}

		public override ValueTask CloseAsync()
		{
			if (GetState() == State.Closing || GetState() == State.Closed)
			{
				CloseConsumerTasks();
				return new ValueTask(null);
			}

			if (!Connected)
			{
				Log.LogInformation("[{}] [{}] Closed Consumer (not connected)", Topic, Subscription);
				SetState(State.Closed);
				CloseConsumerTasks();
				Client.CleanupConsumer(this);
                return new ValueTask(null);
			}

            ConsumerStats.StatTimeout.Cancel();

			SetState(State.Closing);

			CloseConsumerTasks();

			var requestId = Client.NewRequestId();

			var closeTask = new TaskCompletionSource<Task>();
			var cnx = ClientCnx;
			if (null == cnx)
			{
				CleanupAtClose(closeTask);
			}
			else
			{
				var cmd = Commands.NewCloseConsumer(ConsumerId, requestId);
				cnx.SendRequestWithId(cmd, requestId).AsTask().ContinueWith(task =>
                {
                    var exception = task.Exception;
				    cnx.RemoveConsumer(ConsumerId);
				    if (exception == null || !cnx.Ctx().Channel.Active)
				    {
					    CleanupAtClose(closeTask);
				    }
				    else
				    {
					    closeTask.SetException(exception);
				    }
					closeTask.SetResult(task);
                });
			}

			return new ValueTask(closeTask.Task);
		}

		private void CleanupAtClose(TaskCompletionSource<Task> closeTask)
		{
			Log.LogInformation("[{}] [{}] Closed consumer", Topic, Subscription);
			SetState(State.Closed);
			CloseConsumerTasks();
			closeTask.SetResult(null);
			Client.CleanupConsumer(this);
			// fail all pending-receive futures to notify application
			FailPendingReceive();
		}

		private void CloseConsumerTasks()
		{
			UnAckedMessageTracker.Close();
            _possibleSendToDeadLetterTopicMessages?.Clear();

            _acknowledgmentsGroupingTracker.Close();
		}

		private void FailPendingReceive()
		{
			_lock.AcquireReaderLock(3000);
			try
			{
				if (ListenerExecutor != null /*&& !ListenerExecutor.Shutdown*/)
				{
					while (!PendingReceives.IsEmpty)
					{
						if (PendingReceives.TryPeek(out var receiveTask))
						{
                            receiveTask.SetException(new PulsarClientException.AlreadyClosedException(string.Format("The consumer which subscribes the topic {0} with subscription name {1} " + "was already closed when cleaning and closing the consumers", _topicName.ToString(), Subscription)));
						}
						else
						{
							break;
						}
					}
				}
			}
			finally
			{
				_lock.ReleaseReaderLock();
			}
		}

		public void ActiveConsumerChanged(bool isActive)
		{
			if (ConsumerEventListener == null)
			{
				return;
			}

            Task.Run(() =>
            {
                if (isActive)
                {
                    ConsumerEventListener.BecameActive(this, PartitionIndex);
                }
                else
                {
                    ConsumerEventListener.BecameInactive(this, PartitionIndex);
                }
			});
		}

		public void MessageReceived(MessageIdData messageId, int redeliveryCount, IByteBuffer headersAndPayload, ClientCnx cnx)
		{
			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("[{}][{}] Received message: {}/{}", Topic, Subscription, messageId.LedgerId, messageId.EntryId);
			}

			if (!VerifyChecksum(headersAndPayload, messageId))
			{
				// discard message with checksum error
				DiscardCorruptedMessage(messageId, cnx, CommandAck.Types.ValidationError.ChecksumMismatch);
				return;
			}

			MessageMetadata msgMetadata;
			try
			{
				msgMetadata = Commands.ParseMessageMetadata(headersAndPayload);
			}
			catch (Exception)
			{
				DiscardCorruptedMessage(messageId, cnx, CommandAck.Types.ValidationError.ChecksumMismatch);
				return;
			}
			var numMessages = msgMetadata.NumMessagesInBatch;

			var msgId = new MessageIdImpl((long)messageId.LedgerId, (long)messageId.EntryId, PartitionIndex);
			if (_acknowledgmentsGroupingTracker.IsDuplicate(msgId))
			{
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] [{}] Ignoring message as it was already being acked earlier by same consumer {}/{}", Topic, Subscription, ConsumerName, msgId);
				}

				IncreaseAvailablePermits(cnx, numMessages);
				return;
			}

			var decryptedPayload = DecryptPayloadIfNeeded(messageId, msgMetadata, headersAndPayload, cnx);

			var isMessageUndecryptable = IsMessageUndecryptable(msgMetadata);

			if (decryptedPayload == null)
			{
				// Message was discarded or CryptoKeyReader isn't implemented
				return;
			}

			// uncompress decryptedPayload and release decryptedPayload-ByteBuf
			var uncompressedPayload = isMessageUndecryptable ? (IByteBuffer)decryptedPayload.Retain() : UncompressPayloadIfNeeded(messageId, msgMetadata, decryptedPayload, cnx);
			decryptedPayload.Release();
			if (uncompressedPayload == null)
			{
				// Message was discarded on decompression error
				return;
			}

			// if message is not decryptable then it can't be parsed as a batch-message. so, add EncyrptionCtx to message
			// and return undecrypted payload
			if (isMessageUndecryptable || (numMessages == 1 && !msgMetadata.HasNumMessagesInBatch))
			{

				if (IsResetIncludedAndSameEntryLedger(messageId) && IsPriorEntryIndex((long)messageId.EntryId))
				{
					// We need to discard entries that were prior to startMessageId
					if (Log.IsEnabled(LogLevel.Debug))
					{
						Log.LogDebug("[{}] [{}] Ignoring message from before the startMessageId: {}", Subscription, ConsumerName, _startMessageId);
					}

					uncompressedPayload.Release();
					msgMetadata.Recycle();
					return;
				}

				var message = new MessageImpl<T>(_topicName.ToString(), msgId, msgMetadata, uncompressedPayload, CreateEncryptionContext(msgMetadata), cnx, Schema, redeliveryCount);
				uncompressedPayload.Release();
				msgMetadata.Recycle();

				_lock.AcquireReaderLock(3000);
				try
				{
					// Enqueue the message so that it can be retrieved when application calls receive()
					// if the conf.getReceiverQueueSize() is 0 then discard message if no one is waiting for it.
					// if asyncReceive is waiting then notify callback without adding to incomingMessages queue
					if (_deadLetterPolicy != null && _possibleSendToDeadLetterTopicMessages != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
					{
						_possibleSendToDeadLetterTopicMessages[(MessageIdImpl)message.GetMessageId()] = new List<MessageImpl<T>>{message};
					}
					if (!PendingReceives.IsEmpty)
					{
						NotifyPendingReceivedCallback(message, null);
					}
					else if (EnqueueMessageAndCheckBatchReceive(message))
					{
						if (HasPendingBatchReceive())
						{
							NotifyPendingBatchReceivedCallBack();
						}
					}
				}
				finally
				{
					_lock.ReleaseReaderLock();
				}
			}
			else
			{
				// handle batch message enqueuing; uncompressed payload has all messages in batch
				ReceiveIndividualMessagesFromBatch(msgMetadata, redeliveryCount, uncompressedPayload, messageId, cnx);

				uncompressedPayload.Release();
				msgMetadata.Recycle();
			}

			if (Listener != null)
			{
				TriggerListener(numMessages);
			}
		}

		public void TriggerListener(int numMessages)
		{
			// Trigger the notification on the message listener in a separate thread to avoid blocking the networking
			// thread while the message processing happens
            Task.Run(() =>
            {
                for (var i = 0; i < numMessages; i++)
                {
                    try
                    {
                        var msg = InternalReceive(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
                        if (msg == null)
                        {
                            if (Log.IsEnabled(LogLevel.Debug))
                            {
                                Log.LogDebug("[{}] [{}] Message has been cleared from the queue", Topic, Subscription);
                            }
                            break;
                        }
                        try
                        {
                            if (Log.IsEnabled(LogLevel.Debug))
                            {
                                Log.LogDebug("[{}][{}] Calling message listener for message {}", Topic, Subscription, msg.MessageId);
                            }
                            Listener.Received(this, msg);
                        }
                        catch (Exception T)
                        {
                            Log.LogError("[{}][{}] Message listener error in processing message: {}", Topic, Subscription, msg.MessageId, T);
                        }
                    }
                    catch (PulsarClientException e)
                    {
                        Log.LogWarning("[{}] [{}] Failed to dequeue the message for listener", Topic, Subscription, e);
                        return;
                    }
                }
			});
        }

		/// <summary>
		/// Notify waiting asyncReceive request with the received message
		/// </summary>
		/// <param name="message"> </param>
		public void NotifyPendingReceivedCallback(in IMessage<T> message, Exception exception)
		{
			if (PendingReceives.IsEmpty)
			{
				return;
			}

			// fetch receivedCallback from queue
			if (!PendingReceives.TryPeek(out var receivedTask))
			{
				return;
			}

			if (exception != null)
            {
                Task.Run(() => receivedTask.SetException(exception));
				return;
			}

			if (message == null)
			{
                var e = new InvalidOperationException("received message can't be null");
                Task.Run(() => receivedTask.SetException(e));
				return;
			}

			if (Conf.ReceiverQueueSize == 0)
			{
				// call interceptor and complete received callback
				InterceptAndComplete(message, receivedTask);
				return;
			}

			// increase permits for available message-queue
			MessageProcessed(message);
			// call interceptor and complete received callback
			InterceptAndComplete(message, receivedTask);
		}

		private void InterceptAndComplete(IMessage<T> message, TaskCompletionSource<IMessage<T>> receivedTask)
        {
			// call proper interceptor
			var interceptMessage = BeforeConsume(message);
			// return message to receivedCallback
            Task.Run(() => receivedTask.SetResult(interceptMessage));
		}

		public void ReceiveIndividualMessagesFromBatch(MessageMetadata msgMetadata, int redeliveryCount, IByteBuffer uncompressedPayload, MessageIdData messageId, ClientCnx cnx)
		{
			var batchSize = msgMetadata.NumMessagesInBatch;

			// create ack tracker for entry aka batch
			var batchMessage = new MessageIdImpl((long)messageId.LedgerId, (long)messageId.EntryId, PartitionIndex);
			var acker = BatchMessageAcker.NewAcker(batchSize);
			IList<MessageImpl<T>> possibleToDeadLetter = null;
			if (_deadLetterPolicy != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
			{
				possibleToDeadLetter = new List<MessageImpl<T>>();
			}
			var skippedMessages = 0;
			try
			{
				for (var i = 0; i < batchSize; ++i)
				{
					if (Log.IsEnabled(LogLevel.Debug))
					{
						Log.LogDebug("[{}] [{}] processing message num - {} in batch", Subscription, ConsumerName, i);
					}
					var singleMessageMetadataBuilder = SingleMessageMetadata.NewBuilder();
					var singleMessagePayload = Commands.DeSerializeSingleMessageInBatch(uncompressedPayload, singleMessageMetadataBuilder, i, batchSize);

					if (IsResetIncludedAndSameEntryLedger(messageId) && IsPriorBatchIndex(i))
					{
						// If we are receiving a batch message, we need to discard messages that were prior
						// to the startMessageId
						if (Log.IsEnabled(LogLevel.Debug))
						{
							Log.LogDebug("[{}] [{}] Ignoring message from before the startMessageId: {}", Subscription, ConsumerName, _startMessageId);
						}
						singleMessagePayload.Release();
						singleMessageMetadataBuilder.Recycle();

						++skippedMessages;
						continue;
					}

					if (singleMessageMetadataBuilder.CompactedOut)
					{
						// message has been compacted out, so don't send to the user
						singleMessagePayload.Release();
						singleMessageMetadataBuilder.Recycle();

						++skippedMessages;
						continue;
					}

					var batchMessageIdImpl = new BatchMessageIdImpl((long)messageId.LedgerId, (long)messageId.EntryId, PartitionIndex, i, acker);

                    var message = new MessageImpl<T>(_topicName.ToString(), batchMessageIdImpl, msgMetadata, singleMessageMetadataBuilder.Build(), singleMessagePayload, CreateEncryptionContext(msgMetadata), cnx, Schema, redeliveryCount);
                    possibleToDeadLetter?.Add(message);
                    _lock.AcquireReaderLock(300);
					try
					{
						if (!PendingReceives.IsEmpty)
						{
							NotifyPendingReceivedCallback(message, null);
						}
						else if (EnqueueMessageAndCheckBatchReceive(message))
						{
							if (HasPendingBatchReceive())
							{
								NotifyPendingBatchReceivedCallBack();
							}
						}
					}
					finally
					{
						_lock.ReleaseReaderLock();
					}
					singleMessagePayload.Release();
					singleMessageMetadataBuilder.Recycle();
				}
			}
			catch (IOException)
			{
				Log.LogWarning("[{}] [{}] unable to obtain message in batch", Subscription, ConsumerName);
				DiscardCorruptedMessage(messageId, cnx, CommandAck.Types.ValidationError.BatchDeSerializeError);
			}

			if (possibleToDeadLetter != null && _possibleSendToDeadLetterTopicMessages != null)
			{
				_possibleSendToDeadLetterTopicMessages[batchMessage] = possibleToDeadLetter;
			}

			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("[{}] [{}] enqueued messages in batch. queue size - {}, available queue size - {}", Subscription, ConsumerName, IncomingMessages.size(), IncomingMessages.RemainingCapacity());
			}

			if (skippedMessages > 0)
			{
				IncreaseAvailablePermits(cnx, skippedMessages);
			}
		}

		private bool IsPriorEntryIndex(long idx)
		{
			return _resetIncludeHead ? idx < _startMessageId.EntryId : idx <= _startMessageId.EntryId;
		}

		private bool IsPriorBatchIndex(long idx)
		{
			return _resetIncludeHead ? idx < _startMessageId.BatchIndex : idx <= _startMessageId.BatchIndex;
		}

		private bool IsResetIncludedAndSameEntryLedger(MessageIdData messageId)
		{
			return !_resetIncludeHead && _startMessageId != null && (long)messageId.LedgerId == _startMessageId.LedgerId && (long)messageId.EntryId == _startMessageId.EntryId;
		}

		/// <summary>
		/// Record the event that one message has been processed by the application.
		/// 
		/// Periodically, it sends a Flow command to notify the broker that it can push more messages
		/// </summary>
		public override void MessageProcessed(IMessage<T> msg)
		{
			lock (this)
			{
				var currentCnx = Cnx();
				var msgCnx = ((MessageImpl<T>) msg).Cnx;
				LastDequeuedMessage = msg.MessageId;
        
				if (msgCnx != currentCnx)
				{
					// The processed message did belong to the old queue that was cleared after reconnection.
					return;
				}
        
				IncreaseAvailablePermits(currentCnx);
				ConsumerStats.UpdateNumMsgsReceived(msg);
        
				TrackMessage(msg);
				IncomingMessagesSize[this] = -msg.Data.Length;
			}
		}

		public void TrackMessage<T1>(IMessage<T1> msg)
		{
			if (msg != null)
			{
				var messageId = msg.MessageId;
				if (Conf.AckTimeoutMillis > 0 && messageId is MessageIdImpl id)
				{
                    if (id is BatchMessageIdImpl)
					{
						// do not add each item in batch message into tracker
						id = new MessageIdImpl(id.LedgerId, id.EntryId, PartitionIndex);
					}
					if (_hasParentConsumer)
					{
						// we should no longer track this message, TopicsConsumer will take care from now onwards
						UnAckedMessageTracker.Remove(id);
					}
					else
					{
						UnAckedMessageTracker.Add(id);
					}
				}
			}
		}

		public void IncreaseAvailablePermits(ClientCnx currentCnx)
		{
			IncreaseAvailablePermits(currentCnx, 1);
		}

		private void IncreaseAvailablePermits(ClientCnx currentCnx, int delta)
        {
            _availablePermits[this] = delta;
			var available = _availablePermits[this];

			while (available >= _receiverQueueRefillThreshold && !Paused)
            {
                if (_availablePermits.TryUpdate(this, 0, available))
				{
					SendFlowPermitsToBroker(currentCnx, available);
					break;
				}

                available = _availablePermits[this];
            }
		}

		public override void Pause()
		{
			Paused = true;
		}

		public override void Resume()
		{
			if (Paused)
			{
				Paused = false;
				IncreaseAvailablePermits(Cnx(), 0);
			}
		}

		private IByteBuffer DecryptPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, IByteBuffer payload, ClientCnx currentCnx)
		{

			if (msgMetadata.EncryptionKeys.Count == 0)
			{
				payload.Retain();
                return payload;
            }

			// If KeyReader is not configured throw exception based on config param
			if (Conf.CryptoKeyReader == null)
			{
				switch (Conf.CryptoFailureAction)
				{
					case ConsumerCryptoFailureAction.Consume:
						Log.LogWarning("[{}][{}][{}] CryptoKeyReader interface is not implemented. Consuming encrypted message.", Topic, Subscription, ConsumerName);
						payload.Retain();
                        return payload;
					case ConsumerCryptoFailureAction.Discard:
						Log.LogWarning("[{}][{}][{}] Skipping decryption since CryptoKeyReader interface is not implemented and config is set to discard", Topic, Subscription, ConsumerName);
						DiscardMessage(messageId, currentCnx, CommandAck.Types.ValidationError.DecryptionError);
						return null;
					case ConsumerCryptoFailureAction.Fail:
						IMessageId m = new MessageIdImpl((long)messageId.LedgerId, (long)messageId.EntryId, PartitionIndex);
						Log.LogError("[{}][{}][{}][{}] Message delivery failed since CryptoKeyReader interface is not implemented to consume encrypted message", Topic, Subscription, ConsumerName, m);
						UnAckedMessageTracker.Add(m);
						return null;
				}
			}

			var decryptedData = _msgCrypto.Decrypt(msgMetadata, payload, Conf.CryptoKeyReader);
			if (decryptedData != null)
			{
				return decryptedData;
			}

			switch (Conf.CryptoFailureAction)
			{
				case ConsumerCryptoFailureAction.Consume:
					// Note, batch message will fail to consume even if config is set to consume
					Log.LogWarning("[{}][{}][{}][{}] Decryption failed. Consuming encrypted message since config is set to consume.", Topic, Subscription, ConsumerName, messageId);
					payload.Retain();
                    return payload;
				case ConsumerCryptoFailureAction.Discard:
					Log.LogWarning("[{}][{}][{}][{}] Discarding message since decryption failed and config is set to discard", Topic, Subscription, ConsumerName, messageId);
					DiscardMessage(messageId, currentCnx, CommandAck.Types.ValidationError.DecryptionError);
					return null;
				case ConsumerCryptoFailureAction.Fail:
					var m = new MessageIdImpl((long)messageId.LedgerId, (long)messageId.EntryId, PartitionIndex);
					Log.LogError("[{}][{}][{}][{}] Message delivery failed since unable to decrypt incoming message", Topic, Subscription, ConsumerName, m);
					UnAckedMessageTracker.Add(m);
					return null;
			}
			return null;
		}

		private IByteBuffer UncompressPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, IByteBuffer payload, ClientCnx currentCnx)
		{
			var compressionType = msgMetadata.Compression;
			var codec = CompressionCodecProvider.GetCompressionCodec((int)compressionType);
			var uncompressedSize = (int)msgMetadata.UncompressedSize;
			var payloadSize = payload.ReadableBytes;
			if (payloadSize > ClientCnx.MaxMessageSize)
			{
				// payload size is itself corrupted since it cannot be bigger than the MaxMessageSize
				Log.LogError("[{}][{}] Got corrupted payload message size {} at {}", Topic, Subscription, payloadSize, messageId);
				DiscardCorruptedMessage(messageId, currentCnx, CommandAck.Types.ValidationError.UncompressedSizeCorruption);
				return null;
			}

			try
			{
				var uncompressedPayload = codec.Decode(payload, uncompressedSize);
				return uncompressedPayload;
			}
			catch (IOException e)
			{
				Log.LogError("[{}][{}] Failed to decompress message with {} at {}: {}", Topic, Subscription, compressionType, messageId, e.Message, e);
				DiscardCorruptedMessage(messageId, currentCnx, CommandAck.Types.ValidationError.DecompressionError);
				return null;
			}
		}

		private bool VerifyChecksum(IByteBuffer headersAndPayload, MessageIdData messageId)
		{

			if (Commands.HasChecksum(headersAndPayload))
			{
				var checksum = Commands.ReadChecksum(headersAndPayload);
				int computedChecksum = Commands.ComputeChecksum(headersAndPayload);
				if (checksum != computedChecksum)
				{
					Log.LogError("[{}][{}] Checksum mismatch for message at {}:{}. Received checksum: 0x{}, Computed checksum: 0x{}", Topic, Subscription, messageId.LedgerId, messageId.EntryId, checksum.ToString("x"), computedChecksum.ToString("x"));
					return false;
				}
			}

			return true;
		}

		private void DiscardCorruptedMessage(MessageIdData messageId, ClientCnx currentCnx, CommandAck.Types.ValidationError validationError)
		{
			Log.LogError("[{}][{}] Discarding corrupted message at {}:{}", Topic, Subscription, messageId.LedgerId, messageId.EntryId);
			DiscardMessage(messageId, currentCnx, validationError);
		}

		private void DiscardMessage(MessageIdData messageId, ClientCnx currentCnx, CommandAck.Types.ValidationError validationError)
		{
			var cmd = Commands.NewAck(ConsumerId, (long)messageId.LedgerId, (long)messageId.EntryId, CommandAck.Types.AckType.Individual, validationError, new Dictionary<string,long>());
			currentCnx.Ctx().WriteAndFlushAsync(cmd);
			IncreaseAvailablePermits(currentCnx);
			ConsumerStats.IncrementNumReceiveFailed();
		}

		public new string HandlerName => Subscription;

        public override bool Connected => ClientCnx != null && (GetState() == State.Ready);


        public override int AvailablePermits => _availablePermits[this];

        public override int NumMessagesInQueue()
		{
			return IncomingMessages.size();
		}

		public override void RedeliverUnacknowledgedMessages()
		{
			var cnx = ClientCnx;
			if (Connected && cnx.RemoteEndpointProtocolVersion >= (int)ProtocolVersion.V2)
			{
				var currentSize = 0;
				lock (this)
				{
					currentSize = IncomingMessages.size();
					IncomingMessages.clear();
					IncomingMessagesSize[this] =  0;
					UnAckedMessageTracker.Clear();
				}
				cnx.Ctx().WriteAndFlushAsync(Commands.NewRedeliverUnacknowledgedMessages(ConsumerId));
				if (currentSize > 0)
				{
					IncreaseAvailablePermits(cnx, currentSize);
				}
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] [{}] [{}] Redeliver unacked messages and send {} permits", Subscription, Topic, ConsumerName, currentSize);
				}
				return;
			}
			if (cnx == null || (GetState() == State.Connecting))
			{
				Log.LogWarning("[{}] Client Connection needs to be established for redelivery of unacknowledged messages", this);
			}
			else
			{
				Log.LogWarning("[{}] Reconnecting the client to redeliver the messages.", this);
				cnx.Ctx().CloseAsync();
			}
		}

		public override void RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds)
		{
			if (messageIds.Count == 0)
			{
				return;
			}

			if(!(messageIds.First() is MessageIdImpl))
				throw new ArgumentException();

			if (Conf.SubscriptionType != SubscriptionType.Shared && Conf.SubscriptionType != SubscriptionType.KeyShared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				RedeliverUnacknowledgedMessages();
				return;
			}
			var cnx = ClientCnx;
			if (Connected && cnx.RemoteEndpointProtocolVersion >= (int)ProtocolVersion.V2)
			{
				var messagesFromQueue = RemoveExpiredMessagesFromQueue(messageIds);
                var i = 0;
				var batches = messageIds.PartitionMessageId(MaxRedeliverUnacknowledged);
				var builder = MessageIdData.NewBuilder();
				batches.ForEach(ids =>
				{
				    var messageIdDatas = ids.Where(messageId => !ProcessPossibleToDlq(messageId)).Select(messageId =>
				    {
					    builder.SetPartition(messageId.PartitionIndex);
					    builder.SetLedgerId(messageId.LedgerId);
					    builder.SetEntryId(messageId.EntryId);
					    return builder.Build();
				    }).ToList();
				    var cmd = Commands.NewRedeliverUnacknowledgedMessages(ConsumerId, messageIdDatas);
				    cnx.Ctx().WriteAndFlushAsync(cmd);
				    messageIdDatas.ForEach(x => x.Recycle());
				});
				if (messagesFromQueue > 0)
				{
					IncreaseAvailablePermits(cnx, messagesFromQueue);
				}
				builder.Recycle();
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] [{}] [{}] Redeliver unacked messages and increase {} permits", Subscription, Topic, ConsumerName, messagesFromQueue);
				}
				return;
			}
			if (cnx == null || (GetState() == State.Connecting))
			{
				Log.LogWarning("[{}] Client Connection needs to be established for redelivery of unacknowledged messages", this);
			}
			else
			{
				Log.LogWarning("[{}] Reconnecting the client to redeliver the messages.", this);
				cnx.Ctx().CloseAsync();
			}
		}

		public override void CompleteOpBatchReceive(OpBatchReceive<T> op)
		{
			NotifyPendingBatchReceivedCallBack(op);
		}

		private bool ProcessPossibleToDlq(MessageIdImpl messageId)
		{
			IList<MessageImpl<T>> deadLetterMessages = null;
			if (_possibleSendToDeadLetterTopicMessages != null)
            {
                deadLetterMessages = messageId is BatchMessageIdImpl ? _possibleSendToDeadLetterTopicMessages[new MessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex)] : _possibleSendToDeadLetterTopicMessages[messageId];
            }
			if (deadLetterMessages != null)
			{
				if (_deadLetterProducer == null)
				{
					try
					{
						_deadLetterProducer = Client.NewProducer(Schema).Topic(_deadLetterPolicy.DeadLetterTopic).BlockIfQueueFull(false).Create();
					}
					catch (Exception e)
					{
						Log.LogError("Create dead letter producer exception with topic: {}", _deadLetterPolicy.DeadLetterTopic, e);
					}
				}
				if (_deadLetterProducer != null)
				{
					try
					{
						foreach (var message in deadLetterMessages)
						{
							_deadLetterProducer.NewMessage().Value(message.Value).Properties(message.Properties).Send();
						}
						Acknowledge(messageId);
						return true;
					}
					catch (Exception e)
					{
						Log.LogError("Send to dead letter topic exception with topic: {}, messageId: {}", _deadLetterProducer.Topic, messageId, e);
					}
				}
			}
			return false;
		}

		public override void Seek(IMessageId messageId)
		{
			try
			{
				SeekAsync(messageId);
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public override void Seek(long timestamp)
		{
			try
			{
				SeekAsync(timestamp);
			}
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public override ValueTask SeekAsync(long timestamp)
		{
			if (GetState() == State.Closing || GetState() == State.Closed)
			{
				return new ValueTask(Task.FromException(new PulsarClientException.AlreadyClosedException(
                    $"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic " +
                    $"{_topicName.ToString()} to the timestamp {timestamp:D}")));
			}

			if (!Connected)
			{
                return new ValueTask(Task.FromException(new PulsarClientException(
                    $"The client is not connected to the broker when seeking the subscription {Subscription} of the " +
                    $"topic {_topicName.ToString()} to the timestamp {timestamp:D}")));
			}

			var seekTask = new TaskCompletionSource<Task>();

			var requestId = Client.NewRequestId();
			var seek = Commands.NewSeek(ConsumerId, requestId, timestamp);
			var cnx = ClientCnx;

			Log.LogInformation("[{}][{}] Seek subscription to publish time {}", Topic, Subscription, timestamp);

			cnx.SendRequestWithId(seek, requestId).AsTask().ContinueWith(task =>
			{
                if (task.IsFaulted)
                {
                    Log.LogError("[{}][{}] Failed to reset subscription: {}", Topic, Subscription, task.Exception.Message);
                    seekTask.SetException(PulsarClientException.Wrap(task.Exception,
                        $"Failed to seek the subscription {Subscription} of the topic {_topicName.ToString()} to the timestamp {timestamp:D}"));
                    
					return;
                }
			    Log.LogInformation("[{}][{}] Successfully reset subscription to publish time {}", Topic, Subscription, timestamp);
			    _acknowledgmentsGroupingTracker.FlushAndClean();
			    LastDequeuedMessage = MessageIdFields.Earliest;
			    IncomingMessages.clear();
			    IncomingMessagesSize[this] = 0;
			    seekTask.SetResult(null);
			});
			return new ValueTask(seekTask.Task); 
		}

		public override ValueTask SeekAsync(IMessageId messageId)
		{
			if (GetState() == State.Closing || GetState() == State.Closed)
			{
                return new ValueTask(Task.FromException(new PulsarClientException.AlreadyClosedException(
                    $"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic " +
                    $"{_topicName.ToString()} to the message {messageId.ToString()}")));
			}

			if (!Connected)
			{
                return new ValueTask(Task.FromException(new PulsarClientException(
                    $"The client is not connected to the broker when seeking the subscription {Subscription} of the " +
                    $"topic {_topicName.ToString()} to the message {messageId.ToString()}")));
			}

            var seekTask = new TaskCompletionSource<Task>();

			var requestId = Client.NewRequestId();
			var msgId = (MessageIdImpl) messageId;
			var seek = Commands.NewSeek(ConsumerId, requestId, msgId.LedgerId, msgId.EntryId);
			var cnx = ClientCnx;

			Log.LogInformation("[{}][{}] Seek subscription to message id {}", Topic, Subscription, messageId);

			cnx.SendRequestWithId(seek, requestId).AsTask().ContinueWith( task =>
			{
                if (task.IsFaulted)
                {
                    Log.LogError("[{}][{}] Failed to reset subscription: {}", Topic, Subscription, task.Exception.Message);
                    seekTask.TrySetException(PulsarClientException.Wrap(task.Exception, string.Format("[{0}][{1}] Failed to seek the subscription {2} of the topic {3} to the message {4}", Subscription, _topicName.ToString(), messageId.ToString())));
                    return;
                }
			    Log.LogInformation("[{}][{}] Successfully reset subscription to message id {}", Topic, Subscription, messageId);
			    _acknowledgmentsGroupingTracker.FlushAndClean();
			    LastDequeuedMessage = messageId;
			    IncomingMessages.clear();
			    IncomingMessagesSize[this] = 0;
			    seekTask.SetResult(null);
			});
			return new ValueTask(seekTask.Task);
		}

		public bool HasMessageAvailable()
		{
			// we need to seek to the last position then the last message can be received when the resetIncludeHead
			// specified.
			if (Equals(LastDequeuedMessage, MessageIdFields.Latest) && _resetIncludeHead)
			{
				LastDequeuedMessage = LastMessageId;
				Seek(LastDequeuedMessage);
			}
			try
            {
                return HasMoreMessages(_lastMessageIdInBroker, LastDequeuedMessage) || HasMessageAvailableAsync().Result;
            }
			catch (Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public ValueTask<bool> HasMessageAvailableAsync()
		{
			var boolTask = new TaskCompletionSource<bool>();

			if (HasMoreMessages(_lastMessageIdInBroker, LastDequeuedMessage))
			{
				boolTask.SetResult(true);
			}
			else
			{
				LastMessageIdAsync.AsTask().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Log.LogError("[{}][{}] Failed getLastMessageId command", Topic, Subscription);
                        boolTask.SetException(task.Exception ?? throw new InvalidOperationException());
                        return;
					}
                    _lastMessageIdInBroker = task.Result;
                    boolTask.SetResult(HasMoreMessages(_lastMessageIdInBroker, LastDequeuedMessage));
                });
			}
			return new ValueTask<bool>(boolTask.Task.Result);
		}

		private bool HasMoreMessages(IMessageId lastMessageIdInBroker, IMessageId lastDequeuedMessage)
		{
			if (lastMessageIdInBroker.CompareTo(lastDequeuedMessage) > 0 && ((MessageIdImpl)lastMessageIdInBroker).EntryId != -1)
			{
				return true;
			}

            // Make sure batching message can be read completely.
            return lastMessageIdInBroker.CompareTo(lastDequeuedMessage) == 0 && IncomingMessages.size() > 0;
        }

		public override ValueTask<IMessageId> LastMessageIdAsync
		{
			get
			{
				if (GetState() == State.Closing || GetState() == State.Closed)
				{
					return new ValueTask<IMessageId>(Task.FromException<IMessageId>(new PulsarClientException.AlreadyClosedException(string.Format("The consumer {0} was already closed when the subscription {1} of the topic {2} " + "getting the last message id", ConsumerName, Subscription, _topicName.ToString()))));
				}
    
				var opTimeoutMs = new AtomicLong(Client.Configuration.OperationTimeoutMs);
				var backoff = (new BackoffBuilder()).SetInitialTime(100, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).SetMax(opTimeoutMs.Get() * 2, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).SetMandatoryStop(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).Create();
    
				var getLastMessageIdTask = new TaskCompletionSource<IMessageId>();
    
				InternalGetLastMessageIdAsync(backoff, opTimeoutMs, getLastMessageIdTask);
				return new ValueTask<IMessageId>(getLastMessageIdTask.Task.Result); 
			}
		}

		private void InternalGetLastMessageIdAsync(Backoff backoff, AtomicLong remainingTime, TaskCompletionSource<IMessageId> task)
		{
			var cnx = ClientCnx;
			if (Connected && cnx != null)
			{
				if (!Commands.PeerSupportsGetLastMessageId(cnx.RemoteEndpointProtocolVersion))
				{
					task.SetException(new PulsarClientException.NotSupportedException(
                        $"The command `GetLastMessageId` is not supported for the protocol version {cnx.RemoteEndpointProtocolVersion:D}. " +
                        $"The consumer is {ConsumerName}, topic {_topicName.ToString()}, subscription {Subscription}"));
				}

				var requestId = Client.NewRequestId();
				var getLastIdCmd = Commands.NewGetLastMessageId(ConsumerId, requestId);
				Log.LogInformation("[{}][{}] Get topic last message Id", Topic, Subscription);

				cnx.SendGetLastMessageId(getLastIdCmd, requestId).AsTask().ContinueWith(taskC =>
                {
                    if (taskC.IsFaulted)
                    {
                        Log.LogError("[{}][{}] Failed getLastMessageId command", Topic, Subscription);
                        task.SetException(PulsarClientException.Wrap(taskC.Exception,
                            $"The subscription {Subscription} of the topic {_topicName.ToString()} gets the last message id was failed"));
                        return;
                    }
                    var result = taskC.Result;
				    Log.LogInformation("[{}][{}] Successfully getLastMessageId {}:{}", Topic, Subscription, result.LedgerId, result.EntryId);
				    task.SetResult(new MessageIdImpl((long)result.LedgerId, (long)result.EntryId, result.Partition));
				});
			}
			else
			{
				var nextDelay = Math.Min(backoff.Next(), remainingTime.Get());
				if (nextDelay <= 0)
				{
					task.SetException(new PulsarClientException.TimeoutException(
                        $"The subscription {Subscription} of the topic {_topicName.ToString()} could not get the last message id " +
                        "withing configured timeout"));
					return;
				}

				ListenerExecutor.Schedule(() =>
                {
                    
				    Log.LogWarning("[{}] [{}] Could not get connection while getLastMessageId -- Will try again in {} ms", Topic, HandlerName, nextDelay);
				        remainingTime.AddAndGet(-nextDelay);
				    InternalGetLastMessageIdAsync(backoff, remainingTime, task);
				}, TimeSpan.FromMilliseconds(nextDelay));
			}
		}

		private MessageIdImpl GetMessageIdImpl<T1>(IMessage<T1> msg)
		{
			var messageId = (MessageIdImpl) msg.MessageId;
			if (messageId is BatchMessageIdImpl)
			{
				// messageIds contain MessageIdImpl, not BatchMessageIdImpl
				messageId = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex);
			}
			return messageId;
		}


		private bool IsMessageUndecryptable(MessageMetadata msgMetadata)
		{
			return (msgMetadata.EncryptionKeys.Count > 0 && Conf.CryptoKeyReader == null && Conf.CryptoFailureAction == ConsumerCryptoFailureAction.Consume);
		}

		/// <summary>
		/// Create EncryptionContext if message payload is encrypted
		/// </summary>
		/// <param name="msgMetadata"> </param>
		/// <returns> <seealso cref="Optional"/><<seealso cref="EncryptionContext"/>> </returns>
		private EncryptionContext CreateEncryptionContext(MessageMetadata msgMetadata)
		{

			EncryptionContext encryptionCtx = null;
			if (msgMetadata.EncryptionKeys.Count > 0)
			{
				encryptionCtx = new EncryptionContext();
				IDictionary<string, EncryptionContext.EncryptionKey> keys = msgMetadata.EncryptionKeys.ToDictionary(e => e.Key, e => new EncryptionContext.EncryptionKey{ KeyValue = (sbyte[])(object)e.Value.ToByteArray(), Metadata = e.Metadata?.ToDictionary(k=> k.Key, k => k.Value)});
				var encParam = new sbyte[MessageCrypto.IvLen];
				msgMetadata.EncryptionParam.CopyTo((byte[])(object)encParam, 0);
				int? batchSize = msgMetadata.HasNumMessagesInBatch ? msgMetadata.NumMessagesInBatch : 0;
				encryptionCtx.Keys = keys;
				encryptionCtx.Param = encParam;
				encryptionCtx.Algorithm = msgMetadata.EncryptionAlgo;
                encryptionCtx.CompressionType = (int)msgMetadata.Compression;//CompressionCodecProvider.ConvertFromWireProtocol(msgMetadata.Compression);
				encryptionCtx.UncompressedMessageSize = (int)msgMetadata.UncompressedSize;
				encryptionCtx.BatchSize = batchSize;
			}
			return encryptionCtx;
		}

		private int RemoveExpiredMessagesFromQueue(ISet<IMessageId> messageIds)
		{
			var messagesFromQueue = 0;
			var peek = IncomingMessages.Peek();
			if (peek != null)
			{
				var messageId = GetMessageIdImpl(peek);
				if (!messageIds.Contains(messageId))
				{
					// first message is not expired, then no message is expired in queue.
					return 0;
				}

				// try not to remove elements that are added while we remove
				var message = IncomingMessages.Poll();
				while (message != null)
				{
					IncomingMessagesSize[this] =  -message.Data.Length;
					messagesFromQueue++;
					var id = GetMessageIdImpl(message);
					if (!messageIds.Contains(id))
					{
						messageIds.Add(id);
						break;
					}
					message = IncomingMessages.Poll();
				}
			}
			return messagesFromQueue;
		}

		public override IConsumerStats Stats => ConsumerStats;

        public void SetTerminated()
		{
			Log.LogInformation("[{}] [{}] [{}] Consumer has reached the end of topic", Subscription, Topic, ConsumerName);
			_hasReachedEndOfTopic = true;
            // Propagate notification to listener
            Listener?.ReachedEndOfTopic(this);
        }

		public override bool HasReachedEndOfTopic()
		{
			return _hasReachedEndOfTopic;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Topic, Subscription, ConsumerName);
		}

		// wrapper for connection methods
		public ClientCnx Cnx()
		{
			return Handler.Cnx();
		}

		public void ResetBackoff()
		{
			Handler.ResetBackoff();
		}

		public void ConnectionClosed(ClientCnx cnx)
		{
			Handler.ConnectionClosed(cnx);
		}

		public ClientCnx ClientCnx
		{
			get => Handler.ClientCnx;
            set => Handler.ClientCnx = value;
        }


		public void ReconnectLater(Exception exception)
		{
			Handler.ReconnectLater(exception);
		}

		public void GrabCnx()
		{
			Handler.GrabCnx();
		}


		internal static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(ConsumerImpl<T>));

	}

}