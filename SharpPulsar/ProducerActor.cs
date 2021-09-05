using Akka.Actor;
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

		// Producer id, used to identify a producer within a single connection
		private readonly long _producerId;

		// Variable is used through the atomic updater
		private long _msgIdGenerator;
		private ICancelable _sendTimeout;
		private long _createProducerTimeout;
		private readonly IBatchMessageContainerBase<T> _batchMessageContainer;
		private Queue<(OpSendMsg<T> Msg, IActorRef Sender)> _pendingMessages;
		private IActorRef _generator;
        private readonly Dictionary<string, IActorRef> _watchedActors = new Dictionary<string, IActorRef>();

		private readonly ICancelable _regenerateDataKeyCipherCancelable;

		// Globally unique producer name
		private string _producerName;
		private bool _userProvidedProducerName = false;

		private ILoggingAdapter _log;

		private string _connectionId;
		private string _connectedSince;
		private readonly int _partitionIndex;
		private ISchemaInfo _schemaInfo;

		private readonly IProducerStatsRecorder _stats;

		private readonly CompressionCodec _compressor;

		private long _lastSequenceIdPublished;

		protected internal long LastSequenceIdPushed;
		private bool _isLastSequenceIdPotentialDuplicated;

		private readonly IMessageCrypto _msgCrypto;

		private ICancelable _keyGeneratorTask = null;

		private readonly IDictionary<string, string> _metadata;

		private Option<byte[]> _schemaVersion = null;

		private readonly IActorRef _connectionHandler;
		private readonly IActorRef _self;
		private IActorRef _sender;
		private int _protocolVersion;

		private ICancelable _batchTimerTask;

		private readonly IScheduler _scheduler;
		private IActorRef _replyTo;
		private long _requestId = -1;
		private long _maxMessageSize;
		private IActorRef _cnx;

        public ProducerActor(long producerid, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ProducerConfigurationData conf, int partitionIndex, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData clientConfiguration) : base(client, lookup, cnxPool, topic, conf, schema, interceptors, clientConfiguration)
		{
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
			_pendingMessages = new Queue<(OpSendMsg<T> Msg, IActorRef Sender)>(conf.MaxPendingMessages);

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
						Context.System.Log.Warning($"[{Topic}] [{ProducerName().GetAwaiter().GetResult()}] [{_producerId}] Failed to add public key cipher.");
						Context.System.Log.Error(e.ToString());
					}
				});
			}

			if(conf.SendTimeoutMs > 0)
			{
				_sendTimeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(conf.SendTimeoutMs), Self, RunSendTimeout.Instance, ActorRefs.NoSender);
			}

			_connectionHandler = Context.ActorOf(ConnectionHandler.Prop(clientConfiguration, State, new BackoffBuilder().SetInitialTime(TimeSpan.FromMilliseconds(clientConfiguration.InitialBackoffIntervalMs)).SetMax(TimeSpan.FromMilliseconds(clientConfiguration.MaxBackoffIntervalMs)).SetMandatoryStop(TimeSpan.FromMilliseconds(0)).Create(), Self));

			_createProducerTimeout = DateTimeHelper.CurrentUnixTimeMillis() + clientConfiguration.OperationTimeoutMs;
			if(conf.BatchingEnabled)
			{
				var containerBuilder = conf.BatcherBuilder;
				if(containerBuilder == null)
				{
					containerBuilder = IBatcherBuilder.Default(Context.System);
				}
				_batchMessageContainer = (IBatchMessageContainerBase<T>)containerBuilder.Build<T>();
				_batchMessageContainer.Producer = Self;
			}
			else
			{
				_batchMessageContainer = null;
			}
			if (clientConfiguration.StatsIntervalSeconds > 0)
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
		}
		private async ValueTask Connection(AskResponse response)
		{
            if (response.Failed)
            {
                _replyTo.Tell(response);
                return;
            }

            await ConnectionOpened(response.ConvertTo<ConnectionOpened>());
        }
		private void Ready()
        {
            ReceiveAsync<Connect>(async _ =>
            {
                _replyTo = Sender;
                var askResponse = await _connectionHandler.Ask<AskResponse>(new GrabCnx($"Create connection from producer: {_producerName}"));
                await Connection(askResponse);
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
			ReceiveAsync<ConnectionClosed>(async m => 
            {
				await ConnectionClosed(m.ClientCnx);
			});
			ReceiveAsync<Flush>(async _ => {
				await Flush();
			});
			Receive<GetProducerName>(_ => Sender.Tell(_producerName));
			Receive<GetLastSequenceId>(_ => Sender.Tell(_lastSequenceIdPublished));
			Receive<GetTopic>(_ => Sender.Tell(Topic));
			Receive<IsConnected>(_ => Sender.Tell(Connected()));
			Receive<GetStats>(_ => Sender.Tell(_stats));
			Receive<GetLastDisconnectedTimestamp>(_ => 
			{
				_replyTo = Sender;
				Become(LastDisconnectedTimestamp);
			});
            Receive<Akka.Actor.Terminated>(t => 
            {
                _watchedActors.Remove(t.ActorRef.Path.Name);
            });
			ReceiveAsync<InternalSend<T>>(async m => 
			{
                try
                {
                    Context.Watch(Sender);
                    _watchedActors.Add(Sender.Path.Name, Sender);
					await InternalSend(m.Message);
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
                    Context.Watch(Sender);
                    _watchedActors.Add(Sender.Path.Name, Sender);
                    await InternalSendWithTxn(m.Message, m.Txn);
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
			// we set the cnx reference before registering the producer on the cnx, so if the cnx breaks before creating the
			// producer, it will try to grab a new cnx
			_connectionHandler.Tell(new SetCnx(o.ClientCnx));
			_cnx = o.ClientCnx;
			_maxMessageSize = o.MaxMessageSize;
			_cnx.Tell(new RegisterProducer(_producerId, _self));
			_protocolVersion = o.ProtocolVersion;

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
            var cmd = Commands.NewProducer(base.Topic, _producerId, _requestId, _producerName, Conf.EncryptionEnabled, _metadata, _schemaInfo, epoch, _userProvidedProducerName, Conf.AccessMode);
            var payload = new Payload(cmd, _requestId, "NewProducer");
            var request = await _cnx.Ask<AskResponse>(payload);
            if (request.Failed)
            {
                var ex = request.Exception;
                _log.Error($"[{Topic}] [{_producerName}] Failed to create producer: {ex}");
                _cnx.Tell(new RemoveProducer(_producerId));
                if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
                {
                    //cx.Tell(PoisonPill.Instance);
                    _replyTo.Tell(new AskResponse(ex));
                }
                else if (ex is TopicDoesNotExistException e)
                {
                    _log.Error($"Failed to close producer on TopicDoesNotExistException: {Topic}");
                    _replyTo.Tell(new AskResponse(e));
                }
                else if (ex is ProducerBlockedQuotaExceededException)
                {
                    _log.Warning($"[{Topic}] [{_producerName}] Topic backlog quota exceeded. Throwing Exception on producer.");
                    var pe = new ProducerBlockedQuotaExceededException($"The backlog quota of the topic {Topic} that the producer {_producerName} produces to is exceeded");

                    FailPendingMessages(_cnx, pe);
                    _replyTo.Tell(new AskResponse(pe));
                }
                else if (ex is ProducerBlockedQuotaExceededError pexe)
                {
                    _log.Warning($"[{_producerName}] [{Topic}] Producer is blocked on creation because backlog exceeded on topic.");

                    _replyTo.Tell(new AskResponse(pexe));
                }
                else if (ex is ProducerBusyException busy)
                {
                    _log.Warning($"[{_producerName}] [{Topic}] Producer is busy, dear.");

                    _replyTo.Tell(new AskResponse(busy));
                }
                else if (ex is TopicTerminatedException tex)
                {
                    State.ConnectionState = HandlerState.State.Terminated;
                    FailPendingMessages(_cnx, tex);
                    Client.Tell(new CleanupProducer(Self));
                    _replyTo.Tell(new AskResponse(tex));
                }
                else if (ex is ProducerFencedException pfe)
                {
                    State.ConnectionState = HandlerState.State.ProducerFenced;
                    FailPendingMessages(_cnx, pfe);
                    Client.Tell(new CleanupProducer(Self));
                    _replyTo.Tell(new AskResponse(pfe));
                }
                else if ((ex is PulsarClientException && IsRetriableError(ex) && DateTimeHelper.CurrentUnixTimeMillis() < _createProducerTimeout))
                {
                    await ReconnectLater(ex);
                }
                else
                {
                    State.ConnectionState = HandlerState.State.Failed;
                    Client.Tell(new CleanupProducer(Self));
                    _replyTo.Tell(new AskResponse(new Exception()));
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
                    if (BatchMessagingEnabled)
                    {
                        _batchTimerTask = _scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(Conf.BatchingMaxPublishDelayMs), TimeSpan.FromMilliseconds(Conf.BatchingMaxPublishDelayMs), Self, BatchTask.Instance, ActorRefs.NoSender);
                    }
                    await ResendMessages(res);
                }
                catch (Exception e)
                {
                    _replyTo.Tell(new AskResponse(new PulsarClientException(e)));
                }
            }
		}

		private void ConnectionFailed(PulsarClientException exception)
		{
			var nonRetriableError = !IsRetriableError(exception);
			var producerTimeout = DateTimeHelper.CurrentUnixTimeMillis() > _createProducerTimeout;
			if ((nonRetriableError || producerTimeout))
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
				Client.Tell(new CleanupProducer(Self));
                _replyTo.Tell(new AskResponse(exception));
				Self.Tell(PoisonPill.Instance);
			}
		}

        private async ValueTask ReconnectLater(Exception exception)
        {
            var askResponse = await _connectionHandler.Ask<AskResponse>(new ReconnectLater(exception));
            await Connection(askResponse);
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

		internal override async ValueTask InternalSend(IMessage<T> message)
        {
            _sender = Sender;
			var interceptorMessage = (Message<T>) BeforeSend(message);
			if(Interceptors != null)
			{

				_ = interceptorMessage.Properties;
			}
			await Send(interceptorMessage);
		}
		private async ValueTask InternalSendWithTxn(IMessage<T> message, IActorRef txn)
        {
            var res = await txn.Ask<RegisterProducedTopicResponse>(new RegisterProducedTopic(Topic));

            while(res.Error == ServerError.TransactionCoordinatorNotFound)
                res = await txn.Ask<RegisterProducedTopicResponse>(new RegisterProducedTopic(Topic));

            if (res.Error == ServerError.UnknownError)
                await InternalSend(message);
        }

		private async ValueTask Send(IMessage<T> message)
		{
			Condition.CheckArgument(message is Message<T>);
			var maxMessageSize = (int)_maxMessageSize;
			if (Conf.ChunkingEnabled && Conf.MaxMessageSize > 0)
				maxMessageSize = Math.Min(Conf.MaxMessageSize, maxMessageSize);
			if (!IsValidProducerState(message.SequenceId))
			{
                return;
			}

			var msg = (Message<T>) message;
			var msgMetadata = msg.Metadata.OriginalMetadata;

            var payload = msg.Data.ToArray();

            try
			{
                // If compression is enabled, we are compressing, otherwise it will simply use the same buffer
                var uncompressedSize = payload.Length;
                var compressedPayload = payload;
                // Batch will be compressed when closed
                // If a message has a delayed delivery time, we'll always send it individually
                if (!BatchMessagingEnabled || msgMetadata.ShouldSerializeDeliverAtTime())
                {
                    compressedPayload = _compressor.Encode(payload);

                    // validate msg-size (For batching this will be check at the batch completion size)
                    var compressedSize = compressedPayload.Length;

                    if (compressedSize > maxMessageSize && !Conf.ChunkingEnabled)
                    {
                        var compressedStr = (!BatchMessagingEnabled && Conf.CompressionType != CompressionType.None) ? "Compressed" : "";
                        var invalidMessageException = new PulsarClientException.InvalidMessageException($"The producer {_producerName} of the topic {Topic} sends a {compressedStr} message with {compressedSize:d} bytes that exceeds {maxMessageSize:d} bytes");
                        _log.Error(invalidMessageException.ToString());
                        Sender.Tell(invalidMessageException);
                        return;
                    }
                }

                if (!msg.Replicated && !string.IsNullOrWhiteSpace(msgMetadata.ProducerName))
                {
                    var invalidMessageException = new PulsarClientException.InvalidMessageException($"The producer {_producerName} of the topic {Topic} can not reuse the same message {msgMetadata.SequenceId}");
                    _log.Error(invalidMessageException.ToString());
                    Sender.Tell(invalidMessageException);
                    return;
                }

                if (!PopulateMessageSchema(msg, out _))
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
				msgMetadata.SequenceId = (ulong)sequenceId;
				_msgIdGenerator++;
				var uuid = totalChunks > 1 ? string.Format("{0}-{1:D}", _producerName, sequenceId) : null;
                //We need to send NoMessageIdYet to unblock the caller
                if (totalChunks > 1)
                    Sender.Tell(NoMessageIdYet.Instance);

				for (var chunkId = 0; chunkId < totalChunks; chunkId++)
				{
					await SerializeAndSendMessage(msg, msgMetadata, sequenceId, uuid, chunkId, totalChunks, readStartIndex, maxMessageSize, compressedPayload, compressedPayload.Length, uncompressedSize);
					readStartIndex = ((chunkId + 1) * maxMessageSize);
				}
			}
			catch (PulsarClientException e)
			{
				e.SequenceId = msg.SequenceId;
				SendComplete(msg, DateTimeHelper.CurrentUnixTimeMillis(), -1, -1, -1, e);
                Sender.Tell(e);
			}
			catch (Exception t)
			{
                Sender.Tell(PulsarClientException.Unwrap(t));
				SendComplete(msg, DateTimeHelper.CurrentUnixTimeMillis(), -1, -1, -1, t);
			}
		}

		private async ValueTask SerializeAndSendMessage(Message<T> msg, MessageMetadata msgMetadata, long sequenceId, string uuid, int chunkId, int totalChunks, int readStartIndex, int chunkMaxSizeInBytes, byte[] compressedPayload, int compressedPayloadSize, int uncompressedSize)
		{
			var chunkPayload = compressedPayload;
			var chunkMsgMetadata = msgMetadata;
			if(totalChunks > 1 && TopicName.Get(Topic).Persistent)
			{
				chunkPayload = compressedPayload.Slice(readStartIndex, Math.Min(chunkMaxSizeInBytes, chunkPayload.Length - readStartIndex));
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
			}
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

            if (CanAddToBatch(msg) && totalChunks <= 1)
			{
                Sender.Tell(NoMessageIdYet.Instance);
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
						await DoBatchSendAndAdd(msg);
					}
					else
					{
						// Should flush the last potential duplicated since can't combine potential duplicated messages
						// and non-duplicated messages into a batch.
						if(_isLastSequenceIdPotentialDuplicated)
						{
							await DoBatchSendAndAdd(msg);
						}
						else
						{
							// handle boundary cases where message being added would exceed
							// batch size and/or max message size
							var isBatchFull = _batchMessageContainer.Add(msg, (a, e) =>
							{
								if (a != null)
									_lastSequenceIdPublished = (long)a;
								if (e != null)
									SendComplete(msg, DateTimeHelper.CurrentUnixTimeMillis(), -1, -1, -1, e);
							});
							if (isBatchFull)
							{
								await BatchMessageAndSend();
							}
						}
						_isLastSequenceIdPotentialDuplicated = false;
					}
				}
				else
				{
					await DoBatchSendAndAdd(msg);
				}
			}
			else
			{
				var encryptedPayload = EncryptMessage(chunkMsgMetadata, chunkPayload);

				msgMetadata = chunkMsgMetadata;
				// When publishing during replication, we need to set the correct number of message in batch
				// This is only used in tracking the publish rate stats
				var numMessages = msg.Metadata.OriginalMetadata.ShouldSerializeNumMessagesInBatch() ? msg.Metadata.OriginalMetadata.NumMessagesInBatch : 1;

				OpSendMsg<T> op;
				if(msg.GetSchemaState() == Message<T>.SchemaState.Ready)
				{
					var cmd = SendMessage(_producerId, sequenceId, numMessages, msgMetadata, encryptedPayload);
					op = OpSendMsg<T>.Create(msg, cmd, sequenceId);
				}
				else
				{
					op = OpSendMsg<T>.Create(msg, ReadOnlySequence<byte>.Empty, sequenceId);
					op.Cmd = SendMessage(_producerId, sequenceId, numMessages, msgMetadata, encryptedPayload);
				}
				op.NumMessagesInBatch = numMessages;
				op.BatchSizeByte = encryptedPayload.Length;
				if(totalChunks > 1)
				{
					op.TotalChunks = totalChunks;
					op.ChunkId = chunkId;
				}
				await ProcessOpSendMsg(op);
			}
		}

		private bool PopulateMessageSchema(Message<T> msg, out Exception exception)
		{
			exception = new Exception();
			if(msg.SchemaInternal() == Schema)
			{
				if (_schemaVersion.HasValue)
					msg.Metadata.OriginalMetadata.SchemaVersion = _schemaVersion.Value;
				msg.SetSchemaState(Message<T>.SchemaState.Ready);
				return true;
			}
			if(!IsMultiSchemaEnabled(true))
			{
				exception = new PulsarClientException.InvalidMessageException($"The producer {_producerName} of the topic {Topic} is disabled the `MultiSchema`: Seq:{msg.SequenceId}");
                Sender.Tell(exception);
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
                _sender.Tell(new AskResponse(err));
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
                    _sender.Tell(new AskResponse(ex));
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

		private ReadOnlySequence<byte> SendMessage(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, MessageMetadata msgMetadata, byte[] compressedPayload)
		{
			return Commands.NewSend(producerId, lowestSequenceId, highestSequenceId, numMessages, msgMetadata, new ReadOnlySequence<byte>(compressedPayload));
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

		private async ValueTask DoBatchSendAndAdd(Message<T> msg)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}] [{_producerName}] Closing out batch to accommodate large message with size {msg.Data.Length}");
			}
			try
			{
				await BatchMessageAndSend();
				_batchMessageContainer.Add(msg, (a, e) =>
				{
					if (a != null)
						_lastSequenceIdPublished = (long)a;
					if (e != null)
						SendComplete(msg, DateTimeHelper.CurrentUnixTimeMillis(), -1, -1, -1, e);
				});
			}
			catch(Exception ex)
            {
				_log.Error(ex.ToString());
			}
		}

		private bool IsValidProducerState(long sequenceId)
		{
			switch(State.ConnectionState)
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
					Sender.Tell(new AlreadyClosedException("Producer already closed", sequenceId));
					return false;
				case HandlerState.State.Terminated:
                    Sender.Tell(new TopicTerminatedException("Topic was terminated", sequenceId));
					return false;
				case HandlerState.State.ProducerFenced:
                    Sender.Tell(new ProducerFencedException("Producer was fenced"));
					return false;
				case HandlerState.State.Failed:
				case HandlerState.State.Uninitialized:
				default:
					Sender.Tell(new NotConnectedException(sequenceId).ToString());
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

		protected internal override void LastDisconnectedTimestamp()
		{
			Receive<LastConnectionClosedTimestampResponse>(l => 
			{
				_replyTo.Tell(l.TimeStamp);
				Become(Ready);
			
			});
			ReceiveAny(_ => Stash.Stash());
			_connectionHandler.Tell(LastConnectionClosedTimestamp.Instance);
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
                var expectedSequenceId = GetHighestSequenceId(op.Msg);
                if (sequenceId == expectedSequenceId)
                {
                    var corrupted = !VerifyLocalBufferIsNotCorrupted(op.Msg);
                    if (corrupted)
                    {
                        // remove message from pendingMessages queue and fail callback
                        var error = new PulsarClientException.ChecksumException($"The checksum of the message which is produced by producer {_producerName} to the topic '{Topic}' is corrupted");
                        _pendingMessages.TryDequeue(out op);
                        try
                        {

                            SendComplete(op.Msg.Msg, DateTimeHelper.CurrentUnixTimeMillis(), -1, -1, -1, error);
                            // as msg is not corrupted : let producer resend pending-messages again including checksum failed message
                            FailPendingMessages(null, error);
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
            catch(Exception ex)
            {
                _log.Error(ex.ToString());
                return false;
            }

            
        }
        private void RecoverNotAllowedError(long sequenceId)
        {
            if (_pendingMessages.TryPeek(out var op) && sequenceId == GetHighestSequenceId(op.Msg))
            {
                _pendingMessages.TryDequeue(out op);
                try
                {
                    SendComplete(op.Msg.Msg, DateTimeHelper.CurrentUnixTimeMillis(), -1, -1, -1, new NotAllowedException($"The size of the message which is produced by producer {_producerName} to the topic '{Topic}' is not allowed"));
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
						ex.SequenceId = op.Msg.SequenceId;
						if (op.Msg.TotalChunks <= 1 || (op.Msg.ChunkId == op.Msg.TotalChunks - 1))
						{
						 _log.Error(ex?.ToString());
						}
					}
					catch (Exception t)
					{
						_log.Warning($"[{Topic}] [{_producerName}] Got exception while completing the callback for msg {op.Msg.SequenceId}:{t}");
					}
					op.Msg.Recycle();
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
			else if (ackReceived.SequenceId > op.Msg.SequenceId)
			{
				var msg = $"[{Topic}] [{_producerName}] Got ack for msg. expecting: {op.Msg.SequenceId} - {op.Msg.HighestSequenceId} - got: {ackReceived.SequenceId} - {ackReceived.HighestSequenceId} - queue-size: {_pendingMessages.Count}";
				_log.Warning(msg);
				// Force connection closing so that messages can be re-transmitted in a new connection
				cnx.Tell(Messages.Requests.Close.Instance);
                return;
			}
			else if (ackReceived.SequenceId < op.Msg.SequenceId)
			{
				var msg = $"[{Topic}] [{_producerName}] Got ack for timed out msg. expecting: {op.Msg.SequenceId} - {op.Msg.HighestSequenceId} - got: {ackReceived.SequenceId} - {ackReceived.HighestSequenceId}";
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
				if (ackReceived.SequenceId >= ackReceived.HighestSequenceId || ackReceived.HighestSequenceId == op.Msg.HighestSequenceId)
				{
					if (_log.IsDebugEnabled)
					{
						_log.Debug($"[{Topic}] [{_producerName}] Received ack for msg {ackReceived.SequenceId} ");
					}

                    _pendingMessages.TryDequeue(out op);
				}
				else
				{
					var msg = $"[{Topic}] [{_producerName}] Got ack for batch msg error. expecting: {op.Msg.SequenceId} - {op.Msg.HighestSequenceId} - got: {ackReceived.SequenceId} - {ackReceived.HighestSequenceId} - queue-size: {_pendingMessages.Count}";
					_log.Warning(msg);
					// Force connection closing so that messages can be re-transmitted in a new connection
					cnx.Tell(Messages.Requests.Close.Instance);
                    return;
				}
			}

            _lastSequenceIdPublished = Math.Max(_lastSequenceIdPublished, GetHighestSequenceId(op.Msg));
            op.Msg.SetMessageId(ackReceived.LedgerId, ackReceived.EntryId, _partitionIndex);
            if(Conf.BatchingEnabled)
                Conf.AckReceivedListerner(ackReceived);
            else
            {
                if (_watchedActors.TryGetValue(op.Sender.Path.Name, out var sender))
                {
                    _watchedActors.Remove(op.Sender.Path.Name);
                    Context.Unwatch(sender);
                    sender.Tell(new MessageId(ackReceived.LedgerId, ackReceived.EntryId, _partitionIndex));
                }
            }
                

            try
            {
                // if message is chunked then call callback only on last chunk
                if (op.Msg.TotalChunks <= 1 || (op.Msg.ChunkId == op.Msg.TotalChunks - 1))
                {
                    if (op.Msg.Msg != null)
                        SendComplete(op.Msg.Msg, op.Msg.CreatedAt, op.Msg.FirstSentAt, op.Msg.LastSentAt, op.Msg.RetryCount, null);
                }
            }
            catch (Exception t)
            {
                var msg = $"[{Topic}] [{_producerName}] Got exception while completing the callback for msg {ackReceived.SequenceId}";
                _log.Warning($"{msg}:{t}");
            }
            op.Msg.Recycle();
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
                    _replyTo.Tell(new AskResponse(response));
				}
				return;
			}
			_log.Info($"[{Topic}] [{_producerName}] Re-Sending {messagesToResend} messages to server");
			await RecoverProcessOpSendMsgFrom(null);
		}
		
		private int BrokerChecksumSupportedVersion()
		{
			return (int)ProtocolVersion.V6;
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
				timeToWaitMs = Conf.SendTimeoutMs;
			}
			else
			{
				// If there is at least one message, calculate the diff between the message timeout and the elapsed
				// time since first message was created.
				var diff = Conf.SendTimeoutMs - (DateTimeHelper.CurrentUnixTimeMillis() - firstMsg.Msg.CreatedAt);
				if (diff <= 0)
				{
					// The diff is less than or equal to zero, meaning that the message has been timed out.
					// Set the callback to timeout on every message, then clear the pending queue.
					_log.Info("[{}] [{}] Message send timed out. Failing {} messages", Topic, _producerName, _pendingMessages.Count);

					var te = new PulsarClientException.TimeoutException($"The producer {_producerName} can not send message to the topic {Topic} within given timeout: {firstMsg.Msg.SequenceId}");
					FailPendingMessages(_cnx, te);
					_stats.IncrementSendFailed(_pendingMessages.Count);
					// Since the pending queue is cleared now, set timer to expire after configured value.
					timeToWaitMs = Conf.SendTimeoutMs;
				}
				else
				{
					// The diff is greater than zero, set the timeout to the diff value
					timeToWaitMs = diff; 
				}
			}
			_sendTimeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(timeToWaitMs), Self, RunSendTimeout.Instance, ActorRefs.NoSender);

		}


		private async ValueTask Flush()
		{
			if (BatchMessagingEnabled)
			{
				await BatchMessageAndSend();
			}
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
				_pendingMessages.Enqueue((op, Sender));
				if (op.Msg != null)
				{
					LastSequenceIdPushed = Math.Max(LastSequenceIdPushed, GetHighestSequenceId(op));
				}
				if(ShouldWriteOpSendMsg())
				{
					if (op.Msg != null && op.Msg.GetSchemaState() == Message<T>.SchemaState.None)
					{
						await TryRegisterSchema(op.Msg);
					}
                    else
                    {
						_stats.UpdateNumMsgsSent(op.NumMessagesInBatch, op.BatchSizeByte);
						SendCommand(op);
					}
				}
				else
				{
					//stash message
					if (_log.IsDebugEnabled)
					{
						_log.Debug($"[{Topic}] [{_producerName}] Connection is not ready -- sequenceId {op.SequenceId}");
					}
				}
			}
			catch (Exception t)
			{
				_log.Warning($"[{Topic}] [{_producerName}] error while closing out batch -- {t}");
				SendComplete(op.Msg, op.CreatedAt, op.FirstSentAt, op.LastSentAt, op.RetryCount, new PulsarClientException(t.ToString(), op.SequenceId));
                Sender.Tell(PulsarClientException.Unwrap(t));
            }
		}
		private void SendComplete(Message<T> interceptorMessage, long createdAt, long firstSentAt, long lastSentAt, long retryCount, Exception e)
		{
			if (e != null &&  e is PulsarClientException.TimeoutException te)
			{
				var sequenceid = te.SequenceId;
				var ns = DateTimeHelper.CurrentUnixTimeMillis();
				var errMsg = string.Format("{0} : createdAt {1} ms ago, firstSentAt {2} ms ago, lastSentAt {3} ns ago, retryCount {4}, sequenceId {5}", te.Message, ns - createdAt, ns - firstSentAt, ns - lastSentAt, retryCount, sequenceid);

				_stats.IncrementSendFailed();
				OnSendAcknowledgement(interceptorMessage, null, e);
				_log.Error(errMsg);
			}
			else
			{
				OnSendAcknowledgement(interceptorMessage, interceptorMessage.MessageId, null);
				_stats.IncrementNumAcksReceived(DateTimeHelper.CurrentUnixTimeMillis() - createdAt);
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
					if (op.Msg?.Msg == from)
					{
						from = null;
					}
					else
					{
						continue;
					}
				}

				if (op.Msg.Msg != null)
				{
					if (op.Msg.Msg.GetSchemaState() == Message<T>.SchemaState.None)
					{
						if (!RePopulateMessageSchema(op.Msg.Msg))
						{
							pendingRegisteringOp = op.Msg;
							break;
						}
					}
					else if (op.Msg.Msg.GetSchemaState() == Message<T>.SchemaState.Broken)
					{
						_pendingMessages = new Queue<(OpSendMsg<T> Msg, IActorRef Sender)>(_pendingMessages.Where(x => x.Msg != op.Msg));
						op.Msg.Recycle();
						continue;
					}
				}

				if (_log.IsDebugEnabled)
				{
					_log.Debug($"[{Topic}] [{_producerName}] Re-Sending message in sequenceId {op.Msg.SequenceId}");
				}
				SendCommand(op.Msg);
				op.Msg.UpdateSentTimestamp();
				_stats.UpdateNumMsgsSent(op.Msg.NumMessagesInBatch, op.Msg.BatchSizeByte);
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
					return DateTimeHelper.CurrentUnixTimeMillis() - firstMsg.Msg.CreatedAt;
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
		private bool HasEventTime(ulong seq)
		{
			if (seq > 0)
				return true;
			return false;
		}
		public virtual string ConnectedSince
		{
			get
			{
				return Cnx() != null ? _connectedSince : null;
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

		private async ValueTask ConnectionClosed(IActorRef cnx)
		{
            var askResponse = await _connectionHandler.Ask<AskResponse>(new ConnectionClosed(cnx));
            await Connection(askResponse);
        }

		internal async ValueTask<IActorRef> ClientCnx()
		{
			return await Cnx();
		}

		protected internal override async ValueTask<IProducerStats> Stats() => await Task.FromResult(_stats);
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
			internal long HighestSequenceId;

			internal int TotalChunks = 0;
			internal int ChunkId = -1;

			internal static OpSendMsg<T1> Create(Message<T1> msg, ReadOnlySequence<byte> cmd, long sequenceId)
			{
				var op = new OpSendMsg<T1>
				{
					Msg = msg,
					Cmd = cmd,
					SequenceId = sequenceId,
					CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
				};
				return op;
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
			internal static OpSendMsg<T1> Create(IList<Message<T1>> msgs, ReadOnlySequence<byte> cmd, long sequenceId)
			{
				var op = new OpSendMsg<T1>
				{
					Msgs = msgs,
					Cmd = cmd,
					SequenceId = sequenceId,
					CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
				};
				return op;
			}

			internal static OpSendMsg<T1> Create(IList<Message<T1>> msgs, ReadOnlySequence<byte> cmd, long lowestSequenceId, long highestSequenceId)
			{
				var op = new OpSendMsg<T1>
				{
					Msgs = msgs,
					Cmd = cmd,
					SequenceId = lowestSequenceId,
					HighestSequenceId = highestSequenceId,
					CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
				};
				return op;
			}

			internal int NumMessagesInBatch { get; set; } = 1;

			internal long BatchSizeByte { get; set; } = 0;

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