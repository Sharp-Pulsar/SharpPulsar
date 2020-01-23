using System;
using System.Collections.Generic;
using System.Threading;

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
namespace SharpPulsar.Impl.Producer
{

	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;
	using Queues = com.google.common.collect.Queues;

	using ByteBuf = io.netty.buffer.ByteBuf;
	using Recycler = io.netty.util.Recycler;
	using Handle = io.netty.util.Recycler.Handle;
	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;
	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;
	using ScheduledFuture = io.netty.util.concurrent.ScheduledFuture;


	using StringUtils = org.apache.commons.lang3.StringUtils;
	using BatcherBuilder = org.apache.pulsar.client.api.BatcherBuilder;
	using CompressionType = org.apache.pulsar.client.api.CompressionType;
	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using Producer = org.apache.pulsar.client.api.Producer;
	using ProducerCryptoFailureAction = org.apache.pulsar.client.api.ProducerCryptoFailureAction;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using CryptoException = org.apache.pulsar.client.api.PulsarClientException.CryptoException;
	using Schema = org.apache.pulsar.client.api.Schema;
	using ProducerConfigurationData = SharpPulsar.Impl.conf.ProducerConfigurationData;
	using SharpPulsar.Impl.Schema;
	using MessageMetadata = org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata;
	using ProtocolVersion = org.apache.pulsar.common.api.proto.PulsarApi.ProtocolVersion;
	using CompressionCodec = org.apache.pulsar.common.compression.CompressionCodec;
	using CompressionCodecProvider = org.apache.pulsar.common.compression.CompressionCodecProvider;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using ByteBufPair = org.apache.pulsar.common.protocol.ByteBufPair;
	using Commands = org.apache.pulsar.common.protocol.Commands;
	using ChecksumType = org.apache.pulsar.common.protocol.Commands.ChecksumType;
	using SchemaHash = org.apache.pulsar.common.protocol.schema.SchemaHash;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using DateFormatter = org.apache.pulsar.common.util.DateFormatter;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
	using ByteString = org.apache.pulsar.shaded.com.google.protobuf.v241.ByteString;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;
    using SharpPulsar.Impl.Message;
    using SharpPulsar.Interface.Message;
    using DotNetty.Buffers;

    public class ProducerImpl<T> : ProducerBase<T>, TimerTask, ConnectionHandler.Connection
	{

		// Producer id, used to identify a producer within a single connection
		protected internal readonly long producerId;

		// Variable is used through the atomic updater
		private volatile long msgIdGenerator;

		private readonly BlockingQueue<OpSendMsg> pendingMessages;
		private readonly BlockingQueue<OpSendMsg> pendingCallbacks;
		private readonly Semaphore semaphore;
		private volatile Timeout sendTimeout = null;
		private volatile Timeout batchMessageAndSendTimeout = null;
		private long createProducerTimeout;
		private readonly BatchMessageContainerBase batchMessageContainer;
		private CompletableFuture<MessageId> lastSendFuture = CompletableFuture.completedFuture(null);

		// Globally unique producer name
		private string producerName;
		private bool userProvidedProducerName = false;

		private string connectionId;
		private string connectedSince;
		private readonly int partitionIndex;

		private readonly ProducerStatsRecorder stats;

		private readonly CompressionCodec compressor;

		private volatile long lastSequenceIdPublished;
		protected internal volatile long lastSequenceIdPushed;

		private MessageCrypto msgCrypto = null;
		private ScheduledFuture<object> keyGeneratorTask = null;

		private readonly IDictionary<string, string> metadata;

		private Optional<sbyte[]> schemaVersion = null;

		private readonly ConnectionHandler connectionHandler;
		private static readonly AtomicLongFieldUpdater<ProducerImpl> msgIdGeneratorUpdater = AtomicLongFieldUpdater.newUpdater(typeof(ProducerImpl), "msgIdGenerator");

		public ProducerImpl(PulsarClientImpl client, string topic, ProducerConfigurationData conf, CompletableFuture<Producer<T>> producerCreatedFuture, int partitionIndex, Schema<T> schema, ProducerInterceptors interceptors) : base(client, topic, conf, producerCreatedFuture, schema, interceptors)
		{
			this.producerId = client.newProducerId();
			this.producerName = conf.ProducerName;
			if (StringUtils.isNotBlank(producerName))
			{
				this.userProvidedProducerName = true;
			}
			this.partitionIndex = partitionIndex;
			this.pendingMessages = Queues.newArrayBlockingQueue(conf.MaxPendingMessages);
			this.pendingCallbacks = Queues.newArrayBlockingQueue(conf.MaxPendingMessages);
			this.semaphore = new Semaphore(conf.MaxPendingMessages, true);

			this.compressor = CompressionCodecProvider.getCompressionCodec(conf.CompressionType);

			if (conf.InitialSequenceId != null)
			{
				long initialSequenceId = conf.InitialSequenceId;
				this.lastSequenceIdPublished = initialSequenceId;
				this.lastSequenceIdPushed = initialSequenceId;
				this.msgIdGenerator = initialSequenceId + 1L;
			}
			else
			{
				this.lastSequenceIdPublished = -1L;
				this.lastSequenceIdPushed = -1L;
				this.msgIdGenerator = 0L;
			}

			if (conf.EncryptionEnabled)
			{
				string logCtx = "[" + topic + "] [" + producerName + "] [" + producerId + "]";
				this.msgCrypto = new MessageCrypto(logCtx, true);

				// Regenerate data key cipher at fixed interval
				keyGeneratorTask = client.eventLoopGroup().scheduleWithFixedDelay(() =>
				{
				try
				{
					msgCrypto.addPublicKeyCipher(conf.EncryptionKeys, conf.CryptoKeyReader);
				}
				catch (CryptoException e)
				{
					if (!producerCreatedFuture.Done)
					{
						log.warn("[{}] [{}] [{}] Failed to add public key cipher.", topic, producerName, producerId);
						producerCreatedFuture.completeExceptionally(PulsarClientException.wrap(e, string.Format("The producer {0} of the topic {1} " + "adds the public key cipher was failed", producerName, topic)));
					}
				}
				}, 0L, 4L, TimeUnit.HOURS);

			}

			if (conf.SendTimeoutMs > 0)
			{
				sendTimeout = client.timer().newTimeout(this, conf.SendTimeoutMs, TimeUnit.MILLISECONDS);
			}

			this.createProducerTimeout = DateTimeHelper.CurrentUnixTimeMillis() + client.Configuration.OperationTimeoutMs;
			if (conf.BatchingEnabled)
			{
				BatcherBuilder containerBuilder = conf.BatcherBuilder;
				if (containerBuilder == null)
				{
					containerBuilder = BatcherBuilder.DEFAULT;
				}
				this.batchMessageContainer = (BatchMessageContainerBase)containerBuilder.build();
				this.batchMessageContainer.Producer = this;
			}
			else
			{
				this.batchMessageContainer = null;
			}
			if (client.Configuration.StatsIntervalSeconds > 0)
			{
				stats = new ProducerStatsRecorderImpl(client, conf, this);
			}
			else
			{
				stats = ProducerStatsDisabled.INSTANCE;
			}

			if (conf.Properties.Empty)
			{
				metadata = Collections.emptyMap();
			}
			else
			{
				metadata = Collections.unmodifiableMap(new Dictionary<>(conf.Properties));
			}

			this.connectionHandler = new ConnectionHandler(this, new BackoffBuilder()
					.setInitialTime(client.Configuration.InitialBackoffIntervalNanos, TimeUnit.NANOSECONDS).setMax(client.Configuration.MaxBackoffIntervalNanos, TimeUnit.NANOSECONDS).setMandatoryStop(Math.Max(100, conf.SendTimeoutMs - 100), TimeUnit.MILLISECONDS).create(), this);

			grabCnx();
		}

