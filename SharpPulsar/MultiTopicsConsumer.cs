using Akka.Actor;
using Akka.Util.Internal;
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
using SharpPulsar.Schema;
using SharpPulsar.Schemas.Generic;
using SharpPulsar.Stats.Consumer;
using SharpPulsar.Stats.Consumer.Api;
using SharpPulsar.Tracker;
using SharpPulsar.Tracker.Messages;
using System;
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
		public MultiTopicsConsumer(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration) : this(stateActor, client, lookup, cnxPool, idGenerator, DummyTopicNamePrefix + Utility.ConsumerName.GenerateRandomName(), conf, listenerExecutor, schema, createTopicIfDoesNotExist, clientConfiguration)
		{
		}
		public MultiTopicsConsumer(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, bool createTopicIfDoesNotExist, IMessageId startMessageId, long startMessageRollbackDurationInSec, ClientConfigurationData clientConfiguration) : this(stateActor, client, lookup, cnxPool, idGenerator, DummyTopicNamePrefix + Utility.ConsumerName.GenerateRandomName(), conf, listenerExecutor, schema, createTopicIfDoesNotExist, startMessageId, startMessageRollbackDurationInSec, clientConfiguration)
		{
		}

		public MultiTopicsConsumer(IActorRef stateActor,IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string singleTopic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration) : this(stateActor, client, lookup, cnxPool, idGenerator, singleTopic, conf, listenerExecutor, schema, createTopicIfDoesNotExist, null, 0, clientConfiguration)
		{
		}

		public MultiTopicsConsumer(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string singleTopic, ConsumerConfigurationData<T> conf, IAdvancedScheduler listenerExecutor, ISchema<T> schema, bool createTopicIfDoesNotExist, IMessageId startMessageId, long startMessageRollbackDurationInSec, ClientConfigurationData clientConfiguration) : base(stateActor, lookup, cnxPool, singleTopic, conf, Math.Max(2, conf.ReceiverQueueSize), listenerExecutor, schema)
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
                foreach (var topic in s.Topics)
                {
                    _ = await Subscribe(topic, s.CreateTopicIfDoesNotExist);
                }

                if (AllTopicPartitionsNumber == TopicsMap.Values.Sum())
                {
                    MaxReceiverQueueSize = AllTopicPartitionsNumber;
                    State.ConnectionState = HandlerState.State.Ready;
                    StartReceivingMessages(_consumers.Values.ToList());
                    _log.Info("[{}] [{}] Created topics consumer with {} sub-consumers", Topic, Subscription, AllTopicPartitionsNumber);
                    Sender.Tell(new AskResponse());
                    Become(Ready);
                }
                else
                {
                    var msg = $"[{Topic}] [{Subscription}] Failed to create all topics consumer with {AllTopicPartitionsNumber} sub-consumers so far";

                    _log.Warning(msg);
                    Sender.Tell(new AskResponse(new PulsarClientException(msg)));
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
			Receive<BatchReceive>(_ =>
			{
                BatchReceive();
			});
            Receive<Messages.Consumer.Receive>(_ =>
            {
                Receive();
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
				var (_, response) = await Subscribe(s.TopicName, s.CreateTopicIfDoesNotExist);
                Sender.Tell(response);
			});
			Receive<GetLastDisconnectedTimestamp>(m =>
			{
				var l = LastDisconnectedTimestamp();
                Sender.Tell(l);
			});
			Receive<GetConsumerName>(m => {
                Sender.Tell(ConsumerName);
			});
			Receive<GetSubscription>(m => {
                Sender.Tell(Subscription);
			});
			Receive<GetTopic>(m => {
                Sender.Tell(Topic); 
			});
            Receive<GetIncomingMessageCount>(_ =>
            {
                Sender.Tell(new AskResponse(IncomingMessages.Count));
            });
            ReceiveAsync<ReceivedMessage<T>>(async m => {
				await ReceiveMessageFromConsumer(Sender, m.Message);
			});
			Receive<ClearUnAckedChunckedMessageIdSequenceMap>(_ => {
				UnAckedChunckedMessageIdSequenceMap.Clear();
			});
			ReceiveAsync<HasReachedEndOfTopic>(async _ => {
				var hasReached = await HasReachedEndOfTopic();
                Sender.Tell(new AskResponse(hasReached));
            });
			Receive<GetAvailablePermits>(_ => {
				var permits = AvailablePermits();
				Sender.Tell(permits);
			});
			Receive<IsConnected>( _ => {
				var connected = Connected();
                Sender.Tell(connected);
            });
			Receive<Pause>(_ => {
				Pause();
			});
			ReceiveAsync<RemoveTopicConsumer>(async t => {
				await RemoveConsumer(t.Topic);
			});
			ReceiveAsync<HasMessageAvailable>(async _ => {
				var has = await HasMessageAvailable();
				Sender.Tell(has);
			});
			Receive<GetNumMessagesInQueue>(_ => {
				var num = NumMessagesInQueue();
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
                    Sender.Tell(lmsid);
				}
				catch (Exception ex)
				{
                    Sender.Tell(PulsarClientException.Unwrap(ex));
				}
			});
			Receive<GetStats>(m =>
			{
				try
				{
					var stats = Stats;
					Sender.Tell(stats);
				}
				catch (Exception ex)
				{
					_log.Error(ex.ToString());
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<NegativeAcknowledgeMessage<T>>(m =>
			{
				try
				{
                    var topicMessageId = (TopicMessageId)m.Message.MessageId;
                    var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
                    consumer.Tell(m, Sender);
				}
				catch (Exception ex)
				{
					Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<NegativeAcknowledgeMessages<T>>(m =>
			{
				try
				{
                    foreach(var message in m.Messages)
                    {
                        var topicMessageId = (TopicMessageId)message.MessageId;
                        var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
                        consumer.Tell(new NegativeAcknowledgeMessage<T>(message), Sender);
                    }
                    Sender.Tell(new AskResponse());
				}
				catch (Exception ex)
				{
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex))); 
				}
			});
			Receive<NegativeAcknowledgeMessageId>(m =>
			{
				try
				{
                    var topicMessageId = (TopicMessageId)m.MessageId;
                    var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
                    consumer.Tell(m, Sender);
				}
				catch (Exception ex)
				{
					Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<IAcknowledge>(m =>
			{
                switch(m)
                {
                    case AcknowledgeMessage<T> ack:
                        DoAcknowledge(ack.Message.MessageId, AckType.Individual, new Dictionary<string, long>(), null);
                        break;
                    case AcknowledgeWithTxn ack:
                        DoAcknowledge(ack.MessageId, ack.AckType, ack.Properties, ack.Txn);
                        break;
                    case AcknowledgeWithTxnMessages ack:
                        DoAcknowledge(ack.MessageIds, ack.AckType, ack.Properties, ack.Txn);
                        break;
                    case AcknowledgeMessageId ack:
                        DoAcknowledge(ack.MessageId, AckType.Individual, new Dictionary<string, long>(), null);
                        break;
                    case AcknowledgeMessageIds ack:
                        DoAcknowledge(ack.MessageIds, AckType.Individual, new Dictionary<string, long>(), null);
                        break;
                    case AcknowledgeMessages<T> ms:
                        foreach (var x in ms.Messages)
                        {
                            DoAcknowledge(x.MessageId, AckType.Individual, new Dictionary<string, long>(), null);
                        }
                        break;
                    default:
                        Sender.Tell(new AskResponse());
                        break;
                }
                
			});
			Receive<ICumulative>(message =>
			{
                switch(message)
                {
                    case AcknowledgeCumulativeMessage<T> m:
                        DoAcknowledge(m.Message.MessageId, AckType.Cumulative, new Dictionary<string, long>(), null);
                        break;
                    case AcknowledgeCumulativeMessageId m:
                        DoAcknowledge(m.MessageId, AckType.Cumulative, new Dictionary<string, long>(), null);
                        break;
                    case AcknowledgeCumulativeTxn m:
                        DoAcknowledge(m.MessageId, AckType.Cumulative, new Dictionary<string, long>(), m.Txn);
                        break;
                    case ReconsumeLaterCumulative<T> ack:
                        DoReconsumeLater(ack.Message, AckType.Cumulative, new Dictionary<string, long>(), ack.DelayTime);
                        break;
                    default:
                        Sender.Tell(new AskResponse());
                        break;
                }
                
			});

            Receive<ReconsumeLaterMessages<T>>(m =>
            {
                try
                {
                    foreach(var message in m.Messages)
                        DoReconsumeLater(message, AckType.Individual, new Dictionary<string, long>(), m.DelayTime);

                    Sender.Tell(new AskResponse());
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
                }
            });
			Receive<RedeliverUnacknowledgedMessages>(m =>
			{
				try
				{
					RedeliverUnacknowledgedMessages();
                    Sender.Tell(new AskResponse());
				}
				catch (Exception ex)
				{
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex))); 
				}
			});
			Receive<RedeliverUnacknowledgedMessageIds>(m =>
			{
				try
				{
					RedeliverUnacknowledgedMessages(m.MessageIds);
                    Sender.Tell(new AskResponse());
				}
				catch (Exception ex)
				{
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex))); 
				}
			});
			Receive<UnsubscribeTopic>( u =>
			{
				try
				{
					Unsubscribe(u.Topic);
                    Sender.Tell(new AskResponse());
				}
				catch (Exception ex)
				{
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex))); 
				}
			});
			Receive<SeekMessageId>(m =>
			{
				try
				{
					Seek(m.MessageId);
                    Sender.Tell(new AskResponse());
                }
				catch (Exception ex)
				{
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<SeekTimestamp>(m =>
			{
				try
				{
					Seek(m.Timestamp);
                    Sender.Tell(new AskResponse());
				}
				catch (Exception ex)
				{
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
				}
			});

			Stash.UnstashAll();
		}
        
        // subscribe one more given topic
        private async ValueTask<(IActorRef consumer, AskResponse response)> Subscribe(string topicName, bool createTopicIfDoesNotExist)
		{
			var topicNameInstance = GetTopicName(topicName);
			if (topicNameInstance == null)
			{
				return (null, new AskResponse(new PulsarClientException.AlreadyClosedException("Topic name not valid")));
			}
			var fullTopicName = topicNameInstance.ToString();
			if (TopicsMap.ContainsKey(fullTopicName) || TopicsMap.ContainsKey(topicNameInstance.PartitionedTopicName))
			{
                return (null, new AskResponse(new PulsarClientException.AlreadyClosedException("Already subscribed to " + topicName)));
			}

			if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
                return (null, new AskResponse(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed")));
			}

			var result = await _lookup.Ask(new GetPartitionedTopicMetadata(TopicName.Get(topicName)));
			if (result is PartitionedTopicMetadata metadata)
			{
				return await SubscribeTopicPartitions(fullTopicName, metadata.Partitions, createTopicIfDoesNotExist);
			}
			else if (result is Failure failure)
            {
                var error = $"[{fullTopicName}] Failed to get partitioned topic metadata: {failure.Exception}";

                _log.Warning(error);
                return (null, new AskResponse(new PulsarClientException(error)));
            }
            return (null, new AskResponse());
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
			MessageReceived(topic, message);
			var size = IncomingMessages.Count;
			if (size >= MaxReceiverQueueSize || (size > _sharedQueueResumeThreshold && _pausedConsumers.Count > 0))
			{
				_pausedConsumers.Enqueue(consumer);
				consumer.Tell(Messages.Consumer.Pause.Instance);
			}
		}

		private void MessageReceived(string topic, IMessage<T> message)
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
                while (true)
				{
                    try
                    {
                        var consumer = _pausedConsumers.Dequeue();

                        if (consumer == null)
                        {
                            break;
                        }
                        consumer.Tell(Messages.Consumer.Resume.Instance);
                    }
                    catch
                    {
                        break;
                    }
				}
			}
		}

		private void DoAcknowledge(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txnImpl)
		{
			Condition.CheckArgument(messageId is TopicMessageId);
			var topicMessageId = (TopicMessageId) messageId;

			if(State.ConnectionState != HandlerState.State.Ready)
			{
                Sender.Tell(new AskResponse(new PulsarClientException("Consumer already closed")));
			}

			if(ackType == AckType.Cumulative)
			{
				var consumer= _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				if(consumer != null)
				{
					var innerId = topicMessageId.InnerMessageId;
					consumer.Tell(new AcknowledgeCumulativeMessageId(innerId), Sender);
				}
				else
				{
					Sender.Tell(new AskResponse(new PulsarClientException.NotConnectedException()));
				}
			}
			else
			{
				var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);

				var innerId = topicMessageId.InnerMessageId;
				consumer.Tell(new AcknowledgeWithTxnMessages(new List<IMessageId> { innerId }, properties, txnImpl), Sender);
				_unAckedMessageTracker.Tell(new Remove(topicMessageId));
			}
		}

		private void DoAcknowledge(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
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
				foreach(var messageId in messageIdList)
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
		}

		private void DoReconsumeLater(IMessage<T> message, AckType ackType, IDictionary<string, long> properties, TimeSpan delayTime)
		{
			var messageId = message.MessageId;
			Condition.CheckArgument(messageId is TopicMessageId);
			var topicMessageId = (TopicMessageId) messageId;
			if(State.ConnectionState != HandlerState.State.Ready)
			{
				Sender.Tell(new AskResponse(new PulsarClientException("Consumer already closed")));
                return;
			}

			if(ackType == AckType.Cumulative)
			{
				var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				if(consumer != null)
				{
					consumer.Tell(new ReconsumeLaterCumulative<T>(message, delayTime), Sender);
				}
				else
				{
                    Sender.Tell(new AskResponse(new PulsarClientException.NotConnectedException()));
				}
			}
			else
			{
				var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
				consumer.Tell(new ReconsumeLaterMessage<T>(message, delayTime), Sender);
				_unAckedMessageTracker.Tell(new Remove(topicMessageId));
			}
		}

		internal override void NegativeAcknowledge(IMessageId messageId)
		{
			Condition.CheckArgument(messageId is TopicMessageId);
			var topicMessageId = (TopicMessageId) messageId;

			var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
			consumer.Tell(new NegativeAcknowledgeMessageId(topicMessageId.InnerMessageId), Sender);
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


		internal override bool Connected()
		{
			foreach (var c in _consumers.Values)
			{
				var s = c.Ask<bool>(IsConnected.Instance).GetAwaiter().GetResult();
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
				var internalConsumerConfig = Conf;
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
				consumer.Tell(Messages.Consumer.RedeliverUnacknowledgedMessages.Instance);
				consumer.Tell(ClearUnAckedChunckedMessageIdSequenceMap.Instance);
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
				.Tell(new RedeliverUnacknowledgedMessageIds(t.Select(mid => mid.InnerMessageId).ToHashSet())));
			ResumeReceivingFromPausedConsumersIfNeeded();
		}

		internal override void Seek(IMessageId messageId)
		{
			try
			{
				var targetMessageId = MessageId.ConvertToMessageId(messageId);
				if (targetMessageId == null || IsIllegalMultiTopicsMessageId(messageId))
				{
					Sender.Tell(new AskResponse(new PulsarClientException("Illegal messageId, messageId can only be earliest/latest")));
				}
				_consumers.Values.ForEach(c => c.Tell(new SeekMessageId(targetMessageId)));

				_unAckedMessageTracker.Tell(Clear.Instance);
				IncomingMessages.Empty();
				IncomingMessagesSize = 0;
			}
			catch(Exception e)
			{
                Sender.Tell(new AskResponse(PulsarClientException.Unwrap(e)));
			}
		}

		internal override void Seek(long timestamp)
		{
			try
			{
				_consumers.Values.ForEach(c => c.Tell(new SeekTimestamp(timestamp)));
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}


		internal override int AvailablePermits()
		{
			var sum = 0;
			foreach (var c in _consumers.Values)
			{
				var s = c.Ask<int>(GetAvailablePermits.Instance).GetAwaiter().GetResult();
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

		internal override int NumMessagesInQueue()
		{
			var sum = 0;
			foreach(var c in _consumers.Values)
            {
				var s = c.Ask<int>(GetNumMessagesInQueue.Instance).GetAwaiter().GetResult();
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

		private void RemoveExpiredMessagesFromQueue(ISet<IMessageId> messageIds)
		{
			var peek = IncomingMessages.Receive();
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
					message = IncomingMessages.Receive();
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
			var topicName = GetTopicName(topic);
			return (topicName != null) ? topicName.ToString() : null;
		}

		private void RemoveTopic(string topic)
		{
			var fullTopicName = GetFullTopicName(topic);
			if(!ReferenceEquals(fullTopicName, null))
			{
				TopicsMap.Remove(topic);
			}
		}

		internal async ValueTask Subscribe(string topicName, int numberPartitions)
		{
			var topicNameInstance = GetTopicName(topicName);
			if(topicNameInstance == null)
			{
				Sender.Tell(new AskResponse(new PulsarClientException.AlreadyClosedException("Topic name not valid")));
			}
			var fullTopicName = topicNameInstance.ToString();
			if(TopicsMap.ContainsKey(fullTopicName) || TopicsMap.ContainsKey(topicNameInstance.PartitionedTopicName))
			{
                Sender.Tell(new AskResponse(new PulsarClientException.AlreadyClosedException("Already subscribed to " + topicName)));
			}

			if(State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
                Sender.Tell(new AskResponse(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed")));
			}
			
			var response = await SubscribeTopicPartitions(fullTopicName, numberPartitions, true);
            Sender.Tell(response.response);
		}

		private async ValueTask<(IActorRef consumer, AskResponse response)> SubscribeTopicPartitions(string topicName, int numPartitions, bool createIfDoesNotExist)
		{
            try
            {
                var schemaClone = await PreProcessSchemaBeforeSubscribe(Schema, topicName).ConfigureAwait(false);
                return await DoSubscribeTopicPartitions(schemaClone, topicName, numPartitions, createIfDoesNotExist);
            }
            catch(Exception ex)
            {
                return (null, new AskResponse(new PulsarClientException(ex)));
            }
		}

		private async ValueTask<(IActorRef consumer, AskResponse response)> DoSubscribeTopicPartitions(ISchema<T> schema, string topicName, int numPartitions, bool createIfDoesNotExist)
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
					var errorMessage = $"[{Topic}] Failed to subscribe for topic [{topicName}] in topics consumer. Topic is already being subscribed for in other thread.";
					_log.Warning(errorMessage);
					return (null, new AskResponse(new PulsarClientException(errorMessage)));
				}
				TopicsMap.Add(topicName, numPartitions);
				AllTopicPartitionsNumber += numPartitions;

				var receiverQueueSize = Math.Min(Conf.ReceiverQueueSize, Conf.MaxTotalReceiverQueueSizeAcrossPartitions / numPartitions);
				var configurationData = InternalConsumerConfig;
				configurationData.ReceiverQueueSize = receiverQueueSize;
				for(var i = 0; i < numPartitions; i++)
                {
					var consumerId = await _generator.Ask<long>(NewConsumerId.Instance);
					var partitionName = TopicName.Get(topicName).GetPartition(i).ToString();
					var newConsumer = await CreateConsumer(consumerId, partitionName, configurationData, i, schema, createIfDoesNotExist);
					if(!newConsumer.response.Failed)
                        _consumers.Add(partitionName, newConsumer.consumer);
				}
                if (_consumers.Count > 0)
                    return (_consumers.First().Value, new AskResponse());
                else 
                    return (null, new AskResponse(new PulsarClientException("Failed to subscribe to consumers")));
            }
			else
			{
				if (TopicsMap.TryGetValue(topicName, out var parts))
				{
					var errorMessage = $"[{Topic}] Failed to subscribe for topic [{topicName}] in topics consumer. Topic is already being subscribed for in other thread.";
					_log.Warning(errorMessage);
                    return (null, new AskResponse(new PulsarClientException(errorMessage)));
				}
				TopicsMap.Add(topicName, 1);
				++AllTopicPartitionsNumber;
				var consumerId = await _generator.Ask<long>(NewConsumerId.Instance);

                var newConsumer = await CreateConsumer(consumerId, topicName, _internalConfig, -1, schema, createIfDoesNotExist);
                if (!newConsumer.response.Failed)
                    _consumers.Add(topicName, newConsumer.consumer);
                else
                    return newConsumer;
			}

			if (AllTopicPartitionsNumber > MaxReceiverQueueSize)
			{
				MaxReceiverQueueSize = AllTopicPartitionsNumber;
			}
			var numTopics = TopicsMap.Values.Sum();
			var currentAllTopicsPartitionsNumber = AllTopicPartitionsNumber;
			if(currentAllTopicsPartitionsNumber != numTopics)
				return (null, new AskResponse(PulsarClientException.Unwrap(new ArgumentException("allTopicPartitionsNumber " + currentAllTopicsPartitionsNumber + " not equals expected: " + numTopics))));

			var recFromTops = new List<IActorRef>();
			foreach(var c in _consumers.Values)
            {
				var consumerTopicName = await c.Ask<string>(GetTopic.Instance);
				if (TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(TopicName.Get(topicName).PartitionedTopicName))
					recFromTops.Add(c);
			}
			StartReceivingMessages(recFromTops);
			_log.Info($"[{Topic}] [{Subscription}] Success subscribe new topic {topicName} in topics consumer, partitions: {numPartitions}, allTopicPartitionsNumber: {AllTopicPartitionsNumber}");

            return (null, new AskResponse());
			//HandleSubscribeOneTopicError(topicName, ex, subscribeResult);
		}

        private async ValueTask<(IActorRef consumer, AskResponse response)> CreateConsumer(long consumerId, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, ISchema<T> schema, bool createIfDoesNotExist)
        {
            IActorRef consumer = null;
			if (conf.ReceiverQueueSize == 0)
			{
				consumer = Context.ActorOf(Props.Create(() => new ZeroQueueConsumer<T>(consumerId, _stateActor, _client, _lookup, _cnxPool, _generator, topic, conf, Context.System.Scheduler.Advanced, partitionIndex, false, _startMessageId, schema, createIfDoesNotExist, _clientConfiguration)));
			}
            else
			    consumer = Context.ActorOf(Props.Create(() => new ConsumerActor<T>(consumerId, _stateActor, _client, _lookup, _cnxPool, _generator, topic, conf, Context.System.Scheduler.Advanced, partitionIndex, true, _startMessageId, _startMessageRollbackDurationInSec, schema, createIfDoesNotExist, _clientConfiguration)));
            
            var response = await consumer.Ask<AskResponse>(Connect.Instance).ConfigureAwait(false);
            return (consumer, response);
		}
		// handling failure during subscribe new topic, unsubscribe success created partitions
		private async ValueTask HandleSubscribeOneTopicError(string topicName, Exception error)
		{
			_log.Warning($"[{Topic}] Failed to subscribe for topic [{topicName}] in topics consumer {error}");
			var toCloseNum = 0;
			var recFromTops = new List<IActorRef>();
			foreach (var c in _consumers.Values)
			{
				var consumerTopicName = await c.Ask<string>(GetTopic.Instance);
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
		private void Unsubscribe(string topicName)
		{
			Condition.CheckArgument(TopicName.IsValid(topicName), "Invalid topic name:" + topicName);

			if(State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				Sender.Tell(new AskResponse(new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed")));
			}

			if(_partitionsAutoUpdateTimeout != null)
			{
				_partitionsAutoUpdateTimeout.Cancel();
				_partitionsAutoUpdateTimeout = null;
			}

			var topicPartName = TopicName.Get(topicName).PartitionedTopicName;

			var recFromTops = new List<IActorRef>();
			foreach (var c in _consumers.Values)
			{
				var consumerTopicName = c.Ask<string>(GetTopic.Instance).GetAwaiter().GetResult();
				if (TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(topicPartName))
				{
					recFromTops.Add(c);
				}
			}
			var unsubed = true;
			ClientExceptions except = null;
			foreach(var co in recFromTops)
            {
				var response = co.Ask(Messages.Consumer.Unsubscribe.Instance).GetAwaiter().GetResult();
				if (response == null)
				{
					var t = co.Ask<string>(GetTopic.Instance).GetAwaiter().GetResult();
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

			var topicPartName = TopicName.Get(topicName).PartitionedTopicName;


			var recFromTops = new List<IActorRef>();
			foreach (var c in _consumers.Values)
			{
				var consumerTopicName = await c.Ask<string>(GetTopic.Instance);
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

		internal override void Resume()
		{
			_paused = false;
			_consumers.ForEach(x => x.Value.Tell(Messages.Consumer.Resume.Instance));
		}

		internal override long LastDisconnectedTimestamp()
		{
			var lastDisconnectedTimestamp = -1L;
			foreach(var c in _consumers.Values)
            {
				var x = c.Ask<long>(GetLastDisconnectedTimestamp.Instance).GetAwaiter().GetResult();
				
				if (x > lastDisconnectedTimestamp)
					lastDisconnectedTimestamp = x;
			}
			return lastDisconnectedTimestamp;
		}

		// subscribe increased partitions for a given topic
		private async ValueTask SubscribeIncreasedTopicPartitions(string topic)
		{
			var topicName = TopicName.Get(topic);
			var metadata = await _lookup.Ask<PartitionedTopicMetadata>(new GetPartitionedTopicMetadata(topicName));
			var topics = GetPartitionsForTopic(topicName, metadata).ToList();
			var oldPartitionNumber = TopicsMap.GetValueOrNull(topic);
			var currentPartitionNumber = topics.Count;
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
				IList<string> newPartitions = topics.GetRange(oldPartitionNumber, currentPartitionNumber);
				foreach (var partitionName in newPartitions)
				{
					var consumerId = await _generator.Ask<long>(NewConsumerId.Instance);
					var partitionIndex = TopicName.GetPartitionIndex(partitionName);
					var configurationData = InternalConsumerConfig;
					var newConsumer = _context.ActorOf(Props.Create(() => new ConsumerActor<T>(consumerId, _stateActor, _client, _lookup, _cnxPool, _generator, partitionName, configurationData, Context.System.Scheduler.Advanced, partitionIndex, true, null, Schema, true, _clientConfiguration)));
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
		private async ValueTask<IMessageId> LastMessageId()
		{
			var multiMessageId = new Dictionary<string, IMessageId>();
			foreach (var v in _consumers.Values)
			{
				var t = await v.Ask<string>(GetTopic.Instance).ConfigureAwait(false);
				IMessageId messageId;
				try
				{
					messageId = await v.Ask<IMessageId>(GetLastMessageId.Instance).ConfigureAwait(false);
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

		private IList<string> GetPartitionsForTopic(TopicName topicName, PartitionedTopicMetadata metadata)
		{
			if (metadata.Partitions > 0)
			{
				IList<string> partitions = new List<string>(metadata.Partitions);
				for (var i = 0; i < metadata.Partitions; i++)
				{
					partitions.Add(topicName.GetPartition(i).ToString());
				}
				return partitions;
			}
			else
			{
				return new List<string> { topicName.ToString() };
			}
		}
		internal sealed class UpdatePartitionSub
        {
			public static UpdatePartitionSub Instance = new UpdatePartitionSub();
        }
	}

}