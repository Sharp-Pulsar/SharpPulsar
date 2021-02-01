using Akka.Actor;
using Akka.Event;
using Akka.Util;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Batch;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common;
using SharpPulsar.Common.Compression;
using SharpPulsar.Common.Entity;
using SharpPulsar.Configuration;
using SharpPulsar.Crypto;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Impl;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Queues;
using SharpPulsar.Schemas;
using SharpPulsar.Shared;
using SharpPulsar.Stats.Producer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

	public class ProducerActor<T> : ProducerActorBase<T>, IWithUnboundedStash
	{

		// Producer id, used to identify a producer within a single connection
		protected internal readonly long ProducerId;

		// Variable is used through the atomic updater
		private long _msgIdGenerator;
		private ICancelable _sendTimeout;
		private long _createProducerTimeout;
		private readonly IBatchMessageContainerBase _batchMessageContainer;
		private readonly List<OpSendMsg> _pendingMessages;
		private CompletableFuture<MessageId> _lastSendFuture = CompletableFuture.completedFuture(null);

		private readonly ICancelable _regenerateDataKeyCipherCancelable;

		// Globally unique producer name
		private string _producerName;
		private bool _userProvidedProducerName = false;

		private ILoggingAdapter _log;

		private string _connectionId;
		private string _connectedSince;
		private readonly int _partitionIndex;

		private readonly IProducerStatsRecorder _stats;

		private readonly CompressionCodec _compressor;

		private long _lastSequenceIdPublished;

		protected internal long LastSequenceIdPushed;
		private volatile bool _isLastSequenceIdPotentialDuplicated;

		private readonly IMessageCrypto _msgCrypto;

		private ScheduledFuture<object> _keyGeneratorTask = null;

		private readonly IDictionary<string, string> _metadata;

		private Option<sbyte[]> _schemaVersion = null;

		private readonly IActorRef _connectionHandler;

		private ScheduledFuture<object> _batchTimerTask;

		public ProducerActor(IActorRef client, string topic, ProducerConfigurationData conf, int partitionIndex, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData clientConfiguration, ProducerQueueCollection queue) : base(client, topic, conf, schema, interceptors, clientConfiguration, queue)
		{
			_log = Context.GetLogger();
			ProducerId = client.AskFor<long>(NewProducerId.Instance);
			_producerName = conf.ProducerName;
			if(!string.IsNullOrWhiteSpace(_producerName))
			{
				_userProvidedProducerName = true;
			}
			_partitionIndex = partitionIndex;
			_pendingMessages = new List<OpSendMsg>(conf.MaxPendingMessages);

			_compressor = CompressionCodecProvider.GetCompressionCodec(conf.CompressionType);

			if (conf.InitialSequenceId != null)
			{
				long initialSequenceId = conf.InitialSequenceId.Value;
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
				string logCtx = "[" + topic + "] [" + _producerName + "] [" + ProducerId + "]";

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
						Context.System.Log.Warning($"[{Topic}] [{ProducerName}] [{ProducerId}] Failed to add public key cipher.");
						Context.System.Log.Error(e.ToString());
					}
				});
			}

			if(conf.SendTimeoutMs > 0)
			{
				_sendTimeout = client.Timer().newTimeout(this, conf.SendTimeoutMs, TimeUnit.MILLISECONDS);
			}

			_createProducerTimeout = DateTimeHelper.CurrentUnixTimeMillis() + clientConfiguration.OperationTimeoutMs;
			if(conf.BatchingEnabled)
			{
				var containerBuilder = conf.BatcherBuilder;
				if(containerBuilder == null)
				{
					containerBuilder = IBatcherBuilder.Default(Context.System);
				}
				_batchMessageContainer = (IBatchMessageContainerBase)containerBuilder.Build();
				_batchMessageContainer.Producer = Self;
				_batchMessageContainer.Container = new ProducerContainer(Self, Configuration, Configuration.MaxMessageSize, Context.System);

			}
			else
			{
				_batchMessageContainer = null;
			}
			if (clientConfiguration.StatsIntervalSeconds > 0)
			{
				_stats = new ProducerStatsRecorder(Context.System, ProducerName, topic, Configuration.MaxPendingMessages);
			}
			else
			{
				_stats = (IProducerStatsRecorder)ProducerStatsDisabled.Instance;
			}

			if (Configuration.Properties == null)
			{
				_metadata = new Dictionary<string, string>();
			}
			else
			{
				_metadata = new SortedDictionary<string, string>(Configuration.Properties);
			}
			_connectionHandler = Context.ActorOf(ConnectionHandler.Prop(State, new BackoffBuilder().SetInitialTime(clientConfiguration.InitialBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMax(clientConfiguration.MaxBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMandatoryStop(0, TimeUnit.MILLISECONDS).Create(), Self));

			GrabCnx();
		}

		internal virtual void GrabCnx()
		{
			_connectionHandler.Tell(new GrabCnx($"Create connection from producer: {ProducerName}"));
			Become(Connection);
		}
		private void Connection()
		{
			Receive<ConnectionOpened>(c => {
				ConnectionOpened(c.ClientCnx);
			});
			Receive<ConnectionFailed>(c => {
				ConnectionFailed(c.Exception);
			});
			Receive<Failure>(c => {
				_log.Error($"Connection to the server failed: {c.Exception}/{c.Timestamp}");
			});
			Receive<ConnectionAlreadySet>(_ => {
				Become(Ready);
			});
			ReceiveAny(a => Stash.Stash());
		}

		private void ConnectionOpened(IActorRef cnx)
		{
			// we set the cnx reference before registering the producer on the cnx, so if the cnx breaks before creating the
			// producer, it will try to grab a new cnx
			_connectionHandler.Tell(new SetCnx(cnx));
			cnx.Tell(new RegisterProducer(ProducerId, Self));

			_log.Info($"[{Topic}] [{_producerName}] Creating producer on cnx {cnx.Path}");

			long requestId = Client.AskFor<long>(NewRequestId.Instance);

			ISchemaInfo schemaInfo = null;
			if (Schema != null)
			{
				if (Schema.SchemaInfo != null)
				{
					if (Schema.SchemaInfo.Type == SchemaType.JSON)
					{
						// for backwards compatibility purposes
						// JSONSchema originally generated a schema for pojo based of of the JSON schema standard
						// but now we have standardized on every schema to generate an Avro based schema
						var protocolVersion = cnx.AskFor<int>(RemoteEndpointProtocolVersion.Instance);
						if (Commands.PeerSupportJsonSchemaAvroFormat(protocolVersion))
						{
							schemaInfo = Schema.SchemaInfo;
						}
						else if (Schema is JSONSchema<T> jsonSchema)
						{
							schemaInfo = jsonSchema.BackwardsCompatibleJsonSchemaInfo;
						}
						else
						{
							schemaInfo = Schema.SchemaInfo;
						}
					}
					else if (Schema.SchemaInfo.Type == SchemaType.BYTES || Schema.SchemaInfo.Type == SchemaType.NONE)
					{
						// don't set schema info for Schema.BYTES
						schemaInfo = null;
					}
					else
					{
						schemaInfo = Schema.SchemaInfo;
					}
				}
			}
			try
            {
				var epoch = _connectionHandler.AskFor<long>(GetEpoch.Instance);
				var cmd = Commands.NewProducer(Topic, ProducerId, requestId, _producerName, Conf.EncryptionEnabled, _metadata, schemaInfo, epoch, _userProvidedProducerName);
				var payload = new Payload(cmd, requestId, "NewProducer");

				var response = cnx.AskFor<ProducerResponse>(payload);

				string producerName = response.ProducerName;
				long lastSequenceId = response.LastSequenceId;

				_schemaVersion = new Option<sbyte[]>((sbyte[])(object)response.SchemaVersion);

				if (_schemaVersion.HasValue)
					SchemaCache.Add(SchemaHash.Of(Schema), _schemaVersion.Value);
				if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
				{
					cnx.Tell(new RemoveProducer(ProducerId));
					cnx.GracefulStop(TimeSpan.FromSeconds(5));
					return;
				}
				_connectionHandler.Tell(ResetBackoff.Instance);
				_log.Info($"[{Topic}] [{producerName}] Created producer on cnx {cnx.Path}");
				_connectionId = cnx.Path.ToString();
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
					_batchTimerTask = cnx.Ctx().executor().scheduleAtFixedRate(() =>
					{
						if (_log.TraceEnabled)
						{
							_log.trace("[{}] [{}] Batching the messages from the batch container from timer thread", Topic, producerName);
						}
						if (State == State.Closing || State == State.Closed)
						{
							return;
						}
						BatchMessageAndSend();
					}, 0, Conf.BatchingMaxPublishDelayMicros, TimeUnit.MICROSECONDS);
				}
				ResendMessages(cnx);
			}
			catch(Exception ex)
            {
				cnx.Tell(new RemoveProducer(ProducerId));
				if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
				{
					cnx.GracefulStop(TimeSpan.FromSeconds(5));
					return;
				}
				_log.Error($"[{Topic}] [{_producerName}] Failed to create producer: {ex}");
				if (ex is TopicDoesNotExistException e)
				{
					_log.Error($"Failed to close producer on TopicDoesNotExistException: {Topic}");
					ProducerQueue.Producer.Add(new ProducerCreation(e));
					return;
				}
				if (ex is ProducerBlockedQuotaExceededException)
				{
					_log.Warning($"[{Topic}] [{_producerName}] Topic backlog quota exceeded. Throwing Exception on producer.");
					var pe = new ProducerBlockedQuotaExceededException($"The backlog quota of the topic {Topic} that the producer {_producerName} produces to is exceeded");
					ProducerQueue.Producer.Add(new ProducerCreation(pe));
				}
				else if (ex is ProducerBlockedQuotaExceededError pexe)
				{
					_log.Warning($"[{_producerName}] [{Topic}] Producer is blocked on creation because backlog exceeded on topic.");
					ProducerQueue.Producer.Add(new ProducerCreation(pexe));
				}
				if (ex is TopicTerminatedException tex)
				{
					State.ConnectionState = HandlerState.State.Terminated;
					Client.Tell(new CleanupProducer(Self));
				}
				else if ((ex is PulsarClientException && IsRetriableError(ex) && DateTimeHelper.CurrentUnixTimeMillis() < _createProducerTimeout))
				{
					ReconnectLater(ex);
				}
				else
				{
					State.ConnectionState = HandlerState.State.Failed;
					Client.Tell(new CleanupProducer(Self));
					var timeout = _sendTimeout;
					if (timeout != null)
					{
						timeout.cancel();
						_sendTimeout = null;
					}
				}
			}			
		}

		private void ConnectionFailed(PulsarClientException exception)
	{
		bool nonRetriableError = !IsRetriableError(exception);
		bool producerTimeout = DateTimeHelper.CurrentUnixTimeMillis() > _createProducerTimeout;
		if ((nonRetriableError || producerTimeout))
		{
			if (nonRetriableError)
			{
				_log.Info($"[{Topic}] Producer creation failed for producer {ProducerId} with unretriableError = {exception}");
			}
			else
			{
				_log.Info($"[{Topic}] Producer creation failed for producer {ProducerId} after producerTimeout");
			}
			State.ConnectionState = HandlerState.State.Failed;
			Client.Tell(new CleanupProducer(Self));
		}
	}

		private void ReconnectLater(Exception exception)
		{
			_connectionHandler.Tell(new ReconnectLater(exception));
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

		public override long LastSequenceId
		{
			get
			{
				return _lastSequenceIdPublished;
			}
		}

		private IMessageId Send(IMessage<T> message)
		{

			var interceptorMessage = (Message<T>) BeforeSend(message);
			//Retain the buffer used by interceptors callback to get message. Buffer will release after complete interceptors.
			interceptorMessage.Data.retain();
			if(Interceptors != null)
			{
				interceptorMessage.Properties;
			}
			SendAsync(interceptorMessage, new SendCallbackAnonymousInnerClass(this, future, interceptorMessage));
			return future;
		}

		/*private class SendCallbackAnonymousInnerClass : SendCallback
		{
			private readonly ProducerActor<T> _outerInstance;

			private CompletableFuture<MessageId> _future;

			private Org.Apache.Pulsar.Client.Impl.MessageImpl<object> _interceptorMessage;

			public SendCallbackAnonymousInnerClass(ProducerActor<T> outerInstance, CompletableFuture<MessageId> future, Org.Apache.Pulsar.Client.Impl.MessageImpl<T1> interceptorMessage)
			{
				_outerInstance = outerInstance;
				_future = future;
				_interceptorMessage = interceptorMessage;
				NextCallback = null;
				NextMsg = null;
				CreatedAt = System.nanoTime();
			}

			internal SendCallback NextCallback;

			internal MessageImpl<object> NextMsg;
			internal long CreatedAt;

			public CompletableFuture<MessageId> Future
			{
				get
				{
					return _future;
				}
			}

			public SendCallback NextSendCallback
			{
				get
				{
					return NextCallback;
				}
			}

			public MessageImpl<object> NextMessage
			{
				get
				{
					return NextMsg;
				}
			}

			public void SendComplete(Exception e)
			{
				try
				{
					if(e != null)
					{
						outerInstance._stats.IncrementSendFailed();
						outerInstance.OnSendAcknowledgement(_interceptorMessage, null, e);
						_future.completeExceptionally(e);
					}
					else
					{
						outerInstance.OnSendAcknowledgement(_interceptorMessage, _interceptorMessage.getMessageId(), null);
						_future.complete(_interceptorMessage.getMessageId());
						outerInstance._stats.IncrementNumAcksReceived(System.nanoTime() - CreatedAt);
					}
				}
				finally
				{
					_interceptorMessage.DataBuffer.release();
				}

				while(NextCallback != null)
				{
					SendCallback sendCallback = NextCallback;

					MessageImpl<object> msg = NextMsg;
					//Retain the buffer used by interceptors callback to get message. Buffer will release after complete interceptors.
					try
					{
						msg.DataBuffer.retain();
						if(e != null)
						{
							outerInstance._stats.IncrementSendFailed();
							outerInstance.OnSendAcknowledgement(msg, null, e);
							sendCallback.Future.completeExceptionally(e);
						}
						else
						{
							outerInstance.OnSendAcknowledgement(msg, msg.getMessageId(), null);
							sendCallback.Future.complete(msg.getMessageId());
							outerInstance._stats.IncrementNumAcksReceived(System.nanoTime() - CreatedAt);
						}
						NextMsg = NextCallback.NextMessage;
						NextCallback = NextCallback.NextSendCallback;
					}
					finally
					{
						msg.DataBuffer.release();
					}
				}
			}

		}
		*/
		private IMessageId InternalSendWithTxnAsync(IMessage<T> message, IActorRef txn)
		{
			if(txn == null)
			{
				return InternalSendAsync(message);
			}
			else
			{
				return ((TransactionImpl) txn).RegisterProducedTopic(Topic).thenCompose(ignored => InternalSendAsync(message));
			}
		}

		public virtual void SendAsync(IMessage<T> message)
		{
			Condition.CheckArgument(message is Message<T>);

			if(!IsValidProducerState(message.SequenceId))
			{
				return;
			}

			var msg = (Message<T>) message;
			MessageMetadata msgMetadata = new MessageMetadata();
			var payload = msg.Data;

			// If compression is enabled, we are compressing, otherwise it will simply use the same buffer
			int uncompressedSize = payload.Length;
			var compressedPayload = payload;
			// Batch will be compressed when closed
			// If a message has a delayed delivery time, we'll always send it individually
			if(!BatchMessagingEnabled || msgMetadata.ShouldSerializeDeliverAtTime())
			{
				compressedPayload = _compressor.Encode(payload);

				// validate msg-size (For batching this will be check at the batch completion size)
				int compressedSize = compressedPayload.Length;
				if(compressedSize > ClientCnx.MaxMessageSize && !Conf.ChunkingEnabled)
				{
					string compressedStr = (!BatchMessagingEnabled && Conf.CompressionType != CompressionType.None) ? "Compressed" : "";
					PulsarClientException.InvalidMessageException invalidMessageException = new PulsarClientException.InvalidMessageException(format("The producer %s of the topic %s sends a %s message with %d bytes that exceeds %d bytes", _producerName, Topic, compressedStr, compressedSize, ClientCnx.MaxMessageSize));
					CompleteCallbackAndReleaseSemaphore(callback, invalidMessageException);
					return;
				}
			}

			if(!msg.Replicated && !string.IsNullOrWhiteSpace(msgMetadata.ProducerName))
			{
				PulsarClientException.InvalidMessageException invalidMessageException = new PulsarClientException.InvalidMessageException(format("The producer %s of the topic %s can not reuse the same message", _producerName, Topic), msg.SequenceId);
				CompleteCallbackAndReleaseSemaphore(callback, invalidMessageException);
				compressedPayload.release();
				return;
			}

			if(!PopulateMessageSchema(msg, callback))
			{
				compressedPayload.release();
				return;
			}

			// send in chunks
			int totalChunks = CanAddToBatch(msg) ? 1 : Math.Max(1, compressedPayload.Length) / ClientCnx.MaxMessageSize + (Math.Max(1, compressedPayload.readableBytes()) % ClientCnx.MaxMessageSize == 0 ? 0 : 1);
			// chunked message also sent individually so, try to acquire send-permits
			for(int i = 0; i < (totalChunks - 1); i++)
			{
				if(!CanEnqueueRequest(callback, message.SequenceId))
				{
					return;
				}
			}

			try
			{
				lock(this)
				{
					int readStartIndex = 0;
					long sequenceId;
					if(!msgMetadataBuilder.HasSequenceId())
					{
						sequenceId = _msgIdGeneratorUpdater.getAndIncrement(this);
						msgMetadataBuilder.SequenceId = sequenceId;
					}
					else
					{
						sequenceId = msgMetadataBuilder.SequenceId;
					}
					string uuid = totalChunks > 1 ? string.Format("{0}-{1:D}", _producerName, sequenceId) : null;
					for(int chunkId = 0; chunkId < totalChunks; chunkId++)
					{
						SerializeAndSendMessage(msg, msgMetadataBuilder, payload, sequenceId, uuid, chunkId, totalChunks, readStartIndex, ClientCnx.MaxMessageSize, compressedPayload, compressedPayload.readableBytes(), uncompressedSize, callback);
						readStartIndex = ((chunkId + 1) * ClientCnx.MaxMessageSize);
					}
				}
			}
			catch(PulsarClientException e)
			{
				e.SequenceId = msg.SequenceId;
				CompleteCallbackAndReleaseSemaphore(callback, e);
			}
			catch(Exception t)
			{
				CompleteCallbackAndReleaseSemaphore(callback, new PulsarClientException(t, msg.SequenceId));
			}
		}

		private void SerializeAndSendMessage<T1>(MessageImpl<T1> msg, MessageMetadata.Builder msgMetadataBuilder, ByteBuf payload, long sequenceId, string uuid, int chunkId, int totalChunks, int readStartIndex, int chunkMaxSizeInBytes, ByteBuf compressedPayload, int compressedPayloadSize, int uncompressedSize, SendCallback callback)
		{
			ByteBuf chunkPayload = compressedPayload;
			MessageMetadata.Builder chunkMsgMetadataBuilder = msgMetadataBuilder;
			if(totalChunks > 1 && TopicName.Get(Topic).Persistent)
			{
				chunkPayload = compressedPayload.slice(readStartIndex, Math.Min(chunkMaxSizeInBytes, chunkPayload.readableBytes() - readStartIndex));
				// don't retain last chunk payload and builder as it will be not needed for next chunk-iteration and it will
				// be released once this chunk-message is sent
				if(chunkId != totalChunks - 1)
				{
					chunkPayload.retain();
					chunkMsgMetadataBuilder = msgMetadataBuilder.Clone();
				}
				if(!string.ReferenceEquals(uuid, null))
				{
					chunkMsgMetadataBuilder.setUuid(uuid);
				}
				chunkMsgMetadataBuilder.ChunkId = chunkId;
				chunkMsgMetadataBuilder.NumChunksFromMsg = totalChunks;
				chunkMsgMetadataBuilder.TotalChunkMsgSize = compressedPayloadSize;
			}
			if(!chunkMsgMetadataBuilder.HasPublishTime())
			{
				chunkMsgMetadataBuilder.PublishTime = ClientConflict.ClientClock.millis();

				checkArgument(!chunkMsgMetadataBuilder.HasProducerName());

				chunkMsgMetadataBuilder.setProducerName(_producerName);

				if(Conf.CompressionType != CompressionType.NONE)
				{
					chunkMsgMetadataBuilder.Compression = CompressionCodecProvider.ConvertToWireProtocol(Conf.CompressionType);
				}
				chunkMsgMetadataBuilder.UncompressedSize = uncompressedSize;
			}

			if(CanAddToBatch(msg) && totalChunks <= 1)
			{
				if(CanAddToCurrentBatch(msg))
				{
					// should trigger complete the batch message, new message will add to a new batch and new batch
					// sequence id use the new message, so that broker can handle the message duplication
					if(sequenceId <= LastSequenceIdPushed)
					{
						_isLastSequenceIdPotentialDuplicated = true;
						if(sequenceId <= _lastSequenceIdPublished)
						{
							_log.warn("Message with sequence id {} is definitely a duplicate", sequenceId);
						}
						else
						{
							_log.info("Message with sequence id {} might be a duplicate but cannot be determined at this time.", sequenceId);
						}
						DoBatchSendAndAdd(msg, callback, payload);
					}
					else
					{
						// Should flush the last potential duplicated since can't combine potential duplicated messages
						// and non-duplicated messages into a batch.
						if(_isLastSequenceIdPotentialDuplicated)
						{
							DoBatchSendAndAdd(msg, callback, payload);
						}
						else
						{
							// handle boundary cases where message being added would exceed
							// batch size and/or max message size
							bool isBatchFull = _batchMessageContainer.Add(msg, callback);
							_lastSendFuture = callback.Future;
							payload.release();
							if(isBatchFull)
							{
								BatchMessageAndSend();
							}
						}
						_isLastSequenceIdPotentialDuplicated = false;
					}
				}
				else
				{
					DoBatchSendAndAdd(msg, callback, payload);
				}
			}
			else
			{
				ByteBuf encryptedPayload = EncryptMessage(chunkMsgMetadataBuilder, chunkPayload);

				MessageMetadata msgMetadata = chunkMsgMetadataBuilder.Build();
				// When publishing during replication, we need to set the correct number of message in batch
				// This is only used in tracking the publish rate stats
				int numMessages = msg.MessageBuilder.hasNumMessagesInBatch() ? msg.MessageBuilder.NumMessagesInBatch : 1;

				OpSendMsg op;
				if(msg.SchemaState == MessageImpl.SchemaState.Ready)
				{
					ByteBufPair cmd = SendMessage(ProducerId, sequenceId, numMessages, msgMetadata, encryptedPayload);
					op = OpSendMsg.Create(msg, cmd, sequenceId, callback);
					chunkMsgMetadataBuilder.Recycle();
					msgMetadata.Recycle();
				}
				else
				{
					op = OpSendMsg.Create(msg, null, sequenceId, callback);

					MessageMetadata.Builder tmpBuilder = chunkMsgMetadataBuilder;
					op.RePopulate = () =>
					{
					MessageMetadata metadata = msgMetadataBuilder.Build();
					op.Cmd = SendMessage(ProducerId, sequenceId, numMessages, metadata, encryptedPayload);
					tmpBuilder.Recycle();
					msgMetadata.Recycle();
					};
				}
				op.NumMessagesInBatch = numMessages;
				op.BatchSizeByte = encryptedPayload.readableBytes();
				if(totalChunks > 1)
				{
					op.TotalChunks = totalChunks;
					op.ChunkId = chunkId;
				}
				_lastSendFuture = callback.Future;
				ProcessOpSendMsg(op);
			}
		}

		private bool PopulateMessageSchema(Message<T> msg)
		{
			var msgMetadata = new MessageMetadata();
			if(msg.Schema == Schema)
			{
				if (_schemaVersion.HasValue)
					msgMetadata.SchemaVersion = (byte[])(object)_schemaVersion.Value; ;
				msg.SetSchemaState(Message<T>.SchemaState.Ready);
				return true;
			}
			if(!IsMultiSchemaEnabled(true))
			{
				PulsarClientException.InvalidMessageException e = new PulsarClientException.InvalidMessageException(format("The producer %s of the topic %s is disabled the `MultiSchema`", _producerName, Topic), msg.SequenceId);
				CompleteCallbackAndReleaseSemaphore(callback, e);
				return false;
			}
			SchemaHash schemaHash = SchemaHash.Of(msg.Schema);
			sbyte[] schemaVersion = SchemaCache[schemaHash];
			if(schemaVersion != null)
			{
				msgMetadata.SchemaVersion = (byte[])(object)schemaVersion;
				msg.SetSchemaState(Message<T>.SchemaState.Ready);
			}
			return true;
		}

		private bool RePopulateMessageSchema(Message<T> msg)
		{
			SchemaHash schemaHash = SchemaHash.Of(msg.Schema);
			sbyte[] schemaVersion = SchemaCache[schemaHash];
			if(schemaVersion == null)
			{
				return false;
			}
			msg.Metadata.SchemaVersion = (byte[])(object)schemaVersion;
			msg.SetSchemaState(Message<T>.SchemaState.Ready);
			return true;
		}

		private void TryRegisterSchema(IActorRef cnx, Message<T> msg)
		{
			if(!State.ChangeToRegisteringSchemaState())
			{
				return;
			}
			SchemaInfo schemaInfo = Optional.ofNullable(msg.Schema).map(Schema::getSchemaInfo).filter(si => si.Type.Value > 0).orElse(Schema.BYTES.SchemaInfo);
			GetOrCreateSchemaAsync(cnx, schemaInfo).handle((v, ex) =>
			{
					if(ex != null)
					{
						Exception t = FutureUtil.UnwrapCompletionException(ex);
						_log.warn("[{}] [{}] GetOrCreateSchema error", Topic, _producerName, t);
						if(t is PulsarClientException.IncompatibleSchemaException)
						{
							msg.SchemaState = MessageImpl.SchemaState.Broken;
							callback.SendComplete((PulsarClientException.IncompatibleSchemaException) t);
						}
					}
					else
					{
						_log.warn("[{}] [{}] GetOrCreateSchema succeed", Topic, _producerName);
						SchemaHash schemaHash = SchemaHash.Of(msg.Schema);
						SchemaCache.PutIfAbsent(schemaHash, v);
						msg.MessageBuilder.SchemaVersion = ByteString.copyFrom(v);
						msg.SchemaState = MessageImpl.SchemaState.Ready;
					}
					cnx.Ctx().channel().eventLoop().execute(() =>
					{
						RecoverProcessOpSendMsgFrom(cnx, msg);
					});
			return null;
			});
		}

		private sbyte[] GetOrCreateSchemaAsync(IActorRef cnx, ISchemaInfo schemaInfo)
		{
			var protocolVersion = cnx.AskFor<int>(RemoteEndpointProtocolVersion.Instance);
			if(!Commands.PeerSupportsGetOrCreateSchema(protocolVersion))
			{
				throw new PulsarClientException.NotSupportedException($"The command `GetOrCreateSchema` is not supported for the protocol version {protocolVersion}. The producer is {_producerName}, topic is {Topic}");
			}
			long requestId = Client.AskFor<long>(NewRequestId.Instance);
			var request = Commands.NewGetOrCreateSchema(requestId, Topic, schemaInfo);
			var payload = new Payload(request, requestId, "SendGetOrCreateSchema");
			_log.Info($"[{Topic}] [{_producerName}] GetOrCreateSchema request", Topic, _producerName);
			var sch =  cnx.AskFor<GetOrCreateSchemaResponse>(payload);
			return (sbyte[])(object)sch.Response.SchemaVersion;
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

		protected internal virtual ByteBufPair SendMessage(long producerId, long sequenceId, int numMessages, MessageMetadata msgMetadata, ByteBuf compressedPayload)
		{
			return Commands.NewSend(producerId, sequenceId, numMessages, ChecksumType, msgMetadata, compressedPayload);
		}

		protected internal virtual ByteBufPair SendMessage(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, MessageMetadata msgMetadata, ByteBuf compressedPayload)
		{
			return Commands.NewSend(producerId, lowestSequenceId, highestSequenceId, numMessages, ChecksumType, msgMetadata, compressedPayload);
		}

		private Commands.ChecksumType ChecksumType
		{
			get
			{
				if(_connectionHandler.Cnx() == null || _connectionHandler.Cnx().RemoteEndpointProtocolVersion >= BrokerChecksumSupportedVersion())
				{
					return Commands.ChecksumType.Crc32c;
				}
				else
				{
					return Commands.ChecksumType.None;
				}
			}
		}

        public IStash Stash { get; set; }

        private bool CanAddToBatch(Message<T> msg)
		{
			return msg.SchemaState == Message<T>.SchemaState.Ready && BatchMessagingEnabled && !msg.hasDeliverAtTime();
		}

		private bool CanAddToCurrentBatch(Message<T> msg)
		{
			return _batchMessageContainer.HaveEnoughSpace(msg) && (!IsMultiSchemaEnabled(false) || _batchMessageContainer.HasSameSchema(msg)) && _batchMessageContainer.HasSameTxn(msg);
		}

		private void DoBatchSendAndAdd(Message<T> msg, byte[] payload)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}] [{_producerName}] Closing out batch to accommodate large message with size {msg.Data.Length}");
			}
			try
			{
				BatchMessageAndSend();
				_batchMessageContainer.Add(msg, callback);
			}
			finally
			{
				
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
				ProducerQueue.SentMessage.Add(new SentMessage<T>(new AlreadyClosedException("Producer already closed", sequenceId)));
				return false;
			case HandlerState.State.Terminated:
					ProducerQueue.SentMessage.Add(new SentMessage<T>(new TopicTerminatedException("Topic was terminated", sequenceId)));
				return false;
			case HandlerState.State.Failed:
			case HandlerState.State.Uninitialized:
			default:
					ProducerQueue.SentMessage.Add(new SentMessage<T>(new NotConnectedException(sequenceId)));
				return false;
			}
		}

		private sealed class WriteInEventLoopCallback : ThreadStart
		{
			internal ProducerActor<object> Producer;
			internal ByteBufPair Cmd;
			internal long SequenceId;
			internal ClientCnx Cnx;

			internal static WriteInEventLoopCallback Create<T1>(ProducerActor<T1> producer, ClientCnx cnx, OpSendMsg op)
			{
				WriteInEventLoopCallback c = RECYCLER.get();
				c.Producer = producer;
				c.Cnx = cnx;
				c.SequenceId = op.SequenceId;
				c.Cmd = op.Cmd;
				return c;
			}

			public override void Run()
			{
				if(_log.DebugEnabled)
				{
					_log.debug("[{}] [{}] Sending message cnx {}, sequenceId {}", Producer.Topic, Producer._producerName, Cnx, SequenceId);
				}

				try
				{
					Cnx.Ctx().writeAndFlush(Cmd, Cnx.Ctx().voidPromise());
				}
				finally
				{
					Recycle();
				}
			}

		}

		public override CompletableFuture<Void> CloseAsync()
		{
			State currentState = GetAndUpdateState(state =>
			{
			if(state == State.Closed)
			{
				return state;
			}
			return State.Closing;
			});

			if(currentState == State.Closed || currentState == State.Closing)
			{
				return CompletableFuture.completedFuture(null);
			}

			Timeout timeout = _sendTimeout;
			if(timeout != null)
			{
				timeout.cancel();
				_sendTimeout = null;
			}


			ScheduledFuture<object> batchTimerTask = this._batchTimerTask;
			if(batchTimerTask != null)
			{
				batchTimerTask.cancel(false);
				this._batchTimerTask = null;
			}

			if(_keyGeneratorTask != null && !_keyGeneratorTask.Cancelled)
			{
				_keyGeneratorTask.cancel(false);
			}

			_stats.CancelStatsTimeout();

			ClientCnx cnx = Cnx();
			if(cnx == null || currentState != State.Ready)
			{
				_log.info("[{}] [{}] Closed Producer (not connected)", Topic, _producerName);
				lock(this)
				{
					State = State.Closed;
					ClientConflict.CleanupProducer(this);
					PulsarClientException ex = new PulsarClientException.AlreadyClosedException(format("The producer %s of the topic %s was already closed when closing the producers", _producerName, Topic));
					_pendingMessages.forEach(msg =>
					{
					msg.callback.sendComplete(ex);
					msg.cmd.release();
					msg.recycle();
					});
					_pendingMessages.clear();
				}

				return CompletableFuture.completedFuture(null);
			}

			long requestId = ClientConflict.NewRequestId();
			ByteBuf cmd = Commands.NewCloseProducer(ProducerId, requestId);

			CompletableFuture<Void> closeFuture = new CompletableFuture<Void>();
			cnx.SendRequestWithId(cmd, requestId).handle((v, exception) =>
			{
			cnx.RemoveProducer(ProducerId);
			if(exception == null || !cnx.Ctx().channel().Active)
			{
				lock(this)
				{
					_log.info("[{}] [{}] Closed Producer", Topic, _producerName);
					State = State.Closed;
					_pendingMessages.forEach(msg =>
					{
						msg.cmd.release();
						msg.recycle();
					});
					_pendingMessages.clear();
				}
				closeFuture.complete(null);
				ClientConflict.CleanupProducer(this);
			}
			else
			{
				closeFuture.completeExceptionally(exception);
			}
			return null;
			});

			return closeFuture;
		}

		public override bool Connected
		{
			get
			{
				return _connectionHandler.Cnx() != null && (State == State.Ready);
			}
		}

		public override long LastDisconnectedTimestamp
		{
			get
			{
				return _connectionHandler.LastConnectionClosedTimestamp;
			}
		}

		public virtual bool Writable
		{
			get
			{
				ClientCnx cnx = _connectionHandler.Cnx();
				return cnx != null && cnx.Channel().Writable;
			}
		}

		public virtual void Terminated(ClientCnx cnx)
		{
			State previousState = GetAndUpdateState(state => (state == State.Closed ? State.Closed : State.Terminated));
			if(previousState != State.Terminated && previousState != State.Closed)
			{
				_log.info("[{}] [{}] The topic has been terminated", Topic, _producerName);
				ClientCnx = null;

				FailPendingMessages(cnx, new PulsarClientException.TopicTerminatedException(format("The topic %s that the producer %s produces to has been terminated", Topic, _producerName)));
			}
		}

		internal virtual void AckReceived(ClientCnx cnx, long sequenceId, long highestSequenceId, long ledgerId, long entryId)
		{
			OpSendMsg op = null;
			bool callback = false;
			lock(this)
			{
				op = _pendingMessages.peek();
				if(op == null)
				{
					if(_log.DebugEnabled)
					{
						_log.debug("[{}] [{}] Got ack for timed out msg {} - {}", Topic, _producerName, sequenceId, highestSequenceId);
					}
					return;
				}

				if(sequenceId > op.SequenceId)
				{
					_log.warn("[{}] [{}] Got ack for msg. expecting: {} - {} - got: {} - {} - queue-size: {}", Topic, _producerName, op.SequenceId, op.HighestSequenceId, sequenceId, highestSequenceId, _pendingMessages.size());
					// Force connection closing so that messages can be re-transmitted in a new connection
					cnx.Channel().close();
				}
				else if(sequenceId < op.SequenceId)
				{
					// Ignoring the ack since it's referring to a message that has already timed out.
					if(_log.DebugEnabled)
					{
						_log.debug("[{}] [{}] Got ack for timed out msg. expecting: {} - {} - got: {} - {}", Topic, _producerName, op.SequenceId, op.HighestSequenceId, sequenceId, highestSequenceId);
					}
				}
				else
				{
					// Add check `sequenceId >= highestSequenceId` for backward compatibility.
					if(sequenceId >= highestSequenceId || highestSequenceId == op.HighestSequenceId)
					{
						// Message was persisted correctly
						if(_log.DebugEnabled)
						{
							_log.debug("[{}] [{}] Received ack for msg {} ", Topic, _producerName, sequenceId);
						}
						_pendingMessages.remove();
						ReleaseSemaphoreForSendOp(op);
						callback = true;
						_pendingCallbacks.add(op);
					}
					else
					{
						_log.warn("[{}] [{}] Got ack for batch msg error. expecting: {} - {} - got: {} - {} - queue-size: {}", Topic, _producerName, op.SequenceId, op.HighestSequenceId, sequenceId, highestSequenceId, _pendingMessages.size());
						// Force connection closing so that messages can be re-transmitted in a new connection
						cnx.Channel().close();
					}
				}
			}
			if(callback)
			{
				op = _pendingCallbacks.poll();
				if(op != null)
				{
					OpSendMsg finalOp = op;
					LastSeqIdPublishedUpdater.getAndUpdate(this, last => Math.Max(last, GetHighestSequenceId(finalOp)));
					op.SetMessageId(ledgerId, entryId, _partitionIndex);
					try
					{
						// if message is chunked then call callback only on last chunk
						if(op.TotalChunks <= 1 || (op.ChunkId == op.TotalChunks - 1))
						{
							try
							{

								// Need to protect ourselves from any exception being thrown in the future handler from the
								// application
								op.Callback.SendComplete(null);
							}
							catch(Exception t)
							{
								_log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", Topic, _producerName, sequenceId, t);
							}
						}
					}
					catch(Exception t)
					{
						_log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", Topic, _producerName, sequenceId, t);
					}
					ReferenceCountUtil.safeRelease(op.Cmd);
					op.Recycle();
				}
			}
		}

		private long GetHighestSequenceId(OpSendMsg op)
		{
			return Math.Max(op.HighestSequenceId, op.SequenceId);
		}

		private void ReleaseSemaphoreForSendOp(OpSendMsg op)
		{
			_semaphore.release(BatchMessagingEnabled ? op.NumMessagesInBatchConflict : 1);
		}

		private void CompleteCallbackAndReleaseSemaphore(SendCallback callback, Exception exception)
		{
			_semaphore.release();
			callback.SendComplete(exception);
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
		protected internal virtual void RecoverChecksumError(ClientCnx cnx, long sequenceId)
		{
			lock(this)
			{
				OpSendMsg op = _pendingMessages.peek();
				if(op == null)
				{
					if(_log.DebugEnabled)
					{
						_log.debug("[{}] [{}] Got send failure for timed out msg {}", Topic, _producerName, sequenceId);
					}
				}
				else
				{
					long expectedSequenceId = GetHighestSequenceId(op);
					if(sequenceId == expectedSequenceId)
					{
						bool corrupted = !VerifyLocalBufferIsNotCorrupted(op);
						if(corrupted)
						{
							// remove message from pendingMessages queue and fail callback
							_pendingMessages.remove();
							ReleaseSemaphoreForSendOp(op);
							try
							{
								op.Callback.SendComplete(new PulsarClientException.ChecksumException(format("The checksum of the message which is produced by producer %s to the topic " + "%s is corrupted", _producerName, Topic)));
							}
							catch(Exception t)
							{
								_log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", Topic, _producerName, sequenceId, t);
							}
							ReferenceCountUtil.safeRelease(op.Cmd);
							op.Recycle();
							return;
						}
						else
						{
							if(_log.DebugEnabled)
							{
								_log.debug("[{}] [{}] Message is not corrupted, retry send-message with sequenceId {}", Topic, _producerName, sequenceId);
							}
						}
        
					}
					else
					{
						if(_log.DebugEnabled)
						{
							_log.debug("[{}] [{}] Corrupt message is already timed out {}", Topic, _producerName, sequenceId);
						}
					}
				}
				// as msg is not corrupted : let producer resend pending-messages again including checksum failed message
				ResendMessages(cnx);
			}
		}

		/// <summary>
		/// Computes checksum again and verifies it against existing checksum. If checksum doesn't match it means that
		/// message is corrupt.
		/// </summary>
		/// <param name="op"> </param>
		/// <returns> returns true only if message is not modified and computed-checksum is same as previous checksum else
		///         return false that means that message is corrupted. Returns true if checksum is not present. </returns>
		protected internal virtual bool VerifyLocalBufferIsNotCorrupted(OpSendMsg op)
		{
			ByteBufPair msg = op.Cmd;

			if(msg != null)
			{
				ByteBuf headerFrame = msg.First;
				headerFrame.markReaderIndex();
				try
				{
					// skip bytes up to checksum index
					headerFrame.skipBytes(4); // skip [total-size]
					int cmdSize = (int) headerFrame.readUnsignedInt();
					headerFrame.skipBytes(cmdSize);
					// verify if checksum present
					if(hasChecksum(headerFrame))
					{
						int checksum = readChecksum(headerFrame);
						// msg.readerIndex is already at header-payload index, Recompute checksum for headers-payload
						int metadataChecksum = computeChecksum(headerFrame);
						long computedChecksum = resumeChecksum(metadataChecksum, msg.Second);
						return checksum == computedChecksum;
					}
					else
					{
						_log.warn("[{}] [{}] checksum is not present into message with id {}", Topic, _producerName, op.SequenceId);
					}
				}
				finally
				{
					headerFrame.resetReaderIndex();
				}
				return true;
			}
			else
			{
				_log.warn("[{}] Failed while casting {} into ByteBufPair", _producerName, (op.Cmd == null ? null : op.Cmd.GetType().FullName));
				return false;
			}
		}
		private void ResendMessages()
		{
			var messagesToResend = _pendingMessages.Count;
			if (messagesToResend == 0)
			{
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"[{Topic}] [{ProducerName}] No pending messages to resend {messagesToResend}");
				}
				return;
			}
			_log.Info($"[{Topic}] [{ProducerName}] Re-Sending {messagesToResend} messages to server");
			RecoverProcessOpSendMsgFrom(null);
		}
		
		private int BrokerChecksumSupportedVersion()
		{
			return (int)ProtocolVersion.V6;
		}

		internal override string HandlerName
		{
			get
			{
				return _producerName;
			}
		}

		/// <summary>
		/// Process sendTimeout events
		/// </summary>
		public override void Run(Timeout timeout)
		{
			if(timeout.Cancelled)
			{
				return;
			}

			long timeToWaitMs;

			lock(this)
			{
				// If it's closing/closed we need to ignore this timeout and not schedule next timeout.
				if(State == State.Closing || State == State.Closed)
				{
					return;
				}

				OpSendMsg firstMsg = _pendingMessages.peek();
				if(firstMsg == null)
				{
					// If there are no pending messages, reset the timeout to the configured value.
					timeToWaitMs = Conf.SendTimeoutMs;
				}
				else
				{
					// If there is at least one message, calculate the diff between the message timeout and the elapsed
					// time since first message was created.
					long diff = Conf.SendTimeoutMs - TimeUnit.NANOSECONDS.toMillis(System.nanoTime() - firstMsg.CreatedAt);
					if(diff <= 0)
					{
						// The diff is less than or equal to zero, meaning that the message has been timed out.
						// Set the callback to timeout on every message, then clear the pending queue.
						_log.info("[{}] [{}] Message send timed out. Failing {} messages", Topic, _producerName, _pendingMessages.size());

						PulsarClientException te = new PulsarClientException.TimeoutException(format("The producer %s can not send message to the topic %s within given timeout", _producerName, Topic), firstMsg.SequenceId);
						FailPendingMessages(Cnx(), te);
						_stats.IncrementSendFailed(_pendingMessages.size());
						// Since the pending queue is cleared now, set timer to expire after configured value.
						timeToWaitMs = Conf.SendTimeoutMs;
					}
					else
					{
						// The diff is greater than zero, set the timeout to the diff value
						timeToWaitMs = diff;
					}
				}

				_sendTimeout = ClientConflict.Timer().newTimeout(this, timeToWaitMs, TimeUnit.MILLISECONDS);
			}
		}


		public override CompletableFuture<Void> FlushAsync()
		{
			CompletableFuture<MessageId> lastSendFuture;
			lock(this)
			{
				if(BatchMessagingEnabled)
				{
					BatchMessageAndSend();
				}
				lastSendFuture = this._lastSendFuture;
			}
			return lastSendFuture.thenApply(ignored => null);
		}

		protected internal override void TriggerFlush()
		{
			if(BatchMessagingEnabled)
			{
				lock(this)
				{
					BatchMessageAndSend();
				}
			}
		}

		// must acquire semaphore before enqueuing
		private void BatchMessageAndSend()
		{
			if(_log.IsDebugEnabled)
			{
				_log.Info($"[{Topic}] [{_producerName}] Batching the messages from the batch container with {_batchMessageContainer.NumMessagesInBatch} messages");
			}
			if(!_batchMessageContainer.Empty)
			{
				try
				{
					IList<OpSendMsg> opSendMsgs;
					if(_batchMessageContainer.MultiBatches)
					{
						opSendMsgs = _batchMessageContainer.CreateOpSendMsgs();
					}
					else
					{
						opSendMsgs = new List<OpSendMsg> { _batchMessageContainer.CreateOpSendMsg() };
					}
					_batchMessageContainer.Clear();
					foreach(OpSendMsg opSendMsg in opSendMsgs)
					{
						ProcessOpSendMsg(opSendMsg);
					}
				}
				catch(Exception t)
				{
					_log.Warning($"[{Topic}] [{_producerName}] error while create opSendMsg by batch message container: {t}");
				}
			}
		}
		private void ProcessOpSendMsg(OpSendMsg op)
		{
			if (op == null)
			{
				return;
			}
			try
			{
				if (op.Msg != null && BatchMessagingEnabled)
				{
					BatchMessageAndSend();
				}
				_pendingMessages.Add(op);
				if (op.Msg != null)
				{
					LastSequenceIdPushed = Math.Max(LastSequenceIdPushed, GetHighestSequenceId(op));
				}
				if(Connected)
				{
					if (op.Msg != null && op.Msg.GetSchemaState() == Message<T>.SchemaState.None)
					{
						TryRegisterSchema(op.Msg);
						return;
					}

					_stats.UpdateNumMsgsSent(op.NumMessagesInBatch, op.BatchSizeByte);
					//stash message
					//cnx.Ctx().channel().eventLoop().execute(WriteInEventLoopCallback.Create(this, cnx, op));
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
				_log.Warning($"[{Topic}] [{ProducerName}] error while closing out batch -- {t}");
				SendComplete(op.Msg, DateTimeHelper.CurrentUnixTimeMillis(), new PulsarClientException(t.ToString(), op.SequenceId));
			}
		}
		private void SendComplete(Message<T> interceptorMessage, long createdAt, Exception e)
		{
			try
			{
				if (e != null)
				{
					_stats.IncrementSendFailed();
					OnSendAcknowledgement(interceptorMessage, null, e);
					_log.Error(e.ToString());
				}
				else
				{
					OnSendAcknowledgement(interceptorMessage, interceptorMessage.MessageId, null);
					_stats.IncrementNumAcksReceived(DateTimeHelper.CurrentUnixTimeMillis() - createdAt);
				}
			}
			finally
			{
				interceptorMessage = null;
			}

		}
		private void RecoverProcessOpSendMsgFrom(Message<T> from)
		{
			var pendingMessages = _pendingMessages;
			OpSendMsg pendingRegisteringOp = null;
			foreach (var op in pendingMessages)
			{
				if (from != null)
				{
					if (op?.Msg == from)
					{
						from = null;
					}
					else
					{
						continue;
					}
					if (op?.Msg != null)
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
							_pendingMessages.Remove(op);
							op.Recycle();
							continue;
						}
					}

					if (_log.IsDebugEnabled)
					{
						_log.Debug($"[{Topic}] [{ProducerName}] Re-Sending message in sequenceId {op.SequenceId}");
					}
				}
			}
			if (pendingRegisteringOp != null)
			{
				TryRegisterSchema(pendingRegisteringOp.Msg);
			}
		}

		public virtual long DelayInMillis
		{
			get
			{
				var firstMsg = _pendingMessages.FirstOrDefault();
				if (firstMsg != null)
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

		public override string ProducerName
		{
			get
			{
				return _producerName;
			}
		}

		// wrapper for connection methods
		internal virtual ClientCnx Cnx()
		{
			return this._connectionHandler.Cnx();
		}

		internal virtual void ConnectionClosed(ClientCnx cnx)
		{
			this._connectionHandler.ConnectionClosed(cnx);
		}

		internal virtual ClientCnx ClientCnx
		{
			get
			{
				return this._connectionHandler.Cnx();
			}
			set
			{
				this._connectionHandler.ClientCnx = value;
			}
		}

	}
}