		public virtual ConnectionHandler ConnectionHandler
		{
			get
			{
				return connectionHandler;
			}
		}

		private bool BatchMessagingEnabled
		{
			get
			{
				return conf.BatchingEnabled;
			}
		}

		private bool isMultiSchemaEnabled(bool autoEnable)
		{
			if (multiSchemaMode != Auto)
			{
				return multiSchemaMode == Enabled;
			}
			if (autoEnable)
			{
				multiSchemaMode = Enabled;
				return true;
			}
			return false;
		}

		public override long LastSequenceId
		{
			get
			{
				return lastSequenceIdPublished;
			}
		}

		internal override CompletableFuture<MessageId> internalSendAsync<T1>(Message<T1> message)
		{

			CompletableFuture<MessageId> future = new CompletableFuture<MessageId>();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> interceptorMessage = (MessageImpl) beforeSend(message);
			MessageImpl<object> interceptorMessage = (MessageImpl) beforeSend(message);
			//Retain the buffer used by interceptors callback to get message. Buffer will release after complete interceptors.
			interceptorMessage.DataBuffer.retain();
			if (interceptors != null)
			{
				interceptorMessage.Properties;
			}
			sendAsync(interceptorMessage, new SendCallbackAnonymousInnerClass(this, future, interceptorMessage));
			return future;
		}

		private class SendCallbackAnonymousInnerClass : SendCallback
		{
			private readonly ProducerImpl<T> outerInstance;

			private CompletableFuture<MessageId> future;
			private SharpPulsar.Impl.MessageImpl<object> interceptorMessage;

			public SendCallbackAnonymousInnerClass(ProducerImpl<T> outerInstance, CompletableFuture<MessageId> future, SharpPulsar.Impl.MessageImpl<T1> interceptorMessage)
			{
				this.outerInstance = outerInstance;
				this.future = future;
				this.interceptorMessage = interceptorMessage;
				nextCallback = null;
				nextMsg = null;
				createdAt = System.nanoTime();
			}

			internal SendCallback nextCallback;
			internal MessageImpl<object> nextMsg;
			internal long createdAt;

			public CompletableFuture<MessageId> Future
			{
				get
				{
					return future;
				}
			}

			public SendCallback NextSendCallback
			{
				get
				{
					return nextCallback;
				}
			}
			public MessageImpl<T> NextMessage
			{
				get
				{
					return nextMsg;
				}
			}

			public void sendComplete(Exception e)
			{
				try
				{
					if (e != null)
					{
						outerInstance.stats.incrementSendFailed();
						outerInstance.onSendAcknowledgement(interceptorMessage, null, e);
						future.completeExceptionally(e);
					}
					else
					{
						outerInstance.onSendAcknowledgement(interceptorMessage, interceptorMessage.getMessageId(), null);
						future.complete(interceptorMessage.getMessageId());
						outerInstance.stats.incrementNumAcksReceived(System.nanoTime() - createdAt);
					}
				}
				finally
				{
					interceptorMessage.DataBuffer.release();
				}

				while (nextCallback != null)
				{
					SendCallback sendCallback = nextCallback;
					MessageImpl<T> msg = nextMsg;
					//Retain the buffer used by interceptors callback to get message. Buffer will release after complete interceptors.
					try
					{
						msg.DataBuffer.retain();
						if (e != null)
						{
							outerInstance.stats.incrementSendFailed();
							outerInstance.onSendAcknowledgement(msg, null, e);
							sendCallback.Future.completeExceptionally(e);
						}
						else
						{
							outerInstance.onSendAcknowledgement(msg, msg.getMessageId(), null);
							sendCallback.Future.complete(msg.getMessageId());
							outerInstance.stats.incrementNumAcksReceived(System.nanoTime() - createdAt);
						}
						nextMsg = nextCallback.NextMessage;
						nextCallback = nextCallback.NextSendCallback;
					}
					finally
					{
						msg.DataBuffer.release();
					}
				}
			}

			public void addCallback<T1>(MessageImpl<T1> msg, SendCallback scb)
			{
				nextMsg = msg;
				nextCallback = scb;
			}
		}

		public virtual void sendAsync<T1>(IMessage<T1> message, SendCallback callback)
		{
			checkArgument(message is MessageImpl);

			if (!isValidProducerState(callback))
			{
				return;
			}

			if (!canEnqueueRequest(callback))
			{
				return;
			}
			MessageImpl<T1> msg = (MessageImpl<T1>) message;
			MessageMetadata.Builder msgMetadataBuilder = msg.MessageBuilder;
			IByteBuffer payload = msg.DataBuffer;

			// If compression is enabled, we are compressing, otherwise it will simply use the same buffer
			int uncompressedSize = payload.ReadableBytes;
			IByteBuffer compressedPayload = payload;
			// Batch will be compressed when closed
			// If a message has a delayed delivery time, we'll always send it individually
			if (!BatchMessagingEnabled || msgMetadataBuilder.hasDeliverAtTime())
			{
				compressedPayload = compressor.encode(payload);
				payload.release();

				// validate msg-size (For batching this will be check at the batch completion size)
				int compressedSize = compressedPayload.ReadableBytes;
				if (compressedSize > ClientConnection.maxMessageSize)
				{
					compressedPayload.Release();
					string compressedStr = (!BatchMessagingEnabled && conf.CompressionType != CompressionType.NONE) ? "Compressed" : "";
					PulsarClientException.InvalidMessageException invalidMessageException = new PulsarClientException.InvalidMessageException(format("The producer %s of the topic %s sends a %s message with %d bytes that exceeds %d bytes", producerName, topic, compressedStr, compressedSize, ClientCnx.MaxMessageSize));
					callback.sendComplete(invalidMessageException);
					return;
				}
			}

			if (!msg.Replicated && msgMetadataBuilder.hasProducerName())
			{
				PulsarClientException.InvalidMessageException invalidMessageException = new PulsarClientException.InvalidMessageException(format("The producer %s of the topic %s can not reuse the same message", producerName, topic));
				callback.sendComplete(invalidMessageException);
				compressedPayload.release();
				return;
			}

			if (!PopulateMessageSchema(msg, callback))
			{
				compressedPayload.release();
				return;
			}

			try
			{
				lock (this)
				{
					long sequenceId;
					if (!msgMetadataBuilder.hasSequenceId())
					{
						sequenceId = msgIdGeneratorUpdater.getAndIncrement(this);
						msgMetadataBuilder.SequenceId = sequenceId;
					}
					else
					{
						sequenceId = msgMetadataBuilder.SequenceId;
					}
					if (!msgMetadataBuilder.hasPublishTime())
					{
						msgMetadataBuilder.PublishTime = client.ClientClock.millis();

						checkArgument(!msgMetadataBuilder.hasProducerName());

						msgMetadataBuilder.ProducerName = producerName;

						if (conf.CompressionType != CompressionType.NONE)
						{
							msgMetadataBuilder.Compression = CompressionCodecProvider.convertToWireProtocol(conf.CompressionType);
						}
						msgMetadataBuilder.UncompressedSize = uncompressedSize;
					}
					if (canAddToBatch(msg))
					{
						if (canAddToCurrentBatch(msg))
						{
							// should trigger complete the batch message, new message will add to a new batch and new batch
							// sequence id use the new message, so that broker can handle the message duplication
							if (sequenceId <= lastSequenceIdPushed)
							{
								if (sequenceId <= lastSequenceIdPublished)
								{
									log.warn("Message with sequence id {} is definitely a duplicate", sequenceId);
								}
								else
								{
									log.info("Message with sequence id {} might be a duplicate but cannot be determined at this time.", sequenceId);
								}
								doBatchSendAndAdd(msg, callback, payload);
							}
							else
							{
								// handle boundary cases where message being added would exceed
								// batch size and/or max message size
								bool isBatchFull = batchMessageContainer.add(msg, callback);
								lastSendFuture = callback.Future;
								payload.release();
								if (isBatchFull)
								{
									batchMessageAndSend();
								}
							}
						}
						else
						{
							doBatchSendAndAdd(msg, callback, payload);
						}
					}
					else
					{
						ByteBuf encryptedPayload = encryptMessage(msgMetadataBuilder, compressedPayload);
						// When publishing during replication, we need to set the correct number of message in batch
						// This is only used in tracking the publish rate stats
						int numMessages = msg.MessageBuilder.hasNumMessagesInBatch() ? msg.MessageBuilder.NumMessagesInBatch : 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final OpSendMsg op;
						OpSendMsg op;
						if (msg.getSchemaState() == MessageImpl.SchemaState.Ready)
						{
							MessageMetadata msgMetadata = msgMetadataBuilder.build();
							ByteBufPair cmd = sendMessage(producerId, sequenceId, numMessages, msgMetadata, encryptedPayload);
							op = OpSendMsg.create(msg, cmd, sequenceId, callback);
							msgMetadataBuilder.recycle();
							msgMetadata.recycle();
						}
						else
						{
							op = OpSendMsg.create(msg, null, sequenceId, callback);
							op.rePopulate = () =>
							{
							MessageMetadata msgMetadata = msgMetadataBuilder.build();
							op.cmd = sendMessage(producerId, sequenceId, numMessages, msgMetadata, encryptedPayload);
							msgMetadataBuilder.recycle();
							msgMetadata.recycle();
							};
						}
						op.NumMessagesInBatch = numMessages;
						op.BatchSizeByte = encryptedPayload.readableBytes();
						lastSendFuture = callback.Future;
						processOpSendMsg(op);
					}
				}
			}
			catch (PulsarClientException e)
			{
				semaphore.release();
				callback.sendComplete(e);
			}
			catch (Exception t)
			{
				semaphore.release();
				callback.sendComplete(new PulsarClientException(t));
			}
		}

