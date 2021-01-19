using Akka.Actor;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Akka;
using SharpPulsar.Batch;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Crypto;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Stats.Consumer;
using SharpPulsar.Tracker;
using SharpPulsar.Tracker.Api;
using SharpPulsar.Transaction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
namespace SharpPulsar
{

	public class ConsumerActor<T> : ConsumerActorBase<T>
	{
		private const int MaxRedeliverUnacknowledged = 1000;

		internal readonly long ConsumerId;

		// Number of messages that have delivered to the application. Every once in a while, this number will be sent to the
		// broker to notify that we are ready to get (and store in the incoming messages queue) more messages

		private volatile int _availablePermits = 0;

		protected internal volatile IMessageId LastDequeuedMessageId = IMessageId.Earliest;
		private volatile IMessageId _lastMessageIdInBroker = IMessageId.Earliest;

		private long _subscribeTimeout;
		private readonly int _partitionIndex;
		private readonly bool _hasParentConsumer;

		private readonly int _receiverQueueRefillThreshold;

		private readonly UnAckedMessageTracker _unAckedMessageTracker;
		private readonly IAcknowledgmentsGroupingTracker _acknowledgmentsGroupingTracker;
		private readonly NegativeAcksTracker<T> _negativeAcksTracker;

		protected internal readonly ConsumerStatsRecorder StatsConflict;
		private readonly int _priorityLevel;
		private readonly SubscriptionMode _subscriptionMode;
		private volatile BatchMessageId _startMessageId;

		private volatile BatchMessageId _seekMessageId;
		private readonly AtomicBoolean _duringSeek;

		private readonly BatchMessageId _initialStartMessageId;

		private readonly long _startMessageRollbackDurationInSec;

		private volatile bool _hasReachedEndOfTopic;

		private readonly IMessageCrypto _msgCrypto;

		private readonly IDictionary<string, string> _metadata;

		private readonly bool _readCompacted;
		private readonly bool _resetIncludeHead;

		private readonly SubscriptionInitialPosition _subscriptionInitialPosition;
		private readonly ConnectionHandler _connectionHandler;

		private readonly TopicName _topicName;
		private readonly string _topicNameWithoutPartition;

		private readonly IDictionary<MessageId, IList<Message<T>>> _possibleSendToDeadLetterTopicMessages;

		private readonly DeadLetterPolicy _deadLetterPolicy;

		private IActorRef _deadLetterProducer;

		private volatile IActorRef _retryLetterProducer;

		protected internal volatile bool Paused;

		protected internal ConcurrentDictionary<string, ChunkedMessageCtx> ChunkedMessagesMap = new ConcurrentDictionary<string, ChunkedMessageCtx>();
		private int _pendingChunckedMessageCount = 0;
		protected internal long ExpireTimeOfIncompleteChunkedMessageMillis = 0;
		private bool _expireChunkMessageTaskScheduled = false;
		private int _maxPendingChuckedMessage;
		// if queue size is reasonable (most of the time equal to number of producers try to publish messages concurrently on
		// the topic) then it guards against broken chuncked message which was not fully published
		private bool _autoAckOldestChunkedMessageOnQueueFull;
		// it will be used to manage N outstanding chunked mesage buffers
		private readonly BlockingQueue<string> _pendingChunckedMessageUuidQueue;

		private readonly bool _createTopicIfDoesNotExist;

		private readonly ConcurrentLongHashMap<OpForAckCallBack> _ackRequests;

		private readonly AtomicReference<ClientCnx> _clientCnxUsedForConsumerRegistration = new AtomicReference<ClientCnx>();

		public static Props NewConsumer(IActorRef client, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist)
        {
			return Props.Create(() => new ConsumerActor<T>(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, schema, interceptors, createTopicIfDoesNotExist, 0));
        }
		
