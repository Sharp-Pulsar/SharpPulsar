using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkState;

	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;
	using ImmutableMap = com.google.common.collect.ImmutableMap;
	using Builder = com.google.common.collect.ImmutableMap.Builder;
	using Lists = com.google.common.collect.Lists;

	using Queues = com.google.common.collect.Queues;
	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;

	using Pair = org.apache.commons.lang3.tuple.Pair;
	using Consumer = org.apache.pulsar.client.api.Consumer;
	using ConsumerStats = org.apache.pulsar.client.api.ConsumerStats;
	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using Messages = org.apache.pulsar.client.api.Messages;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using NotSupportedException = org.apache.pulsar.client.api.PulsarClientException.NotSupportedException;
	using Schema = org.apache.pulsar.client.api.Schema;
	using SubscriptionType = org.apache.pulsar.client.api.SubscriptionType;
	using SubscriptionMode = SharpPulsar.Impl.ConsumerImpl.SubscriptionMode;
	using SharpPulsar.Impl.conf;
	using TransactionImpl = SharpPulsar.Impl.transaction.TransactionImpl;
	using ConsumerName = org.apache.pulsar.client.util.ConsumerName;
	using AckType = org.apache.pulsar.common.api.proto.PulsarApi.CommandAck.AckType;
	using NamespaceName = org.apache.pulsar.common.naming.NamespaceName;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class MultiTopicsConsumerImpl<T> : ConsumerBase<T>
	{

		public const string DUMMY_TOPIC_NAME_PREFIX = "MultiTopicsConsumer-";

		// All topics should be in same namespace
		protected internal NamespaceName namespaceName;

		// Map <topic+partition, consumer>, when get do ACK, consumer will by find by topic name
		private readonly ConcurrentDictionary<string, ConsumerImpl<T>> consumers;

		// Map <topic, numPartitions>, store partition number for each topic
		protected internal readonly ConcurrentDictionary<string, int> topics;

		// Queue of partition consumers on which we have stopped calling receiveAsync() because the
		// shared incoming queue was full
		private readonly ConcurrentLinkedQueue<ConsumerImpl<T>> pausedConsumers;

		// Threshold for the shared queue. When the size of the shared queue goes below the threshold, we are going to
		// resume receiving from the paused consumer partitions
		private readonly int sharedQueueResumeThreshold;

		// sum of topicPartitions, simple topic has 1, partitioned topic equals to partition number.
		internal AtomicInteger allTopicPartitionsNumber;

		// timeout related to auto check and subscribe partition increasement
		private volatile Timeout partitionsAutoUpdateTimeout = null;
		internal TopicsPartitionChangedListener topicsPartitionChangedListener;
		internal CompletableFuture<Void> partitionsAutoUpdateFuture = null;

		private readonly ReadWriteLock @lock = new ReentrantReadWriteLock();
		private readonly ConsumerStatsRecorder stats;
		private readonly UnAckedMessageTracker unAckedMessageTracker;
		private readonly ConsumerConfigurationData<T> internalConfig;

		internal MultiTopicsConsumerImpl(PulsarClientImpl client, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, CompletableFuture<Consumer<T>> subscribeFuture, Schema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist) : this(client, DUMMY_TOPIC_NAME_PREFIX + ConsumerName.generateRandomName(), conf, listenerExecutor, subscribeFuture, schema, interceptors, createTopicIfDoesNotExist)
		{
		}

		internal MultiTopicsConsumerImpl(PulsarClientImpl client, string singleTopic, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, CompletableFuture<Consumer<T>> subscribeFuture, Schema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist) : base(client, singleTopic, conf, Math.Max(2, conf.ReceiverQueueSize), listenerExecutor, subscribeFuture, schema, interceptors)
		{

			checkArgument(conf.ReceiverQueueSize > 0, "Receiver queue size needs to be greater than 0 for Topics Consumer");

			this.topics = new ConcurrentDictionary<string, int>();
			this.consumers = new ConcurrentDictionary<string, ConsumerImpl<T>>();
			this.pausedConsumers = new ConcurrentLinkedQueue<ConsumerImpl<T>>();
			this.sharedQueueResumeThreshold = maxReceiverQueueSize / 2;
			this.allTopicPartitionsNumber = new AtomicInteger(0);

			if (conf.AckTimeoutMillis != 0)
			{
				if (conf.TickDurationMillis > 0)
				{
					this.unAckedMessageTracker = new UnAckedTopicMessageTracker(client, this, conf.AckTimeoutMillis, conf.TickDurationMillis);
				}
				else
				{
					this.unAckedMessageTracker = new UnAckedTopicMessageTracker(client, this, conf.AckTimeoutMillis);
				}
			}
			else
			{
				this.unAckedMessageTracker = UnAckedMessageTracker.UNACKED_MESSAGE_TRACKER_DISABLED;
			}

			this.internalConfig = InternalConsumerConfig;
			this.stats = client.Configuration.StatsIntervalSeconds > 0 ? new ConsumerStatsRecorderImpl() : null;

			// start track and auto subscribe partition increasement
			if (conf.AutoUpdatePartitions)
			{
				topicsPartitionChangedListener = new TopicsPartitionChangedListener(this);
				partitionsAutoUpdateTimeout = client.timer().newTimeout(partitionsAutoUpdateTimerTask, 1, TimeUnit.MINUTES);
			}

			if (conf.TopicNames.Empty)
			{
				this.namespaceName = null;
				State = State.Ready;
				subscribeFuture().complete(MultiTopicsConsumerImpl.this);
				return;
			}

			checkArgument(conf.TopicNames.Empty || topicNamesValid(conf.TopicNames), "Topics should have same namespace.");
			this.namespaceName = conf.TopicNames.First().flatMap(s => (TopicName.get(s).NamespaceObject)).get();

			IList<CompletableFuture<Void>> futures = conf.TopicNames.Select(t => subscribeAsync(t, createTopicIfDoesNotExist)).ToList();
			FutureUtil.waitForAll(futures).thenAccept(finalFuture =>
			{
			if (allTopicPartitionsNumber.get() > maxReceiverQueueSize)
			{
				MaxReceiverQueueSize = allTopicPartitionsNumber.get();
			}
			State = State.Ready;
			startReceivingMessages(new List<ConsumerImpl<T>>(consumers.Values));
			log.info("[{}] [{}] Created topics consumer with {} sub-consumers", topic, subscription, allTopicPartitionsNumber.get());
			subscribeFuture().complete(MultiTopicsConsumerImpl.this);
			}).exceptionally(ex =>
			{
			log.warn("[{}] Failed to subscribe topics: {}", topic, ex.Message);
			subscribeFuture.completeExceptionally(ex);
			return null;
		});
		}

		// Check topics are valid.
		// - each topic is valid,
		// - every topic has same namespace,
		// - topic names are unique.
		private static bool topicNamesValid(ICollection<string> topics)
		{
			checkState(topics != null && topics.Count >= 1, "topics should contain more than 1 topic");

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String namespace = org.apache.pulsar.common.naming.TopicName.get(topics.stream().findFirst().get()).getNamespace();
			string @namespace = TopicName.get(topics.First().get()).Namespace;

			Optional<string> result = topics.Where(topic =>
			{
			bool topicInvalid = !TopicName.isValid(topic);
			if (topicInvalid)
			{
				return true;
			}
			string newNamespace = TopicName.get(topic).Namespace;
			if (!@namespace.Equals(newNamespace))
			{
				return true;
			}
			else
			{
				return false;
			}
			}).First();

			if (result.Present)
			{
				log.warn("Received invalid topic name: {}", result.get());
				return false;
			}

			// check topic names are unique
			HashSet<string> set = new HashSet<string>(topics);
			if (set.Count == topics.Count)
			{
				return true;
			}
			else
			{
				log.warn("Topic names not unique. unique/all : {}/{}", set.Count, topics.Count);
				return false;
			}
		}

		private void startReceivingMessages(IList<ConsumerImpl<T>> newConsumers)
		{
			if (log.DebugEnabled)
			{
				log.debug("[{}] startReceivingMessages for {} new consumers in topics consumer, state: {}", topic, newConsumers.Count, State);
			}
			if (State == State.Ready)
			{
				newConsumers.ForEach(consumer =>
				{
				consumer.sendFlowPermitsToBroker(consumer.ConnectionHandler.cnx(), conf.ReceiverQueueSize);
				receiveMessageFromConsumer(consumer);
				});
			}
		}

		private void receiveMessageFromConsumer(ConsumerImpl<T> consumer)
		{
			consumer.receiveAsync().thenAccept(message =>
			{
			if (log.DebugEnabled)
			{
				log.debug("[{}] [{}] Receive message from sub consumer:{}", topic, subscription, consumer.Topic);
			}
			messageReceived(consumer, message);
			@lock.writeLock().@lock();
			try
			{
				int size = incomingMessages.size();
				if (size >= maxReceiverQueueSize || (size > sharedQueueResumeThreshold && !pausedConsumers.Empty))
				{
					pausedConsumers.add(consumer);
				}
				else
				{
					client.eventLoopGroup().execute(() =>
					{
						receiveMessageFromConsumer(consumer);
					});
				}
			}
			finally
			{
				@lock.writeLock().unlock();
			}
			});
		}

		private void messageReceived(ConsumerImpl<T> consumer, Message<T> message)
		{
			checkArgument(message is MessageImpl);
			@lock.writeLock().@lock();
			try
			{
				TopicMessageImpl<T> topicMessage = new TopicMessageImpl<T>(consumer.Topic, consumer.TopicNameWithoutPartition, message);

				if (log.DebugEnabled)
				{
					log.debug("[{}][{}] Received message from topics-consumer {}", topic, subscription, message.MessageId);
				}

				// if asyncReceive is waiting : return message to callback without adding to incomingMessages queue
				if (!pendingReceives.Empty)
				{
					CompletableFuture<Message<T>> receivedFuture = pendingReceives.poll();
					unAckedMessageTracker.add(topicMessage.MessageId);
					listenerExecutor.execute(() => receivedFuture.complete(topicMessage));
				}
				else if (enqueueMessageAndCheckBatchReceive(topicMessage))
				{
					if (hasPendingBatchReceive())
					{
						notifyPendingBatchReceivedCallBack();
					}
				}
			}
			finally
			{
				@lock.writeLock().unlock();
			}

			if (listener != null)
			{
				// Trigger the notification on the message listener in a separate thread to avoid blocking the networking
				// thread while the message processing happens
				listenerExecutor.execute(() =>
				{
				Message<T> msg;
				try
				{
					msg = internalReceive();
				}
				catch (PulsarClientException e)
				{
					log.warn("[{}] [{}] Failed to dequeue the message for listener", topic, subscription, e);
					return;
				}
				try
				{
					if (log.DebugEnabled)
					{
						log.debug("[{}][{}] Calling message listener for message {}", topic, subscription, message.MessageId);
					}
					listener.received(MultiTopicsConsumerImpl.this, msg);
				}
				catch (Exception t)
				{
					log.error("[{}][{}] Message listener error in processing message: {}", topic, subscription, message, t);
				}
				});
			}
		}

		protected internal override void messageProcessed<T1>(Message<T1> msg)
		{
			lock (this)
			{
				unAckedMessageTracker.add(msg.MessageId);
				INCOMING_MESSAGES_SIZE_UPDATER.addAndGet(this, -msg.Data.length);
			}
		}

		private void resumeReceivingFromPausedConsumersIfNeeded()
		{
			@lock.readLock().@lock();
			try
			{
				if (incomingMessages.size() <= sharedQueueResumeThreshold && !pausedConsumers.Empty)
				{
					while (true)
					{
						ConsumerImpl<T> consumer = pausedConsumers.poll();
						if (consumer == null)
						{
							break;
						}

						// if messages are readily available on consumer we will attempt to writeLock on the same thread
						client.eventLoopGroup().execute(() =>
						{
						receiveMessageFromConsumer(consumer);
						});
					}
				}
			}
			finally
			{
				@lock.readLock().unlock();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected org.apache.pulsar.client.api.Message<T> internalReceive() throws org.apache.pulsar.client.api.PulsarClientException
		protected internal override Message<T> internalReceive()
		{
			Message<T> message;
			try
			{
				message = incomingMessages.take();
				INCOMING_MESSAGES_SIZE_UPDATER.addAndGet(this, -message.Data.length);
				checkState(message is TopicMessageImpl);
				unAckedMessageTracker.add(message.MessageId);
				resumeReceivingFromPausedConsumersIfNeeded();
				return message;
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected org.apache.pulsar.client.api.Message<T> internalReceive(int timeout, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.api.PulsarClientException
		protected internal override Message<T> internalReceive(int timeout, TimeUnit unit)
		{
			Message<T> message;
			try
			{
				message = incomingMessages.poll(timeout, unit);
				if (message != null)
				{
					INCOMING_MESSAGES_SIZE_UPDATER.addAndGet(this, -message.Data.length);
					checkArgument(message is TopicMessageImpl);
					unAckedMessageTracker.add(message.MessageId);
				}
				resumeReceivingFromPausedConsumersIfNeeded();
				return message;
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected org.apache.pulsar.client.api.Messages<T> internalBatchReceive() throws org.apache.pulsar.client.api.PulsarClientException
		protected internal override Messages<T> internalBatchReceive()
		{
			try
			{
				return internalBatchReceiveAsync().get();
			}
			catch (Exception e) when (e is InterruptedException || e is ExecutionException)
			{
				State state = State;
				if (state != State.Closing && state != State.Closed)
				{
					stats.incrementNumBatchReceiveFailed();
					throw PulsarClientException.unwrap(e);
				}
				else
				{
					return null;
				}
			}
		}

		protected internal override CompletableFuture<Messages<T>> internalBatchReceiveAsync()
		{
			CompletableFuture<Messages<T>> result = new CompletableFuture<Messages<T>>();
			try
			{
				@lock.writeLock().@lock();
				if (pendingBatchReceives == null)
				{
					pendingBatchReceives = Queues.newConcurrentLinkedQueue();
				}
				if (hasEnoughMessagesForBatchReceive())
				{
					MessagesImpl<T> messages = NewMessagesImpl;
					Message<T> msgPeeked = incomingMessages.peek();
					while (msgPeeked != null && messages.canAdd(msgPeeked))
					{
						Message<T> msg = incomingMessages.poll();
						if (msg != null)
						{
							INCOMING_MESSAGES_SIZE_UPDATER.addAndGet(this, -msg.Data.length);
							Message<T> interceptMsg = beforeConsume(msg);
							messages.add(interceptMsg);
						}
						msgPeeked = incomingMessages.peek();
					}
					result.complete(messages);
				}
				else
				{
					pendingBatchReceives.add(OpBatchReceive.of(result));
				}
			}
			finally
			{
				@lock.writeLock().unlock();
			}
			return result;
		}

		protected internal override CompletableFuture<Message<T>> internalReceiveAsync()
		{
			CompletableFuture<Message<T>> result = new CompletableFuture<Message<T>>();
			Message<T> message;
			try
			{
				@lock.writeLock().@lock();
				message = incomingMessages.poll(0, TimeUnit.SECONDS);
				if (message == null)
				{
					pendingReceives.add(result);
				}
				else
				{
					INCOMING_MESSAGES_SIZE_UPDATER.addAndGet(this, -message.Data.length);
					checkState(message is TopicMessageImpl);
					unAckedMessageTracker.add(message.MessageId);
					resumeReceivingFromPausedConsumersIfNeeded();
					result.complete(message);
				}
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				result.completeExceptionally(new PulsarClientException(e));
			}
			finally
			{
				@lock.writeLock().unlock();
			}

			return result;
		}

		protected internal override CompletableFuture<Void> doAcknowledge(MessageId messageId, AckType ackType, IDictionary<string, long> properties, TransactionImpl txnImpl)
		{
			checkArgument(messageId is TopicMessageIdImpl);
			TopicMessageIdImpl topicMessageId = (TopicMessageIdImpl) messageId;

			if (State != State.Ready)
			{
				return FutureUtil.failedFuture(new PulsarClientException("Consumer already closed"));
			}

			if (ackType == AckType.Cumulative)
			{
				Consumer individualConsumer = consumers[topicMessageId.TopicPartitionName];
				if (individualConsumer != null)
				{
					MessageId innerId = topicMessageId.InnerMessageId;
					return individualConsumer.acknowledgeCumulativeAsync(innerId);
				}
				else
				{
					return FutureUtil.failedFuture(new PulsarClientException.NotConnectedException());
				}
			}
			else
			{
				ConsumerImpl<T> consumer = consumers[topicMessageId.TopicPartitionName];

				MessageId innerId = topicMessageId.InnerMessageId;
				return consumer.doAcknowledgeWithTxn(innerId, ackType, properties, txnImpl).thenRun(() => unAckedMessageTracker.remove(topicMessageId));
			}
		}

		public override void negativeAcknowledge(MessageId messageId)
		{
			checkArgument(messageId is TopicMessageIdImpl);
			TopicMessageIdImpl topicMessageId = (TopicMessageIdImpl) messageId;

			ConsumerImpl<T> consumer = consumers[topicMessageId.TopicPartitionName];
			consumer.negativeAcknowledge(topicMessageId.InnerMessageId);
		}

		public override CompletableFuture<Void> unsubscribeAsync()
		{
			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}
			State = State.Closing;

			CompletableFuture<Void> unsubscribeFuture = new CompletableFuture<Void>();
			IList<CompletableFuture<Void>> futureList = consumers.Values.Select(c => c.unsubscribeAsync()).ToList();

			FutureUtil.waitForAll(futureList).whenComplete((r, ex) =>
			{
			if (ex == null)
			{
				State = State.Closed;
				unAckedMessageTracker.Dispose();
				unsubscribeFuture.complete(null);
				log.info("[{}] [{}] [{}] Unsubscribed Topics Consumer", topic, subscription, consumerName);
			}
			else
			{
				State = State.Failed;
				unsubscribeFuture.completeExceptionally(ex);
				log.error("[{}] [{}] [{}] Could not unsubscribe Topics Consumer", topic, subscription, consumerName, ex.Cause);
			}
			});

			return unsubscribeFuture;
		}

		public override CompletableFuture<Void> closeAsync()
		{
			if (State == State.Closing || State == State.Closed)
			{
				unAckedMessageTracker.Dispose();
				return CompletableFuture.completedFuture(null);
			}
			State = State.Closing;

			if (partitionsAutoUpdateTimeout != null)
			{
				partitionsAutoUpdateTimeout.cancel();
				partitionsAutoUpdateTimeout = null;
			}

			CompletableFuture<Void> closeFuture = new CompletableFuture<Void>();
			IList<CompletableFuture<Void>> futureList = consumers.Values.Select(c => c.closeAsync()).ToList();

			FutureUtil.waitForAll(futureList).whenComplete((r, ex) =>
			{
			if (ex == null)
			{
				State = State.Closed;
				unAckedMessageTracker.Dispose();
				closeFuture.complete(null);
				log.info("[{}] [{}] Closed Topics Consumer", topic, subscription);
				client.cleanupConsumer(this);
				failPendingReceive();
			}
			else
			{
				State = State.Failed;
				closeFuture.completeExceptionally(ex);
				log.error("[{}] [{}] Could not close Topics Consumer", topic, subscription, ex.Cause);
			}
			});

			return closeFuture;
		}

		private void failPendingReceive()
		{
			@lock.readLock().@lock();
			try
			{
				if (listenerExecutor != null && !listenerExecutor.Shutdown)
				{
					while (!pendingReceives.Empty)
					{
						CompletableFuture<Message<T>> receiveFuture = pendingReceives.poll();
						if (receiveFuture != null)
						{
							receiveFuture.completeExceptionally(new PulsarClientException.AlreadyClosedException("Consumer is already closed"));
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
				@lock.readLock().unlock();
			}
		}

		public override bool Connected
		{
			get
			{
				return consumers.Values.All(consumer => consumer.Connected);
			}
		}

		internal override string HandlerName
		{
			get
			{
				return subscription;
			}
		}

		private ConsumerConfigurationData<T> InternalConsumerConfig
		{
			get
			{
				ConsumerConfigurationData<T> internalConsumerConfig = conf.clone();
				internalConsumerConfig.SubscriptionName = subscription;
				internalConsumerConfig.ConsumerName = consumerName;
				internalConsumerConfig.MessageListener = null;
				return internalConsumerConfig;
			}
		}

		public override void redeliverUnacknowledgedMessages()
		{
			@lock.writeLock().@lock();
			try
			{
				consumers.Values.ForEach(consumer => consumer.redeliverUnacknowledgedMessages());
				incomingMessages.clear();
				INCOMING_MESSAGES_SIZE_UPDATER.set(this, 0);
				unAckedMessageTracker.clear();
			}
			finally
			{
				@lock.writeLock().unlock();
			}
			resumeReceivingFromPausedConsumersIfNeeded();
		}

		public override void redeliverUnacknowledgedMessages(ISet<MessageId> messageIds)
		{
			if (messageIds.Count == 0)
			{
				return;
			}

			checkArgument(messageIds.First().get() is TopicMessageIdImpl);

			if (conf.SubscriptionType != SubscriptionType.Shared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				redeliverUnacknowledgedMessages();
				return;
			}
			removeExpiredMessagesFromQueue(messageIds);
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			messageIds.Select(messageId => (TopicMessageIdImpl)messageId).collect(Collectors.groupingBy(TopicMessageIdImpl::getTopicPartitionName, Collectors.toSet())).ForEach((topicName, messageIds1) => consumers[topicName].redeliverUnacknowledgedMessages(messageIds1.Select(mid => mid.InnerMessageId).collect(Collectors.toSet())));
			resumeReceivingFromPausedConsumersIfNeeded();
		}

		protected internal override void completeOpBatchReceive(OpBatchReceive<T> op)
		{
			notifyPendingBatchReceivedCallBack(op);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.api.PulsarClientException
		public override void seek(MessageId messageId)
		{
			try
			{
				seekAsync(messageId).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(long timestamp) throws org.apache.pulsar.client.api.PulsarClientException
		public override void seek(long timestamp)
		{
			try
			{
				seekAsync(timestamp).get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

		public override CompletableFuture<Void> seekAsync(MessageId messageId)
		{
			return FutureUtil.failedFuture(new PulsarClientException("Seek operation not supported on topics consumer"));
		}

		public override CompletableFuture<Void> seekAsync(long timestamp)
		{
			IList<CompletableFuture<Void>> futures = new List<CompletableFuture<Void>>(consumers.Count);
			consumers.Values.forEach(consumer => futures.Add(consumer.seekAsync(timestamp)));
			return FutureUtil.waitForAll(futures);
		}

		public override int AvailablePermits
		{
			get
			{
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				return consumers.Values.Select(ConsumerImpl::getAvailablePermits).Sum();
			}
		}

		public override bool hasReachedEndOfTopic()
		{
			return consumers.Values.All(Consumer.hasReachedEndOfTopic);
		}

		public override int numMessagesInQueue()
		{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			return incomingMessages.size() + consumers.Values.Select(ConsumerImpl::numMessagesInQueue).Sum();
		}

		public override ConsumerStats Stats
		{
			get
			{
				lock (this)
				{
					if (stats == null)
					{
						return null;
					}
					stats.reset();
            
					consumers.Values.ForEach(consumer => stats.updateCumulativeStats(consumer.Stats));
					return stats;
				}
			}
		}

		public virtual UnAckedMessageTracker UnAckedMessageTracker
		{
			get
			{
				return unAckedMessageTracker;
			}
		}

		private void removeExpiredMessagesFromQueue(ISet<MessageId> messageIds)
		{
			Message<T> peek = incomingMessages.peek();
			if (peek != null)
			{
				if (!messageIds.Contains(peek.MessageId))
				{
					// first message is not expired, then no message is expired in queue.
					return;
				}

				// try not to remove elements that are added while we remove
				Message<T> message = incomingMessages.poll();
				checkState(message is TopicMessageImpl);
				while (message != null)
				{
					INCOMING_MESSAGES_SIZE_UPDATER.addAndGet(this, -message.Data.length);
					MessageId messageId = message.MessageId;
					if (!messageIds.Contains(messageId))
					{
						messageIds.Add(messageId);
						break;
					}
					message = incomingMessages.poll();
				}
			}
		}

		private bool topicNameValid(string topicName)
		{
			checkArgument(TopicName.isValid(topicName), "Invalid topic name:" + topicName);
			checkArgument(!topics.ContainsKey(topicName), "Topics already contains topic:" + topicName);

			if (this.namespaceName != null)
			{
				checkArgument(TopicName.get(topicName).Namespace.ToString().Equals(this.namespaceName.ToString()), "Topic " + topicName + " not in same namespace with Topics");
			}

			return true;
		}

		// subscribe one more given topic
		public virtual CompletableFuture<Void> subscribeAsync(string topicName, bool createTopicIfDoesNotExist)
		{
			if (!topicNameValid(topicName))
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topic name not valid"));
			}

			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			CompletableFuture<Void> subscribeResult = new CompletableFuture<Void>();

			client.getPartitionedTopicMetadata(topicName).thenAccept(metadata => subscribeTopicPartitions(subscribeResult, topicName, metadata.partitions, createTopicIfDoesNotExist)).exceptionally(ex1 =>
			{
			log.warn("[{}] Failed to get partitioned topic metadata: {}", topicName, ex1.Message);
			subscribeResult.completeExceptionally(ex1);
			return null;
			});

			return subscribeResult;
		}

		// create consumer for a single topic with already known partitions.
		// first create a consumer with no topic, then do subscription for already know partitionedTopic.
		public static MultiTopicsConsumerImpl<T> createPartitionedConsumer<T>(PulsarClientImpl client, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, CompletableFuture<Consumer<T>> subscribeFuture, int numPartitions, Schema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			checkArgument(conf.TopicNames.size() == 1, "Should have only 1 topic for partitioned consumer");

			// get topic name, then remove it from conf, so constructor will create a consumer with no topic.
			ConsumerConfigurationData cloneConf = conf.clone();
			string topicName = cloneConf.SingleTopic;
			cloneConf.TopicNames.remove(topicName);

			CompletableFuture<Consumer> future = new CompletableFuture<Consumer>();
			MultiTopicsConsumerImpl consumer = new MultiTopicsConsumerImpl(client, topicName, cloneConf, listenerExecutor, future, schema, interceptors, true);

			future.thenCompose(c => ((MultiTopicsConsumerImpl)c).subscribeAsync(topicName, numPartitions)).thenRun(() => subscribeFuture.complete(consumer)).exceptionally(e =>
			{
			log.warn("Failed subscription for createPartitionedConsumer: {} {}, e:{}", topicName, numPartitions, e);
			subscribeFuture.completeExceptionally(PulsarClientException.wrap(((Exception) e).InnerException, string.Format("Failed to subscribe {0} with {1:D} partitions", topicName, numPartitions)));
			return null;
			});
			return consumer;
		}

		// subscribe one more given topic, but already know the numberPartitions
		private CompletableFuture<Void> subscribeAsync(string topicName, int numberPartitions)
		{
			if (!topicNameValid(topicName))
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topic name not valid"));
			}

			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			CompletableFuture<Void> subscribeResult = new CompletableFuture<Void>();
			subscribeTopicPartitions(subscribeResult, topicName, numberPartitions, true);

			return subscribeResult;
		}

		private void subscribeTopicPartitions(CompletableFuture<Void> subscribeResult, string topicName, int numPartitions, bool createIfDoesNotExist)
		{
			client.preProcessSchemaBeforeSubscribe(client, schema, topicName).whenComplete((ignored, cause) =>
			{
			if (null == cause)
			{
				doSubscribeTopicPartitions(subscribeResult, topicName, numPartitions, createIfDoesNotExist);
			}
			else
			{
				subscribeResult.completeExceptionally(cause);
			}
			});
		}

		private void doSubscribeTopicPartitions(CompletableFuture<Void> subscribeResult, string topicName, int numPartitions, bool createIfDoesNotExist)
		{
			if (log.DebugEnabled)
			{
				log.debug("Subscribe to topic {} metadata.partitions: {}", topicName, numPartitions);
			}

			IList<CompletableFuture<Consumer<T>>> futureList;
			if (numPartitions > 0)
			{
				this.topics.GetOrAdd(topicName, numPartitions);
				allTopicPartitionsNumber.addAndGet(numPartitions);

				int receiverQueueSize = Math.Min(conf.ReceiverQueueSize, conf.MaxTotalReceiverQueueSizeAcrossPartitions / numPartitions);
				ConsumerConfigurationData<T> configurationData = InternalConsumerConfig;
				configurationData.ReceiverQueueSize = receiverQueueSize;

				futureList = IntStream.range(0, numPartitions).mapToObj(partitionIndex =>
				{
				string partitionName = TopicName.get(topicName).getPartition(partitionIndex).ToString();
				CompletableFuture<Consumer<T>> subFuture = new CompletableFuture<Consumer<T>>();
				ConsumerImpl<T> newConsumer = ConsumerImpl.newConsumerImpl(client, partitionName, configurationData, client.externalExecutorProvider().Executor, partitionIndex, true, subFuture, SubscriptionMode.Durable, null, schema, interceptors, createIfDoesNotExist);
				consumers.GetOrAdd(newConsumer.Topic, newConsumer);
				return subFuture;
				}).collect(Collectors.toList());
			}
			else
			{
				this.topics.GetOrAdd(topicName, 1);
				allTopicPartitionsNumber.incrementAndGet();

				CompletableFuture<Consumer<T>> subFuture = new CompletableFuture<Consumer<T>>();
				ConsumerImpl<T> newConsumer = ConsumerImpl.newConsumerImpl(client, topicName, internalConfig, client.externalExecutorProvider().Executor, -1, true, subFuture, SubscriptionMode.Durable, null, schema, interceptors, createIfDoesNotExist);
				consumers.GetOrAdd(newConsumer.Topic, newConsumer);

				futureList = Collections.singletonList(subFuture);
			}

			FutureUtil.waitForAll(futureList).thenAccept(finalFuture =>
			{
			if (allTopicPartitionsNumber.get() > maxReceiverQueueSize)
			{
				MaxReceiverQueueSize = allTopicPartitionsNumber.get();
			}
			int numTopics = this.topics.Values.Select(int?.intValue).Sum();
			checkState(allTopicPartitionsNumber.get() == numTopics, "allTopicPartitionsNumber " + allTopicPartitionsNumber.get() + " not equals expected: " + numTopics);
			startReceivingMessages(consumers.Values.Where(consumer1 =>
			{
				string consumerTopicName = consumer1.Topic;
				if (TopicName.get(consumerTopicName).PartitionedTopicName.Equals(TopicName.get(topicName).PartitionedTopicName.ToString()))
				{
					return true;
				}
				else
				{
					return false;
				}
			}).ToList());
			subscribeResult.complete(null);
			log.info("[{}] [{}] Success subscribe new topic {} in topics consumer, partitions: {}, allTopicPartitionsNumber: {}", topic, subscription, topicName, numPartitions, allTopicPartitionsNumber.get());
			if (this.namespaceName == null)
			{
				this.namespaceName = TopicName.get(topicName).NamespaceObject;
			}
			return;
			}).exceptionally(ex =>
			{
			handleSubscribeOneTopicError(topicName, ex, subscribeResult);
			return null;
		});
		}

		// handling failure during subscribe new topic, unsubscribe success created partitions
		private void handleSubscribeOneTopicError(string topicName, Exception error, CompletableFuture<Void> subscribeFuture)
		{
			log.warn("[{}] Failed to subscribe for topic [{}] in topics consumer {}", topic, topicName, error.Message);

			client.externalExecutorProvider().Executor.submit(() =>
			{
			AtomicInteger toCloseNum = new AtomicInteger(0);
			consumers.Values.Where(consumer1 =>
			{
				string consumerTopicName = consumer1.Topic;
				if (TopicName.get(consumerTopicName).PartitionedTopicName.Equals(topicName))
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
					allTopicPartitionsNumber.decrementAndGet();
					consumers.Remove(consumer2.Topic);
					if (toCloseNum.decrementAndGet() == 0)
					{
						log.warn("[{}] Failed to subscribe for topic [{}] in topics consumer, subscribe error: {}", topic, topicName, error.Message);
						topics.Remove(topicName);
						checkState(allTopicPartitionsNumber.get() == consumers.Values.Count);
						subscribeFuture.completeExceptionally(error);
					}
					return;
				});
			});
			});
		}

		// un-subscribe a given topic
		public virtual CompletableFuture<Void> unsubscribeAsync(string topicName)
		{
			checkArgument(TopicName.isValid(topicName), "Invalid topic name:" + topicName);

			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			if (partitionsAutoUpdateTimeout != null)
			{
				partitionsAutoUpdateTimeout.cancel();
				partitionsAutoUpdateTimeout = null;
			}

			CompletableFuture<Void> unsubscribeFuture = new CompletableFuture<Void>();
			string topicPartName = TopicName.get(topicName).PartitionedTopicName;

			IList<ConsumerImpl<T>> consumersToUnsub = consumers.Values.Where(consumer =>
			{
			string consumerTopicName = consumer.Topic;
			if (TopicName.get(consumerTopicName).PartitionedTopicName.Equals(topicPartName))
			{
				return true;
			}
			else
			{
				return false;
			}
			}).ToList();

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			IList<CompletableFuture<Void>> futureList = consumersToUnsub.Select(ConsumerImpl::unsubscribeAsync).ToList();

			FutureUtil.waitForAll(futureList).whenComplete((r, ex) =>
			{
			if (ex == null)
			{
				consumersToUnsub.ForEach(consumer1 =>
				{
					consumers.Remove(consumer1.Topic);
					pausedConsumers.remove(consumer1);
					allTopicPartitionsNumber.decrementAndGet();
				});
				topics.Remove(topicName);
				((UnAckedTopicMessageTracker) unAckedMessageTracker).removeTopicMessages(topicName);
				unsubscribeFuture.complete(null);
				log.info("[{}] [{}] [{}] Unsubscribed Topics Consumer, allTopicPartitionsNumber: {}", topicName, subscription, consumerName, allTopicPartitionsNumber);
			}
			else
			{
				unsubscribeFuture.completeExceptionally(ex);
				State = State.Failed;
				log.error("[{}] [{}] [{}] Could not unsubscribe Topics Consumer", topicName, subscription, consumerName, ex.Cause);
			}
			});

			return unsubscribeFuture;
		}

		// Remove a consumer for a topic
		public virtual CompletableFuture<Void> removeConsumerAsync(string topicName)
		{
			checkArgument(TopicName.isValid(topicName), "Invalid topic name:" + topicName);

			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			CompletableFuture<Void> unsubscribeFuture = new CompletableFuture<Void>();
			string topicPartName = TopicName.get(topicName).PartitionedTopicName;


			IList<ConsumerImpl<T>> consumersToClose = consumers.Values.Where(consumer =>
			{
			string consumerTopicName = consumer.Topic;
			if (TopicName.get(consumerTopicName).PartitionedTopicName.Equals(topicPartName))
			{
				return true;
			}
			else
			{
				return false;
			}
			}).ToList();

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			IList<CompletableFuture<Void>> futureList = consumersToClose.Select(ConsumerImpl::closeAsync).ToList();

			FutureUtil.waitForAll(futureList).whenComplete((r, ex) =>
			{
			if (ex == null)
			{
				consumersToClose.ForEach(consumer1 =>
				{
					consumers.Remove(consumer1.Topic);
					pausedConsumers.remove(consumer1);
					allTopicPartitionsNumber.decrementAndGet();
				});
				topics.Remove(topicName);
				((UnAckedTopicMessageTracker) unAckedMessageTracker).removeTopicMessages(topicName);
				unsubscribeFuture.complete(null);
				log.info("[{}] [{}] [{}] Removed Topics Consumer, allTopicPartitionsNumber: {}", topicName, subscription, consumerName, allTopicPartitionsNumber);
			}
			else
			{
				unsubscribeFuture.completeExceptionally(ex);
				State = State.Failed;
				log.error("[{}] [{}] [{}] Could not remove Topics Consumer", topicName, subscription, consumerName, ex.Cause);
			}
			});

			return unsubscribeFuture;
		}


		// get topics name
		public virtual IList<string> Topics
		{
			get
			{
				return topics.Keys.ToList();
			}
		}

		// get partitioned topics name
		public virtual IList<string> PartitionedTopics
		{
			get
			{
				return consumers.Keys.ToList();
			}
		}

		// get partitioned consumers
		public virtual IList<ConsumerImpl<T>> Consumers
		{
			get
			{
				return consumers.Values.ToList();
			}
		}

		public override void pause()
		{
			consumers.forEach((name, consumer) => consumer.pause());
		}

		public override void resume()
		{
			consumers.forEach((name, consumer) => consumer.resume());
		}

		// This listener is triggered when topics partitions are updated.
		private class TopicsPartitionChangedListener : PartitionsChangedListener
		{
			private readonly MultiTopicsConsumerImpl<T> outerInstance;

			public TopicsPartitionChangedListener(MultiTopicsConsumerImpl<T> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			// Check partitions changes of passed in topics, and subscribe new added partitions.
			public virtual CompletableFuture<Void> onTopicsExtended(ICollection<string> topicsExtended)
			{
				CompletableFuture<Void> future = new CompletableFuture<Void>();
				if (topicsExtended.Count == 0)
				{
					future.complete(null);
					return future;
				}

				if (log.DebugEnabled)
				{
					log.debug("[{}]  run onTopicsExtended: {}, size: {}", outerInstance.topic, topicsExtended.ToString(), topicsExtended.Count);
				}

				IList<CompletableFuture<Void>> futureList = Lists.newArrayListWithExpectedSize(topicsExtended.Count);
				topicsExtended.forEach(outerInstance.topic => futureList.Add(outerInstance.subscribeIncreasedTopicPartitions(outerInstance.topic)));
				FutureUtil.waitForAll(futureList).thenAccept(finalFuture => future.complete(null)).exceptionally(ex =>
				{
				log.warn("[{}] Failed to subscribe increased topics partitions: {}", outerInstance.topic, ex.Message);
				future.completeExceptionally(ex);
				return null;
				});

				return future;
			}
		}

		// subscribe increased partitions for a given topic
		private CompletableFuture<Void> subscribeIncreasedTopicPartitions(string topicName)
		{
			CompletableFuture<Void> future = new CompletableFuture<Void>();

			client.getPartitionsForTopic(topicName).thenCompose(list =>
			{
			int oldPartitionNumber = topics[topicName.ToString()];
			int currentPartitionNumber = list.size();
			if (log.DebugEnabled)
			{
				log.debug("[{}] partitions number. old: {}, new: {}", topicName.ToString(), oldPartitionNumber, currentPartitionNumber);
			}
			if (oldPartitionNumber == currentPartitionNumber)
			{
				future.complete(null);
				return future;
			}
			else if (oldPartitionNumber < currentPartitionNumber)
			{
				IList<string> newPartitions = list.subList(oldPartitionNumber, currentPartitionNumber);
				IList<CompletableFuture<Consumer<T>>> futureList = newPartitions.Select(partitionName =>
				{
					int partitionIndex = TopicName.getPartitionIndex(partitionName);
					CompletableFuture<Consumer<T>> subFuture = new CompletableFuture<Consumer<T>>();
					ConsumerConfigurationData<T> configurationData = InternalConsumerConfig;
					ConsumerImpl<T> newConsumer = ConsumerImpl.newConsumerImpl(client, partitionName, configurationData, client.externalExecutorProvider().Executor, partitionIndex, true, subFuture, SubscriptionMode.Durable, null, schema, interceptors, true);
					consumers.GetOrAdd(newConsumer.Topic, newConsumer);
					if (log.DebugEnabled)
					{
						log.debug("[{}] create consumer {} for partitionName: {}", topicName.ToString(), newConsumer.Topic, partitionName);
					}
					return subFuture;
				}).ToList();
				FutureUtil.waitForAll(futureList).thenAccept(finalFuture =>
				{
					IList<ConsumerImpl<T>> newConsumerList = newPartitions.Select(partitionTopic => consumers[partitionTopic]).ToList();
					startReceivingMessages(newConsumerList);
					future.complete(null);
				}).exceptionally(ex =>
				{
					log.warn("[{}] Failed to subscribe {} partition: {} - {}", topic, topicName.ToString(), oldPartitionNumber, currentPartitionNumber, ex.Message);
					future.completeExceptionally(ex);
					return null;
				});
			}
			else
			{
				log.error("[{}] not support shrink topic partitions. old: {}, new: {}", topicName.ToString(), oldPartitionNumber, currentPartitionNumber);
				future.completeExceptionally(new NotSupportedException("not support shrink topic partitions"));
			}
			return future;
			});

			return future;
		}

		private TimerTask partitionsAutoUpdateTimerTask = new TimerTaskAnonymousInnerClass();

		private class TimerTaskAnonymousInnerClass : TimerTask
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
			public override void run(Timeout timeout)
			{
				if (timeout.Cancelled || outerInstance.State != State.Ready)
				{
					return;
				}

				if (log.DebugEnabled)
				{
					log.debug("[{}]  run partitionsAutoUpdateTimerTask for multiTopicsConsumer: {}", outerInstance.topic);
				}

				// if last auto update not completed yet, do nothing.
				if (outerInstance.partitionsAutoUpdateFuture == null || outerInstance.partitionsAutoUpdateFuture.Done)
				{
					outerInstance.partitionsAutoUpdateFuture = outerInstance.topicsPartitionChangedListener.onTopicsExtended(outerInstance.topics.Keys);
				}

				// schedule the next re-check task
				outerInstance.partitionsAutoUpdateTimeout = outerInstance.client.timer().newTimeout(partitionsAutoUpdateTimerTask, 1, TimeUnit.MINUTES);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public io.netty.util.Timeout getPartitionsAutoUpdateTimeout()
		public virtual Timeout PartitionsAutoUpdateTimeout
		{
			get
			{
				return partitionsAutoUpdateTimeout;
			}
		}

		public override CompletableFuture<MessageId> LastMessageIdAsync
		{
			get
			{
				CompletableFuture<MessageId> returnFuture = new CompletableFuture<MessageId>();
    
				IDictionary<string, CompletableFuture<MessageId>> messageIdFutures = consumers.SetOfKeyValuePairs().Select(entry => Pair.of(entry.Key,entry.Value.LastMessageIdAsync)).ToDictionary(Pair.getKey, Pair.getValue);
    
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: java.util.concurrent.CompletableFuture.allOf(messageIdFutures.entrySet().stream().map(java.util.Map.Entry::getValue).toArray(java.util.concurrent.CompletableFuture<?>[]::new)).whenComplete((ignore, ex) ->
	//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				CompletableFuture.allOf(messageIdFutures.SetOfKeyValuePairs().Select(DictionaryEntry.getValue).ToArray(CompletableFuture<object>[]::new)).whenComplete((ignore, ex) =>
				{
				Builder<string, MessageId> builder = ImmutableMap.builder<string, MessageId>();
				messageIdFutures.forEach((key, future) =>
				{
					MessageId messageId;
					try
					{
						messageId = future.get();
					}
					catch (Exception e)
					{
						log.warn("[{}] Exception when topic {} getLastMessageId.", key, e);
						messageId = MessageId.earliest;
					}
					builder.put(key, messageId);
				});
				returnFuture.complete(new MultiMessageIdImpl(builder.build()));
				});
    
				return returnFuture;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(MultiTopicsConsumerImpl));
	}

}