		private bool PopulateMessageSchema(MessageImpl msg, SendCallback callback)
		{
			MessageMetadata.Builder msgMetadataBuilder = msg.MessageBuilder;
			if (msg.Schema == schema)
			{
				schemaVersion.ifPresent(v => msgMetadataBuilder.setSchemaVersion(ByteString.copyFrom(v)));
				msg.setSchemaState(MessageImpl.SchemaState.Ready);
				return true;
			}
			if (!isMultiSchemaEnabled(true))
			{
				callback.sendComplete(new PulsarClientException.InvalidMessageException(format("The producer %s of the topic %s is disabled the `MultiSchema`", producerName, topic)));
				return false;
			}
			SchemaHash schemaHash = SchemaHash.of(msg.Schema);
			sbyte[] schemaVersion = schemaCache.get(schemaHash);
			if (schemaVersion != null)
			{
				msgMetadataBuilder.SchemaVersion = ByteString.copyFrom(schemaVersion);
				msg.setSchemaState(MessageImpl.SchemaState.Ready);
			}
			return true;
		}

		private bool rePopulateMessageSchema(MessageImpl msg)
		{
			SchemaHash schemaHash = SchemaHash.of(msg.Schema);
			sbyte[] schemaVersion = schemaCache.get(schemaHash);
			if (schemaVersion == null)
			{
				return false;
			}
			msg.MessageBuilder.SchemaVersion = ByteString.copyFrom(schemaVersion);
			msg.setSchemaState(MessageImpl.SchemaState.Ready);
			return true;
		}

		private void tryRegisterSchema(ClientCnx cnx, MessageImpl msg, SendCallback callback)
		{
			if (!changeToRegisteringSchemaState())
			{
				return;
			}
			SchemaInfo schemaInfo = Optional.ofNullable(msg.Schema).map(Schema.getSchemaInfo).filter(si => si.Type.Value > 0).orElse(Schema.BYTES.SchemaInfo);
			getOrCreateSchemaAsync(cnx, schemaInfo).handle((v, ex) =>
			{
			if (ex != null)
			{
				Exception t = FutureUtil.unwrapCompletionException(ex);
				log.warn("[{}] [{}] GetOrCreateSchema error", topic, producerName, t);
				if (t is PulsarClientException.IncompatibleSchemaException)
				{
					msg.setSchemaState(MessageImpl.SchemaState.Broken);
					callback.sendComplete((PulsarClientException.IncompatibleSchemaException) t);
				}
			}
			else
			{
				log.warn("[{}] [{}] GetOrCreateSchema succeed", topic, producerName);
				SchemaHash schemaHash = SchemaHash.of(msg.Schema);
				schemaCache.putIfAbsent(schemaHash, v);
				msg.MessageBuilder.SchemaVersion = ByteString.copyFrom(v);
				msg.setSchemaState(MessageImpl.SchemaState.Ready);
			}
			cnx.ctx().channel().eventLoop().execute(() =>
			{
				lock (ProducerImpl.this)
				{
					recoverProcessOpSendMsgFrom(cnx, msg);
				}
			});
			return null;
			});
public void connectionFailed(org.apache.pulsar.client.api.PulsarClientException exception)
		{
			throw new NotImplementedException();
		}

		public void connectionOpened(ClientCnx cnx)
		{
			throw new NotImplementedException();
		}
	}

		private CompletableFuture<sbyte[]> getOrCreateSchemaAsync(ClientCnx cnx, SchemaInfo schemaInfo)
		{
			if (!Commands.peerSupportsGetOrCreateSchema(cnx.RemoteEndpointProtocolVersion))
			{
				return FutureUtil.failedFuture(new PulsarClientException.NotSupportedException(format("The command `GetOrCreateSchema` is not supported for the protocol version %d. " + "The producer is %s, topic is %s", cnx.RemoteEndpointProtocolVersion, producerName, topic)));
			}
			long requestId = client.newRequestId();
			ByteBuf request = Commands.newGetOrCreateSchema(requestId, topic, schemaInfo);
			log.info("[{}] [{}] GetOrCreateSchema request", topic, producerName);
			return cnx.sendGetOrCreateSchema(request, requestId);
		}
		protected internal virtual ByteBuf encryptMessage(MessageMetadata.Builder msgMetadata, ByteBuf compressedPayload)
		{

			ByteBuf encryptedPayload = compressedPayload;
			if (!conf.EncryptionEnabled || msgCrypto == null)
			{
				return encryptedPayload;
			}
			try
			{
				encryptedPayload = msgCrypto.encrypt(conf.EncryptionKeys, conf.CryptoKeyReader, msgMetadata, compressedPayload);
			}
			catch (PulsarClientException e)
			{
				// Unless config is set to explicitly publish un-encrypted message upon failure, fail the request
				if (conf.CryptoFailureAction == ProducerCryptoFailureAction.SEND)
				{
					log.warn("[{}] [{}] Failed to encrypt message {}. Proceeding with publishing unencrypted message", topic, producerName, e.Message);
					return compressedPayload;
				}
				throw e;
			}
			return encryptedPayload;
		}

