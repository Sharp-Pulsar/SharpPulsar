using Akka.Actor;
using Akka.Util;
using ProtoBuf;
using SharpPulsar.Auth;
using SharpPulsar.Batch;
using SharpPulsar.Builder;
using SharpPulsar.Client;
using SharpPulsar.Common.Compression;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Crypto;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Shared;
using SharpPulsar.Stats.Consumer;
using SharpPulsar.Stats.Consumer.Api;
using SharpPulsar.Tracker;
using SharpPulsar.Tracker.Messages;
using SharpPulsar.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static SharpPulsar.Exceptions.PulsarClientException;
using static SharpPulsar.Protocol.Proto.CommandAck;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
using ConsumerCryptoFailureAction = SharpPulsar.Common.Compression.ConsumerCryptoFailureAction;
using DeadLetterPolicy = SharpPulsar.Common.Compression.DeadLetterPolicy;
using SubscriptionInitialPosition = SharpPulsar.Common.SubscriptionInitialPosition;

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
namespace SharpPulsar.Consumer
{

    internal class ConsumerActor<T> : ConsumerActorBase<T>
    {
        private const int MaxRedeliverUnacknowledged = 1000;

        private readonly long _consumerId;
        private long _prevconsumerId;

        // Number of messages that have delivered to the application. Every once in a while, this number will be sent to the
        // broker to notify that we are ready to get (and store in the incoming messages queue) more messages

        private int _availablePermits = 0;

        protected IMessageId _lastDequeuedMessageId = IMessageId.Earliest;
        private IMessageId _lastMessageIdInBroker = IMessageId.Earliest;
        private readonly ClientConfigurationData _clientConfigurationData;
        private readonly long _subscribeTimeout;
        private readonly int _partitionIndex;
        private readonly bool _parentConsumerHasListener;
        private readonly int _receiverQueueRefillThreshold;

        private readonly IActorRef _unAckedMessageTracker;
        private readonly IActorRef _acknowledgmentsGroupingTracker;
        private readonly IActorRef _negativeAcksTracker;
        private readonly CancellationTokenSource _tokenSource;
        private readonly int _priorityLevel;
        private readonly SubscriptionMode _subscriptionMode;
        private BatchMessageId _startMessageId;
        private readonly IActorContext _context;
        private readonly Collection<Exception> _previousExceptions = new Collection<Exception>();
        private Queue<(IActorRef, Messages.Consumer.Receive)> _receives = new Queue<(IActorRef, Messages.Consumer.Receive)>();
        private Queue<(IActorRef, BatchReceive)> _batchReceives = new Queue<(IActorRef, BatchReceive)>();

        private readonly ICancelable _receiveRun;
        private readonly ICancelable _batchRun;
        private readonly IActorRef _lookup;
        private readonly IActorRef _cnxPool;
        private BatchMessageId _seekMessageId;
        private bool _duringSeek;
        private readonly BatchMessageId _initialStartMessageId;

        private readonly long _startMessageRollbackDurationInSec;
        private readonly IActorRef _client;

        private readonly IConsumerStatsRecorder _stats;

        private volatile bool _hasReachedEndOfTopic;

        private readonly IMessageCrypto _msgCrypto;

        private readonly ImmutableDictionary<string, string> _metadata;

        private readonly bool _readCompacted;
        private readonly bool _resetIncludeHead;
        //private readonly bool _poolMessages = false;


        private readonly ActorSystem _actorSystem;

        private readonly SubscriptionInitialPosition _subscriptionInitialPosition;
        private readonly IActorRef _connectionHandler;
        private readonly IActorRef _generator;

        private readonly Dictionary<long, (List<IMessageId> messageid, TransactionImpl.TxnID txnid)> _ackRequests;

        private readonly TopicName _topicName;
        private readonly string _topicNameWithoutPartition;

        private readonly IDictionary<IMessageId, IList<IMessage<T>>> _possibleSendToDeadLetterTopicMessages;

        private readonly DeadLetterPolicy _deadLetterPolicy;

        private Producer<byte[]> _deadLetterProducer;

        private int _maxMessageSize;
        private int _protocolVersion;

        private Producer<T> _retryLetterProducer;
        private IActorRef _replyTo;

        private long _subscribeDeadline = 0; // gets set on first successful connection

        protected internal bool Paused;
        private int _pendingChunkedMessageCount = 0;
        private readonly Dictionary<string, ChunkedMessageCtx> _chunkedMessagesMap = new Dictionary<string, ChunkedMessageCtx>();
        protected internal TimeSpan ExpireTimeOfIncompleteChunkedMessage = TimeSpan.Zero;
        private bool _expireChunkMessageTaskScheduled = false;
        private readonly int _maxPendingChuckedMessage;
        // if queue size is reasonable (most of the time equal to number of producers try to publish messages concurrently on
        // the topic) then it guards against broken chuncked message which was not fully published
        private readonly bool _autoAckOldestChunkedMessageOnQueueFull;
        // it will be used to manage N outstanding chunked mesage buffers
        private Queue<string> _pendingChunckedMessageUuidQueue;

        private readonly bool _createTopicIfDoesNotExist;
        protected IActorRef _self;
        protected IActorRef _clientCnx;

        private IActorRef _clientCnxUsedForConsumerRegistration;
        private readonly Dictionary<string, long> _properties = new Dictionary<string, long>();


        public ConsumerActor(long consumerId, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, bool parentConsumerHasListener, IMessageId startMessageId, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfigurationData, TaskCompletionSource<IActorRef> subscribeFuture) : this
            (consumerId, stateActor, client, lookup, cnxPool, idGenerator, topic, conf, partitionIndex, hasParentConsumer, parentConsumerHasListener, startMessageId, 0, schema, createTopicIfDoesNotExist, clientConfigurationData, subscribeFuture)
        {
        }

