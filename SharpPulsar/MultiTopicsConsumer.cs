using Akka.Actor;
using Akka.Util;
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
using SharpPulsar.Messages;
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
		private readonly List<(string Topic, IActorRef Consumer)> _pausedConsumers;

		// Threshold for the shared queue. When the size of the shared queue goes below the threshold, we are going to
		// resume receiving from the paused consumer partitions
		private readonly int _sharedQueueResumeThreshold;

		// sum of topicPartitions, simple topic has 1, partitioned topic equals to partition number.
		internal int AllTopicPartitionsNumber;

		private readonly IScheduler _scheduler;

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

		private readonly IActorRef _self;

		public MultiTopicsConsumer(IActorRef client, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : this(client, DummyTopicNamePrefix + Utility.ConsumerName.GenerateRandomName(), conf, listenerExecutor, schema, interceptors, createTopicIfDoesNotExist, clientConfiguration, queue, 0)
		{
		}

		public MultiTopicsConsumer(IActorRef client, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, IMessageId startMessageId, long startMessageRollbackDurationInSec, int numberPartitions, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : this(client, DummyTopicNamePrefix + Utility.ConsumerName.GenerateRandomName(), conf, listenerExecutor, schema, interceptors, createTopicIfDoesNotExist, startMessageId, startMessageRollbackDurationInSec, numberPartitions, clientConfiguration, queue)
		{
		}

		public MultiTopicsConsumer(IActorRef client, string singleTopic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue, int numPartitions) : this(client, singleTopic, conf, listenerExecutor, schema, interceptors, createTopicIfDoesNotExist, null, 0, numPartitions, clientConfiguration, queue)
		{
		}

		public MultiTopicsConsumer(IActorRef client, string singleTopic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, IMessageId startMessageId, long startMessageRollbackDurationInSec, int numberPartition, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : base(client, singleTopic, conf, Math.Max(2, conf.ReceiverQueueSize), listenerExecutor, schema, interceptors, queue)
		{
			Condition.CheckArgument(conf.ReceiverQueueSize > 0, "Receiver queue size needs to be greater than 0 for Topics Consumer");
			_self = Self;
			_scheduler = Context.System.Scheduler;
			_clientConfiguration = clientConfiguration;
			TopicsMap = new Dictionary<string, int>();
			_consumers = new Dictionary<string, (string Topic, IActorRef Consumer)>();
			_pausedConsumers = new List<(string Topic, IActorRef Consumer)>();
			_sharedQueueResumeThreshold = MaxReceiverQueueSize / 2;
			AllTopicPartitionsNumber = 0;
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
				State.ConnectionState = HandlerState.State.Ready;
				Self.Tell(new Subscribe(singleTopic, numberPartition));
			}
            else
            {

				Condition.CheckArgument(conf.TopicNames.Count == 0 || TopicNamesValid(conf.TopicNames), "Topics is empty or invalid.");

				var tasks = conf.TopicNames.Select(t => Task.Run(()=> Subscribe(t, createTopicIfDoesNotExist))).ToArray();
				Task.WhenAll(tasks).ContinueWith(t => 
				{
                    if (!t.IsFaulted)
                    {
						if (AllTopicPartitionsNumber > MaxReceiverQueueSize)
						{
							MaxReceiverQueueSize = AllTopicPartitionsNumber;
						}
						State.ConnectionState = HandlerState.State.Ready;
						StartReceivingMessages(_consumers.Values.ToList());
						_log.Info("[{}] [{}] Created topics consumer with {} sub-consumers", Topic, Subscription, AllTopicPartitionsNumber);						
					}
                    else
                    {
						_log.Warning($"[{Topic}] Failed to subscribe topics: {t.Exception}");
					}
				});
			}			
			Receive<Subscribe>(s => 
			{
				SubscribeAsync(s.TopicName, s.NumberOfPartitions);
			});		
			Receive<ReceiveMessageFrom>(r => 
			{
				ReceiveMessageFromConsumer(r.Consumer);
			});
		}
		// subscribe one more given topic
		private void Subscribe(string topicName, bool createTopicIfDoesNotExist)
		{
			TopicName topicNameInstance = GetTopicName(topicName);
			if (topicNameInstance == null)
			{
				throw new PulsarClientException.AlreadyClosedException("Topic name not valid");
			}
			string fullTopicName = topicNameInstance.ToString();
			if (TopicsMap.ContainsKey(fullTopicName) || TopicsMap.ContainsKey(topicNameInstance.PartitionedTopicName))
			{
				throw new PulsarClientException.AlreadyClosedException("Already subscribed to " + topicName);
			}

			if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				throw new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed");
			}

			var result = _client.AskFor(new GetPartitionedTopicMetadata(TopicName.Get(topicName)));
			if (result is PartitionedTopicMetadata metadata)
			{
				SubscribeTopicPartitions(fullTopicName, metadata.Partitions, createTopicIfDoesNotExist);
			}
			else if (result is Failure failure)
            {
				_log.Warning($"[{fullTopicName}] Failed to get partitioned topic metadata: {failure.Exception}");
			}
		}
		// Check topics are valid.
		// - each topic is valid,
		// - topic names are unique.
		private bool TopicNamesValid(ICollection<string> topics)
		{
			if(topics != null && topics.Count >= 1)
				throw new ArgumentException("topics should contain more than 1 topic");

			Option<string> result = topics.Where(Topic => !TopicName.IsValid(Topic)).First();

			if(result.HasValue)
			{
				_log.Warning($"Received invalid topic name: {result}");
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
				_log.Warning($"Topic names not unique. unique/all : {set.Count}/{topics.Count}");
				return false;
			}
		}

		private void StartReceivingMessages(IList<(string topic, IActorRef consumer)> newConsumers)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}] startReceivingMessages for {newConsumers.Count} new consumers in topics consumer, state: {State.ConnectionState}");
			}
			if(State.ConnectionState == HandlerState.State.Ready)
			{
				newConsumers.ForEach(consumer =>
				{
					consumer.consumer.Tell(new IncreaseAvailablePermits(Conf.ReceiverQueueSize));
					ReceiveMessageFromConsumer(consumer);
				});
			}
		}
		private void ReceiveMessageFromConsumer((string topic, IActorRef consumer) c)
		{
			c.consumer.Ask(Messages.Consumer.Receive.Instance).ContinueWith(task => 
			{
				var self = _self;
				var log = _log;
				if(!task.IsFaulted)
                {
					var obj = task.Result;
					if(obj is NullMessage<T> message)
                    {
						log.Error(message.Exception.ToString());
                    }
                    else
                    {
						var m = (Message<T>)obj;
						if (_log.IsDebugEnabled)
						{
							log.Debug($"[{Topic}] [{Subscription}] Receive message from sub consumer:{c.topic}");
						}
						MessageReceived(c, m);
						int size = IncomingMessages.Count;
						if (size >= MaxReceiverQueueSize || (size > _sharedQueueResumeThreshold && _pausedConsumers.Count > 0))
						{
							_pausedConsumers.Add(c);
						}
						else
						{							
							_scheduler.ScheduleTellOnce(TimeSpan.FromMilliseconds(100), self, new ReceiveMessageFrom(c), ActorRefs.Nobody);
						}
					}
                }
			});
		}

		private void MessageReceived((string topic, IActorRef consumer) c, IMessage<T> message)
		{
			Condition.CheckArgument(message is Message<T>);
			var topicMessage = new TopicMessage<T>(c.topic, TopicName.Get(c.topic).PartitionedTopicName, message);

			if(_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}][{Subscription}] Received message from topics-consumer {message.MessageId}");
			}
			_unAckedMessageTracker.Tell(new Add(topicMessage.MessageId));
			ConsumerQueue.Receive.Add(topicMessage);
			if (Listener != null)
			{
				// Trigger the notification on the message listener in a separate thread to avoid blocking the networking
				// thread while the message processing happens
				Task.Run(() =>
				{
					var log = _log;
					IMessage<T> msg;
					try
					{
						msg = InternalReceive(0, TimeUnit.MILLISECONDS);
						if (msg == null)
						{
							if (log.IsDebugEnabled)
							{
								log.Debug($"[{Topic}] [{Subscription}] Message has been cleared from the queue");
							}
							return;
						}
					}
					catch (PulsarClientException e)
					{
						log.Warning($"[{Topic}] [{Subscription}] Failed to dequeue the message for listener");
						return;
					}
					try
					{
						if (log.IsDebugEnabled)
						{
							log.Debug($"[{Topic}][{Subscription}] Calling message listener for message {message.MessageId}");
						}
						Listener.Received(c.consumer, msg);
					}
					catch (Exception t)
					{
						log.Error($"[{Topic}][{Subscription}] Message listener error in processing message: {message}: {t}");
					}
				});
			}
		}

		protected internal override void MessageProcessed<T1>(IMessage<T1> msg)
		{
			_unAckedMessageTracker.Tell(new Add(msg.MessageId));
			IncomingMessagesSize -= msg.Data.Length;
		}

		private void ResumeReceivingFromPausedConsumersIfNeeded()
		{
			if(IncomingMessages.Count <= _sharedQueueResumeThreshold && _pausedConsumers.Count > 0)
			{
				while(true)
				{
					var (topic, consumer) = _pausedConsumers.FirstOrDefault();
					if(consumer == null)
					{
						break;
					}

					Task.Run(() => {
						ReceiveMessageFromConsumer((topic, consumer));
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

		// create consumer for a single topic with already known partitions.
		// first create a consumer with no topic, then do subscription for already know partitionedTopic.
		public static Props CreatePartitionedConsumer(IActorRef client, ConsumerConfigurationData<T> conf,IAdvancedScheduler listenerExecutor, int numPartitions, ISchema<T> schema, ConsumerInterceptors<T> interceptors, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue)
		{
			Condition.CheckArgument(conf.TopicNames.Count == 1, "Should have only 1 topic for partitioned consumer");

			// get topic name, then remove it from conf, so constructor will create a consumer with no topic.
			var cloneConf = conf;
			string topicName = cloneConf.SingleTopic;
			cloneConf.TopicNames.Remove(topicName);

			return Props.Create(()=> new MultiTopicsConsumer<T>(client, topicName, conf, listenerExecutor, schema, interceptors, true, clientConfiguration, queue, numPartitions));
			
		}

		internal virtual void Subscribe(string topicName, int numberPartitions)
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
			
			SubscribeTopicPartitions(fullTopicName, numberPartitions, true);
		}

		private void SubscribeTopicPartitions(string topicName, int numPartitions, bool createIfDoesNotExist)
		{
			_client.Ask(new PreProcessSchemaBeforeSubscribe<T>(Schema, topicName)).ContinueWith(t =>
			{

				if (!t.IsFaulted)
				{
					if(t.Result is PreProcessedSchema<T> schema)
						DoSubscribeTopicPartitions(schema.Schema, topicName, numPartitions, createIfDoesNotExist);
					else
						_log.Debug($"[PreProcessSchemaBeforeSubscribe] Received:{t.Result.GetType().FullName}");
				}
				else
				{
					_log.Error(t.Exception.ToString());
				}
			});
		}

		private void DoSubscribeTopicPartitions(ISchema<T> schema, string topicName, int numPartitions, bool createIfDoesNotExist)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug($"Subscribe to topic {topicName} metadata.partitions: {numPartitions}");
			}

			if(numPartitions > 0)
			{
				// Below condition is true if subscribeAsync() has been invoked second time with same
				// topicName before the first invocation had reached this point.
				if(TopicsMap.TryGetValue(topicName, out var parts))
				{
					string errorMessage = $"[{Topic}] Failed to subscribe for topic [{topicName}] in topics consumer. Topic is already being subscribed for in other thread.";
					_log.Warning(errorMessage);
					throw new PulsarClientException(errorMessage);
				}
				TopicsMap.Add(topicName, numPartitions);
				AllTopicPartitionsNumber += numPartitions;

				int receiverQueueSize = Math.Min(Conf.ReceiverQueueSize, Conf.MaxTotalReceiverQueueSizeAcrossPartitions / numPartitions);
				ConsumerConfigurationData<T> configurationData = InternalConsumerConfig;
				configurationData.ReceiverQueueSize = receiverQueueSize;
				Enumerable.Range(0, numPartitions).ForEach(partitionIndex=> {

					string partitionName = TopicName.Get(topicName).GetPartition(partitionIndex).ToString();
					var newConsumer = Context.ActorOf(ConsumerActor<T>.NewConsumer(_client, partitionName, configurationData, Context.System.Scheduler.Advanced, partitionIndex, true, _startMessageId, schema, Interceptors, createIfDoesNotExist, _startMessageRollbackDurationInSec, _clientConfiguration, ConsumerQueue));
					_consumers.Add(Topic, (partitionName, newConsumer));
				});
			}
			else
			{
				if (TopicsMap.TryGetValue(topicName, out var parts))
				{
					string errorMessage = $"[{Topic}] Failed to subscribe for topic [{topicName}] in topics consumer. Topic is already being subscribed for in other thread.";
					_log.Warning(errorMessage);
					throw new PulsarClientException(errorMessage);
				}
				TopicsMap.Add(topicName, 1);
				++AllTopicPartitionsNumber;

				var newConsumer = Context.ActorOf(ConsumerActor<T>.NewConsumer(_client, topicName, _internalConfig, Context.System.Scheduler.Advanced, -1, true, null, schema, Interceptors, createIfDoesNotExist, _clientConfiguration, ConsumerQueue));
				_consumers.Add(Topic, (topicName, newConsumer));
			}

			if (AllTopicPartitionsNumber > MaxReceiverQueueSize)
			{
				MaxReceiverQueueSize = AllTopicPartitionsNumber;
			}
			int numTopics = TopicsMap.Values.Sum();
			int currentAllTopicsPartitionsNumber = AllTopicPartitionsNumber;
			if(currentAllTopicsPartitionsNumber == numTopics)
				throw new ArgumentException("allTopicPartitionsNumber " + currentAllTopicsPartitionsNumber + " not equals expected: " + numTopics);
			
			StartReceivingMessages(_consumers.Values.Where(consumer1 =>
			{
				string consumerTopicName = consumer1.Topic;
				return TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(TopicName.Get(topicName).PartitionedTopicName);
			}).ToList());
			_log.Info($"[{Topic}] [{Subscription}] Success subscribe new topic {topicName} in topics consumer, partitions: {numPartitions}, allTopicPartitionsNumber: {AllTopicPartitionsNumber}");
			
			//HandleSubscribeOneTopicError(topicName, ex, subscribeResult);
		}

		// handling failure during subscribe new topic, unsubscribe success created partitions
		private void HandleSubscribeOneTopicError(string topicName, Exception error)
		{
			_log.Warning($"[{Topic}] Failed to subscribe for topic [{topicName}] in topics consumer {error}");
			Task.Run(() => 
			{

				int toCloseNum = 0;
				_consumers.Values.Where(consumer1 =>
				{
					string consumerTopicName = consumer1.Topic;
					if (TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(TopicName.Get(topicName).PartitionedTopicName))
					{
						++toCloseNum;
						return true;
					}
					else
					{
						return false;
					}
				}).ToList().ForEach(consumer2 =>
				{
					consumer2.Consumer.GracefulStop(TimeSpan.FromMilliseconds(100)).ContinueWith(c=> 
					{
						--AllTopicPartitionsNumber;
						_consumers.Remove(consumer2.Topic);
						if (--toCloseNum == 0)
						{
							_log.Warning($"[{Topic}] Failed to subscribe for topic [{topicName}] in topics consumer, subscribe error: {error}");
							RemoveTopic(topicName);
						}

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
						_pausedConsumers.Remove(consumer1);
						--AllTopicPartitionsNumber;
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
						_pausedConsumers.Remove(consumer1);
						--AllTopicPartitionsNumber;
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
		internal sealed class ReceiveMessageFrom
        {
			internal (string topic, IActorRef consumer) Consumer { get; }
            public ReceiveMessageFrom((string topic, IActorRef consumer) c)
            {
				Consumer = c;
            }
        }
	}

}