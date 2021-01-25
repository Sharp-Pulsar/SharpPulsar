using Akka.Actor;
using Akka.Util.Internal;
using App.Metrics.Concurrency;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Batch;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Impl;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Queues;
using SharpPulsar.Stats.Consumer;
using SharpPulsar.Stats.Consumer.Api;
using SharpPulsar.Tracker;
using SharpPulsar.Tracker.Messages;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SharpPulsar.Protocol.Proto.CommandAck;

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
		private readonly Dictionary<string, (string Topic , IActorRef Consumer)> _consumers;

		// Map <topic, numPartitions>, store partition number for each topic
		protected internal readonly Dictionary<string, int> TopicsMap;

		// Queue of partition consumers on which we have stopped calling receiveAsync() because the
		// shared incoming queue was full
		private readonly Queue<IActorRef> _pausedConsumers;

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
		private readonly ClientConfigurationData _clientConfiguration;
		private readonly IActorRef _client;

		internal MultiTopicsConsumer(IActorRef client, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : this(client, DummyTopicNamePrefix + Utility.ConsumerName.GenerateRandomName(), conf, listenerExecutor, schema, interceptors, createTopicIfDoesNotExist, queue)
		{
		}

		internal MultiTopicsConsumer(IActorRef client, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, IMessageId startMessageId, long startMessageRollbackDurationInSec, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : this(client, DummyTopicNamePrefix + Utility.ConsumerName.GenerateRandomName(), conf, listenerExecutor, schema, interceptors, createTopicIfDoesNotExist, startMessageId, startMessageRollbackDurationInSec, clientConfiguration, queue)
		{
		}

		internal MultiTopicsConsumer(IActorRef client, string singleTopic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : this(client, singleTopic, conf, listenerExecutor, schema, interceptors, createTopicIfDoesNotExist, null, 0, clientConfiguration, queue)
		{
		}

		internal MultiTopicsConsumer(IActorRef client, string singleTopic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, IMessageId startMessageId, long startMessageRollbackDurationInSec, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : base(client, singleTopic, conf, Math.Max(2, conf.ReceiverQueueSize), listenerExecutor, schema, interceptors, queue)
		{

			Condition.CheckArgument(conf.ReceiverQueueSize > 0, "Receiver queue size needs to be greater than 0 for Topics Consumer");
			_clientConfiguration = clientConfiguration;
			TopicsMap = new Dictionary<string, int>();
			_consumers = new Dictionary<string, (string Topic, IActorRef Consumer)>();
			_pausedConsumers = new Queue<IActorRef>();
			_sharedQueueResumeThreshold = MaxReceiverQueueSize / 2;
			AllTopicPartitionsNumber = new AtomicInteger(0);
			_startMessageId = startMessageId != null ? new BatchMessageId(MessageId.ConvertToMessageId(startMessageId)) : null;
			_startMessageRollbackDurationInSec = startMessageRollbackDurationInSec;

			if (conf.AckTimeoutMillis != 0)
			{
				if (conf.TickDurationMillis > 0)
				{
					_unAckedMessageTracker = Context.ActorOf(UnAckedTopicMessageTracker.Prop(conf.AckTimeoutMillis, conf.TickDurationMillis, Self), "UnAckedTopicMessageTracker");
				}
				else
				{
					_unAckedMessageTracker = Context.ActorOf(UnAckedTopicMessageTracker.Prop(Self, conf.AckTimeoutMillis), "UnAckedTopicMessageTracker");
				}
			}
			else
			{
				_unAckedMessageTracker = Context.ActorOf(UnAckedMessageTrackerDisabled.Prop(), "UnAckedMessageTrackerDisabled");
			}

			_internalConfig = InternalConsumerConfig;
			_stats = _clientConfiguration.StatsIntervalSeconds > 0 ? new ConsumerStatsRecorder<T>(Context.System, conf, Topic, ConsumerName, Subscription, clientConfiguration.StatsIntervalSeconds) : ConsumerStatsDisabled.Instance;

			// start track and auto subscribe partition increasement
			if(conf.AutoUpdatePartitions)
			{
				TopicsPartitionChangedListener = new TopicsPartitionChangedListener(this);
				_partitionsAutoUpdateTimeout = client.Timer().newTimeout(_partitionsAutoUpdateTimerTask, conf.AutoUpdatePartitionsIntervalSeconds, TimeUnit.SECONDS);
			}

			if(conf.TopicNames.Count == 0)
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

		private void ReceiveMessageFromConsumer(IActorRef consumer)
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

		private void MessageReceived(IActorRef consumer, IMessage<T> message)
		{
			Condition.CheckArgument(message is Message<T>);
			var topicMessage = new TopicMessage<T>(consumer.Topic, consumer.TopicNameWithoutPartition, message);

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
			if(IncomingMessages.Count <= _sharedQueueResumeThreshold && _pausedConsumers.Count > 0)
			{
				while(true)
				{
					if(!_pausedConsumers.TryDequeue(out var consumer))
					{
						break;
					}

					Task.Run(() => {
						ReceiveMessageFromConsumer(consumer);
					});
				}
			}
		}

		protected internal override IMessage<T> InternalReceive()
		{
			IMessage<T> message;
			try
			{
				message = IncomingMessages.Take();
				IncomingMessagesSize -= message.Data.Length;
				Condition.CheckState(message is TopicMessage<T>);
				_unAckedMessageTracker.Tell(new Add(message.MessageId));
				ResumeReceivingFromPausedConsumersIfNeeded();
				return message;
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		protected internal override IMessage<T> InternalReceive(int timeout, TimeUnit unit)
		{
            try
            {
                if (IncomingMessages.TryTake(out IMessage<T> message, (int)unit.ToMilliseconds(timeout)))
                {
                    IncomingMessagesSize -= message.Data.Length;
                    Condition.CheckArgument(message is TopicMessage<T>);
                    _unAckedMessageTracker.Tell(new Add(message.MessageId));
                }
                ResumeReceivingFromPausedConsumersIfNeeded();
                return message;
            }
            catch (Exception e)
            {
                throw PulsarClientException.Unwrap(e);
            }
        }

		protected internal override IMessages<T> InternalBatchReceive()
		{
			try
			{
				var messages = NewMessages;
				while (IncomingMessages.TryTake(out var msgPeeked) && messages.CanAdd(msgPeeked))
				{
					IncomingMessagesSize -= msgPeeked.Data.Length;
					var interceptMsg = BeforeConsume(msgPeeked);
					messages.Add(interceptMsg);
				}
				ResumeReceivingFromPausedConsumersIfNeeded();
				return messages;
			}
			catch(Exception e) 
			{
				HandlerState.State state = State.ConnectionState;
				if(state != HandlerState.State.Closing && state != HandlerState.State.Closed)
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


		protected internal override void DoAcknowledge(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txnImpl)
		{
			Condition.CheckArgument(messageId is TopicMessageId);
			var topicMessageId = (TopicMessageId) messageId;

			if(State.ConnectionState != HandlerState.State.Ready)
			{
				throw new PulsarClientException("Consumer already closed");
			}

			if(ackType == AckType.Cumulative)
			{
				var (topic, consumer) = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				if(consumer != null)
				{
					var innerId = topicMessageId.InnerMessageId;
					consumer.Tell(new AcknowledgeCumulativeMessageId(innerId));
				}
				else
				{
					throw new PulsarClientException.NotConnectedException();
				}
			}
			else
			{
				var (topic, consumer) = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);

				var innerId = topicMessageId.InnerMessageId;
				consumer.Ask(new AcknowledgeWithTxn(new List<IMessageId> { innerId }, ackType, properties, txnImpl)).ContinueWith
					(t=> {
						_unAckedMessageTracker.Tell(new Remove(topicMessageId));
					});
			}
		}

		protected internal override void DoAcknowledge(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
		{
			if(ackType == AckType.Cumulative)
			{
				messageIdList.ForEach(messageId => DoAcknowledge(messageId, ackType, properties, txn));
			}
			else
			{
				if(State.ConnectionState != HandlerState.State.Ready)
				{
					throw new PulsarClientException("Consumer already closed");
				}
				IDictionary<string, IList<IMessageId>> topicToMessageIdMap = new Dictionary<string, IList<IMessageId>>();
				foreach(IMessageId messageId in messageIdList)
				{
					if(!(messageId is TopicMessageId))
					{
						throw new ArgumentException("messageId is not instance of TopicMessageIdImpl");
					}
					var topicMessageId = (TopicMessageId) messageId;
					if(!topicToMessageIdMap.ContainsKey(topicMessageId.TopicPartitionName)) 
						topicToMessageIdMap.Add(topicMessageId.TopicPartitionName, new List<IMessageId>());

					topicToMessageIdMap.GetValueOrNull(topicMessageId.TopicPartitionName)
						.Add(topicMessageId.InnerMessageId);
				}
				topicToMessageIdMap.ForEach(t =>
				{
					var consumer = _consumers.GetValueOrNull(t.Key);
					consumer.Consumer.Tell(new AcknowledgeWithTxn(t.Value, ackType, properties, txn));
					messageIdList.ForEach(x => _unAckedMessageTracker.Tell(new Remove(x)));
				});
			}
		}

		protected internal override void DoReconsumeLater<T1>(IMessage<T1> message, AckType ackType, IDictionary<string, long> properties, long delayTime, TimeUnit unit)
		{
			var messageId = message.MessageId;
			Condition.CheckArgument(messageId is TopicMessageId);
			var topicMessageId = (TopicMessageId) messageId;
			if(State.ConnectionState != HandlerState.State.Ready)
			{
				throw new PulsarClientException("Consumer already closed");
			}

			if(ackType == AckType.Cumulative)
			{
				var(topic, consumer) = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				if(consumer != null)
				{
					var innerId = topicMessageId.InnerMessageId;
					consumer.Tell(new ReconsumeLaterCumulative<T1>(message, delayTime, unit));
				}
				else
				{
					throw new PulsarClientException.NotConnectedException();
				}
			}
			else
			{
				var (topic, consumer) = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				var innerId = topicMessageId.InnerMessageId;
				consumer.Tell(new ReconsumeLaterWithProperties<T1>(message, ackType, properties, delayTime, unit));
				_unAckedMessageTracker.Tell(new Remove(topicMessageId));
			}
		}

		internal override void NegativeAcknowledge(IMessageId messageId)
		{
			Condition.CheckArgument(messageId is TopicMessageId);
			var topicMessageId = (TopicMessageId) messageId;

			var (topic, consumer) = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
			consumer.Tell(new NegativeAcknowledgeMessageId(topicMessageId.InnerMessageId));
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


		internal override bool Connected
		{
			get
			{
				return _consumers.Values.All(consumer => consumer.Consumer.AskFor<bool>(IsConnected.Instance));
			}
		}

		internal string HandlerName
		{
			get
			{
				return Subscription;
			}
		}

		private ConsumerConfigurationData<T> InternalConsumerConfig
		{
			get
			{
				ConsumerConfigurationData<T> internalConsumerConfig = Conf;
				internalConsumerConfig.SubscriptionName = Subscription;
				internalConsumerConfig.ConsumerName = ConsumerName;
				internalConsumerConfig.MessageListener = null;
				return internalConsumerConfig;
			}
		}

		internal override void RedeliverUnacknowledgedMessages()
		{
			_consumers.Values.ForEach(consumer =>
			{
				consumer.Consumer.Tell(Messages.Consumer.RedeliverUnacknowledgedMessages.Instance);
				consumer.Consumer.Tell(ClearUnAckedChunckedMessageIdSequenceMap.Instance);
			});
			IncomingMessages = new BlockingCollection<IMessage<T>>();
			IncomingMessagesSize =  0;
			_unAckedMessageTracker.Tell(Clear.Instance);
			ResumeReceivingFromPausedConsumersIfNeeded();
		}

		protected internal override void RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds)
		{
			if(messageIds.Count == 0)
			{
				return;
			}

			Condition.CheckArgument(messageIds.First() is TopicMessageId);

			if(Conf.SubscriptionType != CommandSubscribe.SubType.Shared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				RedeliverUnacknowledgedMessages();
				return;
			}
			RemoveExpiredMessagesFromQueue(messageIds);
			messageIds.Select(messageId => (TopicMessageId)messageId).Collect()
				.ForEach(t => _consumers.GetValueOrNull(t.First().TopicPartitionName)
				.Consumer.Tell(new RedeliverUnacknowledgedMessageIds(t.Select(mid => mid.InnerMessageId).ToHashSet())));
			ResumeReceivingFromPausedConsumersIfNeeded();
		}

		internal override void Seek(IMessageId messageId)
		{
			try
			{
				var targetMessageId = MessageId.ConvertToMessageId(messageId);
				if (targetMessageId == null || IsIllegalMultiTopicsMessageId(messageId))
				{
					throw new PulsarClientException("Illegal messageId, messageId can only be earliest/latest");
				}
				_consumers.Values.ForEach(c => c.Consumer.Tell(new SeekMessageId(targetMessageId)));

				_unAckedMessageTracker.Tell(Clear.Instance);
				IncomingMessages = new BlockingCollection<IMessage<T>>();
				IncomingMessagesSize = 0;
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
				_consumers.Values.ForEach(c => c.Consumer.Tell(new SeekTimestamp(timestamp)));
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}


		internal override int AvailablePermits
		{
			get
			{
				return _consumers.Values.Select(x => x.Consumer.AskFor<int>(GetAvailablePermits.Instance)).Sum();
			}
		}

		internal bool HasReachedEndOfTopic()
		{
			return _consumers.Values.All(c => c.Consumer.AskFor<bool>(Messages.Consumer.HasReachedEndOfTopic.Instance));
		}

		public virtual bool HasMessageAvailable()
		{
			try
			{
				return _consumers.Values.All(c => c.Consumer.AskFor<bool>(Messages.Consumer.HasMessageAvailable.Instance));
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		internal override int NumMessagesInQueue()
		{
			return IncomingMessages.Count + _consumers.Values.Select(c => c.Consumer.AskFor<int>(GetNumMessagesInQueue.Instance)).Sum();
		}

		internal override IConsumerStats Stats
		{
			get
			{
				if (_stats == null)
				{
					return null;
				}
				_stats.Reset();

				_consumers.Values.ForEach(consumer => _stats.UpdateCumulativeStats(consumer.Consumer.AskFor<IConsumerStats>(GetStats.Instance)));
				return _stats;
			}
		}

		public virtual IActorRef UnAckedMessageTracker
		{
			get
			{
				return _unAckedMessageTracker;
			}
		}

		private void RemoveExpiredMessagesFromQueue(ISet<IMessageId> messageIds)
		{
			if(IncomingMessages.TryTake(out var peek))
			{
				if(!messageIds.Contains(peek.MessageId))
				{
					// first message is not expired, then no message is expired in queue.
					return;
				}

				// try not to remove elements that are added while we remove
				var message = peek;
				Condition.CheckState(message is TopicMessage);
				while(message != null)
				{
					IncomingMessagesSize -= message.Data.Length;
					var messageId = message.MessageId;
					if(!messageIds.Contains(messageId))
					{
						messageIds.Add(messageId);
						break;
					}
					message = IncomingMessages.Take();
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
			if(!ReferenceEquals(fullTopicName, null))
			{
				TopicsMap.Remove(topic);
			}
		}

		// subscribe one more given topic
		public virtual void Subscribe(string topicName, bool createTopicIfDoesNotExist)
		{
			TopicName topicNameInstance = GetTopicName(topicName);
			if(topicNameInstance == null)
			{
				throw new PulsarClientException.AlreadyClosedException("Topic name not valid");
			}
			string fullTopicName = topicNameInstance.ToString();
			if(TopicsMap.ContainsKey(fullTopicName) || TopicsMap.ContainsKey(topicNameInstance.PartitionedTopicName))
			{
				throw new PulsarClientException.AlreadyClosedException("Already subscribed to " + topicName);
			}

			if(State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				throw new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed");
			}

			var result = _client.AskFor(new GetPartitionedTopicMetadata(topicNameInstance));
			if (result is PartitionedTopicMetadata metadata)
			{
				SubscribeTopicPartitions(fullTopicName, metadata.Partitions, createTopicIfDoesNotExist);
			}
			else if (result is Failure failure)
				throw failure.Exception;
		}

		// create consumer for a single topic with already known partitions.
		// first create a consumer with no topic, then do subscription for already know partitionedTopic.
		public static Props CreatePartitionedConsumer(IActorRef client, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, int numPartitions, ISchema<T> schema, ConsumerInterceptors<T> interceptors)
		{
			Condition.CheckArgument(conf.TopicNames.Count == 1, "Should have only 1 topic for partitioned consumer");

			// get topic name, then remove it from conf, so constructor will create a consumer with no topic.
			var cloneConf = conf;
			string topicName = cloneConf.SingleTopic;
			cloneConf.TopicNames.Remove(topicName);
			var consumer = new MultiTopicsConsumer(client, topicName, cloneConf, listenerExecutor, future, schema, interceptors, true);

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

		private void SubscribeTopicPartitions(string topicName, int numPartitions, bool createIfDoesNotExist)
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
		public virtual void Unsubscribe(string topicName)
		{
			Condition.CheckArgument(TopicName.IsValid(topicName), "Invalid topic name:" + topicName);

			if(State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				throw new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed");
			}

			if(_partitionsAutoUpdateTimeout != null)
			{
				_partitionsAutoUpdateTimeout.Cancel();
				_partitionsAutoUpdateTimeout = null;
			}

			string topicPartName = TopicName.Get(topicName).PartitionedTopicName;

			var consumersToUnsub = _consumers.Values.Where(consumer =>
			{
				string consumerTopicName = consumer.Topic;
				return TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(topicPartName);
			}).ToList();

			var tasks = consumersToUnsub.Select(c => c.Consumer.Ask(Messages.Consumer.Unsubscribe.Instance)).ToList();
			Task.WhenAll(tasks).ContinueWith(t => {
                if (!t.IsFaulted)
                {
					var toUnsub = consumersToUnsub;
					toUnsub.ForEach(consumer1 =>
					{
						_consumers.Remove(consumer1.Topic);
						_pausedConsumers.Remove(consumer1.Consumer);
						AllTopicPartitionsNumber.decrementAndGet();
					});
					RemoveTopic(topicName);
					_unAckedMessageTracker.Tell(new RemoveTopicMessages(topicName));
					_log.Info($"[{topicName}] [{Subscription}] [{ConsumerName}] Unsubscribed Topics Consumer, allTopicPartitionsNumber: {AllTopicPartitionsNumber}");
				}
				else
				{

					State.ConnectionState = HandlerState.State.Failed;
					_log.Error($"[{topicName}] [{Subscription}] [{ConsumerName}] Could not unsubscribe Topics Consumer: {t.Exception}");
				}
			});
		}

		// Remove a consumer for a topic
		public virtual void RemoveConsumerAsync(string topicName)
		{
			Condition.CheckArgument(TopicName.IsValid(topicName), "Invalid topic name:" + topicName);

			if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				throw new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed");
			}

			string topicPartName = TopicName.Get(topicName).PartitionedTopicName;


			var consumersToClose = _consumers.Values.Where(consumer =>
			{
				string consumerTopicName = consumer.Topic;
				return TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(topicPartName);
			}).ToList();

			var tasks = consumersToClose.Select(x => x.Consumer.GracefulStop(TimeSpan.FromSeconds(1))).ToList();
			Task.WhenAll(tasks).ContinueWith(t => 
			{
                if (!t.IsFaulted)
                {
					consumersToClose.ForEach(consumer1 =>
					{
						_consumers.Remove(consumer1.Topic);
						_pausedConsumers.Remove(consumer1.Consumer);
						AllTopicPartitionsNumber.decrementAndGet();
					});
					RemoveTopic(topicName);
					_unAckedMessageTracker.Tell(new RemoveTopicMessages(topicName));
					_log.Info($"[{topicName}] [{Subscription}] [{ConsumerName}] Removed Topics Consumer, allTopicPartitionsNumber: {AllTopicPartitionsNumber}");
				}
				else
				{
					State.ConnectionState = HandlerState.State.Failed;
					_log.Error($"[{topicName}] [{Subscription}] [{ConsumerName}] Could not remove Topics Consumer: {t.Exception}");
				}
			});
		}


		// get topics name
		public virtual IList<string> Topics
		{
			get
			{
				return TopicsMap.Keys.ToList();
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
		public virtual IList<IActorRef> Consumers
		{
			get
			{
				return _consumers.Values.Select(x => x.Consumer).ToList();
			}
		}

		internal override void Pause()
		{
			_paused = true;
			_consumers.ForEach(x => x.Value.Consumer.Tell(Messages.Consumer.Pause.Instance));
		}

		internal override void Resume()
		{
			_paused = false;
			_consumers.ForEach(x => x.Value.Consumer.Tell(Messages.Consumer.Resume.Instance);
		}

		internal override long LastDisconnectedTimestamp
		{
			get
			{
				long lastDisconnectedTimestamp = 0;
				long? c = _consumers.Values.Max(x => x.Consumer.AskFor<long>(GetLastDisconnectedTimestamp.Instance));
				if(c != null)
				{
					lastDisconnectedTimestamp = c.Value;
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
				_outerInstance = outerInstance;
			}

			// Check partitions changes of passed in topics, and subscribe new added partitions.
			public virtual void OnTopicsExtended(ICollection<string> topicsExtended)
			{
				if(topicsExtended.Count == 0)
				{
					return;
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
		private void SubscribeIncreasedTopicPartitions(string topicName)
		{
			
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

		public IMessageId LastMessageId
		{
			get
			{
    
				var messageIdFutures = _consumers.SetOfKeyValuePairs().Select(entry => (entry.Key, entry.Value.Consumer.AskFor<IMessageId>(GetLastMessageId.Instance)).ToDictionary(Pair.getKey, Pair.getValue);
    
				CompletableFuture.allOf(messageIdFutures.SetOfKeyValuePairs().Select(DictionaryEntry.getValue).ToArray(CompletableFuture<object>[]::new)).whenComplete((ignore, ex) =>
				{
				var builder = new Dictionary<string, IMessageId>();
				messageIdFutures.forEach((key, future) =>
				{
					IMessageId messageId;
					try
					{
						messageId = future.get();
					}
					catch(Exception e)
					{
						_log.warn("[{}] Exception when topic {} getLastMessageId.", key, e);
						messageId = IMessageId.Earliest;
					}
					builder.put(key, messageId);
				});
				returnFuture.complete(new MultiMessageId(builder.build()));
				});
    
				return returnFuture;
			}
		}

		public static bool IsIllegalMultiTopicsMessageId(IMessageId messageId)
		{
			//only support earliest/latest
			return !IMessageId.Earliest.Equals(messageId) && !IMessageId.Latest.Equals(messageId);
		}

		public virtual void TryAcknowledgeMessage(IMessage<T> msg)
		{
			if(msg != null)
			{
				AcknowledgeCumulative(msg);
			}
		}
	}

}