using Akka.Actor;
using Akka.Util;
using Akka.Util.Internal;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Batch;
using SharpPulsar.Cache;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Queues;
using SharpPulsar.Schema;
using SharpPulsar.Schemas.Generic;
using SharpPulsar.Stats.Consumer;
using SharpPulsar.Stats.Consumer.Api;
using SharpPulsar.Tracker;
using SharpPulsar.Tracker.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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

    internal class MultiTopicsConsumer<T> : ConsumerActorBase<T>, IWithUnboundedStash
	{

		internal const string DummyTopicNamePrefix = "MultiTopicsConsumer-";

		// Map <topic+partition, consumer>, when get do ACK, consumer will by find by topic name
		private readonly Dictionary<string, IActorRef> _consumers;

		// Map <topic, numPartitions>, store partition number for each topic
		protected internal readonly Dictionary<string, int> TopicsMap;

		// Queue of partition consumers on which we have stopped calling receiveAsync() because the
		// shared incoming queue was full
		private Queue<IActorRef> _pausedConsumers;

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
		private readonly Cache<string, ISchemaInfoProvider> _schemaProviderLoadingCache = new Cache<string, ISchemaInfoProvider>(TimeSpan.FromMinutes(30), 100000);

		private readonly IActorRef _client;

		private readonly IActorRef _self;
		private readonly IActorContext _context;
		public MultiTopicsConsumer(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : this(stateActor, client, lookup, cnxPool, idGenerator, DummyTopicNamePrefix + Utility.ConsumerName.GenerateRandomName(), conf, listenerExecutor, schema, interceptors, createTopicIfDoesNotExist, clientConfiguration, queue)
		{
		}

		public MultiTopicsConsumer(IActorRef stateActor,IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string singleTopic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : this(stateActor, client, lookup, cnxPool, idGenerator, singleTopic, conf, listenerExecutor, schema, interceptors, createTopicIfDoesNotExist, null, 0, clientConfiguration, queue)
		{
		}

		public MultiTopicsConsumer(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string singleTopic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createTopicIfDoesNotExist, IMessageId startMessageId, long startMessageRollbackDurationInSec, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) : base(stateActor, client, singleTopic, conf, Math.Max(2, conf.ReceiverQueueSize), listenerExecutor, schema, interceptors, queue)
		{
			_context = Context;
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
			_consumers = new Dictionary<string, IActorRef>();
			_pausedConsumers = new Queue<IActorRef>();
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
						

			if(conf.TopicNames.Count == 0)
			{
				State.ConnectionState = HandlerState.State.Ready;
				Ready();
			}
            else
            {
				Self.Tell(new SubscribeAndCreateTopicsIfDoesNotExist(conf.TopicNames.ToList(), createTopicIfDoesNotExist));
			}

			ReceiveAsync<SubscribeAndCreateTopicsIfDoesNotExist>(async s =>
			{
				foreach(var topic in s.Topics )
					await Subscribe(topic, s.CreateTopicIfDoesNotExist);

				if (AllTopicPartitionsNumber == TopicsMap.Values.Sum())
				{
					MaxReceiverQueueSize = AllTopicPartitionsNumber;
					State.ConnectionState = HandlerState.State.Ready;
					StartReceivingMessages(_consumers.Values.ToList());
					_log.Info("[{}] [{}] Created topics consumer with {} sub-consumers", Topic, Subscription, AllTopicPartitionsNumber);

					Become(Ready);
				}
                else
                {
					_log.Warning("[{}] [{}] Failed to create all topics consumer with {} sub-consumers so far", Topic, Subscription, AllTopicPartitionsNumber);

				}
			});
			ReceiveAny(_ => Stash.Stash());
		}
		private void Ready()
		{			// start track and auto subscribe partition increasement
			if (_internalConfig.AutoUpdatePartitions)
			{
				_partitionsAutoUpdateTimeout = _scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(60000), TimeSpan.FromSeconds(_internalConfig.AutoUpdatePartitionsIntervalSeconds), Self, UpdatePartitionSub.Instance, ActorRefs.NoSender);
			}
			Receive<SendState>(_ =>
			{
				StateActor.Tell(new SetConumerState(State.ConnectionState));
			});
			ReceiveAsync<UpdatePartitionSub>(async s =>
			{
				await SubscribeIncreasedTopicPartitions(Topic);
			});
			
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
			ReceiveAsync<AcknowledgeMessage<T>>(async m => {
				try
				{
					await Acknowledge(m.Message);

					Push(ConsumerQueue.AcknowledgeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			ReceiveAsync<AcknowledgeMessageId>(async m => {
				try
				{
					await Acknowledge(m.MessageId);
					Push(ConsumerQueue.AcknowledgeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			ReceiveAsync<AcknowledgeMessageIds>(async m => {
				try
				{
					await Acknowledge(m.MessageIds);
					Push(ConsumerQueue.AcknowledgeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			ReceiveAsync<AcknowledgeMessages<T>>(async m => {
				try
				{
					await Acknowledge(m.Messages);
					Push(ConsumerQueue.AcknowledgeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			ReceiveAsync<AcknowledgeCumulativeMessageId>(async m => {
				try
				{
					await AcknowledgeCumulative(m.MessageId);
					Push(ConsumerQueue.AcknowledgeCumulativeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeCumulativeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			ReceiveAsync<AcknowledgeCumulativeMessage<T>>(async m => {
				try
				{
					await AcknowledgeCumulative(m.Message);
					Push(ConsumerQueue.AcknowledgeCumulativeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeCumulativeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			ReceiveAsync<AcknowledgeCumulativeTxn>(async m => {
				try
				{
					await AcknowledgeCumulative(m.MessageId, m.Txn);
					Push(ConsumerQueue.AcknowledgeCumulativeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.AcknowledgeCumulativeException, new ClientExceptions(new PulsarClientException(ex)));
				}
			});
			ReceiveAsync<ReconsumeLaterCumulative<T>>(async m => {
				try
				{
					await ReconsumeLaterCumulative(m.Message, m.DelayTime);
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
			ReceiveAsync<ReconsumeLaterCumulative<T>>(async m => {
				try
				{
					await ReconsumeLaterCumulative(m.Message, m.DelayTime);
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
			ReceiveAsync<ReceivedMessage<T>>(async m => {
				await ReceiveMessageFromConsumer(Sender, m.Message);
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
			ReceiveAsync<RemoveTopicConsumer>(async t => {
				await RemoveConsumer(t.Topic);
			});
			ReceiveAsync<HasMessageAvailable>(async _ => {
				var has = await HasMessageAvailable();
				ConsumerQueue.HasMessageAvailable.Add(has);
			});
			ReceiveAsync<GetNumMessagesInQueue>(async _ => {
				var num = await NumMessagesInQueue();
				Sender.Tell(num);
			});
			ReceiveAsync<Resume>(async _ => {
				await Resume();
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
			ReceiveAsync<AcknowledgeWithTxnMessages>(async m =>
			{
				try
				{
					await DoAcknowledgeWithTxn(m.MessageIds, m.AckType, m.Properties, m.Txn);
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
			ReceiveAsync<ReconsumeLaterMessages<T>>(async m =>
			{
				try
				{
					await ReconsumeLater(m.Messages, m.DelayTime);
					Push(ConsumerQueue.ReconsumeLaterException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.ReconsumeLaterException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			ReceiveAsync<ReconsumeLaterWithProperties<T>>(async m =>
			{
				try
				{
					await DoReconsumeLater(m.Message, m.AckType, m.Properties.ToDictionary(x => x.Key, x => x.Value), m.DelayTime);
					Push(ConsumerQueue.ReconsumeLaterException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.ReconsumeLaterException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			ReceiveAsync<ReconsumeLaterMessage<T>>(async m =>
			{
				try
				{
					await ReconsumeLater (m.Message, m.DelayTime);
					Push(ConsumerQueue.ReconsumeLaterException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.ReconsumeLaterException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			ReceiveAsync<RedeliverUnacknowledgedMessages>(async m =>
			{
				try
				{
					await RedeliverUnacknowledgedMessages();
					Push(ConsumerQueue.RedeliverUnacknowledgedException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.RedeliverUnacknowledgedException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			ReceiveAsync<RedeliverUnacknowledgedMessageIds>(async m =>
			{
				try
				{
					await RedeliverUnacknowledgedMessages(m.MessageIds);
					Push(ConsumerQueue.RedeliverUnacknowledgedException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.RedeliverUnacknowledgedException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			ReceiveAsync<UnsubscribeTopic>(async u =>
			{
				try
				{
					await Unsubscribe(u.Topic);
					Push(ConsumerQueue.UnsubscribeException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.UnsubscribeException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			ReceiveAsync<SeekMessageId>(async m =>
			{
				try
				{
					await Seek(m.MessageId);
					Push(ConsumerQueue.SeekException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.SeekException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});
			ReceiveAsync<SeekTimestamp>(async m =>
			{
				try
				{
					await Seek(m.Timestamp);
					Push(ConsumerQueue.SeekException, null);
				}
				catch (Exception ex)
				{
					Push(ConsumerQueue.SeekException, new ClientExceptions(PulsarClientException.Unwrap(ex)));
				}
			});

			Stash.UnstashAll();
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

			var result = await _client.Ask(new GetPartitionedTopicMetadata(TopicName.Get(topicName)));
			if (result is PartitionedTopicMetadata metadata)
			{
				await SubscribeTopicPartitions(fullTopicName, metadata.Partitions, createTopicIfDoesNotExist);
			}
			else if (result is Failure failure)
            {
				_log.Warning($"[{fullTopicName}] Failed to get partitioned topic metadata: {failure.Exception}");
			}
		}
		private async ValueTask<ISchema<T>> PreProcessSchemaBeforeSubscribe(ISchema<T> schema, string topicName)
		{
			if (schema != null && schema.SupportSchemaVersioning())
			{
				ISchemaInfoProvider schemaInfoProvider;
				try
				{
					schemaInfoProvider = _schemaProviderLoadingCache.Get(topicName);
					if (schemaInfoProvider == null)
						_schemaProviderLoadingCache.Put(topicName, NewSchemaProvider(topicName));
				}
				catch (Exception e)
				{
					_log.Error($"Failed to load schema info provider for topic {topicName}: {e}");
					throw e;
				}
				schema = schema.Clone();
				if (schema.RequireFetchingSchemaInfo())
				{
					var finalSchema = schema;
					var schemaInfo = await schemaInfoProvider.LatestSchema().ConfigureAwait(false);
					if (null == schemaInfo)
					{
						if (!(finalSchema is AutoConsumeSchema))
						{
							throw new PulsarClientException.NotFoundException("No latest schema found for topic " + topicName);
						}
					}
					_log.Info($"Configuring schema for topic {topicName} : {schemaInfo}");
					finalSchema.ConfigureSchemaInfo(topicName, "topic", schemaInfo);
					finalSchema.SchemaInfoProvider = schemaInfoProvider;
					return finalSchema;
				}
				else
				{
					schema.SchemaInfoProvider = schemaInfoProvider;
				}
			}
			return schema;
		}
		private ISchemaInfoProvider NewSchemaProvider(string topicName)
		{
			return new MultiVersionSchemaInfoProvider(TopicName.Get(topicName), _log, _lookup);
		}
		

		private void StartReceivingMessages(IList<IActorRef> newConsumers)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}] startReceivingMessages for {newConsumers.Count} new consumers in topics consumer, state: {State.ConnectionState}");
			}
			if(State.ConnectionState == HandlerState.State.Ready)
			{
				newConsumers.ForEach(consumer =>
				{
					consumer.Tell(new IncreaseAvailablePermits(Conf.ReceiverQueueSize));
				});
			}
		}
		private async ValueTask ReceiveMessageFromConsumer(IActorRef consumer, IMessage<T> message)
		{
			var c = _consumers.Where(x => x.Value == consumer).FirstOrDefault();
			var topic = await consumer.Ask<string>(GetTopic.Instance);
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}] [{Subscription}] Receive message from sub consumer:{topic}");
			}
			MessageReceived(topic, consumer, message);
			int size = IncomingMessages.Count;
			if (size >= MaxReceiverQueueSize || (size > _sharedQueueResumeThreshold && _pausedConsumers.Count > 0))
			{
				_pausedConsumers.Enqueue(consumer);
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
				try
				{
					if (_log.IsDebugEnabled)
					{
						_log.Debug($"[{Topic}][{Subscription}] Calling message listener for message {message.MessageId}");
					}
					Listener.Received(_self, message);
				}
				catch (Exception t)
				{
					_log.Error($"[{Topic}][{Subscription}] Message listener error in processing message: {message}: {t}");
				}
			}
            else
            {
				IncomingMessages.Post(topicMessage);
            }
		}
		protected override void Unhandled(object message)
		{
			_log.Warning($"Unhandled Message '{message.GetType().FullName}' from '{Sender.Path}'");
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
					var consumer = _pausedConsumers.FirstOrDefault();
					if(consumer == null)
					{
						break;
					}
					consumer.Tell(Messages.Consumer.Resume.Instance);
				}
			}
		}

		protected internal override async ValueTask DoAcknowledge(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txnImpl)
		{
			Condition.CheckArgument(messageId is TopicMessageId);
			var topicMessageId = (TopicMessageId) messageId;

			if(State.ConnectionState != HandlerState.State.Ready)
			{
				throw new PulsarClientException("Consumer already closed");
			}

			if(ackType == AckType.Cumulative)
			{
				var consumer= _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
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
				var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);

				var innerId = topicMessageId.InnerMessageId;
				consumer.Tell(new AcknowledgeWithTxnMessages(new List<IMessageId> { innerId }, properties, txnImpl));
				_unAckedMessageTracker.Tell(new Remove(topicMessageId));
			}
			await Task.CompletedTask;
		}

		protected internal override async ValueTask DoAcknowledge(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
		{
			if(ackType == AckType.Cumulative)
			{
				messageIdList.ForEach( async messageId => await DoAcknowledge(messageId, ackType, properties, txn));
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
					consumer.Tell(new AcknowledgeWithTxnMessages(t.Value, properties, txn));
					messageIdList.ForEach(x => _unAckedMessageTracker.Tell(new Remove(x)));
				});
			}
			await Task.CompletedTask;
		}

		protected internal override async ValueTask DoReconsumeLater(IMessage<T> message, AckType ackType, IDictionary<string, long> properties, long delayTime)
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
				var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				if(consumer != null)
				{
					var innerId = topicMessageId.InnerMessageId;
					consumer.Tell(new ReconsumeLaterCumulative<T>(message, delayTime));
				}
				else
				{
					throw new PulsarClientException.NotConnectedException();
				}
			}
			else
			{
				var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				var innerId = topicMessageId.InnerMessageId;
				consumer.Tell(new ReconsumeLaterWithProperties<T>(message, ackType, properties, delayTime));
				_unAckedMessageTracker.Tell(new Remove(topicMessageId));
			}
			await Task.CompletedTask;
		}

		internal override void NegativeAcknowledge(IMessageId messageId)
		{
			Condition.CheckArgument(messageId is TopicMessageId);
			var topicMessageId = (TopicMessageId) messageId;

			var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
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
			_consumers.Values.ForEach(c => c.GracefulStop(TimeSpan.FromMilliseconds(100)));
			State.ConnectionState = HandlerState.State.Closed;
			_unAckedMessageTracker.GracefulStop(TimeSpan.FromMilliseconds(100));
			_log.Info($"[{Topic}] [{Subscription}] Closed Topics Consumer");
			_client.Tell(new CleanupConsumer(Self));

		}


		internal override async ValueTask<bool> Connected()
		{
			foreach (var c in _consumers.Values)
			{
				var s = await c.Ask<bool>(IsConnected.Instance);
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

		internal override async ValueTask RedeliverUnacknowledgedMessages()
		{
			_consumers.Values.ForEach(consumer =>
			{
				consumer.Tell(Messages.Consumer.RedeliverUnacknowledgedMessages.Instance);
				consumer.Tell(ClearUnAckedChunckedMessageIdSequenceMap.Instance);
			});
			await IncomingMessages.Empty();
			IncomingMessagesSize =  0;
			_unAckedMessageTracker.Tell(Clear.Instance);
			ResumeReceivingFromPausedConsumersIfNeeded();
			await Task.CompletedTask;
		}

		protected internal override async ValueTask RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds)
		{
			if(messageIds.Count == 0)
			{
				return;
			}

			Condition.CheckArgument(messageIds.First() is TopicMessageId);

			if(Conf.SubscriptionType != CommandSubscribe.SubType.Shared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				await RedeliverUnacknowledgedMessages();
				return;
			}
			await RemoveExpiredMessagesFromQueue(messageIds);
			messageIds.Select(messageId => (TopicMessageId)messageId).Collect()
				.ForEach(t => _consumers.GetValueOrNull(t.First().TopicPartitionName)
				.Tell(new RedeliverUnacknowledgedMessageIds(t.Select(mid => mid.InnerMessageId).ToHashSet())));
			ResumeReceivingFromPausedConsumersIfNeeded();
		}

		internal override async ValueTask Seek(IMessageId messageId)
		{
			try
			{
				var targetMessageId = MessageId.ConvertToMessageId(messageId);
				if (targetMessageId == null || IsIllegalMultiTopicsMessageId(messageId))
				{
					throw new PulsarClientException("Illegal messageId, messageId can only be earliest/latest");
				}
				_consumers.Values.ForEach(c => c.Tell(new SeekMessageId(targetMessageId)));

				_unAckedMessageTracker.Tell(Clear.Instance);
				await IncomingMessages .Empty();
				IncomingMessagesSize = 0;
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
			await Task.CompletedTask;
		}

		internal override async ValueTask Seek(long timestamp)
		{
			try
			{
				_consumers.Values.ForEach(c => c.Tell(new SeekTimestamp(timestamp)));
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
			await Task.CompletedTask;
		}


		internal override async ValueTask<int> AvailablePermits()
		{
			var sum = 0;
			foreach (var c in _consumers.Values)
			{
				var s = await c.Ask<int>(GetAvailablePermits.Instance);
				sum += s;
			}
			return sum;
		}

		private async ValueTask<bool> HasReachedEndOfTopic()
		{
			foreach (var c in _consumers.Values)
			{
				var s = await c.Ask<bool>(Messages.Consumer.HasReachedEndOfTopic.Instance);
				if (!s)
					return false;
			}
			return true;
		}
		private async ValueTask HandlerStates()
		{
			var st = await _stateActor.Ask<HandlerStateResponse>(GetHandlerState.Instance);
			Sender.Tell(st);
		}
		private async ValueTask<bool> HasMessageAvailable()
		{
			foreach (var c in _consumers.Values)
			{
                try
                {
					var s = await c.Ask<bool>(Messages.Consumer.HasMessageAvailable.Instance);
					if (!s)
						return false;
				}
                catch
                {
					return false;
                }
			}
			return true;
		}

		internal override async ValueTask<int> NumMessagesInQueue()
		{
			var sum = 0;
			foreach(var c in _consumers.Values)
            {
				var s = await c.Ask<int>(GetNumMessagesInQueue.Instance);
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

				_consumers.Values.ForEach(async consumer => _stats.UpdateCumulativeStats(await consumer.Ask<IConsumerStats>(GetStats.Instance)));
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

		private async ValueTask RemoveExpiredMessagesFromQueue(ISet<IMessageId> messageIds)
		{
			var peek = await IncomingMessages.ReceiveAsync();
			if (peek != null)
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
					message = await IncomingMessages.ReceiveAsync();
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
			var schemaClone = await PreProcessSchemaBeforeSubscribe(Schema, topicName).ConfigureAwait(false);
			await DoSubscribeTopicPartitions(schemaClone, topicName, numPartitions, createIfDoesNotExist);
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
				for(var i = 0; i < numPartitions; i++)
                {
					var consumerId = await _generator.Ask<long>(NewConsumerId.Instance);
					string partitionName = TopicName.Get(topicName).GetPartition(i).ToString();
					var newConsumer = CreateConsumer(consumerId, partitionName, configurationData, i, schema, Interceptors, createIfDoesNotExist);
					_consumers.Add(partitionName, newConsumer);
				}
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
				var consumerId = await _generator.Ask<long>(NewConsumerId.Instance);

				var newConsumer = CreateConsumer(consumerId, topicName, _internalConfig, -1, schema, Interceptors, createIfDoesNotExist);
				_consumers.Add(topicName, newConsumer);
			}

			if (AllTopicPartitionsNumber > MaxReceiverQueueSize)
			{
				MaxReceiverQueueSize = AllTopicPartitionsNumber;
			}
			int numTopics = TopicsMap.Values.Sum();
			int currentAllTopicsPartitionsNumber = AllTopicPartitionsNumber;
			if(currentAllTopicsPartitionsNumber != numTopics)
				throw new ArgumentException("allTopicPartitionsNumber " + currentAllTopicsPartitionsNumber + " not equals expected: " + numTopics);

			var recFromTops = new List<IActorRef>();
			foreach(var c in _consumers.Values)
            {
				string consumerTopicName = await c.Ask<string>(GetTopic.Instance);
				if (TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(TopicName.Get(topicName).PartitionedTopicName))
					recFromTops.Add(c);
			}
			StartReceivingMessages(recFromTops);
			_log.Info($"[{Topic}] [{Subscription}] Success subscribe new topic {topicName} in topics consumer, partitions: {numPartitions}, allTopicPartitionsNumber: {AllTopicPartitionsNumber}");
			
			//HandleSubscribeOneTopicError(topicName, ex, subscribeResult);
		}

		private IActorRef CreateConsumer(long consumerId, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, ISchema<T> schema, ConsumerInterceptors<T> interceptors, bool createIfDoesNotExist)
        {
			if (conf.ReceiverQueueSize == 0)
			{
				return Context.ActorOf(Props.Create(() => new ZeroQueueConsumer<T>(consumerId, _stateActor, _client, _lookup, _cnxPool, _generator, topic, conf, Context.System.Scheduler.Advanced, partitionIndex, false, _startMessageId, schema, null, createIfDoesNotExist, _clientConfiguration, ConsumerQueue)));
			}
			return Context.ActorOf(Props.Create(() => new ConsumerActor<T>(consumerId, _stateActor, _client, _lookup, _cnxPool, _generator, topic, conf, Context.System.Scheduler.Advanced, partitionIndex, true, _startMessageId, _startMessageRollbackDurationInSec, schema, null, createIfDoesNotExist, _clientConfiguration, ConsumerQueue)));

		}
		// handling failure during subscribe new topic, unsubscribe success created partitions
		private async ValueTask HandleSubscribeOneTopicError(string topicName, Exception error)
		{
			_log.Warning($"[{Topic}] Failed to subscribe for topic [{topicName}] in topics consumer {error}");
			int toCloseNum = 0;
			var recFromTops = new List<IActorRef>();
			foreach (var c in _consumers.Values)
			{
				string consumerTopicName = await c.Ask<string>(GetTopic.Instance);
				if (TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(TopicName.Get(topicName).PartitionedTopicName))
				{
					++toCloseNum;
					recFromTops.Add(c);
				}
			}
			foreach (var consumer2 in recFromTops)
			{
				await consumer2.GracefulStop(TimeSpan.FromMilliseconds(100));
				--AllTopicPartitionsNumber;
				var topic = await consumer2.Ask<string>(GetTopic.Instance);
				_consumers.Remove(topic);
				if (--toCloseNum == 0)
				{
					_log.Warning($"[{Topic}] Failed to subscribe for topic [{topicName}] in topics consumer, subscribe error: {error}");
					RemoveTopic(topicName);
				}
			}
		}

		// un-subscribe a given topic
		private async ValueTask Unsubscribe(string topicName)
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

			var recFromTops = new List<IActorRef>();
			foreach (var c in _consumers.Values)
			{
				string consumerTopicName = await c.Ask<string>(GetTopic.Instance);
				if (TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(topicPartName))
				{
					recFromTops.Add(c);
				}
			}
			var unsubed = true;
			ClientExceptions except = null;
			foreach(var co in recFromTops)
            {
				var response = await co.Ask(Messages.Consumer.Unsubscribe.Instance);
				if (response == null)
				{
					var t = await co.Ask<string>(GetTopic.Instance);
					_consumers.Remove(t);
					_pausedConsumers = new Queue<IActorRef>(_pausedConsumers.Where(x => x != co));
					--AllTopicPartitionsNumber;
				}
				else
                {
					unsubed = false;
					except = response as ClientExceptions;
				}

				
			}
			if(unsubed)
			{
				RemoveTopic(topicName);
				_unAckedMessageTracker.Tell(new RemoveTopicMessages(topicName));
				_log.Info($"[{topicName}] [{Subscription}] [{ConsumerName}] Unsubscribed Topics Consumer, allTopicPartitionsNumber: {AllTopicPartitionsNumber}");

			}
            else
            {
				State.ConnectionState = HandlerState.State.Failed;
				_log.Error($"[{topicName}] [{Subscription}] [{ConsumerName}] Could not unsubscribe Topics Consumer: {except?.Exception}");

			}
		}

		// Remove a consumer for a topic
		private async ValueTask RemoveConsumer(string topicName)
		{
			Condition.CheckArgument(TopicName.IsValid(topicName), "Invalid topic name:" + topicName);

			if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				throw new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed");
			}

			string topicPartName = TopicName.Get(topicName).PartitionedTopicName;


			var recFromTops = new List<IActorRef>();
			foreach (var c in _consumers.Values)
			{
				string consumerTopicName = await c.Ask<string>(GetTopic.Instance);
				if (TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(topicPartName))
				{
					recFromTops.Add(c);
				}
			}
			foreach (var co in recFromTops)
			{
				var response = await co.GracefulStop(TimeSpan.FromSeconds(1));
				var t = await co.Ask<string>(GetTopic.Instance);
				_consumers.Remove(t);
				_pausedConsumers = new Queue<IActorRef>(_pausedConsumers.Where(x => x != co));
				--AllTopicPartitionsNumber;
			}
			RemoveTopic(topicName);
			_unAckedMessageTracker.Tell(new RemoveTopicMessages(topicName));
			_log.Info($"[{topicName}] [{Subscription}] [{ConsumerName}] Removed Topics Consumer, allTopicPartitionsNumber: {AllTopicPartitionsNumber}");

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
				return _consumers.Values.ToList();
			}
		}

        public IStash Stash { get; set; }

        internal override void Pause()
		{
			_paused = true;
			_consumers.ForEach(x => x.Value.Tell(Messages.Consumer.Pause.Instance));
		}

		internal override async ValueTask Resume()
		{
			_paused = false;
			_consumers.ForEach(x => x.Value.Tell(Messages.Consumer.Resume.Instance));
			await Task.CompletedTask;
		}

		internal override async ValueTask<long> LastDisconnectedTimestamp()
		{
			long lastDisconnectedTimestamp = 0;
			foreach(var c in _consumers.Values)
            {
				var x = await c.Ask<long>(GetLastDisconnectedTimestamp.Instance);
				if (x > lastDisconnectedTimestamp)
					lastDisconnectedTimestamp = x;
			}
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
					var consumerId = await _generator.Ask<long>(NewConsumerId.Instance);
					int partitionIndex = TopicName.GetPartitionIndex(partitionName);
					ConsumerConfigurationData<T> configurationData = InternalConsumerConfig;
					var newConsumer = _context.ActorOf(Props.Create(() => new ConsumerActor<T>(consumerId, _stateActor, _client, _lookup, _cnxPool, _generator, partitionName, configurationData, Context.System.Scheduler.Advanced, partitionIndex, true, null, Schema, Interceptors, true, _clientConfiguration, ConsumerQueue)));
					if (_paused)
					{
						newConsumer.Tell(Messages.Consumer.Pause.Instance);
					}
					_consumers.Add(partitionName, newConsumer);

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
				var t = await v.Ask<string>(GetTopic.Instance);
				IMessageId messageId;
				try
				{
					messageId = await v.Ask<IMessageId>(GetLastMessageId.Instance);
				}
				catch (Exception e)
				{
					_log.Warning($"[{t}] Exception when topic {e} getLastMessageId.");
					messageId = IMessageId.Earliest;
				}

				multiMessageId.Add(t, messageId);
			}
			return new MultiMessageId(multiMessageId);
		}

		internal static bool IsIllegalMultiTopicsMessageId(IMessageId messageId)
		{
			//only support earliest/latest
			return !IMessageId.Earliest.Equals(messageId) && !IMessageId.Latest.Equals(messageId);
		}

		internal virtual async ValueTask TryAcknowledgeMessage(IMessage<T> msg)
		{
			if(msg != null)
			{
				await AcknowledgeCumulative(msg);
			}
		}

		internal sealed class UpdatePartitionSub
        {
			public static UpdatePartitionSub Instance = new UpdatePartitionSub();
        }
	}

}