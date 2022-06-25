﻿using Akka.Actor;
using Akka.Event;
using Akka.Util;
using Akka.Util.Internal;
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
using SharpPulsar.Interfaces.ISchema;
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static SharpPulsar.Exceptions.PulsarClientException;

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
        /*private bool InstanceFieldsInitialized = false;

        private void InitializeInstanceFields()
        {
            lastSendFutureWrapper = LastSendFutureWrapper.Create(lastSendFuture);
        }
        private sealed class LastSendFutureWrapper
        {
            internal readonly MessageId LastSendFuture;
            internal const int FALSE = 0;
            internal const int TRUE = 1;
            internal static readonly LastSendFutureWrapper ThrowOnceUpdater;
            internal volatile int ThrowOnce = FALSE;

            internal LastSendFutureWrapper(MessageId lastSendFuture)
            {
                this.LastSendFuture = lastSendFuture;
            }
            internal static LastSendFutureWrapper Create(MessageId lastSendFuture)
            {
                return new LastSendFutureWrapper(lastSendFuture);
            }
            public void HandleOnce()
            {
                return LastSendFuture.handle((ignore, t) =>
                {
                    if (t != null && ThrowOnceUpdater.compareAndSet(this, FALSE, TRUE))
                    {
                        throw FutureUtil.WrapToCompletionException(t);
                    }
                    return null;
                });
            }
        }
        */
        // Producer id, used to identify a producer within a single connection
        private readonly long _producerId;

		// Variable is used through the atomic updater
		private long _msgIdGenerator;
		private ICancelable _sendTimeout;
		private readonly long _createProducerTimeout;
		private readonly IBatchMessageContainerBase<T> _batchMessageContainer;
		private Queue<OpSendMsg<T>> _pendingMessages;
		private readonly IActorRef _generator;
        private readonly Dictionary<string, IActorRef> _watchedActors = new Dictionary<string, IActorRef>();

		private readonly ICancelable _regenerateDataKeyCipherCancelable;

		// Globally unique producer name
		private string _producerName;
		private readonly bool _userProvidedProducerName = false;
        private TaskCompletionSource<Message<T>> _lastSendFuture;


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

		private ICancelable _batchTimerTask;

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

        public ProducerActor(long producerid, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ProducerConfigurationData conf, TaskCompletionSource<IActorRef> producerCreatedFuture, int partitionIndex, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData clientConfiguration, Option<string> overrideProducerName) : base(client, lookup, cnxPool, topic, conf, producerCreatedFuture, schema, interceptors, clientConfiguration)
		{
            Self.Path.WithUid(producerid);
            /*if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }
            */
            
            
            _memoryLimitController = new MemoryLimitController(clientConfiguration.MemoryLimitBytes);
            _isTxnEnabled = clientConfiguration.EnableTransaction;
			_self = Self;
			_generator = idGenerator;
			_scheduler = Context.System.Scheduler;
			_log = Context.GetLogger();
			_producerId = producerid;
			_producerName = conf.ProducerName;
            
			if(!string.IsNullOrWhiteSpace(_producerName))
			{
				_userProvidedProducerName = true;
			}
			_partitionIndex = partitionIndex;
			_pendingMessages = new Queue<OpSendMsg<T>>(conf.MaxPendingMessages);
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

			if(conf.EncryptionEnabled)
			{
				var logCtx = "[" + topic + "] [" + _producerName + "] [" + _producerId + "]";

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
						msgCryptoBc = new MessageCrypto(logCtx, true, _log);
					}
					catch(Exception e)
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

			if(_msgCrypto != null)
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

			if(conf.SendTimeoutMs.TotalMilliseconds > 0)
			{
				_sendTimeout = _scheduler.ScheduleTellOnceCancelable(conf.SendTimeoutMs, Self, RunSendTimeout.Instance, ActorRefs.NoSender);
			}
            
            _lookupDeadline = TimeSpan.FromMilliseconds(DateTimeHelper.CurrentUnixTimeMillis() + clientConfiguration.LookupTimeout.TotalMilliseconds);
            _connectionHandler = Context.ActorOf(ConnectionHandler.Prop(clientConfiguration, State, new BackoffBuilder().SetInitialTime(TimeSpan.FromMilliseconds(clientConfiguration.InitialBackoffIntervalMs)).SetMax(TimeSpan.FromMilliseconds(clientConfiguration.MaxBackoffIntervalMs)).SetMandatoryStop(TimeSpan.FromMilliseconds(0)).Create(), Self));

			_createProducerTimeout = DateTimeHelper.CurrentUnixTimeMillis() + (long)clientConfiguration.OperationTimeout.TotalMilliseconds;
			if(conf.BatchingEnabled)
			{
				var containerBuilder = conf.BatcherBuilder;
				if(containerBuilder == null)
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
            Ready();
            GrabCnx();
        }
		
        public static Props Prop(long producerid, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ProducerConfigurationData conf, TaskCompletionSource<IActorRef> producerCreatedFuture, int partitionIndex, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData clientConfiguration, Option<string> overrideProducerName)
        {
            return Props.Create(()=> new ProducerActor<T>(producerid, client, lookup, cnxPool, idGenerator, topic, conf, producerCreatedFuture, partitionIndex, schema, interceptors, clientConfiguration, overrideProducerName));
        }
        //producerCreatedFuture used from here
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
			Receive<RunSendTimeout>( _ => {
				FailTimedoutMessages();
			});
            Receive<Messages.Terminated>(x =>
                {
                    Terminated(x.ClientCnx);
                }
            );
            Receive<RecoverChecksumError>(x =>
                {
                    RecoverChecksumError(x.ClientCnx, x.SequenceId);
                }
            );
            Receive<RecoverNotAllowedError>(x =>
                {
                    RecoverNotAllowedError(x.SequenceId);
                }
            );
			ReceiveAsync<BatchTask>( async _ => {
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
                catch(Exception ex)
                {
					_log.Error(ex.ToString());
				}
			});
			ReceiveAsync<InternalSendWithTxn<T>>( async m =>
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
            _connectionHandler.Tell(new SetCnx(o.ClientCnx));
			_cnx = o.ClientCnx;
			_maxMessageSize = o.MaxMessageSize;
			_cnx.Tell(new RegisterProducer(_producerId, _self));
			_protocolVersion = o.ProtocolVersion;
            _producerDeadline = TimeSpan.FromMilliseconds(DateTimeHelper.CurrentUnixTimeMillis() + ClientConfiguration.OperationTimeout.TotalMilliseconds);

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
						if (Commands.PeerSupportJsonSchemaAvroFormat(protocolVersion))
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
            var response  = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
            _requestId = response.Id;
            var epochResponse = await _connectionHandler.Ask<GetEpochResponse>(GetEpoch.Instance);
            var epoch = epochResponse.Epoch;
            _log.Info($"[{Topic}] [{_producerName}] Creating producer on cnx {_cnx.Path.Name}");
            var cmd = Commands.NewProducer(base.Topic, _producerId, _requestId, _producerName, Conf.EncryptionEnabled, _metadata, _schemaInfo, epoch, _userProvidedProducerName, Conf.AccessMode, _topicEpoch, _isTxnEnabled, Conf.InitialSubscriptionName);
            var payload = new Payload(cmd, _requestId, "NewProducer");
            await _cnx.Ask<AskResponse>(payload).ContinueWith( async response=>
             {
                 var isFaulted = response.IsFaulted;
                 var request = isFaulted ? null : response.Result;
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
                     switch (ex)
                     {
                         case IncompatibleSchemaException se:
                             _log.Error($"Failed to connect producer on IncompatibleSchemaException: {Topic}");
                             ProducerCreatedFuture.TrySetException(se);
                             break;

                         case TopicDoesNotExistException e:
                             _log.Error($"Failed to close producer on TopicDoesNotExistException: {Topic}");
                             ProducerCreatedFuture.TrySetException(e);
                             break;

                         case ProducerBlockedQuotaExceededException e:
                             _log.Warning($"[{Topic}] [{_producerName}] Topic backlog quota exceeded. Throwing Exception on producer.");
                             if (_log.IsDebugEnabled)
                             {
                                 _log.Debug($"[{Topic}] [{_producerName}] Pending messages: {_pendingMessages.Count}");
                             }
                             var pe = new ProducerBlockedQuotaExceededException($"The backlog quota of the topic {Topic} that the producer {_producerName} produces to is exceeded");

                             FailPendingMessages(_cnx, pe);
                             //ProducerCreatedFuture.TrySetException(pe);
                             break;
                         case ProducerBlockedQuotaExceededError pexe:
                             _log.Warning($"[{_producerName}] [{Topic}] Producer is blocked on creation because backlog exceeded on topic.");

                             ProducerCreatedFuture.TrySetException(pexe);
                             break;
                         case ProducerBusyException busy:
                             _log.Warning($"[{_producerName}] [{Topic}] Producer is busy, dear.");
                             ProducerCreatedFuture.TrySetException(busy);
                             break;
                         case TopicTerminatedException tex:
                             State.ConnectionState = HandlerState.State.Terminated;
                             FailPendingMessages(_cnx, tex);
                             Client.Tell(new CleanupProducer(_self));
                             break;
                         case ProducerFencedException pfe:
                             State.ConnectionState = HandlerState.State.ProducerFenced;
                             FailPendingMessages(_cnx, pfe);
                             Client.Tell(new CleanupProducer(Self));
                             break;
                         default:
                             {
                                 if ( ProducerCreatedFuture.Task.IsCompleted || (ex is PulsarClientException && IsRetriableError(ex) && DateTimeHelper.CurrentUnixTimeMillis() < _createProducerTimeout))
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
                             break;
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
                         if (!ProducerCreatedFuture.Task.IsCompleted && BatchMessagingEnabled)
                         {
                             _batchTimerTask = _scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromMilliseconds(Conf.BatchingMaxPublishDelayMs), 
                                 TimeSpan.FromMilliseconds(Conf.BatchingMaxPublishDelayMs), async()=> 
                                 {
                                     if (_log.IsDebugEnabled)
                                     {
                                         _log.Debug($"[{Topic}] [{_producerName}] Batching the messages from the batch container from timer thread");
                                     }
                                     if ( State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
                                     {
                                         return;
                                     }
                                     await BatchMessageAndSend();
                                 });
                         }
                         await ResendMessages(res);
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

			await BatchMessageAndSend();
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
			if(_multiSchemaMode != MultiSchemaMode.Auto)
			{
				return _multiSchemaMode == MultiSchemaMode.Enabled;
			}
			if(autoEnable)
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

		internal override async ValueTask InternalSend(IMessage<T> message, TaskCompletionSource<Message<T>> future)
        {
            _sender = Sender;
			var interceptorMessage = (Message<T>) BeforeSend(message);
			if(Interceptors != null)
			{
				_ = interceptorMessage.Properties;
            }
            var callback = new SendCallback<T>(this, future, interceptorMessage);
            await Send(interceptorMessage, callback);
		}
		private async ValueTask InternalSendWithTxn(IMessage<T> message, IActorRef txn, TaskCompletionSource<Message<T>> callback)
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

			if (Conf.ChunkingEnabled && Conf.MaxMessageSize > 0)
				maxMessageSize = Math.Min(Conf.MaxMessageSize, maxMessageSize);

			if (!IsValidProducerState(callback, message.SequenceId))
			{
                return;
			}

			var msg = (Message<T>) message;
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

                // send in chunks
                var totalChunks = CanAddToBatch(msg) ? 1 : Math.Max(1, compressedPayload.Length) / maxMessageSize + (Math.Max(1, compressedPayload.Length) % maxMessageSize == 0 ? 0 : 1);
                // chunked message also sent individually so, try to acquire send-permits
                /*for (var i = 0; i < (totalChunks - 1); i++)
                {
                    if (!CanEnqueueRequest(message.SequenceId))
                    {
                        return;
                    }
                }
                */
                var readStartIndex = 0;
				var sequenceId = _msgIdGenerator;

                if (msgMetadata.SequenceId == 0)
                {
                    msgMetadata.SequenceId = (ulong)sequenceId;
                    _msgIdGenerator++;
                }
                else
                {
                    sequenceId = (long)msgMetadata.SequenceId;
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
                    if(chunkId > 0)
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
                    await SerializeAndSendMessage(msg, payload, msgMetadata, sequenceId, uuid, chunkId, totalChunks, readStartIndex, maxMessageSize, compressedPayload, compressed, compressedPayload.Length, uncompressedSize, callback, chunkedMessageCtx);
					readStartIndex = ((chunkId + 1) * maxMessageSize);
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
			if(totalChunks > 1 && TopicName.Get(Topic).Persistent)
			{
				chunkPayload = compressedPayload.Slice(readStartIndex, 
                    Math.Min(chunkMaxSizeInBytes, chunkPayload.Length - readStartIndex));
				if (chunkId != totalChunks - 1)
				{
					chunkMsgMetadata = msgMetadata;
				}
				if(!string.IsNullOrWhiteSpace(uuid))
				{
					chunkMsgMetadata.Uuid = uuid;
				}
				chunkMsgMetadata.ChunkId = chunkId;
				chunkMsgMetadata.NumChunksFromMsg = totalChunks;
				chunkMsgMetadata.TotalChunkMsgSize = compressedPayloadSize;
			}
			if(!HasPublishTime(chunkMsgMetadata.PublishTime))
			{
				chunkMsgMetadata.PublishTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (!string.IsNullOrWhiteSpace(chunkMsgMetadata.ProducerName))
                {
                    _log.Warning($"changing producer name from '{chunkMsgMetadata.ProducerName}' to ''. Just helping out ;)");
                    chunkMsgMetadata.ProducerName = string.Empty;
                }
                chunkMsgMetadata.ProducerName = _producerName;
                if (Conf.CompressionType != CompressionType.None)
                {
                    chunkMsgMetadata.Compression = CompressionCodecProvider.ConvertToWireProtocol(Conf.CompressionType);
                }
                chunkMsgMetadata.UncompressedSize = (uint)uncompressedSize;
            }
            
            if (CanAddToBatch(msg) && totalChunks <= 1)
			{
                if (CanAddToCurrentBatch(msg))
				{
					// should trigger complete the batch message, new message will add to a new batch and new batch
					// sequence id use the new message, so that broker can handle the message duplication
					if(sequenceId <= LastSequenceIdPushed)
					{
						_isLastSequenceIdPotentialDuplicated = true;
						if(sequenceId <= _lastSequenceIdPublished)
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
						if(_isLastSequenceIdPotentialDuplicated)
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
								await BatchMessageAndSend();
							}
                            /*else
                            {
                                callback.Future.TrySetResult(null);
                            }*/
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
				if(msg.GetSchemaState() == Message<T>.SchemaState.Ready)
				{
					var cmd = SendMessage(_producerId, (long)sequenceId, numMessages, msgMetadata, encryptedPayload);
					op = OpSendMsg<T>.Create(msg, cmd, (long)sequenceId, callback);
				}
				else
				{
					op = OpSendMsg<T>.Create(msg, ReadOnlySequence<byte>.Empty, (long)sequenceId, callback);
                    var finalMsgMetadata = msgMetadata;
                    op.Cmd = SendMessage(_producerId, (long)sequenceId, numMessages, finalMsgMetadata, encryptedPayload);
				}
				op.NumMessagesInBatch = numMessages;
				op.BatchSizeByte = encryptedPayload.Length;
				if(totalChunks > 1)
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
			if(msg.SchemaInternal() == Schema)
			{
				if (_schemaVersion.HasValue)
					msg.Metadata.OriginalMetadata.SchemaVersion = _schemaVersion.Value;

				msg.SetSchemaState(Message<T>.SchemaState.Ready);
				return true;
			}
			if(!IsMultiSchemaEnabled(true))
			{
				var exception = new PulsarClientException.InvalidMessageException($"The producer {_producerName} of the topic {Topic} is disabled the `MultiSchema`: Seq:{msg.SequenceId}");
                callback.Future.TrySetException(exception);
                return false;
			}
			var schemaHash = SchemaHash.Of(msg.Schema);
			if(SchemaCache.TryGetValue(schemaHash, out var schemaVersion))
            {
                msg.Metadata.OriginalMetadata.SchemaVersion = schemaVersion;
				msg.SetSchemaState(Message<T>.SchemaState.Ready);
			}
			return true;
		}

		private bool RePopulateMessageSchema(Message<T> msg)
		{
			var schemaHash = SchemaHash.Of(msg.Schema);
			if(!SchemaCache.TryGetValue(schemaHash, out var schemaVersion))
            {
				return false;
            }
			msg.Metadata.OriginalMetadata.SchemaVersion = schemaVersion;
			msg.SetSchemaState(Message<T>.SchemaState.Ready);
			return true;
		}

		private async ValueTask TryRegisterSchema(Message<T> msg)
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
            if (!Commands.PeerSupportsGetOrCreateSchema(_protocolVersion))
            {
                var err = new PulsarClientException.NotSupportedException($"The command `GetOrCreateSchema` is not supported for the protocol version {_protocolVersion}. The producer is {_producerName}, topic is {base.Topic}");
                _log.Error(err.ToString());
            }
            var request = Commands.NewGetOrCreateSchema(_requestId, base.Topic, _schemaInfo);
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
                    _log.Error(ex.ToString());
                }
                else
                {
                    _log.Error($"[{Topic}] [{_producerName}] GetOrCreateSchema failed: [{schemaResponse.Response.ErrorCode}:{schemaResponse.Response.ErrorMessage}]");
                    msg.SetSchemaState(Message<T>.SchemaState.None);
                }
            }

            await RecoverProcessOpSendMsgFrom(msg);
        }

		private byte[] EncryptMessage(MessageMetadata msgMetadata, byte[] compressedPayload)
		{

			var encryptedPayload = compressedPayload;
			if(!Conf.EncryptionEnabled || _msgCrypto == null)
			{
				return encryptedPayload;
			}
			try
			{
				encryptedPayload = _msgCrypto.Encrypt(Conf.EncryptionKeys, Conf.CryptoKeyReader, msgMetadata, compressedPayload);
			}
			catch(PulsarClientException e)
			{
				// Unless config is set to explicitly publish un-encrypted message upon failure, fail the request
				if(Conf.CryptoFailureAction == ProducerCryptoFailureAction.Send)
				{
					_log.Warning($"[{Topic}] [{_producerName}] Failed to encrypt message {e}. Proceeding with publishing unencrypted message");
					return compressedPayload;
				}
				throw e;
			}
			return encryptedPayload;
		}

		private ReadOnlySequence<byte> SendMessage(long producerId, long sequenceId, int numMessages, MessageMetadata msgMetadata, byte[] compressedPayload)
		{
			_log.Info($"Send message with {_producerName}:{producerId}");
			return Commands.NewSend(producerId, sequenceId, numMessages, msgMetadata, new ReadOnlySequence<byte>(compressedPayload));
		}

        public IStash Stash { get; set; }

        private bool CanAddToBatch(Message<T> msg)
		{
			return msg.GetSchemaState() == Message<T>.SchemaState.Ready && BatchMessagingEnabled && !msg.Metadata.OriginalMetadata.ShouldSerializeDeliverAtTime();
		}

		private bool CanAddToCurrentBatch(Message<T> msg)
		{
			return _batchMessageContainer.HaveEnoughSpace(msg) && (!IsMultiSchemaEnabled(false) || _batchMessageContainer.HasSameSchema(msg)) && _batchMessageContainer.HasSameTxn(msg);
		}

		private async ValueTask DoBatchSendAndAdd(Message<T> msg, SendCallback<T> callback, byte[] payload)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}] [{_producerName}] Closing out batch to accommodate large message with size {msg.Data.Length}");
			}
			try
			{
				await BatchMessageAndSend();
				_batchMessageContainer.Add(msg, callback);
                _lastSendFuture = callback.Future;
			}
			catch(Exception ex)
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
			Close().ConfigureAwait(false);
			base.PostStop();
        }
        private async ValueTask Close()
		{
			var currentState = State.GetAndUpdateState(State.ConnectionState == HandlerState.State.Closed ? HandlerState.State.Closed : HandlerState.State.Closing);

			if(currentState == HandlerState.State.Closed || currentState == HandlerState.State.Closing)
			{
				return;
			}
					

			_stats.CancelStatsTimeout();

			var cnx = await Cnx();
			if(cnx == null || currentState != HandlerState.State.Ready)
			{
				_log.Info("[{}] [{}] Closed Producer (not connected)", Topic, _producerName);
				State.ConnectionState = HandlerState.State.Closed;
				Client.Tell(new CleanupProducer(Self));
				var ex = new AlreadyClosedException($"The producer {_producerName} of the topic {Topic} was already closed when closing the producers");
				_pendingMessages.ForEach(msg =>
				{
					msg.Msg.Recycle();
				});
				_pendingMessages.Clear();
			}

			var requestId = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).Id;
			var cmd = Commands.NewCloseProducer(_producerId, requestId);
			var response = await cnx.Ask(new SendRequestWithId(cmd, requestId));
			if (!(response is Exception))
			{
				_log.Info($"[{Topic}] [{_producerName}] Closed Producer", Topic, _producerName);
				State.ConnectionState = HandlerState.State.Closed;
				_pendingMessages.ForEach(msg =>
				{
					msg.Msg.Recycle();
				});
				_pendingMessages.Clear();
				Client.Tell(new CleanupProducer(Self));
			}
		}
        private bool ShouldWriteOpSendMsg()
        {
            return Connected();
        }
        protected internal override bool Connected()
		{
			var cnx = _cnx;
			return cnx != null && (State.ConnectionState == HandlerState.State.Ready);
		}
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
			if(previousState != HandlerState.State.Terminated && previousState != HandlerState.State.Closed)
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
        private void RecoverChecksumError(IActorRef cnx, long sequenceId)
        {
            if (!_pendingMessages.TryPeek(out var op))
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
                        var error = new PulsarClientException.ChecksumException($"The checksum of the message which is produced by producer {_producerName} to the topic '{Topic}' is corrupted");
                        _pendingMessages.TryDequeue(out op);
                        try
                        {

                            op.SendComplete(error);                            
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
        }
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
        private void RecoverNotAllowedError(long sequenceId)
        {
            if (_pendingMessages.TryPeek(out var op) && sequenceId == GetHighestSequenceId(op))
            {
                _pendingMessages.TryDequeue(out op);
                try
                {
                    op.SendComplete(new NotAllowedException($"The size of the message which is produced by producer {_producerName} to the topic '{Topic}' is not allowed"));
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
			if (!_pendingMessages.TryPeek(out var op))
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
				var msg = $"[{Topic}] [{_producerName}] Got ack for msg. expecting: {op.Msg.SequenceId} - {op.HighestSequenceId} - got: {ackReceived.SequenceId} - {ackReceived.HighestSequenceId} - queue-size: {_pendingMessages.Count}";
				_log.Warning(msg);
				// Force connection closing so that messages can be re-transmitted in a new connection
				cnx.Tell(Messages.Requests.Close.Instance);
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

                    _pendingMessages.TryDequeue(out op);
				}
				else
				{
					var msg = $"[{Topic}] [{_producerName}] Got ack for batch msg error. expecting: {op.SequenceId} - {op.HighestSequenceId} - got: {ackReceived.SequenceId} - {ackReceived.HighestSequenceId} - queue-size: {_pendingMessages.Count}";
					_log.Warning(msg);
					// Force connection closing so that messages can be re-transmitted in a new connection
					cnx.Tell(Messages.Requests.Close.Instance);
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

		private async ValueTask ResendMessages(ProducerResponse response)
		{
			var messagesToResend = _pendingMessages.Count;
			if (messagesToResend == 0)
			{
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"[{Topic}] [{_producerName}] No pending messages to resend {messagesToResend}");
				}

				if (State.ChangeToReadyState())
				{
                    ProducerCreatedFuture.TrySetResult(_self);
				}
				return;
			}
			_log.Info($"[{Topic}] [{_producerName}] Re-Sending {messagesToResend} messages to server");
			await RecoverProcessOpSendMsgFrom(null);
		}

        /// <summary>
        /// Process sendTimeout events
        /// </summary>
        private void FailTimedoutMessages()
		{
			if(_sendTimeout.IsCancellationRequested)
			{
				return;
			}

			long timeToWaitMs;
			// If it's closing/closed we need to ignore this timeout and not schedule next timeout.
			if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				return;
			}

			if (!_pendingMessages.TryPeek(out var firstMsg))
			{
				// If there are no pending messages, reset the timeout to the configured value.
				timeToWaitMs = (long)Conf.SendTimeoutMs.TotalMilliseconds;
			}
			else
			{
				// If there is at least one message, calculate the diff between the message timeout and the elapsed
				// time since first message was created.
				var diff = Conf.SendTimeoutMs.TotalMilliseconds - (DateTimeHelper.CurrentUnixTimeMillis() - firstMsg.CreatedAt);
				if (diff <= 0)
				{
					// The diff is less than or equal to zero, meaning that the message has been timed out.
					// Set the callback to timeout on every message, then clear the pending queue.
					_log.Info($"[{Topic}] [{_producerName}] Message send timed out. Failing {_pendingMessages.Count} messages");

					var te = new PulsarClientException.TimeoutException($"The producer {_producerName} can not send message to the topic {Topic} within given timeout: {Conf.SendTimeoutMs}");
					FailPendingMessages(_cnx, te);
					_stats.IncrementSendFailed(_pendingMessages.Count);
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
                await BatchMessageAndSend();
            }
            if (_lastSendFuture != null)
                await _lastSendFuture.Task;
        }

		private async ValueTask TriggerFlush()
		{
			if(BatchMessagingEnabled)
			{
				await BatchMessageAndSend();
			}
		}

		// must acquire semaphore before enqueuing
		private async ValueTask  BatchMessageAndSend()
		{
			if(_log.IsDebugEnabled)
			{
				_log.Info($"[{Topic}] [{_producerName}] Batching the messages from the batch container with {_batchMessageContainer.NumMessagesInBatch} messages");
			}
			if(!_batchMessageContainer.Empty)
			{
				try
				{
					IList<OpSendMsg<T>> opSendMsgs;
					if(_batchMessageContainer.MultiBatches)
					{
						opSendMsgs = _batchMessageContainer.CreateOpSendMsgs();
					}
					else
					{
						opSendMsgs = new List<OpSendMsg<T>> { _batchMessageContainer.CreateOpSendMsg() };
					}
					_batchMessageContainer.Clear();
					foreach(var opSendMsg in opSendMsgs)
					{
						await ProcessOpSendMsg(opSendMsg);
					}
				}
				catch(Exception t)
				{
					_log.Warning($"[{Topic}] [{_producerName}] error while create opSendMsg by batch message container: {t}");
				}
			}
		}
		private async  ValueTask ProcessOpSendMsg(OpSendMsg<T> op)
		{
			try
			{
				if (op.Msg != null && BatchMessagingEnabled)
				{
					await BatchMessageAndSend();
				}
				_pendingMessages.Enqueue(op);
				if (op.Msg != null)
				{
					LastSequenceIdPushed = Math.Max(LastSequenceIdPushed, GetHighestSequenceId(op));
				}
                if(ShouldWriteOpSendMsg())
                {
                    if (op.Msg != null && op.Msg.GetSchemaState() == Message<T>.SchemaState.None)
                    {
                        await TryRegisterSchema(op.Msg);
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

		private async ValueTask RecoverProcessOpSendMsgFrom(Message<T> from)
		{
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
						_pendingMessages = new Queue<OpSendMsg<T>>(_pendingMessages.Where(x => x.Msg != op.Msg));
						op.Msg.Recycle();
						continue;
					}
				}

				if (_log.IsDebugEnabled)
				{
					_log.Debug($"[{Topic}] [{_producerName}] Re-Sending message in sequenceId {op.Msg.SequenceId}");
				}
				SendCommand(op);
				op.UpdateSentTimestamp();
				_stats.UpdateNumMsgsSent(op.NumMessagesInBatch, op.BatchSizeByte);
			}
			if (pendingRegisteringOp != null)
            {
                await TryRegisterSchema(pendingRegisteringOp.Msg);
            }
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

		public virtual int PendingQueueSize
		{
			get
			{
				return _pendingMessages.Count;
			}
		}

		protected internal override async ValueTask<string> ProducerName()
		{
			return await Task.FromResult(_producerName);
		}

		// wrapper for connection methods
		private async ValueTask<IActorRef> Cnx()
		{
			var response = await _connectionHandler.Ask<AskResponse>(GetCnx.Instance);
            if (response.Data == null)
                return null;

            return response.ConvertTo<IActorRef>();
        }

		private void ConnectionClosed(IActorRef cnx)
		{
            _connectionHandler.Tell(new ConnectionClosed(cnx));
        }

		internal async ValueTask<IActorRef> ClientCnx()
		{
			return await Cnx();
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
            internal static OpSendMsg<T1> Create(Message<T1> msg, ReadOnlySequence<byte> cmd, long sequenceId, SendCallback<T1> callback)
			{
				var op = new OpSendMsg<T1>
				{
					Msg = msg,
					Cmd = cmd,
					SequenceId = sequenceId,
					CreatedAt = DateTimeHelper.CurrentUnixTimeMillis(),
                    Callback = callback,
                    UncompressedSize = msg.UncompressedSize
                };
				return op;
			}

            internal static OpSendMsg<T1> Create(IList<Message<T1>> msgs, ReadOnlySequence<byte> cmd, long sequenceId, SendCallback<T1> callback)
            {
                var op = new OpSendMsg<T1>
                {
                    Msgs = msgs,
                    Cmd = cmd,
                    SequenceId = sequenceId,
                    CreatedAt = DateTimeHelper.CurrentUnixTimeMillis(),
                    Callback = callback,
                    UncompressedSize = 0
                };
                for (var i = 0; i < msgs.Count; i++)
                {
                    op.UncompressedSize += msgs[i].UncompressedSize;
                }
                return op;
            }

            internal static OpSendMsg<T1> Create(IList<Message<T1>> msgs, ReadOnlySequence<byte> cmd, long lowestSequenceId, long highestSequenceId, SendCallback<T1> callback)
            {
                var op = new OpSendMsg<T1>
                {
                    Msgs = msgs,
                    Cmd = cmd,
                    SequenceId = lowestSequenceId,
                    HighestSequenceId = highestSequenceId,
                    CreatedAt = DateTimeHelper.CurrentUnixTimeMillis(),
                    Callback = callback,
                    UncompressedSize = 0
                };

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
                BatchSizeByte = 0 ;
                ChunkedMessageCtx = null;
            }

		}
	}
	internal sealed class RunSendTimeout
	{
		internal static RunSendTimeout Instance = new RunSendTimeout();
    }
	internal sealed class GetReceivedMessageIdsResponse
	{
        public readonly List<AckReceived> MessageIds;
        public GetReceivedMessageIdsResponse(HashSet<AckReceived> ids)
        {
            MessageIds = new List<AckReceived>(ids.ToArray());
        }
    }
	internal sealed class BatchTask
	{
		internal static BatchTask Instance = new BatchTask();
    }
}