		protected internal virtual ByteBufPair sendMessage(long producerId, long sequenceId, int numMessages, MessageMetadata msgMetadata, ByteBuf compressedPayload)
		{
			return Commands.newSend(producerId, sequenceId, numMessages, ChecksumType, msgMetadata, compressedPayload);
		}

		protected internal virtual ByteBufPair sendMessage(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, MessageMetadata msgMetadata, ByteBuf compressedPayload)
		{
			return Commands.newSend(producerId, lowestSequenceId, highestSequenceId, numMessages, ChecksumType, msgMetadata, compressedPayload);
		}

		private Commands.ChecksumType ChecksumType
		{
			get
			{
				if (connectionHandler.ClientCnx == null || connectionHandler.ClientCnx.RemoteEndpointProtocolVersion >= brokerChecksumSupportedVersion())
				{
					return Commands.ChecksumType.Crc32c;
				}
				else
				{
					return Commands.ChecksumType.None;
				}
			}
		}

		private bool canAddToBatch<T1>(MessageImpl<T1> msg)
		{
			return msg.getSchemaState() == MessageImpl.SchemaState.Ready && BatchMessagingEnabled && !msg.MessageBuilder.hasDeliverAtTime();
		}

		private bool canAddToCurrentBatch<T1>(MessageImpl<T1> msg)
		{
			return batchMessageContainer.haveEnoughSpace(msg) && (!isMultiSchemaEnabled(false) || batchMessageContainer.hasSameSchema(msg));
		}

		private void doBatchSendAndAdd<T1>(MessageImpl<T1> msg, SendCallback callback, ByteBuf payload)
		{
			if (log.DebugEnabled)
			{
				log.debug("[{}] [{}] Closing out batch to accommodate large message with size {}", topic, producerName, msg.DataBuffer.readableBytes());
			}
			try
			{
				batchMessageAndSend();
				batchMessageContainer.add(msg, callback);
				lastSendFuture = callback.Future;
			}
			finally
			{
				payload.release();
			}
		}

		private bool isValidProducerState(SendCallback callback)
		{
			switch (State)
			{
			case Ready:
				// OK
			case Connecting:
				// We are OK to queue the messages on the client, it will be sent to the broker once we get the connection
			case RegisteringSchema:
				// registering schema
				return true;
			case Closing:
			case Closed:
				callback.sendComplete(new PulsarClientException.AlreadyClosedException("Producer already closed"));
				return false;
			case Terminated:
				callback.sendComplete(new PulsarClientException.TopicTerminatedException("Topic was terminated"));
				return false;
			case Failed:
			case Uninitialized:
			default:
				callback.sendComplete(new PulsarClientException.NotConnectedException());
				return false;
			}
		}

		private bool canEnqueueRequest(SendCallback callback)
		{
			try
			{
				if (conf.BlockIfQueueFull)
				{
					semaphore.acquire();
				}
				else
				{
					if (!semaphore.tryAcquire())
					{
						callback.sendComplete(new PulsarClientException.ProducerQueueIsFullError("Producer send queue is full"));
						return false;
					}
				}
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				callback.sendComplete(new PulsarClientException(e));
				return false;
			}

			return true;
		}

		private sealed class WriteInEventLoopCallback : ThreadStart
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private ProducerImpl<?> producer;
			internal ProducerImpl<object> producer;
			internal ByteBufPair cmd;
			internal long sequenceId;
			internal ClientCnx cnx;

			internal static WriteInEventLoopCallback create<T1>(ProducerImpl<T1> producer, ClientCnx cnx, OpSendMsg op)
			{
				WriteInEventLoopCallback c = RECYCLER.get();
				c.producer = producer;
				c.cnx = cnx;
				c.sequenceId = op.sequenceId;
				c.cmd = op.cmd;
				return c;
			}