		public static Props NewConsumer(IActorRef client, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, long startMessageRollbackDurationInSec)
		{
			if(conf.ReceiverQueueSize == 0)
			{
				return ZeroQueueConsumer<T>.NewZeroQueueConsumer(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, schema, interceptors, createTopicIfDoesNotExist);
			}
			else
			{
				return Props.Create(() => new ConsumerActor<T>(client, topic, conf, listenerExecutor, partitionIndex, hasParentConsumer, startMessageId, startMessageRollbackDurationInSec, schema, interceptors, createTopicIfDoesNotExist);
			}
		}

		protected internal ConsumerActor(IActorRef client, string topic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, long startMessageRollbackDurationInSec, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist) : base(client, topic, conf, conf.ReceiverQueueSize, listenerExecutor, schema, interceptors)
		{
			ConsumerId = client.NewConsumerId();
			_subscriptionMode = conf.SubscriptionMode;
			_startMessageId = startMessageId != null ? new BatchMessageId((MessageId) startMessageId) : null;
			_initialStartMessageId = _startMessageId;
			_startMessageRollbackDurationInSec = startMessageRollbackDurationInSec;
			_availablePermitsUpdater.set(this, 0);
			_subscribeTimeout = DateTimeHelper.CurrentUnixTimeMillis() + client.Configuration.OperationTimeoutMs;
			_partitionIndex = partitionIndex;
			_hasParentConsumer = hasParentConsumer;
			_receiverQueueRefillThreshold = conf.ReceiverQueueSize / 2;
			_priorityLevel = conf.PriorityLevel;
			_readCompacted = conf.ReadCompacted;
			_subscriptionInitialPosition = conf.SubscriptionInitialPosition;
			_negativeAcksTracker = new NegativeAcksTracker<T>(conf, Self);
			_resetIncludeHead = conf.ResetIncludeHead;
			_createTopicIfDoesNotExist = createTopicIfDoesNotExist;
			_maxPendingChuckedMessage = conf.MaxPendingChuckedMessage;
			_pendingChunckedMessageUuidQueue = new GrowableArrayBlockingQueue<string>();
			ExpireTimeOfIncompleteChunkedMessageMillis = conf.ExpireTimeOfIncompleteChunkedMessageMillis;
			_autoAckOldestChunkedMessageOnQueueFull = conf.AutoAckOldestChunkedMessageOnQueueFull;

			if(client.Configuration.StatsIntervalSeconds > 0)
			{
				StatsConflict = new ConsumerStatsRecorderImpl(client, conf, this);
			}
			else
			{
				StatsConflict = ConsumerStatsDisabled.INSTANCE;
			}

			_duringSeek = new AtomicBoolean(false);

			if(conf.AckTimeoutMillis != 0)
			{
				if(conf.TickDurationMillis > 0)
				{
					_unAckedMessageTracker = new UnAckedMessageTracker(client, this, conf.AckTimeoutMillis, Math.Min(conf.TickDurationMillis, conf.AckTimeoutMillis));
				}
				else
				{
					_unAckedMessageTracker = new UnAckedMessageTracker(client, this, conf.AckTimeoutMillis);
				}
			}
			else
			{
				_unAckedMessageTracker = UnAckedMessageTracker.UNACKED_MESSAGE_TRACKER_DISABLED;
			}

			// Create msgCrypto if not created already
			if(conf.CryptoKeyReader != null)
			{
				if(conf.MessageCrypto != null)
				{
					_msgCrypto = conf.MessageCrypto;
				}
				else
				{
					// default to use MessageCryptoBc;
					IMessageCrypto msgCryptoBc;
					try
					{
						msgCryptoBc = new MessageCrypto(string.Format("[{0}] [{1}]", topic, SubscriptionConflict), false);
					}
					catch(Exception e)
					{
						_log.error("MessageCryptoBc may not included in the jar. e:", e);
						msgCryptoBc = null;
					}
					_msgCrypto = msgCryptoBc;
				}
			}
			else
			{
				_msgCrypto = null;
			}

			if(conf.Properties.Empty)
			{
				_metadata = Collections.emptyMap();
			}
			else
			{
				_metadata = Collections.unmodifiableMap(new Dictionary<>(conf.Properties));
			}

			_connectionHandler = new ConnectionHandler(this, (new BackoffBuilder()).SetInitialTime(client.Configuration.InitialBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMax(client.Configuration.MaxBackoffIntervalNanos, TimeUnit.NANOSECONDS).setMandatoryStop(0, TimeUnit.MILLISECONDS).create(), this);

			_topicName = TopicName.Get(topic);
			if(_topicName.Persistent)
			{
				_acknowledgmentsGroupingTracker = new PersistentAcknowledgmentsGroupingTracker(this, conf, client.EventLoopGroup());
			}
			else
			{
				_acknowledgmentsGroupingTracker = NonPersistentAcknowledgmentGroupingTracker.Of();
			}

			if(conf.DeadLetterPolicy != null)
			{
				_possibleSendToDeadLetterTopicMessages = new ConcurrentDictionary<MessageId, IList<Message<T>>>();
				if(StringUtils.isNotBlank(conf.DeadLetterPolicy.DeadLetterTopic))
				{
					_deadLetterPolicy = DeadLetterPolicy.builder().maxRedeliverCount(conf.DeadLetterPolicy.MaxRedeliverCount).deadLetterTopic(conf.DeadLetterPolicy.DeadLetterTopic).build();
				}
				else
				{
					_deadLetterPolicy = DeadLetterPolicy.builder().maxRedeliverCount(conf.DeadLetterPolicy.MaxRedeliverCount).deadLetterTopic(string.Format("{0}-{1}" + RetryMessageUtil.DlqGroupTopicSuffix, topic, SubscriptionConflict)).build();
				}

				if(StringUtils.isNotBlank(conf.DeadLetterPolicy.RetryLetterTopic))
				{
					_deadLetterPolicy.RetryLetterTopic = conf.DeadLetterPolicy.RetryLetterTopic;
				}
				else
				{
					_deadLetterPolicy.RetryLetterTopic = string.Format("{0}-{1}" + RetryMessageUtil.RetryGroupTopicSuffix, topic, SubscriptionConflict);
				}

			}
			else
			{
				_deadLetterPolicy = null;
				_possibleSendToDeadLetterTopicMessages = null;
			}

			_topicNameWithoutPartition = _topicName.PartitionedTopicName;
			_ackRequests = new ConcurrentLongHashMap<OpForAckCallBack>(16, 1);

			GrabCnx();
		}

		internal virtual ConnectionHandler ConnectionHandler
		{
			get
			{
				return _connectionHandler;
			}
		}

		internal virtual UnAckedMessageTracker UnAckedMessageTracker
		{
			get
			{
				return _unAckedMessageTracker;
			}
		}

		internal override void UnsubscribeAsync()
		{
			if(State == State.Closing || State == State.Closed)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Consumer was already closed"));
			}
			CompletableFuture<Void> unsubscribeFuture = new CompletableFuture<Void>();
			if(Connected)
			{
				State = State.Closing;
				long requestId = ClientConflict.NewRequestId();
				ByteBuf unsubscribe = Commands.NewUnsubscribe(ConsumerId, requestId);
				ClientCnx cnx = Cnx();
				cnx.SendRequestWithId(unsubscribe, requestId).thenRun(() =>
				{
				CloseConsumerTasks();
				DeregisterFromClientCnx();
				ClientConflict.CleanupConsumer(this);
				_log.info("[{}][{}] Successfully unsubscribed from topic", Topic, SubscriptionConflict);
				State = State.Closed;
				unsubscribeFuture.complete(null);
				}).exceptionally(e =>
				{
				_log.error("[{}][{}] Failed to unsubscribe: {}", Topic, SubscriptionConflict, e.Cause.Message);
				State = State.Ready;
				unsubscribeFuture.completeExceptionally(PulsarClientException.Wrap(e.Cause, string.Format("Failed to unsubscribe the subscription {0} of topic {1}", _topicName.ToString(), SubscriptionConflict)));
				return null;
			});
			}
			else
			{
				unsubscribeFuture.completeExceptionally(new PulsarClientException(string.Format("The client is not connected to the broker when unsubscribing the " + "subscription {0} of the topic {1}", SubscriptionConflict, _topicName.ToString())));
			}
			return unsubscribeFuture;
		}

		protected internal override IMessage<T> InternalReceive()
		{
			IMessage<T> message;
			try
			{
				message = IncomingMessages.take();
				MessageProcessed(message);
				return BeforeConsume(message);
			}
			catch(InterruptedException e)
			{
				StatsConflict.IncrementNumReceiveFailed();
				throw PulsarClientException.Unwrap(e);
			}
		}

		protected internal override Task<IMessage<T>> InternalReceiveAsync()
		{
			CompletableFutureCancellationHandler cancellationHandler = new CompletableFutureCancellationHandler();
			CompletableFuture<Message<T>> result = cancellationHandler.CreateFuture();
			Message<T> message = null;
			try
			{
				@lock.writeLock().@lock();
				message = IncomingMessages.poll(0, TimeUnit.MILLISECONDS);
				if(message == null)
				{
					PendingReceives.add(result);
					cancellationHandler.CancelAction = () => PendingReceives.remove(result);
				}
			}
			catch(InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				result.completeExceptionally(e);
			}
			finally
			{
				@lock.writeLock().unlock();
			}

			if(message != null)
			{
				MessageProcessed(message);
				result.complete(BeforeConsume(message));
			}

			return result;
		}
		protected internal override IMessage<T> InternalReceive(int timeout, TimeUnit unit)
		{
			Message<T> message;
			try
			{
				message = IncomingMessages.poll(timeout, unit);
				if(message == null)
				{
					return null;
				}
				MessageProcessed(message);
				return BeforeConsume(message);
			}
			catch(InterruptedException e)
			{
				State state = State;
				if(state != State.Closing && state != State.Closed)
				{
					StatsConflict.IncrementNumReceiveFailed();
					throw PulsarClientException.Unwrap(e);
				}
				else
				{
					return null;
				}
			}
		}

		protected internal override IMessages<T> InternalBatchReceive()
		{
			try
			{
				return InternalBatchReceiveAsync().get();
			}
			catch(Exception e) when (e is InterruptedException || e is ExecutionException)
			{
				State state = State;
				if(state != State.Closing && state != State.Closed)
				{
					StatsConflict.IncrementNumBatchReceiveFailed();
					throw PulsarClientException.Unwrap(e);
				}
				else
				{
					return null;
				}
			}
		}

		protected internal override IMessages<T> InternalBatchReceiveAsync()
		{
			CompletableFutureCancellationHandler cancellationHandler = new CompletableFutureCancellationHandler();
			CompletableFuture<IMessages<T>> result = cancellationHandler.CreateFuture();
			try
			{
				@lock.writeLock().@lock();
				if(PendingBatchReceives == null)
				{
					PendingBatchReceives = Queues.newConcurrentLinkedQueue();
				}
				if(HasEnoughMessagesForBatchReceive())
				{
					MessagesImpl<T> messages = NewMessagesImpl;
					Message<T> msgPeeked = IncomingMessages.peek();
					while(msgPeeked != null && messages.CanAdd(msgPeeked))
					{
						Message<T> msg = IncomingMessages.poll();
						if(msg != null)
						{
							MessageProcessed(msg);
							Message<T> interceptMsg = BeforeConsume(msg);
							messages.Add(interceptMsg);
						}
						msgPeeked = IncomingMessages.peek();
					}
					result.complete(messages);
				}
				else
				{
					OpBatchReceive<T> opBatchReceive = OpBatchReceive.Of(result);
					PendingBatchReceives.add(opBatchReceive);
					cancellationHandler.CancelAction = () => PendingBatchReceives.remove(opBatchReceive);
				}
			}
			finally
			{
				@lock.writeLock().unlock();
			}
			return result;
		}

		internal virtual bool MarkAckForBatchMessage(BatchMessageId batchMessageId, CommandAck.AckType ackType, IDictionary<string, long> properties, TransactionImpl txn)
		{
			bool isAllMsgsAcked;
			if(ackType == CommandAck.AckType.Individual)
			{
				isAllMsgsAcked = txn == null && batchMessageId.AckIndividual();
			}
			else
			{
				isAllMsgsAcked = batchMessageId.AckCumulative();
			}
			int outstandingAcks = 0;
			if(_log.DebugEnabled)
			{
				outstandingAcks = batchMessageId.OutstandingAcksInSameBatch;
			}

			int batchSize = batchMessageId.BatchSize;
			// all messages in this batch have been acked
			if(isAllMsgsAcked)
			{
				if(_log.DebugEnabled)
				{
					_log.debug("[{}] [{}] can ack message to broker {}, acktype {}, cardinality {}, length {}", SubscriptionConflict, ConsumerNameConflict, batchMessageId, ackType, outstandingAcks, batchSize);
				}
				return true;
			}
			else
			{
				if(CommandAck.AckType.Cumulative == ackType && !batchMessageId.Acker.PrevBatchCumulativelyAcked)
				{
					SendAcknowledge(batchMessageId.PrevBatchMessageId(), CommandAck.AckType.Cumulative, properties, null);
					batchMessageId.Acker.PrevBatchCumulativelyAcked = true;
				}
				else
				{
					OnAcknowledge(batchMessageId, null);
				}
				if(_log.DebugEnabled)
				{
					_log.debug("[{}] [{}] cannot ack message to broker {}, acktype {}, pending acks - {}", SubscriptionConflict, ConsumerNameConflict, batchMessageId, ackType, outstandingAcks);
				}
			}
			return false;
		}

		protected internal override Task DoAcknowledge(IMessageId messageId, CommandAck.AckType ackType, IDictionary<string, long> properties, TransactionImpl txn)
		{
			checkArgument(messageId is MessageIdImpl);
			if(State != State.Ready && State != State.Connecting)
			{
				StatsConflict.IncrementNumAcksFailed();
				PulsarClientException exception = new PulsarClientException("Consumer not ready. State: " + State);
				if(CommandAck.AckType.Individual.Equals(ackType))
				{
					OnAcknowledge(messageId, exception);
				}
				else if(CommandAck.AckType.Cumulative.Equals(ackType))
				{
					OnAcknowledgeCumulative(messageId, exception);
				}
				return FutureUtil.FailedFuture(exception);
			}

			if(txn != null)
			{
				return DoTransactionAcknowledgeForResponse(messageId, ackType, null, properties, new TxnID(txn.TxnIdMostBits, txn.TxnIdLeastBits));
			}

			if(messageId is BatchMessageId)
			{
				BatchMessageIdImpl batchMessageId = (BatchMessageIdImpl) messageId;
				if(ackType == CommandAck.AckType.Cumulative && txn != null)
				{
					return SendAcknowledge(messageId, ackType, properties, txn);
				}
				if(MarkAckForBatchMessage(batchMessageId, ackType, properties, txn))
				{
					// all messages in batch have been acked so broker can be acked via sendAcknowledge()
					if(_log.DebugEnabled)
					{
						_log.debug("[{}] [{}] acknowledging message - {}, acktype {}", SubscriptionConflict, ConsumerNameConflict, messageId, ackType);
					}
				}
				else
				{
					if(Conf.BatchIndexAckEnabled)
					{
						_acknowledgmentsGroupingTracker.AddBatchIndexAcknowledgment(batchMessageId, batchMessageId.BatchIndex, batchMessageId.BatchSize, ackType, properties, txn);
					}
					// other messages in batch are still pending ack.
					return CompletableFuture.completedFuture(null);
				}
			}
			return SendAcknowledge(messageId, ackType, properties, txn);
		}

		protected internal override void DoAcknowledge(IList<IMessageId> messageIdList, CommandAck.AckType ackType, IDictionary<string, long> properties, TransactionImpl txn)
		{
			if(CommandAck.AckType.Cumulative.Equals(ackType))
			{
				IList<CompletableFuture<Void>> completableFutures = new List<CompletableFuture<Void>>();
				messageIdList.ForEach(messageId => completableFutures.Add(doAcknowledge(messageId, ackType, properties, txn)));
				return CompletableFuture.allOf(((List<CompletableFuture<Void>>)completableFutures).ToArray());
			}
			if(State != State.Ready && State != State.Connecting)
			{
				StatsConflict.IncrementNumAcksFailed();
				PulsarClientException exception = new PulsarClientException("Consumer not ready. State: " + State);
				messageIdList.ForEach(messageId => OnAcknowledge(messageId, exception));
				return FutureUtil.FailedFuture(exception);
			}
			IList<MessageId> nonBatchMessageIds = new List<MessageId>();
			foreach(MessageId messageId in messageIdList)
			{
				MessageId messageIdImpl;
				if(messageId is BatchMessageId && !MarkAckForBatchMessage((BatchMessageId) messageId, ackType, properties, txn))
				{
					BatchMessageId batchMessageId = (BatchMessageId) messageId;
					messageIdImpl = new MessageId(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex);
					_acknowledgmentsGroupingTracker.AddBatchIndexAcknowledgment(batchMessageId, batchMessageId.BatchIndex, batchMessageId.BatchSize, ackType, properties, txn);
					StatsConflict.IncrementNumAcksSent(batchMessageId.BatchSize);
				}
				else
				{
					messageIdImpl = (MessageId) messageId;
					StatsConflict.IncrementNumAcksSent(1);
					nonBatchMessageIds.Add(messageIdImpl);
				}
				_unAckedMessageTracker.Remove(messageIdImpl);
				if(_possibleSendToDeadLetterTopicMessages != null)
				{
					_possibleSendToDeadLetterTopicMessages.Remove(messageIdImpl);
				}
				OnAcknowledge(messageId, null);
			}
			if(nonBatchMessageIds.Count > 0)
			{
				_acknowledgmentsGroupingTracker.AddListAcknowledgment(nonBatchMessageIds, ackType, properties);
			}
			return CompletableFuture.completedFuture(null);
		}

		protected internal override Task DoReconsumeLater<T1>(IMessage<T1> message, CommandAck.AckType ackType, IDictionary<string, long> properties, long delayTime, TimeUnit unit)
		{
			IMessageId messageId = message.MessageId;
			if(messageId is TopicMessageId)
			{
				messageId = ((TopicMessageId)messageId).InnerMessageId;
			}
			checkArgument(messageId is MessageId);
			if(State != State.Ready && State != State.Connecting)
			{
				StatsConflict.IncrementNumAcksFailed();
				PulsarClientException exception = new PulsarClientException("Consumer not ready. State: " + State);
				if(CommandAck.AckType.Individual.Equals(ackType))
				{
					OnAcknowledge(messageId, exception);
				}
				else if(CommandAck.AckType.Cumulative.Equals(ackType))
				{
					OnAcknowledgeCumulative(messageId, exception);
				}
				return FutureUtil.FailedFuture(exception);
			}
			if(delayTime < 0)
			{
				delayTime = 0;
			}
			if(_retryLetterProducer == null)
			{
				try
				{
					_createProducerLock.writeLock().@lock();
					if(_retryLetterProducer == null)
					{
						_retryLetterProducer = ClientConflict.NewProducer(Schema).Topic(_deadLetterPolicy.RetryLetterTopic).EnableBatching(false).blockIfQueueFull(false).create();
					}
				}
				catch(Exception e)
				{
					_log.error("Create retry letter producer exception with topic: {}", _deadLetterPolicy.RetryLetterTopic, e);
				}
				finally
				{
					_createProducerLock.writeLock().unlock();
				}
			}
			if(_retryLetterProducer != null)
			{
				try
				{
					MessageImpl<T> retryMessage = null;
					string originMessageIdStr = null;
					string originTopicNameStr = null;
					if(message is TopicMessageImpl)
					{
						retryMessage = (MessageImpl<T>)((TopicMessageImpl<T>) message).Message;
						originMessageIdStr = ((TopicMessageIdImpl) message.MessageId).InnerMessageId.ToString();
						originTopicNameStr = ((TopicMessageIdImpl) message.MessageId).TopicName;
					}
					else if(message is MessageImpl)
					{
						retryMessage = (MessageImpl<T>) message;
						originMessageIdStr = ((MessageImpl<T>) message).getMessageId().ToString();
						originTopicNameStr = ((MessageImpl<T>) message).TopicName;
					}
					SortedDictionary<string, string> propertiesMap = new SortedDictionary<string, string>();
					int reconsumetimes = 1;
					if(message.Properties != null)
					{
						propertiesMap.PutAll(message.Properties);
					}

					if(propertiesMap.ContainsKey(RetryMessageUtil.SystemPropertyReconsumetimes))
					{
						reconsumetimes = Convert.ToInt32(propertiesMap.GetValueOrNull(RetryMessageUtil.SystemPropertyReconsumetimes));
						reconsumetimes = reconsumetimes + 1;

					}
					else
					{
						propertiesMap[RetryMessageUtil.SystemPropertyRealTopic] = originTopicNameStr;
						propertiesMap[RetryMessageUtil.SystemPropertyOriginMessageId] = originMessageIdStr;
					}

					propertiesMap[RetryMessageUtil.SystemPropertyReconsumetimes] = reconsumetimes.ToString();
					propertiesMap[RetryMessageUtil.SystemPropertyDelayTime] = unit.toMillis(delayTime).ToString();

				   if(reconsumetimes > _deadLetterPolicy.MaxRedeliverCount)
				   {
					   ProcessPossibleToDLQ((MessageIdImpl)messageId);
						if(_deadLetterProducer == null)
						{
							try
							{
								if(_deadLetterProducer == null)
								{
									_createProducerLock.writeLock().@lock();
									_deadLetterProducer = ClientConflict.NewProducer(Schema).Topic(_deadLetterPolicy.DeadLetterTopic).BlockIfQueueFull(false).create();
								}
							}
							catch(Exception e)
							{
							   _log.error("Create dead letter producer exception with topic: {}", _deadLetterPolicy.DeadLetterTopic, e);
							}
						   finally
						   {
							   _createProducerLock.writeLock().unlock();
						   }
						}
					   if(_deadLetterProducer != null)
					   {
						   propertiesMap[RetryMessageUtil.SystemPropertyRealTopic] = originTopicNameStr;
						   propertiesMap[RetryMessageUtil.SystemPropertyOriginMessageId] = originMessageIdStr;
						   TypedMessageBuilder<T> typedMessageBuilderNew = _deadLetterProducer.NewMessage().Value(retryMessage.Value).Properties(propertiesMap);
						   typedMessageBuilderNew.Send();
						   return DoAcknowledge(messageId, ackType, properties, null);
					   }
				   }
					else
					{
						TypedMessageBuilder<T> typedMessageBuilderNew = _retryLetterProducer.NewMessage().Value(retryMessage.Value).Properties(propertiesMap);
						if(delayTime > 0)
						{
							typedMessageBuilderNew.DeliverAfter(delayTime, unit);
						}
						if(message.HasKey())
						{
							typedMessageBuilderNew.Key(message.Key);
						}
						typedMessageBuilderNew.Send();
						return DoAcknowledge(messageId, ackType, properties, null);
					}
				}
				catch(Exception e)
				{
					_log.error("Send to retry letter topic exception with topic: {}, messageId: {}", _deadLetterProducer.Topic, messageId, e);
					ISet<MessageId> messageIds = new HashSet<MessageId>();
					messageIds.Add(messageId);
					_unAckedMessageTracker.Remove(messageId);
					RedeliverUnacknowledgedMessages(messageIds);
				}
			}
			return CompletableFuture.completedFuture(null);
		}

		// TODO: handle transactional acknowledgements.
		private void SendAcknowledge(MessageId messageId, CommandAck.AckType ackType, IDictionary<string, long> properties, TransactionImpl txnImpl)
		{
			MessageIdImpl msgId = (MessageIdImpl) messageId;

			if(ackType == CommandAck.AckType.Individual)
			{
				if(messageId is BatchMessageIdImpl)
				{
					BatchMessageIdImpl batchMessageId = (BatchMessageIdImpl) messageId;

					StatsConflict.IncrementNumAcksSent(batchMessageId.BatchSize);
					_unAckedMessageTracker.Remove(new MessageIdImpl(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex));
					if(_possibleSendToDeadLetterTopicMessages != null)
					{
						_possibleSendToDeadLetterTopicMessages.Remove(new MessageIdImpl(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex));
					}
				}
				else
				{
					// increment counter by 1 for non-batch msg
					_unAckedMessageTracker.Remove(msgId);
					if(_possibleSendToDeadLetterTopicMessages != null)
					{
						_possibleSendToDeadLetterTopicMessages.Remove(msgId);
					}
					StatsConflict.IncrementNumAcksSent(1);
				}
				OnAcknowledge(messageId, null);
			}
			else if(ackType == CommandAck.AckType.Cumulative)
			{
				OnAcknowledgeCumulative(messageId, null);
				StatsConflict.IncrementNumAcksSent(_unAckedMessageTracker.RemoveMessagesTill(msgId));
			}

			_acknowledgmentsGroupingTracker.AddAcknowledgment(msgId, ackType, properties, txnImpl);

			// Consumer acknowledgment operation immediately succeeds. In any case, if we're not able to send ack to broker,
			// the messages will be re-delivered
			return CompletableFuture.completedFuture(null);
		}

		internal override void NegativeAcknowledge(MessageId messageId)
		{
			_negativeAcksTracker.Add(messageId);

			// Ensure the message is not redelivered for ack-timeout, since we did receive an "ack"
			_unAckedMessageTracker.Remove(messageId);
		}

		internal virtual void ConnectionOpened(in ClientCnx cnx)
		{
			if(State == State.Closing || State == State.Closed)
			{
				State = State.Closed;
				CloseConsumerTasks();
				DeregisterFromClientCnx();
				ClientConflict.CleanupConsumer(this);
				FailPendingReceive();
				ClearReceiverQueue();
				return;
			}
			ClientCnx = cnx;

			_log.info("[{}][{}] Subscribing to topic on cnx {}, consumerId {}", Topic, SubscriptionConflict, cnx.Ctx().channel(),ConsumerId);

			long requestId = ClientConflict.NewRequestId();

			int currentSize;
			lock(this)
			{
				currentSize = IncomingMessages.size();
				_startMessageId = ClearReceiverQueue();
				if(_possibleSendToDeadLetterTopicMessages != null)
				{
					_possibleSendToDeadLetterTopicMessages.Clear();
				}
			}

			bool isDurable = _subscriptionMode == SubscriptionMode.Durable;
			MessageIdData startMessageIdData = null;
			if(isDurable)
			{
				// For regular durable subscriptions, the message id from where to restart will be determined by the broker.
				startMessageIdData = null;
			}
			else if(_startMessageId != null)
			{
				// For non-durable we are going to restart from the next entry
				MessageIdData.Builder builder = MessageIdData.NewBuilder();
				builder.LedgerId = _startMessageId.LedgerId;
				builder.EntryId = _startMessageId.EntryId;
				if(_startMessageId is BatchMessageIdImpl)
				{
					builder.BatchIndex = _startMessageId.BatchIndex;
				}

				startMessageIdData = builder.Build();
				builder.Recycle();
			}

			SchemaInfo si = Schema.SchemaInfo;
			if(si != null && (SchemaType.BYTES == si.Type || SchemaType.NONE == si.Type))
			{
				// don't set schema for Schema.BYTES
				si = null;
			}
			// startMessageRollbackDurationInSec should be consider only once when consumer connects to first time
			long startMessageRollbackDuration = (_startMessageRollbackDurationInSec > 0 && _startMessageId != null && _startMessageId.Equals(_initialStartMessageId)) ? _startMessageRollbackDurationInSec : 0;
			ByteBuf request = Commands.NewSubscribe(Topic, SubscriptionConflict, ConsumerId, requestId, SubType, _priorityLevel, ConsumerNameConflict, isDurable, startMessageIdData, _metadata, _readCompacted, Conf.ReplicateSubscriptionState, CommandSubscribe.InitialPosition.valueOf(_subscriptionInitialPosition.Value), startMessageRollbackDuration, si, _createTopicIfDoesNotExist, Conf.KeySharedPolicy);
			if(startMessageIdData != null)
			{
				startMessageIdData.Recycle();
			}

			cnx.SendRequestWithId(request, requestId).thenRun(() =>
			{
			lock(ConsumerActor.this)
			{
				if(ChangeToReadyState())
				{
					ConsumerIsReconnectedToBroker(cnx, currentSize);
				}
				else
				{
					State = State.Closed;
					DeregisterFromClientCnx();
					ClientConflict.CleanupConsumer(this);
					cnx.Channel().close();
					return;
				}
			}
			ResetBackoff();
			bool firstTimeConnect = SubscribeFutureConflict.complete(this);
			if(!(firstTimeConnect && _hasParentConsumer && isDurable) && Conf.ReceiverQueueSize != 0)
			{
				IncreaseAvailablePermits(cnx, Conf.ReceiverQueueSize);
			}
			}).ConsumerActor((e) =>
			{
			DeregisterFromClientCnx();
			if(State == State.Closing || State == State.Closed)
			{
				cnx.Channel().close();
				return null;
			}
			_log.warn("[{}][{}] Failed to subscribe to topic on {}", Topic, SubscriptionConflict, cnx.Channel().remoteAddress());
			if(e.Cause is PulsarClientException && PulsarClientException.IsRetriableError(e.Cause) && DateTimeHelper.CurrentUnixTimeMillis() < _subscribeTimeout)
			{
				ReconnectLater(e.Cause);
			}
			else if(!SubscribeFutureConflict.Done)
			{
				State = State.Failed;
				CloseConsumerTasks();
				SubscribeFutureConflict.completeExceptionally(PulsarClientException.Wrap(e, string.Format("Failed to subscribe the topic {0} with subscription " + "name {1} when connecting to the broker", _topicName.ToString(), SubscriptionConflict)));
				ClientConflict.CleanupConsumer(this);
			}
			else if(e.Cause is PulsarClientException.TopicDoesNotExistException)
			{
				State = State.Failed;
				CloseConsumerTasks();
				ClientConflict.CleanupConsumer(this);
				_log.warn("[{}][{}] Closed consumer because topic does not exist anymore {}", Topic, SubscriptionConflict, cnx.Channel().remoteAddress());
			}
			else
			{
				ReconnectLater(e.Cause);
			}
			return null;
		});
		}

		protected internal virtual void ConsumerIsReconnectedToBroker(ClientCnx cnx, int currentQueueSize)
		{
			_log.info("[{}][{}] Subscribed to topic on {} -- consumer: {}", Topic, SubscriptionConflict, cnx.Channel().remoteAddress(), ConsumerId);

			_availablePermitsUpdater.set(this, 0);
		}

		/// <summary>
		/// Clear the internal receiver queue and returns the message id of what was the 1st message in the queue that was
		/// not seen by the application
		/// </summary>
		private BatchMessageIdImpl ClearReceiverQueue()
		{
			IList<Message<object>> currentMessageQueue = new List<Message<object>>(IncomingMessages.size());
			IncomingMessages.drainTo(currentMessageQueue);
			IncomingMessagesSizeUpdater.set(this, 0);

			if(_duringSeek.compareAndSet(true, false))
			{
				return _seekMessageId;
			}
			else if(_subscriptionMode == SubscriptionMode.Durable)
			{
				return _startMessageId;
			}

			if(currentMessageQueue.Count > 0)
			{
				MessageIdImpl nextMessageInQueue = (MessageIdImpl) currentMessageQueue[0].MessageId;
				BatchMessageIdImpl previousMessage;
				if(nextMessageInQueue is BatchMessageIdImpl)
				{
					// Get on the previous message within the current batch
					previousMessage = new BatchMessageIdImpl(nextMessageInQueue.LedgerId, nextMessageInQueue.EntryId, nextMessageInQueue.PartitionIndex, ((BatchMessageIdImpl) nextMessageInQueue).BatchIndex - 1);
				}
				else
				{
					// Get on previous message in previous entry
					previousMessage = new BatchMessageIdImpl(nextMessageInQueue.LedgerId, nextMessageInQueue.EntryId - 1, nextMessageInQueue.PartitionIndex, -1);
				}

				return previousMessage;
			}
			else if(!LastDequeuedMessageId.Equals(MessageId.earliest))
			{
				// If the queue was empty we need to restart from the message just after the last one that has been dequeued
				// in the past
				return new BatchMessageIdImpl((MessageIdImpl) LastDequeuedMessageId);
			}
			else
			{
				// No message was received or dequeued by this consumer. Next message would still be the startMessageId
				return _startMessageId;
			}
		}

		/// <summary>
		/// send the flow command to have the broker start pushing messages
		/// </summary>
		private void SendFlowPermitsToBroker(ClientCnx cnx, int numMessages)
		{
			if(cnx != null)
			{
				if(_log.DebugEnabled)
				{
					_log.debug("[{}] [{}] Adding {} additional permits", Topic, SubscriptionConflict, numMessages);
				}

				cnx.Ctx().writeAndFlush(Commands.NewFlow(ConsumerId, numMessages), cnx.Ctx().voidPromise());
			}
		}

		internal virtual void ConnectionFailed(PulsarClientException exception)
		{
			bool nonRetriableError = !PulsarClientException.IsRetriableError(exception);
			bool timeout = DateTimeHelper.CurrentUnixTimeMillis() > _subscribeTimeout;
			if((nonRetriableError || timeout) && SubscribeFutureConflict.completeExceptionally(exception))
			{
				State = State.Failed;
				if(nonRetriableError)
				{
					_log.info("[{}] Consumer creation failed for consumer {} with unretriableError {}", Topic, ConsumerId, exception);
				}
				else
				{
					_log.info("[{}] Consumer creation failed for consumer {} after timeout", Topic, ConsumerId);
				}
				CloseConsumerTasks();
				DeregisterFromClientCnx();
				ClientConflict.CleanupConsumer(this);
			}
		}

		internal override CompletableFuture<Void> CloseAsync()
		{
			if(State == State.Closing || State == State.Closed)
			{
				CloseConsumerTasks();
				return CompletableFuture.completedFuture(null);
			}

			if(!Connected)
			{
				_log.info("[{}] [{}] Closed Consumer (not connected)", Topic, SubscriptionConflict);
				State = State.Closed;
				CloseConsumerTasks();
				DeregisterFromClientCnx();
				ClientConflict.CleanupConsumer(this);
				return CompletableFuture.completedFuture(null);
			}

			StatsConflict.StatTimeout.ifPresent(Timeout.cancel);

			State = State.Closing;

			CloseConsumerTasks();

			long requestId = ClientConflict.NewRequestId();

			CompletableFuture<Void> closeFuture = new CompletableFuture<Void>();
			ClientCnx cnx = Cnx();
			if(null == cnx)
			{
				CleanupAtClose(closeFuture, null);
			}
			else
			{
				ByteBuf cmd = Commands.NewCloseConsumer(ConsumerId, requestId);
				cnx.SendRequestWithId(cmd, requestId).handle((v, exception) =>
				{
				bool ignoreException = !cnx.Ctx().channel().Active;
				if(ignoreException && exception != null)
				{
					_log.debug("Exception ignored in closing consumer", exception);
				}
				CleanupAtClose(closeFuture, ignoreException ? null : exception);
				return null;
				});
			}

			return closeFuture;
		}

		private void CleanupAtClose(CompletableFuture<Void> closeFuture, Exception exception)
		{
			_log.info("[{}] [{}] Closed consumer", Topic, SubscriptionConflict);
			State = State.Closed;
			CloseConsumerTasks();
			if(exception != null)
			{
				closeFuture.completeExceptionally(exception);
			}
			else
			{
				closeFuture.complete(null);
			}
			DeregisterFromClientCnx();
			ClientConflict.CleanupConsumer(this);
			// fail all pending-receive futures to notify application
			FailPendingReceive();
		}

		private void CloseConsumerTasks()
		{
			_unAckedMessageTracker.Dispose();
			if(_possibleSendToDeadLetterTopicMessages != null)
			{
				_possibleSendToDeadLetterTopicMessages.Clear();
			}

			if(!_ackRequests.Empty)
			{
				_ackRequests.ForEach((key, value) =>
				{
				value.callback.completeExceptionally(new PulsarClientException.MessageAcknowledgeException("Consumer has closed!"));
				value.recycle();
				});

				_ackRequests.Clear();
			}

			_acknowledgmentsGroupingTracker.close();
			if(BatchReceiveTimeout != null)
			{
				BatchReceiveTimeout.cancel();
			}
			StatsConflict.StatTimeout.ifPresent(Timeout.cancel);
		}

		private void FailPendingReceive()
		{
			@lock.readLock().@lock();
			try
			{
				if(ListenerExecutor != null && !ListenerExecutor.Shutdown)
				{
					FailPendingReceives(this.PendingReceives);
					FailPendingBatchReceives(this.PendingBatchReceives);
				}
			}
			finally
			{
				@lock.readLock().unlock();
			}
		}

		internal virtual void ActiveConsumerChanged(bool isActive)
		{
			if(ConsumerEventListener == null)
			{
				return;
			}

			ListenerExecutor.execute(() =>
			{
			if(isActive)
			{
				ConsumerEventListener.BecameActive(this, _partitionIndex);
			}
			else
			{
				ConsumerEventListener.BecameInactive(this, _partitionIndex);
			}
			});
		}

		internal virtual void MessageReceived(MessageIdData messageId, int redeliveryCount, IList<long> ackSet, byte[] headersAndPayload, ClientCnx cnx)
		{
			if(_log.DebugEnabled)
			{
				_log.debug("[{}][{}] Received message: {}/{}", Topic, SubscriptionConflict, messageId.LedgerId, messageId.EntryId);
			}

			if(!VerifyChecksum(headersAndPayload, messageId))
			{
				// discard message with checksum error
				DiscardCorruptedMessage(messageId, cnx, CommandAck.ValidationError.ChecksumMismatch);
				return;
			}

			MessageMetadata msgMetadata;
			try
			{
				msgMetadata = Commands.ParseMessageMetadata(headersAndPayload);
			}
			catch(Exception)
			{
				DiscardCorruptedMessage(messageId, cnx, CommandAck.ValidationError.ChecksumMismatch);
				return;
			}

			int numMessages = msgMetadata.NumMessagesInBatch;
		
			bool isChunkedMessage = msgMetadata.NumChunksFromMsg > 1 && Conf.SubscriptionType != SubscriptionType.Shared;

			MessageId msgId = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, PartitionIndex);
			if(_acknowledgmentsGroupingTracker.IsDuplicate(msgId))
			{
				if(_log.DebugEnabled)
				{
					_log.debug("[{}] [{}] Ignoring message as it was already being acked earlier by same consumer {}/{}", Topic, SubscriptionConflict, ConsumerNameConflict, msgId);
				}

				IncreaseAvailablePermits(cnx, numMessages);
				return;
			}

			byte[] decryptedPayload = DecryptPayloadIfNeeded(messageId, msgMetadata, headersAndPayload, cnx);

			bool isMessageUndecryptable = IsMessageUndecryptable(msgMetadata);

			if(decryptedPayload == null)
			{
				// Message was discarded or CryptoKeyReader isn't implemented
				return;
			}

			// uncompress decryptedPayload and release decryptedPayload-ByteBuf
			ByteBuf uncompressedPayload = (isMessageUndecryptable || isChunkedMessage) ? decryptedPayload.retain() : UncompressPayloadIfNeeded(messageId, msgMetadata, decryptedPayload, cnx, true);
			decryptedPayload.release();
			if(uncompressedPayload == null)
			{
				// Message was discarded on decompression error
				return;
			}

			// if message is not decryptable then it can't be parsed as a batch-message. so, add EncyrptionCtx to message
			// and return undecrypted payload
			if(isMessageUndecryptable || (numMessages == 1 && !msgMetadata.HasNumMessagesInBatch()))
			{

				// right now, chunked messages are only supported by non-shared subscription
				if(isChunkedMessage)
				{
					uncompressedPayload = ProcessMessageChunk(uncompressedPayload, msgMetadata, msgId, messageId, cnx);
					if(uncompressedPayload == null)
					{
						msgMetadata.Recycle();
						return;
					}
				}

				if(IsSameEntry(messageId) && IsPriorEntryIndex(messageId.EntryId))
				{
					// We need to discard entries that were prior to startMessageId
					if(_log.DebugEnabled)
					{
						_log.debug("[{}] [{}] Ignoring message from before the startMessageId: {}", SubscriptionConflict, ConsumerNameConflict, _startMessageId);
					}

					uncompressedPayload.release();
					msgMetadata.Recycle();
					return;
				}

				MessageImpl<T> message = new MessageImpl<T>(_topicName.ToString(), msgId, msgMetadata, uncompressedPayload, CreateEncryptionContext(msgMetadata), cnx, Schema, redeliveryCount);
				uncompressedPayload.release();
				msgMetadata.Recycle();

				@lock.readLock().@lock();
				try
				{
					// Enqueue the message so that it can be retrieved when application calls receive()
					// if the conf.getReceiverQueueSize() is 0 then discard message if no one is waiting for it.
					// if asyncReceive is waiting then notify callback without adding to incomingMessages queue
					if(_deadLetterPolicy != null && _possibleSendToDeadLetterTopicMessages != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
					{
						_possibleSendToDeadLetterTopicMessages[(MessageIdImpl)message.getMessageId()] = Collections.singletonList(message);
					}
					if(PeekPendingReceive() != null)
					{
						NotifyPendingReceivedCallback(message, null);
					}
					else if(EnqueueMessageAndCheckBatchReceive(message))
					{
						if(HasPendingBatchReceive())
						{
							NotifyPendingBatchReceivedCallBack();
						}
					}
				}
				finally
				{
					@lock.readLock().unlock();
				}
			}
			else
			{
				// handle batch message enqueuing; uncompressed payload has all messages in batch
				ReceiveIndividualMessagesFromBatch(msgMetadata, redeliveryCount, ackSet, uncompressedPayload, messageId, cnx);

				uncompressedPayload.release();
				msgMetadata.Recycle();
			}

			if(Listener != null)
			{
				TriggerListener(numMessages);
			}
		}

		private bool IsTxnMessage(MessageMetadata messageMetadata)
		{
			return messageMetadata.HasTxnidMostBits() && messageMetadata.HasTxnidLeastBits();
		}

		private ByteBuf ProcessMessageChunk(ByteBuf compressedPayload, MessageMetadata msgMetadata, MessageIdImpl msgId, MessageIdData messageId, ClientCnx cnx)
		{

			// Lazy task scheduling to expire incomplete chunk message
			if(!_expireChunkMessageTaskScheduled && ExpireTimeOfIncompleteChunkedMessageMillis > 0)
			{
				((ScheduledExecutorService) ListenerExecutor).scheduleAtFixedRate(() =>
				{
				RemoveExpireIncompleteChunkedMessages();
				}, ExpireTimeOfIncompleteChunkedMessageMillis, ExpireTimeOfIncompleteChunkedMessageMillis, TimeUnit.MILLISECONDS);
				_expireChunkMessageTaskScheduled = true;
			}

			if(msgMetadata.ChunkId == 0)
			{
				ByteBuf chunkedMsgBuffer = Unpooled.directBuffer(msgMetadata.TotalChunkMsgSize, msgMetadata.TotalChunkMsgSize);
				int totalChunks = msgMetadata.NumChunksFromMsg;
				ChunkedMessagesMap.ComputeIfAbsent(msgMetadata.Uuid, (key) => ChunkedMessageCtx.Get(totalChunks, chunkedMsgBuffer));
				_pendingChunckedMessageCount++;
				if(_maxPendingChuckedMessage > 0 && _pendingChunckedMessageCount > _maxPendingChuckedMessage)
				{
					RemoveOldestPendingChunkedMessage();
				}
				_pendingChunckedMessageUuidQueue.add(msgMetadata.Uuid);
			}

			ChunkedMessageCtx chunkedMsgCtx = ChunkedMessagesMap.Get(msgMetadata.Uuid);
			// discard message if chunk is out-of-order
			if(chunkedMsgCtx == null || chunkedMsgCtx.ChunkedMsgBuffer == null || msgMetadata.ChunkId != (chunkedMsgCtx.LastChunkedMessageId + 1) || msgMetadata.ChunkId >= msgMetadata.TotalChunkMsgSize)
			{
				// means we lost the first chunk: should never happen
				_log.info("Received unexpected chunk messageId {}, last-chunk-id{}, chunkId = {}, total-chunks {}", msgId, (chunkedMsgCtx != null ? chunkedMsgCtx.LastChunkedMessageId : null), msgMetadata.ChunkId, msgMetadata.TotalChunkMsgSize);
				if(chunkedMsgCtx != null)
				{
					if(chunkedMsgCtx.ChunkedMsgBuffer != null)
					{
						ReferenceCountUtil.safeRelease(chunkedMsgCtx.ChunkedMsgBuffer);
					}
					chunkedMsgCtx.Recycle();
				}
				ChunkedMessagesMap.Remove(msgMetadata.Uuid);
				compressedPayload.release();
				IncreaseAvailablePermits(cnx);
				if(ExpireTimeOfIncompleteChunkedMessageMillis > 0 && DateTimeHelper.CurrentUnixTimeMillis() > (msgMetadata.PublishTime + ExpireTimeOfIncompleteChunkedMessageMillis))
				{
					doAcknowledge(msgId, CommandAck.AckType.Individual, Collections.emptyMap(), null);
				}
				else
				{
					TrackMessage(msgId);
				}
				return null;
			}

			chunkedMsgCtx.ChunkedMessageIds[msgMetadata.ChunkId] = msgId;
			// append the chunked payload and update lastChunkedMessage-id
			chunkedMsgCtx.ChunkedMsgBuffer.writeBytes(compressedPayload);
			chunkedMsgCtx.LastChunkedMessageId = msgMetadata.ChunkId;

			// if final chunk is not received yet then release payload and return
			if(msgMetadata.ChunkId != (msgMetadata.NumChunksFromMsg - 1))
			{
				compressedPayload.release();
				IncreaseAvailablePermits(cnx);
				return null;
			}

			// last chunk received: so, stitch chunked-messages and clear up chunkedMsgBuffer
			if(_log.DebugEnabled)
			{
				_log.debug("Chunked message completed chunkId {}, total-chunks {}, msgId {} sequenceId {}", msgMetadata.ChunkId, msgMetadata.NumChunksFromMsg, msgId, msgMetadata.SequenceId);
			}
			// remove buffer from the map, add chucked messageId to unack-message tracker, and reduce pending-chunked-message count
			ChunkedMessagesMap.Remove(msgMetadata.Uuid);
			UnAckedChunckedMessageIdSequenceMap.Put(msgId, chunkedMsgCtx.ChunkedMessageIds);
			_pendingChunckedMessageCount--;
			compressedPayload.release();
			compressedPayload = chunkedMsgCtx.ChunkedMsgBuffer;
			chunkedMsgCtx.Recycle();
			ByteBuf uncompressedPayload = UncompressPayloadIfNeeded(messageId, msgMetadata, compressedPayload, cnx, false);
			compressedPayload.release();
			return uncompressedPayload;
		}

		protected internal virtual void TriggerListener(int numMessages)
		{
			// Trigger the notification on the message listener in a separate thread to avoid blocking the networking
			// thread while the message processing happens
			ListenerExecutor.execute(() =>
			{
			for(int i = 0; i < numMessages; i++)
			{
				try
				{
					Message<T> msg = InternalReceive(0, TimeUnit.MILLISECONDS);
					if(msg == null)
					{
						if(_log.DebugEnabled)
						{
							_log.debug("[{}] [{}] Message has been cleared from the queue", Topic, SubscriptionConflict);
						}
						break;
					}
					try
					{
						if(_log.DebugEnabled)
						{
							_log.debug("[{}][{}] Calling message listener for message {}", Topic, SubscriptionConflict, msg.MessageId);
						}
						Listener.Received(ConsumerActor.this, msg);
					}
					catch(Exception t)
					{
						_log.error("[{}][{}] Message listener error in processing message: {}", Topic, SubscriptionConflict, msg.MessageId, t);
					}
				}
				catch(PulsarClientException e)
				{
					_log.warn("[{}] [{}] Failed to dequeue the message for listener", Topic, SubscriptionConflict, e);
					return;
				}
			}
			});
		}

		/// <summary>
		/// Notify waiting asyncReceive request with the received message
		/// </summary>
		/// <param name="message"> </param>
		internal virtual void NotifyPendingReceivedCallback(in Message<T> message, Exception exception)
		{
			if(PendingReceives.Empty)
			{
				return;
			}

			// fetch receivedCallback from queue
			CompletableFuture<Message<T>> receivedFuture = PollPendingReceive();
			if(receivedFuture == null)
			{
				return;
			}

			if(exception != null)
			{
				ListenerExecutor.execute(() => receivedFuture.completeExceptionally(exception));
				return;
			}

			if(message == null)
			{
				System.InvalidOperationException e = new System.InvalidOperationException("received message can't be null");
				ListenerExecutor.execute(() => receivedFuture.completeExceptionally(e));
				return;
			}

			if(Conf.ReceiverQueueSize == 0)
			{
				// call interceptor and complete received callback
				TrackMessage(message);
				InterceptAndComplete(message, receivedFuture);
				return;
			}

			// increase permits for available message-queue
			MessageProcessed(message);
			// call interceptor and complete received callback
			InterceptAndComplete(message, receivedFuture);
		}

		private void InterceptAndComplete(in Message<T> message, in CompletableFuture<Message<T>> receivedFuture)
		{
			// call proper interceptor

			Message<T> interceptMessage = BeforeConsume(message);
			// return message to receivedCallback
			CompletePendingReceive(receivedFuture, interceptMessage);
		}

		internal virtual void ReceiveIndividualMessagesFromBatch(MessageMetadata msgMetadata, int redeliveryCount, IList<long> ackSet, ByteBuf uncompressedPayload, MessageIdData messageId, ClientCnx cnx)
		{
			int batchSize = msgMetadata.NumMessagesInBatch;

			// create ack tracker for entry aka batch
			MessageIdImpl batchMessage = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex);
			IList<MessageImpl<T>> possibleToDeadLetter = null;
			if(_deadLetterPolicy != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
			{
				possibleToDeadLetter = new List<MessageImpl<T>>();
			}

			BatchMessageAcker acker = BatchMessageAcker.NewAcker(batchSize);
			BitSetRecyclable ackBitSet = null;
			if(ackSet != null && ackSet.Count > 0)
			{
				ackBitSet = BitSetRecyclable.ValueOf(SafeCollectionUtils.LongListToArray(ackSet));
			}

			int skippedMessages = 0;
			try
			{
				for(int i = 0; i < batchSize; ++i)
				{
					if(_log.DebugEnabled)
					{
						_log.debug("[{}] [{}] processing message num - {} in batch", SubscriptionConflict, ConsumerNameConflict, i);
					}
					SingleMessageMetadata.Builder singleMessageMetadataBuilder = SingleMessageMetadata.NewBuilder();
					ByteBuf singleMessagePayload = Commands.DeSerializeSingleMessageInBatch(uncompressedPayload, singleMessageMetadataBuilder, i, batchSize);

					if(IsSameEntry(messageId) && IsPriorBatchIndex(i))
					{
						// If we are receiving a batch message, we need to discard messages that were prior
						// to the startMessageId
						if(_log.DebugEnabled)
						{
							_log.debug("[{}] [{}] Ignoring message from before the startMessageId: {}", SubscriptionConflict, ConsumerNameConflict, _startMessageId);
						}
						singleMessagePayload.release();
						singleMessageMetadataBuilder.Recycle();

						++skippedMessages;
						continue;
					}

					if(singleMessageMetadataBuilder.CompactedOut)
					{
						// message has been compacted out, so don't send to the user
						singleMessagePayload.release();
						singleMessageMetadataBuilder.Recycle();

						++skippedMessages;
						continue;
					}

					if(ackBitSet != null && !ackBitSet.Get(i))
					{
						singleMessagePayload.release();
						singleMessageMetadataBuilder.Recycle();
						++skippedMessages;
						continue;
					}

					BatchMessageIdImpl batchMessageIdImpl = new BatchMessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex, i, batchSize, acker);

					MessageImpl<T> message = new MessageImpl<T>(_topicName.ToString(), batchMessageIdImpl, msgMetadata, singleMessageMetadataBuilder.Build(), singleMessagePayload, CreateEncryptionContext(msgMetadata), cnx, Schema, redeliveryCount);
					if(possibleToDeadLetter != null)
					{
						possibleToDeadLetter.Add(message);
					}
					@lock.readLock().@lock();
					try
					{
						if(PeekPendingReceive() != null)
						{
							NotifyPendingReceivedCallback(message, null);
						}
						else if(EnqueueMessageAndCheckBatchReceive(message))
						{
							if(HasPendingBatchReceive())
							{
								NotifyPendingBatchReceivedCallBack();
							}
						}
					}
					finally
					{
						@lock.readLock().unlock();
					}
					singleMessagePayload.release();
					singleMessageMetadataBuilder.Recycle();
				}
				if(ackBitSet != null)
				{
					ackBitSet.Recycle();
				}
			}
			catch(IOException)
			{
				_log.warn("[{}] [{}] unable to obtain message in batch", SubscriptionConflict, ConsumerNameConflict);
				DiscardCorruptedMessage(messageId, cnx, CommandAck.ValidationError.BatchDeSerializeError);
			}

			if(possibleToDeadLetter != null && _possibleSendToDeadLetterTopicMessages != null)
			{
				_possibleSendToDeadLetterTopicMessages[batchMessage] = possibleToDeadLetter;
			}

			if(_log.DebugEnabled)
			{
				_log.debug("[{}] [{}] enqueued messages in batch. queue size - {}, available queue size - {}", SubscriptionConflict, ConsumerNameConflict, IncomingMessages.size(), IncomingMessages.remainingCapacity());
			}

			if(skippedMessages > 0)
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

		private bool IsSameEntry(MessageIdData messageId)
		{
			return _startMessageId != null && messageId.LedgerId == _startMessageId.LedgerId && messageId.EntryId == _startMessageId.EntryId;
		}

		/// <summary>
		/// Record the event that one message has been processed by the application.
		/// 
		/// Periodically, it sends a Flow command to notify the broker that it can push more messages
		/// </summary>
		protected internal override void MessageProcessed<T1>(Message<T1> msg)
		{
			lock(this)
			{
				ClientCnx currentCnx = Cnx();
				ClientCnx msgCnx = ((MessageImpl<object>) msg).Cnx;
				LastDequeuedMessageId = msg.MessageId;
        
				if(msgCnx != currentCnx)
				{
					// The processed message did belong to the old queue that was cleared after reconnection.
					return;
				}
        
				IncreaseAvailablePermits(currentCnx);
				StatsConflict.UpdateNumMsgsReceived(msg);
        
				TrackMessage(msg);
				IncomingMessagesSizeUpdater.addAndGet(this, msg.Data == null ? 0 : -msg.Data.Length);
			}
		}

		protected internal virtual void TrackMessage<T1>(Message<T1> msg)
		{
			if(msg != null)
			{
				TrackMessage(msg.MessageId);
			}
		}

		protected internal virtual void TrackMessage(MessageId messageId)
		{
			if(Conf.AckTimeoutMillis > 0 && messageId is MessageIdImpl)
			{
				MessageIdImpl id = (MessageIdImpl)messageId;
				if(id is BatchMessageIdImpl)
				{
					// do not add each item in batch message into tracker
					id = new MessageIdImpl(id.LedgerId, id.EntryId, PartitionIndex);
				}
				if(_hasParentConsumer)
				{
					//TODO: check parent consumer here
					// we should no longer track this message, TopicsConsumer will take care from now onwards
					_unAckedMessageTracker.Remove(id);
				}
				else
				{
					_unAckedMessageTracker.Add(id);
				}
			}
		}

		internal virtual void IncreaseAvailablePermits(ClientCnx currentCnx)
		{
			IncreaseAvailablePermits(currentCnx, 1);
		}

		protected internal virtual void IncreaseAvailablePermits(ClientCnx currentCnx, int delta)
		{
			int available = _availablePermitsUpdater.addAndGet(this, delta);

			while(available >= _receiverQueueRefillThreshold && !Paused)
			{
				if(_availablePermitsUpdater.compareAndSet(this, available, 0))
				{
					SendFlowPermitsToBroker(currentCnx, available);
					break;
				}
				else
				{
					available = _availablePermitsUpdater.get(this);
				}
			}
		}

		internal virtual void IncreaseAvailablePermits(int delta)
		{
			IncreaseAvailablePermits(Cnx(), delta);
		}

		internal override void Pause()
		{
			Paused = true;
		}

		internal override void Resume()
		{
			if(Paused)
			{
				Paused = false;
				IncreaseAvailablePermits(Cnx(), 0);
			}
		}

		internal override long LastDisconnectedTimestamp
		{
			get
			{
				return _connectionHandler.LastConnectionClosedTimestamp;
			}
		}

		private ByteBuf DecryptPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, ByteBuf payload, ClientCnx currentCnx)
		{

			if(msgMetadata.EncryptionKeysCount == 0)
			{
				return payload.retain();
			}

			// If KeyReader is not configured throw exception based on config param
			if(Conf.CryptoKeyReader == null)
			{
				switch(Conf.CryptoFailureAction)
				{
					case CONSUME:
						_log.warn("[{}][{}][{}] CryptoKeyReader interface is not implemented. Consuming encrypted message.", Topic, SubscriptionConflict, ConsumerNameConflict);
						return payload.retain();
					case DISCARD:
						_log.warn("[{}][{}][{}] Skipping decryption since CryptoKeyReader interface is not implemented and config is set to discard", Topic, SubscriptionConflict, ConsumerNameConflict);
						DiscardMessage(messageId, currentCnx, CommandAck.ValidationError.DecryptionError);
						return null;
					case FAIL:
						MessageId m = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, _partitionIndex);
						_log.error("[{}][{}][{}][{}] Message delivery failed since CryptoKeyReader interface is not implemented to consume encrypted message", Topic, SubscriptionConflict, ConsumerNameConflict, m);
						_unAckedMessageTracker.Add(m);
						return null;
				}
			}

