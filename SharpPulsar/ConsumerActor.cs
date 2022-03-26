using Akka.Actor;
using Akka.Util;
using Akka.Util.Internal;
using ProtoBuf;
using SharpPulsar.Auth;
using SharpPulsar.Batch;
using SharpPulsar.Builder;
using SharpPulsar.Common;
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
using SharpPulsar.Transaction;
using SharpPulsar.User;
using SharpPulsar.Utils;
using System;
using System.Buffers;
using System.Collections;
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
		private readonly bool _hasParentConsumer;

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
        private readonly bool _poolMessages = false;


        private readonly ActorSystem _actorSystem;

		private readonly SubscriptionInitialPosition _subscriptionInitialPosition;
		private readonly IActorRef _connectionHandler;
		private readonly IActorRef _generator;

		private readonly Dictionary<long, (IMessageId messageid, TxnID txnid)> _ackRequests;

		private readonly TopicName _topicName;
		private readonly string _topicNameWithoutPartition;

		private readonly IDictionary<IMessageId, IList<IMessage<T>>> _possibleSendToDeadLetterTopicMessages;

		private readonly DeadLetterPolicy _deadLetterPolicy;

		private Producer<byte[]> _deadLetterProducer;

		private int _maxMessageSize;
		private int _protocolVersion;

		private volatile Producer<T> _retryLetterProducer;
		private volatile IActorRef _replyTo;

        private long _subscribeDeadline = 0; // gets set on first successful connection

        protected internal bool Paused;

		private readonly Dictionary<string, ChunkedMessageCtx> _chunkedMessagesMap = new Dictionary<string, ChunkedMessageCtx>();
		private int _pendingChunckedMessageCount = 0;
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

        public ConsumerActor(long consumerId, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfigurationData, TaskCompletionSource<IActorRef> subscribeFuture) :this
			(consumerId, stateActor, client, lookup, cnxPool, idGenerator, topic, conf, partitionIndex, hasParentConsumer, startMessageId, 0, schema, createTopicIfDoesNotExist, clientConfigurationData, subscribeFuture)
		{
		}

		public ConsumerActor(long consumerId, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, long startMessageRollbackDurationInSec, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture) : base(stateActor, lookup, cnxPool, topic, conf, conf.ReceiverQueueSize, schema, subscribeFuture)
		{
            Paused = conf.StartPaused;
			_context = Context;
			_clientConfigurationData = clientConfiguration;
			_ackRequests = new Dictionary<long, (IMessageId messageid, TxnID txnid)>();
			_generator = idGenerator;
			_topicName = TopicName.Get(topic);
			_cnxPool = cnxPool;
			_actorSystem = Context.System;
			_lookup = lookup;
			_self = Self;
			_tokenSource = new CancellationTokenSource();
			_client = client;
			_consumerId = consumerId;
			_subscriptionMode = conf.SubscriptionMode;
			_startMessageId = startMessageId != null ? new BatchMessageId((MessageId) startMessageId) : null;
			_initialStartMessageId = _startMessageId;
			_startMessageRollbackDurationInSec = startMessageRollbackDurationInSec;
			_availablePermits = 0;
			_subscribeTimeout = DateTimeHelper.CurrentUnixTimeMillis() + (long)clientConfiguration.OperationTimeout.TotalMilliseconds;
			_partitionIndex = partitionIndex;
			_hasParentConsumer = hasParentConsumer;
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

			if(clientConfiguration.StatsIntervalSeconds.TotalMilliseconds > 0)
			{
				_stats = new ConsumerStatsRecorder<T>(Context.System, conf, _topicName.ToString(), ConsumerName, Subscription, clientConfiguration.StatsIntervalSeconds);
			}
			else
			{
				_stats = ConsumerStatsDisabled.Instance;
			}

			_duringSeek = false;

			if(conf.AckTimeout.TotalMilliseconds > 0)
			{
				if(conf.TickDuration.TotalMilliseconds > 0)
				{
					_unAckedMessageTracker = Context.ActorOf(UnAckedMessageTracker.Prop(conf.AckTimeout, conf.TickDuration > conf.AckTimeout? conf.AckTimeout: conf.TickDuration, _self, UnAckedChunckedMessageIdSequenceMap), "UnAckedMessageTracker");
				}
				else
				{
					_unAckedMessageTracker = Context.ActorOf(UnAckedMessageTracker.Prop(conf.AckTimeout, TimeSpan.Zero, _self, UnAckedChunckedMessageIdSequenceMap), "UnAckedMessageTracker");
				}
			}
			else
			{
				_unAckedMessageTracker = Context.ActorOf(UnAckedMessageTrackerDisabled.Prop(), "UnAckedMessageTrackerDisabled");
			}

			_negativeAcksTracker = Context.ActorOf(NegativeAcksTracker<T>.Prop(conf, _self, UnAckedChunckedMessageIdSequenceMap));
			// Create msgCrypto if not created already
			if (conf.CryptoKeyReader != null)
			{
				if(conf.MessageCrypto != null)
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
					catch(Exception e)
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

			if(conf.Properties.Count == 0)
			{
				_metadata = ImmutableDictionary.Create<string,string>();
			}
			else
			{
				_metadata = new Dictionary<string,string>(conf.Properties).ToImmutableDictionary();
			}

			_connectionHandler = Context.ActorOf(ConnectionHandler.Prop(clientConfiguration, State, new BackoffBuilder().SetInitialTime(TimeSpan.FromMilliseconds(clientConfiguration.InitialBackoffIntervalMs)).SetMax(TimeSpan.FromMilliseconds(clientConfiguration.MaxBackoffIntervalMs)).SetMandatoryStop(TimeSpan.FromMilliseconds(0)).Create(), Self));
						
			if(_topicName.Persistent)
			{
				_acknowledgmentsGroupingTracker = Context.ActorOf(PersistentAcknowledgmentsGroupingTracker<T>.Prop(UnAckedChunckedMessageIdSequenceMap, _self, idGenerator, _consumerId, _connectionHandler, conf));
			}
			else
			{
				_acknowledgmentsGroupingTracker = Context.ActorOf(NonPersistentAcknowledgmentGroupingTracker.Prop());
			}

			if(conf.DeadLetterPolicy != null)
			{
				_possibleSendToDeadLetterTopicMessages = new Dictionary<IMessageId, IList<IMessage<T>>>();
				if(!string.IsNullOrWhiteSpace(conf.DeadLetterPolicy.DeadLetterTopic))
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

				if(!string.IsNullOrWhiteSpace(conf.DeadLetterPolicy.RetryLetterTopic))
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
            
            Ready();
            GrabCnx();
		}
        public static Props Prop(long consumerId, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfigurationData, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(()=> new ConsumerActor<T>(consumerId, stateActor, client, lookup, cnxPool, idGenerator, topic, conf, partitionIndex, hasParentConsumer, startMessageId, schema, createTopicIfDoesNotExist, clientConfigurationData, subscribeFuture));
        }

        public static Props Prop(long consumerId, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ConsumerConfigurationData<T> conf, int partitionIndex, bool hasParentConsumer, IMessageId startMessageId, long startMessageRollbackDurationInSec, ISchema<T> schema, bool createTopicIfDoesNotExist, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(() => new ConsumerActor<T>(consumerId, stateActor, client, lookup, cnxPool, idGenerator, topic, conf, partitionIndex, hasParentConsumer, startMessageId, startMessageRollbackDurationInSec, schema, createTopicIfDoesNotExist, clientConfiguration, subscribeFuture));
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
                var startMessageRollbackDuration = (_startMessageRollbackDurationInSec > 0 && _startMessageId != null && _startMessageId.Equals(_initialStartMessageId)) ? _startMessageRollbackDurationInSec : 0;
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
                    var firstTimeConnect = SubscribeFuture.TrySetResult(_self);
                    if (!(firstTimeConnect && _hasParentConsumer) && Conf.ReceiverQueueSize != 0)
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
                            SubscribeFuture.TrySetException(PulsarClientException.Wrap(e, $"Failed to subscribe the topic {_topicName} with subscription name {Subscription} when connecting to the broker"));

                            _client.Tell(new CleanupConsumer(_self));
                        }
                        else if (e is PulsarClientException.TopicDoesNotExistException)
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
            Receive<BatchReceive>(_ =>
            {
                BatchReceive();
            });
            Receive<Messages.Consumer.Receive>(_ =>
            {
                Receive();
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
			Receive<ConnectionClosed>(m => {
				ConnectionClosed(m.ClientCnx);
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
			ReceiveAsync<IAcknowledge>(async ack => 
			{
                _replyTo = Sender;
				await Acknowledge(ack);
			});
			ReceiveAsync<ICumulative>( async cumulative => 
			{
                _replyTo = Sender;
                await Cumulative(cumulative);
			});

			Receive<GetLastDisconnectedTimestamp>(m =>
			{
				var last = LastDisconnectedTimestamp();
				Sender.Tell(last);
			});
			Receive<GetConsumerName>(m => {
				Sender.Tell(ConsumerName);
			});
			Receive<AckReceipt>(m => {
				AckReceipt(m.RequestId);
			});
			Receive<AckError>(m => {
				AckError(m.RequestId, m.Exception);
			});
			Receive<ActiveConsumerChanged>(m => {
				ActiveConsumerChanged(m.IsActive);
			});
			ReceiveAsync<MessageReceived>(async m => 
			{
				await MessageReceived(m);
			});
			Receive<GetSubscription>(m => {
				Sender.Tell(Subscription);
			});
			Receive<GetTopic>(m => {
                Sender.Tell(_topicName.ToString());
			});
			Receive<ClearUnAckedChunckedMessageIdSequenceMap>(_ => {
				UnAckedChunckedMessageIdSequenceMap.Tell(Clear.Instance);
			});
			Receive<HasReachedEndOfTopic>(_ => {
				var hasReached = HasReachedEndOfTopic();
                Sender.Tell(new AskResponse(hasReached));
			});
			Receive<GetAvailablePermits>( _ => {
				var permits = AvailablePermits();
				Sender.Tell(permits);
			});
			Receive<MessageProcessed<T>>(m => 
			{
                try
                {
					MessageProcessed(m.Message);
				}
				catch(Exception ex)
                {
					_log.Error($"{m}===>>>{ex}");
                }
			});
			Receive<IsConnected>( _ => {
                Sender.Tell(Connected());
			});
			Receive<Pause>(_ => {
				Pause();
			});
			ReceiveAsync<HasMessageAvailable>(async _ => {
                try
                {
                    _replyTo = Sender;
                    var has = await HasMessageAvailableAsync();
                    _replyTo.Tell(new AskResponse(has));
                }
                catch(Exception ex)
                {
                    _replyTo.Tell(new AskResponse(ex));
                    _log.Error($"[{Topic}][{Subscription}] Failed getLastMessageId command: {ex}");
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
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
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
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
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
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
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
                    Sender.Tell(PulsarClientException.Unwrap(ex));
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
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
				}
			});
			ReceiveAsync<ReconsumeLaterMessage<T>>(async m => 
			{
                try
                {
                    _replyTo = Sender;
					await ReconsumeLater(m.Message, m.DelayTime);
                    _replyTo.Tell(new AskResponse());
				}
                catch (Exception ex)
                {
                    _replyTo.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<RedeliverUnacknowledgedMessages>(m => 
			{
                RedeliverUnacknowledged();
                Sender.Tell(new AskResponse());
            });
			ReceiveAsync<RedeliverUnacknowledgedMessageIds>(async m => 
			{
                try
				{
                    await RedeliverUnacknowledged(m.MessageIds);
                    Sender.Tell(new AskResponse());
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
				}
			});
			Receive<Unsubscribe>(_ => 
			{
                try
                {
					Unsubscribe();
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
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

		private async ValueTask Acknowledge(IAcknowledge ack)
        {
            try
            {
				switch (ack)
				{
					case AcknowledgeMessage<T> m:
					    await DoAcknowledgeWithTxn(m.Message.MessageId, AckType.Individual, _properties, null).Task;
						break;
					case AcknowledgeMessageId id:
                        await DoAcknowledgeWithTxn(id.MessageId, AckType.Individual, _properties, null).Task;
						break;
					case AcknowledgeMessageIds ids:
						await DoAcknowledgeWithTxn (ids.MessageIds, AckType.Individual, _properties, null).Task;
						break;
					case AcknowledgeWithTxnMessages mTxn:
						await DoAcknowledgeWithTxn(mTxn.MessageIds, mTxn.AckType, mTxn.Properties, mTxn.Txn).Task;
						break;
					case AcknowledgeWithTxn txn:
                        await DoAcknowledgeWithTxn(txn.MessageId, txn.AckType, txn.Properties, txn.Txn).Task;
						break;
					case AcknowledgeMessages<T> ms:
						foreach (var x in ms.Messages)
						{
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

		private async ValueTask Cumulative(ICumulative cumulative)
		{
            try
            {
                if (!IsCumulativeAcknowledgementAllowed(Conf.SubscriptionType))
                {
                    _replyTo.Tell(new AskResponse(new PulsarClientException.InvalidConfigurationException(
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
						await DoAcknowledgeWithTxn (ack.MessageId, AckType.Cumulative, _properties, ack.Txn).Task;
						break;
					case ReconsumeLaterCumulative<T> ack:
                        await DoReconsumeLater(ack.Message, AckType.Cumulative, _properties, ack.DelayTime).Task;
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
		private TaskCompletionSource<object> DoAcknowledgeWithTxn(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        {
            var ackFuture = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (txn != null)
            {
                txn.Ask(new RegisterAckedTopic(Topic, Subscription)).ContinueWith(task =>
                {
                    var msgidList = messageIdList;
                    if (task.Exception != null)
                    {
                        DoAcknowledge(msgidList, ackType, properties, txn);
                        ackFuture.TrySetResult(null);
                    }
                    else
                        ackFuture.TrySetException(task.Exception);

                });
            }			
            else
            {
                DoAcknowledge(messageIdList, ackType, properties, txn);
                ackFuture.TrySetResult(null);
            }
            return ackFuture;
		}
		private void Unsubscribe()
		{
			if(State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
                Sender.Tell(new AskResponse(new AlreadyClosedException("AlreadyClosedException: Consumer was already closed")));
			}
            else
            {
				if (Connected())
				{
					State.ConnectionState = HandlerState.State.Closing;
					var res =  _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).GetAwaiter().GetResult();
					var requestId =  res.Id;
					var unsubscribe = Commands.NewUnsubscribe(_consumerId, requestId);
				    var cnx = _clientCnx;
					cnx.Tell(new SendRequestWithId(unsubscribe, requestId)); 
					CloseConsumerTasks();
					DeregisterFromClientCnx();
					_client.Tell(new CleanupConsumer(Self));
					_log.Info($"[{Topic}][{Subscription}] Successfully unsubscribed from topic");
					State.ConnectionState = HandlerState.State.Closed;
                    Sender.Tell(new AskResponse());
				}
				else
				{
                    var err = $"The client is not connected to the broker when unsubscribing the subscription {Subscription} of the topic {_topicName}";

                    Sender.Tell(new AskResponse(new PulsarClientException(err)));
					_log.Error(err);
				}
			}
		}
        
        protected override void PostStop()
        {
			_tokenSource.Cancel();
			if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				CloseConsumerTasks();
			}

			if (!Connected())
			{
				_log.Info($"[{Topic}] [{Subscription}] Closed Consumer (not connected)");
				State.ConnectionState = HandlerState.State.Closed;
				CloseConsumerTasks();
				DeregisterFromClientCnx();
				_client.Tell(new CleanupConsumer(Self));
			}

			_stats.StatTimeout?.Cancel();

			State.ConnectionState = HandlerState.State.Closing;

			CloseConsumerTasks();

			var requestId = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).GetAwaiter().GetResult().Id;
            try
            {
				var cnx = _clientCnx;
				if (null == cnx)
				{
					CleanupAtClose(null);
				}
				else
				{
					var cmd = Commands.NewCloseConsumer(_consumerId, requestId);
					cnx.Tell(new SendRequestWithId(cmd, requestId));
				}

			}
            catch { }
			base.PostStop();
        }
		internal override IConsumerStatsRecorder Stats
		{
			get
			{
				return _stats;
			}
		}

		private async Task ReconsumeLater(IMessage<T> message, TimeSpan delayTime)
		{
			if (!Conf.RetryEnable)
			{
				throw new PulsarClientException("reconsumeLater method not support!");
			}
			try
			{
				var tcs = DoReconsumeLater(message, AckType.Individual, new Dictionary<string, long>(), delayTime);
                await tcs.Task;
			}
			catch (Exception e)
			{
				var t = e.InnerException;
				if (t is PulsarClientException)
				{
					throw (PulsarClientException)t;
				}
				else
				{
					throw new PulsarClientException(t);
				}
			}
		}

		private void ReconsumeLater(IMessages<T> messages, TimeSpan delayTime)
		{
			try
			{
				messages.ForEach(async message => await ReconsumeLater(message, delayTime));
			}
			catch (NullReferenceException npe)
			{
				throw new PulsarClientException.InvalidMessageException(npe.Message);
			}
		}

		private void DoAcknowledge(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
		{
			Condition.CheckArgument(messageId is MessageId);
			if(State.ConnectionState != HandlerState.State.Ready && State.ConnectionState != HandlerState.State.Connecting)
			{
				Stats.IncrementNumAcksFailed();
				var exception = new PulsarClientException("Consumer not ready. State: " + State);
				if(AckType.Individual.Equals(ackType))
				{
					OnAcknowledge(messageId, exception);
				}
				else if(AckType.Cumulative.Equals(ackType))
				{
					OnAcknowledgeCumulative(messageId, exception);
				}
				return;
			}

			if(txn != null)
			{
				var requestId = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).GetAwaiter().GetResult();
				var bits = txn.Ask<GetTxnIdBitsResponse>(GetTxnIdBits.Instance).GetAwaiter().GetResult();
				DoTransactionAcknowledgeForResponse(messageId, ackType, null, properties, new TxnID(bits.MostBits, bits.LeastBits), requestId.Id);
                return;
            }
            _acknowledgmentsGroupingTracker.Tell(new AddAcknowledgment(messageId, ackType, properties));
        }
		private void DoAcknowledge(IList<IMessageId> messageIdList, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
		{
            _acknowledgmentsGroupingTracker.Tell(new AddListAcknowledgment(messageIdList, ackType, properties));
		}
        private TaskCompletionSource<object> DoReconsumeLater(IMessage<T> message, AckType ackType, IDictionary<string, long> properties, TimeSpan delayTime)
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
			if(State.ConnectionState != HandlerState.State.Ready && State.ConnectionState != HandlerState.State.Connecting)
			{
				Stats.IncrementNumAcksFailed();
				var exception = new PulsarClientException("Consumer not ready. State: " + State);
				if(AckType.Individual.Equals(ackType))
				{
					OnAcknowledge(messageId, exception);
				}
				else if(AckType.Cumulative.Equals(ackType))
				{
					OnAcknowledgeCumulative(messageId, exception);
				}
                result.TrySetException(exception);

                return result;
			}
			if(delayTime.TotalMilliseconds < 0)
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
                        catch(Exception ex)
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
                            if(task.Exception != null)
                            {
                                result.TrySetException(task.Exception);
                            }
                            else
                            {
                                try
                                {
                                    DoAcknowledge(finalMessageId, ackType, properties, null);
                                    result.TrySetResult(null);
                                }
                                catch(Exception ex)
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
                                    DoAcknowledge(finalMessageId, ackType, properties, null);
                                    result.TrySetResult(null);
                                }
                                catch(Exception ex)
                                {
                                    result.TrySetException(ex);
                                }
                            });
					}
				}
				catch(Exception e)
				{
					_log.Error($"Send to retry letter topic exception with topic: {_deadLetterPolicy.DeadLetterTopic}, messageId: {messageId}: {e}");
                    ISet<IMessageId> messageIds = new HashSet<IMessageId>
                    {
                        messageId
                    };
                    _unAckedMessageTracker.Tell(new Remove(messageId));
					Akka.Dispatch.ActorTaskScheduler.RunTask(async ()=> await RedeliverUnacknowledged(messageIds));
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
                Akka.Dispatch.ActorTaskScheduler.RunTask(async () => await RedeliverUnacknowledged(messageIds));
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
				if(IncomingMessages.TryReceive(out var m))
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

			if(currentMessageQueue.Count > 0)
			{
				var nextMessageInQueue = currentMessageQueue[0].MessageId;
				BatchMessageId previousMessage;
				if(nextMessageInQueue is BatchMessageId next)
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
			else if(!_lastDequeuedMessageId.Equals(IMessageId.Earliest))
			{
				// If the queue was empty we need to restart from the message just after the last one that has been dequeued
				// in the past
				return new BatchMessageId((MessageId) _lastDequeuedMessageId);
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
			if(cnx != null && numMessages > 0)
			{
				if(_log.IsDebugEnabled)
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
			var nonRetriableError = !PulsarClientException.IsRetriableError(exception);
			var timeout = DateTimeHelper.CurrentUnixTimeMillis() > _subscribeTimeout;
			if((nonRetriableError || timeout))
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
            
		}

		private void CloseConsumerTasks()
		{
			_unAckedMessageTracker.GracefulStop(TimeSpan.FromSeconds(3));
			if(_possibleSendToDeadLetterTopicMessages != null)
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
			if(ConsumerEventListener == null)
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
				&& Conf.SubscriptionType != CommandSubscribe.SubType.Shared;

			var msgId = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, PartitionIndex);
			var decryptedPayload = DecryptPayloadIfNeeded(messageId, msgMetadata, data, _clientCnx);
            var isMessageUndecryptable = IsMessageUndecryptable(msgMetadata);
            if (decryptedPayload == null)
            {
                // Message was discarded or CryptoKeyReader isn't implemented
                return;
            }

			
            // uncompress decryptedPayload and release decryptedPayload-ByteBuf
            var uncompressedPayload = (isMessageUndecryptable || isChunkedMessage) ? decryptedPayload : UncompressPayloadIfNeeded(messageId, msgMetadata, decryptedPayload, _clientCnx, true);


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
            if (isMessageUndecryptable || (numMessages == 1 && !HasNumMessagesInBatch(msgMetadata)))
            {
                // right now, chunked messages are only supported by non-shared subscription
                if (isChunkedMessage)
                {
                    uncompressedPayload = ProcessMessageChunk(uncompressedPayload, msgMetadata, msgId, messageId, _clientCnx);
                    if (uncompressedPayload == null)
                    {
                        return;
                    }
                }

                if (_topicName.Persistent && IsSameEntry(messageId) && IsPriorEntryIndex((long)messageId.entryId))
                {
                    // We need to discard entries that were prior to startMessageId
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{Subscription}] [{ConsumerName}] Ignoring message from before the startMessageId: {_startMessageId}");
                    }
                    return;
                }
                var message = Message<T>.Create(_topicName.ToString(), msgId, msgMetadata, new ReadOnlySequence<byte>(uncompressedPayload), CreateEncryptionContext(msgMetadata), _clientCnx, Schema, redeliveryCount, _poolMessages, consumerEpoch);
                message.BrokerEntryMetadata = received.BrokerEntryMetadata;
                try
                {
                    // Enqueue the message so that it can be retrieved when application calls receive()
                    // if the conf.getReceiverQueueSize() is 0 then discard message if no one is waiting for it.
                    // if asyncReceive is waiting then notify callback without adding to incomingMessages queue
                    if (_deadLetterPolicy != null && _possibleSendToDeadLetterTopicMessages != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
                    {
                        _possibleSendToDeadLetterTopicMessages[(MessageId)message.MessageId] = new List<IMessage<T>> { message };
                    }
                }
                finally
                {
                    EnqueueMessageAndCheckBatchReceive(message);
                }
            }
            else
            {
                // handle batch message enqueuing; uncompressed payload has all messages in batch
                ReceiveIndividualMessagesFromBatch(brokerEntryMetadata, msgMetadata, redeliveryCount, ackSet, uncompressedPayload, messageId, _clientCnx, consumerEpoch);

            }

            if (Listener != null)
            {
                TriggerListener(numMessages);
            }
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

                var payloadBuffer = (singleMessagePayload != null) ? singleMessagePayload : payload.ToArray();
                
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
            try
            {
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
                        EnqueueMessageAndCheckBatchReceive(message);
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

        private void TryTriggerListener()
        {
            if (Listener != null)
            {
                TriggerListener();
            }
        }
        private bool HasNumMessagesInBatch(MessageMetadata m)
		{
			var should = m.ShouldSerializeNumMessagesInBatch();
			return should;
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
				_pendingChunckedMessageCount++;
				if (_maxPendingChuckedMessage > 0 && _pendingChunckedMessageCount > _maxPendingChuckedMessage)
				{
					RemoveOldestPendingChunkedMessage();
				}
				_pendingChunckedMessageUuidQueue.Enqueue(msgMetadata.Uuid);
			}

			ChunkedMessageCtx chunkedMsgCtx = null;
			if (_chunkedMessagesMap.ContainsKey(msgMetadata.Uuid))
				chunkedMsgCtx = _chunkedMessagesMap[msgMetadata.Uuid];

			// discard message if chunk is out-of-order
			if (chunkedMsgCtx == null || chunkedMsgCtx.ChunkedMsgBuffer == null || msgMetadata.ChunkId != (chunkedMsgCtx.LastChunkedMessageId + 1) || msgMetadata.ChunkId >= msgMetadata.TotalChunkMsgSize)
			{
				// means we lost the first chunk: should never happen
				_log.Info($"Received unexpected chunk messageId {msgId}, last-chunk-id{chunkedMsgCtx?.LastChunkedMessageId ?? 0}, chunkId = {msgMetadata.ChunkId}, total-chunks {msgMetadata.TotalChunkMsgSize}");
				chunkedMsgCtx?.Recycle();
				_chunkedMessagesMap.Remove(msgMetadata.Uuid);
				IncreaseAvailablePermits(cnx);
				if(ExpireTimeOfIncompleteChunkedMessage.TotalMilliseconds > 0 && DateTimeHelper.CurrentUnixTimeMillis() > ((long)msgMetadata.PublishTime + ExpireTimeOfIncompleteChunkedMessage.TotalMilliseconds))
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
			if (msgMetadata.ChunkId != (msgMetadata.NumChunksFromMsg - 1))
			{
				IncreaseAvailablePermits(cnx);
				return null;
			}

            
			// last chunk received: so, stitch chunked-messages and clear up chunkedMsgBuffer
			if(_log.IsDebugEnabled)
			{
				_log.Debug($"Chunked message completed chunkId {msgMetadata.ChunkId}, total-chunks {msgMetadata.NumChunksFromMsg}, msgId {msgId} sequenceId {msgMetadata.SequenceId}");
			}
			// remove buffer from the map, add chucked messageId to unack-message tracker, and reduce pending-chunked-message count
			_chunkedMessagesMap.Remove(msgMetadata.Uuid);
			UnAckedChunckedMessageIdSequenceMap.Tell(new AddMessageIds(msgId, chunkedMsgCtx.ChunkedMessageIds));
			_pendingChunckedMessageCount--;
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
		private void ReceiveIndividualMessagesFromBatch(BrokerEntryMetadata brokerEntryMetadata, MessageMetadata msgMetadata, int redeliveryCount, IList<long> ackSet, byte[] payload, MessageIdData messageId, IActorRef cnx, long consumerEpoch)
		{
			var batchSize = msgMetadata.NumMessagesInBatch;
			// create ack tracker for entry aka batch
			var batchMessage = new MessageId((long)messageId.ledgerId, (long)messageId.entryId, PartitionIndex);
			IList<IMessage<T>> possibleToDeadLetter = null;
			if(_deadLetterPolicy != null && redeliveryCount >= _deadLetterPolicy.MaxRedeliverCount)
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
			catch(Exception ex)
			{
				_log.Warning($"[{Subscription}] [{ConsumerName}] unable to obtain message in batch: {ex}");
				DiscardCorruptedMessage(messageId, cnx, ValidationError.BatchDeSerializeError);
			}

			if(possibleToDeadLetter != null && _possibleSendToDeadLetterTopicMessages != null)
			{
				_possibleSendToDeadLetterTopicMessages[batchMessage] = possibleToDeadLetter;
			}

			if(_log.IsDebugEnabled)
			{
				_log.Debug($"[{Subscription}] [{ConsumerName}] enqueued messages in batch. queue size - {IncomingMessages.Count}, available queue size - (-1)");
			}

			if(skippedMessages > 0)
			{
				IncreaseAvailablePermits(cnx, skippedMessages);
			}
		}
		private bool EnqueueMessageAndCheckBatchReceive(IMessage<T> message)
		{
			if (CanEnqueueMessage(message))
			{
				Push(message);
			}
			return true;
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
        private void MessageProcessed(IMessage<T> msg)
		{
			var currentCnx = _clientCnx;
			var msgCnx = ((Message<T>)msg).Cnx();
			_lastDequeuedMessageId = msg.MessageId;

			if (msgCnx != currentCnx)
			{
				// The processed message did belong to the old queue that was cleared after reconnection.
				return;
			}

			IncreaseAvailablePermits(currentCnx);
			Stats.UpdateNumMsgsReceived(msg);

			TrackMessage(msg);
			IncomingMessagesSize -= msg.Data.Length;
		}

		protected internal virtual void TrackMessage(IMessage<T> msg)
		{
			if(msg != null)
			{
				TrackMessage(msg.MessageId);
			}
		}

		protected internal virtual void TrackMessage(IMessageId messageId)
		{
			if(Conf.AckTimeout > TimeSpan.Zero)
			{
                MessageId id;
                if (messageId is BatchMessageId msgId)
				{
					// do not add each item in batch message into tracker
					id = new MessageId(msgId.LedgerId, msgId.EntryId, PartitionIndex);
				}
				else
					id = (MessageId)messageId;

				if (_hasParentConsumer)
				{
					//TODO: check parent consumer here
					// we should no longer track this message, TopicsConsumer will take care from now onwards
					_unAckedMessageTracker.Tell(new Remove(id));
				}
				else
				{
					_unAckedMessageTracker.Tell(new Add(id));
				}
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

			while(available >= _receiverQueueRefillThreshold && !Paused)
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
			if(Paused)
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

			if(msgMetadata.EncryptionKeys.Count == 0)
			{
				return payload;
			}

			// If KeyReader is not configured throw exception based on config param
			if(Conf.CryptoKeyReader == null)
			{
				switch(Conf.CryptoFailureAction)
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
			if(decryptedData != null)
			{
				return decryptedData;
			}

			switch(Conf.CryptoFailureAction)
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
			catch(Exception e)
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
			var cmd = Commands.NewAck(_consumerId, (long)messageId.LedgerId, (long)messageId.EntryId, null, AckType.Individual, validationError, new Dictionary<string, long>());
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
			return _clientCnx != null && (State.ConnectionState == HandlerState.State.Ready);
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

		private void RedeliverUnacknowledged()
		{
			var cnx = _clientCnx;
			var protocolVersion = _protocolVersion;
			if (Connected() && protocolVersion >= (int)ProtocolVersion.V2)
			{
				var currentSize = IncomingMessages.Count;
				//possible deadlocks here
				IncomingMessages.Empty();
				IncomingMessagesSize = 0;
				_unAckedMessageTracker.Tell(new Clear());
				var cmd = Commands.NewRedeliverUnacknowledgedMessages(_consumerId);
				var payload = new Payload(cmd, -1, "NewRedeliverUnacknowledgedMessages");
				cnx.Tell(payload);

				if(currentSize > 0)
					IncreaseAvailablePermits(cnx, currentSize);

				if (_log.IsDebugEnabled)
				{
					_log.Debug($"[{Subscription}] [{Topic}] [{ConsumerName}] Redeliver unacked messages and send {currentSize} permits");
				}
			}
            else
            {
                if (cnx == null || (State.ConnectionState == HandlerState.State.Connecting))
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
			IncomingMessages.Empty();
			IncomingMessagesSize = 0;
			_unAckedMessageTracker.Tell(Clear.Instance);

			return messagesNumber;
		}

		protected internal override async Task RedeliverUnacknowledged(ISet<IMessageId> messageIds)
		{
			if(messageIds.Count == 0)
			{
				return;
			}

			Condition.CheckArgument(messageIds.First() is MessageId);

			if(Conf.SubscriptionType != CommandSubscribe.SubType.Shared && Conf.SubscriptionType != CommandSubscribe.SubType.KeyShared)
			{
				// We cannot redeliver single messages if subscription type is not Shared
				RedeliverUnacknowledged();
				return;
			}
			var cnx = _clientCnx;
			var protocolVersion = _protocolVersion;
			if (Connected() && protocolVersion >= (int)ProtocolVersion.V2)
			{
				var messagesFromQueue = RemoveExpiredMessagesFromQueue(messageIds);

				var batches = messageIds.PartitionMessageId(MaxRedeliverUnacknowledged);
				foreach(var ids in batches)
                {
                    var messageIdData = await GetRedeliveryMessageIdData(ids);
                    if (messageIdData.Count > 0)
                    {
                        var cmd = Commands.NewRedeliverUnacknowledgedMessages(_consumerId, messageIdData);
                        var payload = new Payload(cmd, -1, "NewRedeliverUnacknowledgedMessages");
                        cnx.Tell(payload);
                    }
                }
				if(messagesFromQueue > 0)
				{
					IncreaseAvailablePermits(cnx, messagesFromQueue);
				}
				if(_log.IsDebugEnabled)
				{
					_log.Debug($"[{Subscription}] [{Topic}] [{ConsumerName}] Redeliver unacked messages and increase {messagesFromQueue} permits");
				}
				return;
			}
			if(cnx == null || (State.ConnectionState == HandlerState.State.Connecting))
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
				if(messageId is BatchMessageId bmid)
				{
					deadLetterMessages = _possibleSendToDeadLetterTopicMessages.GetValueOrNull(new MessageId(bmid.LedgerId, bmid.EntryId, PartitionIndex));
				}
				else
				{
					deadLetterMessages = _possibleSendToDeadLetterTopicMessages.GetValueOrNull(messageId);
				}
			}
            if(deadLetterMessages != null)
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
                            var r =  await DoAcknowledgeWithTxn(messageId, AckType.Individual, new Dictionary<string, long>(), null).Task;                            
                        }
                        catch(Exception ex)
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

                    seek = Commands.NewSeek(_consumerId, requestId, msgId.LedgerId, msgId.EntryId, ackSetArr);
                }
                else
                {
                    var msgid = (MessageId)messageId;
                    seek = Commands.NewSeek(_consumerId, requestId, msgid.LedgerId, msgid.EntryId, new long[0]);
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
            catch(Exception ex)
            {
                _seekMessageId = originSeekMessageId;
                _duringSeek = false;
                _log.Error($"[{Topic}][{Subscription}] Failed to reset subscription: {ex}");
                throw PulsarClientException.Wrap(ex, $"Failed to seek the subscription {Subscription} of the topic {Topic} to {seekBy}");

            }
        }
        private bool SeekCheckState(string seekBy)
        {
            if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
            {
                throw new PulsarClientException.AlreadyClosedException($"The consumer {ConsumerName} was already closed when seeking the subscription {Subscription} of the topic {_topicName} to {seekBy}");
                
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
			if(inclusive && lastMessageIdInBroker.CompareTo(messageId) >= 0 && ((MessageId)lastMessageIdInBroker).EntryId != -1)
			{
				return true;
			}

			if(!inclusive && lastMessageIdInBroker.CompareTo(messageId) > 0 && ((MessageId)lastMessageIdInBroker).EntryId != -1)
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
				throw new PulsarClientException.AlreadyClosedException($"The consumer {ConsumerName} was already closed when the subscription {Subscription} of the topic {_topicName} getting the last message id");
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
                        MessageId markDeletePosition = null;
                        if (result.MarkDeletePosition != null)
                        {
                            markDeletePosition = new MessageId(result.MarkDeletePosition.LedgerId, result.MarkDeletePosition.EntryId, -1);
                        }
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
                        throw PulsarClientException.Wrap(ex, $"The subscription {Subscription} of the topic {_topicName} gets the last message id was failed");
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
			var messageId = (MessageId) msg.MessageId;
			if(messageId is BatchMessageId)
			{
				// messageIds contain MessageIdImpl, not BatchMessageIdImpl
				messageId = new MessageId(messageId.LedgerId, messageId.EntryId, PartitionIndex);
			}
			return messageId;
		}


		private bool IsMessageUndecryptable(MessageMetadata msgMetadata)
		{
			return (msgMetadata.EncryptionKeys.Count > 0 && Conf.CryptoKeyReader == null && Conf.CryptoFailureAction == ConsumerCryptoFailureAction.Consume);
		}

		/// <summary>
		/// Create EncryptionContext if message payload is encrypted
		/// </summary>
		/// <param name="msgMetadata"> </param>
		/// <returns> <seealso cref="Optional"/><<seealso cref="EncryptionContext"/>> </returns>
		private Option<EncryptionContext> CreateEncryptionContext(MessageMetadata msgMetadata)
		{

			EncryptionContext encryptionCtx = null;
			if(msgMetadata.EncryptionKeys.Count > 0)
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
			return new Option<EncryptionContext>(encryptionCtx);
		}
        private int RemoveExpiredMessagesFromQueue(ISet<IMessageId> messageIds)
		{
			var messagesFromQueue = 0;
			if (IncomingMessages.TryReceive(out var peek))
			{
				var messageId = GetMessageId(peek);
				if(!messageIds.Contains(messageId))
				{
					// first message is not expired, then no message is expired in queue.
					return 0;
				}

				// try not to remove elements that are added while we remove
				while(IncomingMessages.Count > 0)
				{
					if(IncomingMessages.TryReceive(out var message))
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
			if(Listener != null)
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
			while(chunkedMsgCtx == null && _pendingChunckedMessageUuidQueue.Count > 0)
			{
				// remove oldest pending chunked-message group and free memory
				firstPendingMsgUuid = _pendingChunckedMessageUuidQueue.Dequeue();
				chunkedMsgCtx = !string.IsNullOrWhiteSpace(firstPendingMsgUuid) ? _chunkedMessagesMap[firstPendingMsgUuid] : null;
			}
			RemoveChunkMessage(firstPendingMsgUuid, chunkedMsgCtx, _autoAckOldestChunkedMessageOnQueueFull);
		}

		private void RemoveExpireIncompleteChunkedMessages()
		{
			if(ExpireTimeOfIncompleteChunkedMessage <= TimeSpan.Zero)
			{
				return;
			}
			ChunkedMessageCtx chunkedMsgCtx = null;
            
            while (_pendingChunckedMessageUuidQueue.TryDequeue(out var messageUUID))
			{
				chunkedMsgCtx = !string.IsNullOrWhiteSpace(messageUUID) ? _chunkedMessagesMap[messageUUID] : null;
				if(chunkedMsgCtx != null && DateTimeHelper.CurrentUnixTimeMillis() > (chunkedMsgCtx.ReceivedTime + ExpireTimeOfIncompleteChunkedMessage.TotalMilliseconds))
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
			if(chunkedMsgCtx == null)
			{
				return;
			}
			// clean up pending chuncked-Message
			_chunkedMessagesMap.Remove(msgUUID);
			if(chunkedMsgCtx.ChunkedMessageIds != null)
			{
				foreach(var msgId in chunkedMsgCtx.ChunkedMessageIds)
				{
					if(msgId == null)
					{
						continue;
					}
					if(autoAck)
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
			if(chunkedMsgCtx.ChunkedMsgBuffer != null)
			{
				chunkedMsgCtx.ChunkedMsgBuffer = new List<byte>();
			}
			chunkedMsgCtx.Recycle();
			_pendingChunckedMessageCount--;
		}
		private void Push(IMessage<T> obj)
		{
			var o = (Message<T>)obj;
			if (_hasParentConsumer)
            {
				_context.Parent.Tell(new ReceivedMessage<T>(o));
				_log.Info($"Pushed message with sequnceid {o.SequenceId} (topic:{Topic}) to consumer parent");

			}
			else
            {
				if (IncomingMessages.Post(o))
					_log.Info($"Added message with sequnceid {o.SequenceId} (key:{o.Key}) to IncomingMessages. Message Count: {IncomingMessages.Count}");
				else
					_log.Info($"Failed to add message with sequnceid {o.SequenceId} to IncomingMessages");
			}
		}
		private void DoTransactionAcknowledgeForResponse(IMessageId messageId, AckType ackType, ValidationError? validationError, IDictionary<string, long> properties, TxnID txnID, long requestId)
		{
			long ledgerId;
			long entryId;
            ReadOnlySequence<byte> cmd;
			if(messageId is BatchMessageId batchMessageId)
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
				cmd = Commands.NewAck(_consumerId, ledgerId, entryId, bitSet.ToLongArray(), ackType, validationError, properties, txnID.LeastSigBits, txnID.MostSigBits, requestId, batchMessageId.BatchSize);
			}
			else
			{
				var singleMessage = (MessageId) messageId;
				ledgerId = singleMessage.LedgerId;
				entryId = singleMessage.EntryId;
				cmd = Commands.NewAck(_consumerId, ledgerId, entryId, new long[]{ }, ackType, validationError, properties, txnID.LeastSigBits, txnID.MostSigBits, requestId);
			}

			_ackRequests.Add(requestId, (messageId, txnID));
			if(ackType == AckType.Cumulative)
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

		private TaskCompletionSource<object> DoAcknowledgeWithTxn(IMessageId messageId, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
		{
            var ackFuture = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (txn != null)
			{
				// it is okay that we register acked topic after sending the acknowledgements. because
				// the transactional ack will not be visiable for consumers until the transaction is
				// committed
				if (ackType == AckType.Cumulative)
				{
					txn.Tell(new RegisterCumulativeAckConsumer(_self));
				}
                txn.Ask(new RegisterAckedTopic(Topic, Subscription)).ContinueWith(task =>
                {
                    var msgid = messageId;
                    if (!task.IsFaulted)
                    {
                        DoAcknowledge(msgid, ackType, properties, txn);
                        ackFuture.TrySetResult(null);
                    }
                    else
                        ackFuture.TrySetException(task.Exception);

                });
            }
			else
            {
                DoAcknowledge(messageId, ackType, properties, txn);
                ackFuture.TrySetResult(null);
            }
            return ackFuture;
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
			if(_ackRequests.TryGetValue(requestId, out var ot))
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

	}

}