        public ConsumerActor(long consumerId, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, bool parentConsumerHasListener, IMessageId startMessageId, long startMessageRollbackDurationInSec, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture) : base(stateActor, lookup, cnxPool, topic, conf, conf.ReceiverQueueSize, schema, subscribeFuture)
        {
            Self.Path.WithUid(consumerId);
            Paused = conf.StartPaused;
            _context = Context;
            _clientConfigurationData = clientConfiguration;
            _ackRequests = new Dictionary<long, (List<IMessageId> messageid, TransactionImpl.TxnID txnid)>();
            _generator = idGenerator;
            _topicName = TopicName.Get(topic);
            _cnxPool = cnxPool;
            _actorSystem = Context.System;
            _parentConsumerHasListener = parentConsumerHasListener;
            _lookup = lookup;
            _self = Self;
            _tokenSource = new CancellationTokenSource();
            _client = client;
            _consumerId = consumerId;
            if (!_topicName.Persistent && conf.SubscriptionMode.Equals(SubscriptionMode.Durable))
            {
                conf.SubscriptionMode = SubscriptionMode.NonDurable;
                _log.Warning($"[{topic}] Cannot create a [Durable] subscription for a NonPersistentTopic, "
                        + $"will use [NonDurable] to subscribe. Subscription name: {conf.SubscriptionName}");
            }
            _subscriptionMode = conf.SubscriptionMode;
            if (startMessageId != null)
            {
                if (startMessageId is ChunkMessageId)
                {
                    _startMessageId = new BatchMessageId(((ChunkMessageId)startMessageId).FirstChunkMessageId);
                }
                else
                {
                    _startMessageId = new BatchMessageId((MessageId)startMessageId);
                }
            }
            _initialStartMessageId = _startMessageId;
            _startMessageRollbackDurationInSec = startMessageRollbackDurationInSec;
            _availablePermits = 0;
            _subscribeTimeout = DateTimeHelper.CurrentUnixTimeMillis() + (long)clientConfiguration.OperationTimeout.TotalMilliseconds;
            _partitionIndex = partitionIndex;
            HasParentConsumer = hasParentConsumer;
            _receiverQueueRefillThreshold = conf.ReceiverQueueSize / 2;
            _priorityLevel = conf.PriorityLevel;
            _readCompacted = conf.ReadCompacted;
            _subscriptionInitialPosition = conf.SubscriptionInitialPosition;
            _resetIncludeHead = conf.ResetIncludeHead;
            _createTopicIfDoesNotExist = createTopicIfDoesNotExist;
            _maxPendingChuckedMessage = conf.MaxPendingChuckedMessage;
            _pendingChunckedMessageUuidQueue = new Queue<string>();
            ExpireTimeOfIncompleteChunkedMessage = conf.ExpireTimeOfIncompleteChunkedMessage;
            _autoAckOldestChunkedMessageOnQueueFull = conf.AutoAckOldestChunkedMessageOnQueueFull;

            if (clientConfiguration.StatsIntervalSeconds.TotalMilliseconds > 0)
            {
                _stats = new ConsumerStatsRecorder<T>(Context.System, conf, _topicName.ToString(), ConsumerName, Subscription, clientConfiguration.StatsIntervalSeconds);
            }
            else
            {
                _stats = ConsumerStatsDisabled.Instance;
            }

            _duringSeek = false;

            if (conf.AckTimeout.TotalMilliseconds > 0)
            {
                if (conf.AckTimeoutRedeliveryBackoff != null)
                {
                    _unAckedMessageTracker = Context.ActorOf(UnAckedMessageRedeliveryTracker<T>.Prop(Self, UnAckedChunckedMessageIdSequenceMap, conf), "UnAckedMessageRedeliveryTracker");
                }
                else
                {
                    _unAckedMessageTracker = Context.ActorOf(UnAckedMessageTracker<T>.Prop(Self, UnAckedChunckedMessageIdSequenceMap, conf), "UnAckedMessageTracker");
                }
            }
            else
            {
                _unAckedMessageTracker = Context.ActorOf(UnAckedMessageTrackerDisabled<T>.Prop(), "UnAckedMessageTrackerDisabled");
            }

            _negativeAcksTracker = Context.ActorOf(NegativeAcksTracker<T>.Prop(conf, _self, UnAckedChunckedMessageIdSequenceMap));
            // Create msgCrypto if not created already
            if (conf.CryptoKeyReader != null)
            {
                if (conf.MessageCrypto != null)
                {
                    _msgCrypto = conf.MessageCrypto;
                }
                else
                {
                    // default to use MessageCryptoBc;
                    IMessageCrypto msgCryptoBc;
                    try
                    {
                        msgCryptoBc = new MessageCrypto($"[{topic}] [{Subscription}]", false, _log);
                    }
                    catch (Exception e)
                    {
                        _log.Error("MessageCryptoBc may not included in the jar. e:", e);
                        msgCryptoBc = null;
                    }
                    _msgCrypto = msgCryptoBc;
                }
            }
            else
            {
                _msgCrypto = null;
            }

            if (conf.Properties.Count == 0)
            {
                _metadata = ImmutableDictionary.Create<string, string>();
            }
            else
            {
                _metadata = new Dictionary<string, string>(conf.Properties).ToImmutableDictionary();
            }

            _connectionHandler = Context.ActorOf(ConnectionHandler.Prop(clientConfiguration, State, new BackoffBuilder()
                .SetInitialTime(TimeSpan.FromMilliseconds(clientConfiguration.InitialBackoffIntervalMs))
                .SetMax(TimeSpan.FromMilliseconds(clientConfiguration.MaxBackoffIntervalMs))
                .SetMandatoryStop(TimeSpan.FromMilliseconds(0)).Create(), Self));

            if (_topicName.Persistent)
            {
                _acknowledgmentsGroupingTracker = Context.ActorOf(PersistentAcknowledgmentsGroupingTracker<T>.Prop(UnAckedChunckedMessageIdSequenceMap, _self, idGenerator, _consumerId, _connectionHandler, conf));
            }
            else
            {
                _acknowledgmentsGroupingTracker = Context.ActorOf(NonPersistentAcknowledgmentGroupingTracker.Prop());
            }

            if (conf.DeadLetterPolicy != null)
            {
                _possibleSendToDeadLetterTopicMessages = new Dictionary<IMessageId, IList<IMessage<T>>>();
                if (!string.IsNullOrWhiteSpace(conf.DeadLetterPolicy.DeadLetterTopic))
                {
                    _deadLetterPolicy = new DeadLetterPolicy()
                    {
                        MaxRedeliverCount = conf.DeadLetterPolicy.MaxRedeliverCount,
                        DeadLetterTopic = conf.DeadLetterPolicy.DeadLetterTopic
                    };
                }
                else
                {
                    _deadLetterPolicy = new DeadLetterPolicy()
                    {
                        MaxRedeliverCount = conf.DeadLetterPolicy.MaxRedeliverCount,
                        DeadLetterTopic = $"{RetryMessageUtil.DlqGroupTopicSuffix}-{topic} {Subscription}"
                    };
                }

                if (!string.IsNullOrWhiteSpace(conf.DeadLetterPolicy.RetryLetterTopic))
                {
                    _deadLetterPolicy.RetryLetterTopic = conf.DeadLetterPolicy.RetryLetterTopic;
                }
                else
                {
                    _deadLetterPolicy.RetryLetterTopic = string.Format("{0}-{1}" + RetryMessageUtil.RetryGroupTopicSuffix, topic, Subscription);
                }

                if (!string.IsNullOrWhiteSpace(conf.DeadLetterPolicy.InitialSubscriptionName))
                {
                    _deadLetterPolicy.InitialSubscriptionName = conf.DeadLetterPolicy.InitialSubscriptionName;
                }
            }
            else
            {
                _deadLetterPolicy = null;
                _possibleSendToDeadLetterTopicMessages = null;
            }

            _topicNameWithoutPartition = _topicName.PartitionedTopicName;
            _receiveRun = Context.System.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(100), () =>
            {
                _receives.TryDequeue(out var queue);
                if (queue.Item1 != null)
                {
                    try
                    {
                        var message = queue.Item2.Time == TimeSpan.Zero ? Receive() : Receive(queue.Item2.Time);
                        queue.Item1.Tell(new AskResponse(message));
                    }
                    catch (Exception ex)
                    {
                        queue.Item1.Tell(new AskResponse(ex));
                    }
                }

            });
            _batchRun = Context.System.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(100), () =>
            {
                _batchReceives.TryDequeue(out var queue);
                if (queue.Item1 != null)
                {
                    try
                    {
                        var message = BatchReceive();
                        queue.Item1.Tell(new AskResponse(message));
                    }
                    catch (Exception ex)
                    {
                        queue.Item1.Tell(new AskResponse(ex));
                    }
                }

            });
            Ready();
            GrabCnx();
        }
        public static Props Prop(long consumerId, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, bool parentConsumerHasListener, IMessageId startMessageId, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfigurationData, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(() => new ConsumerActor<T>(consumerId, stateActor, client, lookup, cnxPool, idGenerator, topic, conf, partitionIndex, hasParentConsumer, parentConsumerHasListener, startMessageId, schema, createTopicIfDoesNotExist, clientConfigurationData, subscribeFuture));

        }

        public static Props Prop(long consumerId, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, bool parentConsumerHasListener, IMessageId startMessageId, long startMessageRollbackDurationInSec, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(() => new ConsumerActor<T>(consumerId, stateActor, client, lookup, cnxPool, idGenerator, topic, conf, partitionIndex, hasParentConsumer, parentConsumerHasListener, startMessageId, startMessageRollbackDurationInSec, schema, createTopicIfDoesNotExist, clientConfiguration, subscribeFuture));
        }
        protected internal override void CompleteOpBatchReceive(OpBatchReceive op)
        {
            NotifyPendingBatchReceivedCallBack(op);
        }
        private async ValueTask ConnectionOpened(ConnectionOpened c)
        {
            try
            {
                _previousExceptions.Clear();
                _maxMessageSize = (int)c.MaxMessageSize;
                _protocolVersion = c.ProtocolVersion;
                if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
                {
                    State.ConnectionState = HandlerState.State.Closed;
                    CloseConsumerTasks();
                    DeregisterFromClientCnx();
                    _client.Tell(new CleanupConsumer(Self));
                    ClearReceiverQueue();
                    SubscribeFuture.TrySetException(new PulsarClientException("Consumer is in a closing state"));
                    return;
                }
                _clientCnx = c.ClientCnx;
                SetCnx(c.ClientCnx);
                _log.Info($"[{Topic}][{Subscription}] Subscribing to topic on cnx {_clientCnx.Path.Name}, consumerId {_consumerId}");

                var id = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).ConfigureAwait(false);
                var requestId = id.Id;
                if (_duringSeek)
                {
                    _acknowledgmentsGroupingTracker.Tell(FlushAndClean.Instance);
                }

                _subscribeDeadline = DateTimeHelper.CurrentUnixTimeMillis() + (long)_clientConfigurationData.OperationTimeout.TotalMilliseconds;

                var currentSize = IncomingMessages.Count;

                _startMessageId = ClearReceiverQueue();

                if (_possibleSendToDeadLetterTopicMessages != null)
                {
                    _possibleSendToDeadLetterTopicMessages.Clear();
                }

                var isDurable = _subscriptionMode == SubscriptionMode.Durable;
                MessageIdData startMessageIdData = null;
                if (isDurable)
                {
                    // For regular durable subscriptions, the message id from where to restart will be determined by the broker.
                    startMessageIdData = null;
                }
                else if (_startMessageId != null)
                {
                    // For non-durable we are going to restart from the next entry
                    var builder = new MessageIdData
                    {
                        ledgerId = (ulong)_startMessageId.LedgerId,
                        entryId = (ulong)_startMessageId.EntryId
                    };
                    if (_startMessageId is BatchMessageId _)
                    {
                        builder.BatchIndex = _startMessageId.BatchIndex;
                    }

                }

                var si = Schema.SchemaInfo;
                if (si != null && (si.Type == SchemaType.BYTES || si.Type == SchemaType.NONE))
                {
                    // don't set schema for Schema.BYTES
                    si = null;
                }
                // startMessageRollbackDurationInSec should be consider only once when consumer connects to first time
                var startMessageRollbackDuration = _startMessageRollbackDurationInSec > 0 && _startMessageId != null && _startMessageId.Equals(_initialStartMessageId) ? _startMessageRollbackDurationInSec : 0;
                var request = Commands.NewSubscribe(Topic, Subscription, _consumerId, requestId, SubType, _priorityLevel, ConsumerName, isDurable, startMessageIdData, _metadata, _readCompacted, Conf.ReplicateSubscriptionState, _subscriptionInitialPosition.ValueOf(), startMessageRollbackDuration, si, _createTopicIfDoesNotExist, Conf.KeySharedPolicy, Conf.SubscriptionProperties, ConsumerEpoch);

                var result = await _clientCnx.Ask(new SendRequestWithId(request, requestId), _clientConfigurationData.OperationTimeout).ConfigureAwait(false);

                if (result is CommandSuccessResponse _)
                {
                    if (State.ChangeToReadyState())
                    {
                        ConsumerIsReconnectedToBroker(_clientCnx, currentSize);
                    }
                    else
                    {
                        State.ConnectionState = HandlerState.State.Closed;
                        DeregisterFromClientCnx();
                        _client.Tell(new CleanupConsumer(_self));
                        //await _clientCnx.GracefulStop(TimeSpan.FromSeconds(1));
                        SubscribeFuture.TrySetException(new PulsarClientException("Consumer is closed"));
                        return;
                    }
                    ResetBackoff();
                    SubscribeFuture.TrySetResult(_self);
                    if (Conf.ReceiverQueueSize != 0)
                    {
                        IncreaseAvailablePermits(Conf.ReceiverQueueSize);
                    }
                }
                else if (result is AskResponse response)
                {
                    if (response.Failed)
                    {
                        DeregisterFromClientCnx();
                        var e = response.Exception;
                        if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
                        {
                            await _clientCnx.GracefulStop(TimeSpan.FromSeconds(1));
                            SubscribeFuture.TrySetException(new PulsarClientException("Consumer is in a closing state"));
                            return;
                        }
                        else if (!SubscribeFuture.Task.IsCompleted)
                        {
                            State.ConnectionState = HandlerState.State.Closed;
                            CloseConsumerTasks();
                            SubscribeFuture.TrySetException(Wrap(e, $"Failed to subscribe the topic {_topicName} with subscription name {Subscription} when connecting to the broker"));

                            _client.Tell(new CleanupConsumer(_self));
                        }
                        else if (e is TopicDoesNotExistException)
                        {
                            var msg = $"[{Topic}][{Subscription}] Closed consumer because topic does not exist anymore";
                            State.ConnectionState = HandlerState.State.Failed;
                            CloseConsumerTasks();
                            _client.Tell(new CleanupConsumer(Self));
                            _log.Warning(msg);
                            SubscribeFuture.TrySetException(new PulsarClientException(msg));
                        }
                        else
                        {
                            ReconnectLater(e);
                        }
                    }
                    else
                    {
                        SubscribeFuture.TrySetResult(_self);

                    }
                }
                else if (result is ConnectionFailed failed)
                    ConnectionFailed(failed.Exception);
            }
            catch (Exception e)
            {
                SubscribeFuture.TrySetException(new PulsarClientException(e));
            }
        }
        private void GrabCnx()
        {
            _connectionHandler.Tell(new GrabCnx($"Create connection from consumer: {ConsumerName}"));
        }
        private void Ready()
        {
            ReceiveAsync<AskResponse>(async ask =>
            {
                if (ask.Failed)
                {
                    ConnectionFailed(ask.Exception);
                    return;
                }

                await ConnectionOpened(ask.ConvertTo<ConnectionOpened>()).ConfigureAwait(false);
            });
            Receive<PossibleSendToDeadLetterTopicMessagesRemove>(s =>
            {
                if (_possibleSendToDeadLetterTopicMessages != null)
                {
                    _possibleSendToDeadLetterTopicMessages.Remove(s.MessageId);
                }
            });
            Receive<RemoveMessagesTill>(s =>
            {
                _unAckedMessageTracker.Tell(s, Sender);
            });
            Receive<UnAckedMessageTrackerRemove>(s =>
            {
                _unAckedMessageTracker.Tell(new Remove(s.MessageId));
            });
            Receive<IncrementNumAcksSent>(s =>
            {
                Stats.IncrementNumAcksSent(s.Sent);
            });
            Receive<OnAcknowledge>(on =>
            {
                OnAcknowledge(on.MessageId, on.Exception);
            });
            Receive<SetTerminated>(on =>
            {
                SetTerminated();
            });
            Receive<OnAcknowledgeCumulative>(on =>
            {
                OnAcknowledgeCumulative(on.MessageId, on.Exception);
            });
            Receive<BatchReceive>(batch =>
            {
                _batchReceives.Enqueue((Sender, batch));
            });
            Receive<Messages.Consumer.Receive>(receive =>
            {
                _receives.Enqueue((Sender, receive));
            });
            Receive<SendState>(_ =>
            {
                StateActor.Tell(new SetConumerState(State.ConnectionState));
            });

            Receive<AckTimeoutSend>(ack =>
            {
                OnAckTimeoutSend(ack.MessageIds);
            });
            Receive<OnNegativeAcksSend>(ack =>
            {
                OnNegativeAcksSend(ack.MessageIds);
            });
            Receive<ConnectionClosed>(m =>
            {
                ConnectionClosed(m.ClientCnx);
            });
            ReceiveAsync<Close>(async c =>
            {
                _replyTo = Sender;
                if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
                {
                    CloseConsumerTasks();
                    FailPendingReceive();
                }

                if (!Connected())
                {
                    _log.Info($"[{Topic}] [{Subscription}] Closed Consumer (not connected)");
                    State.ConnectionState = HandlerState.State.Closed;
                    CloseConsumerTasks();
                    DeregisterFromClientCnx();
                    _client.Tell(new CleanupConsumer(Self));
                    FailPendingReceive();
                }

                State.ConnectionState = HandlerState.State.Closing;

                CloseConsumerTasks();

                var requestId = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).GetAwaiter().GetResult();

                try
                {

                    if (null == _clientCnx)
                    {
                        CleanupAtClose(null);
                        _replyTo.Tell(new AskResponse());
                    }
                    else
                    {
                        var cmd = Commands.NewCloseConsumer(_consumerId, requestId.Id);
                        var pay = new Payload(cmd, requestId.Id, "NewCloseConsumer");
                        var ask = await _clientCnx.Ask<AskResponse>(pay).ConfigureAwait(false);
                        _replyTo.Tell(ask);
                    }

                }
                catch (Exception ex)
                {
                    _replyTo.Tell(new AskResponse(ex));
                    _log.Error(ex.ToString());
                }

                _replyTo = null;
            });
            Receive<ClearIncomingMessagesAndGetMessageNumber>(_ =>
            {
                var cleared = ClearIncomingMessagesAndGetMessageNumber();
                Sender.Tell(new IncomingMessagesCleared(cleared));
            });
            Receive<GetHandlerState>(_ =>
            {
                Sender.Tell(new AskResponse(State.ConnectionState));
            });
            Receive<GetIncomingMessageSize>(_ =>
            {
                Sender.Tell(new AskResponse(IncomingMessagesSize));
            });
            Receive<GetIncomingMessageCount>(_ =>
            {
                Sender.Tell(new AskResponse(IncomingMessages.Count));
            });
            Receive<GetCnx>(_ =>
            {
                Sender.Tell(new AskResponse(_clientCnx));
            });
            Receive<IncreaseAvailablePermits>(i =>
            {
                if (i.Available > 0)
                    IncreaseAvailablePermits(_clientCnx, i.Available);
                else
                    IncreaseAvailablePermits(_clientCnx);
            });

            Receive<IncreaseAvailablePermits<T>>(i =>
            {
                IncreaseAvailablePermits((Message<T>)i.Message);
            });
            ReceiveAsync<IAcknowledge>(async ack =>
            {
                _replyTo = Sender;
                await Acknowledge(ack);
            });
            ReceiveAsync<ICumulative>(async cumulative =>
            {
                _replyTo = Sender;
                await Cumulative(cumulative);
            });

            Receive<GetLastDisconnectedTimestamp>(m =>
            {
                var last = LastDisconnectedTimestamp();
                Sender.Tell(last);
            });
            Receive<GetConsumerName>(m =>
            {
                Sender.Tell(ConsumerName);
            });
            Receive<AckReceipt>(m =>
            {
                AckReceipt(m.RequestId);
            });
            Receive<AckError>(m =>
            {
                AckError(m.RequestId, m.Exception);
            });
            Receive<ActiveConsumerChanged>(m =>
            {
                ActiveConsumerChanged(m.IsActive);
            });
            ReceiveAsync<MessageReceived>(async m =>
            {
                await MessageReceived(m);
            });
            Receive<GetSubscription>(m =>
            {
                Sender.Tell(Subscription);
            });
            Receive<GetTopicNameWithoutPartition>(m =>
            {
                Sender.Tell(TopicNameWithoutPartition);
            });
            Receive<GetTopic>(m =>
            {
                Sender.Tell(_topicName.ToString());
            });
            Receive<ClearUnAckedChunckedMessageIdSequenceMap>(_ =>
            {
                UnAckedChunckedMessageIdSequenceMap.Tell(Clear.Instance);
            });
            Receive<HasReachedEndOfTopic>(_ =>
            {
                var hasReached = HasReachedEndOfTopic();
                Sender.Tell(new AskResponse(hasReached));
            });
            Receive<GetAvailablePermits>(_ =>
            {
                var permits = AvailablePermits();
                Sender.Tell(permits);
            });
            Receive<MessageProcessed<T>>(m =>
            {
                try
                {
                    MessageProcessed(m.Message);
                }
                catch (Exception ex)
                {
                    _log.Error($"{m}===>>>{ex}");
                }
            });
            Receive<IsConnected>(_ =>
            {
                Sender.Tell(Connected());
            });
            Receive<Pause>(_ =>
            {
                Pause();
            });
            ReceiveAsync<HasMessageAvailable>(async _ =>
            {
                try
                {
                    _replyTo = Sender;
                    var has = await HasMessageAvailableAsync();
                    _replyTo.Tell(new AskResponse(has));
                }
                catch (Exception ex)
                {
                    _replyTo.Tell(new AskResponse(ex));
                    _log.Error($"[{Topic}][{Subscription}] Failed getLastMessageId command: {ex}");
                }
            });
            Receive<GetNumMessagesInQueue>(_ =>
            {
                var num = NumMessagesInQueue();
                Sender.Tell(num);
            });
            Receive<Resume>(_ =>
            {
                Resume();
            });
            ReceiveAsync<GetLastMessageId>(async m =>
            {
                try
                {
                    _replyTo = Sender;
                    var lmsid = await LastMessageId();
                    _replyTo.Tell(lmsid);
                }
                catch (Exception ex)
                {
                    var nul = new NullMessageId(ex);
                    _replyTo.Tell(nul);
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
                    Sender.Tell(null);
                }
            });
            Receive<NegativeAcknowledgeMessage<T>>(m =>
            {
                try
                {
                    NegativeAcknowledge(m.Message);
                    Sender.Tell(new AskResponse());
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(Unwrap(ex)));
                }
            });
            Receive<NegativeAcknowledgeMessages<T>>(m =>
            {
                try
                {
                    NegativeAcknowledge(m.Messages);
                    Sender.Tell(new AskResponse());
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(Unwrap(ex)));
                }
            });
            Receive<NegativeAcknowledgeMessageId>(m =>
            {
                try
                {
                    NegativeAcknowledge(m.MessageId);
                    Sender.Tell(new AskResponse());
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(Unwrap(ex)));
                }
            });
            Receive<NegativeAcknowledgeMessage<T>>(m =>
            {
                try
                {
                    NegativeAcknowledge(m.Message);
                    Sender.Tell(null);
                }
                catch (Exception ex)
                {
                    Sender.Tell(Unwrap(ex));
                }
            });
            Receive<ReconsumeLaterMessages<T>>(m =>
            {
                try
                {
                    ReconsumeLater(m.Messages, m.DelayTime);
                    Sender.Tell(new AskResponse());
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(Unwrap(ex)));
                }
            });
            ReceiveAsync<ReconsumeLaterMessage<T>>(async m =>
            {
                try
                {

                    _replyTo = Sender;
                    await ReconsumeLater(m.Message, m.Properties != null ? m.Properties.ToDictionary(p => p.Key, p => p.Value) : new Dictionary<string, string>(), m.DelayTime);
                    _replyTo.Tell(new AskResponse());
                }
                catch (Exception ex)
                {
                    _replyTo.Tell(new AskResponse(Unwrap(ex)));
                }
            });
            Receive<RedeliverUnacknowledgedMessages>(m =>
            {
                RedeliverUnacknowledgedMessages();
                Sender.Tell(new AskResponse());
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
                    Sender.Tell(new AskResponse(Unwrap(ex)));
                }
            });
            Receive<Unsubscribe>(un =>
            {
                try
                {
                    Unsubscribe(un.Force);
                    Sender.Tell(new AskResponse($"[{Topic}][{Subscription}] Successfully unsubscribed from topic"));
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
                    Sender.Tell(new AskResponse(Unwrap(ex)));
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
                    Sender.Tell(new AskResponse(Unwrap(ex)));
                }
            });
            Receive<bool>(c => { });
            Receive<string>(s => { });
        }

        private async ValueTask Acknowledge(IAcknowledge ack)
        {
            try
            {
                switch (ack)
                {
                    case AcknowledgeMessage<T> m:
                        ValidateMessageId(m.Message.MessageId);
                        await DoAcknowledgeWithTxn(m.Message.MessageId, AckType.Individual, _properties, null).Task;
                        break;
                    case AcknowledgeMessageId id:
                        ValidateMessageId(id.MessageId);
                        await DoAcknowledgeWithTxn(id.MessageId, AckType.Individual, _properties, null).Task;
                        break;
                    case AcknowledgeMessageIds ids:
                        foreach (var message in ids.MessageIds)
                        {
                            ValidateMessageId(message);
                        }
                        await DoAcknowledgeWithTxn(ids.MessageIds, AckType.Individual, _properties, null).Task;
                        break;
                    case AcknowledgeWithTxnMessages mTxn:
                        foreach (var message in mTxn.MessageIds)
                        {
                            ValidateMessageId(message);
                        }
                        await DoAcknowledgeWithTxn(mTxn.MessageIds, mTxn.AckType, mTxn.Properties, mTxn.Txn).Task;
                        break;
                    case AcknowledgeWithTxn txn:
                        ValidateMessageId(txn.MessageId);
                        await DoAcknowledgeWithTxn(txn.MessageId, txn.AckType, txn.Properties, txn.Txn).Task;
                        break;
                    case AcknowledgeMessages<T> ms:
                        foreach (var x in ms.Messages)
                        {
                            ValidateMessageId(x.MessageId);
                            await DoAcknowledgeWithTxn(x.MessageId, AckType.Individual, _properties, null).Task;
                        }
                        break;
                    default:
                        _log.Warning($"{ack.GetType().FullName} not supported");
                        break;
                }

                _replyTo.Tell(new AskResponse());
            }
            catch (Exception ex)
            {
                _replyTo.Tell(new AskResponse(new PulsarClientException(ex)));
            }
        }

        private void ValidateMessageId(IMessageId messageId)
        {
            if (messageId == null)
            {
                throw new PulsarClientException.InvalidMessageException("Cannot handle message with null messageId");
            }
        }
        private async ValueTask Cumulative(ICumulative cumulative)
        {
            try
            {
                if (!IsCumulativeAcknowledgementAllowed(Conf.SubscriptionType))
                {
                    _replyTo.Tell(new AskResponse(new InvalidConfigurationException(
                            "Cannot use cumulative acks on a non-exclusive/non-failover subscription")));
                    return;
                }

                switch (cumulative)
                {
                    case AcknowledgeCumulativeMessageId ack:
                        await DoAcknowledgeWithTxn(ack.MessageId, AckType.Cumulative, _properties, null).Task;
                        break;
                    case AcknowledgeCumulativeMessage<T> ack:
                        await DoAcknowledgeWithTxn(ack.Message.MessageId, AckType.Cumulative, _properties, null).Task;
                        break;
                    case AcknowledgeCumulativeTxn ack:
                        await DoAcknowledgeWithTxn(ack.MessageId, AckType.Cumulative, _properties, ack.Txn).Task;
                        break;
                    case ReconsumeLaterCumulative<T> ack:
                        if (ack.Properties != null)
                            await DoReconsumeLater(ack.Message, AckType.Cumulative, ack.Properties, ack.DelayTime).Task;
                        else
                            await DoReconsumeLater(ack.Message, AckType.Cumulative, new Dictionary<string, string>(), ack.DelayTime).Task;
                        break;
                    default:
                        _log.Warning($"{cumulative.GetType().FullName} not supported");
                        break;
                }
                _replyTo.Tell(new AskResponse());
            }
            catch (Exception ex)
            {
                _replyTo.Tell(new AskResponse(new PulsarClientException(ex)));
            }
        }

        internal override void Unsubscribe(bool force)
        {
            if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
            {
                throw new AlreadyClosedException("AlreadyClosedException: Consumer was already closed");
            }
            else
            {
                if (Connected())
                {
                    State.ConnectionState = HandlerState.State.Closing;
                    var res = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).GetAwaiter().GetResult();
                    var requestId = res.Id;
                    var unsubscribe = Commands.NewUnsubscribe(_consumerId, requestId, force);
                    var cnx = _clientCnx;
                    cnx.Tell(new SendRequestWithId(unsubscribe, requestId));
                    CloseConsumerTasks();
                    DeregisterFromClientCnx();
                    _client.Tell(new CleanupConsumer(Self));
                    _log.Info($"[{Topic}][{Subscription}] Successfully unsubscribed from topic");
                    State.ConnectionState = HandlerState.State.Closed;
                }
                else
                {
                    var err = $"The client is not connected to the broker when unsubscribing the subscription {Subscription} of the topic {_topicName}";
                    _log.Error(err);
                    throw new NotConnectedException(err);

                }
            }
        }
        public override int MinReceiverQueueSize()
        {
            var size = Math.Min(InitialReceiverQueueSize, MaxReceiverQueueSize);
            if (BatchReceivePolicy.MaxNumMessages > 0)
            {
                // consumerImpl may store (half-1) permits locally.
                size = Math.Max(size, 2 * BatchReceivePolicy.MaxNumMessages - 2);
            }
            return size;
        }
        protected override void PostStop()
        {
            _stats.StatTimeout?.Cancel();
            _tokenSource.Cancel();
            _receiveRun.Cancel();
            _batchRun.Cancel();
            base.PostStop();
        }

        internal override IConsumerStatsRecorder Stats
        {
            get
            {
                return _stats;
            }
        }
        protected internal override void DoAcknowledge(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        {
            Condition.CheckArgument(messageId is MessageId);
            if (State.ConnectionState != HandlerState.State.Ready && State.ConnectionState != HandlerState.State.Connecting)
            {
                Stats.IncrementNumAcksFailed();
                var exception = new PulsarClientException("Consumer not ready. State: " + State);
                if (AckType.Individual.Equals(ackType))
                {
                    OnAcknowledge(messageId, exception);
                }
                else if (AckType.Cumulative.Equals(ackType))
                {
                    OnAcknowledgeCumulative(messageId, exception);
                }
                throw exception;
            }

            if (txn != null)
            {
                var requestId = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).GetAwaiter().GetResult();
                var bits = txn.Ask<GetTxnIdBitsResponse>(GetTxnIdBits.Instance).GetAwaiter().GetResult();
                DoTransactionAcknowledgeForResponse(messageId, ackType, null, properties, new TransactionImpl.TxnID(bits.MostBits, bits.LeastBits), requestId.Id);
                return;
            }
            _acknowledgmentsGroupingTracker.Tell(new AddAcknowledgment(messageId, ackType, properties));
        }
        protected internal override void DoAcknowledge(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        {
            foreach (var messageId in messageIdList)
            {
                Condition.CheckArgument(messageId is MessageId);
            }
            if (State.ConnectionState != HandlerState.State.Ready && State.ConnectionState != HandlerState.State.Connecting)
            {
                Stats.IncrementNumAcksFailed();
                var exception = new PulsarClientException("Consumer not ready. State: " + State);
                if (AckType.Individual.Equals(ackType))
                {
                    OnAcknowledge(messageIdList, exception);
                }
                else if (AckType.Cumulative.Equals(ackType))
                {
                    OnAcknowledgeCumulative(messageIdList, exception);
                }
                throw exception;
            }
            if (txn != null)
            {
                var requestId = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).GetAwaiter().GetResult();
                var bits = txn.Ask<GetTxnIdBitsResponse>(GetTxnIdBits.Instance).GetAwaiter().GetResult();
                DoTransactionAcknowledgeForResponse(messageIdList, ackType, null, properties, new TransactionImpl.TxnID(bits.MostBits, bits.LeastBits), requestId.Id);
                return;
            }
            else
            {
                _acknowledgmentsGroupingTracker.Tell(new AddListAcknowledgment(messageIdList, ackType, properties));
                return;
            };
        }
        protected internal override TaskCompletionSource<object> DoReconsumeLater(IMessage<T> message, AckType ackType, IDictionary<string, string> properties, TimeSpan delayTime)
        {
            var result = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            var messageId = message.MessageId;
            if (messageId == null)
            {
                result.TrySetException(new PulsarClientException.InvalidMessageException("Cannot handle message with null messageId"));
                return result;
            }
            if (messageId is TopicMessageId id)
            {
                messageId = id.InnerMessageId;
            }
            Condition.CheckArgument(messageId is MessageId);
            if (State.ConnectionState != HandlerState.State.Ready && State.ConnectionState != HandlerState.State.Connecting)
            {
                Stats.IncrementNumAcksFailed();
                var exception = new PulsarClientException("Consumer not ready. State: " + State);
                if (AckType.Individual.Equals(ackType))
                {
                    OnAcknowledge(messageId, exception);
                }
                else if (AckType.Cumulative.Equals(ackType))
                {
                    OnAcknowledgeCumulative(messageId, exception);
                }
                result.TrySetException(exception);

                return result;
            }
            if (delayTime.TotalMilliseconds < 0)
            {
                delayTime = TimeSpan.Zero;
            }
            try
            {
                if (_retryLetterProducer == null)
                {
                    var client = new PulsarClient(_client, _lookup, _cnxPool, _generator, _clientConfigurationData, Context.System, null);
                    var builder = new ProducerConfigBuilder<T>()
                    .Topic(_deadLetterPolicy.RetryLetterTopic)
                    .EnableBatching(false);
                    _retryLetterProducer = client.NewProducer(Schema, builder);
                }
            }
            catch (Exception e)
            {
                _log.Error($"Create retry letter producer exception with topic: {_deadLetterPolicy.RetryLetterTopic}:{e}");
            }
            if (_retryLetterProducer != null)
            {
                try
                {
                    var retryMessage = GetMessage(message);
                    var originMessageIdStr = GetOriginMessageIdStr(message);
                    var originTopicNameStr = GetOriginTopicNameStr(message);

                    var propertiesMap = GetPropertiesMap(message, originMessageIdStr, originTopicNameStr);
                    var reconsumetimes = 1;
                    if (propertiesMap.ContainsKey(RetryMessageUtil.SystemPropertyReconsumetimes))
                    {
                        reconsumetimes = int.Parse(propertiesMap[RetryMessageUtil.SystemPropertyReconsumetimes]);
                        reconsumetimes = reconsumetimes + 1;
                    }
                    propertiesMap[RetryMessageUtil.SystemPropertyReconsumetimes] = reconsumetimes.ToString();
                    propertiesMap[RetryMessageUtil.SystemPropertyDelayTime] = delayTime.TotalMilliseconds.ToString();

                    var finalMessageId = messageId;
                    if (reconsumetimes > _deadLetterPolicy.MaxRedeliverCount && !string.IsNullOrWhiteSpace(_deadLetterPolicy.DeadLetterTopic))
                    {
                        try
                        {
                            InitDeadLetterProducerIfNeeded();
                        }
                        catch (Exception ex)
                        {
                            result.TrySetException(ex);
                            _deadLetterProducer = null;
                            return result;
                        }

                        var typedMessageBuilderNew = _deadLetterProducer.NewMessage(ISchema<T>.AutoProduceBytes(retryMessage.ReaderSchema().Value))
                            .Value(retryMessage.Data.ToArray())
                            .Properties(propertiesMap);

                        typedMessageBuilderNew.SendAsync().AsTask().ContinueWith(task =>
                        {
                            if (task.Exception != null)
                            {
                                result.TrySetException(task.Exception);
                            }
                            else
                            {
                                try
                                {
                                    DoAcknowledge(finalMessageId, ackType, new Dictionary<string, long>(), null);
                                    result.TrySetResult(null);
                                }
                                catch (Exception ex)
                                {
                                    result.TrySetException(ex);
                                }
                            }
                        });
                    }
                    else
                    {
                        var typedMessageBuilderNew = _retryLetterProducer.NewMessage()
                            .Value(retryMessage.Value).Properties(propertiesMap);
                        if (delayTime > TimeSpan.Zero)
                        {
                            typedMessageBuilderNew.DeliverAfter(delayTime);
                        }
                        if (message.HasKey())
                        {
                            typedMessageBuilderNew.Key(message.Key);
                        }
                        typedMessageBuilderNew.SendAsync().AsTask()
                            .ContinueWith(__ =>
                            {
                                try
                                {
                                    DoAcknowledge(finalMessageId, ackType, new Dictionary<string, long>(), null);
                                    result.TrySetResult(null);
                                }
                                catch (Exception ex)
                                {
                                    result.TrySetException(ex);
                                }
                            });
                    }
                }
                catch (Exception e)
                {
                    _log.Error($"Send to retry letter topic exception with topic: {_deadLetterPolicy.DeadLetterTopic}, messageId: {messageId}: {e}");
                    ISet<IMessageId> messageIds = new HashSet<IMessageId>
                    {
                        messageId
                    };
                    _unAckedMessageTracker.Tell(new Remove(messageId));
                    Akka.Dispatch.ActorTaskScheduler.RunTask(() => RedeliverUnacknowledgedMessages(messageIds));
                    //RedeliverUnacknowledgedMessages(messageIds);
                }
            }
            else
            {
                var finalMessageId = messageId;
                result.TrySetException(new NullReferenceException("deadletterproducer"));
                var messageIds = new HashSet<IMessageId>
                    {
                        finalMessageId
                    };
                _unAckedMessageTracker.Tell(new Remove(finalMessageId));
                Akka.Dispatch.ActorTaskScheduler.RunTask(() => RedeliverUnacknowledgedMessages(messageIds));
                //RedeliverUnacknowledgedMessages(messageIds);
            }
            return result;
        }
        private Message<T> GetMessage(IMessage<T> message)
        {
            if (message is TopicMessage<T> m)
            {
                return (Message<T>)m.Message;
            }
            else if (message is Message<T> ms)
            {
                return ms;
            }
            return null;
        }
        internal override void NegativeAcknowledge(IMessageId messageId)
        {
            _negativeAcksTracker.Tell(new Add(messageId));

            // Ensure the message is not redelivered for ack-timeout, since we did receive an "ack"
            _unAckedMessageTracker.Tell(new Remove(messageId));
        }

        protected internal virtual void ConsumerIsReconnectedToBroker(IActorRef cnx, int currentQueueSize)
        {
            _log.Info($"[{Topic}][{Subscription}] Subscribed to topic on -- consumer: {_consumerId}");

            _availablePermits = 0;
        }

        /// <summary>
        /// Clear the internal receiver queue and returns the message id of what was the 1st message in the queue that was
        /// not seen by the application
        /// </summary>
        private BatchMessageId ClearReceiverQueue()
        {
            _log.Warning($"Clearing {IncomingMessages.Count} message(s) in queue");
            var currentMessageQueue = new List<IMessage<T>>(IncomingMessages.Count);
            var mcount = IncomingMessages.Count;
            var n = 0;
            //incomingMessages.drainTo(CurrentMessageQueue);
            while (n < mcount)
            {
                if (IncomingMessages.TryReceive(out var m))
                    currentMessageQueue.Add(m);
                else
                    break;
                ++n;
            }
            IncomingMessagesSize = 0;

            if (_duringSeek)
            {
                _duringSeek = false;
                return _seekMessageId;
            }
            else if (_subscriptionMode == SubscriptionMode.Durable)
            {
                return _startMessageId;
            }

            if (currentMessageQueue.Count > 0)
            {
                var nextMessageInQueue = currentMessageQueue[0].MessageId;
                BatchMessageId previousMessage;
                if (nextMessageInQueue is BatchMessageId next)
                {
                    // Get on the previous message within the current batch
                    previousMessage = new BatchMessageId(next.LedgerId, next.EntryId, next.PartitionIndex, next.BatchIndex - 1);
                }
                else
                {
                    var msgid = (MessageId)nextMessageInQueue;
                    // Get on previous message in previous entry
                    previousMessage = new BatchMessageId(msgid.LedgerId, msgid.EntryId - 1, msgid.PartitionIndex, -1);
                }

                return previousMessage;
            }
            else if (!_lastDequeuedMessageId.Equals(IMessageId.Earliest))
            {
                // If the queue was empty we need to restart from the message just after the last one that has been dequeued
                // in the past
                return new BatchMessageId((MessageId)_lastDequeuedMessageId);
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
        /// 

        private bool IsCumulativeAcknowledgementAllowed(SubType type)
        {
            return SubType.Shared != type && SubType.KeyShared != type;
        }

        private void SendFlowPermitsToBroker(IActorRef cnx, int numMessages)
        {
            if (cnx != null && numMessages > 0)
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{Topic}] [{Subscription}] Adding {numMessages} additional permits");
                }
                var cmd = Commands.NewFlow(_consumerId, numMessages);
                var pay = new Payload(cmd, -1, "NewFlow");
                cnx.Tell(pay);
            }
        }

        internal virtual void ConnectionFailed(PulsarClientException exception)
        {
            var nonRetriableError = !IsRetriableError(exception);
            var timeout = DateTimeHelper.CurrentUnixTimeMillis() > _subscribeTimeout;
            if (nonRetriableError || timeout)
            {
                exception.SetPreviousExceptions(_previousExceptions);
                if (SubscribeFuture.TrySetException(exception))
                {
                    State.ConnectionState = HandlerState.State.Failed;
                    string msg;
                    if (nonRetriableError)
                    {
                        msg = $"[{Topic}] Consumer creation failed for consumer {_consumerId} with unretriableError: {exception}";
                    }
                    else
                    {
                        msg = $"[{Topic}] Consumer creation failed for consumer {_consumerId} after timeout";
                    }
                    _log.Info(msg);
                    CloseConsumerTasks();
                    DeregisterFromClientCnx();
                    _client.Tell(new CleanupConsumer(_self));
                }

            }
            else
            {
                _previousExceptions.Add(exception);
            }
        }

        private void CleanupAtClose(Exception exception)
        {
            _log.Info($"[{Topic}] [{Subscription}] Closed consumer");
            State.ConnectionState = HandlerState.State.Closed;
            CloseConsumerTasks();
            DeregisterFromClientCnx();
            _client.Tell(new CleanupConsumer(Self));
            // fail all pending-receive futures to notify application
            FailPendingReceive();

        }

        private void CloseConsumerTasks()
        {
            _unAckedMessageTracker.GracefulStop(TimeSpan.FromSeconds(3));
            if (_possibleSendToDeadLetterTopicMessages != null)
            {
                _possibleSendToDeadLetterTopicMessages.Clear();
            }
            _acknowledgmentsGroupingTracker.GracefulStop(TimeSpan.FromSeconds(3));
            if (BatchReceiveTimeout != null)
            {
                BatchReceiveTimeout.Cancel();
            }
            Stats.StatTimeout?.Cancel();
        }

        internal virtual void ActiveConsumerChanged(bool isActive)
        {
            if (ConsumerEventListener == null)
            {
                return;
            }

            if (isActive)
            {
                ConsumerEventListener.BecameActive(Self, _partitionIndex);
            }
            else
            {
                ConsumerEventListener.BecameInactive(Self, _partitionIndex);
            }
        }
        private async ValueTask MessageReceived(MessageReceived received)
        {
            var ms = received;
            var messageId = ms.MessageId;
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{Topic}][{Subscription}] Received message: {messageId.ledgerId}/{messageId.entryId}");
            }
            var msgId = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, PartitionIndex);

            if (!received.HasMagicNumber && !received.HasValidCheckSum)
            {
                // discard message with checksum error
                DiscardCorruptedMessage(messageId, _clientCnx, ValidationError.ChecksumMismatch);
                return;
            }

            try
            {
                var isDub = await _acknowledgmentsGroupingTracker.Ask<bool>(new IsDuplicate(msgId));
                if (isDub)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{Topic}] [{Subscription}] Ignoring message as it was already being acked earlier by same consumer {ConsumerName}/{msgId}");
                    }
                    IncreaseAvailablePermits(_clientCnx, ms.Metadata.NumMessagesInBatch);
                }
                else
                    ProcessMessage(ms);

            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }

        private void ProcessMessage(MessageReceived received)
        {
            var messageId = received.MessageId;
            var data = received.Payload.ToArray();
            var redeliveryCount = received.RedeliveryCount;
            var consumerEpoch = Commands.DefaultConsumerEpoch;
            // if broker send messages to client with consumerEpoch, we should set consumerEpoch to message
            if (received.HasConsumerEpoch)
            {
                consumerEpoch = received.ConsumerEpoch;
            }
            IList<long> ackSet = messageId.AckSets;

            var msgMetadata = received.Metadata;
            var brokerEntryMetadata = received.BrokerEntryMetadata;
            var numMessages = msgMetadata.NumMessagesInBatch;
            var isChunkedMessage = msgMetadata.NumChunksFromMsg > 1
                && Conf.SubscriptionType != SubType.Shared;

            var msgId = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, PartitionIndex);
            var decryptedPayload = DecryptPayloadIfNeeded(messageId, msgMetadata, data, _clientCnx);
            var isMessageUndecryptable = IsMessageUndecryptable(msgMetadata);
            if (decryptedPayload == null)
            {
                // Message was discarded or CryptoKeyReader isn't implemented
                return;
            }


            // uncompress decryptedPayload and release decryptedPayload-ByteBuf
            var uncompressedPayload = isMessageUndecryptable || isChunkedMessage ? decryptedPayload : UncompressPayloadIfNeeded(messageId, msgMetadata, decryptedPayload, _clientCnx, true);


            if (uncompressedPayload == null)
            {

                // Message was discarded on decompression error
                return;
            }
            if (Conf.PayloadProcessor != null)
            {
                // uncompressedPayload is released in this method so we don't need to call release() again
                ProcessPayloadByProcessor(brokerEntryMetadata, msgMetadata, uncompressedPayload, msgId, Schema, redeliveryCount, ackSet, consumerEpoch);
                return;
            }

            // if message is not decryptable then it can't be parsed as a batch-message. so, add EncyrptionCtx to message
            // and return undecrypted payload
            if (isMessageUndecryptable || numMessages == 1 && !HasNumMessagesInBatch(msgMetadata))
            {

                // right now, chunked messages are only supported by non-shared subscription
                if (isChunkedMessage)
                {
                    uncompressedPayload = ProcessMessageChunk(uncompressedPayload, msgMetadata, msgId, messageId, _clientCnx);
                    if (uncompressedPayload == null)
                    {
                        return;
                    }

                    // last chunk received: so, stitch chunked-messages and clear up chunkedMsgBuffer
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"Chunked message completed chunkId {msgMetadata.ChunkId}, total-chunks {msgMetadata.NumChunksFromMsg}, msgId {msgId} sequenceId {msgMetadata.SequenceId}");
                    }

                    // remove buffer from the map, set the chunk message id
                    var chunkedMsgCtx = _chunkedMessagesMap.RemoveEx(msgMetadata.Uuid);
                    if (chunkedMsgCtx.ChunkedMessageIds.Length > 0)
                    {
                        msgId = new ChunkMessageId(chunkedMsgCtx.ChunkedMessageIds[0], chunkedMsgCtx.ChunkedMessageIds[chunkedMsgCtx.ChunkedMessageIds.Length - 1]);
                    }
                    // add chunked messageId to unack-message tracker, and reduce pending-chunked-message count
                    UnAckedChunckedMessageIdSequenceMap.Tell(new AddMessageIds(msgId, chunkedMsgCtx.ChunkedMessageIds));
                    _pendingChunkedMessageCount--;
                    chunkedMsgCtx.Recycle();
                }

                // If the topic is non-persistent, we should not ignore any messages.
                if (_topicName.Persistent && IsSameEntry(msgId) && IsPriorEntryIndex((long)messageId.entryId))
                {
                    // We need to discard entries that were prior to startMessageId
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{Subscription}] [{ConsumerName}] Ignoring message from before the startMessageId: {_startMessageId}");
                    }
                    return;
                }
                var message = NewMessage(msgId, brokerEntryMetadata, msgMetadata, new ReadOnlySequence<byte>(uncompressedPayload), Schema, redeliveryCount, consumerEpoch);
                message.BrokerEntryMetadata = received.BrokerEntryMetadata;

                if (_deadLetterPolicy != null && _possibleSendToDeadLetterTopicMessages != null)
                {
                    if (redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
                    {
                        _possibleSendToDeadLetterTopicMessages[(MessageId)message.MessageId] = new List<IMessage<T>> { message };
                        if (redeliveryCount > _deadLetterPolicy.MaxRedeliverCount)
                        {
                            RedeliverUnacknowledgedMessages(new HashSet<IMessageId> { message.MessageId });
                            // The message is skipped due to reaching the max redelivery count,
                            // so we need to increase the available permits
                            IncreaseAvailablePermits(_clientCnx);
                            return;
                        }
                    }
                }
                ExecuteNotifyCallback(message);
            }

            else
            {
                // handle batch message enqueuing; uncompressed payload has all messages in batch
                ReceiveIndividualMessagesFromBatch(brokerEntryMetadata, msgMetadata, redeliveryCount, ackSet, uncompressedPayload, messageId, _clientCnx, consumerEpoch);

            }

            TryTriggerListener();
        }
        protected Message<T> NewSingleMessage(int index, int numMessages, BrokerEntryMetadata brokerEntryMetadata, MessageMetadata msgMetadata, SingleMessageMetadata singleMessageMetadata, byte[] payload, MessageId messageId, ISchema<T> schema, bool containMetadata, BitSet ackBitSet, BatchMessageAcker acker, int redeliveryCount, long consumerEpoch)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{Subscription}] [{ConsumerName}] processing message num - {index} in batch");
            }
            using var stream = new MemoryStream(payload);
            using var binaryReader = new BinaryReader(stream);
            byte[] singleMessagePayload = null;
            try
            {
                if (containMetadata)
                {
                    singleMessageMetadata = Serializer.DeserializeWithLengthPrefix<SingleMessageMetadata>(stream, PrefixStyle.Fixed32BigEndian);
                    singleMessagePayload = binaryReader.ReadBytes(singleMessageMetadata.PayloadSize);

                    singleMessagePayload = binaryReader.ReadBytes(singleMessageMetadata.PayloadSize);
                }

                // If the topic is non-persistent, we should not ignore any messages.
                if (_topicName.Persistent && IsSameEntry(messageId) && IsPriorBatchIndex(index))
                {
                    // If we are receiving a batch message, we need to discard messages that were prior
                    // to the startMessageId
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{Subscription}] [{ConsumerName}] Ignoring message from before the startMessageId: {_startMessageId}");
                    }
                    return null;
                }

                if (singleMessageMetadata != null && singleMessageMetadata.CompactedOut)
                {
                    // message has been compacted out, so don't send to the user
                    return null;
                }

                if (ackBitSet != null && ackBitSet.Get(index, index) != null)
                {
                    return null;
                }

                var batchMessageId = new BatchMessageId(messageId.LedgerId, messageId.EntryId, PartitionIndex, index, numMessages, acker);

                var payloadBuffer = singleMessagePayload != null ? singleMessagePayload : payload.ToArray();

                var message = Message<T>.Create(_topicName.ToString(), batchMessageId, msgMetadata, singleMessageMetadata, new ReadOnlySequence<byte>(payloadBuffer), CreateEncryptionContext(msgMetadata), _clientCnx, schema, redeliveryCount, false, consumerEpoch);
                message.BrokerEntryMetadata = brokerEntryMetadata;
                return message;
            }
            catch (Exception e) when (e is IOException || e is InvalidOperationException)
            {
                throw;
            }
            finally
            {
                if (singleMessagePayload != null)
                {
                    singleMessagePayload = null;
                }
            }
        }
        protected Message<T> NewSingleMessage(int index, int numMessages, BrokerEntryMetadata brokerEntryMetadata, MessageMetadata msgMetadata, MemoryStream stream, BinaryReader binaryReader, MessageId messageId, ISchema<T> schema, bool containMetadata, BitSet ackBitSet, BatchMessageAcker acker, int redeliveryCount, long consumerEpoch)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{Subscription}] [{ConsumerName}] processing message num - {index} in batch");
            }

            byte[] singleMessagePayload = null;
            SingleMessageMetadata singleMessageMetadata = null;
            if (containMetadata)
            {
                singleMessageMetadata = Serializer.DeserializeWithLengthPrefix<SingleMessageMetadata>(stream, PrefixStyle.Fixed32BigEndian);
                singleMessagePayload = binaryReader.ReadBytes(singleMessageMetadata.PayloadSize);

            }

            // If the topic is non-persistent, we should not ignore any messages.
            if (_topicName.Persistent && IsSameEntry(messageId) && IsPriorBatchIndex(index))
            {
                // If we are receiving a batch message, we need to discard messages that were prior
                // to the startMessageId
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{Subscription}] [{ConsumerName}] Ignoring message from before the startMessageId: {_startMessageId}");
                }
                return null;
            }

            if (singleMessageMetadata != null && singleMessageMetadata.CompactedOut)
            {
                // message has been compacted out, so don't send to the user
                return null;
            }

            if (ackBitSet != null && ackBitSet.Get(index, index) != null)
            {
                return null;
            }

            var batchMessageId = new BatchMessageId(messageId.LedgerId, messageId.EntryId, PartitionIndex, index, numMessages, acker);

            var message = Message<T>.Create(_topicName.ToString(), batchMessageId, msgMetadata, singleMessageMetadata, new ReadOnlySequence<byte>(singleMessagePayload), CreateEncryptionContext(msgMetadata), _clientCnx, schema, redeliveryCount, false, consumerEpoch);
            message.BrokerEntryMetadata = brokerEntryMetadata;
            return message;
        }

        protected Message<T> NewMessage(MessageId messageId, BrokerEntryMetadata brokerEntryMetadata, MessageMetadata messageMetadata, ReadOnlySequence<byte> payload, ISchema<T> schema, int redeliveryCount, long consumerEpoch)
        {
            var Message = Message<T>.Create(_topicName.ToString(), messageId, messageMetadata, payload, CreateEncryptionContext(messageMetadata), _clientCnx, schema, redeliveryCount, false, consumerEpoch);
            Message.BrokerEntryMetadata = brokerEntryMetadata;
            return Message;
        }
        protected internal virtual bool IsBatch(MessageMetadata messageMetadata)
        {
            // if message is not decryptable then it can't be parsed as a batch-message. so, add EncyrptionCtx to message
            // and return undecrypted payload
            return !IsMessageUndecryptable(messageMetadata) && (messageMetadata.ShouldSerializeNumMessagesInBatch() || messageMetadata.NumMessagesInBatch != 1);
        }
        private void ExecuteNotifyCallback(Message<T> message)
        {
            // Enqueue the message so that it can be retrieved when application calls receive()
            // if the conf.getReceiverQueueSize() is 0 then discard message if no one is waiting for it.
            // if asyncReceive is waiting then notify callback without adding to incomingMessages queue
            Akka.Dispatch.ActorTaskScheduler.RunTask(() =>
            {
                if (HasNextPendingReceive())
                {
                    NotifyPendingReceivedCallback(message, null);
                }
                else if (EnqueueMessageAndCheckBatchReceive(message) && HasPendingBatchReceive())
                {
                    NotifyPendingBatchReceivedCallBack();
                }
            });
        }
        private void ProcessPayloadByProcessor(BrokerEntryMetadata brokerEntryMetadata, MessageMetadata messageMetadata, byte[] payload, MessageId messageId, ISchema<T> schema, int redeliveryCount, in IList<long> ackSet, long consumerEpoch)
        {
            var msgPayload = MessagePayload.Create(new ReadOnlySequence<byte>(payload));

            var entryContext = MessagePayloadContext<T>.Get(brokerEntryMetadata, messageMetadata, messageId, redeliveryCount, ackSet, consumerEpoch, IsBatch, NewMessage, NewSingleMessage);

            var skippedMessages = 0;
            try
            {
                Conf.PayloadProcessor.Process(msgPayload, entryContext, schema, message =>
                {
                    if (message != null)
                    {
                        ExecuteNotifyCallback((Message<T>)message);
                        //EnqueueMessageAndCheckBatchReceive(message);
                    }
                    else
                    {
                        skippedMessages++;
                    }
                });
            }
            catch
            {
                _log.Warning($"[{Subscription}] [{ConsumerName}] unable to obtain message in batch");
                DiscardCorruptedMessage(messageId, _clientCnx, ValidationError.BatchDeSerializeError);
            }
            finally
            {
                entryContext.Recycle();
                //payload.Release(); // byteBuf.release() is called in this method
            }

            if (skippedMessages > 0)
            {
                IncreaseAvailablePermits(_clientCnx, skippedMessages);
            }

            TryTriggerListener();
        }

        private bool HasNumMessagesInBatch(MessageMetadata m)
        {
            var should = m.ShouldSerializeNumMessagesInBatch();
            return should;
        }
        /// <summary>
		/// Notify waiting asyncReceive request with the received message.
		/// </summary>
		/// <param name="message"> </param>
		internal virtual void NotifyPendingReceivedCallback(IMessage<T> message, Exception exception)
        {
            if (PendingReceives.Count == 0)
            {
                return;
            }

            // fetch receivedCallback from queue
            var receivedFuture = NextPendingReceive();
            if (receivedFuture == null)
            {
                return;
            }

            if (exception != null)
            {
                receivedFuture.SetException(exception);
                return;
            }

            if (message == null)
            {
                var e = new InvalidOperationException("received message can't be null");
                receivedFuture.SetException(e);
                return;
            }

            if (CurrentReceiverQueueSize == 0)
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

        private void InterceptAndComplete(IMessage<T> message, TaskCompletionSource<IMessage<T>> receivedFuture)
        {
            var interceptMessage = BeforeConsume(message);
            // return message to receivedCallback
            CompletePendingReceive(receivedFuture, interceptMessage);
        }

        private byte[] ProcessMessageChunk(byte[] compressedPayload, MessageMetadata msgMetadata, MessageId msgId, MessageIdData messageId, IActorRef cnx)
        {

            // Lazy task scheduling to expire incomplete chunk message
            if (!_expireChunkMessageTaskScheduled && ExpireTimeOfIncompleteChunkedMessage.TotalMilliseconds > 0)
            {
                Context.System.Scheduler.Advanced.ScheduleRepeatedly(ExpireTimeOfIncompleteChunkedMessage, ExpireTimeOfIncompleteChunkedMessage, RemoveExpireIncompleteChunkedMessages);
                _expireChunkMessageTaskScheduled = true;
            }

            if (msgMetadata.ChunkId == 0)
            {
                var totalChunks = msgMetadata.NumChunksFromMsg;
                _chunkedMessagesMap.TryAdd(msgMetadata.Uuid, ChunkedMessageCtx.Get(totalChunks, new List<byte>()));
                _pendingChunkedMessageCount++;
                if (_maxPendingChuckedMessage > 0 && _pendingChunkedMessageCount > _maxPendingChuckedMessage)
                {
                    RemoveOldestPendingChunkedMessage();
                }
                _pendingChunckedMessageUuidQueue.Enqueue(msgMetadata.Uuid);
            }

            ChunkedMessageCtx chunkedMsgCtx = null;
            if (_chunkedMessagesMap.ContainsKey(msgMetadata.Uuid))
                chunkedMsgCtx = _chunkedMessagesMap[msgMetadata.Uuid];

            // discard message if chunk is out-of-order
            if (chunkedMsgCtx == null || chunkedMsgCtx.ChunkedMsgBuffer == null || msgMetadata.ChunkId != chunkedMsgCtx.LastChunkedMessageId + 1 || msgMetadata.ChunkId >= msgMetadata.TotalChunkMsgSize)
            {
                // means we lost the first chunk: should never happen
                _log.Info($"Received unexpected chunk messageId {msgId}, last-chunk-id{chunkedMsgCtx?.LastChunkedMessageId ?? 0}, chunkId = {msgMetadata.ChunkId}, total-chunks {msgMetadata.TotalChunkMsgSize}");
                chunkedMsgCtx?.Recycle();
                _chunkedMessagesMap.Remove(msgMetadata.Uuid);
                IncreaseAvailablePermits(cnx);
                if (ExpireTimeOfIncompleteChunkedMessage.TotalMilliseconds > 0 && DateTimeHelper.CurrentUnixTimeMillis() > (long)msgMetadata.PublishTime + ExpireTimeOfIncompleteChunkedMessage.TotalMilliseconds)
                {
                    DoAcknowledge(msgId, AckType.Individual, new Dictionary<string, long>(), null);
                }
                else
                {
                    TrackMessage(msgId);
                }
                return null;
            }

            chunkedMsgCtx.ChunkedMessageIds[msgMetadata.ChunkId] = msgId;
            // append the chunked payload and update lastChunkedMessage-id
            chunkedMsgCtx.ChunkedMsgBuffer.AddRange(compressedPayload);
            chunkedMsgCtx.LastChunkedMessageId = msgMetadata.ChunkId;

            // if final chunk is not received yet then release payload and return
            if (msgMetadata.ChunkId != msgMetadata.NumChunksFromMsg - 1)
            {
                IncreaseAvailablePermits(cnx);
                return null;
            }


            // last chunk received: so, stitch chunked-messages and clear up chunkedMsgBuffer
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Chunked message completed chunkId {msgMetadata.ChunkId}, total-chunks {msgMetadata.NumChunksFromMsg}, msgId {msgId} sequenceId {msgMetadata.SequenceId}");
            }
            // remove buffer from the map, add chucked messageId to unack-message tracker, and reduce pending-chunked-message count
            //_chunkedMessagesMap.Remove(msgMetadata.Uuid);
            UnAckedChunckedMessageIdSequenceMap.Tell(new AddMessageIds(msgId, chunkedMsgCtx.ChunkedMessageIds));
            _pendingChunkedMessageCount--;
            compressedPayload = chunkedMsgCtx.ChunkedMsgBuffer.ToArray();
            chunkedMsgCtx.Recycle();
            var uncompressedPayload = UncompressPayloadIfNeeded(messageId, msgMetadata, compressedPayload, cnx, false);
            return uncompressedPayload;
        }

        protected internal virtual void TriggerListener(int numMessages = 0)
        {
            if (numMessages == 0)
                numMessages = IncomingMessages.Count;

            for (var i = 0; i < numMessages; i++)
            {
                if (!IncomingMessages.TryReceive(out var msg))
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{Topic}] [{Subscription}] Message has been cleared from the queue");
                    }
                    break;
                }
                try
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{Topic}][{Subscription}] Calling message listener for message {msg.MessageId}");
                    }
                    Listener.Received(_self, msg);
                }
                catch (Exception t)
                {
                    _log.Error($"[{Topic}][{Subscription}] Message listener error in processing message: {msg.MessageId} => {t}");
                }
            }
        }
        protected internal override IMessage<T> InternalReceive()
        {
            IMessage<T> message;
            try
            {
                if (IncomingMessages.Count == 0)
                {
                    ExpectMoreIncomingMessages();
                    return null;
                }
                IncomingMessages.TryReceive(out message);
                MessageProcessed(message);
                if (!IsValidConsumerEpoch(message))
                {
                    return InternalReceive();
                }
                return BeforeConsume(message);
            }
            catch (Exception e)
            {
                Stats.IncrementNumReceiveFailed();
                throw Unwrap(e);
            }
        }
        private bool IsValidConsumerEpoch(IMessage<T> message)
        {
            return base.IsValidConsumerEpoch((Message<T>)message);
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
                    MessageProcessed(message);
                    if (!IsValidConsumerEpoch(message))
                    {
                        PendingReceives.Enqueue(result);
                        result.Task.ContinueWith(s =>
                        {
                            if (s.IsCanceled)
                                PendingReceives.TryDequeue(out result);
                        });
                        return;
                    }
                    result.SetResult(BeforeConsume(message));
                }
            });

            return result;
        }

        protected internal override IMessage<T> InternalReceive(TimeSpan timeOut)
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
                if (message == null)
                {
                    return null;
                }
                MessageProcessed(message);
                if (!IsValidConsumerEpoch(message))
                {
                    var executionTime = NanoTime() - callTime;
                    var timeoutInNanos = timeOut.TotalMilliseconds;
                    if (executionTime >= timeoutInNanos)
                    {
                        return null;
                    }
                    else
                    {
                        return InternalReceive(TimeSpan.FromMilliseconds(timeoutInNanos - executionTime));
                    }
                }
                return BeforeConsume(message);
            }
            catch (Exception e)
            {
                if (State.ConnectionState != HandlerState.State.Closing && State.ConnectionState != HandlerState.State.Closed)
                {
                    Stats.IncrementNumReceiveFailed();
                    throw Unwrap(e);
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
            var result = new TaskCompletionSource<IMessages<T>>();
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
                            MessageProcessed(msg);
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
            });
            return result;
        }

        internal virtual void ReceiveIndividualMessagesFromBatch(BrokerEntryMetadata brokerEntryMetadata, MessageMetadata msgMetadata, int redeliveryCount, IList<long> ackSet, byte[] payload, MessageIdData messageId, IActorRef cnx, long consumerEpoch)
        {
            var batchSize = msgMetadata.NumMessagesInBatch;
            // create ack tracker for entry aka batch
            var batchMessage = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, PartitionIndex);
            IList<IMessage<T>> possibleToDeadLetter = null;
            if (_deadLetterPolicy != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
            {
                possibleToDeadLetter = new List<IMessage<T>>();
            }

            var acker = BatchMessageAcker.NewAcker(batchSize);
            BitSet ackBitSet = null;
            if (ackSet != null && ackSet.Count > 0)
            {
                ackBitSet = BitSet.ValueOf(ackSet.ToArray());
            }

            using var stream = new MemoryStream(payload);
            using var binaryReader = new BinaryReader(stream);
            var skippedMessages = 0;
            try
            {
                for (var i = 0; i < batchSize; ++i)
                {
                    var message = NewSingleMessage(i, batchSize, brokerEntryMetadata, msgMetadata, stream, binaryReader, new MessageId((long)messageId.ledgerId, (long)messageId.entryId, i), Schema, true, ackBitSet, acker, redeliveryCount, consumerEpoch);

                    if (message == null)
                    {
                        ++skippedMessages;
                        continue;
                    }
                    _ = EnqueueMessageAndCheckBatchReceive(message);
                }
                if (ackBitSet != null)
                {
                    ackBitSet = null;
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"[{Subscription}] [{ConsumerName}] unable to obtain message in batch: {ex}");
                DiscardCorruptedMessage(messageId, cnx, ValidationError.BatchDeSerializeError);
            }

            if (possibleToDeadLetter != null && _possibleSendToDeadLetterTopicMessages != null)
            {
                _possibleSendToDeadLetterTopicMessages[batchMessage] = possibleToDeadLetter;
            }

            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{Subscription}] [{ConsumerName}] enqueued messages in batch. queue size - {IncomingMessages.Count}, available queue size - ({IncomingMessages.Count})");
            }

            if (skippedMessages > 0)
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
            return _startMessageId != null && messageId.ledgerId == (ulong)_startMessageId.LedgerId && messageId.entryId == (ulong)_startMessageId.EntryId;
        }
        private bool IsSameEntry(MessageId MessageId)
        {
            return _startMessageId != null && MessageId.LedgerId == _startMessageId.LedgerId && MessageId.EntryId == _startMessageId.EntryId;
        }
        /// <summary>
        /// Record the event that one message has been processed by the application.
        /// 
        /// Periodically, it sends a Flow command to notify the broker that it can push more messages
        /// </summary>
        protected internal override void MessageProcessed(IMessage<T> msg)
        {
            var currentCnx = _clientCnx;
            var msgCnx = ((Message<T>)msg).Cnx();
            _lastDequeuedMessageId = msg.MessageId;

            if (msgCnx != currentCnx)
            {
                // The processed message did belong to the old queue that was cleared after reconnection.
                return;
            }
            else
            {
                if (Listener == null && !_parentConsumerHasListener)
                {
                    IncreaseAvailablePermits(currentCnx);
                }
                Stats.UpdateNumMsgsReceived(msg);

                TrackMessage(msg);
            }
            DecreaseIncomingMessageSize(msg);
        }

        protected internal virtual void TrackMessage(IMessage<T> msg)
        {
            if (msg != null)
            {
                TrackMessage(msg.MessageId, msg.RedeliveryCount);
            }
        }

        protected internal virtual void TrackMessage(IMessageId messageId)
        {
            TrackMessage(messageId, 0);
        }

        protected internal virtual void TrackMessage(IMessageId messageId, int redeliveryCount)
        {
            if (Conf.AckTimeout > TimeSpan.Zero)
            {
                MessageId id;
                if (messageId is BatchMessageId msgId)
                {
                    // do not add each item in batch message into tracker
                    id = new MessageId(msgId.LedgerId, msgId.EntryId, PartitionIndex);
                }
                else
                    id = (MessageId)messageId;

                if (HasParentConsumer)
                {
                    //TODO: check parent consumer here
                    // we should no longer track this message, TopicsConsumer will take care from now onwards
                    _unAckedMessageTracker.Tell(new Remove(id));
                }
                else
                {
                    _unAckedMessageTracker.Tell(new Add(id, redeliveryCount));
                }
            }
        }
        internal virtual void IncreaseAvailablePermits(Message<T> msg)
        {
            var currentCnx = _clientCnx;
            var msgCnx = msg.Cnx();
            if (msgCnx == currentCnx)
            {
                IncreaseAvailablePermits(currentCnx);
            }
        }
        internal virtual void IncreaseAvailablePermits(IActorRef currentCnx)
        {
            IncreaseAvailablePermits(currentCnx, 1);
        }

        protected internal virtual void IncreaseAvailablePermits(IActorRef currentCnx, int delta)
        {
            _availablePermits += delta;
            var available = _availablePermits;

            while (available >= _receiverQueueRefillThreshold && !Paused)
            {
                if (_availablePermits == available)
                {
                    _availablePermits = 0;
                    SendFlowPermitsToBroker(currentCnx, available);
                    break;
                }
                available = _availablePermits;
            }
        }

        private void IncreaseAvailablePermits(int delta)
        {
            var cnx = _clientCnx;
            IncreaseAvailablePermits(cnx, delta);
        }

        internal override void Pause()
        {
            Paused = true;
        }

        internal override void Resume()
        {
            if (Paused)
            {
                var cnx = _clientCnx;
                Paused = false;
                IncreaseAvailablePermits(cnx, 0);
            }
        }

        internal override long LastDisconnectedTimestamp()
        {
            var response = _connectionHandler.Ask<LastConnectionClosedTimestampResponse>(LastConnectionClosedTimestamp.Instance).GetAwaiter().GetResult();
            return response.TimeStamp;
        }

        private byte[] DecryptPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, byte[] payload, IActorRef currentCnx)
        {

            if (msgMetadata.EncryptionKeys.Count == 0)
            {
                return payload;
            }

            // If KeyReader is not configured throw exception based on config param
            if (Conf.CryptoKeyReader == null)
            {
                switch (Conf.CryptoFailureAction)
                {
                    case ConsumerCryptoFailureAction.Consume:
                        _log.Warning($"[{Topic}][{Subscription}][{ConsumerName}] CryptoKeyReader interface is not implemented. Consuming encrypted message.");
                        return payload;
                    case ConsumerCryptoFailureAction.Discard:
                        _log.Warning($"[{Topic}][{Subscription}][{ConsumerName}] Skipping decryption since CryptoKeyReader interface is not implemented and config is set to discard");
                        DiscardMessage(messageId, currentCnx, ValidationError.DecryptionError);
                        return null;
                    case ConsumerCryptoFailureAction.Fail:
                        var m = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex);
                        _log.Error($"[{Topic}][{Subscription}][{ConsumerName}][{m}] Message delivery failed since CryptoKeyReader interface is not implemented to consume encrypted message");
                        _unAckedMessageTracker.Tell(new Add(m));
                        return null;
                }
            }

            var decryptedData = _msgCrypto.Decrypt(msgMetadata, payload, Conf.CryptoKeyReader);
            if (decryptedData != null)
            {
                return decryptedData;
            }

            switch (Conf.CryptoFailureAction)
            {
                case ConsumerCryptoFailureAction.Consume:
                    // Note, batch message will fail to consume even if config is set to consume
                    _log.Warning($"[{Topic}][{Subscription}][{ConsumerName}][{messageId}] Decryption failed. Consuming encrypted message since config is set to consume.");
                    return payload;
                case ConsumerCryptoFailureAction.Discard:
                    _log.Warning($"[{Topic}][{Subscription}][{ConsumerName}][{messageId}] Discarding message since decryption failed and config is set to discard");
                    DiscardMessage(messageId, currentCnx, ValidationError.DecryptionError);
                    return null;
                case ConsumerCryptoFailureAction.Fail:
                    var m = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, _partitionIndex);
                    _log.Error($"[{Topic}][{Subscription}][{ConsumerName}][{m}] Message delivery failed since unable to decrypt incoming message");
                    _unAckedMessageTracker.Tell(new Add(m));
                    return null;
            }
            return null;
        }

        private byte[] UncompressPayloadIfNeeded(MessageIdData messageId, MessageMetadata msgMetadata, byte[] payload, IActorRef currentCnx, bool checkMaxMessageSize)
        {
            var compressionType = msgMetadata.Compression;
            var codec = CompressionCodecProvider.GetCompressionCodec((int)compressionType);
            var uncompressedSize = (int)msgMetadata.UncompressedSize;
            var payloadSize = payload.Length;

            var maxMessageSize = _maxMessageSize;
            if (checkMaxMessageSize && payloadSize > maxMessageSize)
            {
                // payload size is itself corrupted since it cannot be bigger than the MaxMessageSize
                _log.Error($"[{Topic}][{Subscription}] Got corrupted payload message size {payloadSize} at {messageId}");
                DiscardCorruptedMessage(messageId, currentCnx, ValidationError.UncompressedSizeCorruption);
                return null;
            }
            try
            {
                var uncompressedPayload = codec.Decode(payload, uncompressedSize);
                return uncompressedPayload;
            }
            catch (Exception e)
            {
                _log.Error($"[{Topic}][{Subscription}] Failed to decompress message with {compressionType} at {messageId}: {e}");
                DiscardCorruptedMessage(messageId, currentCnx, ValidationError.DecompressionError);
                return null;
            }
        }

        private void DiscardCorruptedMessage(MessageIdData messageId, IActorRef currentCnx, ValidationError validationError)
        {
            _log.Error($"[{Topic}][{Subscription}] Discarding corrupted message at {messageId.ledgerId}:{messageId.entryId}");
            DiscardMessage(messageId, currentCnx, validationError);
        }

        private void DiscardCorruptedMessage(MessageId messageId, IActorRef currentCnx, ValidationError validationError)
        {
            _log.Error($"[{Topic}][{Subscription}] Discarding corrupted message at {messageId.LedgerId}:{messageId.EntryId}");
            DiscardMessage(messageId, currentCnx, validationError);
        }
        private void DiscardMessage(MessageIdData messageId, IActorRef currentCnx, ValidationError validationError)
        {
            var cmd = Commands.NewAck(_consumerId, (long)messageId.ledgerId, (long)messageId.entryId, null, AckType.Individual, validationError, new Dictionary<string, long>());
            currentCnx.Tell(new Payload(cmd, -1, "NewAck"));
            IncreaseAvailablePermits(currentCnx);
            Stats.IncrementNumReceiveFailed();
        }
        private void DiscardMessage(MessageId messageId, IActorRef currentCnx, ValidationError validationError)
        {
            var cmd = Commands.NewAck(_consumerId, messageId.LedgerId, messageId.EntryId, null, AckType.Individual, validationError, new Dictionary<string, long>());
            currentCnx.Tell(new Payload(cmd, -1, "NewAck"));
            IncreaseAvailablePermits(currentCnx);
            Stats.IncrementNumReceiveFailed();
        }

        internal string HandlerName
        {
            get
            {
                return Subscription;
            }
        }

        internal override bool Connected()
        {
            return _clientCnx != null && State.ConnectionState == HandlerState.State.Ready;
        }

        internal virtual int PartitionIndex
        {
            get
            {
                return _partitionIndex;
            }
        }

        internal override int AvailablePermits()
        {
            return _availablePermits;
        }
        internal override int NumMessagesInQueue()
        {
            return IncomingMessages.Count;
        }

        protected internal override void RedeliverUnacknowledgedMessages()
        {
            // First : synchronized in order to handle consumer reconnect produce race condition, when broker receive
            // redeliverUnacknowledgedMessages and consumer have not be created and
            // then receive reconnect epoch change the broker is smaller than the client epoch, this will cause client epoch
            // smaller than broker epoch forever. client will not receive message anymore.
            // Second : we should synchronized `ClientCnx cnx = cnx()` to
            // prevent use old cnx to send redeliverUnacknowledgedMessages to a old broker
            var cnx = _clientCnx;
            var protocolVersion = _protocolVersion;
            if (Connected() && protocolVersion >= (int)ProtocolVersion.V2)
            {
                var currentSize = IncomingMessages.Count;
                //possible deadlocks here
                ClearIncomingMessages();
                _unAckedMessageTracker.Tell(new Clear());
                var cmd = Commands.NewRedeliverUnacknowledgedMessages(_consumerId);
                var payload = new Payload(cmd, -1, "NewRedeliverUnacknowledgedMessages");
                cnx.Tell(payload);

                // we should increase epoch every time, because MultiTopicsConsumerImpl also increase it,
                // we need to keep both epochs the same
                if (Conf.SubscriptionType == SubType.Failover || Conf.SubscriptionType == SubType.Exclusive)
                {
                    ConsumerEpoch += 1;
                }

                if (currentSize > 0)
                    IncreaseAvailablePermits(cnx, currentSize);

                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{Subscription}] [{Topic}] [{ConsumerName}] Redeliver unacked messages and send {currentSize} permits");
                }
            }
            else
            {
                if (cnx == null || State.ConnectionState == HandlerState.State.Connecting)
                {
                    _log.Warning($"[{Self}] Client Connection needs to be established for redelivery of unacknowledged messages");
                }
                else
                {
                    _log.Warning($"[{Self}] Reconnecting the client to redeliver the messages.");
                    cnx.Tell(PoisonPill.Instance);
                }
            }
        }

        private int ClearIncomingMessagesAndGetMessageNumber()
        {
            var messagesNumber = IncomingMessages.Count;
            ClearIncomingMessages();
            _unAckedMessageTracker.Tell(Clear.Instance);

            return messagesNumber;
        }

        protected internal override void RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds)
        {
            if (messageIds.Count == 0)
            {
                return;
            }

            Condition.CheckArgument(messageIds.First() is MessageId);

            if (Conf.SubscriptionType != SubType.Shared && Conf.SubscriptionType != SubType.KeyShared)
            {
                // We cannot redeliver single messages if subscription type is not Shared
                RedeliverUnacknowledgedMessages();
                return;
            }
            var cnx = _clientCnx;
            var protocolVersion = _protocolVersion;
            if (Connected() && protocolVersion >= (int)ProtocolVersion.V2)
            {
                var messagesFromQueue = RemoveExpiredMessagesFromQueue(messageIds);

                var batches = messageIds.PartitionMessageId(MaxRedeliverUnacknowledged);
                foreach (var ids in batches)
                {
                    GetRedeliveryMessageIdData(ids).AsTask().ContinueWith(task =>
                    {
                        var messageIdData = task.Result;
                        if (messageIdData.Count > 0)
                        {
                            var cmd = Commands.NewRedeliverUnacknowledgedMessages(_consumerId, messageIdData);
                            var payload = new Payload(cmd, -1, "NewRedeliverUnacknowledgedMessages");
                            cnx.Tell(payload);
                        }

                    });

                }
                if (messagesFromQueue > 0)
                {
                    IncreaseAvailablePermits(cnx, messagesFromQueue);
                }
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{Subscription}] [{Topic}] [{ConsumerName}] Redeliver unacked messages and increase {messagesFromQueue} permits");
                }
                return;
            }
            if (cnx == null || State.ConnectionState == HandlerState.State.Connecting)
            {
                _log.Warning($"[{Self}] Client Connection needs to be established for redelivery of unacknowledged messages");
            }
            else
            {
                _log.Warning($"[{Self}] Reconnecting the client to redeliver the messages.");
                cnx.Tell(PoisonPill.Instance);
            }
        }
        private async ValueTask<IList<MessageIdData>> GetRedeliveryMessageIdData(IList<MessageId> messageIds)
        {
            IList<MessageIdData> data = new List<MessageIdData>(messageIds.Count);
            foreach (var messageId in messageIds)
            {
                await ProcessPossibleToDLQ(messageId).AsTask();
                var msgIdData = new MessageIdData
                {
                    Partition = messageId.PartitionIndex,
                    ledgerId = (ulong)messageId.LedgerId,
                    entryId = (ulong)messageId.EntryId
                };
                data.Add(msgIdData);
            }
            return data;
        }
        private async ValueTask ProcessPossibleToDLQ(IMessageId messageId)
        {
            IList<IMessage<T>> deadLetterMessages = null;

            if (_possibleSendToDeadLetterTopicMessages != null)
            {
                if (messageId is BatchMessageId bmid)
                {
                    deadLetterMessages = _possibleSendToDeadLetterTopicMessages.GetValueOrNull(new MessageId(bmid.LedgerId, bmid.EntryId, PartitionIndex));
                }
                else
                {
                    deadLetterMessages = _possibleSendToDeadLetterTopicMessages.GetValueOrNull(messageId);
                }
            }
            if (deadLetterMessages != null)
            {
                try
                {
                    InitDeadLetterProducerIfNeeded();
                    var finalDeadLetterMessages = deadLetterMessages;
                    var finalMessageId = messageId;
                    foreach (var message in finalDeadLetterMessages)
                    {
                        try
                        {
                            var originMessageIdStr = GetOriginMessageIdStr(message);
                            var originTopicNameStr = GetOriginTopicNameStr(message);
                            var messageIdInDLQ = await _deadLetterProducer
                                .NewMessage(ISchema<object>
                                .AutoProduceBytes(message.ReaderSchema.Value))
                                .Value(message.Data.ToArray())
                                .Properties(GetPropertiesMap(message, originMessageIdStr, originTopicNameStr))
                                .SendAsync();

                            _possibleSendToDeadLetterTopicMessages.Remove(finalMessageId);
                            var r = await DoAcknowledgeWithTxn(messageId, AckType.Individual, new Dictionary<string, long>(), null).Task;
                        }
                        catch (Exception ex)
                        {
                            _log.Warning($"[{Topic}] [{Subscription}] [{ConsumerName}] Failed to send DLQ message to {finalMessageId} for message id {ex}");
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error($"Send to dead letter topic exception with topic: {_deadLetterPolicy.DeadLetterTopic}, messageId: {messageId} => {e}");
                    _deadLetterProducer = null;
                }
            }
        }
        private SortedDictionary<string, string> GetPropertiesMap(IMessage<T> message, string originMessageIdStr, string originTopicNameStr)
        {
            var propertiesMap = new SortedDictionary<string, string>();
            if (message.Properties != null)
            {
                propertiesMap = new SortedDictionary<string, string>(message.Properties);
            }
            if (!propertiesMap.ContainsKey(RetryMessageUtil.SystemPropertyRealTopic))
                propertiesMap.Add(RetryMessageUtil.SystemPropertyRealTopic, originTopicNameStr);

            if (!propertiesMap.ContainsKey(RetryMessageUtil.SystemPropertyOriginMessageId))
                propertiesMap.Add(RetryMessageUtil.SystemPropertyOriginMessageId, originMessageIdStr);

            return propertiesMap;
        }
        private string GetOriginMessageIdStr(IMessage<T> message)
        {
            if (message is TopicMessage<T> m)
            {
                return ((TopicMessageId)m.MessageId).InnerMessageId.ToString();
            }
            else if (message is Message<T> msg)
            {
                return msg.MessageId.ToString();
            }
            return null;
        }

        private string GetOriginTopicNameStr(IMessage<T> message)
        {
            if (message is TopicMessage<T> m)
            {
                return ((TopicMessageId)m.MessageId).TopicName;
            }
            else if (message is Message<T> msg)
            {
                return msg.Topic;
            }
            return null;
        }
        private void InitDeadLetterProducerIfNeeded()
        {
            if (_deadLetterProducer == null)
            {
                var builder = new ProducerConfigBuilder<byte[]>()
                       .Topic(_deadLetterPolicy.DeadLetterTopic)
                       .InitialSubscriptionName(_deadLetterPolicy.InitialSubscriptionName)
                       .EnableBatching(false);
                var client = new PulsarClient(_client, _lookup, _cnxPool, _generator, _clientConfigurationData, Context.System, null);

                _deadLetterProducer = client.NewProducer(ISchema<byte>.AutoProduceBytes(Schema), builder);

            }
        }
        internal override async ValueTask Seek(IMessageId messageId)
        {
            var seekBy = $"the message {messageId}";
            if (SeekCheckState(seekBy))
            {
                var result = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
                var requestId = result.Id;
                var seek = ReadOnlySequence<byte>.Empty;
                if (messageId is BatchMessageId msgId)
                {
                    // Initialize ack set
                    var ackSet = BitSet.Create();
                    ackSet.Set(0, msgId.BatchSize);
                    ackSet.Clear(0, Math.Max(msgId.BatchIndex, 0));
                    var ackSetArr = ackSet.ToLongArray();

                    seek = Commands.NewSeek(_consumerId, requestId, msgId.LedgerId, msgId.EntryId, ackSetArr.ToList());
                }
                else
                {
                    var msgid = (MessageId)messageId;
                    seek = Commands.NewSeek(_consumerId, requestId, msgid.LedgerId, msgid.EntryId, new List<long> { 0 });
                }
                await SeekInternal(requestId, seek, messageId, seekBy);
            }
        }

        internal override async ValueTask Seek(long timestamp)
        {
            var seekBy = $"the timestamp {timestamp:D}";
            if (SeekCheckState(seekBy))
            {
                var result = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
                var requestId = result.Id;
                await SeekInternal(requestId, Commands.NewSeek(_consumerId, requestId, timestamp), IMessageId.Earliest, seekBy);
            };
        }

        private async ValueTask SeekInternal(long requestId, ReadOnlySequence<byte> seek, IMessageId seekId, string seekBy)
        {
            var cnx = _clientCnx;

            var originSeekMessageId = _seekMessageId;
            _seekMessageId = new BatchMessageId((MessageId)seekId);
            _duringSeek = true;
            _log.Info($"[{Topic}][{Subscription}] Seeking subscription to {seekBy}");

            try
            {
                var ask = await cnx.Ask(new SendRequestWithId(seek, requestId));
                _log.Info($"[{Topic}][{Subscription}] Successfully reset subscription to {seekBy}");
                _acknowledgmentsGroupingTracker.Tell(FlushAndClean.Instance);
                _lastDequeuedMessageId = IMessageId.Earliest;
                IncomingMessages.Empty();
            }
            catch (Exception ex)
            {
                _seekMessageId = originSeekMessageId;
                _duringSeek = false;
                _log.Error($"[{Topic}][{Subscription}] Failed to reset subscription: {ex}");
                throw Wrap(ex, $"Failed to seek the subscription {Subscription} of the topic {Topic} to {seekBy}");

            }
        }
        private bool SeekCheckState(string seekBy)
        {
            if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
            {
                throw new AlreadyClosedException($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {_topicName} to {seekBy}");

            }

            if (!Connected())
            {
                throw new PulsarClientException($"The client is not connected to the broker when seeking the subscription {Subscription} of the topic {_topicName} to {seekBy}");

            }

            return true;
        }
        private async ValueTask<bool> HasMessageAvailableAsync()
        {
            if (_lastDequeuedMessageId == IMessageId.Earliest)
            {
                // if we are starting from latest, we should seek to the actual last message first.
                // allow the last one to be read when read head inclusively.
                if (_startMessageId.Equals(IMessageId.Latest))
                {
                    var lastMessageIdResponse = await InternalGetLastMessageIdAsync();
                    if (_resetIncludeHead)
                    {
                        await Seek(lastMessageIdResponse.LastMessageId);
                    }
                    var id = lastMessageIdResponse.LastMessageId;
                    var lastMessageId = MessageId.ConvertToMessageId(id);
                    var markDeletePosition = MessageId.ConvertToMessageId(id);
                    if (markDeletePosition != null)
                    {
                        var result = markDeletePosition.CompareTo(lastMessageId);
                        if (lastMessageId.EntryId < 0)
                        {
                            return CompletehasMessageAvailableWithValue(false);
                        }
                        else
                        {
                            return CompletehasMessageAvailableWithValue(_resetIncludeHead ? result <= 0 : result < 0);
                        }
                    }
                    else if (lastMessageId == null || lastMessageId.EntryId < 0)
                    {
                        return CompletehasMessageAvailableWithValue(false);
                    }
                    else
                    {
                        return CompletehasMessageAvailableWithValue(_resetIncludeHead);
                    }
                }
                if (HasMoreMessages(_lastMessageIdInBroker, _startMessageId, _resetIncludeHead))
                {
                    return CompletehasMessageAvailableWithValue(true);
                }
                var messageId = await LastMessageId();

                _lastMessageIdInBroker = messageId;
                if (HasMoreMessages(_lastMessageIdInBroker, _startMessageId, _resetIncludeHead))
                {
                    return CompletehasMessageAvailableWithValue(true);
                }
                else
                {
                    return CompletehasMessageAvailableWithValue(false);
                }
            }
            else
            {
                // read before, use lastDequeueMessage for comparison
                if (HasMoreMessages(_lastMessageIdInBroker, _lastDequeuedMessageId, false))
                {
                    return CompletehasMessageAvailableWithValue(true);
                }
                var messageId = await LastMessageId();
                _lastMessageIdInBroker = messageId;
                if (HasMoreMessages(_lastMessageIdInBroker, _lastDequeuedMessageId, false))
                {
                    return CompletehasMessageAvailableWithValue(true);
                }
                else
                {
                    return CompletehasMessageAvailableWithValue(false);
                }
            }
        }

        private bool HasMoreMessages(IMessageId lastMessageIdInBroker, IMessageId messageId, bool inclusive)
        {
            if (inclusive && lastMessageIdInBroker.CompareTo(messageId) >= 0 && ((MessageId)lastMessageIdInBroker).EntryId != -1)
            {
                return true;
            }

            if (!inclusive && lastMessageIdInBroker.CompareTo(messageId) > 0 && ((MessageId)lastMessageIdInBroker).EntryId != -1)
            {
                return true;
            }

            return false;
        }

        private async ValueTask<IMessageId> LastMessageId()
        {
            var id = await InternalGetLastMessageIdAsync();
            return id.LastMessageId;
        }
        private bool CompletehasMessageAvailableWithValue(bool value)
        {
            return value;
        }
        private async ValueTask<GetLastMessageIdResponse> InternalGetLastMessageIdAsync()
        {

            if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
            {
                throw new AlreadyClosedException($"The consumer {ConsumerName} was already closed when the subscription {Subscription} of the topic {_topicName} getting the last message id");
            }

            var opTimeoutMs = _clientConfigurationData.OperationTimeout;
            var backoff = new BackoffBuilder().SetInitialTime(TimeSpan.FromMilliseconds(100)).SetMax(opTimeoutMs.Multiply(2)).SetMandatoryStop(TimeSpan.FromMilliseconds(0)).Create();

            return await InternalGetLastMessageIdAsync(backoff, (long)opTimeoutMs.TotalMilliseconds);
        }
        private async ValueTask<GetLastMessageIdResponse> InternalGetLastMessageIdAsync(Backoff backoff, long remainingTim)
        {
            ///todo: add response to queue, where there is a retry, add something in the queue so that client knows we are 
            ///retrying in times delay
            ///
            var rmTime = remainingTim;
            while (true)
            {
                var cnx = _clientCnx;
                if (Connected() && cnx != null)
                {
                    var protocolVersion = _protocolVersion;
                    if (!Commands.PeerSupportsGetLastMessageId(protocolVersion))
                    {
                        throw new PulsarClientException.NotSupportedException($"The command `GetLastMessageId` is not supported for the protocol version {protocolVersion:D}. The consumer is {base.ConsumerName}, topic {_topicName}, subscription {base.Subscription}");
                    }

                    var res = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
                    var requestId = res.Id;
                    var getLastIdCmd = Commands.NewGetLastMessageId(_consumerId, requestId);
                    _log.Info($"[{Topic}][{Subscription}] Get topic last message Id");
                    var payload = new Payload(getLastIdCmd, requestId, "NewGetLastMessageId");
                    try
                    {
                        var result = await cnx.Ask<LastMessageIdResponse>(payload);
                        IMessageId lastMessageId;
                        var markDeletePosition = new MessageId(result.MarkDeletePosition.LedgerId, result.MarkDeletePosition.EntryId, -1);
                        _log.Info($"[{Topic}][{Subscription}] Successfully getLastMessageId {result.LedgerId}:{result.EntryId}");
                        if (result.BatchIndex < 0)
                        {
                            lastMessageId = new MessageId(result.LedgerId, result.EntryId, result.Partition);
                        }
                        else
                        {
                            lastMessageId = new BatchMessageId(result.LedgerId, result.EntryId, result.Partition, result.BatchIndex);

                        }

                        return new GetLastMessageIdResponse(lastMessageId, markDeletePosition);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"[{Topic}][{Subscription}] Failed getLastMessageId command");
                        throw Wrap(ex, $"The subscription {Subscription} of the topic {_topicName} gets the last message id was failed");
                    }
                }
                else
                {
                    var nextDelay = Math.Min(backoff.Next(), rmTime);
                    if (nextDelay <= 0)
                    {
                        throw new PulsarClientException.TimeoutException($"The subscription {Subscription} of the topic {_topicName} could not get the last message id " + "withing configured timeout");

                    }
                    _log.Warning("[{}] [{}] Could not get connection while getLastMessageId -- Will try again in {} ms", Topic, HandlerName, nextDelay);
                    await Task.Delay(TimeSpan.FromMilliseconds(nextDelay));
                    rmTime = rmTime - nextDelay;
                }
            }
        }

        private IMessageId GetMessageId<T1>(IMessage<T1> msg)
        {
            var messageId = (MessageId)msg.MessageId;
            if (messageId is BatchMessageId)
            {
                // messageIds contain MessageIdImpl, not BatchMessageIdImpl
                messageId = new MessageId(messageId.LedgerId, messageId.EntryId, PartitionIndex);
            }
            return messageId;
        }


        private bool IsMessageUndecryptable(MessageMetadata msgMetadata)
        {
            return msgMetadata.EncryptionKeys.Count > 0 && Conf.CryptoKeyReader == null && Conf.CryptoFailureAction == ConsumerCryptoFailureAction.Consume;
        }

        /// <summary>
        /// Create EncryptionContext if message payload is encrypted
        /// </summary>
        /// <param name="msgMetadata"> </param>
        /// <returns> <seealso cref="Optional"/><<seealso cref="EncryptionContext"/>> </returns>
        private Option<EncryptionContext> CreateEncryptionContext(MessageMetadata msgMetadata)
        {

            EncryptionContext encryptionCtx = null;
            if (msgMetadata.EncryptionKeys.Count > 0)
            {
                encryptionCtx = new EncryptionContext();
                IDictionary<string, EncryptionContext.EncryptionKey> keys = new Dictionary<string, EncryptionContext.EncryptionKey>();
                foreach (var kv in msgMetadata.EncryptionKeys)
                {
                    var neC = new EncryptionContext.EncryptionKey
                    {
                        KeyValue = kv.Value,
                        Metadata = new Dictionary<string, string>()
                    };
                    foreach (var m in kv.Metadatas)
                    {
                        if (!neC.Metadata.ContainsKey(m.Key))
                        {
                            neC.Metadata.Add(m.Key, m.Value);
                        }
                    }

                    if (!keys.ContainsKey(kv.Key))
                    {
                        keys.Add(kv.Key, neC);
                    }
                }
                var encParam = new byte[IMessageCrypto.IV_LEN];
                msgMetadata.EncryptionParam.CopyTo(encParam, 0);
                int? batchSize = msgMetadata.NumMessagesInBatch > 0 ? msgMetadata.NumMessagesInBatch : 0;
                encryptionCtx.Keys = keys;
                encryptionCtx.Param = encParam;
                encryptionCtx.Algorithm = msgMetadata.EncryptionAlgo;
                encryptionCtx.CompressionType = (int)msgMetadata.Compression;// CompressionCodecProvider.ConvertFromWireProtocol(msgMetadata.Compression);
                encryptionCtx.UncompressedMessageSize = (int)msgMetadata.UncompressedSize;
                encryptionCtx.BatchSize = batchSize;
            }
            return Option<EncryptionContext>.Create(encryptionCtx);
        }
        private int RemoveExpiredMessagesFromQueue(ISet<IMessageId> messageIds)
        {
            var messagesFromQueue = 0;
            if (IncomingMessages.TryReceive(out var peek))
            {
                var messageId = GetMessageId(peek);
                if (!messageIds.Contains(messageId))
                {
                    // first message is not expired, then no message is expired in queue.
                    return 0;
                }

                // try not to remove elements that are added while we remove
                while (IncomingMessages.Count > 0)
                {
                    if (IncomingMessages.TryReceive(out var message))
                    {
                        IncomingMessagesSize -= message.Data.Length;
                        messagesFromQueue++;
                        var id = GetMessageId(message);
                        if (!messageIds.Contains(id))
                        {
                            messageIds.Add(id);
                            break;
                        }
                    }
                }
            }
            return messagesFromQueue;
        }

        private void SetTerminated()
        {
            _log.Info($"[{Subscription}] [{Topic}] [{ConsumerName}] Consumer has reached the end of topic");
            _hasReachedEndOfTopic = true;
            if (Listener != null)
            {
                // Propagate notification to listener
                Listener.ReachedEndOfTopic(_self);
            }
        }

        private bool HasReachedEndOfTopic()
        {
            return _hasReachedEndOfTopic;
        }


        private void ResetBackoff()
        {
            _connectionHandler.Tell(Messages.Requests.ResetBackoff.Instance);
        }

        private void ConnectionClosed(IActorRef cnx)
        {
            _connectionHandler.Tell(new ConnectionClosed(cnx));
        }

        private void SetCnx(IActorRef cnx)
        {
            if (cnx != null)
            {
                _connectionHandler.Tell(new SetCnx(cnx));
                cnx.Tell(new RegisterConsumer(_consumerId, _self));
                /*if (Conf.AckReceiptEnabled && !Commands.PeerSupportsAckReceipt(value.getRemoteEndpointProtocolVersion()))
                {
                    log.warn("Server don't support ack for receipt! " + "ProtoVersion >=17 support! nowVersion : {}", value.getRemoteEndpointProtocolVersion());
                }*/
            }
            var previousClientCnx = _clientCnxUsedForConsumerRegistration;
            _clientCnxUsedForConsumerRegistration = cnx;
            _prevconsumerId = _consumerId;
            if (previousClientCnx != null && previousClientCnx != cnx)
            {
                previousClientCnx.Tell(new RemoveConsumer(_prevconsumerId));
            }
        }

        internal virtual void DeregisterFromClientCnx()
        {
            SetCnx(null);
        }

        private void ReconnectLater(Exception exception)
        {
            _connectionHandler.Tell(new ReconnectLater(exception));
        }

        protected override void Unhandled(object message)
        {
            _log.Warning($"Unhandled Message '{message.GetType().FullName}' from '{Sender.Path}'");
        }
        internal virtual string TopicNameWithoutPartition
        {
            get
            {
                return _topicNameWithoutPartition;
            }
        }


        private void RemoveOldestPendingChunkedMessage()
        {
            ChunkedMessageCtx chunkedMsgCtx = null;
            string firstPendingMsgUuid = null;
            while (chunkedMsgCtx == null && _pendingChunckedMessageUuidQueue.Count > 0)
            {
                // remove oldest pending chunked-message group and free memory
                firstPendingMsgUuid = _pendingChunckedMessageUuidQueue.Dequeue();
                chunkedMsgCtx = !string.IsNullOrWhiteSpace(firstPendingMsgUuid) ? _chunkedMessagesMap[firstPendingMsgUuid] : null;
            }
            RemoveChunkMessage(firstPendingMsgUuid, chunkedMsgCtx, _autoAckOldestChunkedMessageOnQueueFull);
        }

        private void RemoveExpireIncompleteChunkedMessages()
        {
            if (ExpireTimeOfIncompleteChunkedMessage <= TimeSpan.Zero)
            {
                return;
            }
            ChunkedMessageCtx chunkedMsgCtx = null;

            while (_pendingChunckedMessageUuidQueue.TryDequeue(out var messageUUID))
            {
                chunkedMsgCtx = !string.IsNullOrWhiteSpace(messageUUID) ? _chunkedMessagesMap[messageUUID] : null;
                if (chunkedMsgCtx != null && DateTimeHelper.CurrentUnixTimeMillis() > chunkedMsgCtx.ReceivedTime + ExpireTimeOfIncompleteChunkedMessage.TotalMilliseconds)
                {
                    _pendingChunckedMessageUuidQueue = new Queue<string>(_pendingChunckedMessageUuidQueue.Where(x => !x.Equals(messageUUID)));
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
            if (chunkedMsgCtx == null)
            {
                return;
            }
            // clean up pending chuncked-Message
            _chunkedMessagesMap.Remove(msgUUID);
            if (chunkedMsgCtx.ChunkedMessageIds != null)
            {
                foreach (var msgId in chunkedMsgCtx.ChunkedMessageIds)
                {
                    if (msgId == null)
                    {
                        continue;
                    }
                    if (autoAck)
                    {
                        _log.Info("Removing chunk message-id {}", msgId);
                        DoAcknowledge(msgId, AckType.Individual, new Dictionary<string, long>(), null);
                    }
                    else
                    {
                        TrackMessage(msgId);
                    }
                }
            }
            if (chunkedMsgCtx.ChunkedMsgBuffer != null)
            {
                chunkedMsgCtx.ChunkedMsgBuffer = new List<byte>();
            }
            chunkedMsgCtx.Recycle();
            _pendingChunkedMessageCount--;
        }

        protected internal override void UpdateAutoScaleReceiverQueueHint()
        {
            var prev = ScaleReceiverQueueHint.GetAndSet(AvailablePermits() + IncomingMessages.Count >= CurrentReceiverQueueSize);
            if (_log.IsDebugEnabled && prev != ScaleReceiverQueueHint)
            {
                _log.Debug($"updateAutoScaleReceiverQueueHint {prev} -> {ScaleReceiverQueueHint}");
            }
        }
        private void DoTransactionAcknowledgeForResponse(IMessageId messageId, AckType ackType, ValidationError? validationError, IDictionary<string, long> properties, TransactionImpl.TxnID txnID, long requestId)
        {
            long ledgerId;
            long entryId;
            ReadOnlySequence<byte> cmd;
            if (messageId is BatchMessageId batchMessageId)
            {
                var bitSet = new BitSet(batchMessageId.BatchSize);
                ledgerId = batchMessageId.LedgerId;
                entryId = batchMessageId.EntryId;
                if (ackType == AckType.Cumulative)
                {
                    batchMessageId.AckCumulative();
                    bitSet.Set(0, batchMessageId.BatchSize);
                    bitSet.Clear(0, batchMessageId.BatchIndex + 1);
                }
                else
                {
                    bitSet.Set(0, batchMessageId.BatchSize);
                    bitSet.Clear(batchMessageId.BatchIndex);
                }
                cmd = Commands.NewAck(_consumerId, ledgerId, entryId, bitSet.ToLongArray().ToList(), ackType, validationError, properties, txnID.LeastSigBits, txnID.MostSigBits, requestId, batchMessageId.BatchSize);
            }
            else
            {
                var singleMessage = (MessageId)messageId;
                ledgerId = singleMessage.LedgerId;
                entryId = singleMessage.EntryId;
                cmd = Commands.NewAck(_consumerId, ledgerId, entryId, new List<long> { }, ackType, validationError, properties, txnID.LeastSigBits, txnID.MostSigBits, requestId);
            }

            _ackRequests.Add(requestId, (new List<IMessageId> { messageId }, txnID));
            if (ackType == AckType.Cumulative)
            {
                _unAckedMessageTracker.Tell(new RemoveMessagesTill(messageId));
            }
            else
            {
                _unAckedMessageTracker.Tell(new Remove(messageId));
            }
            var payload = new Payload(cmd, requestId, "NewAckForReceipt");
            _clientCnx.Tell(payload);
        }
        private void DoTransactionAcknowledgeForResponse(IList<IMessageId> messageIds, AckType ackType, ValidationError? validationError, IDictionary<string, long> properties, TransactionImpl.TxnID txnID, long requestId)
        {
            long ledgerId;
            long entryId;
            ReadOnlySequence<byte> cmd;
            IList<MessageIdData> messageIdDataList = new List<MessageIdData>();
            var msgIds = new List<IMessageId>();
            foreach (var messageId in messageIds)
            {
                msgIds.Add(messageId);
                if (messageId is BatchMessageId batchMessageId)
                {
                    var bitSet = new BitSet(batchMessageId.BatchSize);
                    ledgerId = batchMessageId.LedgerId;
                    entryId = batchMessageId.EntryId;
                    if (ackType == AckType.Cumulative)
                    {
                        batchMessageId.AckCumulative();
                        bitSet.Set(0, batchMessageId.BatchSize);
                        bitSet.Clear(0, batchMessageId.BatchIndex + 1);
                    }
                    else
                    {
                        bitSet.Set(0, batchMessageId.BatchSize);
                        bitSet.Clear(batchMessageId.BatchIndex);
                    }
                    var messageIdData = new MessageIdData
                    {
                        ledgerId = (ulong)batchMessageId.LedgerId,
                        entryId = (ulong)batchMessageId.EntryId,
                        BatchSize = batchMessageId.BatchSize
                    };
                    var s = bitSet.ToLongArray();
                    for (var i = 0; i < s.Length; i++)
                    {
                        messageIdData.AckSets.Append(s[i]);
                    }
                    messageIdDataList.Add(messageIdData);
                }
                else
                {
                    var singleMessage = (MessageId)messageId;
                    ledgerId = singleMessage.LedgerId;
                    entryId = singleMessage.EntryId;
                    var messageIdData = new MessageIdData
                    {
                        ledgerId = (ulong)ledgerId,
                        entryId = (ulong)entryId
                    };
                    messageIdDataList.Add(messageIdData);
                }

                if (ackType == AckType.Cumulative)
                {
                    _unAckedMessageTracker.Tell(new RemoveMessagesTill(messageId));
                }
                else
                {
                    _unAckedMessageTracker.Tell(new Remove(messageId));
                }
            }
            _ackRequests.Add(requestId, (msgIds, txnID));
            cmd = Commands.NewAck(_consumerId, messageIdDataList, ackType, validationError, properties, txnID.LeastSigBits, txnID.MostSigBits, requestId);
            var payload = new Payload(cmd, requestId, "NewAckForReceipt");
            _clientCnx.Tell(payload);
        }

        private void AckReceipt(long requestId)
        {
            if (_ackRequests.TryGetValue(requestId, out var ot))
            {
                _ = _ackRequests.Remove(requestId);
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"MessageId : {ot.messageid} has ack by TxnId : {ot.txnid}");
                }
            }
            else
            {
                _log.Info($"Ack request has been handled requestId : {requestId}");
            }
        }

        private void AckError(long requestId, PulsarClientException pulsarClientException)
        {
            if (_ackRequests.TryGetValue(requestId, out var ot))
            {
                _ = _ackRequests.Remove(requestId);
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"MessageId : {ot.messageid} has ack by TxnId : {ot.txnid}");
                }
            }
            else
            {
                _log.Info($"Ack request has been handled requestId : {requestId}");
            }
            //ConsumerQueue.AcknowledgeException.Add(new ClientExceptions(pulsarClientException));
        }
        internal IActorRef Cnx()
        {
            return _clientCnx;
        }
    }
    internal class ChunkedMessageCtx
    {

        protected internal int TotalChunks = -1;
        protected internal List<byte> ChunkedMsgBuffer;
        protected internal int LastChunkedMessageId = -1;
        protected internal MessageId[] ChunkedMessageIds;
        protected internal long ReceivedTime = 0;

        internal static ChunkedMessageCtx Get(int numChunksFromMsg, List<byte> chunkedMsgBuffer)
        {
            var ctx = new ChunkedMessageCtx
            {
                TotalChunks = numChunksFromMsg,
                ChunkedMsgBuffer = chunkedMsgBuffer,
                ChunkedMessageIds = new MessageId[numChunksFromMsg],
                ReceivedTime = DateTimeHelper.CurrentUnixTimeMillis()
            };
            return ctx;
        }

        internal virtual void Recycle()
        {
            TotalChunks = -1;
            ChunkedMsgBuffer = null;
            LastChunkedMessageId = -1;
        }
    }
}