			ByteBuf decryptedData = this._msgCrypto.decrypt(() => msgMetadata, payload, Conf.CryptoKeyReader);
			if(decryptedData != null)
			{
				return decryptedData;
			}

			switch(Conf.CryptoFailureAction)
			{
				case CONSUME:
					// Note, batch message will fail to consume even if config is set to consume
					_log.warn("[{}][{}][{}][{}] Decryption failed. Consuming encrypted message since config is set to consume.", Topic, SubscriptionConflict, ConsumerNameConflict, messageId);
					return payload.retain();
				case DISCARD:
					_log.warn("[{}][{}][{}][{}] Discarding message since decryption failed and config is set to discard", Topic, SubscriptionConflict, ConsumerNameConflict, messageId);
					DiscardMessage(messageId, currentCnx, CommandAck.ValidationError.DecryptionError);
					return null;
				case FAIL:
					MessageId m = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, _partitionIndex);
					_log.error("[{}][{}][{}][{}] Message delivery failed since unable to decrypt incoming message", Topic, SubscriptionConflict, ConsumerNameConflict, m);
					_unAckedMessageTracker.Add(m);
					return null;
			}
			return null;
		}

		private ByteBuf UncompressPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, ByteBuf payload, ClientCnx currentCnx, bool checkMaxMessageSize)
		{
			CompressionType compressionType = msgMetadata.Compression;
			CompressionCodec codec = CompressionCodecProvider.GetCompressionCodec(compressionType);
			int uncompressedSize = msgMetadata.UncompressedSize;
			int payloadSize = payload.readableBytes();
			if(checkMaxMessageSize && payloadSize > ClientCnx.MaxMessageSize)
			{
				// payload size is itself corrupted since it cannot be bigger than the MaxMessageSize
				_log.error("[{}][{}] Got corrupted payload message size {} at {}", Topic, SubscriptionConflict, payloadSize, messageId);
				DiscardCorruptedMessage(messageId, currentCnx, CommandAck.ValidationError.UncompressedSizeCorruption);
				return null;
			}
			try
			{
				ByteBuf uncompressedPayload = codec.Decode(payload, uncompressedSize);
				return uncompressedPayload;
			}
			catch(IOException e)
			{
				_log.error("[{}][{}] Failed to decompress message with {} at {}: {}", Topic, SubscriptionConflict, compressionType, messageId, e.Message, e);
				DiscardCorruptedMessage(messageId, currentCnx, CommandAck.ValidationError.DecompressionError);
				return null;
			}
		}

		private bool VerifyChecksum(ByteBuf headersAndPayload, MessageIdData messageId)
		{

			if(hasChecksum(headersAndPayload))
			{
				int checksum = readChecksum(headersAndPayload);
				int computedChecksum = computeChecksum(headersAndPayload);
				if(checksum != computedChecksum)
				{
					_log.error("[{}][{}] Checksum mismatch for message at {}:{}. Received checksum: 0x{}, Computed checksum: 0x{}", Topic, SubscriptionConflict, messageId.LedgerId, messageId.EntryId, checksum.ToString("x"), computedChecksum.ToString("x"));
					return false;
				}
			}

			return true;
		}

		private void DiscardCorruptedMessage(MessageIdData messageId, ClientCnx currentCnx, CommandAck.ValidationError validationError)
		{
			_log.error("[{}][{}] Discarding corrupted message at {}:{}", Topic, SubscriptionConflict, messageId.LedgerId, messageId.EntryId);
			DiscardMessage(messageId, currentCnx, validationError);
		}

		private void DiscardMessage(MessageIdData messageId, ClientCnx currentCnx, CommandAck.ValidationError validationError)
		{
			ByteBuf cmd = Commands.NewAck(ConsumerId, messageId.LedgerId, messageId.EntryId, null, CommandAck.AckType.Individual, validationError, Collections.emptyMap());
			currentCnx.Ctx().writeAndFlush(cmd, currentCnx.Ctx().voidPromise());
			IncreaseAvailablePermits(currentCnx);
			StatsConflict.IncrementNumReceiveFailed();
		}

		internal override string HandlerName
		{
			get
			{
				return SubscriptionConflict;
			}
		}

		internal override bool Connected
		{
			get
			{
				return ClientCnx != null && (State == State.Ready);
			}
		}

		internal virtual int PartitionIndex
		{
			get
			{
				return _partitionIndex;
			}
		}

		internal override int AvailablePermits
		{
			get
			{
				return _availablePermitsUpdater.get(this);
			}
		}

		internal override int NumMessagesInQueue()
		{
			return IncomingMessages.size();
		}

		internal override void RedeliverUnacknowledgedMessages()
		{
			ClientCnx cnx = Cnx();
			if(Connected && cnx.RemoteEndpointProtocolVersion >= ProtocolVersion.V2.Number)
			{
				int currentSize = 0;
				lock(this)
				{
					currentSize = IncomingMessages.size();
					IncomingMessages.clear();
					IncomingMessagesSizeUpdater.set(this, 0);
					_unAckedMessageTracker.Clear();
				}
				cnx.Ctx().writeAndFlush(Commands.NewRedeliverUnacknowledgedMessages(ConsumerId), cnx.Ctx().voidPromise());
				if(currentSize > 0)
				{
					IncreaseAvailablePermits(cnx, currentSize);
				}
				if(_log.DebugEnabled)
				{
					_log.debug("[{}] [{}] [{}] Redeliver unacked messages and send {} permits", SubscriptionConflict, Topic, ConsumerNameConflict, currentSize);
				}
				return;
			}
			if(cnx == null || (State == State.Connecting))
			{
				_log.warn("[{}] Client Connection needs to be established for redelivery of unacknowledged messages", this);
			}
			else
			{
				_log.warn("[{}] Reconnecting the client to redeliver the messages.", this);
				cnx.Ctx().close();
			}
		}

		internal virtual int ClearIncomingMessagesAndGetMessageNumber()
		{
			int messagesNumber = IncomingMessages.size();
			IncomingMessages.clear();
			IncomingMessagesSizeUpdater.set(this, 0);
			_unAckedMessageTracker.Clear();
			return messagesNumber;
		}

		internal override void RedeliverUnacknowledgedMessages(ISet<MessageId> messageIds)
		{
			if(messageIds.Count == 0)
			{
				return;
			}

			checkArgument(messageIds.First().get() is MessageIdImpl);

			if(Conf.SubscriptionType != SubscriptionType.Shared && Conf.SubscriptionType != SubscriptionType.KeyShared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				RedeliverUnacknowledgedMessages();
				return;
			}
			ClientCnx cnx = Cnx();
			if(Connected && cnx.RemoteEndpointProtocolVersion >= ProtocolVersion.V2.Number)
			{
				int messagesFromQueue = RemoveExpiredMessagesFromQueue(messageIds);

				IEnumerable<IList<MessageIdImpl>> batches = Iterables.partition(messageIds.Select(messageId => (MessageIdImpl)messageId).collect(Collectors.toSet()), MaxRedeliverUnacknowledged);
				MessageIdData.Builder builder = MessageIdData.NewBuilder();
				batches.forEach(ids =>
				{
				IList<MessageIdData> messageIdDatas = ids.Where(messageId => !ProcessPossibleToDLQ(messageId)).Select(messageId =>
				{
					builder.Partition = messageId.PartitionIndex;
					builder.LedgerId = messageId.LedgerId;
					builder.EntryId = messageId.EntryId;
					return builder.Build();
				}).ToList();
				if(messageIdDatas.Count > 0)
				{
					ByteBuf cmd = Commands.NewRedeliverUnacknowledgedMessages(ConsumerId, messageIdDatas);
					cnx.Ctx().writeAndFlush(cmd, cnx.Ctx().voidPromise());
					messageIdDatas.ForEach(MessageIdData.recycle);
				}
				});
				if(messagesFromQueue > 0)
				{
					IncreaseAvailablePermits(cnx, messagesFromQueue);
				}
				builder.Recycle();
				if(_log.DebugEnabled)
				{
					_log.debug("[{}] [{}] [{}] Redeliver unacked messages and increase {} permits", SubscriptionConflict, Topic, ConsumerNameConflict, messagesFromQueue);
				}
				return;
			}
			if(cnx == null || (State == State.Connecting))
			{
				_log.warn("[{}] Client Connection needs to be established for redelivery of unacknowledged messages", this);
			}
			else
			{
				_log.warn("[{}] Reconnecting the client to redeliver the messages.", this);
				cnx.Ctx().close();
			}
		}

		protected internal override void CompleteOpBatchReceive(OpBatchReceive<T> op)
		{
			NotifyPendingBatchReceivedCallBack(op);
		}

		private bool ProcessPossibleToDLQ(MessageIdImpl messageId)
		{
			IList<MessageImpl<T>> deadLetterMessages = null;
			if(_possibleSendToDeadLetterTopicMessages != null)
			{
				if(messageId is BatchMessageIdImpl)
				{
					deadLetterMessages = _possibleSendToDeadLetterTopicMessages.GetValueOrNull(new MessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex));
				}
				else
				{
					deadLetterMessages = _possibleSendToDeadLetterTopicMessages.GetValueOrNull(messageId);
				}
			}
			if(deadLetterMessages != null)
			{
				if(_deadLetterProducer == null)
				{
					try
					{
						_deadLetterProducer = ClientConflict.NewProducer(Schema).Topic(this._deadLetterPolicy.DeadLetterTopic).BlockIfQueueFull(false).create();
					}
					catch(Exception e)
					{
						_log.error("Create dead letter producer exception with topic: {}", _deadLetterPolicy.DeadLetterTopic, e);
					}
				}
				if(_deadLetterProducer != null)
				{
					try
					{
						foreach(MessageImpl<T> message in deadLetterMessages)
						{
							_deadLetterProducer.NewMessage().Value(message.Value).Properties(message.Properties).send();
						}
						Acknowledge(messageId);
						return true;
					}
					catch(Exception e)
					{
						_log.error("Send to dead letter topic exception with topic: {}, messageId: {}", _deadLetterProducer.Topic, messageId, e);
					}
				}
			}
			return false;
		}

		internal override void Seek(MessageId messageId)
		{
			try
			{
				SeekAsync(messageId).get();
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		internal override void Seek(long timestamp)
		{
			try
			{
				SeekAsync(timestamp).get();
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		internal override CompletableFuture<Void> SeekAsync(long timestamp)
		{
			if(State == State.Closing || State == State.Closed)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException(string.Format("The consumer {0} was already closed when seeking the subscription {1} of the topic " + "{2} to the timestamp {3:D}", ConsumerNameConflict, SubscriptionConflict, _topicName.ToString(), timestamp)));
			}

			if(!Connected)
			{
				return FutureUtil.FailedFuture(new PulsarClientException(string.Format("The client is not connected to the broker when seeking the subscription {0} of the " + "topic {1} to the timestamp {2:D}", SubscriptionConflict, _topicName.ToString(), timestamp)));
			}

			CompletableFuture<Void> seekFuture = new CompletableFuture<Void>();

			long requestId = ClientConflict.NewRequestId();
			ByteBuf seek = Commands.NewSeek(ConsumerId, requestId, timestamp);
			ClientCnx cnx = Cnx();

			_log.info("[{}][{}] Seek subscription to publish time {}", Topic, SubscriptionConflict, timestamp);

			cnx.SendRequestWithId(seek, requestId).thenRun(() =>
			{
			_log.info("[{}][{}] Successfully reset subscription to publish time {}", Topic, SubscriptionConflict, timestamp);
			_acknowledgmentsGroupingTracker.FlushAndClean();
			_seekMessageId = new BatchMessageIdImpl((MessageIdImpl) MessageId.earliest);
			_duringSeek.set(true);
			LastDequeuedMessageId = MessageId.earliest;
			IncomingMessages.clear();
			IncomingMessagesSizeUpdater.set(this, 0);
			seekFuture.complete(null);
			}).exceptionally(e =>
			{
			_log.error("[{}][{}] Failed to reset subscription: {}", Topic, SubscriptionConflict, e.Cause.Message);
			seekFuture.completeExceptionally(PulsarClientException.Wrap(e.Cause, string.Format("Failed to seek the subscription {0} of the topic {1} to the timestamp {2:D}", SubscriptionConflict, _topicName.ToString(), timestamp)));
			return null;
		});
			return seekFuture;
		}

		internal override CompletableFuture<Void> SeekAsync(MessageId messageId)
		{
			if(State == State.Closing || State == State.Closed)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException(string.Format("The consumer {0} was already closed when seeking the subscription {1} of the topic " + "{2} to the message {3}", ConsumerNameConflict, SubscriptionConflict, _topicName.ToString(), messageId.ToString())));
			}

			if(!Connected)
			{
				return FutureUtil.FailedFuture(new PulsarClientException(string.Format("The client is not connected to the broker when seeking the subscription {0} of the " + "topic {1} to the message {2}", SubscriptionConflict, _topicName.ToString(), messageId.ToString())));
			}
			CompletableFuture<Void> seekFuture = new CompletableFuture<Void>();

			long requestId = ClientConflict.NewRequestId();
			ByteBuf seek = null;
			if(messageId is BatchMessageIdImpl)
			{
				BatchMessageIdImpl msgId = (BatchMessageIdImpl) messageId;
				// Initialize ack set
				BitSetRecyclable ackSet = BitSetRecyclable.Create();
				ackSet.Set(0, msgId.BatchSize);
				ackSet.Clear(0, Math.Max(msgId.BatchIndex, 0));
				long[] ackSetArr = ackSet.ToLongArray();
				ackSet.Recycle();

				seek = Commands.NewSeek(ConsumerId, requestId, msgId.LedgerId, msgId.EntryId, ackSetArr);
			}
			else
			{
				MessageIdImpl msgId = (MessageIdImpl) messageId;
				seek = Commands.NewSeek(ConsumerId, requestId, msgId.LedgerId, msgId.EntryId, new long[0]);
			}

			ClientCnx cnx = Cnx();

			_log.info("[{}][{}] Seek subscription to message id {}", Topic, SubscriptionConflict, messageId);

			cnx.SendRequestWithId(seek, requestId).thenRun(() =>
			{
			_log.info("[{}][{}] Successfully reset subscription to message id {}", Topic, SubscriptionConflict, messageId);
			_acknowledgmentsGroupingTracker.FlushAndClean();
			_seekMessageId = new BatchMessageIdImpl((MessageIdImpl) messageId);
			_duringSeek.set(true);
			LastDequeuedMessageId = MessageId.earliest;
			IncomingMessages.clear();
			IncomingMessagesSizeUpdater.set(this, 0);
			seekFuture.complete(null);
			}).exceptionally(e =>
			{
			_log.error("[{}][{}] Failed to reset subscription: {}", Topic, SubscriptionConflict, e.Cause.Message);
			seekFuture.completeExceptionally(PulsarClientException.Wrap(e.Cause, string.Format("Failed to seek the subscription {0} of the topic {1} to the message {2}", SubscriptionConflict, _topicName.ToString(), messageId.ToString())));
			return null;
		});
			return seekFuture;
		}

		internal virtual bool HasMessageAvailable()
		{
			try
			{
				return HasMessageAvailableAsync().get();
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		internal virtual CompletableFuture<bool> HasMessageAvailableAsync()
		{ 
			CompletableFuture<bool> booleanFuture = new CompletableFuture<bool>();

			// we haven't read yet. use startMessageId for comparison
			if(LastDequeuedMessageId == MessageId.earliest)
			{
				// if we are starting from latest, we should seek to the actual last message first.
				// allow the last one to be read when read head inclusively.
				if(_startMessageId.LedgerId == long.MaxValue && _startMessageId.EntryId == long.MaxValue && _startMessageId.PartitionIndexConflict == -1)
				{

					LastMessageIdAsync.thenCompose(this.seekAsync).whenComplete((ignore, e) =>
					{
					if(e != null)
					{
						_log.error("[{}][{}] Failed getLastMessageId command", Topic, SubscriptionConflict);
						booleanFuture.completeExceptionally(e.Cause);
					}
					else
					{
						booleanFuture.complete(_resetIncludeHead);
					}
					});

					return booleanFuture;
				}

				if(HasMoreMessages(_lastMessageIdInBroker, _startMessageId, _resetIncludeHead))
				{
					booleanFuture.complete(true);
					return booleanFuture;
				}

				LastMessageIdAsync.thenAccept(messageId =>
				{
				_lastMessageIdInBroker = messageId;
				if(HasMoreMessages(_lastMessageIdInBroker, _startMessageId, _resetIncludeHead))
				{
					booleanFuture.complete(true);
				}
				else
				{
					booleanFuture.complete(false);
				}
				}).exceptionally(e =>
				{
				_log.error("[{}][{}] Failed getLastMessageId command", Topic, SubscriptionConflict);
				booleanFuture.completeExceptionally(e.Cause);
				return null;
			});

			}
			else
			{
				// read before, use lastDequeueMessage for comparison
				if(HasMoreMessages(_lastMessageIdInBroker, LastDequeuedMessageId, false))
				{
					booleanFuture.complete(true);
					return booleanFuture;
				}

				LastMessageIdAsync.thenAccept(messageId =>
				{
				_lastMessageIdInBroker = messageId;
				if(HasMoreMessages(_lastMessageIdInBroker, LastDequeuedMessageId, false))
				{
					booleanFuture.complete(true);
				}
				else
				{
					booleanFuture.complete(false);
				}
				}).exceptionally(e =>
				{
				_log.error("[{}][{}] Failed getLastMessageId command", Topic, SubscriptionConflict);
				booleanFuture.completeExceptionally(e.Cause);
				return null;
			});
			}

			return booleanFuture;
		}

		private bool HasMoreMessages(MessageId lastMessageIdInBroker, MessageId messageId, bool inclusive)
		{
			if(inclusive && lastMessageIdInBroker.CompareTo(messageId) >= 0 && ((MessageIdImpl)lastMessageIdInBroker).EntryId != -1)
			{
				return true;
			}

			if(!inclusive && lastMessageIdInBroker.CompareTo(messageId) > 0 && ((MessageIdImpl)lastMessageIdInBroker).EntryId != -1)
			{
				return true;
			}

			return false;
		}

		internal override CompletableFuture<MessageId> LastMessageIdAsync
		{
			get
			{
				if(State == State.Closing || State == State.Closed)
				{
					return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException(string.Format("The consumer {0} was already closed when the subscription {1} of the topic {2} " + "getting the last message id", ConsumerNameConflict, SubscriptionConflict, _topicName.ToString())));
				}
    
				AtomicLong opTimeoutMs = new AtomicLong(ClientConflict.Configuration.OperationTimeoutMs);
				Backoff backoff = (new BackoffBuilder()).SetInitialTime(100, TimeUnit.MILLISECONDS).SetMax(opTimeoutMs.get() * 2, TimeUnit.MILLISECONDS).SetMandatoryStop(0, TimeUnit.MILLISECONDS).Create();
    
				CompletableFuture<MessageId> getLastMessageIdFuture = new CompletableFuture<MessageId>();
    
				InternalGetLastMessageIdAsync(backoff, opTimeoutMs, getLastMessageIdFuture);
				return getLastMessageIdFuture;
			}
		}

		private void InternalGetLastMessageIdAsync(in Backoff backoff, in AtomicLong remainingTime, CompletableFuture<MessageId> future)
		{
			ClientCnx cnx = Cnx();
			if(Connected && cnx != null)
			{
				if(!Commands.PeerSupportsGetLastMessageId(cnx.RemoteEndpointProtocolVersion))
				{
					future.completeExceptionally(new PulsarClientException.NotSupportedException(string.Format("The command `GetLastMessageId` is not supported for the protocol version {0:D}. " + "The consumer is {1}, topic {2}, subscription {3}", cnx.RemoteEndpointProtocolVersion, ConsumerNameConflict, _topicName.ToString(), SubscriptionConflict)));
				}

				long requestId = ClientConflict.NewRequestId();
				ByteBuf getLastIdCmd = Commands.NewGetLastMessageId(ConsumerId, requestId);
				_log.info("[{}][{}] Get topic last message Id", Topic, SubscriptionConflict);

				cnx.SendGetLastMessageId(getLastIdCmd, requestId).thenAccept((result) =>
				{
				_log.info("[{}][{}] Successfully getLastMessageId {}:{}", Topic, SubscriptionConflict, result.LedgerId, result.EntryId);
				if(result.BatchIndex < 0)
				{
					future.complete(new MessageIdImpl(result.LedgerId, result.EntryId, result.Partition));
				}
				else
				{
					future.complete(new BatchMessageIdImpl(result.LedgerId, result.EntryId, result.Partition, result.BatchIndex));
				}
				}).exceptionally(e =>
				{
				_log.error("[{}][{}] Failed getLastMessageId command", Topic, SubscriptionConflict);
				future.completeExceptionally(PulsarClientException.Wrap(e.Cause, string.Format("The subscription {0} of the topic {1} gets the last message id was failed", SubscriptionConflict, _topicName.ToString())));
				return null;
			});
			}
			else
			{
				long nextDelay = Math.Min(backoff.Next(), remainingTime.get());
				if(nextDelay <= 0)
				{
					future.completeExceptionally(new PulsarClientException.TimeoutException(string.Format("The subscription {0} of the topic {1} could not get the last message id " + "withing configured timeout", SubscriptionConflict, _topicName.ToString())));
					return;
				}

				((ScheduledExecutorService) ListenerExecutor).schedule(() =>
				{
				_log.warn("[{}] [{}] Could not get connection while getLastMessageId -- Will try again in {} ms", Topic, HandlerName, nextDelay);
				remainingTime.addAndGet(-nextDelay);
				InternalGetLastMessageIdAsync(backoff, remainingTime, future);
				}, nextDelay, TimeUnit.MILLISECONDS);
			}
		}

		private MessageIdImpl GetMessageIdImpl<T1>(Message<T1> msg)
		{
			MessageIdImpl messageId = (MessageIdImpl) msg.MessageId;
			if(messageId is BatchMessageIdImpl)
			{
				// messageIds contain MessageIdImpl, not BatchMessageIdImpl
				messageId = new MessageIdImpl(messageId.LedgerId, messageId.EntryId, PartitionIndex);
			}
			return messageId;
		}


		private bool IsMessageUndecryptable(MessageMetadata msgMetadata)
		{
			return (msgMetadata.EncryptionKeysCount > 0 && Conf.CryptoKeyReader == null && Conf.CryptoFailureAction == ConsumerCryptoFailureAction.CONSUME);
		}

		/// <summary>
		/// Create EncryptionContext if message payload is encrypted
		/// </summary>
		/// <param name="msgMetadata"> </param>
		/// <returns> <seealso cref="Optional"/><<seealso cref="EncryptionContext"/>> </returns>
		private Optional<EncryptionContext> CreateEncryptionContext(MessageMetadata msgMetadata)
		{

			EncryptionContext encryptionCtx = null;
			if(msgMetadata.EncryptionKeysCount > 0)
			{
				encryptionCtx = new EncryptionContext();
				IDictionary<string, EncryptionContext.EncryptionKey> keys = msgMetadata.EncryptionKeysList.ToDictionary(EncryptionKeys::getKey, e => new EncryptionContext.EncryptionKey(e.Value.toByteArray(), e.MetadataList != null ? e.MetadataList.ToDictionary(KeyValue::getKey, KeyValue::getValue) : null));
				sbyte[] encParam = new sbyte[MessageCrypto.IV_LEN];
				msgMetadata.EncryptionParam.copyTo(encParam, 0);
				int? batchSize = Optional.ofNullable(msgMetadata.HasNumMessagesInBatch() ? msgMetadata.NumMessagesInBatch : null);
				encryptionCtx.Keys = keys;
				encryptionCtx.Param = encParam;
				encryptionCtx.Algorithm = msgMetadata.EncryptionAlgo;
				encryptionCtx.CompressionType = CompressionCodecProvider.ConvertFromWireProtocol(msgMetadata.Compression);
				encryptionCtx.UncompressedMessageSize = msgMetadata.UncompressedSize;
				encryptionCtx.BatchSize = batchSize;
			}
			return Optional.ofNullable(encryptionCtx);
		}

		private int RemoveExpiredMessagesFromQueue(ISet<MessageId> messageIds)
		{
			int messagesFromQueue = 0;
			Message<T> peek = IncomingMessages.peek();
			if(peek != null)
			{
				MessageIdImpl messageId = GetMessageIdImpl(peek);
				if(!messageIds.Contains(messageId))
				{
					// first message is not expired, then no message is expired in queue.
					return 0;
				}

				// try not to remove elements that are added while we remove
				Message<T> message = IncomingMessages.poll();
				while(message != null)
				{
					IncomingMessagesSizeUpdater.addAndGet(this, -message.Data.Length);
					messagesFromQueue++;
					MessageIdImpl id = GetMessageIdImpl(message);
					if(!messageIds.Contains(id))
					{
						messageIds.Add(id);
						break;
					}
					message = IncomingMessages.poll();
				}
			}
			return messagesFromQueue;
		}

		internal override ConsumerStats Stats
		{
			get
			{
				return StatsConflict;
			}
		}

		internal virtual void SetTerminated()
		{
			_log.info("[{}] [{}] [{}] Consumer has reached the end of topic", SubscriptionConflict, Topic, ConsumerNameConflict);
			_hasReachedEndOfTopic = true;
			if(Listener != null)
			{
				// Propagate notification to listener
				Listener.ReachedEndOfTopic(this);
			}
		}

		internal override bool HasReachedEndOfTopic()
		{
			return _hasReachedEndOfTopic;
		}

		internal override int GetHashCode()
		{
			return Objects.hash(Topic, SubscriptionConflict, ConsumerNameConflict);
		}

		internal override bool Equals(object o)
		{
			if(this == o)
			{
				return true;
			}
			if(!(o is ConsumerActor))
			{
				return false;
			}

			ConsumerActor<object> consumer = (ConsumerActor<object>) o;
			return ConsumerId == consumer.ConsumerId;
		}

		// wrapper for connection methods
		internal virtual ClientCnx Cnx()
		{
			return this._connectionHandler.Cnx();
		}

		internal virtual void ResetBackoff()
		{
			this._connectionHandler.ResetBackoff();
		}

		internal virtual void ConnectionClosed(ClientCnx cnx)
		{
			this._connectionHandler.ConnectionClosed(cnx);
		}


		internal virtual ClientCnx ClientCnx
		{
			get
			{
				return this._connectionHandler.Cnx();
			}
			set
			{
				if(value != null)
				{
					this._connectionHandler.ClientCnx = value;
					value.RegisterConsumer(ConsumerId, this);
				}
				ClientCnx previousClientCnx = _clientCnxUsedForConsumerRegistration.getAndSet(value);
				if(previousClientCnx != null && previousClientCnx != value)
				{
					previousClientCnx.RemoveConsumer(ConsumerId);
				}
			}
		}


		internal virtual void DeregisterFromClientCnx()
		{
			ClientCnx = null;
		}

		internal virtual void ReconnectLater(Exception exception)
		{
			this._connectionHandler.ReconnectLater(exception);
		}

		internal virtual void GrabCnx()
		{
			this._connectionHandler.GrabCnx();
		}

		internal virtual string TopicNameWithoutPartition
		{
			get
			{
				return _topicNameWithoutPartition;
			}
		}

		internal class ChunkedMessageCtx
		{

			protected internal int TotalChunks = -1;
			protected internal ByteBuf ChunkedMsgBuffer;
			protected internal int LastChunkedMessageId = -1;
			protected internal MessageIdImpl[] ChunkedMessageIds;
			protected internal long ReceivedTime = 0;

			internal static ChunkedMessageCtx Get(int numChunksFromMsg, ByteBuf chunkedMsgBuffer)
			{
				ChunkedMessageCtx ctx = RECYCLER.get();
				ctx.TotalChunks = numChunksFromMsg;
				ctx.ChunkedMsgBuffer = chunkedMsgBuffer;
				ctx.ChunkedMessageIds = new MessageIdImpl[numChunksFromMsg];
				ctx.ReceivedTime = DateTimeHelper.CurrentUnixTimeMillis();
				return ctx;
			}

			internal readonly Recycler.Handle<ChunkedMessageCtx> RecyclerHandle;

			internal ChunkedMessageCtx(Recycler.Handle<ChunkedMessageCtx> recyclerHandle)
			{
				RecyclerHandle = recyclerHandle;
			}

			internal static readonly Recycler<ChunkedMessageCtx> RECYCLER = new RecyclerAnonymousInnerClass();

			private class RecyclerAnonymousInnerClass : Recycler<ChunkedMessageCtx>
			{
				protected internal ChunkedMessageCtx NewObject(Recycler.Handle<ChunkedMessageCtx> handle)
				{
					return new ChunkedMessageCtx(handle);
				}
			}

			internal virtual void Recycle()
			{
				TotalChunks = -1;
				ChunkedMsgBuffer = null;
				LastChunkedMessageId = -1;
				RecyclerHandle.recycle(this);
			}
		}

		private void RemoveOldestPendingChunkedMessage()
		{
			ChunkedMessageCtx chunkedMsgCtx = null;
			string firstPendingMsgUuid = null;
			while(chunkedMsgCtx == null && !_pendingChunckedMessageUuidQueue.Empty)
			{
				// remove oldest pending chunked-message group and free memory
				firstPendingMsgUuid = _pendingChunckedMessageUuidQueue.poll();
				chunkedMsgCtx = StringUtils.isNotBlank(firstPendingMsgUuid) ? ChunkedMessagesMap.Get(firstPendingMsgUuid) : null;
			}
			RemoveChunkMessage(firstPendingMsgUuid, chunkedMsgCtx, this._autoAckOldestChunkedMessageOnQueueFull);
		}

		protected internal virtual void RemoveExpireIncompleteChunkedMessages()
		{
			if(ExpireTimeOfIncompleteChunkedMessageMillis <= 0)
			{
				return;
			}
			ChunkedMessageCtx chunkedMsgCtx = null;
			string messageUUID;
			while(!string.ReferenceEquals((messageUUID = _pendingChunckedMessageUuidQueue.peek()), null))
			{
				chunkedMsgCtx = StringUtils.isNotBlank(messageUUID) ? ChunkedMessagesMap.Get(messageUUID) : null;
				if(chunkedMsgCtx != null && DateTimeHelper.CurrentUnixTimeMillis() > (chunkedMsgCtx.ReceivedTime + ExpireTimeOfIncompleteChunkedMessageMillis))
				{
					_pendingChunckedMessageUuidQueue.remove(messageUUID);
					RemoveChunkMessage(messageUUID, chunkedMsgCtx, true);
				}
				else
				{
					return;
				}
			}
		}

		private void RemoveChunkMessage(string msgUUID, ChunkedMessageCtx chunkedMsgCtx, bool autoAck)
		{
			if(chunkedMsgCtx == null)
			{
				return;
			}
			// clean up pending chuncked-Message
			ChunkedMessagesMap.Remove(msgUUID);
			if(chunkedMsgCtx.ChunkedMessageIds != null)
			{
				foreach(MessageIdImpl msgId in chunkedMsgCtx.ChunkedMessageIds)
				{
					if(msgId == null)
					{
						continue;
					}
					if(autoAck)
					{
						_log.info("Removing chunk message-id {}", msgId);
						doAcknowledge(msgId, CommandAck.AckType.Individual, Collections.emptyMap(), null);
					}
					else
					{
						TrackMessage(msgId);
					}
				}
			}
			if(chunkedMsgCtx.ChunkedMsgBuffer != null)
			{
				chunkedMsgCtx.ChunkedMsgBuffer.release();
			}
			chunkedMsgCtx.Recycle();
			_pendingChunckedMessageCount--;
		}

		private CompletableFuture<Void> DoTransactionAcknowledgeForResponse(MessageId messageId, CommandAck.AckType ackType, CommandAck.ValidationError validationError, IDictionary<string, long> properties, TxnID txnID)
		{
			CompletableFuture<Void> callBack = new CompletableFuture<Void>();
			BitSetRecyclable bitSetRecyclable = null;
			long ledgerId;
			long entryId;
			ByteBuf cmd;
			long requestId = ClientConflict.NewRequestId();
			if(messageId is BatchMessageIdImpl)
			{
				BatchMessageIdImpl batchMessageId = (BatchMessageIdImpl) messageId;
				bitSetRecyclable = BitSetRecyclable.Create();
				ledgerId = batchMessageId.LedgerId;
				entryId = batchMessageId.EntryId;
				if(ackType == CommandAck.AckType.Cumulative)
				{
					batchMessageId.AckCumulative();
					bitSetRecyclable.Set(0, batchMessageId.BatchSize);
					bitSetRecyclable.Clear(0, batchMessageId.BatchIndex + 1);
				}
				else
				{
					bitSetRecyclable.Set(0, batchMessageId.BatchSize);
					bitSetRecyclable.Clear(batchMessageId.BatchIndex);
				}
				cmd = Commands.NewAck(ConsumerId, ledgerId, entryId, bitSetRecyclable, ackType, validationError, properties, txnID.LeastSigBits, txnID.MostSigBits, requestId, batchMessageId.BatchSize);
			}
			else
			{
				MessageIdImpl singleMessage = (MessageIdImpl) messageId;
				ledgerId = singleMessage.LedgerId;
				entryId = singleMessage.EntryId;
				cmd = Commands.NewAck(ConsumerId, ledgerId, entryId, bitSetRecyclable, ackType, validationError, properties, txnID.LeastSigBits, txnID.MostSigBits, requestId);
			}

			OpForAckCallBack op = OpForAckCallBack.Create(cmd, callBack, messageId, new TxnID(txnID.MostSigBits, txnID.LeastSigBits));
			_ackRequests.Put(requestId, op);
			if(ackType == CommandAck.AckType.Cumulative)
			{
				_unAckedMessageTracker.RemoveMessagesTill(messageId);
			}
			else
			{
				_unAckedMessageTracker.Remove(messageId);
			}
			cmd.retain();
			Cnx().Ctx().writeAndFlush(cmd, Cnx().Ctx().voidPromise());
			return callBack;
		}

		protected internal virtual void AckReceipt(long requestId)
		{
			OpForAckCallBack callBackOp = _ackRequests.Remove(requestId);
			if(callBackOp == null || callBackOp.Callback.Done)
			{
				_log.info("Ack request has been handled requestId : {}", requestId);
			}
			else
			{
				callBackOp.Callback.complete(null);
				if(_log.DebugEnabled)
				{
					_log.debug("MessageId : {} has ack by TxnId : {}", callBackOp.MessageId.LedgerId + ":" + callBackOp.MessageId.EntryId, callBackOp.TxnID.ToString());
				}
			}
		}

		protected internal virtual void AckError(long requestId, PulsarClientException pulsarClientException)
		{
			OpForAckCallBack callBackOp = _ackRequests.Remove(requestId);
			if(callBackOp == null || callBackOp.Callback.Done)
			{
				_log.info("Ack request has been handled requestId : {}", requestId);
			}
			else
			{
				callBackOp.Callback.completeExceptionally(pulsarClientException);
				if(_log.DebugEnabled)
				{
					_log.debug("MessageId : {} has ack by TxnId : {}", callBackOp.MessageId, callBackOp.TxnID);
				}
				callBackOp.Recycle();
			}
		}

		private class OpForAckCallBack
		{
			protected internal ByteBuf Cmd;
			protected internal CompletableFuture<Void> Callback;
			protected internal MessageIdImpl MessageId;
			protected internal TxnID TxnID;

			internal static OpForAckCallBack Create(ByteBuf cmd, CompletableFuture<Void> callback, MessageId messageId, TxnID txnID)
			{
				OpForAckCallBack op = RECYCLER.get();
				op.Callback = callback;
				op.Cmd = cmd;
				op.MessageId = (MessageIdImpl) messageId;
				op.TxnID = txnID;
				return op;
			}

			internal OpForAckCallBack(Recycler.Handle<OpForAckCallBack> recyclerHandle)
			{
				RecyclerHandle = recyclerHandle;
			}

			internal virtual void Recycle()
			{
				if(Cmd != null)
				{
					ReferenceCountUtil.safeRelease(Cmd);
				}
				RecyclerHandle.recycle(this);
			}

			internal readonly Recycler.Handle<OpForAckCallBack> RecyclerHandle;
			internal static readonly Recycler<OpForAckCallBack> RECYCLER = new RecyclerAnonymousInnerClass();

			private class RecyclerAnonymousInnerClass : Recycler<OpForAckCallBack>
			{
				protected internal override OpForAckCallBack NewObject(Recycler.Handle<OpForAckCallBack> handle)
				{
					return new OpForAckCallBack(handle);
				}
			}
		}

		private static readonly Logger _log = LoggerFactory.getLogger(typeof(ConsumerActor));

	}

}