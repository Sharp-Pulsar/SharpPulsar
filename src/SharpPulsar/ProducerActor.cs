using Akka.Actor;
using Akka.Event;
using Akka.Util;
using App.Metrics.Concurrency;
using DotNetty.Common.Utilities;
using SharpPulsar.Batch;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common;
using SharpPulsar.Common.Compression;
using SharpPulsar.Common.Entity;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Crypto;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.Schema;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Schemas;
using SharpPulsar.Shared;
using SharpPulsar.Stats.Producer;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static SharpPulsar.Exceptions.PulsarClientException;
using static SharpPulsar.Protocol.Commands;
using JsonSerializer = System.Text.Json.JsonSerializer;
using TimeoutException = SharpPulsar.Exceptions.PulsarClientException.TimeoutException;

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

    internal class ProducerActor<T> : ProducerActorBase<T>, IWithUnboundedStash
    {
        private bool InstanceFieldsInitialized = false;

        private void InitializeInstanceFields()
        {
            _lastSendFutureWrapper = LastSendFutureWrapper.Create(_lastSendFuture);
        }
        private sealed class LastSendFutureWrapper
        {
            internal readonly Akka.Util.AtomicReference<object> ThrowOnceUpdater = new Akka.Util.AtomicReference<object>(FALSE);

            internal readonly TaskCompletionSource<IMessageId> LastSendFuture;
            internal const int FALSE = 0;
            internal const int TRUE = 1;
            internal volatile int ThrowOnce = FALSE;

            internal LastSendFutureWrapper(TaskCompletionSource<IMessageId> lastSendFuture)
            {
                this.LastSendFuture = lastSendFuture;
            }
            internal static LastSendFutureWrapper Create(TaskCompletionSource<IMessageId> lastSendFuture)
            {
                return new LastSendFutureWrapper(lastSendFuture);
            }
            public void HandleOnce()
            {
                LastSendFuture.Task.ContinueWith( t =>
                {
                    if (t.IsFaulted && ThrowOnceUpdater.CompareAndSet(FALSE, TRUE))
                    {
                        throw t.Exception;
                    }
                }); 
            }
        }
        
        // Producer id, used to identify a producer within a single connection
        private readonly long _producerId;

        // Variable is used through the atomic updater
        private long _msgIdGenerator;
        private ICancelable _sendTimeout;
        private readonly long _createProducerTimeout;
        private readonly IBatchMessageContainerBase<T> _batchMessageContainer;
        private readonly OpSendMsgQueue _pendingMessages;
        private readonly IActorRef _generator;
        private readonly Dictionary<string, IActorRef> _watchedActors = new Dictionary<string, IActorRef>();

        private readonly ICancelable _regenerateDataKeyCipherCancelable;
        private LastSendFutureWrapper _lastSendFutureWrapper;
        // Globally unique producer name
        private string _producerName;
        private readonly bool _userProvidedProducerName = false;
        private TaskCompletionSource<IMessageId> _lastSendFuture;
        private readonly Commands _commands = new Commands(); 
        private readonly ILoggingAdapter _log;

        private string _connectionId;
        private string _connectedSince;
        private readonly int _partitionIndex;
        private ISchemaInfo _schemaInfo;

        protected internal IProducerStatsRecorder _stats;

        private readonly CompressionCodec _compressor;

        private long _lastSequenceIdPublished;

        protected internal long LastSequenceIdPushed;
        private bool _isLastSequenceIdPotentialDuplicated;

        private readonly IMessageCrypto _msgCrypto;

        private readonly ICancelable _keyGeneratorTask = null;

        private readonly IDictionary<string, string> _metadata;

        private Option<byte[]> _schemaVersion = null;
        private long? _topicEpoch = null;

        private readonly IActorRef _connectionHandler;
        private readonly IActorRef _self;
        private IActorRef _sender;
        private int _protocolVersion;

        // A batch flush task is scheduled when one of the following is true:
        // - A message is added to a message batch without also triggering a flush for that batch.
        // - A batch flush task executes with messages in the batchMessageContainer, thus actually triggering messages.
        // - A message was sent more recently than the configured BatchingMaxPublishDelayMicros. In this case, the task is
        //   scheduled to run BatchingMaxPublishDelayMicros after the most recent send time.
        // The goal is to optimize batch density while also ensuring that a producer never waits longer than the configured
        // batchingMaxPublishDelayMicros to send a batch.
        // Only update from within synchronized block on this producer.
        private ICancelable _batchFlushTask;

        private ICancelable _batchTimerTask;
        private int _chunkMaxMessageSize;
        private readonly IScheduler _scheduler;
        private IActorRef _replyTo;
        private long _requestId = -1;
        private readonly bool _isTxnEnabled;
        private long _maxMessageSize;
        private IActorRef _cnx;
        private readonly TimeSpan _lookupDeadline;
        private TimeSpan _producerDeadline = TimeSpan.Zero;
        private readonly Collection<Exception> _previousExceptions = new Collection<Exception>();
        private readonly MemoryLimitController _memoryLimitController;
        // The time, in nanos, of the last batch send. This field ensures that we don't deliver batches via the
        // batchFlushTask before the batchingMaxPublishDelayMicros duration has passed.
        private long _lastBatchSendNanoTime;

        public ProducerActor(long producerid, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ProducerConfigurationData conf, TaskCompletionSource<IActorRef> producerCreatedFuture, int partitionIndex, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData clientConfiguration, Option<string> overrideProducerName) : base(client, lookup, cnxPool, topic, conf, producerCreatedFuture, schema, interceptors, clientConfiguration)
        {
            Self.Path.WithUid(producerid);
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }
            


            _memoryLimitController = new MemoryLimitController(clientConfiguration.MemoryLimitBytes);
            _isTxnEnabled = clientConfiguration.EnableTransaction;
            _self = Self;
            _generator = idGenerator;
            _scheduler = Context.System.Scheduler;
            _log = Context.GetLogger();
            _producerId = producerid;
            _producerName = conf.ProducerName;

            if (!string.IsNullOrWhiteSpace(_producerName))
            {
                _userProvidedProducerName = true;
            }
            _partitionIndex = partitionIndex;
            _pendingMessages = CreatePendingMessagesQueue();
            overrideProducerName.OnSuccess(key => _producerName = key);
            _compressor = CompressionCodecProvider.GetCompressionCodec((int)conf.CompressionType);

            if (conf.InitialSequenceId != null)
            {
                var initialSequenceId = conf.InitialSequenceId.Value;
                _lastSequenceIdPublished = initialSequenceId;
                LastSequenceIdPushed = initialSequenceId;
                _msgIdGenerator = initialSequenceId + 1L;
            }
            else
            {
                _lastSequenceIdPublished = -1L;
                LastSequenceIdPushed = -1L;
                _msgIdGenerator = 0L;
            }

            if (conf.EncryptionEnabled)
            {
                var logCtx = "[" + topic + "] [" + _producerName + "] [" + _producerId + "]";

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
                        msgCryptoBc = new MessageCrypto(logCtx, true, _log);
                    }
                    catch (Exception e)
                    {
                        _log.Error($"MessageCryptoBc may not included in the jar in Producer: {e}");
                        msgCryptoBc = null;
                    }
                    _msgCrypto = msgCryptoBc;
                }
            }
            else
            {
                _msgCrypto = null;
            }

            if (_msgCrypto != null)
            {
                // Regenerate data key cipher at fixed interval
                _regenerateDataKeyCipherCancelable = Context.System.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromHours(0), TimeSpan.FromHours(4), () =>
                {
                    try
                    {
                        _msgCrypto.AddPublicKeyCipher(conf.EncryptionKeys, conf.CryptoKeyReader);
                    }
                    catch (CryptoException e)
                    {
                        _log.Warning($"[{Topic}] [{ProducerName().GetAwaiter().GetResult()}] [{_producerId}] Failed to add public key cipher.");
                        _log.Error(e.ToString());
                    }
                });
            }

            if (conf.SendTimeoutMs.TotalMilliseconds > 0)
            {
                _sendTimeout = _scheduler.ScheduleTellOnceCancelable(conf.SendTimeoutMs, Self, RunSendTimeout.Instance, ActorRefs.NoSender);
            }

            _lookupDeadline = TimeSpan.FromMilliseconds(DateTimeHelper.CurrentUnixTimeMillis() + clientConfiguration.LookupTimeout.TotalMilliseconds);
            _connectionHandler = Context.ActorOf(ConnectionHandler.Prop(clientConfiguration, State, new BackoffBuilder().SetInitialTime(TimeSpan.FromMilliseconds(clientConfiguration.InitialBackoffIntervalMs)).SetMax(TimeSpan.FromMilliseconds(clientConfiguration.MaxBackoffIntervalMs)).SetMandatoryStop(TimeSpan.FromMilliseconds(0)).Create(), Self));

            _createProducerTimeout = DateTimeHelper.CurrentUnixTimeMillis() + (long)clientConfiguration.OperationTimeout.TotalMilliseconds;
            if (conf.BatchingEnabled)
            {
                var containerBuilder = conf.BatcherBuilder;
                if (containerBuilder == null)
                {
                    containerBuilder = IBatcherBuilder.Default(Context.GetLogger());
                }
                _batchMessageContainer = (IBatchMessageContainerBase<T>)containerBuilder.Build<T>();
                _batchMessageContainer.Producer = Self;
            }
            else
            {
                _batchMessageContainer = null;
            }
            if (clientConfiguration.StatsIntervalSeconds > TimeSpan.Zero)
            {
                _stats = new ProducerStatsRecorder(Context.System, ProducerName().GetAwaiter().GetResult(), topic, Configuration.MaxPendingMessages);
            }
            else
            {
                _stats = ProducerStatsDisabled.Instance;
            }

            if (Configuration.Properties == null)
            {
                _metadata = new Dictionary<string, string>();
            }
            else
            {
                _metadata = new SortedDictionary<string, string>(Configuration.Properties);
            }
        }
        private  OpSendMsgQueue CreatePendingMessagesQueue()
        {
            return new OpSendMsgQueue();
        }

        protected override void PreStart()
        {
            base.PreStart();

            Become(Ready);
            GrabCnx();

        }
        public static Props Prop(long producerid, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ProducerConfigurationData conf, TaskCompletionSource<IActorRef> producerCreatedFuture, int partitionIndex, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData clientConfiguration, Option<string> overrideProducerName)
        {
            return Props.Create(() => new ProducerActor<T>(producerid, client, lookup, cnxPool, idGenerator, topic, conf, producerCreatedFuture, partitionIndex, schema, interceptors, clientConfiguration, overrideProducerName));
        }
        private void GrabCnx()
        {
            _connectionHandler.Tell(new GrabCnx($"Create connection from producer: {_producerName}"));
        }
        private void Ready()
        {
            ReceiveAsync<AskResponse>(async response =>
            {
                if (response.Failed)
                {
                    ConnectionFailed(response.Exception);
                    return;
                }
                await ConnectionOpened(response.ConvertTo<ConnectionOpened>());
            });
            Receive<AckReceived>(a =>
            {
                AckReceived(a);
            });
            Receive<RunSendTimeout>(_ => {
                Run();
            });
            Receive<Messages.Terminated>(x =>
                {
                    Terminated(x.ClientCnx);
                }
            );
            ReceiveAsync<Close>(async c =>
            {
                _replyTo = Sender;
                try
                {
                    var currentState = State.GetAndUpdateState(State.ConnectionState == HandlerState.State.Closed ? HandlerState.State.Closed : HandlerState.State.Closing);

                    if (currentState == HandlerState.State.Closed || currentState == HandlerState.State.Closing)
                    {
                        _replyTo.Tell(new AskResponse());
                        return;
                    }

                    CloseProducerTasks();
                    _stats.CancelStatsTimeout();

                    var cnx = Cnx();
                    if (cnx == null || currentState != HandlerState.State.Ready)
                    {
                        _log.Info("[{}] [{}] Closed Producer (not connected)", Topic, _producerName);
                        CloseAndClearPendingMessages();
                        _replyTo.Tell(new AskResponse());
                        return;
                    }
                    var resp = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
                    var requestId = resp.Id;
                    var cmd = _commands.NewCloseProducer(_producerId, requestId);
                    var pay = new Payload(cmd, requestId, "NewCloseProducer");
                    var ask = await cnx.Ask<AskResponse>(pay).ConfigureAwait(false);
                    _log.Info($"[{Topic}] [{_producerName}] Closed Producer", Topic, _producerName);
                    CloseAndClearPendingMessages();
                    _replyTo.Tell(ask);
                    
                }
                catch (Exception ex)
                {
                    _replyTo.Tell(new AskResponse(ex));
                    _log.Error(ex.ToString());
                }
                _replyTo = null;
            });
            ReceiveAsync<RecoverChecksumError>(async x =>
                {
                   await RecoverChecksumError(x.ClientCnx, x.SequenceId);
                }
            );
            Receive<RecoverNotAllowedError>(x =>
                {
                    RecoverNotAllowedError(x.SequenceId, x.ErrorMsg);
                }
            );
            ReceiveAsync<BatchTask>(async _ => {
                await RunBatchTask();
            });
            Receive<ConnectionClosed>(m =>
            {
                ConnectionClosed(m.ClientCnx);
            });
            ReceiveAsync<Flush>(async _ => {
                await Flush();
            });
            ReceiveAsync<TriggerFlush>(async _ => {
                await TriggerFlush();
            });
            Receive<GetProducerName>(_ => Sender.Tell(_producerName));
            Receive<GetLastSequenceId>(_ => Sender.Tell(_lastSequenceIdPublished));
            Receive<GetTopic>(_ => Sender.Tell(Topic));
            Receive<IsConnected>(_ => Sender.Tell(Connected()));
            Receive<GetStats>(_ => Sender.Tell(_stats));
            Receive<GetLastDisconnectedTimestamp>(_ =>
            {
                _replyTo = Sender;
                Become(DisconnectedTimestamp);
            });
            Receive<Akka.Actor.Terminated>(t =>
            {
                _watchedActors.Remove(t.ActorRef.Path.Name);
            });
            ReceiveAsync<InternalSend<T>>(async m =>
            {
                try
                {
                    await InternalSend(m.Message, m.Callback);
                }
                catch (Exception ex)
                {
                    _log.Error(ex.ToString());
                }
            });
            ReceiveAsync<InternalSendWithTxn<T>>(async m =>
            {
                try
                {
                    await InternalSendWithTxn(m.Message, m.Txn, m.Callback);
                }
                catch (Exception ex)
                {
                    _log.Error(ex.ToString());
                }
            });
            Stash?.UnstashAll();
        }
        private async ValueTask ConnectionOpened(ConnectionOpened o)
        {
            _previousExceptions.Clear();
            // we set the cnx reference before registering the producer on the cnx, so if the cnx breaks before creating the
            // producer, it will try to grab a new cnx
            // Because the state could have been updated while retrieving the connection, we set it back to connecting,
            // as long as the change from current state to connecting is a valid state change.
            //if (!State.ChangeToConnecting())
            //{
             //   return;
            //}
            // We set the cnx reference before registering the producer on the cnx, so if the cnx breaks before creating
            // the producer, it will try to grab a new cnx. We also increment and get the epoch value for the producer.
            var epochResponse =  await _connectionHandler.Ask<GetEpochResponse> (new SwitchClientCnx(o.ClientCnx));
            var epoch = epochResponse.Epoch;
            //_connectionHandler.Tell(new SetCnx(o.ClientCnx));
            _cnx = o.ClientCnx;
            _maxMessageSize = o.MaxMessageSize;
            _cnx.Tell(new RegisterProducer(_producerId, _self));
            _protocolVersion = o.ProtocolVersion;
            _producerDeadline = TimeSpan.FromMilliseconds(DateTimeHelper.CurrentUnixTimeMillis() + ClientConfiguration.OperationTimeout.TotalMilliseconds);
            _chunkMaxMessageSize = Conf.ChunkMaxMessageSize > 0 ? (int)Math.Min(Conf.ChunkMaxMessageSize, o.MaxMessageSize) :(int) o.MaxMessageSize;

            if (_batchMessageContainer != null)
            {
                _batchMessageContainer.Container = new ProducerContainer(Self, Configuration, (int)o.MaxMessageSize, Context.System)
                {
                    ProducerId = _producerId
                };
            }

            if (Schema != null)
            {
                if (Schema.SchemaInfo != null)
                {
                    if (Schema.SchemaInfo.Type == SchemaType.JSON)
                    {
                        // for backwards compatibility purposes
                        // JSONSchema originally generated a schema for pojo based of of the JSON schema standard
                        // but now we have standardized on every schema to generate an Avro based schema
                        var protocolVersion = _protocolVersion;
                        if (_commands.PeerSupportJsonSchemaAvroFormat(protocolVersion))
                        {
                            _schemaInfo = Schema.SchemaInfo;
                        }
                        else if (Schema is JSONSchema<T> jsonSchema)
                        {
                            _schemaInfo = jsonSchema.BackwardsCompatibleJsonSchemaInfo;
                        }
                        else
                        {
                            _schemaInfo = Schema.SchemaInfo;
                        }
                    }
                    else if (Schema.SchemaInfo.Type == SchemaType.BYTES || Schema.SchemaInfo.Type == SchemaType.NONE)
                    {
                        // don't set schema info for Schema.BYTES
                        _schemaInfo = null;
                    }
                    else
                    {
                        _schemaInfo = Schema.SchemaInfo;
                    }
                }
            }
            var response = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
            _requestId = response.Id;
            _log.Info($"[{Topic}] [{_producerName}] Creating producer on cnx {_cnx.Path.Name}");
            var cmd = _commands.NewProducer(base.Topic, _producerId, _requestId, _producerName, Conf.EncryptionEnabled, _metadata, _schemaInfo, epoch, _userProvidedProducerName, Conf.AccessMode, _topicEpoch, _isTxnEnabled, Conf.InitialSubscriptionName);
            var payload = new Payload(cmd, _requestId, "NewProducer");
            await _cnx.Ask<AskResponse>(payload).ContinueWith(async response =>
             {
                 var isFaulted = response.IsFaulted;
                 var request = response.GetAwaiter().GetResult();
                 if (isFaulted || request.Failed)
                 {
                     var ex = request.Exception;
                     _log.Error($"[{Topic}] [{_producerName}] Failed to create producer: {ex}");
                     _cnx.Tell(new RemoveProducer(_producerId));
                     if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
                     {
                         //cx.Tell(PoisonPill.Instance);
                         //_replyTo.Tell(new AskResponse(ex));
                         ProducerCreatedFuture.TrySetException(ex);
                         return;
                     }
                     if (ex is TimeoutException)
                     {
                         var r = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
                         var id = r.Id;
                         var c = _commands.NewCloseProducer(_producerId, id);
                         _cnx.Tell(new SendRequestWithId(c, id));
                     }
                     if (ex is ProducerFencedException)
                     {
                         if (_log.IsDebugEnabled)
                         {
                             _log.Debug($"[{Topic}] [{_producerName}] Failed to create producer: {ex.Message}");
                         }
                     }
                     else
                     {
                         _log.Error($"[{Topic}] [{_producerName}] Failed to create producer: {ex.Message}");
                     }
                     if (ex is TopicDoesNotExistException)
                     {
                         _log.Error($"Failed to close producer on TopicDoesNotExistException: {Topic}");
                         ProducerCreatedFuture.TrySetException(ex);
                     }
                     if (ex is ProducerBlockedQuotaExceededException)
                     {
                         _log.Warning($"[{Topic}] [{_producerName}] Topic backlog quota exceeded. Throwing Exception on producer.");
                         if (_log.IsDebugEnabled)
                         {
                             _log.Debug($"[{Topic}] [{_producerName}] Pending messages: {_pendingMessages.messagesCount}");
                         }
                         var pe = new ProducerBlockedQuotaExceededException($"The backlog quota of the topic {Topic} that the producer {_producerName} produces to is exceeded");

                         FailPendingMessages(_cnx, pe);
                         //ProducerCreatedFuture.TrySetException(pe);
                     }
                     else if (ex is ProducerBlockedQuotaExceededError)
                     {
                         _log.Warning($"[{_producerName}] [{Topic}] Producer is blocked on creation because backlog exceeded on topic.");

                         ProducerCreatedFuture.TrySetException(ex);
                     }
                     if (ex is TopicTerminatedException)
                     {
                         State.ConnectionState = HandlerState.State.Terminated;
                         FailPendingMessages(_cnx, ex);
                         ProducerCreatedFuture.TrySetException(ex);
                         CloseProducerTasks();
                         Client.Tell(new CleanupProducer(_self));
                     } 
                     else if (ex is ProducerFencedException)
                     {
                         State.ConnectionState = HandlerState.State.ProducerFenced;
                         FailPendingMessages(_cnx, ex);
                         ProducerCreatedFuture.TrySetException(ex);
                         CloseProducerTasks();
                         Client.Tell(new CleanupProducer(Self));
                     }

                     else if (ProducerCreatedFuture.Task.IsCompleted || (ex is PulsarClientException && IsRetriableError(ex) && DateTimeHelper.CurrentUnixTimeMillis() < _createProducerTimeout))
                     {
                         ReconnectLater(ex);
                     }
                     else
                     {
                         State.ConnectionState = HandlerState.State.Failed;
                         Client.Tell(new CleanupProducer(Self));
                         ProducerCreatedFuture.TrySetException(new Exception());
                         var timeout = _sendTimeout;
                         if (timeout != null)
                         {
                             timeout.Cancel();
                             _sendTimeout = null;
                         }
                     }
                 }
                 else
                 {
                     try
                     {
                         var res = request.ConvertTo<ProducerResponse>();
                         var producerName = res.ProducerName;
                         var lastSequenceId = res.LastSequenceId;
                         if (Conf.AccessMode != Common.ProducerAccessMode.Shared && !_topicEpoch.HasValue)
                         {
                             _log.Info($"[{Topic}] [{_producerName}] Producer epoch is {res.TopicEpoch}");
                         }
                         _topicEpoch = res.TopicEpoch;
                         _schemaVersion = new Option<byte[]>(res.SchemaVersion);

                         if (_schemaVersion.HasValue)
                             SchemaCache.Add(SchemaHash.Of(Schema), _schemaVersion.Value);
                         if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
                         {
                             _log.Info($"[{Topic}] [{producerName}] State:{State.ConnectionState}, poisoning {_cnx.Path} to death. Becoming 'Connection'");
                             _cnx.Tell(new RemoveProducer(_producerId));
                             _cnx.Tell(PoisonPill.Instance);
                             return;
                         }

                         if (_batchMessageContainer != null)
                             _batchMessageContainer.Container.ProducerName = producerName;

                         _connectionHandler.Tell(ResetBackoff.Instance);
                         _log.Info($"[{Topic}] [{producerName}] Created producer on cnx {_cnx.Path}");
                         _connectionId = _cnx.Path.ToString();
                         _connectedSince = DateTime.Now.ToLongDateString();
                         if (string.IsNullOrWhiteSpace(_producerName))
                         {
                             _producerName = producerName;
                         }
                         if (_msgIdGenerator == 0 && Conf.InitialSequenceId == null)
                         {
                             _lastSequenceIdPublished = lastSequenceId;
                             _msgIdGenerator = lastSequenceId + 1;
                         }
                        /* if (!ProducerCreatedFuture.Task.IsCompleted && BatchMessagingEnabled)
                         {
                             _batchTimerTask = _scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromMilliseconds(Conf.BatchingMaxPublishDelayMs),
                                 TimeSpan.FromMilliseconds(Conf.BatchingMaxPublishDelayMs), async () =>
                                 {
                                     if (_log.IsDebugEnabled)
                                     {
                                         _log.Debug($"[{Topic}] [{_producerName}] Batching the messages from the batch container from timer thread");
                                     }
                                     if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
                                     {
                                         return;
                                     }
                                     await BatchMessageAndSend();
                                 });
                         }*/
                         await ResendMessages(_cnx, epoch);
                     }
                     catch (Exception e)
                     {
                         ProducerCreatedFuture.TrySetException(new PulsarClientException(e));
                     }
                 }

             });
        }

        private void ConnectionFailed(PulsarClientException exception)
        {
            var nonRetriableError = !IsRetriableError(exception);
            var timeout = DateTimeHelper.CurrentUnixTimeMillis() > _lookupDeadline.TotalMilliseconds;
            if (nonRetriableError || timeout)
            {
                exception.SetPreviousExceptions(_previousExceptions);
                if (ProducerCreatedFuture.TrySetException(exception))
                {
                    if (nonRetriableError)
                    {
                        _log.Info($"[{Topic}] Producer creation failed for producer {_producerId} with unretriableError = {exception}");
                    }
                    else
                    {
                        _log.Info($"[{Topic}] Producer creation failed for producer {_producerId} after producerTimeout");
                    }
                    State.ConnectionState = HandlerState.State.Failed;
                    Client.Tell(new CleanupProducer(_self));
                }
            }
            else
            {
                _previousExceptions.Add(exception);
            }
        }

        private void ReconnectLater(Exception exception)
        {
            _connectionHandler.Tell(new ReconnectLater(exception));
        }
        private async ValueTask RunBatchTask()
        {
            if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
            {
                return;
            }
            _log.Info($"[{Topic}] [{_producerName}] Batching the messages from the batch container from timer thread");

            await BatchMessageAndSend(true);
        }
        private bool BatchMessagingEnabled
        {
            get
            {
                return Conf.BatchingEnabled;
            }
        }

        private bool IsMultiSchemaEnabled(bool autoEnable)
        {
            if (_multiSchemaMode != MultiSchemaMode.Auto)
            {
                return _multiSchemaMode == MultiSchemaMode.Enabled;
            }
            if (autoEnable)
            {
                _multiSchemaMode = MultiSchemaMode.Enabled;
                return true;
            }
            return false;
        }

        protected internal override async ValueTask<long> LastSequenceId()
        {
            return await Task.FromResult(_lastSequenceIdPublished);
        }

        internal override async ValueTask InternalSend(IMessage<T> message, TaskCompletionSource<IMessageId> future)
        {
            _sender = Sender;
            var interceptorMessage = (Message<T>)BeforeSend(message);
            if (Interceptors != null)
            {
                _ = interceptorMessage.Properties;
            }
            var callback = new SendCallback<T>(this, future, interceptorMessage);
            await Send(interceptorMessage, callback);
        }
        private async ValueTask InternalSendWithTxn(IMessage<T> message, IActorRef txn, TaskCompletionSource<IMessageId> callback)
        {

            if (txn == null)
            {
                await InternalSend(message, callback);
            }
            else
            {
                await txn.Ask<RegisterProducedTopicResponse>(new RegisterProducedTopic(Topic))
                    .ContinueWith(async task =>
                    {
                        var cb = callback;
                        await InternalSend(message, cb);
                    });
            }
        }

        private async ValueTask Send(IMessage<T> message, SendCallback<T> callback)
        {
            //TODO: create local function for callback
            Condition.CheckArgument(message is Message<T>);
            var maxMessageSize = (int)_maxMessageSize;

            if (!IsValidProducerState(callback, message.SequenceId))
            {
                return;
            }

            var msg = (Message<T>)message;
            var msgMetadata = msg.Metadata.OriginalMetadata;

            var payload = msg.Data.ToArray();

            // If compression is enabled, we are compressing, otherwise it will simply use the same buffer
            var uncompressedSize = payload.Length;
            if (!CanEnqueueRequest(callback, message.SequenceId, uncompressedSize))
            {
                return;
            }

            try
            {

                var compressedPayload = payload;
                var compressed = false;
                // Batch will be compressed when closed
                // If a message has a delayed delivery time, we'll always send it individually
                if (!BatchMessagingEnabled || msgMetadata.ShouldSerializeDeliverAtTime())
                {
                    compressedPayload = ApplyCompression(payload);
                    compressed = true;
                    // validate msg-size (For batching this will be check at the batch completion size)
                    var compressedSize = compressedPayload.Length;

                    if (compressedSize > maxMessageSize && !Conf.ChunkingEnabled)
                    {
                        var compressedStr = (!BatchMessagingEnabled && Conf.CompressionType != CompressionType.None) ? "Compressed" : "";
                        var invalidMessageException = new PulsarClientException.InvalidMessageException($"The producer {_producerName} of the topic {Topic} sends a {compressedStr} message with {compressedSize:d} bytes that exceeds {maxMessageSize:d} bytes");
                        _log.Error(invalidMessageException.ToString());
                        callback.Future.TrySetException(invalidMessageException);
                        return;
                    }
                }

                if (!msg.Replicated && !string.IsNullOrWhiteSpace(msgMetadata.ProducerName))
                {
                    var invalidMessageException = new PulsarClientException.InvalidMessageException($"The producer {_producerName} of the topic {Topic} can not reuse the same message {msgMetadata.SequenceId}");
                    _log.Error(invalidMessageException.ToString());
                    callback.Future.TrySetException(invalidMessageException);
                    return;
                }

                if (!PopulateMessageSchema(msg, callback))
                {
                    return;
                }

                var readStartIndex = 0;
                var sequenceId = UpdateMessageMetadata(msgMetadata, uncompressedSize);
                int totalChunks;
                int payloadChunkSize;
                if (CanAddToBatch(msg) || !Conf.ChunkingEnabled)
                {
                    totalChunks = 1;
                    payloadChunkSize = maxMessageSize;
                }
                else
                {
                    // Reserve current metadata size for chunk size to avoid message size overflow.
                    // NOTE: this is not strictly bounded, as metadata will be updated after chunking.
                    // So there is a small chance that the final message size is larger than ClientCnx.getMaxMessageSize().
                    // But it won't cause produce failure as broker have 10 KB padding space for these cases.
                    payloadChunkSize = maxMessageSize - (int)Size(msgMetadata);
                    
                    if (payloadChunkSize <= 0)
                    {
                        var invalidMessageException = new PulsarClientException.InvalidMessageException($"The producer {_producerName} of the topic {Topic} sends a message with {(int)Size(msgMetadata)} bytes metadata that exceeds {_maxMessageSize} bytes");
                        _log.Error(invalidMessageException.ToString());
                        callback.Future.TrySetException(invalidMessageException);
                        return;
                    }
                    payloadChunkSize = Math.Min(_chunkMaxMessageSize, payloadChunkSize);
                    totalChunks = Math.Max(1, compressedPayload.Length) / payloadChunkSize + (Math.Max(1, compressedPayload.Length) % payloadChunkSize);// MathUtils.CeilDiv(Math.Max(1, CompressedPayload.readableBytes()), PayloadChunkSize);
                }
                var uuid = totalChunks > 1 ? string.Format("{0}-{1:D}", _producerName, sequenceId) : null;
                var chunkedMessageCtx = totalChunks > 1 ? ChunkedMessageCtx.Get(totalChunks) : null;
                var ord = totalChunks > 1 && msg.HasOrderingKey() ? msg.OrderingKey : null;
                for (var chunkId = 0; chunkId < totalChunks; chunkId++)
                {
                    // Need to reset the schemaVersion, because the schemaVersion is based on a ByteBuf object in
                    // `MessageMetadata`, if we want to re-serialize the `SEND` command using a same `MessageMetadata`,
                    // we need to reset the ByteBuf of the schemaVersion in `MessageMetadata`, I think we need to
                    // reset `ByteBuf` objects in `MessageMetadata` after call the method `MessageMetadata#writeTo()`.
                    if (chunkId > 0)
                    {
                        if (_schemaVersion.HasValue)
                        {
                            msg.Metadata.SchemaVersion = _schemaVersion.Value;

                        }
                        if (ord != null)
                        {

                            msg.Metadata.OrderingKey = ord;
                        }
                    }
                    await SerializeAndSendMessage(msg, payload, msgMetadata, sequenceId, uuid, chunkId, totalChunks, readStartIndex, payloadChunkSize, compressedPayload, compressed, compressedPayload.Length, uncompressedSize, callback, chunkedMessageCtx);
                    readStartIndex = ((chunkId + 1) * payloadChunkSize);
                }
            }
            catch (PulsarClientException e)
            {
                e.SequenceId = msg.SequenceId;
                callback.SendComplete(e);
            }
            catch (Exception t)
            {
                callback.SendComplete(t);
            }
        }
        /// <summary>
		/// Update the message metadata except those fields that will be updated for chunks later.
		/// </summary>
		/// <param name="msgMetadata"> </param>
		/// <param name="uncompressedSize"> </param>
		/// <returns> the sequence id </returns>
		private long UpdateMessageMetadata(in MessageMetadata msgMetadata, in int uncompressedSize)
        {
            long sequenceId;
            if (msgMetadata.SequenceId == 0)
            {
                sequenceId = _msgIdGenerator;
                msgMetadata.SequenceId = (ulong)sequenceId;
            }
            else
            {
                sequenceId = (long)msgMetadata.SequenceId;
            }

            if (!HasPublishTime(msgMetadata.PublishTime))
            {
                msgMetadata.PublishTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (!string.IsNullOrWhiteSpace(msgMetadata.ProducerName))
                {
                    _log.Warning($"changing producer name from '{msgMetadata.ProducerName}' to ''. Just helping out ;)");
                    msgMetadata.ProducerName = string.Empty;
                }
                msgMetadata.ProducerName = _producerName;

                if (Conf.CompressionType != CompressionType.None)
                {
                    msgMetadata.Compression = CompressionCodecProvider.ConvertToWireProtocol(Conf.CompressionType);
                }
                msgMetadata.UncompressedSize = (uint)uncompressedSize;
            }
            return sequenceId;
        }

        private bool CanEnqueueRequest(SendCallback<T> Callback, long SequenceId, int PayloadSize)
        {
            /*try
            {
                if (_conf.isBlockIfQueueFull())
                {
                    if (semaphore.isPresent())
                    {
                        semaphore.get().acquire();
                    }
                    client.getMemoryLimitController().reserveMemory(PayloadSize);
                }
                else
                {
                    if (!semaphore.map(Semaphore.tryAcquire).orElse(true))
                    {
                        Callback.sendComplete(new PulsarClientException.ProducerQueueIsFullError("Producer send queue is full", SequenceId));
                        return false;
                    }

                    if (!client.getMemoryLimitController().tryReserveMemory(PayloadSize))
                    {
                        semaphore.ifPresent(Semaphore.release);
                        Callback.sendComplete(new PulsarClientException.MemoryBufferIsFullError("Client memory buffer is full", SequenceId));
                        return false;
                    }
                }
            }
            catch (InterruptedException E)
            {
                Thread.CurrentThread.Interrupt();
                Callback.sendComplete(new PulsarClientException(E, SequenceId));
                return false;
            }
            */
            return true;
        }
        private async ValueTask SerializeAndSendMessage(Message<T> msg, byte[] payload, MessageMetadata msgMetadata, long sequenceId, string uuid, int chunkId, int totalChunks, int readStartIndex, int chunkMaxSizeInBytes, byte[] compressedPayload, bool compressed, int compressedPayloadSize, int uncompressedSize, SendCallback<T> callback, ChunkedMessageCtx chunkedMessageCtx)
        {
            var chunkPayload = compressedPayload;
            var chunkMsgMetadata = msgMetadata;
            if (totalChunks > 1 && TopicName.Get(Topic).Persistent)
            {
                chunkPayload = compressedPayload.Slice(readStartIndex,  Math.Min(chunkMaxSizeInBytes, chunkPayload.Length - readStartIndex));
                if (chunkId != totalChunks - 1)
                {
                    chunkMsgMetadata = msgMetadata;
                }
                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    chunkMsgMetadata.Uuid = uuid;
                }
                chunkMsgMetadata.ChunkId = chunkId;
                chunkMsgMetadata.NumChunksFromMsg = totalChunks;
                chunkMsgMetadata.TotalChunkMsgSize = compressedPayloadSize;
            }
            if (CanAddToBatch(msg) && totalChunks <= 1)
            {
                if (CanAddToCurrentBatch(msg))
                {
                    // should trigger complete the batch message, new message will add to a new batch and new batch
                    // sequence id use the new message, so that broker can handle the message duplication
                    if (sequenceId <= LastSequenceIdPushed)
                    {
                        _isLastSequenceIdPotentialDuplicated = true;
                        if (sequenceId <= _lastSequenceIdPublished)
                        {
                            _log.Warning($"Message with sequence id {sequenceId} is definitely a duplicate");
                        }
                        else
                        {
                            _log.Info($"Message with sequence id {sequenceId} might be a duplicate but cannot be determined at this time.");
                        }
                        await DoBatchSendAndAdd(msg, callback, payload);
                    }
                    else
                    {
                        // Should flush the last potential duplicated since can't combine potential duplicated messages
                        // and non-duplicated messages into a batch.
                        if (_isLastSequenceIdPotentialDuplicated)
                        {
                            await DoBatchSendAndAdd(msg, callback, payload);
                        }
                        else
                        {
                            // handle boundary cases where message being added would exceed
                            // batch size and/or max message size
                            var isBatchFull = _batchMessageContainer.Add(msg, callback);
                            _lastSendFuture = callback.Future;
                            if (isBatchFull)
                            {
                                await BatchMessageAndSend(false);
                            }
                            else
                            {
                                MaybeScheduleBatchFlushTask();
                            }
                        }
                        _isLastSequenceIdPotentialDuplicated = false;
                    }
                }
                else
                {
                    await DoBatchSendAndAdd(msg, callback, payload);
                }
            }
            else
            {
                if (!compressed)
                {
                    chunkPayload = ApplyCompression(chunkPayload);
                }
                var encryptedPayload = EncryptMessage(chunkMsgMetadata, chunkPayload);

                msgMetadata = chunkMsgMetadata;
                // When publishing during replication, we need to set the correct number of message in batch
                // This is only used in tracking the publish rate stats
                var numMessages = msg.Metadata.OriginalMetadata.ShouldSerializeNumMessagesInBatch() ? msg.Metadata.OriginalMetadata.NumMessagesInBatch : 1;

                OpSendMsg<T> op;
                if (msg.GetSchemaState() == Message<T>.SchemaState.Ready)
                {
                    /* var ts = new TaskCompletionSource<OpSendMsg<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
                    Akka.Dispatch.ActorTaskScheduler.RunTask( () =>
                    {
                        var cmd = SendMessage(_producerId, sequenceId, numMessages, msg.MessageId, msgMetadata, encryptedPayload);
                        var p = OpSendMsg<T>.Create(msg, cmd, sequenceId, callback);
                        ts.TrySetResult(p);
                    });
                    var o = ts.Task.GetAwaiter().GetResult();
                    op = o;*/
                    var cmd = SendMessage(_producerId, sequenceId, numMessages, msg.MessageId, msgMetadata, encryptedPayload);
                   
                    op = OpSendMsg<T>.Create(msg, cmd, sequenceId, callback);
                   
                }
                else
                {
                    op = OpSendMsg<T>.Create(msg, ReadOnlySequence<byte>.Empty, sequenceId, callback);
                    var finalMsgMetadata = msgMetadata;
                    op.Cmd = SendMessage(_producerId, sequenceId, numMessages, msg.MessageId, finalMsgMetadata, encryptedPayload);
                }
                op.NumMessagesInBatch = numMessages;
                
                op.BatchSizeByte = encryptedPayload.Length;
                
                if (totalChunks > 1)
                {
                    op.TotalChunks = totalChunks;
                    op.ChunkId = chunkId;
                }
                op.ChunkedMessageCtx = chunkedMessageCtx;
                _lastSendFuture = callback.Future;
                await ProcessOpSendMsg(op);
            }
        }

        private bool PopulateMessageSchema(Message<T> msg, SendCallback<T> callback)
        {
            if (msg.SchemaInternal() == Schema)
            {
                if (_schemaVersion.HasValue)
                    msg.Metadata.OriginalMetadata.SchemaVersion = _schemaVersion.Value;

                msg.SetSchemaState(Message<T>.SchemaState.Ready);
                return true;
            }
            if (!IsMultiSchemaEnabled(true))
            {
                var exception = new PulsarClientException.InvalidMessageException($"The producer {_producerName} of the topic {Topic} is disabled the `MultiSchema`: Seq:{msg.SequenceId}");
                callback.Future.TrySetException(exception);
                return false;
            }
            var schemaHash = SchemaHash.Of(msg.Schema);
            if (SchemaCache.TryGetValue(schemaHash, out var schemaVersion))
            {
                msg.Metadata.OriginalMetadata.SchemaVersion = schemaVersion;
                msg.SetSchemaState(Message<T>.SchemaState.Ready);
            }
            return true;
        }

        private bool RePopulateMessageSchema(Message<T> msg)
        {
            var schemaHash = SchemaHash.Of(msg.Schema);
            if (!SchemaCache.TryGetValue(schemaHash, out var schemaVersion))
            {
                return false;
            }
            msg.Metadata.OriginalMetadata.SchemaVersion = schemaVersion;
            msg.SetSchemaState(Message<T>.SchemaState.Ready);
            return true;
        }

        private async ValueTask TryRegisterSchema(Message<T> msg, SendCallback<T> callback, long expectedCnxEpoch)
        {
            if (!State.ChangeToRegisteringSchemaState())
            {
                return;
            }
            
            var idResponse = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).ConfigureAwait(false);
            _requestId = idResponse.Id;
            var protocolVersionResponse = await _cnx.Ask<RemoteEndpointProtocolVersionResponse>(RemoteEndpointProtocolVersion.Instance).ConfigureAwait(false);
            _protocolVersion = protocolVersionResponse.Version;

            if (msg.SchemaInternal() != null && msg.SchemaInternal().SchemaInfo.Type.Value > 0)
            {
                _schemaInfo = (SchemaInfo)msg.SchemaInternal().SchemaInfo;
            }
            else
            {
                _schemaInfo = (SchemaInfo)ISchema<T>.Bytes.SchemaInfo;
            }
            if (!_commands.PeerSupportsGetOrCreateSchema(_protocolVersion))
            {
                var err = new PulsarClientException.NotSupportedException($"The command `GetOrCreateSchema` is not supported for the protocol version {_protocolVersion}. The producer is {_producerName}, topic is {base.Topic}");
                _log.Error(err.ToString());
            }
            var request = _commands.NewGetOrCreateSchema(_requestId, base.Topic, _schemaInfo);
            var payload = new Payload(request, _requestId, "SendGetOrCreateSchema");
            _log.Info($"[{Topic}] [{_producerName}] GetOrCreateSchema request", Topic, _producerName);
            var schemaResponse = await _cnx.Ask<GetOrCreateSchemaResponse>(payload).ConfigureAwait(false);
            if (schemaResponse.Response.ErrorCode != ServerError.UnknownError)
            {
                _log.Warning($"[{Topic}] [{_producerName}] GetOrCreateSchema succeed");
                var schemaHash = SchemaHash.Of(msg.Schema);
                if (!SchemaCache.ContainsKey(schemaHash))
                    SchemaCache.Add(schemaHash, schemaResponse.Response.SchemaVersion);
                msg.Metadata.OriginalMetadata.SchemaVersion = schemaResponse.Response.SchemaVersion;
                msg.SetSchemaState(Message<T>.SchemaState.Ready);
            }
            else
            {
                if (schemaResponse.Response.ErrorCode == ServerError.IncompatibleSchema)
                {
                    _log.Warning($"[{Topic}] [{_producerName}] GetOrCreateSchema error: [{schemaResponse.Response.ErrorCode}:{schemaResponse.Response.ErrorMessage}]");
                    msg.SetSchemaState(Message<T>.SchemaState.Broken);
                    var ex = new IncompatibleSchemaException(schemaResponse.Response.ErrorMessage);
                    callback.SendComplete(ex);
                    _log.Error(ex.ToString());
                }
               /* else
                {
                    _log.Error($"[{Topic}] [{_producerName}] GetOrCreateSchema failed: [{schemaResponse.Response.ErrorCode}:{schemaResponse.Response.ErrorMessage}]");
                    msg.SetSchemaState(Message<T>.SchemaState.None);
                }*/
            }

            await RecoverProcessOpSendMsgFrom(msg, expectedCnxEpoch);
        }

        private byte[] EncryptMessage(MessageMetadata msgMetadata, byte[] compressedPayload)
        {

            var encryptedPayload = compressedPayload;
            if (!Conf.EncryptionEnabled || _msgCrypto == null)
            {
                return encryptedPayload;
            }
            try
            {
                encryptedPayload = _msgCrypto.Encrypt(Conf.EncryptionKeys, Conf.CryptoKeyReader, msgMetadata, compressedPayload);
            }
            catch (PulsarClientException e)
            {
                // Unless config is set to explicitly publish un-encrypted message upon failure, fail the request
                if (Conf.CryptoFailureAction == ProducerCryptoFailureAction.Send)
                {
                    _log.Warning($"[{Topic}] [{_producerName}] Failed to encrypt message {e}. Proceeding with publishing unencrypted message");
                    return compressedPayload;
                }
                throw e;
            }
            return encryptedPayload;
        }

        private ReadOnlySequence<byte> SendMessage(long producerId, long sequenceId, int numMessages, IMessageId messageId, MessageMetadata msgMetadata, byte[] compressedPayload)
        {
            _log.Info($"Send message with {_producerName}:{producerId}");
            if (messageId is MessageId)
            {
                return _commands.NewSend(producerId, sequenceId, numMessages, ChecksumType, ((MessageId)messageId).LedgerId, ((MessageId)messageId).EntryId, msgMetadata, compressedPayload);
            }
            else
            {
                return _commands.NewSend(producerId, sequenceId, numMessages, ChecksumType, -1, -1, msgMetadata, compressedPayload);
                
            }
        }
        private ReadOnlySequence<byte> SendMessage(long producerId, long sequenceId, int numMessages, MessageMetadata msgMetadata, byte[] compressedPayload)
        {
            _log.Info($"Send message with {_producerName}:{producerId}");
            return _commands.NewSend(producerId, sequenceId, numMessages, ChecksumType, msgMetadata, compressedPayload);
        }
        private ReadOnlySequence<byte> SendMessage(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, MessageMetadata msgMetadata, byte[] compressedPayload)
        {
            return _commands.NewSend(producerId, lowestSequenceId, highestSequenceId, numMessages, ChecksumType, msgMetadata, compressedPayload);
        }
        private ChecksumType ChecksumType
        {
            get
            {
                try
                {
                    var protocolVersionResponse = _cnx.Ask<RemoteEndpointProtocolVersionResponse>(RemoteEndpointProtocolVersion.Instance, TimeSpan.FromMilliseconds(1000)).ConfigureAwait(false).GetAwaiter().GetResult();
                    if (protocolVersionResponse.Version >= BrokerChecksumSupportedVersion())
                    {
                        return ChecksumType.Crc32C;
                    }
                    else
                    {
                        return ChecksumType.None;
                    }
                }
                catch { return ChecksumType.None; }
            }
        }
        private int BrokerChecksumSupportedVersion()
        {
            return (int)ProtocolVersion.V6;
        }
        public IStash Stash { get; set; }

        private bool CanAddToBatch(Message<T> msg)
        {
            return msg.GetSchemaState() == Message<T>.SchemaState.Ready && BatchMessagingEnabled && !msg.Metadata.OriginalMetadata.ShouldSerializeDeliverAtTime();
        }

        private bool CanAddToCurrentBatch(Message<T> msg)
        {
            try
            {
                var enoughSpace = _batchMessageContainer.HaveEnoughSpace(msg);
                var isMulti = !IsMultiSchemaEnabled(false);
                var sameSchema = _batchMessageContainer.HasSameSchema(msg);
                var txn = _batchMessageContainer.HasSameTxn(msg);
                return enoughSpace && (isMulti || sameSchema) && txn;
            }
            catch { }
            return false;
        }

        private async ValueTask DoBatchSendAndAdd(Message<T> msg, SendCallback<T> callback, byte[] payload)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{Topic}] [{_producerName}] Closing out batch to accommodate large message with size {msg.Data.Length}");
            }
            try
            {
                await BatchMessageAndSend(false);
                _batchMessageContainer.Add(msg, callback);
                _lastSendFuture = callback.Future;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }

        private bool IsValidProducerState(SendCallback<T> callback, long sequenceId)
        {
            switch (State.ConnectionState)
            {
                case HandlerState.State.Ready:
                // OK
                case HandlerState.State.Connecting:
                // We are OK to queue the messages on the client, it will be sent to the broker once we get the connection
                case HandlerState.State.RegisteringSchema:
                    // registering schema
                    return true;
                case HandlerState.State.Closing:
                case HandlerState.State.Closed:
                    callback.SendComplete(new PulsarClientException.AlreadyClosedException("Producer already closed", sequenceId));
                    return false;
                case HandlerState.State.ProducerFenced:
                    callback.SendComplete(new PulsarClientException.ProducerFencedException("Producer was fenced"));
                    return false;
                case HandlerState.State.Terminated:
                    callback.SendComplete(new PulsarClientException.TopicTerminatedException("Topic was terminated", sequenceId));
                    return false;
                case HandlerState.State.Failed:
                case HandlerState.State.Uninitialized:
                default:
                    callback.SendComplete(new PulsarClientException.NotConnectedException(sequenceId));
                    return false;
            }
        }
        private void SendCommand(OpSendMsg<T> op)
        {
            var pay = new Payload(op.Cmd, -1, "CommandMessage");
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[{Topic}] [{_producerName}] Sending message cnx, sequenceId {op.SequenceId}");
            }
            op.UpdateSentTimestamp();
            _cnx.Tell(pay);
        }
        protected override void PostStop()
        {
            if (_sendTimeout != null)
            {
                _sendTimeout.Cancel();
                _sendTimeout = null;
            }

            if (_batchTimerTask != null)
            {
                _batchTimerTask.Cancel();
                _batchTimerTask = null;
            }
            if (_keyGeneratorTask != null && !_keyGeneratorTask.IsCancellationRequested)
            {
                _keyGeneratorTask.Cancel();
            }
            //Close().ConfigureAwait(false);
            base.PostStop();
        }
        private void CloseAndClearPendingMessages()
        {
            State.ConnectionState = HandlerState.State.Closed;
            Client.Tell(new CleanupProducer(_self));
            PulsarClientException Ex = new AlreadyClosedException($"The producer {_producerName} of the topic {Topic} was already closed when closing the producers");
            // Use null for cnx to ensure that the pending messages are failed immediately
            FailPendingMessages(null, Ex);
        }
        protected internal override bool Connected()
        {
            return CnxIfReady != null;
        }

        /// <summary>
        /// Hook method for testing. By returning null, it's possible to prevent messages
        /// being delivered to the broker.
        /// </summary>
        /// <returns> cnx if OpSend messages should be written to open connection. Caller must
        /// verify that the returned cnx is not null before using reference. </returns>
        protected internal virtual IActorRef CnxIfReady
        {
            get
            {
                if (State.ConnectionState == HandlerState.State.Ready)
                {;
                    return Cnx();
                }
                else
                {
                    return null;
                }
            }
        }
       
        /*protected internal override bool Connected()
        {
            var cnx = _cnx;
            return cnx != null && (State.ConnectionState == HandlerState.State.Ready);
        }*/
        private void DisconnectedTimestamp()
        {
            Receive<LastConnectionClosedTimestampResponse>(l =>
            {
                var resp = l.TimeStamp;
                _replyTo.Tell(resp);
                Become(Ready);
            });
            ReceiveAny(_ => Stash.Stash());
            _connectionHandler.Tell(LastConnectionClosedTimestamp.Instance);

        }
        protected internal override long LastDisconnectedTimestamp()
        {

            return 0;
        }


        public virtual void Terminated(IActorRef cnx)
        {
            var previousState = State.GetAndUpdateState(State.ConnectionState == HandlerState.State.Closed ? HandlerState.State.Closed : HandlerState.State.Terminated);
            if (previousState != HandlerState.State.Terminated && previousState != HandlerState.State.Closed)
            {
                _log.Info($"[{Topic}] [{_producerName}] The topic has been terminated");

                FailPendingMessages(cnx, new TopicTerminatedException($"The topic {Topic} that the producer {_producerName} produces to has been terminated"));
            }
        }
        /// <summary>
        /// Checks message checksum to retry if message was corrupted while sending to broker. Recomputes checksum of the
        /// message header-payload again.
        /// <ul>
        /// <li><b>if matches with existing checksum</b>: it means message was corrupt while sending to broker. So, resend
        /// message</li>
        /// <li><b>if doesn't match with existing checksum</b>: it means message is already corrupt and can't retry again.
        /// So, fail send-message by failing callback</li>
        /// </ul>
        /// </summary>
        /// <param name="cnx"> </param>
        /// <param name="sequenceId"> </param>
        private async ValueTask RecoverChecksumError(IActorRef cnx, long sequenceId)
        {
            var op = _pendingMessages.Peek();
            if (op == null)
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{Topic}] [{_producerName}] Got send failure for timed out msg {sequenceId}");
                }
            }
            else
            {
                var expectedSequenceId = GetHighestSequenceId(op);
                if (sequenceId == expectedSequenceId)
                {
                    var corrupted = !VerifyLocalBufferIsNotCorrupted(op);
                    if (corrupted)
                    {
                        // remove message from pendingMessages queue and fail callback
                       _pendingMessages.Remove();
                        try
                        {

                            op.SendComplete(new ChecksumException($"The checksum of the message which is produced by producer {_producerName} to the topic '{Topic}' is corrupted"));
                        }
                        catch (Exception t)
                        {
                            _log.Warning($"[{Topic}] [{_producerName}] Got exception while completing the callback for msg {sequenceId}:{t}");
                        }
                        op.Msg.Recycle();
                        return;
                    }
                    else
                    {
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug($"[{Topic}] [{_producerName}] Message is not corrupted, retry send-message with sequenceId {sequenceId}");
                        }
                    }

                }
                else
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{Topic}] [{_producerName}] Corrupt message is already timed out {sequenceId}");
                    }
                }
            }
            // as msg is not corrupted : let producer resend pending-messages again including checksum failed message
            var epoch = await _connectionHandler.Ask<GetEpochResponse>(GetEpoch.Instance);
            await ResendMessages(cnx, epoch.Epoch);
        }

        /// <summary>
        /// Computes checksum again and verifies it against existing checksum. If checksum doesn't match it means that
        /// message is corrupt.
        /// </summary>
        /// <param name="op"> </param>
        /// <returns> returns true only if message is not modified and computed-checksum is same as previous checksum else
        ///         return false that means that message is corrupted. Returns true if checksum is not present. </returns>
        private bool VerifyLocalBufferIsNotCorrupted(OpSendMsg<T> o)
        {
            try
            {
                using var stream = new MemoryStream(o.Msg.Data.ToArray());
                using var reader = new BinaryReader(stream);
                var sizeStream = stream.Length;
                stream.Seek(4L, SeekOrigin.Begin);
                var sizeCmd = reader.ReadInt32().IntFromBigEndian();
                stream.Seek((long)10 + sizeCmd, SeekOrigin.Begin);

                var checkSum = reader.ReadInt32().IntFromBigEndian();
                var checkSumPayload = ((int)sizeStream) - 14 - sizeCmd;
                var computedCheckSum = (int)CRC32C.Get(0u, stream, checkSumPayload);
                return checkSum != computedCheckSum;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return false;
            }
        }
       
        private void RecoverNotAllowedError(long sequenceId, string errorMsg)
        {
            var op = _pendingMessages.Peek();
            if (op == null && sequenceId == GetHighestSequenceId(op))
            {
                _pendingMessages.Remove();
                try
                {
                    //op.SendComplete(new NotAllowedException($"The size of the message which is produced by producer {_producerName} to the topic '{Topic}' is not allowed"));
                    op.SendComplete(new NotAllowedException(errorMsg));
                }
                catch (Exception t)
                {
                    _log.Warning($"[{Topic}] [{_producerName}] Got exception while completing the callback for msg {sequenceId}:{t}");
                }
                op.Msg.Recycle();
            }
        }

        /// <summary>
        /// This fails and clears the pending messages with the given exception. This method should be called from within the
        /// ProducerImpl object mutex.
        /// </summary>
        private void FailPendingMessages(IActorRef cnx, PulsarClientException ex)
        {
            if (cnx == null)
            {
                var batchMessagingEnabled = BatchMessagingEnabled;
                _pendingMessages.ForEach(op =>
                {
                    try
                    {
                        ex.SequenceId = op.SequenceId;
                        if (op.TotalChunks <= 1 || (op.ChunkId == op.TotalChunks - 1))
                        {
                            op.SendComplete(ex);
                        }
                    }
                    catch (Exception t)
                    {
                        _log.Warning($"[{Topic}] [{_producerName}] Got exception while completing the callback for msg {op.SequenceId}:{t}");
                    }
                    op.Recycle();
                });

                _pendingMessages.Clear();
                if (batchMessagingEnabled)
                {
                    FailPendingBatchMessages(ex);
                }

            }
            else
            {
                FailPendingMessages(null, ex);
            }
        }

        /// <summary>
        /// fail any pending batch messages that were enqueued, however batch was not closed out
        /// 
        /// </summary>
        private void FailPendingBatchMessages(PulsarClientException ex)
        {
            if (_batchMessageContainer.Empty)
            {
                return;
            }
            var numMessagesInBatch = _batchMessageContainer.NumMessagesInBatch;
            _batchMessageContainer.Discard(ex);
        }
        private void AckReceived(AckReceived ackReceived)
        {
            var cnx = _cnx;
            var op = _pendingMessages.Peek();
            if (op == null)
            {
                var msg = $"[{Topic}] [{_producerName}] Got ack for timed out msg {ackReceived.SequenceId} - {ackReceived.HighestSequenceId}";
                if (_log.IsDebugEnabled)
                {
                    _log.Debug(msg);
                }
                return;
            }
            else if (ackReceived.SequenceId > op.SequenceId)
            {
                var msg = $"[{Topic}] [{_producerName}] Got ack for msg. expecting: {op.Msg.SequenceId} - {op.HighestSequenceId} - got: {ackReceived.SequenceId} - {ackReceived.HighestSequenceId} - queue-size: {_pendingMessages.MessagesCount()}";
                _log.Warning(msg);
                // Force connection closing so that messages can be re-transmitted in a new connection
                cnx.Tell(Close.Instance);
                return;
            }
            else if (ackReceived.SequenceId < op.SequenceId)
            {
                var msg = $"[{Topic}] [{_producerName}] Got ack for timed out msg. expecting: {op.SequenceId} - {op.HighestSequenceId} - got: {ackReceived.SequenceId} - {ackReceived.HighestSequenceId}";
                // Ignoring the ack since it's referring to a message that has already timed out.
                if (_log.IsDebugEnabled)
                {
                    _log.Debug(msg);
                }
                return;
            }
            else
            {
                // Add check `sequenceId >= highestSequenceId` for backward compatibility.
                if (ackReceived.SequenceId >= ackReceived.HighestSequenceId || ackReceived.HighestSequenceId == op.HighestSequenceId)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{Topic}] [{_producerName}] Received ack for msg {ackReceived.SequenceId} ");
                    }

                    _pendingMessages.Remove();
                }
                else
                {
                    var msg = $"[{Topic}] [{_producerName}] Got ack for batch msg error. expecting: {op.SequenceId} - {op.HighestSequenceId} - got: {ackReceived.SequenceId} - {ackReceived.HighestSequenceId} - queue-size: {_pendingMessages.MessagesCount()}";
                    _log.Warning(msg);
                    // Force connection closing so that messages can be re-transmitted in a new connection
                    cnx.Tell(Close.Instance);
                    return;
                }
            }
            var finalOp = op;
            _lastSequenceIdPublished = Math.Max(_lastSequenceIdPublished, GetHighestSequenceId(finalOp));

            op.SetMessageId(ackReceived.LedgerId, ackReceived.EntryId, _partitionIndex);

            if (op.TotalChunks > 1)
            {
                if (op.ChunkId == 0)
                {
                    op.ChunkedMessageCtx.FirstChunkMessageId = new MessageId(ackReceived.LedgerId, ackReceived.EntryId, _partitionIndex);
                }
                else if (op.ChunkId == op.TotalChunks - 1)
                {
                    op.ChunkedMessageCtx.LastChunkMessageId = new MessageId(ackReceived.LedgerId, ackReceived.EntryId, _partitionIndex);
                    op.MessageId = op.ChunkedMessageCtx.ChunkMessageId;
                }
            }
            // if message is chunked then call callback only on last chunk
            if (op.TotalChunks <= 1 || (op.ChunkId == op.TotalChunks - 1))
            {

                try
                {
                    // Need to protect ourselves from any exception being thrown in the future handler from the
                    // application
                    op.SendComplete(null);
                }
                catch (Exception t)
                {
                    var msg = $"[{Topic}] [{_producerName}] Got exception while completing the callback for msg {ackReceived.SequenceId}";
                    _log.Warning($"{msg}:{t}");
                }
            }

            op.Recycle();
        }

        private long GetHighestSequenceId(OpSendMsg<T> op)
        {
            return Math.Max(op.HighestSequenceId, op.SequenceId);
        }

        private async ValueTask ResendMessages(IActorRef cnx, long expectedEpoch)
        {
            if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
            {
                cnx.Tell(Close.Instance);
                return;
            }
            var messagesToResend = _pendingMessages.MessagesCount();
            if (messagesToResend == 0)
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{Topic}] [{_producerName}] No pending messages to resend {messagesToResend}");
                }

                if (State.ChangeToReadyState())
                {
                    ProducerCreatedFuture.TrySetResult(_self);
                    ScheduleBatchFlushTask(0);
                    return;
                }
                cnx.Tell(Close.Instance);
                return;
            }
            _log.Info($"[{Topic}] [{_producerName}] Re-Sending {messagesToResend} messages to server");
            await RecoverProcessOpSendMsgFrom(null, expectedEpoch);
        }

        /// <summary>
        /// Process sendTimeout events
        /// </summary>
        private void Run()
        {
            if (_sendTimeout.IsCancellationRequested)
            {
                return;
            }

            long timeToWaitMs;
            // If it's closing/closed we need to ignore this timeout and not schedule next timeout.
            if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
            {
                return;
            }
            var firstMsg = _pendingMessages.Peek();
            if (firstMsg == null && (_batchMessageContainer == null || _batchMessageContainer.Empty))
            {
                // If there are no pending messages, reset the timeout to the configured value.
                timeToWaitMs = (long)Conf.SendTimeoutMs.TotalMilliseconds;
            }
            else
            {
                long createdAt;
                if (firstMsg != null)
                {
                    createdAt = firstMsg.CreatedAt;
                }
                else
                {
                    
                    // Because we don't flush batch messages while disconnected, we consider them "createdAt" when
                    // they would have otherwise been flushed.
                    createdAt = _lastBatchSendNanoTime + (long)TimeSpan.FromMilliseconds(Conf.BatchingMaxPublishDelayMillis).TotalMilliseconds;
                }
                // If there is at least one message, calculate the diff between the message timeout and the elapsed
                // time since first message was created.
                var diff = Conf.SendTimeoutMs.TotalMilliseconds - (DateTimeHelper.CurrentUnixTimeMillis() - createdAt);
                if (diff <= 0)
                {
                    // The diff is less than or equal to zero, meaning that the message has been timed out.
                    // Set the callback to timeout on every message, then clear the pending queue.
                    _log.Info($"[{Topic}] [{_producerName}] Message send timed out. Failing {PendingQueueSize} messages");

                    var msg = new TimeoutException($"The producer {_producerName} can not send message to the topic {Topic} within given timeout: {Conf.SendTimeoutMs}");
                    if (firstMsg != null)
                    {
                        var te = new TimeoutException(msg, firstMsg.SequenceId);
                        FailPendingMessages(_cnx, te);
                    }
                    else
                    {
                        FailPendingBatchMessages(new TimeoutException(msg));
                    }
                    
                    // Since the pending queue is cleared now, set timer to expire after configured value.
                    timeToWaitMs = (long)Conf.SendTimeoutMs.TotalMilliseconds;
                }
                else
                {
                    // The diff is greater than zero, set the timeout to the diff value
                    timeToWaitMs = (long)diff;
                }
            }
            _sendTimeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(timeToWaitMs), _self, RunSendTimeout.Instance, ActorRefs.NoSender);

        }

        private async ValueTask Flush()
        {
            
            if (BatchMessagingEnabled)
            {
               await BatchMessageAndSend(false);
            }
            var lastSendFuture = _lastSendFuture;
            if (!(lastSendFuture == _lastSendFutureWrapper.LastSendFuture))
            {
                _lastSendFutureWrapper = LastSendFutureWrapper.Create(lastSendFuture);
            }
            _lastSendFutureWrapper.HandleOnce();
        }
        private void CloseProducerTasks()
        {
           
            if (_sendTimeout != null)
            {
                _sendTimeout.Cancel();
               _sendTimeout = null;
            }

            if (_keyGeneratorTask != null && !_keyGeneratorTask.IsCancellationRequested)
            {
                _keyGeneratorTask.Cancel(false);
            }

            _stats.CancelStatsTimeout();
        }
        private async ValueTask TriggerFlush()
        {
            if (BatchMessagingEnabled)
            {
                await BatchMessageAndSend(false);
            }
        }
        // must acquire semaphore before calling
        private void MaybeScheduleBatchFlushTask()
        {
            if (_batchFlushTask != null || State.ConnectionState != HandlerState.State.Ready)
            {
                return;
            }
            ScheduleBatchFlushTask((long)Conf.BatchingMaxPublishDelayMs);
        }

        // must acquire semaphore before calling
        private void ScheduleBatchFlushTask(long batchingDelayMicros)
        {
            if (_cnx != null && BatchMessagingEnabled)
            {
                _batchFlushTask = _scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(batchingDelayMicros), async ()=> await BatchFlushTask());
            }
        }
        private async ValueTask BatchFlushTask()
        {
            if (_log.IsDebugEnabled/*.isTraceEnabled()*/)
            {
                _log.Debug($"[{Topic}] [{_producerName}] Batching the messages from the batch container from flush thread");
            }
            _batchFlushTask.Cancel();
            _batchFlushTask = null;
            // If we're not ready, don't schedule another flush and don't try to send.
            if (State.ConnectionState != HandlerState. State.Ready)
            {
                return;
            }
            // If a batch was sent more recently than the BatchingMaxPublishDelayMicros, schedule another flush to run just
            // at BatchingMaxPublishDelayMicros after the last send.
            var microsSinceLastSend = (long)TimeSpan.FromMilliseconds(NanoTime() - _lastBatchSendNanoTime).TotalMilliseconds;
            if (microsSinceLastSend < Conf.BatchingMaxPublishDelayMs)
            {
                ScheduleBatchFlushTask((long)(Conf.BatchingMaxPublishDelayMs - microsSinceLastSend));
            }
            else if (_lastBatchSendNanoTime == 0)
            {
                // The first time a producer sends a message, the lastBatchSendNanoTime is 0.
                _lastBatchSendNanoTime = NanoTime();
                ScheduleBatchFlushTask((long)Conf.BatchingMaxPublishDelayMs);
            }
            else
            {
               await BatchMessageAndSend(true);
            }
        }
        private static long NanoTime()
        {
            var nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }
        // must acquire semaphore before enqueuing
        private async ValueTask BatchMessageAndSend(bool shouldScheduleNextBatchFlush)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Info($"[{Topic}] [{_producerName}] Batching the messages from the batch container with {_batchMessageContainer.NumMessagesInBatch} messages");
            }
            if (!_batchMessageContainer.Empty)
            {
                try
                {
                    IList<OpSendMsg<T>> opSendMsgs;
                    if (_batchMessageContainer.MultiBatches)
                    {
                        opSendMsgs = _batchMessageContainer.CreateOpSendMsgs();
                    }
                    else
                    {
                        opSendMsgs = new List<OpSendMsg<T>> { _batchMessageContainer.CreateOpSendMsg() };
                    }
                    _batchMessageContainer.Clear();
                    foreach (var opSendMsg in opSendMsgs)
                    {
                        await ProcessOpSendMsg(opSendMsg);
                    }
                }
                catch (Exception t)
                {
                    _log.Warning($"[{Topic}] [{_producerName}] error while create opSendMsg by batch message container: {t}");
                }
                finally
                {
                    if (shouldScheduleNextBatchFlush)
                    {
                        MaybeScheduleBatchFlushTask();
                    }
                }
            }
        }
        private async ValueTask ProcessOpSendMsg(OpSendMsg<T> op)
        {
            try
            {
                if (op.Msg != null && BatchMessagingEnabled)
                {
                    await BatchMessageAndSend(false);
                }
                if (IsMessageSizeExceeded(op))
                {
                    return;
                }
                _pendingMessages.Add(op);
                if (op.Msg != null)
                {
                    LastSequenceIdPushed = Math.Max(LastSequenceIdPushed, GetHighestSequenceId(op));
                }
                var cnx = CnxIfReady;
                if (cnx != null)
                {
                    if (op.Msg != null && op.Msg.GetSchemaState() == Message<T>.SchemaState.None)
                    {
                        var epoch = await _connectionHandler.Ask<GetEpochResponse>(GetEpoch.Instance);
                        await TryRegisterSchema(op.Msg, op.Callback, epoch.Epoch);
                        return;
                    }
                    _stats.UpdateNumMsgsSent(op.NumMessagesInBatch, op.BatchSizeByte);
                    SendCommand(op);
                }
                else
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{Topic}] [{_producerName}] Connection is not ready -- sequenceId {op.SequenceId}");
                    }
                }
            }
            catch (Exception t)
            {
                _log.Warning($"[{Topic}] [{_producerName}] error while closing out batch -- {t}");
                op.SendComplete(new PulsarClientException(t, op.SequenceId));
            }
        }

        private async ValueTask RecoverProcessOpSendMsgFrom(Message<T> from, long expectedEpoch)
        {
            var epoch = await _connectionHandler.Ask<GetEpochResponse>(GetEpoch.Instance);
            if (expectedEpoch != epoch.Epoch || Cnx() == null)
            {
                // In this case, the cnx passed to this method is no longer the active connection. This method will get
                // called again once the new connection registers the producer with the broker.
                _log.Info($"[{Topic}][{_producerName}] Producer epoch mismatch or the current connection is null. Skip re-sending the  {_pendingMessages.messagesCount} pending messages since they will deliver using another connection.");
                return;
            }
            var protocolVersionResponse = await _cnx.Ask<RemoteEndpointProtocolVersionResponse>(RemoteEndpointProtocolVersion.Instance).ConfigureAwait(false);
            
            var stripChecksum = protocolVersionResponse.Version < BrokerChecksumSupportedVersion();
            var pendingMessages = _pendingMessages;
            OpSendMsg<T> pendingRegisteringOp = null;
            foreach (var op in pendingMessages)
            {
                if (from != null)
                {
                    if (op.Msg == from)
                    {
                        from = null;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (op.Msg != null)
                {
                    if (op.Msg.GetSchemaState() == Message<T>.SchemaState.None)
                    {
                        if (!RePopulateMessageSchema(op.Msg))
                        {
                            pendingRegisteringOp = op;
                            break;
                        }
                    }
                    else if (op.Msg.GetSchemaState() == Message<T>.SchemaState.Broken)
                    {
                        _pendingMessages.Remove();
                        op.Msg.Recycle();
                        continue;
                    }
                }
                if (op.Cmd.IsEmpty)
                {
                    if (IsMessageSizeExceeded(op))
                    {
                        continue;
                    }
                }
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{Topic}] [{_producerName}] Re-Sending message in sequenceId {op.Msg.SequenceId}");
                }
                //SendCommand(op);
                op.UpdateSentTimestamp();
                _stats.UpdateNumMsgsSent(op.NumMessagesInBatch, op.BatchSizeByte);
            }
            if (!State.ChangeToReadyState())
            {
                // Producer was closed while reconnecting, close the connection to make sure the broker
                // drops the producer on its side
                _cnx.Tell(Close.Instance);
                return;
            }
            // If any messages were enqueued while the producer was not Ready, we would have skipped
            // scheduling the batch flush task. Schedule it now, if there are messages in the batch container.
            if (BatchMessagingEnabled && !_batchMessageContainer.Empty)
            {
                MaybeScheduleBatchFlushTask();
            }
            if (pendingRegisteringOp != null)
            {
                await TryRegisterSchema(pendingRegisteringOp.Msg, pendingRegisteringOp.Callback, expectedEpoch);
            }
        }
        /// <summary>
		///  Check if final message size for non-batch and non-chunked messages is larger than max message size.
		/// </summary>
		private bool IsMessageSizeExceeded(OpSendMsg<T> op)
        {
            if (op.Msg != null && !Conf.ChunkingEnabled)
            {
                var messageSize = op.MessageHeaderAndPayloadSize;
                if (messageSize > _maxMessageSize)
                {
                    //ReleaseSemaphoreForSendOp(Op);
                    op.SendComplete(new PulsarClientException.InvalidMessageException($"The producer {_producerName} of the topic {Topic} sends a message with {messageSize} bytes that exceeds {_maxMessageSize} bytes"));
                    return true;
                }
            }
            return false;
        }
        public virtual long DelayInMillis
        {
            get
            {
                var firstMsg = _pendingMessages.FirstOrDefault();
                if (firstMsg.Msg != null)
                {
                    return DateTimeHelper.CurrentUnixTimeMillis() - firstMsg.CreatedAt;
                }
                return 0L;
            }
        }

        public virtual string ConnectionId
        {
            get
            {
                return Cnx() != null ? _connectionId : null;
            }
        }
        private bool HasPublishTime(ulong seq)
        {
            if (seq > 0)
                return true;
            return false;
        }

        public virtual string ConnectedSince
        {
            get
            {
                return _connectedSince;
            }
        }

        private int PendingQueueSize
        {
            get
            {
                if (BatchMessagingEnabled)
                {
                    return _pendingMessages.MessagesCount() + _batchMessageContainer.NumMessagesInBatch;
                }
                return _pendingMessages.MessagesCount();
            }
            
        }

        protected internal override async ValueTask<string> ProducerName()
        {
            return await Task.FromResult(_producerName);
        }

        // wrapper for connection methods
        private IActorRef Cnx()
        {
            if (!_cnx.IsNobody())
                return _cnx;

            throw new Exception("IActorRef null");
           /* var response = await _connectionHandler.Ask<AskResponse>(GetCnx.Instance);
            if (response.Data == null)
                return null;

            return response.ConvertTo<IActorRef>();*/
        }

        private void ConnectionClosed(IActorRef cnx)
        {
            _connectionHandler.Tell(new ConnectionClosed(cnx));
        }

        internal IActorRef ClientCnx()
        {
            return Cnx();
        }
        // / <summary>
        // / Compress the payload if compression is configured </summary>
        // / <param name="payload"> </param>
        // / <returns> a new payload </returns>
        private byte[] ApplyCompression(byte[] payload)
        {
            return _compressor.Encode(payload);
        }
        protected internal override async ValueTask<IProducerStats> Stats() => await Task.FromResult(_stats);

        internal class ChunkedMessageCtx
        {
            protected internal MessageId FirstChunkMessageId;
            protected internal MessageId LastChunkMessageId;
            protected internal int TotalChunks = -1;

            public virtual ChunkMessageId ChunkMessageId
            {
                get
                {
                    return new ChunkMessageId(FirstChunkMessageId, LastChunkMessageId);
                }
            }
            protected internal void Deallocate()
            {
                FirstChunkMessageId = null;
                LastChunkMessageId = null;

            }

            internal static ChunkedMessageCtx Get(int totalChunks)
            {
                ChunkedMessageCtx ctx = new ChunkedMessageCtx
                {
                    TotalChunks = totalChunks
                };
                return ctx;
            }

            internal virtual void Recycle()
            {
                TotalChunks = -1;
            }
        }

        /// <summary>
        /// Queue implementation that is used as the pending messages queue.
        /// 
        /// This implementation postpones adding of new OpSendMsg entries that happen
        /// while the forEach call is in progress. This is needed for preventing
        /// ConcurrentModificationExceptions that would occur when the forEach action
        /// calls the add method via a callback in user code.
        /// 
        /// This queue is not thread safe.
        /// </summary>
        protected internal class OpSendMsgQueue : IEnumerable<OpSendMsg<T>>
        {
            internal readonly List<OpSendMsg<T>> Delegate = new List<OpSendMsg<T>>();
            internal int ForEachDepth = 0;
            internal IList<OpSendMsg<T>> PostponedOpSendMgs;
            internal readonly AtomicInteger messagesCount = new AtomicInteger(0);

            
            public  void ForEach(Action<OpSendMsg<T>> action)
            {
                try
                {
                    // track any forEach call that is in progress in the current call stack
                    // so that adding a new item while iterating doesn't cause ConcurrentModificationException
                    ForEachDepth++;
                    Delegate.ForEach(action);
                }
                finally
                {
                    ForEachDepth--;
                    // if this is the top-most forEach call and there are postponed items, add them
                    if (ForEachDepth == 0 && PostponedOpSendMgs != null && PostponedOpSendMgs.Count > 0)
                    {
                        Delegate.AddRange(PostponedOpSendMgs);
                        PostponedOpSendMgs.Clear();
                    }
                }
            }

            public virtual bool Add(OpSendMsg<T> O)
            {
                // postpone adding to the queue while forEach iteration is in progress
                messagesCount.GetAndAdd(O.NumMessagesInBatch);
                if (ForEachDepth > 0)
                {
                    if (PostponedOpSendMgs == null)
                    {
                        PostponedOpSendMgs = new List<OpSendMsg<T>>();
                    }
                    PostponedOpSendMgs.Add(O);
                    return true;
                }
                else
                {
                    Delegate.Add(O);
                    return false;   
                }
            }

            public virtual void Clear()
            {
                Delegate.Clear();
                messagesCount.SetValue(0);
            }

            public virtual void Remove()
            {
                var op = Delegate.First();
                if (op != null)
                {

                    Delegate.Remove(op);
                    messagesCount.GetAndAdd(-op.NumMessagesInBatch);
                }
            }

            public virtual OpSendMsg<T> Peek()
            {
                return Delegate.FirstOrDefault();
            }

            public virtual int MessagesCount()
            {
                return messagesCount.GetValue();
            }


            IEnumerator<OpSendMsg<T>> IEnumerable<OpSendMsg<T>>.GetEnumerator()
            {
                return Delegate.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
        protected internal sealed class OpSendMsg<T1>
        {
            internal Message<T1> Msg;
            internal IList<Message<T1>> Msgs;
            internal ReadOnlySequence<byte> Cmd;
            internal long SequenceId;
            internal long CreatedAt;
            internal long FirstSentAt;
            internal long LastSentAt;
            internal int RetryCount;
            internal long UncompressedSize;
            internal long HighestSequenceId;
            internal SendCallback<T1> Callback;
            internal long BatchSizeByte = 0;
            internal int NumMessagesInBatch = 1;
            internal int TotalChunks = 0;
            internal int ChunkId = -1;
            internal ChunkedMessageCtx ChunkedMessageCtx;
            internal void Initialize()
            {
                Msg = null;
                Msgs = null;
                Cmd = ReadOnlySequence<byte>.Empty;
                Callback = null;
                SequenceId = -1L;
                CreatedAt = -1L;
                FirstSentAt = -1L;
                LastSentAt = -1L;
                HighestSequenceId = -1L;
                TotalChunks = 0;
                ChunkId = -1;
                UncompressedSize = 0;
                RetryCount = 0;
                BatchSizeByte = 0;
                NumMessagesInBatch = 1;
                ChunkedMessageCtx = null;
            }
           
            internal static OpSendMsg<T1> Create(Message<T1> msg, ReadOnlySequence<byte> cmd, long sequenceId, SendCallback<T1> callback)
            {
                var op = new OpSendMsg<T1>();
                op.Initialize();
                op.Msg = msg;
                op.Cmd = cmd;
                op.SequenceId = sequenceId;
                op.CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
                op.Callback = callback;
                op.UncompressedSize = msg.UncompressedSize;
                return op;
            }

            internal static OpSendMsg<T1> Create(IList<Message<T1>> msgs, ReadOnlySequence<byte> cmd, long sequenceId, SendCallback<T1> callback)
            {
                var op = new OpSendMsg<T1>();
                op.Initialize();
                op.Msgs = msgs;
                op.Cmd = cmd;
                op.SequenceId = sequenceId;
                op.CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
                op.Callback = callback;
                op.UncompressedSize = 0;
                for (var i = 0; i < msgs.Count; i++)
                {
                    op.UncompressedSize += msgs[i].UncompressedSize;
                }
                return op;
            }

            internal static OpSendMsg<T1> Create(IList<Message<T1>> msgs, ReadOnlySequence<byte> cmd, long lowestSequenceId, long highestSequenceId, SendCallback<T1> callback)
            {
                var op = new OpSendMsg<T1>();
                op.Initialize();
                op.Msgs = msgs;
                op.Cmd = cmd;
                op.SequenceId = lowestSequenceId;
                op.HighestSequenceId = highestSequenceId;
                op.CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
                op.Callback = callback;
                op.UncompressedSize = 0;
                for (var i = 0; i < msgs.Count; i++)
                {
                    op.UncompressedSize += msgs[i].UncompressedSize;
                }
                return op;
            }
            internal void SendComplete(in Exception e)
            {
                var callback = Callback;
                if (null != callback)
                {
                    var finalEx = e;
                    if (finalEx != null && finalEx is PulsarClientException.TimeoutException)
                    {
                        var te = (PulsarClientException.TimeoutException)e;
                        var sequenceId = te.SequenceId;
                        var ns = DateTimeHelper.CurrentUnixTimeMillis();
                        var errMsg = string.Format("{0} : createdAt {1} ns ago, firstSentAt {2} ns ago, lastSentAt {3} ns ago, retryCount {4}", te.Message, ns - CreatedAt, FirstSentAt <= 0 ? ns - LastSentAt : ns - FirstSentAt, ns - LastSentAt, RetryCount);

                        finalEx = new PulsarClientException.TimeoutException(errMsg, sequenceId);
                    }

                    callback.SendComplete(finalEx);
                }
            }
            internal void UpdateSentTimestamp()
            {
                LastSentAt = DateTimeHelper.CurrentUnixTimeMillis();
                if (FirstSentAt == -1L)
                {
                    FirstSentAt = LastSentAt;
                }
                ++RetryCount;
            }


            internal void SetMessageId(long ledgerId, long entryId, int partitionIndex)
            {
                if (Msg != null)
                {
                    Msg.SetMessageId(new MessageId(ledgerId, entryId, partitionIndex));
                }
                else
                {
                    for (var batchIndex = 0; batchIndex < Msgs.Count; batchIndex++)
                    {
                        Msgs[batchIndex].SetMessageId(new BatchMessageId(ledgerId, entryId, partitionIndex, batchIndex));
                    }
                }
            }
            internal ChunkMessageId MessageId
            {
                set
                {
                    if (Msg != null)
                    {
                        Msg.MessageId = value;
                    }
                }
            }

            public int MessageHeaderAndPayloadSize
            {
                get
                {
                    
                    if (Cmd.Length == 0)
                    {
                        return 0;
                    }
                    using var cmdHeader = new MemoryStream(Cmd.First.ToArray());
                    using var reader = new BinaryReader(cmdHeader);
                    var sizeStream = cmdHeader.Length;
                    cmdHeader.Seek(4L, SeekOrigin.Begin);
                    var totalSize = reader.ReadInt32().IntFromBigEndian(); ;
                    var cmdSize = reader.ReadInt32().IntFromBigEndian(); 
                    // The totalSize includes:
                    // | cmdLength | cmdSize | magic and checksum | msgMetadataLength | msgMetadata |
                    // | --------- | ------- | ------------------ | ----------------- | ----------- |
                    // | 4         |         | 6                  | 4                 |             |
                    var msgHeadersAndPayloadSize = totalSize - 4 - cmdSize - 6 - 4;
                    //cmdHeader.resetReaderIndex();
                    return msgHeadersAndPayloadSize;
                }
            }
            internal void Recycle()
            {
                Msg = null;
                Msgs = null;
                Cmd = ReadOnlySequence<byte>.Empty;
                SequenceId = -1L;
                CreatedAt = -1L;
                HighestSequenceId = -1L;
                FirstSentAt = -1L;
                LastSentAt = -1L;
                TotalChunks = 0;
                ChunkId = -1;
                NumMessagesInBatch = 1;
                BatchSizeByte = 0;
                ChunkedMessageCtx = null;
            }
        }
        private long Size(object o)
        {

            var resultBytes = JsonSerializer.SerializeToUtf8Bytes(o,
                    new JsonSerializerOptions { WriteIndented = false, IgnoreNullValues = true });
            return resultBytes.Length;

        }
    }
    public record struct RunSendTimeout
    {
        internal static RunSendTimeout Instance = new RunSendTimeout();
    }
    public record struct GetReceivedMessageIdsResponse
    {
        public readonly List<AckReceived> MessageIds;
        public GetReceivedMessageIdsResponse(HashSet<AckReceived> ids)
        {
            MessageIds = new List<AckReceived>(ids.ToArray());
        }
    }
    public record struct BatchTask
    {
        internal static BatchTask Instance = new BatchTask();
    }
}