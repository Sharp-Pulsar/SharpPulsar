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
	using Consumer = SharpPulsar.Api.Consumer;
	using ConsumerStats = SharpPulsar.Api.ConsumerStats;
	using SharpPulsar.Api;
	using MessageId = SharpPulsar.Api.IMessageId;
	using SharpPulsar.Api;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using NotSupportedException = SharpPulsar.Api.PulsarClientException.NotSupportedException;
	using SharpPulsar.Api;
	using SubscriptionType = SharpPulsar.Api.SubscriptionType;
	using SubscriptionMode = SharpPulsar.Impl.ConsumerImpl.SubscriptionMode;
	using SharpPulsar.Impl.Conf;
	using TransactionImpl = SharpPulsar.Impl.Transaction.TransactionImpl;
	using ConsumerName = SharpPulsar.Util.ConsumerName;
	using AckType = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandAck.AckType;
	using NamespaceName = Org.Apache.Pulsar.Common.Naming.NamespaceName;
	using TopicName = Org.Apache.Pulsar.Common.Naming.TopicName;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class MultiTopicsConsumerImpl<T> : ConsumerBase<T>
	{

		public const string DummyTopicNamePrefix = "MultiTopicsConsumer-";

		// All topics should be in same namespace
		protected internal NamespaceName NamespaceName;

		// Map <topic+partition, consumer>, when get do ACK, consumer will by find by topic name
		private readonly ConcurrentDictionary<string, ConsumerImpl<T>> consumers;

		// Map <topic, numPartitions>, store partition number for each topic
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly ConcurrentDictionary<string, int> TopicsConflict;

		// Queue of partition consumers on which we have stopped calling receiveAsync() because the
		// shared incoming queue was full
		private readonly ConcurrentLinkedQueue<ConsumerImpl<T>> pausedConsumers;

		// Threshold for the shared queue. When the size of the shared queue goes below the threshold, we are going to
		// resume receiving from the paused consumer partitions
		private readonly int sharedQueueResumeThreshold;

		// sum of topicPartitions, simple topic has 1, partitioned topic equals to partition number.
		internal AtomicInteger AllTopicPartitionsNumber;

		// timeout related to auto check and subscribe partition increasement
		public virtual PartitionsAutoUpdateTimeout {get;} = null;
		internal TopicsPartitionChangedListener TopicsPartitionChangedListener;
		internal CompletableFuture<Void> PartitionsAutoUpdateFuture = null;

		private readonly ReadWriteLock @lock = new ReentrantReadWriteLock();
		private readonly ConsumerStatsRecorder stats;
		public virtual UnAckedMessageTracker {get;}
		private readonly ConsumerConfigurationData<T> internalConfig;

		public MultiTopicsConsumerImpl(PulsarClientImpl Client, ConsumerConfigurationData<T> Conf, ExecutorService ListenerExecutor, CompletableFuture<Consumer<T>> SubscribeFuture, Schema<T> Schema, ConsumerInterceptors<T> Interceptors, bool CreateTopicIfDoesNotExist) : this(Client, DummyTopicNamePrefix + ConsumerName.generateRandomName(), Conf, ListenerExecutor, SubscribeFuture, Schema, Interceptors, CreateTopicIfDoesNotExist)
		{
		}

		public MultiTopicsConsumerImpl(PulsarClientImpl Client, string SingleTopic, ConsumerConfigurationData<T> Conf, ExecutorService ListenerExecutor, CompletableFuture<Consumer<T>> SubscribeFuture, Schema<T> Schema, ConsumerInterceptors<T> Interceptors, bool CreateTopicIfDoesNotExist) : base(Client, SingleTopic, Conf, Math.Max(2, Conf.ReceiverQueueSize), ListenerExecutor, SubscribeFuture, Schema, Interceptors)
		{

			checkArgument(Conf.ReceiverQueueSize > 0, "Receiver queue size needs to be greater than 0 for Topics Consumer");

			this.TopicsConflict = new ConcurrentDictionary<string, int>();
			this.consumers = new ConcurrentDictionary<string, ConsumerImpl<T>>();
			this.pausedConsumers = new ConcurrentLinkedQueue<ConsumerImpl<T>>();
			this.sharedQueueResumeThreshold = MaxReceiverQueueSizeConflict / 2;
			this.AllTopicPartitionsNumber = new AtomicInteger(0);

			if (Conf.AckTimeoutMillis != 0)
			{
				if (Conf.TickDurationMillis > 0)
				{
					this.UnAckedMessageTracker = new UnAckedTopicMessageTracker(Client, this, Conf.AckTimeoutMillis, Conf.TickDurationMillis);
				}
				else
				{
					this.UnAckedMessageTracker = new UnAckedTopicMessageTracker(Client, this, Conf.AckTimeoutMillis);
				}
			}
			else
			{
				this.UnAckedMessageTracker = UnAckedMessageTracker.UnackedMessageTrackerDisabled;
			}

			this.internalConfig = InternalConsumerConfig;
			this.stats = Client.Configuration.StatsIntervalSeconds > 0 ? new ConsumerStatsRecorderImpl() : null;

			// start track and auto subscribe partition increasement
			if (Conf.AutoUpdatePartitions)
			{
				TopicsPartitionChangedListener = new TopicsPartitionChangedListener(this);
				PartitionsAutoUpdateTimeout = Client.timer().newTimeout(partitionsAutoUpdateTimerTask, 1, BAMCIS.Util.Concurrent.TimeUnit.MINUTES);
			}

			if (Conf.TopicNames.Empty)
			{
				this.NamespaceName = null;
				State = State.Ready;
				subscribeFuture().complete(MultiTopicsConsumerImpl.this);
				return;
			}

			checkArgument(Conf.TopicNames.Empty || TopicNamesValid(Conf.TopicNames), "Topics should have same namespace.");
			this.NamespaceName = Conf.TopicNames.First().flatMap(s => (TopicName.get(s).NamespaceObject)).get();

			IList<CompletableFuture<Void>> Futures = Conf.TopicNames.Select(t => subscribeAsync(t, CreateTopicIfDoesNotExist)).ToList();
			FutureUtil.waitForAll(Futures).thenAccept(finalFuture =>
			{
			if (AllTopicPartitionsNumber.get() > MaxReceiverQueueSizeConflict)
			{
				MaxReceiverQueueSize = AllTopicPartitionsNumber.get();
			}
			State = State.Ready;
			StartReceivingMessages(new List<ConsumerImpl<T>>(consumers.Values));
			log.info("[{}] [{}] Created topics consumer with {} sub-consumers", Topic, SubscriptionConflict, AllTopicPartitionsNumber.get());
			subscribeFuture().complete(MultiTopicsConsumerImpl.this);
			}).exceptionally(ex =>
			{
			log.warn("[{}] Failed to subscribe topics: {}", Topic, ex.Message);
			SubscribeFuture.completeExceptionally(ex);
			return null;
		});
		}

		// Check topics are valid.
		// - each topic is valid,
		// - every topic has same namespace,
		// - topic names are unique.
		private static bool TopicNamesValid(ICollection<string> Topics)
		{
			checkState(Topics != null && Topics.Count >= 1, "topics should contain more than 1 topic");

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String namespace = org.apache.pulsar.common.naming.TopicName.get(topics.stream().findFirst().get()).getNamespace();
			string Namespace = TopicName.get(Topics.First().get()).Namespace;

			Optional<string> Result = Topics.Where(Topic =>
			{
			bool TopicInvalid = !TopicName.isValid(Topic);
			if (TopicInvalid)
			{
				return true;
			}
			string NewNamespace = TopicName.get(Topic).Namespace;
			if (!Namespace.Equals(NewNamespace))
			{
				return true;
			}
			else
			{
				return false;
			}
			}).First();

			if (Result.Present)
			{
				log.warn("Received invalid topic name: {}", Result.get());
				return false;
			}

			// check topic names are unique
			HashSet<string> Set = new HashSet<string>(Topics);
			if (Set.Count == Topics.Count)
			{
				return true;
			}
			else
			{
				log.warn("Topic names not unique. unique/all : {}/{}", Set.Count, Topics.Count);
				return false;
			}
		}

		private void StartReceivingMessages(IList<ConsumerImpl<T>> NewConsumers)
		{
			if (log.DebugEnabled)
			{
				log.debug("[{}] startReceivingMessages for {} new consumers in topics consumer, state: {}", Topic, NewConsumers.Count, State);
			}
			if (State == State.Ready)
			{
				NewConsumers.ForEach(consumer =>
				{
				consumer.sendFlowPermitsToBroker(consumer.ConnectionHandler.cnx(), Conf.ReceiverQueueSize);
				ReceiveMessageFromConsumer(consumer);
				});
			}
		}

		private void ReceiveMessageFromConsumer(ConsumerImpl<T> Consumer)
		{
			Consumer.receiveAsync().thenAccept(message =>
			{
			if (log.DebugEnabled)
			{
				log.debug("[{}] [{}] Receive message from sub consumer:{}", Topic, SubscriptionConflict, Consumer.Topic);
			}
			MessageReceived(Consumer, message);
			@lock.writeLock().@lock();
			try
			{
				int Size = IncomingMessages.size();
				if (Size >= MaxReceiverQueueSizeConflict || (Size > sharedQueueResumeThreshold && !pausedConsumers.Empty))
				{
					pausedConsumers.add(Consumer);
				}
				else
				{
					ClientConflict.eventLoopGroup().execute(() =>
					{
						ReceiveMessageFromConsumer(Consumer);
					});
				}
			}
			finally
			{
				@lock.writeLock().unlock();
			}
			});
		}

		private void MessageReceived(ConsumerImpl<T> Consumer, Message<T> Message)
		{
			checkArgument(Message is MessageImpl);
			@lock.writeLock().@lock();
			try
			{
				TopicMessageImpl<T> TopicMessage = new TopicMessageImpl<T>(Consumer.Topic, Consumer.TopicNameWithoutPartition, Message);

				if (log.DebugEnabled)
				{
					log.debug("[{}][{}] Received message from topics-consumer {}", Topic, SubscriptionConflict, Message.MessageId);
				}

				// if asyncReceive is waiting : return message to callback without adding to incomingMessages queue
				if (!PendingReceives.Empty)
				{
					CompletableFuture<Message<T>> ReceivedFuture = PendingReceives.poll();
					UnAckedMessageTracker.Add(TopicMessage.MessageId);
					ListenerExecutor.execute(() => ReceivedFuture.complete(TopicMessage));
				}
				else if (EnqueueMessageAndCheckBatchReceive(TopicMessage))
				{
					if (HasPendingBatchReceive())
					{
						NotifyPendingBatchReceivedCallBack();
					}
				}
			}
			finally
			{
				@lock.writeLock().unlock();
			}

			if (Listener != null)
			{
				// Trigger the notification on the message listener in a separate thread to avoid blocking the networking
				// thread while the message processing happens
				ListenerExecutor.execute(() =>
				{
				Message<T> Msg;
				try
				{
					Msg = InternalReceive();
				}
				catch (PulsarClientException E)
				{
					log.warn("[{}] [{}] Failed to dequeue the message for listener", Topic, SubscriptionConflict, E);
					return;
				}
				try
				{
					if (log.DebugEnabled)
					{
						log.debug("[{}][{}] Calling message listener for message {}", Topic, SubscriptionConflict, Message.MessageId);
					}
					Listener.received(MultiTopicsConsumerImpl.this, Msg);
				}
				catch (Exception T)
				{
					log.error("[{}][{}] Message listener error in processing message: {}", Topic, SubscriptionConflict, Message, T);
				}
				});
			}
		}

		public virtual void MessageProcessed<T1>(Message<T1> Msg)
		{
			lock (this)
			{
				UnAckedMessageTracker.Add(Msg.MessageId);
				IncomingMessagesSizeUpdater.addAndGet(this, -Msg.Data.Length);
			}
		}

		private void ResumeReceivingFromPausedConsumersIfNeeded()
		{
			@lock.readLock().@lock();
			try
			{
				if (IncomingMessages.size() <= sharedQueueResumeThreshold && !pausedConsumers.Empty)
				{
					while (true)
					{
						ConsumerImpl<T> Consumer = pausedConsumers.poll();
						if (Consumer == null)
						{
							break;
						}

						// if messages are readily available on consumer we will attempt to writeLock on the same thread
						ClientConflict.eventLoopGroup().execute(() =>
						{
						ReceiveMessageFromConsumer(Consumer);
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
//ORIGINAL LINE: @Override protected SharpPulsar.api.Message<T> internalReceive() throws SharpPulsar.api.PulsarClientException
		public override Message<T> InternalReceive()
		{
			Message<T> Message;
			try
			{
				Message = IncomingMessages.take();
				IncomingMessagesSizeUpdater.addAndGet(this, -Message.Data.Length);
				checkState(Message is TopicMessageImpl);
				UnAckedMessageTracker.Add(Message.MessageId);
				ResumeReceivingFromPausedConsumersIfNeeded();
				return Message;
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected SharpPulsar.api.Message<T> internalReceive(int timeout, java.util.concurrent.BAMCIS.Util.Concurrent.TimeUnit unit) throws SharpPulsar.api.PulsarClientException
		public override Message<T> InternalReceive(int Timeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			Message<T> Message;
			try
			{
				Message = IncomingMessages.poll(Timeout, Unit);
				if (Message != null)
				{
					IncomingMessagesSizeUpdater.addAndGet(this, -Message.Data.Length);
					checkArgument(Message is TopicMessageImpl);
					UnAckedMessageTracker.Add(Message.MessageId);
				}
				ResumeReceivingFromPausedConsumersIfNeeded();
				return Message;
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected SharpPulsar.api.Messages<T> internalBatchReceive() throws SharpPulsar.api.PulsarClientException
		public override Messages<T> InternalBatchReceive()
		{
			try
			{
				return InternalBatchReceiveAsync().get();
			}
			catch (Exception e) when (e is InterruptedException || e is ExecutionException)
			{
				State State = State;
				if (State != State.Closing && State != State.Closed)
				{
					stats.IncrementNumBatchReceiveFailed();
					throw PulsarClientException.unwrap(e);
				}
				else
				{
					return null;
				}
			}
		}

		public override CompletableFuture<Messages<T>> InternalBatchReceiveAsync()
		{
			CompletableFuture<Messages<T>> Result = new CompletableFuture<Messages<T>>();
			try
			{
				@lock.writeLock().@lock();
				if (PendingBatchReceives == null)
				{
					PendingBatchReceives = Queues.newConcurrentLinkedQueue();
				}
				if (HasEnoughMessagesForBatchReceive())
				{
					MessagesImpl<T> Messages = NewMessagesImpl;
					Message<T> MsgPeeked = IncomingMessages.peek();
					while (MsgPeeked != null && Messages.canAdd(MsgPeeked))
					{
						Message<T> Msg = IncomingMessages.poll();
						if (Msg != null)
						{
							IncomingMessagesSizeUpdater.addAndGet(this, -Msg.Data.Length);
							Message<T> InterceptMsg = BeforeConsume(Msg);
							Messages.add(InterceptMsg);
						}
						MsgPeeked = IncomingMessages.peek();
					}
					Result.complete(Messages);
				}
				else
				{
					PendingBatchReceives.add(OpBatchReceive.Of(Result));
				}
			}
			finally
			{
				@lock.writeLock().unlock();
			}
			return Result;
		}

		public override CompletableFuture<Message<T>> InternalReceiveAsync()
		{
			CompletableFuture<Message<T>> Result = new CompletableFuture<Message<T>>();
			Message<T> Message;
			try
			{
				@lock.writeLock().@lock();
				Message = IncomingMessages.poll(0, BAMCIS.Util.Concurrent.TimeUnit.SECONDS);
				if (Message == null)
				{
					PendingReceives.add(Result);
				}
				else
				{
					IncomingMessagesSizeUpdater.addAndGet(this, -Message.Data.Length);
					checkState(Message is TopicMessageImpl);
					UnAckedMessageTracker.Add(Message.MessageId);
					ResumeReceivingFromPausedConsumersIfNeeded();
					Result.complete(Message);
				}
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				Result.completeExceptionally(new PulsarClientException(E));
			}
			finally
			{
				@lock.writeLock().unlock();
			}

			return Result;
		}

		public override CompletableFuture<Void> DoAcknowledge(MessageId MessageId, AckType AckType, IDictionary<string, long> Properties, TransactionImpl TxnImpl)
		{
			checkArgument(MessageId is TopicMessageIdImpl);
			TopicMessageIdImpl TopicMessageId = (TopicMessageIdImpl) MessageId;

			if (State != State.Ready)
			{
				return FutureUtil.failedFuture(new PulsarClientException("Consumer already closed"));
			}

			if (AckType == AckType.Cumulative)
			{
				Consumer IndividualConsumer = consumers[TopicMessageId.TopicPartitionName];
				if (IndividualConsumer != null)
				{
					MessageId InnerId = TopicMessageId.InnerMessageId;
					return IndividualConsumer.acknowledgeCumulativeAsync(InnerId);
				}
				else
				{
					return FutureUtil.failedFuture(new PulsarClientException.NotConnectedException());
				}
			}
			else
			{
				ConsumerImpl<T> Consumer = consumers[TopicMessageId.TopicPartitionName];

				MessageId InnerId = TopicMessageId.InnerMessageId;
				return Consumer.doAcknowledgeWithTxn(InnerId, AckType, Properties, TxnImpl).thenRun(() => UnAckedMessageTracker.Remove(TopicMessageId));
			}
		}

		public override void NegativeAcknowledge(MessageId MessageId)
		{
			checkArgument(MessageId is TopicMessageIdImpl);
			TopicMessageIdImpl TopicMessageId = (TopicMessageIdImpl) MessageId;

			ConsumerImpl<T> Consumer = consumers[TopicMessageId.TopicPartitionName];
			Consumer.negativeAcknowledge(TopicMessageId.InnerMessageId);
		}

		public override CompletableFuture<Void> UnsubscribeAsync()
		{
			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}
			State = State.Closing;

			CompletableFuture<Void> UnsubscribeFuture = new CompletableFuture<Void>();
			IList<CompletableFuture<Void>> FutureList = consumers.Values.Select(c => c.unsubscribeAsync()).ToList();

			FutureUtil.waitForAll(FutureList).whenComplete((r, ex) =>
			{
			if (ex == null)
			{
				State = State.Closed;
				UnAckedMessageTracker.Dispose();
				UnsubscribeFuture.complete(null);
				log.info("[{}] [{}] [{}] Unsubscribed Topics Consumer", Topic, SubscriptionConflict, ConsumerNameConflict);
			}
			else
			{
				State = State.Failed;
				UnsubscribeFuture.completeExceptionally(ex);
				log.error("[{}] [{}] [{}] Could not unsubscribe Topics Consumer", Topic, SubscriptionConflict, ConsumerNameConflict, ex.Cause);
			}
			});

			return UnsubscribeFuture;
		}

		public override CompletableFuture<Void> CloseAsync()
		{
			if (State == State.Closing || State == State.Closed)
			{
				UnAckedMessageTracker.Dispose();
				return CompletableFuture.completedFuture(null);
			}
			State = State.Closing;

			if (PartitionsAutoUpdateTimeout != null)
			{
				PartitionsAutoUpdateTimeout.cancel();
				PartitionsAutoUpdateTimeout = null;
			}

			CompletableFuture<Void> CloseFuture = new CompletableFuture<Void>();
			IList<CompletableFuture<Void>> FutureList = consumers.Values.Select(c => c.closeAsync()).ToList();

			FutureUtil.waitForAll(FutureList).whenComplete((r, ex) =>
			{
			if (ex == null)
			{
				State = State.Closed;
				UnAckedMessageTracker.Dispose();
				CloseFuture.complete(null);
				log.info("[{}] [{}] Closed Topics Consumer", Topic, SubscriptionConflict);
				ClientConflict.cleanupConsumer(this);
				FailPendingReceive();
			}
			else
			{
				State = State.Failed;
				CloseFuture.completeExceptionally(ex);
				log.error("[{}] [{}] Could not close Topics Consumer", Topic, SubscriptionConflict, ex.Cause);
			}
			});

			return CloseFuture;
		}

		private void FailPendingReceive()
		{
			@lock.readLock().@lock();
			try
			{
				if (ListenerExecutor != null && !ListenerExecutor.Shutdown)
				{
					while (!PendingReceives.Empty)
					{
						CompletableFuture<Message<T>> ReceiveFuture = PendingReceives.poll();
						if (ReceiveFuture != null)
						{
							ReceiveFuture.completeExceptionally(new PulsarClientException.AlreadyClosedException("Consumer is already closed"));
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

		public override string HandlerName
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
				ConsumerConfigurationData<T> InternalConsumerConfig = Conf.clone();
				InternalConsumerConfig.SubscriptionName = SubscriptionConflict;
				InternalConsumerConfig.ConsumerName = ConsumerNameConflict;
				InternalConsumerConfig.MessageListener = null;
				return InternalConsumerConfig;
			}
		}

		public override void RedeliverUnacknowledgedMessages()
		{
			@lock.writeLock().@lock();
			try
			{
				consumers.Values.ForEach(consumer => consumer.redeliverUnacknowledgedMessages());
				IncomingMessages.clear();
				IncomingMessagesSizeUpdater.set(this, 0);
				UnAckedMessageTracker.Clear();
			}
			finally
			{
				@lock.writeLock().unlock();
			}
			ResumeReceivingFromPausedConsumersIfNeeded();
		}

		public override void RedeliverUnacknowledgedMessages(ISet<MessageId> MessageIds)
		{
			if (MessageIds.Count == 0)
			{
				return;
			}

			checkArgument(MessageIds.First().get() is TopicMessageIdImpl);

			if (Conf.SubscriptionType != SubscriptionType.Shared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				RedeliverUnacknowledgedMessages();
				return;
			}
			RemoveExpiredMessagesFromQueue(MessageIds);
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			MessageIds.Select(messageId => (TopicMessageIdImpl)messageId).collect(Collectors.groupingBy(TopicMessageIdImpl::getTopicPartitionName, Collectors.toSet())).ForEach((topicName, messageIds1) => consumers[topicName].redeliverUnacknowledgedMessages(messageIds1.Select(mid => mid.InnerMessageId).collect(Collectors.toSet())));
			ResumeReceivingFromPausedConsumersIfNeeded();
		}

		public override void CompleteOpBatchReceive(OpBatchReceive<T> Op)
		{
			NotifyPendingBatchReceivedCallBack(Op);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(SharpPulsar.api.MessageId messageId) throws SharpPulsar.api.PulsarClientException
		public override void Seek(MessageId MessageId)
		{
			try
			{
				SeekAsync(MessageId).get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void seek(long timestamp) throws SharpPulsar.api.PulsarClientException
		public override void Seek(long Timestamp)
		{
			try
			{
				SeekAsync(Timestamp).get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Void> SeekAsync(MessageId MessageId)
		{
			return FutureUtil.failedFuture(new PulsarClientException("Seek operation not supported on topics consumer"));
		}

		public override CompletableFuture<Void> SeekAsync(long Timestamp)
		{
			IList<CompletableFuture<Void>> Futures = new List<CompletableFuture<Void>>(consumers.Count);
			consumers.Values.forEach(consumer => Futures.Add(consumer.seekAsync(Timestamp)));
			return FutureUtil.waitForAll(Futures);
		}

		public override int AvailablePermits
		{
			get
			{
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				return consumers.Values.Select(ConsumerImpl::getAvailablePermits).Sum();
			}
		}

		public override bool HasReachedEndOfTopic()
		{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			return consumers.Values.All(Consumer::hasReachedEndOfTopic);
		}

		public override int NumMessagesInQueue()
		{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			return IncomingMessages.size() + consumers.Values.Select(ConsumerImpl::numMessagesInQueue).Sum();
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
					stats.Reset();
            
					consumers.Values.ForEach(consumer => stats.updateCumulativeStats(consumer.Stats));
					return stats;
				}
			}
		}


		private void RemoveExpiredMessagesFromQueue(ISet<MessageId> MessageIds)
		{
			Message<T> Peek = IncomingMessages.peek();
			if (Peek != null)
			{
				if (!MessageIds.Contains(Peek.MessageId))
				{
					// first message is not expired, then no message is expired in queue.
					return;
				}

				// try not to remove elements that are added while we remove
				Message<T> Message = IncomingMessages.poll();
				checkState(Message is TopicMessageImpl);
				while (Message != null)
				{
					IncomingMessagesSizeUpdater.addAndGet(this, -Message.Data.Length);
					MessageId MessageId = Message.MessageId;
					if (!MessageIds.Contains(MessageId))
					{
						MessageIds.Add(MessageId);
						break;
					}
					Message = IncomingMessages.poll();
				}
			}
		}

		private bool TopicNameValid(string TopicName)
		{
			checkArgument(TopicName.isValid(TopicName), "Invalid topic name:" + TopicName);
			checkArgument(!TopicsConflict.ContainsKey(TopicName), "Topics already contains topic:" + TopicName);

			if (this.NamespaceName != null)
			{
				checkArgument(TopicName.get(TopicName).Namespace.ToString().Equals(this.NamespaceName.ToString()), "Topic " + TopicName + " not in same namespace with Topics");
			}

			return true;
		}

		// subscribe one more given topic
		public virtual CompletableFuture<Void> SubscribeAsync(string TopicName, bool CreateTopicIfDoesNotExist)
		{
			if (!TopicNameValid(TopicName))
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topic name not valid"));
			}

			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			CompletableFuture<Void> SubscribeResult = new CompletableFuture<Void>();

			ClientConflict.getPartitionedTopicMetadata(TopicName).thenAccept(metadata => subscribeTopicPartitions(SubscribeResult, TopicName, metadata.partitions, CreateTopicIfDoesNotExist)).exceptionally(ex1 =>
			{
			log.warn("[{}] Failed to get partitioned topic metadata: {}", TopicName, ex1.Message);
			SubscribeResult.completeExceptionally(ex1);
			return null;
			});

			return SubscribeResult;
		}

		// create consumer for a single topic with already known partitions.
		// first create a consumer with no topic, then do subscription for already know partitionedTopic.
		public static MultiTopicsConsumerImpl<T> CreatePartitionedConsumer<T>(PulsarClientImpl Client, ConsumerConfigurationData<T> Conf, ExecutorService ListenerExecutor, CompletableFuture<Consumer<T>> SubscribeFuture, int NumPartitions, Schema<T> Schema, ConsumerInterceptors<T> Interceptors)
		{
			checkArgument(Conf.TopicNames.size() == 1, "Should have only 1 topic for partitioned consumer");

			// get topic name, then remove it from conf, so constructor will create a consumer with no topic.
			ConsumerConfigurationData CloneConf = Conf.clone();
			string TopicName = CloneConf.SingleTopic;
			CloneConf.TopicNames.remove(TopicName);

			CompletableFuture<Consumer> Future = new CompletableFuture<Consumer>();
			MultiTopicsConsumerImpl Consumer = new MultiTopicsConsumerImpl(Client, TopicName, CloneConf, ListenerExecutor, Future, Schema, Interceptors, true);

			Future.thenCompose(c => ((MultiTopicsConsumerImpl)c).SubscribeAsync(TopicName, NumPartitions)).thenRun(() => SubscribeFuture.complete(Consumer)).exceptionally(e =>
			{
			log.warn("Failed subscription for createPartitionedConsumer: {} {}, e:{}", TopicName, NumPartitions, e);
			SubscribeFuture.completeExceptionally(PulsarClientException.wrap(((Exception) e).InnerException, string.Format("Failed to subscribe {0} with {1:D} partitions", TopicName, NumPartitions)));
			return null;
			});
			return Consumer;
		}

		// subscribe one more given topic, but already know the numberPartitions
		private CompletableFuture<Void> SubscribeAsync(string TopicName, int NumberPartitions)
		{
			if (!TopicNameValid(TopicName))
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topic name not valid"));
			}

			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			CompletableFuture<Void> SubscribeResult = new CompletableFuture<Void>();
			SubscribeTopicPartitions(SubscribeResult, TopicName, NumberPartitions, true);

			return SubscribeResult;
		}

		private void SubscribeTopicPartitions(CompletableFuture<Void> SubscribeResult, string TopicName, int NumPartitions, bool CreateIfDoesNotExist)
		{
			ClientConflict.preProcessSchemaBeforeSubscribe(ClientConflict, Schema, TopicName).whenComplete((ignored, cause) =>
			{
			if (null == cause)
			{
				DoSubscribeTopicPartitions(SubscribeResult, TopicName, NumPartitions, CreateIfDoesNotExist);
			}
			else
			{
				SubscribeResult.completeExceptionally(cause);
			}
			});
		}

		private void DoSubscribeTopicPartitions(CompletableFuture<Void> SubscribeResult, string TopicName, int NumPartitions, bool CreateIfDoesNotExist)
		{
			if (log.DebugEnabled)
			{
				log.debug("Subscribe to topic {} metadata.partitions: {}", TopicName, NumPartitions);
			}

			IList<CompletableFuture<Consumer<T>>> FutureList;
			if (NumPartitions > 0)
			{
				this.TopicsConflict.GetOrAdd(TopicName, NumPartitions);
				AllTopicPartitionsNumber.addAndGet(NumPartitions);

				int ReceiverQueueSize = Math.Min(Conf.ReceiverQueueSize, Conf.MaxTotalReceiverQueueSizeAcrossPartitions / NumPartitions);
				ConsumerConfigurationData<T> ConfigurationData = InternalConsumerConfig;
				ConfigurationData.ReceiverQueueSize = ReceiverQueueSize;

				FutureList = IntStream.range(0, NumPartitions).mapToObj(partitionIndex =>
				{
				string PartitionName = TopicName.get(TopicName).getPartition(partitionIndex).ToString();
				CompletableFuture<Consumer<T>> SubFuture = new CompletableFuture<Consumer<T>>();
				ConsumerImpl<T> NewConsumer = ConsumerImpl.NewConsumerImpl(ClientConflict, PartitionName, ConfigurationData, ClientConflict.externalExecutorProvider().Executor, partitionIndex, true, SubFuture, SubscriptionMode.Durable, null, Schema, Interceptors, CreateIfDoesNotExist);
				consumers.GetOrAdd(NewConsumer.Topic, NewConsumer);
				return SubFuture;
				}).collect(Collectors.toList());
			}
			else
			{
				this.TopicsConflict.GetOrAdd(TopicName, 1);
				AllTopicPartitionsNumber.incrementAndGet();

				CompletableFuture<Consumer<T>> SubFuture = new CompletableFuture<Consumer<T>>();
				ConsumerImpl<T> NewConsumer = ConsumerImpl.NewConsumerImpl(ClientConflict, TopicName, internalConfig, ClientConflict.externalExecutorProvider().Executor, -1, true, SubFuture, SubscriptionMode.Durable, null, Schema, Interceptors, CreateIfDoesNotExist);
				consumers.GetOrAdd(NewConsumer.Topic, NewConsumer);

				FutureList = Collections.singletonList(SubFuture);
			}

			FutureUtil.waitForAll(FutureList).thenAccept(finalFuture =>
			{
			if (AllTopicPartitionsNumber.get() > MaxReceiverQueueSizeConflict)
			{
				MaxReceiverQueueSize = AllTopicPartitionsNumber.get();
			}
			int NumTopics = this.TopicsConflict.Values.Select(int?.intValue).Sum();
			checkState(AllTopicPartitionsNumber.get() == NumTopics, "allTopicPartitionsNumber " + AllTopicPartitionsNumber.get() + " not equals expected: " + NumTopics);
			StartReceivingMessages(consumers.Values.Where(consumer1 =>
			{
				string ConsumerTopicName = consumer1.Topic;
				if (TopicName.get(ConsumerTopicName).PartitionedTopicName.Equals(TopicName.get(TopicName).PartitionedTopicName.ToString()))
				{
					return true;
				}
				else
				{
					return false;
				}
			}).ToList());
			SubscribeResult.complete(null);
			log.info("[{}] [{}] Success subscribe new topic {} in topics consumer, partitions: {}, allTopicPartitionsNumber: {}", Topic, SubscriptionConflict, TopicName, NumPartitions, AllTopicPartitionsNumber.get());
			if (this.NamespaceName == null)
			{
				this.NamespaceName = TopicName.get(TopicName).NamespaceObject;
			}
			return;
			}).exceptionally(ex =>
			{
			HandleSubscribeOneTopicError(TopicName, ex, SubscribeResult);
			return null;
		});
		}

		// handling failure during subscribe new topic, unsubscribe success created partitions
		private void HandleSubscribeOneTopicError(string TopicName, Exception Error, CompletableFuture<Void> SubscribeFuture)
		{
			log.warn("[{}] Failed to subscribe for topic [{}] in topics consumer {}", Topic, TopicName, Error.Message);

			ClientConflict.externalExecutorProvider().Executor.submit(() =>
			{
			AtomicInteger ToCloseNum = new AtomicInteger(0);
			consumers.Values.Where(consumer1 =>
			{
				string ConsumerTopicName = consumer1.Topic;
				if (TopicName.get(ConsumerTopicName).PartitionedTopicName.Equals(TopicName))
				{
					ToCloseNum.incrementAndGet();
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
					consumer2.subscribeFuture().completeExceptionally(Error);
					AllTopicPartitionsNumber.decrementAndGet();
					consumers.Remove(consumer2.Topic);
					if (ToCloseNum.decrementAndGet() == 0)
					{
						log.warn("[{}] Failed to subscribe for topic [{}] in topics consumer, subscribe error: {}", Topic, TopicName, Error.Message);
						TopicsConflict.Remove(TopicName);
						checkState(AllTopicPartitionsNumber.get() == consumers.Values.Count);
						SubscribeFuture.completeExceptionally(Error);
					}
					return;
				});
			});
			});
		}

		// un-subscribe a given topic
		public virtual CompletableFuture<Void> UnsubscribeAsync(string TopicName)
		{
			checkArgument(TopicName.isValid(TopicName), "Invalid topic name:" + TopicName);

			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			if (PartitionsAutoUpdateTimeout != null)
			{
				PartitionsAutoUpdateTimeout.cancel();
				PartitionsAutoUpdateTimeout = null;
			}

			CompletableFuture<Void> UnsubscribeFuture = new CompletableFuture<Void>();
			string TopicPartName = TopicName.get(TopicName).PartitionedTopicName;

			IList<ConsumerImpl<T>> ConsumersToUnsub = consumers.Values.Where(consumer =>
			{
			string ConsumerTopicName = consumer.Topic;
			if (TopicName.get(ConsumerTopicName).PartitionedTopicName.Equals(TopicPartName))
			{
				return true;
			}
			else
			{
				return false;
			}
			}).ToList();

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			IList<CompletableFuture<Void>> FutureList = ConsumersToUnsub.Select(ConsumerImpl::unsubscribeAsync).ToList();

			FutureUtil.waitForAll(FutureList).whenComplete((r, ex) =>
			{
			if (ex == null)
			{
				ConsumersToUnsub.ForEach(consumer1 =>
				{
					consumers.Remove(consumer1.Topic);
					pausedConsumers.remove(consumer1);
					AllTopicPartitionsNumber.decrementAndGet();
				});
				TopicsConflict.Remove(TopicName);
				((UnAckedTopicMessageTracker) UnAckedMessageTracker).RemoveTopicMessages(TopicName);
				UnsubscribeFuture.complete(null);
				log.info("[{}] [{}] [{}] Unsubscribed Topics Consumer, allTopicPartitionsNumber: {}", TopicName, SubscriptionConflict, ConsumerNameConflict, AllTopicPartitionsNumber);
			}
			else
			{
				UnsubscribeFuture.completeExceptionally(ex);
				State = State.Failed;
				log.error("[{}] [{}] [{}] Could not unsubscribe Topics Consumer", TopicName, SubscriptionConflict, ConsumerNameConflict, ex.Cause);
			}
			});

			return UnsubscribeFuture;
		}

		// Remove a consumer for a topic
		public virtual CompletableFuture<Void> RemoveConsumerAsync(string TopicName)
		{
			checkArgument(TopicName.isValid(TopicName), "Invalid topic name:" + TopicName);

			if (State == State.Closing || State == State.Closed)
			{
				return FutureUtil.failedFuture(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed"));
			}

			CompletableFuture<Void> UnsubscribeFuture = new CompletableFuture<Void>();
			string TopicPartName = TopicName.get(TopicName).PartitionedTopicName;


			IList<ConsumerImpl<T>> ConsumersToClose = consumers.Values.Where(consumer =>
			{
			string ConsumerTopicName = consumer.Topic;
			if (TopicName.get(ConsumerTopicName).PartitionedTopicName.Equals(TopicPartName))
			{
				return true;
			}
			else
			{
				return false;
			}
			}).ToList();

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			IList<CompletableFuture<Void>> FutureList = ConsumersToClose.Select(ConsumerImpl::closeAsync).ToList();

			FutureUtil.waitForAll(FutureList).whenComplete((r, ex) =>
			{
			if (ex == null)
			{
				ConsumersToClose.ForEach(consumer1 =>
				{
					consumers.Remove(consumer1.Topic);
					pausedConsumers.remove(consumer1);
					AllTopicPartitionsNumber.decrementAndGet();
				});
				TopicsConflict.Remove(TopicName);
				((UnAckedTopicMessageTracker) UnAckedMessageTracker).RemoveTopicMessages(TopicName);
				UnsubscribeFuture.complete(null);
				log.info("[{}] [{}] [{}] Removed Topics Consumer, allTopicPartitionsNumber: {}", TopicName, SubscriptionConflict, ConsumerNameConflict, AllTopicPartitionsNumber);
			}
			else
			{
				UnsubscribeFuture.completeExceptionally(ex);
				State = State.Failed;
				log.error("[{}] [{}] [{}] Could not remove Topics Consumer", TopicName, SubscriptionConflict, ConsumerNameConflict, ex.Cause);
			}
			});

			return UnsubscribeFuture;
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

		public override void Pause()
		{
			consumers.forEach((name, consumer) => consumer.pause());
		}

		public override void Resume()
		{
			consumers.forEach((name, consumer) => consumer.resume());
		}

		// This listener is triggered when topics partitions are updated.
		public class TopicsPartitionChangedListener : PartitionsChangedListener
		{
			private readonly MultiTopicsConsumerImpl<T> outerInstance;

			public TopicsPartitionChangedListener(MultiTopicsConsumerImpl<T> outerInstance)
			{
				this.outerInstance = OuterInstance;
			}

			// Check partitions changes of passed in topics, and subscribe new added partitions.
			public override CompletableFuture<Void> OnTopicsExtended(ICollection<string> TopicsExtended)
			{
				CompletableFuture<Void> Future = new CompletableFuture<Void>();
				if (TopicsExtended.Count == 0)
				{
					Future.complete(null);
					return Future;
				}

				if (log.DebugEnabled)
				{
					log.debug("[{}]  run onTopicsExtended: {}, size: {}", outerInstance.Topic, TopicsExtended.ToString(), TopicsExtended.Count);
				}

				IList<CompletableFuture<Void>> FutureList = Lists.newArrayListWithExpectedSize(TopicsExtended.Count);
				TopicsExtended.forEach(outerInstance.Topic => FutureList.Add(outerInstance.subscribeIncreasedTopicPartitions(outerInstance.Topic)));
				FutureUtil.waitForAll(FutureList).thenAccept(finalFuture => Future.complete(null)).exceptionally(ex =>
				{
				log.warn("[{}] Failed to subscribe increased topics partitions: {}", outerInstance.Topic, ex.Message);
				Future.completeExceptionally(ex);
				return null;
				});

				return Future;
			}
		}

		// subscribe increased partitions for a given topic
		private CompletableFuture<Void> SubscribeIncreasedTopicPartitions(string TopicName)
		{
			CompletableFuture<Void> Future = new CompletableFuture<Void>();

			ClientConflict.getPartitionsForTopic(TopicName).thenCompose(list =>
			{
			int OldPartitionNumber = TopicsConflict[TopicName.ToString()];
			int CurrentPartitionNumber = list.size();
			if (log.DebugEnabled)
			{
				log.debug("[{}] partitions number. old: {}, new: {}", TopicName.ToString(), OldPartitionNumber, CurrentPartitionNumber);
			}
			if (OldPartitionNumber == CurrentPartitionNumber)
			{
				Future.complete(null);
				return Future;
			}
			else if (OldPartitionNumber < CurrentPartitionNumber)
			{
				IList<string> NewPartitions = list.subList(OldPartitionNumber, CurrentPartitionNumber);
				IList<CompletableFuture<Consumer<T>>> FutureList = NewPartitions.Select(partitionName =>
				{
					int PartitionIndex = TopicName.getPartitionIndex(partitionName);
					CompletableFuture<Consumer<T>> SubFuture = new CompletableFuture<Consumer<T>>();
					ConsumerConfigurationData<T> ConfigurationData = InternalConsumerConfig;
					ConsumerImpl<T> NewConsumer = ConsumerImpl.NewConsumerImpl(ClientConflict, partitionName, ConfigurationData, ClientConflict.externalExecutorProvider().Executor, PartitionIndex, true, SubFuture, SubscriptionMode.Durable, null, Schema, Interceptors, true);
					consumers.GetOrAdd(NewConsumer.Topic, NewConsumer);
					if (log.DebugEnabled)
					{
						log.debug("[{}] create consumer {} for partitionName: {}", TopicName.ToString(), NewConsumer.Topic, partitionName);
					}
					return SubFuture;
				}).ToList();
				FutureUtil.waitForAll(FutureList).thenAccept(finalFuture =>
				{
					IList<ConsumerImpl<T>> NewConsumerList = NewPartitions.Select(partitionTopic => consumers[partitionTopic]).ToList();
					StartReceivingMessages(NewConsumerList);
					Future.complete(null);
				}).exceptionally(ex =>
				{
					log.warn("[{}] Failed to subscribe {} partition: {} - {}", Topic, TopicName.ToString(), OldPartitionNumber, CurrentPartitionNumber, ex.Message);
					Future.completeExceptionally(ex);
					return null;
				});
			}
			else
			{
				log.error("[{}] not support shrink topic partitions. old: {}, new: {}", TopicName.ToString(), OldPartitionNumber, CurrentPartitionNumber);
				Future.completeExceptionally(new NotSupportedException("not support shrink topic partitions"));
			}
			return Future;
			});

			return Future;
		}

		private TimerTask partitionsAutoUpdateTimerTask = new TimerTaskAnonymousInnerClass();

		public class TimerTaskAnonymousInnerClass : TimerTask
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
			public override void run(Timeout Timeout)
			{
				if (Timeout.Cancelled || outerInstance.State != State.Ready)
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
				outerInstance.PartitionsAutoUpdateTimeout = outerInstance.client.timer().newTimeout(partitionsAutoUpdateTimerTask, 1, BAMCIS.Util.Concurrent.TimeUnit.MINUTES);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public io.netty.util.Timeout getPartitionsAutoUpdateTimeout()

		public override CompletableFuture<MessageId> LastMessageIdAsync
		{
			get
			{
				CompletableFuture<MessageId> ReturnFuture = new CompletableFuture<MessageId>();
    
				IDictionary<string, CompletableFuture<MessageId>> MessageIdFutures = consumers.SetOfKeyValuePairs().Select(entry => Pair.of(entry.Key,entry.Value.LastMessageIdAsync)).ToDictionary(Pair.getKey, Pair.getValue);
    
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: java.util.concurrent.CompletableFuture.allOf(messageIdFutures.entrySet().stream().map(java.util.Map.Entry::getValue).toArray(java.util.concurrent.CompletableFuture<?>[]::new)).whenComplete((ignore, ex) ->
	//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				CompletableFuture.allOf(MessageIdFutures.SetOfKeyValuePairs().Select(DictionaryEntry.getValue).ToArray(CompletableFuture<object>[]::new)).whenComplete((ignore, ex) =>
				{
				Builder<string, MessageId> Builder = ImmutableMap.builder<string, MessageId>();
				MessageIdFutures.forEach((key, future) =>
				{
					MessageId MessageId;
					try
					{
						MessageId = future.get();
					}
					catch (Exception E)
					{
						log.warn("[{}] Exception when topic {} getLastMessageId.", key, E);
						MessageId = MessageId.earliest;
					}
					Builder.put(key, MessageId);
				});
				ReturnFuture.complete(new MultiMessageIdImpl(Builder.build()));
				});
    
				return ReturnFuture;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(MultiTopicsConsumerImpl));
	}

}