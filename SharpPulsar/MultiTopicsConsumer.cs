using Akka.Actor;
using Akka.Util;
using Akka.Util.Internal;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Batch;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
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

    internal class MultiTopicsConsumer<T> : ConsumerActorBase<T>
	{

		internal const string DummyTopicNamePrefix = "MultiTopicsConsumer-";

		// Map <topic+partition, consumer>, when get do ACK, consumer will by find by topic name
		private readonly Dictionary<string, (string Topic , IActorRef Consumer)> _consumers;

		// Map <topic, numPartitions>, store partition number for each topic
		protected internal readonly Dictionary<string, int> TopicsMap;

		// Queue of partition consumers on which we have stopped calling receiveAsync() because the
		// shared incoming queue was full
		private Queue<(string Topic, IActorRef Consumer)> _pausedConsumers;

		// Threshold for the shared queue. When the size of the shared queue goes below the threshold, we are going to
		// resume receiving from the paused consumer partitions
		private readonly int _sharedQueueResumeThreshold;

		// sum of topicPartitions, simple topic has 1, partitioned topic equals to partition number.
		internal int AllTopicPartitionsNumber;

		private readonly IScheduler _scheduler;
		private readonly IActorRef _lookup;
		private readonly IActorRef _cnxPool;
		private readonly IActorRef _generator;
		private readonly IActorRef _stateActor;

		private bool _paused = false;
		// timeout related to auto check and subscribe partition increasement
		private ICancelable _partitionsAutoUpdateTimeout = null;

		private readonly IConsumerStatsRecorder _stats;
		private readonly IActorRef _unAckedMessageTracker;
		private readonly ConsumerConfigurationData<T> _internalConfig;

		private volatile BatchMessageId _startMessageId = null;
		private readonly long _startMessageRollbackDurationInSec;
		private readonly ClientConfigurationData _clientConfiguration;
		private readonly IActorRef _client;

		private readonly IActorRef _self;
		public MultiTopicsConsumer(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : this(stateActor, client, lookup, cnxPool, idGenerator, DummyTopicNamePrefix + Utility.ConsumerName.GenerateRandomName(), conf, listenerExecutor, schema, interceptors, createTopicIfDoesNotExist, clientConfiguration, queue)
		{
		}

		public MultiTopicsConsumer(IActorRef stateActor,IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string singleTopic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : this(stateActor, client, lookup, cnxPool, idGenerator, singleTopic, conf, listenerExecutor, schema, interceptors, createTopicIfDoesNotExist, null, 0, clientConfiguration, queue)
		{
		}

		public MultiTopicsConsumer(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string singleTopic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, IMessageId startMessageId, long startMessageRollbackDurationInSec, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : base(stateActor, client, singleTopic, conf, Math.Max(2, conf.ReceiverQueueSize), listenerExecutor, schema, interceptors, queue)
		{
			_generator = idGenerator;
			_lookup = lookup;
			_client = client;
			_cnxPool = cnxPool;
			_stateActor = stateActor;
			Condition.CheckArgument(conf.ReceiverQueueSize > 0, "Receiver queue size needs to be greater than 0 for Topics Consumer");
			_self = Self;
			_scheduler = Context.System.Scheduler;
			_clientConfiguration = clientConfiguration;
			TopicsMap = new Dictionary<string, int>();
			_consumers = new Dictionary<string, (string Topic, IActorRef Consumer)>();
			_pausedConsumers = new Queue<(string Topic, IActorRef Consumer)>();
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
				_partitionsAutoUpdateTimeout = _scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromMilliseconds(1000), TimeSpan.FromSeconds(conf.AutoUpdatePartitionsIntervalSeconds), () => SubscribeIncreasedTopicPartitions(Topic));
			}

			if(conf.TopicNames.Count == 0)
			{
				State.ConnectionState = HandlerState.State.Ready;
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
			Ready();
		}
		private void Ready()
		{
			ReceiveAsync<Subscribe>(async s =>
			{
				await Subscribe(s.TopicName, s.NumberOfPartitions);
			});
			Receive<MessageProcessed<T>>(s =>
			{
				MessageProcessed(s.Message);
			});
			ReceiveAsync<SubscribeAndCreateTopicIfDoesNotExist>(async s =>
			{
				await Subscribe(s.TopicName, s.CreateTopicIfDoesNotExist);
			});
			Receive<ReceivedMessage<T>>(r =>
			{
				IncomingMessages.TryAdd(r.Message);
			});
			Receive<AcknowledgeMessage<T>>(m => {
				try
				{
					Acknowledge(m.Message);

					Push(ConsumerQueue.AcknowledgeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<AcknowledgeMessageId>(m => {
				try
				{
					Acknowledge(m.MessageId);
					Push(ConsumerQueue.AcknowledgeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<AcknowledgeMessageIds>(m => {
				try
				{
					Acknowledge(m.MessageIds);
					Push(ConsumerQueue.AcknowledgeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<AcknowledgeMessages<T>>(m => {
				try
				{
					Acknowledge(m.Messages);
					Push(ConsumerQueue.AcknowledgeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<AcknowledgeCumulativeMessageId>(m => {
				try
				{
					AcknowledgeCumulative(m.MessageId);
					Push(ConsumerQueue.AcknowledgeCumulativeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeCumulativeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<AcknowledgeCumulativeTxn>(m => {
				try
				{
					AcknowledgeCumulative(m.MessageId, m.Txn);
					Push(ConsumerQueue.AcknowledgeCumulativeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeCumulativeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<ReconsumeLaterCumulative<T>>(m => {
				try
				{
					ReconsumeLaterCumulative(m.Message, m.DelayTime, m.TimeUnit);
					Push(ConsumerQueue.AcknowledgeCumulativeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeCumulativeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			ReceiveAsync<GetLastDisconnectedTimestamp>(async m =>
			{
				var l = await LastDisconnectedTimestamp();
				Push(ConsumerQueue.LastDisconnectedTimestamp, l);
			});
			Receive<ReconsumeLaterCumulative<T>>(m => {
				try
				{
					ReconsumeLaterCumulative(m.Message, m.DelayTime, m.TimeUnit);
					Push(ConsumerQueue.AcknowledgeCumulativeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeCumulativeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			Receive<GetConsumerName>(m => {
				Push(ConsumerQueue.ConsumerName, ConsumerName);
			});
			Receive<GetSubscription>(m => {
				Push(ConsumerQueue.Subscription, Subscription);
			});
			Receive<GetTopic>(m => {
				Push(ConsumerQueue.Topic, Topic);
			});
			Receive<ReceivedMessage<T>>(m => {
				ReceiveMessageFromConsumer(Sender, m.Message);
			});
			Receive<ClearUnAckedChunckedMessageIdSequenceMap>(_ => {
				UnAckedChunckedMessageIdSequenceMap.Clear();
			});
			ReceiveAsync<HasReachedEndOfTopic>(async _ => {
				var hasReached = await HasReachedEndOfTopic();
				Push(ConsumerQueue.HasReachedEndOfTopic, hasReached);
			});
			ReceiveAsync<GetAvailablePermits>(async _ => {
				var permits = await AvailablePermits();
				Sender.Tell(permits);
			});
			ReceiveAsync<IsConnected>(async _ => {
				var connected = await Connected();
				Push(ConsumerQueue.Connected, connected);
			});
			Receive<Pause>(_ => {
				Pause();
			});
			Receive<RemoveTopicConsumer>(t => {
				RemoveConsumer(t.Topic);
			});
			ReceiveAsync<HasMessageAvailable>(async _ => {
				var has = await HasMessageAvailable();
				Sender.Tell(has);
			});
			ReceiveAsync<GetNumMessagesInQueue>(async _ => {
				var num = await NumMessagesInQueue();
				Sender.Tell(num);
			});
			Receive<Resume>(_ => {
				Resume();
			});
			ReceiveAsync<GetLastMessageId>(async m =>
			{
				try
				{
					var lmsid = await LastMessageId();
					Push(ConsumerQueue.LastMessageId, lmsid);
				}
				catch (Exception ex)
				{
					var nul = new NullMessageId(ex);
					Push(ConsumerQueue.LastMessageId, nul);
				}
			});
			Receive<AcknowledgeWithTxnMessages>(m =>
			{
				try
				{
					DoAcknowledgeWithTxn(m.MessageIds, m.AckType, m.Properties, m.Txn);
					Push(ConsumerQueue.AcknowledgeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<GetStats>(m =>
			{
				try
				{
					var stats = Stats;
					Push(ConsumerQueue.Stats, stats);
				}
				catch (Exception ex)
				{
					_log.Error(ex.ToString());
					Push(ConsumerQueue.Stats, null);
				}
			});
			ReceiveAsync<GetHandlerState>(async m =>
			{
				await HandlerStates();
			});
			Receive<NegativeAcknowledgeMessage<T>>(m =>
			{
				try
				{
					NegativeAcknowledge(m.Message);
					Push(ConsumerQueue.NegativeAcknowledgeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.NegativeAcknowledgeException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<NegativeAcknowledgeMessages<T>>(m =>
			{
				try
				{
					NegativeAcknowledge(m.Messages);
					Push(ConsumerQueue.NegativeAcknowledgeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.NegativeAcknowledgeException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<NegativeAcknowledgeMessageId>(m =>
			{
				try
				{
					NegativeAcknowledge(m.MessageId);
					Push(ConsumerQueue.NegativeAcknowledgeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.NegativeAcknowledgeException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<ReconsumeLaterMessages<T>>(m =>
			{
				try
				{
					ReconsumeLater(m.Messages, m.DelayTime, m.TimeUnit);
					Push(ConsumerQueue.ReconsumeLaterException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.ReconsumeLaterException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<ReconsumeLaterWithProperties<T>>(m =>
			{
				try
				{
					DoReconsumeLater(m.Message, m.AckType, m.Properties.ToDictionary(x => x.Key, x => x.Value), m.DelayTime, m.TimeUnit);
					Push(ConsumerQueue.ReconsumeLaterException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.ReconsumeLaterException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<ReconsumeLaterMessage<T>>(m =>
			{
				try
				{
					ReconsumeLater(m.Message, m.DelayTime, m.TimeUnit);
					Push(ConsumerQueue.ReconsumeLaterException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.ReconsumeLaterException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<RedeliverUnacknowledgedMessages>(m =>
			{
				try
				{
					RedeliverUnacknowledgedMessages();
					Push(ConsumerQueue.RedeliverUnacknowledgedException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.RedeliverUnacknowledgedException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<RedeliverUnacknowledgedMessageIds>(m =>
			{
				try
				{
					RedeliverUnacknowledgedMessages(m.MessageIds);
					Push(ConsumerQueue.RedeliverUnacknowledgedException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.RedeliverUnacknowledgedException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<UnsubscribeTopic>(u =>
			{
				try
				{
					Unsubscribe(u.Topic);
					Push(ConsumerQueue.UnsubscribeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.UnsubscribeException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<SeekMessageId>(m =>
			{
				try
				{
					Seek(m.MessageId);
					Push(ConsumerQueue.SeekException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.SeekException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<SeekTimestamp>(m =>
			{
				try
				{
					Seek(m.Timestamp);
					Push(ConsumerQueue.SeekException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.SeekException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
		}
		// subscribe one more given topic
		private async ValueTask Subscribe(string topicName, bool createTopicIfDoesNotExist)
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

			var result = await _client.AskFor(new GetPartitionedTopicMetadata(TopicName.Get(topicName)));
			if (result is PartitionedTopicMetadata metadata)
			{
				await SubscribeTopicPartitions(fullTopicName, metadata.Partitions, createTopicIfDoesNotExist);
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
				});
			}
		}
		private void ReceiveMessageFromConsumer(IActorRef consumer, IMessage<T> message)
		{
			var c = _consumers.Where(x => x.Value.Consumer == consumer).FirstOrDefault();
			var topic = c.Value.Topic;
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}] [{Subscription}] Receive message from sub consumer:{topic}");
			}
			MessageReceived(topic, consumer, message);
			int size = IncomingMessages.Count;
			if (size >= MaxReceiverQueueSize || (size > _sharedQueueResumeThreshold && _pausedConsumers.Count > 0))
			{
				_pausedConsumers.Enqueue((topic, consumer));
				consumer.Tell(Messages.Consumer.Pause.Instance);
			}
		}

		private void MessageReceived(string topic, IActorRef consumer, IMessage<T> message)
		{
			Condition.CheckArgument(message is Message<T>);
			var topicMessage = new TopicMessage<T>(topic, TopicName.Get(topic).PartitionedTopicName, message);

			if(_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}][{Subscription}] Received message from topics-consumer {message.MessageId}");
			}
			_unAckedMessageTracker.Tell(new Add(topicMessage.MessageId));
			if (Listener != null)
			{
				// Trigger the notification on the message listener in a separate thread to avoid blocking the networking
				// thread while the message processing happens
				Task.Run(() =>
				{
					var listener = Listener;
					var log = _log;
					try
					{
						if (log.IsDebugEnabled)
						{
							log.Debug($"[{Topic}][{Subscription}] Calling message listener for message {message.MessageId}");
						}
						listener.Received(consumer, message);
					}
					catch (Exception t)
					{
						log.Error($"[{Topic}][{Subscription}] Message listener error in processing message: {message}: {t}");
					}
				});
			}
            else
            {
				IncomingMessages.TryAdd(topicMessage);
            }
		}

		private void MessageProcessed(IMessage<T> msg)
		{
			_unAckedMessageTracker.Tell(new Add(msg.MessageId));
			IncomingMessagesSize -= msg.Data.Length;
			ResumeReceivingFromPausedConsumersIfNeeded();
		}

		private void ResumeReceivingFromPausedConsumersIfNeeded()
		{
			if(IncomingMessages.Count <= _sharedQueueResumeThreshold && _pausedConsumers.Count > 0)
			{
				while(true)
				{
					var (_, consumer) = _pausedConsumers.FirstOrDefault();
					if(consumer == null)
					{
						break;
					}
					consumer.Tell(Messages.Consumer.Resume.Instance);
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
				consumer.Ask(new AcknowledgeWithTxnMessages(new List<IMessageId> { innerId }, properties, txnImpl)).ContinueWith
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
					consumer.Consumer.Tell(new AcknowledgeWithTxnMessages(t.Value, properties, txn));
					messageIdList.ForEach(x => _unAckedMessageTracker.Tell(new Remove(x)));
				});
			}
		}

		protected internal override void DoReconsumeLater(IMessage<T> message, AckType ackType, IDictionary<string, long> properties, long delayTime, TimeUnit unit)
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
				var(_, consumer) = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				if(consumer != null)
				{
					var innerId = topicMessageId.InnerMessageId;
					consumer.Tell(new ReconsumeLaterCumulative<T>(message, delayTime, unit));
				}
				else
				{
					throw new PulsarClientException.NotConnectedException();
				}
			}
			else
			{
				var (_, consumer) = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				var innerId = topicMessageId.InnerMessageId;
				consumer.Tell(new ReconsumeLaterWithProperties<T>(message, ackType, properties, delayTime, unit));
				_unAckedMessageTracker.Tell(new Remove(topicMessageId));
			}
		}

		internal override void NegativeAcknowledge(IMessageId messageId)
		{
			Condition.CheckArgument(messageId is TopicMessageId);
			var topicMessageId = (TopicMessageId) messageId;

			var (_, consumer) = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
			consumer.Tell(new NegativeAcknowledgeMessageId(topicMessageId.InnerMessageId));
		}
        protected override void PostStop()
        {
			Close();
            base.PostStop();
        }
        internal void Close()
		{
			if(State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				_unAckedMessageTracker.GracefulStop(TimeSpan.FromMilliseconds(100));
			}
			State.ConnectionState = HandlerState.State.Closing;

			if(_partitionsAutoUpdateTimeout != null)
			{
				_partitionsAutoUpdateTimeout.Cancel();
				_partitionsAutoUpdateTimeout = null;
			}
			_consumers.Values.ForEach(c => c.Consumer.GracefulStop(TimeSpan.FromMilliseconds(100)));
			State.ConnectionState = HandlerState.State.Closed;
			_unAckedMessageTracker.GracefulStop(TimeSpan.FromMilliseconds(100));
			_log.Info($"[{Topic}] [{Subscription}] Closed Topics Consumer");
			_client.Tell(new CleanupConsumer(Self));

		}


		internal override async ValueTask<bool> Connected()
		{
			foreach (var c in _consumers.Values)
			{
				var s = await c.Consumer.AskFor<bool>(IsConnected.Instance);
				if (!s)
					return false;
			}
			return true;
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
			IncomingMessages.Empty();
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
				IncomingMessages.Empty();
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


		internal override async ValueTask<int> AvailablePermits()
		{
			var sum = 0;
			foreach (var c in _consumers.Values)
			{
				var s = await c.Consumer.AskFor<int>(GetAvailablePermits.Instance);
				sum += s;
			}
			return sum;
		}

		private async ValueTask<bool> HasReachedEndOfTopic()
		{
			foreach (var c in _consumers.Values)
			{
				var s = await c.Consumer.AskFor<bool>(Messages.Consumer.HasReachedEndOfTopic.Instance);
				if (!s)
					return false;
			}
			return true;
		}
		private async ValueTask HandlerStates()
		{
			var st = await _stateActor.AskFor<HandlerStateResponse>(GetHandlerState.Instance);
			Sender.Tell(st);
		}
		private async ValueTask<bool> HasMessageAvailable()
		{
			foreach (var c in _consumers.Values)
			{
				var s = await c.Consumer.AskFor<bool>(Messages.Consumer.HasMessageAvailable.Instance);
				if (!s)
					return false;
			}
			return true;
		}

		internal override async ValueTask<int> NumMessagesInQueue()
		{
			var sum = 0;
			foreach(var c in _consumers.Values)
            {
				var s = await c.Consumer.AskFor<int>(GetNumMessagesInQueue.Instance);
				sum += s;
			}
			return IncomingMessages.Count + sum;
		}

		internal override IConsumerStatsRecorder Stats
		{
			get
			{
				if (_stats == null)
				{
					return null;
				}
				_stats.Reset();

				_consumers.Values.ForEach(async consumer => _stats.UpdateCumulativeStats(await consumer.Consumer.AskFor<IConsumerStats>(GetStats.Instance)));
				return _stats;
			}
		}

		internal virtual IActorRef UnAckedMessageTracker
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
				if (!(message is TopicMessage<T>))
					throw new InvalidMessageException(message.GetType().FullName);

				while(IncomingMessages.Count > 0)
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

		internal async ValueTask Subscribe(string topicName, int numberPartitions)
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
			
			await SubscribeTopicPartitions(fullTopicName, numberPartitions, true);
		}

		private async ValueTask SubscribeTopicPartitions(string topicName, int numPartitions, bool createIfDoesNotExist)
		{
			var t = await _client.AskFor(new PreProcessSchemaBeforeSubscribe<T>(Schema, topicName));
			if (t is PreProcessedSchema<T> schema)
				await DoSubscribeTopicPartitions(schema.Schema, topicName, numPartitions, createIfDoesNotExist);
			else
				_log.Debug($"[PreProcessSchemaBeforeSubscribe] Received:{t.GetType().FullName}");
		}

		private async ValueTask DoSubscribeTopicPartitions(ISchema<T> schema, string topicName, int numPartitions, bool createIfDoesNotExist)
		{
			if (_log.IsDebugEnabled)
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
				Enumerable.Range(0, numPartitions).ForEach( async partitionIndex=> 
				{
					var consumerId = await _generator.AskFor<long>(NewConsumerId.Instance);
					string partitionName = TopicName.Get(topicName).GetPartition(partitionIndex).ToString();
					var newConsumer = CreateConsumer(consumerId, partitionName, configurationData, partitionIndex, schema, Interceptors, createIfDoesNotExist);					
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
				var consumerId = await _generator.AskFor<long>(NewConsumerId.Instance);

				var newConsumer = CreateConsumer(consumerId, topicName, _internalConfig, -1, schema, Interceptors, createIfDoesNotExist);
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

		private IActorRef CreateConsumer(long consumerId, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createIfDoesNotExist)
        {
			if (conf.ReceiverQueueSize == 0)
			{
				return Context.ActorOf(Props.Create(() => new ZeroQueueConsumer<T>(consumerId, _stateActor, _client, _lookup, _cnxPool, _generator, topic, conf, Context.System.Scheduler.Advanced, partitionIndex, false, _startMessageId, schema, null, createIfDoesNotExist, _clientConfiguration, ConsumerQueue)));
			}
			return Context.ActorOf(Props.Create(() => new ConsumerActor<T>(consumerId, _stateActor, _client, _lookup, _cnxPool, _generator, topic, conf, Context.System.Scheduler.Advanced, partitionIndex, false, _startMessageId, _startMessageRollbackDurationInSec, schema, null, createIfDoesNotExist, _clientConfiguration, ConsumerQueue)));

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
		internal virtual void Unsubscribe(string topicName)
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

			var tasks = consumersToUnsub.Select(async c => await c.Consumer.Ask(Messages.Consumer.Unsubscribe.Instance)).ToList();
			Task.WhenAll(tasks).ContinueWith(t => {
                if (!t.IsFaulted)
                {
					var toUnsub = consumersToUnsub;
					toUnsub.ForEach(consumer1 =>
					{
						_consumers.Remove(consumer1.Topic);
						_pausedConsumers = new Queue<(string Topic, IActorRef Consumer)>(_pausedConsumers.Where(x => x != consumer1));
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
		internal virtual void RemoveConsumer(string topicName)
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

			var tasks = consumersToClose.Select(async x => await x.Consumer.GracefulStop(TimeSpan.FromSeconds(1))).ToList();
			Task.WhenAll(tasks).ContinueWith(t => 
			{
                if (!t.IsFaulted)
                {
					consumersToClose.ForEach(consumer1 =>
					{
						_consumers.Remove(consumer1.Topic);
						_pausedConsumers = new Queue<(string Topic, IActorRef Consumer)>(_pausedConsumers.Where(x => x != consumer1));

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
		internal virtual IList<string> Topics
		{
			get
			{
				return TopicsMap.Keys.ToList();
			}
		}

		// get partitioned topics name
		internal virtual IList<string> PartitionedTopics
		{
			get
			{
				return _consumers.Keys.ToList();
			}
		}

		// get partitioned consumers
		internal virtual IList<IActorRef> Consumers
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
			_consumers.ForEach(x => x.Value.Consumer.Tell(Messages.Consumer.Resume.Instance));
		}

		internal override async ValueTask<long> LastDisconnectedTimestamp()
		{
			long lastDisconnectedTimestamp = 0;
			long c = await _consumers.Values.Max(async x => await x.Consumer.AskFor<long>(GetLastDisconnectedTimestamp.Instance));
			lastDisconnectedTimestamp = c;
			return lastDisconnectedTimestamp;
		}

		// subscribe increased partitions for a given topic
		private async ValueTask SubscribeIncreasedTopicPartitions(string topicName)
		{			
			var topics = await _client.Ask<PartitionsForTopic>(new GetPartitionsForTopic(topicName));
			var list = topics.Topics.ToList();
			int oldPartitionNumber = TopicsMap.GetValueOrNull(topicName);
			int currentPartitionNumber = list.Count;
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"[{topicName}] partitions number. old: {oldPartitionNumber}, new: {currentPartitionNumber}");
			}
			if (oldPartitionNumber == currentPartitionNumber)
			{
				return;
			}
			else if (oldPartitionNumber < currentPartitionNumber)
			{
				AllTopicPartitionsNumber = currentPartitionNumber;
				IList<string> newPartitions = list.GetRange(oldPartitionNumber, currentPartitionNumber);
				foreach (var partitionName in newPartitions)
				{
					var consumerId = await _generator.AskFor<long>(NewConsumerId.Instance);
					int partitionIndex = TopicName.GetPartitionIndex(partitionName);
					ConsumerConfigurationData<T> configurationData = InternalConsumerConfig;
					var newConsumer = Context.ActorOf(Props.Create(() => new ConsumerActor<T>(consumerId, _stateActor, _client, _lookup, _cnxPool, _generator, partitionName, configurationData, Context.System.Scheduler.Advanced, partitionIndex, true, null, Schema, Interceptors, true, _clientConfiguration, ConsumerQueue)));
					if (_paused)
					{
						newConsumer.Tell(Messages.Consumer.Pause.Instance);
					}
					_consumers.Add(Topic, (partitionName, newConsumer));

					if (_log.IsDebugEnabled)
					{
						_log.Debug($"[{topicName}] create consumer {Topic} for partitionName: {partitionName}");
					}
				}
				var newConsumerList = newPartitions.Select(partitionTopic => _consumers.GetValueOrNull(partitionTopic)).ToList();
				StartReceivingMessages(newConsumerList);
				//_log.warn("[{}] Failed to subscribe {} partition: {} - {} : {}", Topic, topicName, oldPartitionNumber, currentPartitionNumber, ex);
			}
			else
			{
				_log.Error($"[{topicName}] not support shrink topic partitions. old: {oldPartitionNumber}, new: {currentPartitionNumber}");
				//future.completeExceptionally(new PulsarClientException.NotSupportedException("not support shrink topic partitions"));
			}
		}
		private void Push<T1>(BlockingCollection<T1> queue, T1 obj)
		{
				queue.Add(obj);
		}
		private async ValueTask<IMessageId> LastMessageId()
		{
			var multiMessageId = new Dictionary<string, IMessageId>();
			foreach (var v in _consumers.Values)
			{
				IMessageId messageId;
				try
				{
					messageId = await v.Consumer.AskFor<IMessageId>(GetLastMessageId.Instance);
				}
				catch (Exception e)
				{
					_log.Warning($"[{v.Topic}] Exception when topic {e} getLastMessageId.");
					messageId = IMessageId.Earliest;
				}

				multiMessageId.Add(v.Topic, messageId);
			}
			return new MultiMessageId(multiMessageId);
		}

		internal static bool IsIllegalMultiTopicsMessageId(IMessageId messageId)
		{
			//only support earliest/latest
			return !IMessageId.Earliest.Equals(messageId) && !IMessageId.Latest.Equals(messageId);
		}

		internal virtual void TryAcknowledgeMessage(IMessage<T> msg)
		{
			if(msg != null)
			{
				AcknowledgeCumulative(msg);
			}
		}
	}

}