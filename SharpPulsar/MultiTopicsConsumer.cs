using Akka.Actor;
using App.Metrics.Concurrency;
using SharpPulsar.Batch;
using SharpPulsar.Configuration;
using SharpPulsar.Impl;
using SharpPulsar.Stats.Consumer.Api;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

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

	public class MultiTopicsConsumer<T> : ConsumerActorBase<T>
	{

		public const string DummyTopicNamePrefix = "MultiTopicsConsumer-";

		// Map <topic+partition, consumer>, when get do ACK, consumer will by find by topic name
		private readonly ConcurrentDictionary<string, IActorRef> _consumers;

		// Map <topic, numPartitions>, store partition number for each topic
		protected internal readonly ConcurrentDictionary<string, int> TopicsConflict;

		// Queue of partition consumers on which we have stopped calling receiveAsync() because the
		// shared incoming queue was full
		private readonly ConcurrentQueue<IActorRef> _pausedConsumers;

		// Threshold for the shared queue. When the size of the shared queue goes below the threshold, we are going to
		// resume receiving from the paused consumer partitions
		private readonly int _sharedQueueResumeThreshold;

		// sum of topicPartitions, simple topic has 1, partitioned topic equals to partition number.
		internal AtomicInteger AllTopicPartitionsNumber;

		private bool _paused = false;
		// timeout related to auto check and subscribe partition increasement
		private IAdvancedScheduler _partitionsAutoUpdateTimeout = null;
		internal TopicsPartitionChangedListener TopicsPartitionChangedListener;

		private readonly IConsumerStatsRecorder _stats;
		private readonly IActorRef _unAckedMessageTracker;
		private readonly ConsumerConfigurationData<T> _internalConfig;

		private volatile BatchMessageId _startMessageId = null;
		private readonly long _startMessageRollbackDurationInSec;

		internal MultiTopicsConsumer(PulsarClientImpl client, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, CompletableFuture<ConsumerActor<T>> subscribeFuture, Schema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist) : this(client, DummyTopicNamePrefix + ConsumerName.GenerateRandomName(), conf, listenerExecutor, subscribeFuture, schema, interceptors, createTopicIfDoesNotExist)
		{
		}

		internal MultiTopicsConsumer(PulsarClientImpl client, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, CompletableFuture<ConsumerActor<T>> subscribeFuture, Schema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, MessageId startMessageId, long startMessageRollbackDurationInSec) : this(client, DummyTopicNamePrefix + ConsumerName.GenerateRandomName(), conf, listenerExecutor, subscribeFuture, schema, interceptors, createTopicIfDoesNotExist, startMessageId, startMessageRollbackDurationInSec)
		{
		}

		internal MultiTopicsConsumer(PulsarClientImpl client, string singleTopic, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, CompletableFuture<ConsumerActor<T>> subscribeFuture, Schema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist) : this(client, singleTopic, conf, listenerExecutor, subscribeFuture, schema, interceptors, createTopicIfDoesNotExist, null, 0)
		{
		}

		internal MultiTopicsConsumer(PulsarClientImpl client, string singleTopic, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, CompletableFuture<ConsumerActor<T>> subscribeFuture, Schema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, MessageId startMessageId, long startMessageRollbackDurationInSec) : base(client, singleTopic, conf, Math.Max(2, conf.ReceiverQueueSize), listenerExecutor, subscribeFuture, schema, interceptors)
		{

			checkArgument(conf.ReceiverQueueSize > 0, "Receiver queue size needs to be greater than 0 for Topics Consumer");

			this.TopicsConflict = new ConcurrentDictionary<string, int>();
			this._consumers = new ConcurrentDictionary<string, ConsumerImpl<T>>();
			this._pausedConsumers = new ConcurrentLinkedQueue<ConsumerImpl<T>>();
			this._sharedQueueResumeThreshold = MaxReceiverQueueSizeConflict / 2;
			this.AllTopicPartitionsNumber = new AtomicInteger(0);
			this._startMessageId = startMessageId != null ? new BatchMessageIdImpl(MessageIdImpl.ConvertToMessageIdImpl(startMessageId)) : null;
			this._startMessageRollbackDurationInSec = startMessageRollbackDurationInSec;

			if(conf.AckTimeoutMillis != 0)
			{
				if(conf.TickDurationMillis > 0)
				{
					this._unAckedMessageTracker = new UnAckedTopicMessageTracker(client, this, conf.AckTimeoutMillis, conf.TickDurationMillis);
				}
				else
				{
					this._unAckedMessageTracker = new UnAckedTopicMessageTracker(client, this, conf.AckTimeoutMillis);
				}
			}
			else
			{
				this._unAckedMessageTracker = Org.Apache.Pulsar.Client.Impl.UnAckedMessageTracker.UNACKED_MESSAGE_TRACKER_DISABLED;
			}

			this._internalConfig = InternalConsumerConfig;
			this._stats = client.Configuration.StatsIntervalSeconds > 0 ? new ConsumerStatsRecorderImpl(this) : null;

			// start track and auto subscribe partition increasement
			if(conf.AutoUpdatePartitions)
			{
				TopicsPartitionChangedListener = new TopicsPartitionChangedListener(this);
				_partitionsAutoUpdateTimeout = client.Timer().newTimeout(_partitionsAutoUpdateTimerTask, conf.AutoUpdatePartitionsIntervalSeconds, TimeUnit.SECONDS);
			}

			if(conf.TopicNames.Empty)
			{
				State = State.Ready;
				SubscribeFuture().complete(MultiTopicsConsumer.this);
				return;
			}

			checkArgument(conf.TopicNames.Empty || TopicNamesValid(conf.TopicNames), "Topics is empty or invalid.");

			IList<CompletableFuture<Void>> futures = conf.TopicNames.Select(t => subscribeAsync(t, createTopicIfDoesNotExist)).ToList();
			FutureUtil.WaitForAll(futures).thenAccept(finalFuture =>
			{
			if(AllTopicPartitionsNumber.get() > MaxReceiverQueueSizeConflict)
			{
				MaxReceiverQueueSize = AllTopicPartitionsNumber.get();
			}
			State = State.Ready;
			StartReceivingMessages(new List<ConsumerImpl<T>>(_consumers.Values));
			_log.info("[{}] [{}] Created topics consumer with {} sub-consumers", Topic, SubscriptionConflict, AllTopicPartitionsNumber.get());
			SubscribeFuture().complete(MultiTopicsConsumer.this);
			}).exceptionally(ex =>
			{
			_log.warn("[{}] Failed to subscribe topics: {}", Topic, ex.Message);
			subscribeFuture.completeExceptionally(ex);
			return null;
		});
		}

		// Check topics are valid.
		// - each topic is valid,
		// - topic names are unique.
		private static bool TopicNamesValid(ICollection<string> topics)
		{
			checkState(topics != null && topics.Count >= 1, "topics should contain more than 1 topic");

			Optional<string> result = topics.Where(Topic => !TopicName.IsValid(Topic)).First();

			if(result.Present)
			{
				_log.warn("Received invalid topic name: {}", result.get());
				return false;
			}

			// check topic names are unique
			HashSet<string> set = new HashSet<string>(topics);
			if(set.Count == topics.Count)
			{
				return true;
			}
			else
			{
				_log.warn("Topic names not unique. unique/all : {}/{}", set.Count, topics.Count);
				return false;
			}
		}

		private void StartReceivingMessages(IList<ConsumerImpl<T>> newConsumers)
		{
			if(_log.DebugEnabled)
			{
				_log.debug("[{}] startReceivingMessages for {} new consumers in topics consumer, state: {}", Topic, newConsumers.Count, State);
			}
			if(State == State.Ready)
			{
				newConsumers.ForEach(consumer =>
				{
				consumer.increaseAvailablePermits(consumer.ConnectionHandler.cnx(), Conf.ReceiverQueueSize);
				ReceiveMessageFromConsumer(consumer);
				});
			}
		}

		private void ReceiveMessageFromConsumer(ConsumerImpl<T> consumer)
		{
			consumer.ReceiveAsync().thenAccept(message =>
			{
			if(_log.DebugEnabled)
			{
				_log.debug("[{}] [{}] Receive message from sub consumer:{}", Topic, SubscriptionConflict, consumer.Topic);
			}
			MessageReceived(consumer, message);
			int size = IncomingMessages.size();
			if(size >= MaxReceiverQueueSizeConflict || (size > _sharedQueueResumeThreshold && !_pausedConsumers.Empty))
			{
				_pausedConsumers.add(consumer);
			}
			else
			{
				ClientConflict.InternalExecutorService.execute(() => ReceiveMessageFromConsumer(consumer));
			}
			});
		}

		private void MessageReceived(ConsumerImpl<T> consumer, Message<T> message)
		{
			checkArgument(message is MessageImpl);
			TopicMessageImpl<T> topicMessage = new TopicMessageImpl<T>(consumer.Topic, consumer.TopicNameWithoutPartition, message);

			if(_log.DebugEnabled)
			{
				_log.debug("[{}][{}] Received message from topics-consumer {}", Topic, SubscriptionConflict, message.MessageId);
			}

			// if asyncReceive is waiting : return message to callback without adding to incomingMessages queue
			CompletableFuture<Message<T>> receivedFuture = PollPendingReceive();
			if(receivedFuture != null)
			{
				_unAckedMessageTracker.Add(topicMessage.MessageId);
				CompletePendingReceive(receivedFuture, topicMessage);
			}
			else if(EnqueueMessageAndCheckBatchReceive(topicMessage) && HasPendingBatchReceive())
			{
				NotifyPendingBatchReceivedCallBack();
			}

			if(Listener != null)
			{
				// Trigger the notification on the message listener in a separate thread to avoid blocking the networking
				// thread while the message processing happens
				ListenerExecutor.execute(() =>
				{
				Message<T> msg;
				try
				{
					msg = InternalReceive(0, TimeUnit.MILLISECONDS);
					if(msg == null)
					{
						if(_log.DebugEnabled)
						{
							_log.debug("[{}] [{}] Message has been cleared from the queue", Topic, SubscriptionConflict);
						}
						return;
					}
				}
				catch(PulsarClientException e)
				{
					_log.warn("[{}] [{}] Failed to dequeue the message for listener", Topic, SubscriptionConflict, e);
					return;
				}
				try
				{
					if(_log.DebugEnabled)
					{
						_log.debug("[{}][{}] Calling message listener for message {}", Topic, SubscriptionConflict, message.MessageId);
					}
					Listener.Received(MultiTopicsConsumer.this, msg);
				}
				catch(Exception t)
				{
					_log.error("[{}][{}] Message listener error in processing message: {}", Topic, SubscriptionConflict, message, t);
				}
				});
			}
		}

		protected internal override void MessageProcessed<T1>(Message<T1> msg)
		{
			lock(this)
			{
				_unAckedMessageTracker.Add(msg.MessageId);
				IncomingMessagesSizeUpdater.addAndGet(this, -msg.Data.Length);
			}
		}

		private void ResumeReceivingFromPausedConsumersIfNeeded()
		{
			if(IncomingMessages.size() <= _sharedQueueResumeThreshold && !_pausedConsumers.Empty)
			{
				while(true)
				{
					ConsumerImpl<T> consumer = _pausedConsumers.poll();
					if(consumer == null)
					{
						break;
					}

					ClientConflict.InternalExecutorService.execute(() =>
					{
					ReceiveMessageFromConsumer(consumer);
					});
				}
			}
		}

		protected internal override Message<T> InternalReceive()
		{
			Message<T> message;
			try
			{
				message = IncomingMessages.take();
				IncomingMessagesSizeUpdater.addAndGet(this, -message.Data.Length);
				checkState(message is TopicMessageImpl);
				_unAckedMessageTracker.Add(message.MessageId);
				ResumeReceivingFromPausedConsumersIfNeeded();
				return message;
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		protected internal override Message<T> InternalReceive(int timeout, TimeUnit unit)
		{
			Message<T> message;
			try
			{
				message = IncomingMessages.poll(timeout, unit);
				if(message != null)
				{
					IncomingMessagesSizeUpdater.addAndGet(this, -message.Data.Length);
					checkArgument(message is TopicMessageImpl);
					_unAckedMessageTracker.Add(message.MessageId);
				}
				ResumeReceivingFromPausedConsumersIfNeeded();
				return message;
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		protected internal override Messages<T> InternalBatchReceive()
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
					_stats.IncrementNumBatchReceiveFailed();
					throw PulsarClientException.Unwrap(e);
				}
				else
				{
					return null;
				}
			}
		}

		protected internal override CompletableFuture<Messages<T>> InternalBatchReceiveAsync()
		{
			CompletableFutureCancellationHandler cancellationHandler = new CompletableFutureCancellationHandler();
			CompletableFuture<Messages<T>> result = cancellationHandler.CreateFuture();
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
							IncomingMessagesSizeUpdater.addAndGet(this, -msg.Data.Length);
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
				ResumeReceivingFromPausedConsumersIfNeeded();
			}
			finally
			{
				@lock.writeLock().unlock();
			}
			return result;
		}

		protected internal override CompletableFuture<Message<T>> InternalReceiveAsync()
		{
			CompletableFutureCancellationHandler cancellationHandler = new CompletableFutureCancellationHandler();
			CompletableFuture<Message<T>> result = cancellationHandler.CreateFuture();
			Message<T> message = IncomingMessages.poll();
			if(message == null)
			{
				PendingReceives.add(result);
				cancellationHandler.CancelAction = () => PendingReceives.remove(result);
			}
			else
			{
				IncomingMessagesSizeUpdater.addAndGet(this, -message.Data.Length);
				checkState(message is TopicMessageImpl);
				_unAckedMessageTracker.Add(message.MessageId);
				ResumeReceivingFromPausedConsumersIfNeeded();
				result.complete(message);
			}
			return result;
		}

		protected internal override CompletableFuture<Void> DoAcknowledge(MessageId messageId, AckType ackType, IDictionary<string, long> properties, TransactionImpl txnImpl)
		{
			checkArgument(messageId is TopicMessageIdImpl);
			TopicMessageIdImpl topicMessageId = (TopicMessageIdImpl) messageId;

			if(State != State.Ready)
			{
				return FutureUtil.FailedFuture(new PulsarClientException("Consumer already closed"));
			}

			if(ackType == AckType.Cumulative)
			{
				ConsumerActor individualConsumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				if(individualConsumer != null)
				{
					MessageId innerId = topicMessageId.InnerMessageId;
					return individualConsumer.AcknowledgeCumulativeAsync(innerId);
				}
				else
				{
					return FutureUtil.FailedFuture(new PulsarClientException.NotConnectedException());
				}
			}
			else
			{
				ConsumerImpl<T> consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);

				MessageId innerId = topicMessageId.InnerMessageId;
				return consumer.DoAcknowledgeWithTxn(innerId, ackType, properties, txnImpl).thenRun(() => _unAckedMessageTracker.Remove(topicMessageId));
			}
		}

		protected internal override CompletableFuture<Void> DoAcknowledge(IList<MessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, TransactionImpl txn)
		{
			IList<CompletableFuture<Void>> resultFutures = new List<CompletableFuture<Void>>();
			if(ackType == AckType.Cumulative)
			{
				messageIdList.ForEach(messageId => resultFutures.Add(doAcknowledge(messageId, ackType, properties, txn)));
				return CompletableFuture.allOf(((List<CompletableFuture<Void>>)resultFutures).ToArray());
			}
			else
			{
				if(State != State.Ready)
				{
					return FutureUtil.FailedFuture(new PulsarClientException("Consumer already closed"));
				}
				IDictionary<string, IList<MessageId>> topicToMessageIdMap = new Dictionary<string, IList<MessageId>>();
				foreach(MessageId messageId in messageIdList)
				{
					if(!(messageId is TopicMessageIdImpl))
					{
						return FutureUtil.FailedFuture(new System.ArgumentException("messageId is not instance of TopicMessageIdImpl"));
					}
					TopicMessageIdImpl topicMessageId = (TopicMessageIdImpl) messageId;
					if(!topicToMessageIdMap.ContainsKey(topicMessageId.TopicPartitionName)) topicToMessageIdMap.Add(topicMessageId.TopicPartitionName, new List<>());
					topicToMessageIdMap.GetValueOrNull(topicMessageId.TopicPartitionName).add(topicMessageId.InnerMessageId);
				}
				topicToMessageIdMap.forEach((topicPartitionName, messageIds) =>
				{
				ConsumerImpl<T> consumer = _consumers.GetValueOrNull(topicPartitionName);
				resultFutures.Add(consumer.doAcknowledgeWithTxn(messageIds, ackType, properties, txn).thenAccept((res) => messageIdList.ForEach(_unAckedMessageTracker.remove)));
				});
				return CompletableFuture.allOf(((List<CompletableFuture<Void>>)resultFutures).ToArray());
			}
		}

		protected internal override CompletableFuture<Void> DoReconsumeLater<T1>(Message<T1> message, AckType ackType, IDictionary<string, long> properties, long delayTime, TimeUnit unit)
		{
			MessageId messageId = message.MessageId;
			checkArgument(messageId is TopicMessageIdImpl);
			TopicMessageIdImpl topicMessageId = (TopicMessageIdImpl) messageId;
			if(State != State.Ready)
			{
				return FutureUtil.FailedFuture(new PulsarClientException("Consumer already closed"));
			}

			if(ackType == AckType.Cumulative)
			{
				ConsumerActor individualConsumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				if(individualConsumer != null)
				{
					MessageId innerId = topicMessageId.InnerMessageId;
					return individualConsumer.reconsumeLaterCumulativeAsync(message, delayTime, unit);
				}
				else
				{
					return FutureUtil.FailedFuture(new PulsarClientException.NotConnectedException());
				}
			}
			else
			{
				ConsumerImpl<T> consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				MessageId innerId = topicMessageId.InnerMessageId;
				return consumer.DoReconsumeLater(message, ackType, properties, delayTime, unit).thenRun(() => _unAckedMessageTracker.Remove(topicMessageId));
			}
		}

		public override void NegativeAcknowledge(MessageId messageId)
		{
			checkArgument(messageId is TopicMessageIdImpl);
			TopicMessageIdImpl topicMessageId = (TopicMessageIdImpl) messageId;

			ConsumerImpl<T> consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
			consumer.NegativeAcknowledge(topicMessageId.InnerMessageId);
		}

		public override CompletableFuture<Void> UnsubscribeAsync()
		{
			if(State == State.Closing || State == State.Closed)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}
			State = State.Closing;

			CompletableFuture<Void> unsubscribeFuture = new CompletableFuture<Void>();
			IList<CompletableFuture<Void>> futureList = _consumers.Values.Select(c => c.unsubscribeAsync()).ToList();

			FutureUtil.WaitForAll(futureList).whenComplete((r, ex) =>
			{
			if(ex == null)
			{
				State = State.Closed;
				_unAckedMessageTracker.Dispose();
				unsubscribeFuture.complete(null);
				_log.info("[{}] [{}] [{}] Unsubscribed Topics Consumer", Topic, SubscriptionConflict, ConsumerNameConflict);
			}
			else
			{
				State = State.Failed;
				unsubscribeFuture.completeExceptionally(ex);
				_log.error("[{}] [{}] [{}] Could not unsubscribe Topics Consumer", Topic, SubscriptionConflict, ConsumerNameConflict, ex.Cause);
			}
			});

			return unsubscribeFuture;
		}

		public override CompletableFuture<Void> CloseAsync()
		{
			if(State == State.Closing || State == State.Closed)
			{
				_unAckedMessageTracker.Dispose();
				return CompletableFuture.completedFuture(null);
			}
			State = State.Closing;

			if(_partitionsAutoUpdateTimeout != null)
			{
				_partitionsAutoUpdateTimeout.cancel();
				_partitionsAutoUpdateTimeout = null;
			}

			CompletableFuture<Void> closeFuture = new CompletableFuture<Void>();
			IList<CompletableFuture<Void>> futureList = _consumers.Values.Select(c => c.closeAsync()).ToList();

			FutureUtil.WaitForAll(futureList).whenComplete((r, ex) =>
			{
			if(ex == null)
			{
				State = State.Closed;
				_unAckedMessageTracker.Dispose();
				closeFuture.complete(null);
				_log.info("[{}] [{}] Closed Topics Consumer", Topic, SubscriptionConflict);
				ClientConflict.CleanupConsumer(this);
				FailPendingReceive();
			}
			else
			{
				State = State.Failed;
				closeFuture.completeExceptionally(ex);
				_log.error("[{}] [{}] Could not close Topics Consumer", Topic, SubscriptionConflict, ex.Cause);
			}
			});

			return closeFuture;
		}


		public override bool Connected
		{
			get
			{
				return _consumers.Values.All(consumer => consumer.Connected);
			}
		}

		internal override string HandlerName
		{
			get
			{
				return SubscriptionConflict;
			}
		}

		private ConsumerConfigurationData<T> InternalConsumerConfig
		{
			get
			{
				ConsumerConfigurationData<T> internalConsumerConfig = Conf.Clone();
				internalConsumerConfig.SubscriptionName = SubscriptionConflict;
				internalConsumerConfig.ConsumerName = ConsumerNameConflict;
				internalConsumerConfig.MessageListener = null;
				return internalConsumerConfig;
			}
		}

		public override void RedeliverUnacknowledgedMessages()
		{
			@lock.writeLock().@lock();
			try
			{
				_consumers.Values.ForEach(consumer =>
				{
				consumer.redeliverUnacknowledgedMessages();
				consumer.unAckedChunckedMessageIdSequenceMap.clear();
				});
				IncomingMessages.clear();
				IncomingMessagesSizeUpdater.set(this, 0);
				_unAckedMessageTracker.Clear();
			}
			finally
			{
				@lock.writeLock().unlock();
			}
			ResumeReceivingFromPausedConsumersIfNeeded();
		}

		public override void RedeliverUnacknowledgedMessages(ISet<MessageId> messageIds)
		{
			if(messageIds.Count == 0)
			{
				return;
			}

			checkArgument(messageIds.First().get() is TopicMessageIdImpl);

			if(Conf.SubscriptionType != SubscriptionType.Shared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				RedeliverUnacknowledgedMessages();
				return;
			}
			RemoveExpiredMessagesFromQueue(messageIds);
			messageIds.Select(messageId => (TopicMessageIdImpl)messageId).collect(Collectors.groupingBy(TopicMessageIdImpl::getTopicPartitionName, Collectors.toSet())).ForEach((topicName, messageIds1) => _consumers.GetValueOrNull(topicName).redeliverUnacknowledgedMessages(messageIds1.Select(mid => mid.InnerMessageId).collect(Collectors.toSet())));
			ResumeReceivingFromPausedConsumersIfNeeded();
		}

		protected internal override void CompleteOpBatchReceive(OpBatchReceive<T> op)
		{
			NotifyPendingBatchReceivedCallBack(op);
			ResumeReceivingFromPausedConsumersIfNeeded();
		}

		public override void Seek(MessageId messageId)
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

		public override void Seek(long timestamp)
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

		public override CompletableFuture<Void> SeekAsync(MessageId messageId)
		{
			CompletableFuture<Void> seekFuture = new CompletableFuture<Void>();
			MessageIdImpl targetMessageId = MessageIdImpl.ConvertToMessageIdImpl(messageId);
			if(targetMessageId == null || IsIllegalMultiTopicsMessageId(messageId))
			{
				seekFuture.completeExceptionally(new PulsarClientException("Illegal messageId, messageId can only be earliest/latest"));
				return seekFuture;
			}
			IList<CompletableFuture<Void>> futures = new List<CompletableFuture<Void>>(_consumers.Count);
			_consumers.Values.forEach(consumerImpl => futures.Add(consumerImpl.seekAsync(targetMessageId)));

			_unAckedMessageTracker.Clear();
			IncomingMessages.clear();
			MultiTopicsConsumer.IncomingMessagesSizeUpdater.set(this, 0);

			FutureUtil.WaitForAll(futures).whenComplete((result, exception) =>
			{
			if(exception != null)
			{
				seekFuture.completeExceptionally(exception);
			}
			else
			{
				seekFuture.complete(result);
			}
			});
			return seekFuture;
		}

		public override CompletableFuture<Void> SeekAsync(long timestamp)
		{
			IList<CompletableFuture<Void>> futures = new List<CompletableFuture<Void>>(_consumers.Count);
			_consumers.Values.forEach(consumer => futures.Add(consumer.seekAsync(timestamp)));
			return FutureUtil.WaitForAll(futures);
		}

		public override int AvailablePermits
		{
			get
			{
				return _consumers.Values.Select(ConsumerImpl::getAvailablePermits).Sum();
			}
		}

		public override bool HasReachedEndOfTopic()
		{
			return _consumers.Values.All(Consumer::hasReachedEndOfTopic);
		}

		public virtual bool HasMessageAvailable()
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

		public virtual CompletableFuture<bool> HasMessageAvailableAsync()
		{
			IList<CompletableFuture<Void>> futureList = new List<CompletableFuture<Void>>();
			AtomicBoolean hasMessageAvailable = new AtomicBoolean(false);
			foreach(ConsumerImpl<T> consumer in _consumers.Values)
			{
				futureList.Add(consumer.HasMessageAvailableAsync().thenAccept(isAvailable =>
				{
				if(isAvailable)
				{
					hasMessageAvailable.compareAndSet(false, true);
				}
				}));
			}
			CompletableFuture<bool> completableFuture = new CompletableFuture<bool>();
			FutureUtil.WaitForAll(futureList).whenComplete((result, exception) =>
			{
			if(exception != null)
			{
				completableFuture.completeExceptionally(exception);
			}
			else
			{
				completableFuture.complete(hasMessageAvailable.get());
			}
			});
			return completableFuture;
		}

		public override int NumMessagesInQueue()
		{
			return IncomingMessages.size() + _consumers.Values.Select(ConsumerImpl::numMessagesInQueue).Sum();
		}

		public override ConsumerStats Stats
		{
			get
			{
				lock(this)
				{
					if(_stats == null)
					{
						return null;
					}
					_stats.Reset();
            
					_consumers.Values.ForEach(consumer => _stats.UpdateCumulativeStats(consumer.Stats));
					return _stats;
				}
			}
		}

		public virtual UnAckedMessageTracker UnAckedMessageTracker
		{
			get
			{
				return _unAckedMessageTracker;
			}
		}

		private void RemoveExpiredMessagesFromQueue(ISet<MessageId> messageIds)
		{
			Message<T> peek = IncomingMessages.peek();
			if(peek != null)
			{
				if(!messageIds.Contains(peek.MessageId))
				{
					// first message is not expired, then no message is expired in queue.
					return;
				}

				// try not to remove elements that are added while we remove
				Message<T> message = IncomingMessages.poll();
				checkState(message is TopicMessageImpl);
				while(message != null)
				{
					IncomingMessagesSizeUpdater.addAndGet(this, -message.Data.Length);
					MessageId messageId = message.MessageId;
					if(!messageIds.Contains(messageId))
					{
						messageIds.Add(messageId);
						break;
					}
					message = IncomingMessages.poll();
				}
			}
		}

		private TopicName GetTopicName(string topic)
		{
			try
			{
				return TopicName.Get(topic);
			}
			catch(Exception)
			{
				return null;
			}
		}

		private string GetFullTopicName(string topic)
		{
			TopicName topicName = GetTopicName(topic);
			return (topicName != null) ? topicName.ToString() : null;
		}

		private void RemoveTopic(string topic)
		{
			string fullTopicName = GetFullTopicName(topic);
			if(!string.ReferenceEquals(fullTopicName, null))
			{
				TopicsConflict.Remove(topic);
			}
		}

		// subscribe one more given topic
		public virtual CompletableFuture<Void> SubscribeAsync(string topicName, bool createTopicIfDoesNotExist)
		{
			TopicName topicNameInstance = GetTopicName(topicName);
			if(topicNameInstance == null)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Topic name not valid"));
			}
			string fullTopicName = topicNameInstance.ToString();
			if(TopicsConflict.ContainsKey(fullTopicName) || TopicsConflict.ContainsKey(topicNameInstance.PartitionedTopicName))
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Already subscribed to " + topicName));
			}

			if(State == State.Closing || State == State.Closed)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			CompletableFuture<Void> subscribeResult = new CompletableFuture<Void>();

			ClientConflict.GetPartitionedTopicMetadata(topicName).thenAccept(metadata => SubscribeTopicPartitions(subscribeResult, fullTopicName, metadata.partitions, createTopicIfDoesNotExist)).exceptionally(ex1 =>
			{
			_log.warn("[{}] Failed to get partitioned topic metadata: {}", fullTopicName, ex1.Message);
			subscribeResult.completeExceptionally(ex1);
			return null;
			});

			return subscribeResult;
		}

		// create consumer for a single topic with already known partitions.
		// first create a consumer with no topic, then do subscription for already know partitionedTopic.
		public static MultiTopicsConsumer<T> CreatePartitionedConsumer<T>(PulsarClientImpl client, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, CompletableFuture<ConsumerActor<T>> subscribeFuture, int numPartitions, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			checkArgument(conf.TopicNames.size() == 1, "Should have only 1 topic for partitioned consumer");

			// get topic name, then remove it from conf, so constructor will create a consumer with no topic.
			ConsumerConfigurationData cloneConf = conf.Clone();
			string topicName = cloneConf.SingleTopic;
			cloneConf.TopicNames.remove(topicName);

			CompletableFuture<ConsumerActor> future = new CompletableFuture<ConsumerActor>();
			MultiTopicsConsumer consumer = new MultiTopicsConsumerImpl(client, topicName, cloneConf, listenerExecutor, future, schema, interceptors, true);

			future.thenCompose(c => ((MultiTopicsConsumer)c).subscribeAsync(topicName, numPartitions)).thenRun(() => subscribeFuture.complete(consumer)).exceptionally(e =>
			{
			_log.warn("Failed subscription for createPartitionedConsumer: {} {}, e:{}", topicName, numPartitions, e);
			subscribeFuture.completeExceptionally(PulsarClientException.Wrap(((Exception) e).InnerException, string.Format("Failed to subscribe {0} with {1:D} partitions", topicName, numPartitions)));
			return null;
			});
			return consumer;
		}

		internal virtual CompletableFuture<Void> SubscribeAsync(string topicName, int numberPartitions)
		{
			TopicName topicNameInstance = GetTopicName(topicName);
			if(topicNameInstance == null)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Topic name not valid"));
			}
			string fullTopicName = topicNameInstance.ToString();
			if(TopicsConflict.ContainsKey(fullTopicName) || TopicsConflict.ContainsKey(topicNameInstance.PartitionedTopicName))
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Already subscribed to " + topicName));
			}

			if(State == State.Closing || State == State.Closed)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			CompletableFuture<Void> subscribeResult = new CompletableFuture<Void>();
			SubscribeTopicPartitions(subscribeResult, fullTopicName, numberPartitions, true);

			return subscribeResult;
		}

		private void SubscribeTopicPartitions(CompletableFuture<Void> subscribeResult, string topicName, int numPartitions, bool createIfDoesNotExist)
		{
			ClientConflict.PreProcessSchemaBeforeSubscribe(ClientConflict, Schema, topicName).whenComplete((Schema, cause) =>
			{
			if(null == cause)
			{
				DoSubscribeTopicPartitions(Schema, subscribeResult, topicName, numPartitions, createIfDoesNotExist);
			}
			else
			{
				subscribeResult.completeExceptionally(cause);
			}
			});
		}

		private void DoSubscribeTopicPartitions(Schema<T> schema, CompletableFuture<Void> subscribeResult, string topicName, int numPartitions, bool createIfDoesNotExist)
		{
			if(_log.DebugEnabled)
			{
				_log.debug("Subscribe to topic {} metadata.partitions: {}", topicName, numPartitions);
			}

			IList<CompletableFuture<ConsumerActor<T>>> futureList;
			if(numPartitions > 0)
			{
				// Below condition is true if subscribeAsync() has been invoked second time with same
				// topicName before the first invocation had reached this point.
				bool isTopicBeingSubscribedForInOtherThread = this.TopicsConflict.GetOrAdd(topicName, numPartitions) != null;
				if(isTopicBeingSubscribedForInOtherThread)
				{
					string errorMessage = string.Format("[{0}] Failed to subscribe for topic [{1}] in topics consumer. " + "Topic is already being subscribed for in other thread.", Topic, topicName);
					_log.warn(errorMessage);
					subscribeResult.completeExceptionally(new PulsarClientException(errorMessage));
					return;
				}
				AllTopicPartitionsNumber.addAndGet(numPartitions);

				int receiverQueueSize = Math.Min(Conf.ReceiverQueueSize, Conf.MaxTotalReceiverQueueSizeAcrossPartitions / numPartitions);
				ConsumerConfigurationData<T> configurationData = InternalConsumerConfig;
				configurationData.ReceiverQueueSize = receiverQueueSize;

				futureList = IntStream.range(0, numPartitions).mapToObj(partitionIndex =>
				{
				string partitionName = TopicName.Get(topicName).getPartition(partitionIndex).ToString();
				CompletableFuture<ConsumerActor<T>> subFuture = new CompletableFuture<ConsumerActor<T>>();
				ConsumerImpl<T> newConsumer = ConsumerImpl.NewConsumerImpl(ClientConflict, partitionName, configurationData, ClientConflict.ExternalExecutorProvider().Executor, partitionIndex, true, subFuture, _startMessageId, schema, Interceptors, createIfDoesNotExist, _startMessageRollbackDurationInSec);
				_consumers.GetOrAdd(newConsumer.Topic, newConsumer);
				return subFuture;
				}).collect(Collectors.toList());
			}
			else
			{
				bool isTopicBeingSubscribedForInOtherThread = this.TopicsConflict.GetOrAdd(topicName, 1) != null;
				if(isTopicBeingSubscribedForInOtherThread)
				{
					string errorMessage = string.Format("[{0}] Failed to subscribe for topic [{1}] in topics consumer. " + "Topic is already being subscribed for in other thread.", Topic, topicName);
					_log.warn(errorMessage);
					subscribeResult.completeExceptionally(new PulsarClientException(errorMessage));
					return;
				}
				AllTopicPartitionsNumber.incrementAndGet();

				CompletableFuture<ConsumerActor<T>> subFuture = new CompletableFuture<ConsumerActor<T>>();
				ConsumerImpl<T> newConsumer = ConsumerImpl.NewConsumerImpl(ClientConflict, topicName, _internalConfig, ClientConflict.ExternalExecutorProvider().Executor, -1, true, subFuture, null, schema, Interceptors, createIfDoesNotExist);
				_consumers.GetOrAdd(newConsumer.Topic, newConsumer);

				futureList = Collections.singletonList(subFuture);
			}

			FutureUtil.WaitForAll(futureList).thenAccept(finalFuture =>
			{
			if(AllTopicPartitionsNumber.get() > MaxReceiverQueueSizeConflict)
			{
				MaxReceiverQueueSize = AllTopicPartitionsNumber.get();
			}
			int numTopics = this.TopicsConflict.Values.Select(int.intValue).Sum();
			int currentAllTopicsPartitionsNumber = AllTopicPartitionsNumber.get();
			checkState(currentAllTopicsPartitionsNumber == numTopics, "allTopicPartitionsNumber " + currentAllTopicsPartitionsNumber + " not equals expected: " + numTopics);
			StartReceivingMessages(_consumers.Values.Where(consumer1 =>
			{
				string consumerTopicName = consumer1.Topic;
				return TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(TopicName.Get(topicName).PartitionedTopicName);
			}).ToList());
			subscribeResult.complete(null);
			_log.info("[{}] [{}] Success subscribe new topic {} in topics consumer, partitions: {}, allTopicPartitionsNumber: {}", Topic, SubscriptionConflict, topicName, numPartitions, AllTopicPartitionsNumber.get());
			return;
			}).exceptionally(ex =>
			{
			HandleSubscribeOneTopicError(topicName, ex, subscribeResult);
			return null;
		});
		}

		// handling failure during subscribe new topic, unsubscribe success created partitions
		private void HandleSubscribeOneTopicError(string topicName, Exception error, CompletableFuture<Void> subscribeFuture)
		{
			_log.warn("[{}] Failed to subscribe for topic [{}] in topics consumer {}", Topic, topicName, error.Message);
			ClientConflict.ExternalExecutorProvider().Executor.submit(() =>
			{
			AtomicInteger toCloseNum = new AtomicInteger(0);
			_consumers.Values.Where(consumer1 =>
			{
				string consumerTopicName = consumer1.Topic;
				if(TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(TopicName.Get(topicName).PartitionedTopicName))
				{
					toCloseNum.incrementAndGet();
					return true;
				}
				else
				{
					return false;
				}
			}).ToList().ForEach(consumer2 =>
			{
				consumer2.closeAsync().whenComplete((r, ex) =>
				{
					consumer2.subscribeFuture().completeExceptionally(error);
					AllTopicPartitionsNumber.decrementAndGet();
					_consumers.Remove(consumer2.Topic);
					if(toCloseNum.decrementAndGet() == 0)
					{
						_log.warn("[{}] Failed to subscribe for topic [{}] in topics consumer, subscribe error: {}", Topic, topicName, error.Message);
						RemoveTopic(topicName);
						subscribeFuture.completeExceptionally(error);
					}
					return;
				});
			});
			});
		}

		// un-subscribe a given topic
		public virtual CompletableFuture<Void> UnsubscribeAsync(string topicName)
		{
			checkArgument(TopicName.IsValid(topicName), "Invalid topic name:" + topicName);

			if(State == State.Closing || State == State.Closed)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			if(_partitionsAutoUpdateTimeout != null)
			{
				_partitionsAutoUpdateTimeout.cancel();
				_partitionsAutoUpdateTimeout = null;
			}

			CompletableFuture<Void> unsubscribeFuture = new CompletableFuture<Void>();
			string topicPartName = TopicName.Get(topicName).PartitionedTopicName;

			IList<ConsumerImpl<T>> consumersToUnsub = _consumers.Values.Where(consumer =>
			{
			string consumerTopicName = consumer.Topic;
			return TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(topicPartName);
			}).ToList();

			IList<CompletableFuture<Void>> futureList = consumersToUnsub.Select(ConsumerImpl::unsubscribeAsync).ToList();

			FutureUtil.WaitForAll(futureList).whenComplete((r, ex) =>
			{
			if(ex == null)
			{
				consumersToUnsub.ForEach(consumer1 =>
				{
					_consumers.Remove(consumer1.Topic);
					_pausedConsumers.remove(consumer1);
					AllTopicPartitionsNumber.decrementAndGet();
				});
				RemoveTopic(topicName);
				((UnAckedTopicMessageTracker) _unAckedMessageTracker).RemoveTopicMessages(topicName);
				unsubscribeFuture.complete(null);
				_log.info("[{}] [{}] [{}] Unsubscribed Topics Consumer, allTopicPartitionsNumber: {}", topicName, SubscriptionConflict, ConsumerNameConflict, AllTopicPartitionsNumber);
			}
			else
			{
				unsubscribeFuture.completeExceptionally(ex);
				State = State.Failed;
				_log.error("[{}] [{}] [{}] Could not unsubscribe Topics Consumer", topicName, SubscriptionConflict, ConsumerNameConflict, ex.Cause);
			}
			});

			return unsubscribeFuture;
		}

		// Remove a consumer for a topic
		public virtual CompletableFuture<Void> RemoveConsumerAsync(string topicName)
		{
			checkArgument(TopicName.IsValid(topicName), "Invalid topic name:" + topicName);

			if(State == State.Closing || State == State.Closed)
			{
				return FutureUtil.FailedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			CompletableFuture<Void> unsubscribeFuture = new CompletableFuture<Void>();
			string topicPartName = TopicName.Get(topicName).PartitionedTopicName;


			IList<ConsumerImpl<T>> consumersToClose = _consumers.Values.Where(consumer =>
			{
			string consumerTopicName = consumer.Topic;
			return TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(topicPartName);
			}).ToList();

			IList<CompletableFuture<Void>> futureList = consumersToClose.Select(ConsumerImpl::closeAsync).ToList();

			FutureUtil.WaitForAll(futureList).whenComplete((r, ex) =>
			{
			if(ex == null)
			{
				consumersToClose.ForEach(consumer1 =>
				{
					_consumers.Remove(consumer1.Topic);
					_pausedConsumers.remove(consumer1);
					AllTopicPartitionsNumber.decrementAndGet();
				});
				RemoveTopic(topicName);
				((UnAckedTopicMessageTracker) _unAckedMessageTracker).RemoveTopicMessages(topicName);
				unsubscribeFuture.complete(null);
				_log.info("[{}] [{}] [{}] Removed Topics Consumer, allTopicPartitionsNumber: {}", topicName, SubscriptionConflict, ConsumerNameConflict, AllTopicPartitionsNumber);
			}
			else
			{
				unsubscribeFuture.completeExceptionally(ex);
				State = State.Failed;
				_log.error("[{}] [{}] [{}] Could not remove Topics Consumer", topicName, SubscriptionConflict, ConsumerNameConflict, ex.Cause);
			}
			});

			return unsubscribeFuture;
		}


		// get topics name
		public virtual IList<string> Topics
		{
			get
			{
				return TopicsConflict.Keys.ToList();
			}
		}

		// get partitioned topics name
		public virtual IList<string> PartitionedTopics
		{
			get
			{
				return _consumers.Keys.ToList();
			}
		}

		// get partitioned consumers
		public virtual IList<ConsumerImpl<T>> Consumers
		{
			get
			{
				return _consumers.Values.ToList();
			}
		}

		public override void Pause()
		{
			lock(_pauseMutex)
			{
				_paused = true;
				_consumers.forEach((name, consumer) => consumer.pause());
			}
		}

		public override void Resume()
		{
			lock(_pauseMutex)
			{
				_paused = false;
				_consumers.forEach((name, consumer) => consumer.resume());
			}
		}

		public override long LastDisconnectedTimestamp
		{
			get
			{
				long lastDisconnectedTimestamp = 0;
				Optional<ConsumerImpl<T>> c = _consumers.Values.Max(System.Collections.IComparer.comparingLong(ConsumerImpl::getLastDisconnectedTimestamp));
				if(c.Present)
				{
					lastDisconnectedTimestamp = c.get().LastDisconnectedTimestamp;
				}
				return lastDisconnectedTimestamp;
			}
		}

		// This listener is triggered when topics partitions are updated.
		private class TopicsPartitionChangedListener : IPartitionsChangedListener
		{
			private readonly MultiTopicsConsumer<T> _outerInstance;

			public TopicsPartitionChangedListener(MultiTopicsConsumer<T> outerInstance)
			{
				this._outerInstance = outerInstance;
			}

			// Check partitions changes of passed in topics, and subscribe new added partitions.
			public virtual void OnTopicsExtended(ICollection<string> topicsExtended)
			{
				CompletableFuture<Void> future = new CompletableFuture<Void>();
				if(topicsExtended.Count == 0)
				{
					future.complete(null);
					return future;
				}

				if(_log.DebugEnabled)
				{
					_log.debug("[{}]  run onTopicsExtended: {}, size: {}", outerInstance.Topic, topicsExtended.ToString(), topicsExtended.Count);
				}

				IList<CompletableFuture<Void>> futureList = Lists.newArrayListWithExpectedSize(topicsExtended.Count);
				topicsExtended.forEach(outerInstance.Topic => futureList.Add(outerInstance.SubscribeIncreasedTopicPartitions(outerInstance.Topic)));
				FutureUtil.WaitForAll(futureList).thenAccept(finalFuture => future.complete(null)).exceptionally(ex =>
				{
				_log.warn("[{}] Failed to subscribe increased topics partitions: {}", outerInstance.Topic, ex.Message);
				future.completeExceptionally(ex);
				return null;
				});

				return future;
			}
		}

		// subscribe increased partitions for a given topic
		private CompletableFuture<Void> SubscribeIncreasedTopicPartitions(string topicName)
		{
			CompletableFuture<Void> future = new CompletableFuture<Void>();

			ClientConflict.GetPartitionsForTopic(topicName).thenCompose(list =>
			{
			int oldPartitionNumber = TopicsConflict.GetValueOrNull(topicName);
			int currentPartitionNumber = list.size();
			if(_log.DebugEnabled)
			{
				_log.debug("[{}] partitions number. old: {}, new: {}", topicName, oldPartitionNumber, currentPartitionNumber);
			}
			if(oldPartitionNumber == currentPartitionNumber)
			{
				future.complete(null);
				return future;
			}
			else if(oldPartitionNumber < currentPartitionNumber)
			{
				AllTopicPartitionsNumber.compareAndSet(oldPartitionNumber, currentPartitionNumber);
				IList<string> newPartitions = list.subList(oldPartitionNumber, currentPartitionNumber);
				IList<CompletableFuture<ConsumerActor<T>>> futureList = newPartitions.Select(partitionName =>
				{
					int partitionIndex = TopicName.GetPartitionIndex(partitionName);
					CompletableFuture<ConsumerActor<T>> subFuture = new CompletableFuture<ConsumerActor<T>>();
					ConsumerConfigurationData<T> configurationData = InternalConsumerConfig;
					ConsumerImpl<T> newConsumer = ConsumerImpl.NewConsumerImpl(ClientConflict, partitionName, configurationData, ClientConflict.ExternalExecutorProvider().Executor, partitionIndex, true, subFuture, null, Schema, Interceptors, true);
					lock(_pauseMutex)
					{
						if(_paused)
						{
							newConsumer.Pause();
						}
						_consumers.GetOrAdd(newConsumer.Topic, newConsumer);
					}
					if(_log.DebugEnabled)
					{
						_log.debug("[{}] create consumer {} for partitionName: {}", topicName, newConsumer.Topic, partitionName);
					}
					return subFuture;
				}).ToList();
				FutureUtil.WaitForAll(futureList).thenAccept(finalFuture =>
				{
					IList<ConsumerImpl<T>> newConsumerList = newPartitions.Select(partitionTopic => _consumers.GetValueOrNull(partitionTopic)).ToList();
					StartReceivingMessages(newConsumerList);
					future.complete(null);
				}).exceptionally(ex =>
				{
					_log.warn("[{}] Failed to subscribe {} partition: {} - {} : {}", Topic, topicName, oldPartitionNumber, currentPartitionNumber, ex);
					future.completeExceptionally(ex);
					return null;
				});
			}
			else
			{
				_log.error("[{}] not support shrink topic partitions. old: {}, new: {}", topicName, oldPartitionNumber, currentPartitionNumber);
				future.completeExceptionally(new PulsarClientException.NotSupportedException("not support shrink topic partitions"));
			}
			return future;
			});

			return future;
		}

		private TimerTask _partitionsAutoUpdateTimerTask = new TimerTaskAnonymousInnerClass();

		private class TimerTaskAnonymousInnerClass : TimerTask
		{
			public override void Run(Timeout timeout)
			{
				if(timeout.Cancelled || outerInstance.State != State.Ready)
				{
					return;
				}

				if(_log.DebugEnabled)
				{
					_log.debug("[{}] run partitionsAutoUpdateTimerTask", outerInstance.Topic);
				}

				// if last auto update not completed yet, do nothing.
				if(outerInstance.PartitionsAutoUpdateFuture == null || outerInstance.PartitionsAutoUpdateFuture.Done)
				{
					outerInstance.PartitionsAutoUpdateFuture = outerInstance.TopicsPartitionChangedListener.onTopicsExtended(outerInstance.TopicsConflict.Keys);
				}

				// schedule the next re-check task
				outerInstance._partitionsAutoUpdateTimeout = outerInstance.ClientConflict.timer().newTimeout(outerInstance._partitionsAutoUpdateTimerTask, outerInstance.Conf.AutoUpdatePartitionsIntervalSeconds, TimeUnit.SECONDS);
			}
		}

		public virtual Timeout PartitionsAutoUpdateTimeout
		{
			get
			{
				return _partitionsAutoUpdateTimeout;
			}
		}

		public override CompletableFuture<MessageId> LastMessageIdAsync
		{
			get
			{
				CompletableFuture<MessageId> returnFuture = new CompletableFuture<MessageId>();
    
				IDictionary<string, CompletableFuture<MessageId>> messageIdFutures = _consumers.SetOfKeyValuePairs().Select(entry => Pair.of(entry.Key,entry.Value.LastMessageIdAsync)).ToDictionary(Pair.getKey, Pair.getValue);
    
				CompletableFuture.allOf(messageIdFutures.SetOfKeyValuePairs().Select(DictionaryEntry.getValue).ToArray(CompletableFuture<object>[]::new)).whenComplete((ignore, ex) =>
				{
				ImmutableMap.Builder<string, MessageId> builder = ImmutableMap.builder();
				messageIdFutures.forEach((key, future) =>
				{
					MessageId messageId;
					try
					{
						messageId = future.get();
					}
					catch(Exception e)
					{
						_log.warn("[{}] Exception when topic {} getLastMessageId.", key, e);
						messageId = MessageId.earliest;
					}
					builder.put(key, messageId);
				});
				returnFuture.complete(new MultiMessageIdImpl(builder.build()));
				});
    
				return returnFuture;
			}
		}

		private static readonly Logger _log = LoggerFactory.getLogger(typeof(MultiTopicsConsumer));

		public static bool IsIllegalMultiTopicsMessageId(MessageId messageId)
		{
			//only support earliest/latest
			return !MessageId.earliest.Equals(messageId) && !MessageId.latest.Equals(messageId);
		}

		public virtual void TryAcknowledgeMessage(Message<T> msg)
		{
			if(msg != null)
			{
				AcknowledgeCumulativeAsync(msg);
			}
		}
	}

}