using Akka.Actor;
using Akka.Util.Internal;
using SharpPulsar.Batch;
using SharpPulsar.Batch.Api;
using SharpPulsar.Cache;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.Schema;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Schemas;
using SharpPulsar.Schemas.Generic;
using SharpPulsar.Stats.Consumer;
using SharpPulsar.Stats.Consumer.Api;
using SharpPulsar.Tracker;
using SharpPulsar.Tracker.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static SharpPulsar.Exceptions.PulsarClientException;
using static SharpPulsar.Protocol.Proto.CommandAck;
using InvalidMessageException = SharpPulsar.Exceptions.PulsarClientException.InvalidMessageException;
using PartitionedTopicMetadata = SharpPulsar.Common.Partition.PartitionedTopicMetadata;

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
        private readonly ConcurrentDictionary<string, IActorRef> _consumers;

        // Map <topic, numPartitions>, store partition number for each topic
        protected internal readonly Dictionary<string, int> PartitionedTopics;

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

        private readonly BatchMessageId _startMessageId = null;
        private readonly long _startMessageRollbackDurationInSec;
        private readonly ClientConfigurationData _clientConfiguration;
        private readonly Cache<string, ISchemaInfoProvider> _schemaProviderLoadingCache = new Cache<string, ISchemaInfoProvider>(TimeSpan.FromMinutes(30), 100000);

        private readonly IActorRef _client;

        private readonly IActorRef _self;
        private readonly IActorContext _context;
        public MultiTopicsConsumer(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ConsumerConfigurationData<T> conf, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture) : this(stateActor, client, lookup, cnxPool, idGenerator, DummyTopicNamePrefix + Utility.ConsumerName.GenerateRandomName(), conf, schema, createTopicIfDoesNotExist, clientConfiguration, subscribeFuture)
        {
        }
        public static Props Prop(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ConsumerConfigurationData<T> conf, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(() => new MultiTopicsConsumer<T>(stateActor, client, lookup, cnxPool, idGenerator, conf, schema, createTopicIfDoesNotExist, clientConfiguration, subscribeFuture));
        }
        public MultiTopicsConsumer(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ConsumerConfigurationData<T> conf, ISchema<T> schema, bool createTopicIfDoesNotExist, IMessageId startMessageId, long startMessageRollbackDurationInSec, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture) : this(stateActor, client, lookup, cnxPool, idGenerator, DummyTopicNamePrefix + Utility.ConsumerName.GenerateRandomName(), conf, schema, createTopicIfDoesNotExist, startMessageId, startMessageRollbackDurationInSec, clientConfiguration, subscribeFuture)
        {
        }
        public static Props Prop(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ConsumerConfigurationData<T> conf, ISchema<T> schema, bool createTopicIfDoesNotExist, IMessageId startMessageId, long startMessageRollbackDurationInSec, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(() => new MultiTopicsConsumer<T>(stateActor, client, lookup, cnxPool, idGenerator, conf, schema, createTopicIfDoesNotExist, startMessageId, startMessageRollbackDurationInSec, clientConfiguration, subscribeFuture));
        }
        public MultiTopicsConsumer(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string singleTopic, ConsumerConfigurationData<T> conf, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture) : this(stateActor, client, lookup, cnxPool, idGenerator, singleTopic, conf, schema, createTopicIfDoesNotExist, null, 0, clientConfiguration, subscribeFuture)
        {
        }
        public static Props Prop(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string singleTopic, ConsumerConfigurationData<T> conf, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(() => new MultiTopicsConsumer<T>(stateActor, client, lookup, cnxPool, idGenerator, singleTopic, conf, schema, createTopicIfDoesNotExist, clientConfiguration, subscribeFuture));
        }
        public MultiTopicsConsumer(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string singleTopic, ConsumerConfigurationData<T> conf, ISchema<T> schema, bool createTopicIfDoesNotExist, IMessageId startMessageId, long startMessageRollbackDurationInSec, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture) : base(stateActor, lookup, cnxPool, singleTopic, conf, Math.Max(2, conf.ReceiverQueueSize), schema, subscribeFuture)
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
            PartitionedTopics = new Dictionary<string, int>();
            _consumers = new ConcurrentDictionary<string, IActorRef>();
            _pausedConsumers = new Queue<IActorRef>();
            _sharedQueueResumeThreshold = MaxReceiverQueueSize / 2;
            AllTopicPartitionsNumber = 0;
            _startMessageId = startMessageId != null ? new BatchMessageId(MessageId.ConvertToMessageId(startMessageId)) : null;
            _startMessageRollbackDurationInSec = startMessageRollbackDurationInSec;

            if (conf.AckTimeout != TimeSpan.Zero)
            {
                if (conf.AckTimeoutRedeliveryBackoff != null)
                {
                    _unAckedMessageTracker = Context.ActorOf(UnAckedTopicMessageRedeliveryTracker<T>.Prop(Self, UnAckedChunckedMessageIdSequenceMap, conf), "UnAckedTopicMessageRedeliveryTracker");
                }
                else
                {
                    _unAckedMessageTracker = Context.ActorOf(UnAckedTopicMessageTracker<T>.Prop(UnAckedChunckedMessageIdSequenceMap, Self, conf), "UnAckedTopicMessageTracker");
                }
            }
            else
            {
                _unAckedMessageTracker = Context.ActorOf(UnAckedMessageTrackerDisabled.Prop(), "UnAckedMessageTrackerDisabled");
            }

            _internalConfig = InternalConsumerConfig;
            _stats = _clientConfiguration.StatsIntervalSeconds > TimeSpan.Zero ? new ConsumerStatsRecorder<T>(Context.System, conf, Topic, ConsumerName, Subscription, clientConfiguration.StatsIntervalSeconds) : ConsumerStatsDisabled.Instance;

            if (_internalConfig.AutoUpdatePartitions)
            {
                _partitionsAutoUpdateTimeout = _scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(60000), _internalConfig.AutoUpdatePartitionsInterval, Self, UpdatePartitionSub.Instance, ActorRefs.NoSender);
            }
            if (conf.TopicNames.Count == 0)
            {
                State.ConnectionState = HandlerState.State.Ready;
                subscribeFuture.TrySetResult(Self);
                return;
            }
            Condition.CheckArgument(conf.TopicNames.Count == 0 || TopicNamesValid(conf.TopicNames.ToList()), "Topics is empty or invalid.");
            Akka.Dispatch.ActorTaskScheduler.RunTask(async () =>
            {
                PulsarClientException lastError = null;
                foreach (var t in conf.TopicNames)
                {
                    try
                    {
                        await Subscribe(t, createTopicIfDoesNotExist);
                    }
                    catch (PulsarClientException ex)
                    {
                        Close();
                        _log.Warning($"[{Topic}] Failed to subscribe topics: {ex.Message}, closing consumer");
                        //log.error("[{}] Failed to unsubscribe after failed consumer creation: {}", topic, closeEx.getMessage());
                        subscribeFuture.TrySetException(ex);
                        lastError = ex;
                    }
                }
                if (lastError == null)
                {
                    if (AllTopicPartitionsNumber > MaxReceiverQueueSize)
                    {
                        MaxReceiverQueueSize = AllTopicPartitionsNumber;
                    }
                    State.ConnectionState = HandlerState.State.Ready;
                    StartReceivingMessages(_consumers.Values.ToList());
                    _log.Info($"[{Topic}] [{Subscription}] Created topics consumer with {AllTopicPartitionsNumber} sub-consumers");

                    subscribeFuture.TrySetResult(_self);
                }
            });

            Ready();
        }


        public static Props Prop(IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string singleTopic, ConsumerConfigurationData<T> conf, ISchema<T> schema, bool createTopicIfDoesNotExist, IMessageId startMessageId, long startMessageRollbackDurationInSec, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(() => new MultiTopicsConsumer<T>(stateActor, client, lookup, cnxPool, idGenerator, singleTopic, conf, schema, createTopicIfDoesNotExist, startMessageId, startMessageRollbackDurationInSec, clientConfiguration, subscribeFuture));
        }
        private void Ready()
        {
            Receive<SendState>(_ =>
            {
                StateActor.Tell(new SetConumerState(State.ConnectionState));
            });
            Receive<BatchReceive>(_ =>
            {
                try
                {
                    var message = BatchReceive();
                    Sender.Tell(new AskResponse(message));
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(ex));
                }
            });
            Receive<Messages.Consumer.Receive>(receive =>
            {
                try
                {
                    var message = receive.Time == TimeSpan.Zero ? Receive() : Receive(receive.Time);
                    Sender.Tell(new AskResponse(message));
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(ex));
                }
            });
            ReceiveAsync<UpdatePartitionSub>(async s =>
            {
                var tcs = SubscribeIncreasedTopicPartitions(Topic);
                await tcs.Task;
            });
            Receive<MessageProcessed<T>>(s =>
            {
                MessageProcessed(s.Message);
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
            //ZeroQueueConsumer hasParentConsumer
            Receive<ReceivedMessage<T>>(m => {
                ReceiveMessageFromConsumer(Sender, m.Message);
            });
            Receive<ClearUnAckedChunckedMessageIdSequenceMap>(_ => {
                UnAckedChunckedMessageIdSequenceMap.Tell(new Clear());
            });
            ReceiveAsync<HasReachedEndOfTopic>(async _ => {
                var hasReached = await HasReachedEndOfTopic();
                Sender.Tell(new AskResponse(hasReached));
            });
            Receive<GetAvailablePermits>(_ => {
                var permits = AvailablePermits();
                Sender.Tell(permits);
            });
            Receive<IsConnected>(_ => {
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
                try
                {
                    var has = await HasMessageAvailable();
                    Sender.Tell(new AskResponse(has));
                }
                catch (Exception ex)
                {

                    Sender.Tell(new AskResponse(ex));
                }
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
                    foreach (var message in m.Messages)
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
                switch (m)
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
                switch (message)
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
                        if (ack.Properties != null)
                            DoReconsumeLater(ack.Message, AckType.Cumulative, ack.Properties, ack.DelayTime);
                        else
                            DoReconsumeLater(ack.Message, AckType.Cumulative, new Dictionary<string, string>(), ack.DelayTime);
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
                    foreach (var message in m.Messages)
                        DoReconsumeLater(message, AckType.Individual, new Dictionary<string, string>(), m.DelayTime);

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
            Receive<Unsubscribe>(u =>
            {
                try
                {
                    Unsubscribe();
                    Sender.Tell(new AskResponse());
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(Unwrap(ex)));
                }
            });
            ReceiveAsync<UnsubscribeTopicName>(async u =>
            {
                try
                {
                    await Unsubscribe(u.TopicName);
                    Sender.Tell(new AskResponse());
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(Unwrap(ex)));
                }
            });
            ReceiveAsync<SeekMessageId>(async m =>
            {
                try
                {
                    await Seek(m.MessageId);
                    Sender.Tell(new AskResponse());
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
                }
            });
            ReceiveAsync<SeekTimestamp>(async m =>
            {
                try
                {
                    await Seek(m.Timestamp);
                    Sender.Tell(new AskResponse());
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
                }
            });
        }

        // Check topics are valid.
        // - each topic is valid,
        // - topic names are unique.
        private bool TopicNamesValid(List<string> topics)
        {
            if (topics == null && topics.Count == 0)
                throw new ArgumentException("topics should contain more than 1 topic");

            var result = topics.Where(topic => !TopicName.IsValid(topic)).FirstOrDefault();

            if (result != null && result.Count() > 0)
            {

                _log.Warning($"Received invalid topic name: {result[0]}");
                return false;
            }

            // check topic names are unique
            var set = new HashSet<string>(topics);
            if (set.Count == topics.Count)
            {
                return true;
            }
            else
            {
                _log.Warning($"Topic names not unique. unique/all : {set.Count}/{topics.Count}");
                return false;
            }
        }

        private void StartReceivingMessages(IList<IActorRef> newConsumers)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{Topic}] startReceivingMessages for {newConsumers.Count} new consumers in topics consumer, state: {State.ConnectionState}");
            }
            if (State.ConnectionState == HandlerState.State.Ready)
            {
                newConsumers.ForEach(consumer =>
                {
                    consumer.Tell(new IncreaseAvailablePermits(Conf.ReceiverQueueSize));

                    Akka.Dispatch.ActorTaskScheduler.RunTask(async () => await ReceiveMessageFromConsumer(consumer, true));
                });
            }
        }

        private async ValueTask ReceiveMessageFromConsumer(IActorRef consumer, bool batchReceive)
        {
            try
            {
                IList<IMessage<T>> messages;
                if (batchReceive)
                {
                    var msg = await consumer.Ask<AskResponse>(Messages.Consumer.BatchReceive.Instance);
                    messages = msg.ConvertTo<IMessages<T>>().MessageList();
                }
                else
                {
                    var msg = await consumer.Ask<AskResponse>(Messages.Consumer.Receive.Instance);
                    messages = new List<IMessage<T>> { msg.ConvertTo<IMessage<T>>() };
                }
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{Topic}] [{Subscription}] Receive message from sub consumer:{consumer.Path.Address}");
                }
                messages.ForEach(msg => MessageReceived(consumer, msg));
                var size = IncomingMessages.Count;
                var maxReceiverQueueSize = CurrentReceiverQueueSize;
                var sharedQueueResumeThreshold = maxReceiverQueueSize / 2;
                if (size >= maxReceiverQueueSize || (size > sharedQueueResumeThreshold && _pausedConsumers.Count() > 0))
                {
                    _pausedConsumers.Enqueue(consumer);
                    ResumeReceivingFromPausedConsumersIfNeeded();
                }
                else
                {
                    await ReceiveMessageFromConsumer(consumer, messages.Count > 0);
                }
            }
            catch (Exception ex)
            {
                if (ex is PulsarClientException.AlreadyClosedException || ex.InnerException is PulsarClientException.AlreadyClosedException)
                {
                    return;
                }
                _log.Error($"Receive operation failed on consumer {consumer.Path} - Retrying later: {ex}");
                Context.System.Scheduler.Advanced.ScheduleOnce(TimeSpan.FromSeconds(10), async () => await ReceiveMessageFromConsumer(consumer, true));
            }

        }
        private void ReceiveMessageFromConsumer(IActorRef consumer, IMessage<T> message)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{Topic}] [{Subscription}] Receive message from sub consumer");
            }
            MessageReceived(consumer, message);
            var size = IncomingMessages.Count;
            if (size >= MaxReceiverQueueSize || (size > _sharedQueueResumeThreshold && _pausedConsumers.Count > 0))
            {
                _pausedConsumers.Enqueue(consumer);
                consumer.Tell(Messages.Consumer.Pause.Instance);
            }
        }

        // Must be called from the internalPinnedExecutor thread
        private void MessageReceived(IActorRef consumer, IMessage<T> message)
        {
            var topic = consumer.Ask<string>(GetTopic.Instance).GetAwaiter().GetResult();
            var topicNameWithoutPartition = consumer.Ask<string>(GetTopicNameWithoutPartition.Instance).GetAwaiter().GetResult();
            Condition.CheckArgument(message is Message<T>);
            var topicMessage = new TopicMessage<T>(topic, topicNameWithoutPartition, message, consumer);

            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{Topic}][{Subscription}] Received message from topics-consumer {message.MessageId}");
            }

            // if asyncReceive is waiting : return message to callback without adding to incomingMessages queue
            var receivedFuture = NextPendingReceive();
            if (receivedFuture != null)
            {
                _unAckedMessageTracker.Tell(new Add<T>(topicMessage.MessageId, topicMessage.RedeliveryCount));
                CompletePendingReceive(receivedFuture, topicMessage);
            }
            else if (EnqueueMessageAndCheckBatchReceive(topicMessage) && HasPendingBatchReceive())
            {
                NotifyPendingBatchReceivedCallBack();
            }

            TryTriggerListener();
        }
        protected internal override void MessageProcessed(IMessage<T> msg)
        {
            _unAckedMessageTracker.Tell(new Add<T>(msg.MessageId, msg.RedeliveryCount));
            DecreaseIncomingMessageSize(msg);
        }
        private void ResumeReceivingFromPausedConsumersIfNeeded()
        {
            if (IncomingMessages.Count <= CurrentReceiverQueueSize / 2 && _pausedConsumers.Count > 0)
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
                        //consumer.Tell(Messages.Consumer.Resume.Instance);
                        Akka.Dispatch.ActorTaskScheduler.RunTask(async () => await ReceiveMessageFromConsumer(consumer, true));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }

        // If message consumer epoch is smaller than consumer epoch present that
        // it has been sent to the client before the user calls redeliverUnacknowledgedMessages, this message is invalid.
        // so we should release this message and receive again
        private bool IsValidConsumerEpoch(IMessage<T> message)
        {
            return base.IsValidConsumerEpoch(((Message<T>)(((TopicMessage<T>)message)).Message));
        }

        public override int MinReceiverQueueSize()
        {
            var size = Math.Min(InitialReceiverQueueSize, MaxReceiverQueueSize);
            if (BatchReceivePolicy.MaxNumMessages > 0)
            {
                size = Math.Max(size, BatchReceivePolicy.MaxNumMessages);
            }
            if (AllTopicPartitionsNumber > 0)
            {
                size = Math.Max(AllTopicPartitionsNumber, size);
            }
            return size;
        }

        protected internal override IMessage<T> InternalReceive()
        {
            IMessage<T> message;
            try
            {
                if (IncomingMessages.Count == 0)
                {
                    ExpectMoreIncomingMessages();
                }
                message = IncomingMessages.Receive();
                DecreaseIncomingMessageSize(message);
                //checkState(Message is TopicMessageImpl);
                if (!IsValidConsumerEpoch(message))
                {
                    ResumeReceivingFromPausedConsumersIfNeeded();
                    message = null;
                    return InternalReceive();
                }
                _unAckedMessageTracker.Tell(new Add<T>(message.MessageId, message.RedeliveryCount));
                ResumeReceivingFromPausedConsumersIfNeeded();
                return message;
            }
            catch (Exception e)
            {
                throw PulsarClientException.Unwrap(e);
            }
        }

        protected internal override IMessage<T> InternalReceive(TimeSpan time)
        {
            IMessage<T> message;

            var callTime = NanoTime();
            try
            {
                if (IncomingMessages.Count == 0)
                {
                    ExpectMoreIncomingMessages();
                }
                IncomingMessages.TryReceive(out message);
                if (message != null)
                {
                    DecreaseIncomingMessageSize(message);
                    //checkArgument(Message is TopicMessageImpl);
                    if (!IsValidConsumerEpoch(message))
                    {
                        var executionTime = NanoTime() - callTime;
                        var timeoutInNanos = time.TotalMilliseconds;
                        if (executionTime >= timeoutInNanos)
                        {
                            return null;
                        }
                        else
                        {
                            ResumeReceivingFromPausedConsumersIfNeeded();
                            return InternalReceive(TimeSpan.FromMilliseconds(timeoutInNanos - executionTime));
                        }
                    }
                    _unAckedMessageTracker.Tell(new Add<T>(message.MessageId, message.RedeliveryCount));
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
                return InternalBatchReceiveAsync().Task.GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (State.ConnectionState != HandlerState.State.Closing && State.ConnectionState != HandlerState.State.Closed)
                {
                    Stats.IncrementNumBatchReceiveFailed();
                    throw Unwrap(e);
                }
                else
                {
                    return null;
                }
            }
        }

        protected internal override TaskCompletionSource<IMessages<T>> InternalBatchReceiveAsync()
        {
            var result = new TaskCompletionSource<IMessages<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
            Akka.Dispatch.ActorTaskScheduler.RunTask(() =>
            {
                if (HasEnoughMessagesForBatchReceive())
                {
                    var messages = NewMessages;
                    IncomingMessages.TryReceive(out var msg);
                    while (msg != null && messages.CanAdd(msg))
                    {
                        if (msg != null)
                        {
                            DecreaseIncomingMessageSize(msg);
                            if (!IsValidConsumerEpoch(msg))
                            {
                                IncomingMessages.TryReceive(out msg);
                                continue;
                            }
                            var interceptMsg = BeforeConsume(msg);
                            messages.Add(interceptMsg);
                        }
                    }
                    result.SetResult(messages);
                }
                else
                {
                    ExpectMoreIncomingMessages();
                    var opBatchReceive = OpBatchReceive.Of(result);
                    PendingBatchReceives.Enqueue(opBatchReceive);
                    TriggerBatchReceiveTimeoutTask();
                    result.Task.ContinueWith(s =>
                    {
                        if (s.IsCanceled)
                            PendingBatchReceives.TakeLast(1);
                    });
                }
                ResumeReceivingFromPausedConsumersIfNeeded();
            });
            return result;
        }
        protected internal override TaskCompletionSource<IMessage<T>> InternalReceiveAsync()
        {
            var result = new TaskCompletionSource<IMessage<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
            Akka.Dispatch.ActorTaskScheduler.RunTask(() =>
            {
                IncomingMessages.TryReceive(out var message);
                if (message == null)
                {
                    ExpectMoreIncomingMessages();
                    PendingReceives.Enqueue(result);
                    result.Task.ContinueWith(s =>
                    {
                        if (s.IsCanceled)
                            PendingReceives.TryDequeue(out result);
                    });
                }
                else
                {
                    DecreaseIncomingMessageSize(message);
                    //CheckState(Message is TopicMessageImpl);
                    _unAckedMessageTracker.Tell(new Add<T>(message.MessageId, message.RedeliveryCount));
                    ResumeReceivingFromPausedConsumersIfNeeded();
                    result.SetResult(message);
                }
            });

            return result;
        }

        // subscribe one more given topic
        protected internal async ValueTask Subscribe(string topicName, bool createTopicIfDoesNotExist)
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var topicNameInstance = GetTopicName(topicName);
            if (topicNameInstance == null)
            {
                var ex = new PulsarClientException.AlreadyClosedException("Topic name not valid");
                _log.Error($"{ex}");
                tcs.TrySetException(ex);
                await tcs.Task;
                return;
            }
            var fullTopicName = topicNameInstance.ToString();
            if (PartitionedTopics.ContainsKey(fullTopicName) || PartitionedTopics.ContainsKey(topicNameInstance.PartitionedTopicName))
            {
                var ex = new PulsarClientException.AlreadyClosedException("Already subscribed to" + topicName);
                _log.Error($"{ex}");
                tcs.TrySetException(ex);
                await tcs.Task;
                return;

            }

            if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
            {
                var ex = new PulsarClientException.AlreadyClosedException("Topics Consumer was already closed");
                _log.Error($"{ex}");
                tcs.TrySetException(ex);
                await tcs.Task;
                return;
            }

            var result = await _lookup.Ask<AskResponse>(new GetPartitionedTopicMetadata(TopicName.Get(topicName)));
            if (result.Failed)
            {
                var error = $"[{fullTopicName}] Failed to get partitioned topic metadata: {result.Exception}";
                _log.Warning(error);
                tcs.TrySetException(result.Exception);
                await tcs.Task;
                return;
            }

            var metadata = result.ConvertTo<PartitionedTopicMetadata>();
            await SubscribeTopicPartitions(tcs, fullTopicName, metadata.Partitions, createTopicIfDoesNotExist);

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
                    throw;
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

        protected override void Unhandled(object message)
        {
            _log.Warning($"Unhandled Message '{message.GetType().FullName}' from '{Sender.Path}'");
        }


        protected internal override void DoAcknowledge(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txnImpl)
        {
            Condition.CheckArgument(messageId is TopicMessageId);
            var topicMessageId = (TopicMessageId)messageId;

            if (State.ConnectionState != HandlerState.State.Ready)
            {
                Sender.Tell(new AskResponse(new PulsarClientException("Consumer already closed")));
            }

            if (ackType == AckType.Cumulative)
            {
                var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
                if (consumer != null)
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

        protected internal override void DoAcknowledge(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        {
            if (ackType == AckType.Cumulative)
            {
                messageIdList.ForEach(messageId => DoAcknowledge(messageId, ackType, properties, txn));
            }
            else
            {
                if (State.ConnectionState != HandlerState.State.Ready)
                {
                    throw new PulsarClientException("Consumer already closed");
                }
                IDictionary<string, IList<IMessageId>> topicToMessageIdMap = new Dictionary<string, IList<IMessageId>>();
                foreach (var messageId in messageIdList)
                {
                    if (!(messageId is TopicMessageId))
                    {
                        throw new ArgumentException("messageId is not instance of TopicMessageIdImpl");
                    }
                    var topicMessageId = (TopicMessageId)messageId;
                    if (!topicToMessageIdMap.ContainsKey(topicMessageId.TopicPartitionName))
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

        protected internal override TaskCompletionSource<object> DoReconsumeLater(IMessage<T> message, AckType ackType, IDictionary<string, string> properties, TimeSpan delayTime)
        {
            var messageId = message.MessageId;
            Condition.CheckArgument(messageId is TopicMessageId);
            var topicMessageId = (TopicMessageId)messageId;
            if (State.ConnectionState != HandlerState.State.Ready)
            {
                throw new PulsarClientException("Consumer already closed");

            }

            if (ackType == AckType.Cumulative)
            {
                var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
                if (consumer != null)
                {
                    consumer.Tell(new ReconsumeLaterCumulative<T>(message, delayTime), Sender);
                }
                else
                {
                    throw new PulsarClientException.NotConnectedException();
                }
            }
            else
            {
                var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
                consumer.Tell(new ReconsumeLaterMessage<T>(message, delayTime), Sender);
                _unAckedMessageTracker.Tell(new Remove(topicMessageId));
            }
            return null;
        }

        internal override void NegativeAcknowledge(IMessageId messageId)
        {
            Condition.CheckArgument(messageId is TopicMessageId);
            var topicMessageId = (TopicMessageId)messageId;

            var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
            consumer.Tell(new NegativeAcknowledgeMessageId(topicMessageId.InnerMessageId), Sender);
        }
        protected internal new void NegativeAcknowledge(IMessage<T> message)
        {
            var messageId = message.MessageId;
            Condition.CheckArgument(messageId is TopicMessageId);
            var topicMessageId = (TopicMessageId)messageId;

            var consumer = _consumers.GetValueOrNull(topicMessageId.TopicPartitionName);
            consumer.Tell(new NegativeAcknowledgeMessageId(topicMessageId.InnerMessageId), Sender);
        }
        protected override void PostStop()
        {
            Close();
            base.PostStop();
        }
        internal override void Unsubscribe()
        {
            if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
            {
                throw new AlreadyClosedException("AlreadyClosedException: Consumer was already closed");
            }

            State.ConnectionState = HandlerState.State.Closing;
            var futureList = _consumers.Values.Select(async consumer => await consumer.Ask<AskResponse>(Messages.Consumer.Unsubscribe.Instance)).ToList();
            try
            {
                // Wait for all the tasks to finish.
                Task.WaitAll(futureList.ToArray());

                State.ConnectionState = HandlerState.State.Closed;
                CleanupMultiConsumer();
                _log.Error($"[{Topic}] [{Subscription}] [{ConsumerName}] Could not unsubscribe Topics Consumer");
                FailPendingReceive();
            }
            catch (Exception e)
            {
                State.ConnectionState = HandlerState.State.Failed;
                _log.Error($"[{Topic}] [{Subscription}] [{ConsumerName}] Could not unsubscribe Topics Consumer: {e.InnerException}");
                throw e.InnerException;
            }
        }
        // un-subscribe a given topic
        private async ValueTask Unsubscribe(string topicName)
        {
            Condition.CheckArgument(TopicName.IsValid(topicName), "Invalid topic name:" + topicName);

            if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
            {
                throw new AlreadyClosedException("Topics Consumer was already closed");
            }

            if (_partitionsAutoUpdateTimeout != null)
            {
                _partitionsAutoUpdateTimeout.Cancel();
                _partitionsAutoUpdateTimeout = null;
            }

            var topicPartName = TopicName.Get(topicName).PartitionedTopicName;

            var consumersToUnsub = new List<IActorRef>();
            foreach (var c in _consumers.Values)
            {
                var consumerTopicName = await c.Ask<string>(GetTopic.Instance);
                if (TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(topicPartName))
                    consumersToUnsub.Add(c);
            }
            var futureList = consumersToUnsub.Select(async consumer => await consumer.Ask<AskResponse>(Messages.Consumer.Unsubscribe.Instance)).ToList();
            try
            {
                Task.WaitAll(futureList.ToArray());
                foreach (var consumer in consumersToUnsub)
                {
                    var response = await consumer.GracefulStop(TimeSpan.FromSeconds(1));
                    var t = await consumer.Ask<string>(GetTopic.Instance);
                    _consumers.TryRemove(t, out _);
                    _pausedConsumers = new Queue<IActorRef>(_pausedConsumers.Where(x => x != consumer));
                    --AllTopicPartitionsNumber;
                }
                RemoveTopic(topicName);
                if (_unAckedMessageTracker.Path.Name == "UnAckedTopicMessageTracker")
                    _unAckedMessageTracker.Tell(new RemoveTopicMessages(topicName));

                _log.Info($"[{topicName}] [{Subscription}] [{ConsumerName}] Unsubscribed Topics Consumer, allTopicPartitionsNumber: {AllTopicPartitionsNumber}");
            }
            catch (Exception ex)
            {
                State.ConnectionState = HandlerState.State.Failed;
                _log.Error($"[{topicName}] [{Subscription}] [{ConsumerName}] Could not unsubscribe Topics Consumer: {ex}");
                throw;   
            }
        }
    
        private void CleanupMultiConsumer()
        {
            if (_unAckedMessageTracker != null)
            {
                _unAckedMessageTracker.GracefulStop(TimeSpan.FromMilliseconds(500));
            }
            if (_partitionsAutoUpdateTimeout != null)
            {
                _partitionsAutoUpdateTimeout.Cancel();
                _partitionsAutoUpdateTimeout = null;
            }
            _client.Tell(new CleanupConsumer(Context.Self));
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

        protected internal override void RedeliverUnacknowledgedMessages()
        {
            Akka.Dispatch.ActorTaskScheduler.RunTask(()=>
            {
                ConsumerEpoch++;
                _consumers.Values.ForEach(consumer =>
                {
                    consumer.Tell(Messages.Consumer.RedeliverUnacknowledgedMessages.Instance);
                    consumer.Tell(ClearUnAckedChunckedMessageIdSequenceMap.Instance);
                });
                ClearIncomingMessages();
                _unAckedMessageTracker.Tell(Clear.Instance);
            });

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
        protected internal override void UpdateAutoScaleReceiverQueueHint()
        {
            ScaleReceiverQueueHint.GetAndSet(IncomingMessages.Count >= CurrentReceiverQueueSize);
        }

        protected internal override void CompleteOpBatchReceive(OpBatchReceive op)
        {
            NotifyPendingBatchReceivedCallBack(op);
            ResumeReceivingFromPausedConsumersIfNeeded();
        }
        internal override async ValueTask Seek(IMessageId messageId)
		{
            var targetMessageId = MessageId.ConvertToMessageId(messageId);
            if (targetMessageId == null || IsIllegalMultiTopicsMessageId(messageId))
            {
                throw new PulsarClientException("Illegal messageId, messageId can only be earliest/latest");
            }
            _consumers.Values.ForEach(c => c.Tell(new SeekMessageId(targetMessageId)));

            _unAckedMessageTracker.Tell(Clear.Instance);
            ClearIncomingMessages();
            await Task.CompletedTask;
		}

		internal override async ValueTask Seek(long timestamp)
		{
            _consumers.Values.ForEach(c => c.Tell(new SeekTimestamp(timestamp)));
            await Task.CompletedTask;
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
            if (NumMessagesInQueue() > 0)
            {
                return true;
            }
            foreach (var c in _consumers.Values)
			{
                try
                {
                    var response = await c.Ask<AskResponse>(Messages.Consumer.HasMessageAvailable.Instance).ConfigureAwait(false);
                    if (response.Failed)
                        return false;

                    return response.ConvertTo<bool>();
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
                if (!messageIds.Contains(peek.MessageId))
                {
                    // first message is not expired, then no message is expired in queue.
                    return;
                }

                // try not to remove elements that are added while we remove
                var message = peek;
                if (!(message is TopicMessage<T>))
                    throw new InvalidMessageException(message.GetType().FullName);

                if (IncomingMessages.TryReceiveAll(out var messageList))
                {
                    foreach (var m in messageList)
                    {
                        DecreaseIncomingMessageSize(m);
                        var messageId = m.MessageId;
                        if (!messageIds.Contains(messageId))
                        {
                            messageIds.Add(messageId);
                            break;
                        }
                    }
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
				PartitionedTopics.Remove(topic);
			}
		}

		private async ValueTask SubscribeTopicPartitions(TaskCompletionSource<object> subscribeResult,  string topicName, int numPartitions, bool createIfDoesNotExist)
		{
            var schemaClone = await PreProcessSchemaBeforeSubscribe(Schema, topicName); 
            await DoSubscribeTopicPartitions(schemaClone, subscribeResult, topicName, numPartitions, createIfDoesNotExist);
        }

        private async ValueTask DoSubscribeTopicPartitions(ISchema<T> schema, TaskCompletionSource<object> subscribeResult, string topicName, int numPartitions, bool createIfDoesNotExist)
		{
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Subscribe to topic {topicName} metadata.partitions: {numPartitions}");
			}
            var futureList = new List<Task>();
            PulsarClientException lastError = null;
            if (numPartitions != PartitionedTopicMetadata.NonPartitioned)
			{
				// Below condition is true if subscribeAsync() has been invoked second time with same
				// topicName before the first invocation had reached this point.
				if(PartitionedTopics.TryGetValue(topicName, out var parts))
				{
					var errorMessage = $"[{Topic}] Failed to subscribe for topic [{topicName}] in topics consumer. Topic is already being subscribed for in other thread.";
					_log.Warning(errorMessage);
					subscribeResult.TrySetException(new PulsarClientException(errorMessage));
                    return;
				}
				//PartitionedTopics.Add(topicName, numPartitions);
				AllTopicPartitionsNumber += numPartitions;

				var receiverQueueSize = Math.Min(Conf.ReceiverQueueSize, Conf.MaxTotalReceiverQueueSizeAcrossPartitions / numPartitions);
				var configurationData = InternalConsumerConfig;
				configurationData.ReceiverQueueSize = receiverQueueSize;
				for(var i = 0; i < numPartitions; i++)
                {
                    try
                    {
                        var consumerId = await _generator.Ask<long>(NewConsumerId.Instance);
                        var partitionName = TopicName.Get(topicName).GetPartition(i).ToString();
                        var newConsumer = await CreateInternalConsumer(consumerId, partitionName, configurationData, i, schema, createIfDoesNotExist).Task;
                        if (_paused)
                        {
                            newConsumer.Tell(Messages.Consumer.Pause.Instance);
                        }
                        _consumers.TryAdd(partitionName, newConsumer);
                    }
                    catch(PulsarClientException ex)
                    {
                        lastError = ex;
                    }
                }
            }
			else
			{				
				//PartitionedTopics.Add(topicName, 1);
				++AllTopicPartitionsNumber;
                if (PartitionedTopics.TryGetValue(topicName, out var parts))
                {
                    var errorMessage = $"[{Topic}] Failed to subscribe for topic [{topicName}] in topics consumer. Topic is already being subscribed for in other thread.";
                    _log.Warning(errorMessage);
                    subscribeResult.TrySetException(new PulsarClientException(errorMessage));
                    return;
                }
                try
                {
                    var consumerId = await _generator.Ask<long>(NewConsumerId.Instance);
                    var newConsumer = await CreateInternalConsumer(consumerId, topicName, _internalConfig, -1, schema, createIfDoesNotExist).Task;
                    _consumers.TryAdd(topicName, newConsumer);
                }
                catch (PulsarClientException ex)
                {
                    lastError = ex;
                }
            }
            if (lastError != null)
            {
                await HandleSubscribeOneTopicError(topicName, lastError, subscribeResult);
                return;
            }
            if (AllTopicPartitionsNumber > MaxReceiverQueueSize)
            {
                MaxReceiverQueueSize = AllTopicPartitionsNumber;
            }

            // We have successfully created new consumers, so we can start receiving messages for them
            var recFromTops = new List<IActorRef>();
            foreach (var c in _consumers.Values)
            {
                var consumerTopicName = await c.Ask<string>(GetTopic.Instance);
                if (TopicName.Get(consumerTopicName).PartitionedTopicName.Equals(TopicName.Get(topicName).PartitionedTopicName))
                    recFromTops.Add(c);
            }
            StartReceivingMessages(recFromTops);
            subscribeResult.TrySetResult(null);
        }

        private TaskCompletionSource<IActorRef> CreateInternalConsumer(long consumerId, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, ISchema<T> schema, bool createIfDoesNotExist)
        {
            var tcs = new TaskCompletionSource<IActorRef>(TaskCreationOptions.RunContinuationsAsynchronously);
            var internalBatchReceivePolicy = new BatchReceivePolicy.Builder().MaxNumMessages(Math.Max(conf.ReceiverQueueSize / 2, 1)).MaxNumBytes(-1).Timeout((int)TimeSpan.FromSeconds(1).TotalMilliseconds).Build();
            conf.BatchReceivePolicy = internalBatchReceivePolicy;
            
			_context.ActorOf(ConsumerActor<T>.Prop(consumerId, _stateActor, _client, _lookup, _cnxPool, _generator, topic, conf, partitionIndex, true, Listener != null, _startMessageId, _startMessageRollbackDurationInSec, schema, createIfDoesNotExist, _clientConfiguration, tcs));
            
            return tcs;
		}
		// handling failure during subscribe new topic, unsubscribe success created partitions
		private async ValueTask HandleSubscribeOneTopicError(string topicName, Exception error, TaskCompletionSource<object> subscribeResult)
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
				_consumers.TryRemove(topic, out _);
				if (--toCloseNum == 0)
				{
					_log.Warning($"[{Topic}] Failed to subscribe for topic [{topicName}] in topics consumer, subscribe error: {error}");
					RemoveTopic(topicName);
                    subscribeResult.TrySetException(error);
				}
			}
		}
        
        // Remove a consumer for a topic
        private async ValueTask RemoveConsumer(string topicName)
		{
			Condition.CheckArgument(TopicName.IsValid(topicName), "Invalid topic name:" + topicName);

			if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				throw new AlreadyClosedException("Topics Consumer was already closed");
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
				_consumers.TryRemove(t, out _);
				_pausedConsumers = new Queue<IActorRef>(_pausedConsumers.Where(x => x != co));
				--AllTopicPartitionsNumber;
			}
			RemoveTopic(topicName);

            if(_unAckedMessageTracker.Path.Name == "UnAckedTopicMessageTracker")
			     _unAckedMessageTracker.Tell(new RemoveTopicMessages(topicName));

			_log.Info($"[{topicName}] [{Subscription}] [{ConsumerName}] Removed Topics Consumer, allTopicPartitionsNumber: {AllTopicPartitionsNumber}");

		}
        
        // get topics name
        internal virtual IList<string> Topics
		{
			get
			{
				return PartitionedTopics.Keys.ToList();
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
		private TaskCompletionSource<IActorRef> SubscribeIncreasedTopicPartitions(string topic)
		{
            var tcs = new TaskCompletionSource<IActorRef>(TaskCreationOptions.RunContinuationsAsynchronously);
            var oldPartitionNumber = PartitionedTopics.GetValueOrNull(topic);
            var topicName = TopicName.Get(topic);
            _lookup.Ask<AskResponse>(new GetPartitionedTopicMetadata(topicName))
                .ContinueWith( async task => 
                {
                    var result = task.Result;


                    if ( result.Failed)
                    {
                        tcs.TrySetException(result.Exception);
                        return;
                    }

                    var metadata = result.ConvertTo<PartitionedTopicMetadata>();
                    var topics = GetPartitionsForTopic(topicName, metadata).ToList();
                    var currentPartitionNumber = topics.Where(topic => TopicName.Get(topic).Partitioned).ToList().Count;
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{topicName}] partitions number. old: {oldPartitionNumber}, new: {currentPartitionNumber}");
                    }
                    if (oldPartitionNumber == currentPartitionNumber)
                    {
                        tcs.TrySetResult(null);
                        // topic partition number not changed
                        return;
                    }
                    else if (currentPartitionNumber == PartitionedTopicMetadata.NonPartitioned)
                    {
                        // The topic was initially partitioned but then it was deleted. We keep it in the topics
                        PartitionedTopics.Add(topic, 0);

                        AllTopicPartitionsNumber -= oldPartitionNumber;
                        var futures = new List<Task>();
                        foreach (var it in _consumers)
                        {
                            var e = it;
                            var PartitionedTopicName = TopicName.Get(e.Key).PartitionedTopicName;
                            if (PartitionedTopicName.Equals(topicName))
                            {
                                futures.Add(e.Value.GracefulStop(TimeSpan.FromSeconds(5)));
                                _consumers.TryRemove(e.Key, out _);
                            }
                        }
                        await Task.WhenAll(futures.ToArray()).ContinueWith(_=> tcs.TrySetResult(null));
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
                            var consumerTcs = new TaskCompletionSource<IActorRef>(TaskCreationOptions.RunContinuationsAsynchronously);
                            try
                            {
                                _context.ActorOf(Props.Create(() => new ConsumerActor<T>(consumerId, _stateActor, _client, _lookup, _cnxPool, _generator, partitionName, configurationData, partitionIndex, true, Listener != null, null, Schema, true, _clientConfiguration, consumerTcs)));
                                var newConsumer = await consumerTcs.Task;
                                if (_paused)
                                {
                                    newConsumer.Tell(Messages.Consumer.Pause.Instance);
                                }
                                _consumers.TryAdd(partitionName, newConsumer);

                                if (_log.IsDebugEnabled)
                                {
                                    _log.Debug($"[{topicName}] create consumer {Topic} for partitionName: {partitionName}");
                                }
                            }
                            catch (Exception ex)
                            {
                                _log.Warning($"[{Topic}] Failed to subscribe {topicName} partition: {oldPartitionNumber} - {currentPartitionNumber} : {ex}");
                            }
                            tcs.TrySetResult(null);
                        }
                        var newConsumerList = newPartitions.Select(partitionTopic => _consumers.GetValueOrNull(partitionTopic)).ToList();
                        StartReceivingMessages(newConsumerList);
                        //_log.warn("[{}] Failed to subscribe {} partition: {} - {} : {}", Topic, topicName, oldPartitionNumber, currentPartitionNumber, ex);
                    }
                    else
                    {
                        _log.Error($"[{topicName}] not support shrink topic partitions. old: {oldPartitionNumber}, new: {currentPartitionNumber}");
                        tcs.TrySetException(new PulsarClientException.NotSupportedException("not support shrink topic partitions"));
                    }

                });
            return tcs;
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