			public override void run()
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] [{}] Sending message cnx {}, sequenceId {}", producer.topic, producer.producerName, cnx, sequenceId);
				}

				try
				{
					cnx.ctx().writeAndFlush(cmd, cnx.ctx().voidPromise());
				}
				finally
				{
					recycle();
				}
			}

			internal void recycle()
			{
				producer = null;
				cnx = null;
				cmd = null;
				sequenceId = -1;
				recyclerHandle.recycle(this);
			}

			internal readonly Recycler.Handle<WriteInEventLoopCallback> recyclerHandle;

			internal WriteInEventLoopCallback(Recycler.Handle<WriteInEventLoopCallback> recyclerHandle)
			{
				this.recyclerHandle = recyclerHandle;
			}

			internal static readonly Recycler<WriteInEventLoopCallback> RECYCLER = new RecyclerAnonymousInnerClass();

			private class RecyclerAnonymousInnerClass : Recycler<WriteInEventLoopCallback>
			{
				protected internal override WriteInEventLoopCallback newObject(Recycler.Handle<WriteInEventLoopCallback> handle)
				{
					return new WriteInEventLoopCallback(handle);
				}
			}
		}

		public override CompletableFuture<Void> closeAsync()
		{
			State currentState = getAndUpdateState(state =>
			{
			if (state == State.Closed)
			{
				return state;
			}
			return State.Closing;
			});

			if (currentState == State.Closed || currentState == State.Closing)
			{
				return CompletableFuture.completedFuture(null);
			}

			Timeout timeout = sendTimeout;
			if (timeout != null)
			{
				timeout.cancel();
				sendTimeout = null;
			}

			Timeout batchTimeout = batchMessageAndSendTimeout;
			if (batchTimeout != null)
			{
				batchTimeout.cancel();
				batchMessageAndSendTimeout = null;
			}

			if (keyGeneratorTask != null && !keyGeneratorTask.Cancelled)
			{
				keyGeneratorTask.cancel(false);
			}

			stats.cancelStatsTimeout();

			ClientCnx cnx = cnx();
			if (cnx == null || currentState != State.Ready)
			{
				log.info("[{}] [{}] Closed Producer (not connected)", topic, producerName);
				lock (this)
				{
					State = State.Closed;
					client.cleanupProducer(this);
					PulsarClientException ex = new PulsarClientException.AlreadyClosedException(format("The producer %s of the topic %s was already closed when closing the producers", producerName, topic));
					pendingMessages.forEach(msg =>
					{
					msg.callback.sendComplete(ex);
					msg.cmd.release();
					msg.recycle();
					});
					pendingMessages.clear();
				}

				return CompletableFuture.completedFuture(null);
			}

			long requestId = client.newRequestId();
			ByteBuf cmd = Commands.newCloseProducer(producerId, requestId);

			CompletableFuture<Void> closeFuture = new CompletableFuture<Void>();
			cnx.sendRequestWithId(cmd, requestId).handle((v, exception) =>
			{
			cnx.removeProducer(producerId);
			if (exception == null || !cnx.ctx().channel().Active)
			{
				lock (ProducerImpl.this)
				{
					log.info("[{}] [{}] Closed Producer", topic, producerName);
					State = State.Closed;
					pendingMessages.forEach(msg =>
					{
						msg.cmd.release();
						msg.recycle();
					});
					pendingMessages.clear();
				}
				closeFuture.complete(null);
				client.cleanupProducer(this);
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
				return connectionHandler.ClientCnx != null && (State == State.Ready);
			}
		}

		public virtual bool Writable
		{
			get
			{
				ClientCnx cnx = connectionHandler.ClientCnx;
				return cnx != null && cnx.channel().Writable;
			}
		}

		public virtual void terminated(ClientCnx cnx)
		{
			State previousState = getAndUpdateState(state => (state == State.Closed ? State.Closed : State.Terminated));
			if (previousState != State.Terminated && previousState != State.Closed)
			{
				log.info("[{}] [{}] The topic has been terminated", topic, producerName);
				ClientCnx = null;

				failPendingMessages(cnx, new PulsarClientException.TopicTerminatedException(format("The topic %s that the producer %s produces to has been terminated", topic, producerName)));
			}
		}

		internal virtual void ackReceived(ClientCnx cnx, long sequenceId, long highestSequenceId, long ledgerId, long entryId)
		{
			OpSendMsg op = null;
			bool callback = false;
			lock (this)
			{
				op = pendingMessages.peek();
				if (op == null)
				{
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] Got ack for timed out msg {} - {}", topic, producerName, sequenceId, highestSequenceId);
					}
					return;
				}

				if (sequenceId > op.sequenceId)
				{
					log.warn("[{}] [{}] Got ack for msg. expecting: {} - {} - got: {} - {} - queue-size: {}", topic, producerName, op.sequenceId, op.highestSequenceId, sequenceId, highestSequenceId, pendingMessages.size());
					// Force connection closing so that messages can be re-transmitted in a new connection
					cnx.channel().close();
				}
				else if (sequenceId < op.sequenceId)
				{
					// Ignoring the ack since it's referring to a message that has already timed out.
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] Got ack for timed out msg. expecting: {} - {} - got: {} - {}", topic, producerName, op.sequenceId, op.highestSequenceId, sequenceId, highestSequenceId);
					}
				}
				else
				{
					// Add check `sequenceId >= highestSequenceId` for backward compatibility.
					if (sequenceId >= highestSequenceId || highestSequenceId == op.highestSequenceId)
					{
						// Message was persisted correctly
						if (log.DebugEnabled)
						{
							log.debug("[{}] [{}] Received ack for msg {} ", topic, producerName, sequenceId);
						}
						pendingMessages.remove();
						releaseSemaphoreForSendOp(op);
						callback = true;
						pendingCallbacks.add(op);
					}
					else
					{
						log.warn("[{}] [{}] Got ack for batch msg error. expecting: {} - {} - got: {} - {} - queue-size: {}", topic, producerName, op.sequenceId, op.highestSequenceId, sequenceId, highestSequenceId, pendingMessages.size());
						// Force connection closing so that messages can be re-transmitted in a new connection
						cnx.channel().close();
					}
				}
			}
			if (callback)
			{
				op = pendingCallbacks.poll();
				if (op != null)
				{
					lastSequenceIdPublished = Math.Max(lastSequenceIdPublished, getHighestSequenceId(op));
					op.setMessageId(ledgerId, entryId, partitionIndex);
					try
					{
						// Need to protect ourselves from any exception being thrown in the future handler from the
						// application
						op.callback.sendComplete(null);
					}
					catch (Exception t)
					{
						log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", topic, producerName, sequenceId, t);
					}
					ReferenceCountUtil.safeRelease(op.cmd);
					op.recycle();
				}
			}
		}

		private long getHighestSequenceId(OpSendMsg op)
		{
			return Math.Max(op.highestSequenceId, op.sequenceId);
		}

		private void releaseSemaphoreForSendOp(OpSendMsg op)
		{
			semaphore.release(BatchMessagingEnabled ? op.numMessagesInBatch : 1);
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
		protected internal virtual void recoverChecksumError(ClientCnx cnx, long sequenceId)
		{
			lock (this)
			{
				OpSendMsg op = pendingMessages.peek();
				if (op == null)
				{
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] Got send failure for timed out msg {}", topic, producerName, sequenceId);
					}
				}
				else
				{
					long expectedSequenceId = getHighestSequenceId(op);
					if (sequenceId == expectedSequenceId)
					{
						bool corrupted = !verifyLocalBufferIsNotCorrupted(op);
						if (corrupted)
						{
							// remove message from pendingMessages queue and fail callback
							pendingMessages.remove();
							releaseSemaphoreForSendOp(op);
							try
							{
								op.callback.sendComplete(new PulsarClientException.ChecksumException(format("The checksum of the message which is produced by producer %s to the topic " + "%s is corrupted", producerName, topic)));
							}
							catch (Exception t)
							{
								log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", topic, producerName, sequenceId, t);
							}
							ReferenceCountUtil.safeRelease(op.cmd);
							op.recycle();
							return;
						}
						else
						{
							if (log.DebugEnabled)
							{
								log.debug("[{}] [{}] Message is not corrupted, retry send-message with sequenceId {}", topic, producerName, sequenceId);
							}
						}
        
					}
					else
					{
						if (log.DebugEnabled)
						{
							log.debug("[{}] [{}] Corrupt message is already timed out {}", topic, producerName, sequenceId);
						}
					}
				}
				// as msg is not corrupted : let producer resend pending-messages again including checksum failed message
				resendMessages(cnx);
			}
		}

		/// <summary>
		/// Computes checksum again and verifies it against existing checksum. If checksum doesn't match it means that
		/// message is corrupt.
		/// </summary>
		/// <param name="op"> </param>
		/// <returns> returns true only if message is not modified and computed-checksum is same as previous checksum else
		///         return false that means that message is corrupted. Returns true if checksum is not present. </returns>
		protected internal virtual bool verifyLocalBufferIsNotCorrupted(OpSendMsg op)
		{
			ByteBufPair msg = op.cmd;

			if (msg != null)
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
					if (hasChecksum(headerFrame))
					{
						int checksum = readChecksum(headerFrame);
						// msg.readerIndex is already at header-payload index, Recompute checksum for headers-payload
						int metadataChecksum = computeChecksum(headerFrame);
						long computedChecksum = resumeChecksum(metadataChecksum, msg.Second);
						return checksum == computedChecksum;
					}
					else
					{
						log.warn("[{}] [{}] checksum is not present into message with id {}", topic, producerName, op.sequenceId);
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
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				log.warn("[{}] Failed while casting {} into ByteBufPair", producerName, (op.cmd == null ? null : op.cmd.GetType().FullName));
				return false;
			}
		}

		protected internal sealed class OpSendMsg
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> msg;
			internal MessageImpl<object> msg;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<MessageImpl<?>> msgs;
			internal IList<MessageImpl<object>> msgs;
			internal ByteBufPair cmd;
			internal SendCallback callback;
			internal ThreadStart rePopulate;
			internal long sequenceId;
			internal long createdAt;
			internal long batchSizeByte = 0;
			internal int numMessagesInBatch = 1;
			internal long highestSequenceId;

			internal static OpSendMsg create<T1>(MessageImpl<T1> msg, ByteBufPair cmd, long sequenceId, SendCallback callback)
			{
				OpSendMsg op = RECYCLER.get();
				op.msg = msg;
				op.cmd = cmd;
				op.callback = callback;
				op.sequenceId = sequenceId;
				op.createdAt = DateTimeHelper.CurrentUnixTimeMillis();
				return op;
			}

			internal static OpSendMsg create<T1>(IList<T1> msgs, ByteBufPair cmd, long sequenceId, SendCallback callback)
			{
				OpSendMsg op = RECYCLER.get();
				op.msgs = msgs;
				op.cmd = cmd;
				op.callback = callback;
				op.sequenceId = sequenceId;
				op.createdAt = DateTimeHelper.CurrentUnixTimeMillis();
				return op;
			}

			internal static OpSendMsg create<T1>(IList<T1> msgs, ByteBufPair cmd, long lowestSequenceId, long highestSequenceId, SendCallback callback)
			{
				OpSendMsg op = RECYCLER.get();
				op.msgs = msgs;
				op.cmd = cmd;
				op.callback = callback;
				op.sequenceId = lowestSequenceId;
				op.highestSequenceId = highestSequenceId;
				op.createdAt = DateTimeHelper.CurrentUnixTimeMillis();
				return op;
			}

			internal void recycle()
			{
				msg = null;
				msgs = null;
				cmd = null;
				callback = null;
				rePopulate = null;
				sequenceId = -1L;
				createdAt = -1L;
				highestSequenceId = -1L;
				recyclerHandle.recycle(this);
			}

			internal int NumMessagesInBatch
			{
				set
				{
					this.numMessagesInBatch = value;
				}
			}

			internal long BatchSizeByte
			{
				set
				{
					this.batchSizeByte = value;
				}
			}

			internal void setMessageId(long ledgerId, long entryId, int partitionIndex)
			{
				if (msg != null)
				{
					msg.setMessageId(new MessageIdImpl(ledgerId, entryId, partitionIndex));
				}
				else
				{
					for (int batchIndex = 0; batchIndex < msgs.Count; batchIndex++)
					{
						msgs[batchIndex].setMessageId(new BatchMessageIdImpl(ledgerId, entryId, partitionIndex, batchIndex));
					}
				}
			}

			internal OpSendMsg(Recycler.Handle<OpSendMsg> recyclerHandle)
			{
				this.recyclerHandle = recyclerHandle;
			}

			internal readonly Recycler.Handle<OpSendMsg> recyclerHandle;
			internal static readonly Recycler<OpSendMsg> RECYCLER = new RecyclerAnonymousInnerClass();

			private class RecyclerAnonymousInnerClass : Recycler<OpSendMsg>
			{
				protected internal override OpSendMsg newObject(Recycler.Handle<OpSendMsg> handle)
				{
					return new OpSendMsg(handle);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: @Override public void connectionOpened(final ClientCnx cnx)
		public virtual void connectionOpened(ClientCnx cnx)
		{
			// we set the cnx reference before registering the producer on the cnx, so if the cnx breaks before creating the
			// producer, it will try to grab a new cnx
			connectionHandler.ClientCnx = cnx;
			cnx.registerProducer(producerId, this);

			log.info("[{}] [{}] Creating producer on cnx {}", topic, producerName, cnx.ctx().channel());

			long requestId = client.newRequestId();

			SchemaInfo schemaInfo = null;
			if (schema != null)
			{
				if (schema.SchemaInfo != null)
				{
					if (schema.SchemaInfo.Type == SchemaType.JSON)
					{
						// for backwards compatibility purposes
						// JSONSchema originally generated a schema for pojo based of of the JSON schema standard
						// but now we have standardized on every schema to generate an Avro based schema
						if (Commands.peerSupportJsonSchemaAvroFormat(cnx.RemoteEndpointProtocolVersion))
						{
							schemaInfo = schema.SchemaInfo;
						}
						else if (schema is JSONSchema)
						{
							JSONSchema jsonSchema = (JSONSchema) schema;
							schemaInfo = jsonSchema.BackwardsCompatibleJsonSchemaInfo;
						}
						else
						{
							schemaInfo = schema.SchemaInfo;
						}
					}
					else if (schema.SchemaInfo.Type == SchemaType.BYTES || schema.SchemaInfo.Type == SchemaType.NONE)
					{
						// don't set schema info for Schema.BYTES
						schemaInfo = null;
					}
					else
					{
						schemaInfo = schema.SchemaInfo;
					}
				}
			}

			cnx.sendRequestWithId(Commands.newProducer(topic, producerId, requestId, producerName, conf.EncryptionEnabled, metadata, schemaInfo, connectionHandler.epoch, userProvidedProducerName), requestId).thenAccept(response =>
			{
			string producerName = response.ProducerName;
			long lastSequenceId = response.LastSequenceId;
			schemaVersion = Optional.ofNullable(response.SchemaVersion);
			schemaVersion.ifPresent(v => schemaCache.put(SchemaHash.of(schema), v));
			lock (ProducerImpl.this)
			{
				if (State == State.Closing || State == State.Closed)
				{
					cnx.removeProducer(producerId);
					cnx.channel().close();
					return;
				}
				resetBackoff();
				log.info("[{}] [{}] Created producer on cnx {}", topic, producerName, cnx.ctx().channel());
				connectionId = cnx.ctx().channel().ToString();
				connectedSince = DateFormatter.now();
				if (string.ReferenceEquals(this.producerName, null))
				{
					this.producerName = producerName;
				}
				if (this.msgIdGenerator == 0 && conf.InitialSequenceId == null)
				{
					this.lastSequenceIdPublished = lastSequenceId;
					this.msgIdGenerator = lastSequenceId + 1;
				}
				if (!producerCreatedFuture_Conflict.Done && BatchMessagingEnabled)
				{
					client.timer().newTimeout(batchMessageAndSendTask, conf.BatchingMaxPublishDelayMicros, TimeUnit.MICROSECONDS);
				}
				resendMessages(cnx);
			}
			}).exceptionally((e) =>
			{
			Exception cause = e.Cause;
			cnx.removeProducer(producerId);
			if (State == State.Closing || State == State.Closed)
			{
				cnx.channel().close();
				return null;
			}
			log.error("[{}] [{}] Failed to create producer: {}", topic, producerName, cause.Message);
			if (cause is PulsarClientException.ProducerBlockedQuotaExceededException)
			{
				lock (this)
				{
					log.warn("[{}] [{}] Topic backlog quota exceeded. Throwing Exception on producer.", topic, producerName);
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] Pending messages: {}", topic, producerName, pendingMessages.size());
					}
					PulsarClientException bqe = new PulsarClientException.ProducerBlockedQuotaExceededException(format("The backlog quota of the topic %s that the producer %s produces to is exceeded", topic, producerName));
					failPendingMessages(cnx(), bqe);
				}
			}
			else if (cause is PulsarClientException.ProducerBlockedQuotaExceededError)
			{
				log.warn("[{}] [{}] Producer is blocked on creation because backlog exceeded on topic.", producerName, topic);
			}
			if (cause is PulsarClientException.TopicTerminatedException)
			{
				State = State.Terminated;
				failPendingMessages(cnx(), (PulsarClientException) cause);
				producerCreatedFuture_Conflict.completeExceptionally(cause);
				client.cleanupProducer(this);
			}
			else if (producerCreatedFuture_Conflict.Done || (cause is PulsarClientException && connectionHandler.isRetriableError((PulsarClientException) cause) && DateTimeHelper.CurrentUnixTimeMillis() < createProducerTimeout))
			{
				reconnectLater(cause);
			}
			else
			{
				State = State.Failed;
				producerCreatedFuture_Conflict.completeExceptionally(cause);
				client.cleanupProducer(this);
			}
			return null;
		});
		}

		public virtual void connectionFailed(PulsarClientException exception)
		{
			if (DateTimeHelper.CurrentUnixTimeMillis() > createProducerTimeout && producerCreatedFuture_Conflict.completeExceptionally(exception))
			{
				log.info("[{}] Producer creation failed for producer {}", topic, producerId);
				State = State.Failed;
				client.cleanupProducer(this);
			}
		}

		private void resendMessages(ClientCnx cnx)
		{
			cnx.ctx().channel().eventLoop().execute(() =>
			{
			lock (this)
			{
				if (State == State.Closing || State == State.Closed)
				{
					cnx.channel().close();
					return;
				}
				int messagesToResend = pendingMessages.size();
				if (messagesToResend == 0)
				{
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] No pending messages to resend {}", topic, producerName, messagesToResend);
					}
					if (changeToReadyState())
					{
						producerCreatedFuture_Conflict.complete(ProducerImpl.this);
						return;
					}
					else
					{
						cnx.channel().close();
						return;
					}
				}
				log.info("[{}] [{}] Re-Sending {} messages to server", topic, producerName, messagesToResend);
				recoverProcessOpSendMsgFrom(cnx, null);
			}
			});
		}

		/// <summary>
		/// Strips checksum from <seealso cref="OpSendMsg"/> command if present else ignore it.
		/// </summary>
		/// <param name="op"> </param>
		private void stripChecksum(OpSendMsg op)
		{
			int totalMsgBufSize = op.cmd.readableBytes();
			ByteBufPair msg = op.cmd;
			if (msg != null)
			{
				ByteBuf headerFrame = msg.First;
				headerFrame.markReaderIndex();
				try
				{
					headerFrame.skipBytes(4); // skip [total-size]
					int cmdSize = (int) headerFrame.readUnsignedInt();

					// verify if checksum present
					headerFrame.skipBytes(cmdSize);

					if (!hasChecksum(headerFrame))
					{
						return;
					}

					int headerSize = 4 + 4 + cmdSize; // [total-size] [cmd-length] [cmd-size]
					int checksumSize = 4 + 2; // [magic-number] [checksum-size]
					int checksumMark = (headerSize + checksumSize); // [header-size] [checksum-size]
					int metaPayloadSize = (totalMsgBufSize - checksumMark); // metadataPayload = totalSize - checksumMark
					int newTotalFrameSizeLength = 4 + cmdSize + metaPayloadSize; // new total-size without checksum
					headerFrame.resetReaderIndex();
					int headerFrameSize = headerFrame.readableBytes();

					headerFrame.setInt(0, newTotalFrameSizeLength); // rewrite new [total-size]
					ByteBuf metadata = headerFrame.slice(checksumMark, headerFrameSize - checksumMark); // sliced only
																										// metadata
					headerFrame.writerIndex(headerSize); // set headerFrame write-index to overwrite metadata over checksum
					metadata.readBytes(headerFrame, metadata.readableBytes());
					headerFrame.capacity(headerFrameSize - checksumSize); // reduce capacity by removed checksum bytes
				}
				finally
				{
					headerFrame.resetReaderIndex();
				}
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				log.warn("[{}] Failed while casting {} into ByteBufPair", producerName, (op.cmd == null ? null : op.cmd.GetType().FullName));
			}
		}

		public virtual int brokerChecksumSupportedVersion()
		{
			return ProtocolVersion.v6.Number;
		}

		internal override string HandlerName
		{
			get
			{
				return producerName;
			}
		}

		/// <summary>
		/// Process sendTimeout events
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
		public override void run(Timeout timeout)
		{
			if (timeout.Cancelled)
			{
				return;
			}

			long timeToWaitMs;

			lock (this)
			{
				// If it's closing/closed we need to ignore this timeout and not schedule next timeout.
				if (State == State.Closing || State == State.Closed)
				{
					return;
				}

				OpSendMsg firstMsg = pendingMessages.peek();
				if (firstMsg == null)
				{
					// If there are no pending messages, reset the timeout to the configured value.
					timeToWaitMs = conf.SendTimeoutMs;
				}
				else
				{
					// If there is at least one message, calculate the diff between the message timeout and the current
					// time.
					long diff = (firstMsg.createdAt + conf.SendTimeoutMs) - DateTimeHelper.CurrentUnixTimeMillis();
					if (diff <= 0)
					{
						// The diff is less than or equal to zero, meaning that the message has been timed out.
						// Set the callback to timeout on every message, then clear the pending queue.
						log.info("[{}] [{}] Message send timed out. Failing {} messages", topic, producerName, pendingMessages.size());

						PulsarClientException te = new PulsarClientException.TimeoutException(format("The producer %s can not send message to the topic %s within given timeout", producerName, topic));
						failPendingMessages(cnx(), te);
						stats.incrementSendFailed(pendingMessages.size());
						// Since the pending queue is cleared now, set timer to expire after configured value.
						timeToWaitMs = conf.SendTimeoutMs;
					}
					else
					{
						// The diff is greater than zero, set the timeout to the diff value
						timeToWaitMs = diff;
					}
				}

				sendTimeout = client.timer().newTimeout(this, timeToWaitMs, TimeUnit.MILLISECONDS);
			}
		}

		/// <summary>
		/// This fails and clears the pending messages with the given exception. This method should be called from within the
		/// ProducerImpl object mutex.
		/// </summary>
		private void failPendingMessages(ClientCnx cnx, PulsarClientException ex)
		{
			if (cnx == null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger releaseCount = new java.util.concurrent.atomic.AtomicInteger();
				AtomicInteger releaseCount = new AtomicInteger();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean batchMessagingEnabled = isBatchMessagingEnabled();
				bool batchMessagingEnabled = BatchMessagingEnabled;
				pendingMessages.forEach(op =>
				{
				releaseCount.addAndGet(batchMessagingEnabled ? op.numMessagesInBatch: 1);
				try
				{
					op.callback.sendComplete(ex);
				}
				catch (Exception t)
				{
					log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", topic, producerName, op.sequenceId, t);
				}
				ReferenceCountUtil.safeRelease(op.cmd);
				op.recycle();
				});

				pendingMessages.clear();
				pendingCallbacks.clear();
				semaphore.release(releaseCount.get());
				if (batchMessagingEnabled)
				{
					failPendingBatchMessages(ex);
				}

			}
			else
			{
				// If we have a connection, we schedule the callback and recycle on the event loop thread to avoid any
				// race condition since we also write the message on the socket from this thread
				cnx.ctx().channel().eventLoop().execute(() =>
				{
				lock (ProducerImpl.this)
				{
					failPendingMessages(null, ex);
				}
				});
			}
		}

		/// <summary>
		/// fail any pending batch messages that were enqueued, however batch was not closed out
		/// 
		/// </summary>
		private void failPendingBatchMessages(PulsarClientException ex)
		{
			if (batchMessageContainer.Empty)
			{
				return;
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int numMessagesInBatch = batchMessageContainer.getNumMessagesInBatch();
			int numMessagesInBatch = batchMessageContainer.NumMessagesInBatch;
			batchMessageContainer.discard(ex);
			semaphore.release(numMessagesInBatch);
		}

		internal TimerTask batchMessageAndSendTask = new TimerTaskAnonymousInnerClass();

		private class TimerTaskAnonymousInnerClass : TimerTask
		{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
			public override void run(Timeout timeout)
			{
				if (timeout.Cancelled)
				{
					return;
				}
				if (log.TraceEnabled)
				{
					log.trace("[{}] [{}] Batching the messages from the batch container from timer thread", outerInstance.topic, outerInstance.producerName);
				}
				// semaphore acquired when message was enqueued to container
				lock (outerInstance)
				{
					// If it's closing/closed we need to ignore the send batch timer and not schedule next timeout.
					if (outerInstance.State == State.Closing || outerInstance.State == State.Closed)
					{
						return;
					}

					outerInstance.batchMessageAndSend();
					// schedule the next batch message task
					outerInstance.batchMessageAndSendTimeout = outerInstance.client.timer().newTimeout(this, outerInstance.conf.BatchingMaxPublishDelayMicros, TimeUnit.MICROSECONDS);
				}
			}
		}

		public override CompletableFuture<Void> flushAsync()
		{
			CompletableFuture<MessageId> lastSendFuture;
			lock (ProducerImpl.this)
			{
				if (BatchMessagingEnabled)
				{
					batchMessageAndSend();
				}
				lastSendFuture = this.lastSendFuture;
			}
			return lastSendFuture.thenApply(ignored => null);
		}

		protected internal override void triggerFlush()
		{
			if (BatchMessagingEnabled)
			{
				lock (ProducerImpl.this)
				{
					batchMessageAndSend();
				}
			}
		}

		// must acquire semaphore before enqueuing
		private void batchMessageAndSend()
		{
			if (log.TraceEnabled)
			{
				log.trace("[{}] [{}] Batching the messages from the batch container with {} messages", topic, producerName, batchMessageContainer.NumMessagesInBatch);
			}
			if (!batchMessageContainer.Empty)
			{
				try
				{
					IList<OpSendMsg> opSendMsgs;
					if (batchMessageContainer.MultiBatches)
					{
						opSendMsgs = batchMessageContainer.createOpSendMsgs();
					}
					else
					{
						opSendMsgs = Collections.singletonList(batchMessageContainer.createOpSendMsg());
					}
					batchMessageContainer.clear();
					foreach (OpSendMsg opSendMsg in opSendMsgs)
					{
						processOpSendMsg(opSendMsg);
					}
				}
				catch (PulsarClientException)
				{
					Thread.CurrentThread.Interrupt();
					semaphore.release(batchMessageContainer.NumMessagesInBatch);
				}
				catch (Exception t)
				{
					semaphore.release(batchMessageContainer.NumMessagesInBatch);
					log.warn("[{}] [{}] error while create opSendMsg by batch message container", topic, producerName, t);
				}
			}
		}

		private void processOpSendMsg(OpSendMsg op)
		{
			if (op == null)
			{
				return;
			}
			try
			{
				if (op.msg != null && BatchMessagingEnabled)
				{
					batchMessageAndSend();
				}
				pendingMessages.put(op);
				if (op.msg != null)
				{
					lastSequenceIdPushed = Math.Max(lastSequenceIdPushed, getHighestSequenceId(op));
				}
				ClientCnx cnx = cnx();
				if (Connected)
				{
					if (op.msg != null && op.msg.getSchemaState() == None)
					{
						tryRegisterSchema(cnx, op.msg, op.callback);
						return;
					}
					// If we do have a connection, the message is sent immediately, otherwise we'll try again once a new
					// connection is established
					op.cmd.retain();
					cnx.ctx().channel().eventLoop().execute(WriteInEventLoopCallback.create(this, cnx, op));
					stats.updateNumMsgsSent(op.numMessagesInBatch, op.batchSizeByte);
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] Connection is not ready -- sequenceId {}", topic, producerName, op.sequenceId);
					}
				}
			}
			catch (InterruptedException ie)
			{
				Thread.CurrentThread.Interrupt();
				releaseSemaphoreForSendOp(op);
				if (op != null)
				{
					op.callback.sendComplete(new PulsarClientException(ie));
				}
			}
			catch (Exception t)
			{
				releaseSemaphoreForSendOp(op);
				log.warn("[{}] [{}] error while closing out batch -- {}", topic, producerName, t);
				if (op != null)
				{
					op.callback.sendComplete(new PulsarClientException(t));
				}
			}
		}

		private void recoverProcessOpSendMsgFrom(ClientCnx cnx, MessageImpl from)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean stripChecksum = cnx.getRemoteEndpointProtocolVersion() < brokerChecksumSupportedVersion();
			bool stripChecksum = cnx.RemoteEndpointProtocolVersion < brokerChecksumSupportedVersion();
			IEnumerator<OpSendMsg> msgIterator = pendingMessages.GetEnumerator();
			OpSendMsg pendingRegisteringOp = null;
			while (msgIterator.MoveNext())
			{
				OpSendMsg op = msgIterator.Current;
				if (from != null)
				{
					if (op.msg == from)
					{
						from = null;
					}
					else
					{
						continue;
					}
				}
				if (op.msg != null)
				{
					if (op.msg.getSchemaState() == None)
					{
						if (!rePopulateMessageSchema(op.msg))
						{
							pendingRegisteringOp = op;
							break;
						}
					}
					else if (op.msg.getSchemaState() == Broken)
					{
						op.recycle();
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						msgIterator.remove();
						continue;
					}
				}
				if (op.cmd == null)
				{
					checkState(op.rePopulate != null);
					op.rePopulate.run();
				}
				if (stripChecksum)
				{
					stripChecksum(op);
				}
				op.cmd.retain();
				if (log.DebugEnabled)
				{
					log.debug("[{}] [{}] Re-Sending message in cnx {}, sequenceId {}", topic, producerName, cnx.channel(), op.sequenceId);
				}
				cnx.ctx().write(op.cmd, cnx.ctx().voidPromise());
				stats.updateNumMsgsSent(op.numMessagesInBatch, op.batchSizeByte);
			}
			cnx.ctx().flush();
			if (!changeToReadyState())
			{
				// Producer was closed while reconnecting, close the connection to make sure the broker
				// drops the producer on its side
				cnx.channel().close();
				return;
			}
			if (pendingRegisteringOp != null)
			{
				tryRegisterSchema(cnx, pendingRegisteringOp.msg, pendingRegisteringOp.callback);
			}
		}

		public virtual long DelayInMillis
		{
			get
			{
				OpSendMsg firstMsg = pendingMessages.peek();
				if (firstMsg != null)
				{
					return DateTimeHelper.CurrentUnixTimeMillis() - firstMsg.createdAt;
				}
				return 0L;
			}
		}

		public virtual string ConnectionId
		{
			get
			{
				return cnx() != null ? connectionId : null;
			}
		}

		public virtual string ConnectedSince
		{
			get
			{
				return cnx() != null ? connectedSince : null;
			}
		}

		public virtual int PendingQueueSize
		{
			get
			{
				return pendingMessages.size();
			}
		}

		public override ProducerStatsRecorder Stats
		{
			get
			{
				return stats;
			}
		}

		public virtual string ProducerName
		{
			get
			{
				return producerName;
			}
		}

		// wrapper for connection methods
		internal virtual ClientCnx cnx()
		{
			return this.connectionHandler.cnx();
		}

		internal virtual void resetBackoff()
		{
			this.connectionHandler.resetBackoff();
		}

		internal virtual void connectionClosed(ClientCnx cnx)
		{
			this.connectionHandler.connectionClosed(cnx);
		}

		internal virtual ClientCnx ClientCnx
		{
			get
			{
				return this.connectionHandler.ClientCnx;
			}
			set
			{
				this.connectionHandler.ClientCnx = value;
			}
		}


		internal virtual void reconnectLater(Exception exception)
		{
			this.connectionHandler.reconnectLater(exception);
		}

		internal virtual void grabCnx()
		{
			this.connectionHandler.grabCnx();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting Semaphore getSemaphore()
		internal virtual Semaphore Semaphore
		{
			get
			{
				return semaphore;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ProducerImpl));
	}

}