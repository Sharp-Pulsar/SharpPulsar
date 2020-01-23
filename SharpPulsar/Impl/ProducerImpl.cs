﻿using SharpPulsar.Impl;

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
namespace SharpPulsar.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.scurrilous.circe.checksum.Crc32cIntChecksum.computeChecksum;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.scurrilous.circe.checksum.Crc32cIntChecksum.resumeChecksum;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static SharpPulsar.impl.MessageImpl.SchemaState.Broken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static SharpPulsar.impl.MessageImpl.SchemaState.None;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static SharpPulsar.impl.ProducerBase.MultiSchemaMode.Auto;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static SharpPulsar.impl.ProducerBase.MultiSchemaMode.Enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.common.protocol.Commands.hasChecksum;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.common.protocol.Commands.readChecksum;

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
	using BatcherBuilder = SharpPulsar.Api.BatcherBuilder;
	using CompressionType = SharpPulsar.Api.CompressionType;
	using SharpPulsar.Api;
	using MessageId = SharpPulsar.Api.MessageId;
	using Producer = SharpPulsar.Api.Producer;
	using ProducerCryptoFailureAction = SharpPulsar.Api.ProducerCryptoFailureAction;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using CryptoException = SharpPulsar.Api.PulsarClientException.CryptoException;
	using SharpPulsar.Api;
	using ProducerConfigurationData = SharpPulsar.Impl.Conf.ProducerConfigurationData;
	using SharpPulsar.Impl.Schema;
	using MessageMetadata = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.MessageMetadata;
	using ProtocolVersion = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.ProtocolVersion;
	using CompressionCodec = Org.Apache.Pulsar.Common.Compression.CompressionCodec;
	using CompressionCodecProvider = Org.Apache.Pulsar.Common.Compression.CompressionCodecProvider;
	using TopicName = Org.Apache.Pulsar.Common.Naming.TopicName;
	using ByteBufPair = Org.Apache.Pulsar.Common.Protocol.ByteBufPair;
	using Commands = Org.Apache.Pulsar.Common.Protocol.Commands;
	using ChecksumType = Org.Apache.Pulsar.Common.Protocol.Commands.ChecksumType;
	using SchemaHash = Org.Apache.Pulsar.Common.Protocol.Schema.SchemaHash;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;
	using DateFormatter = Org.Apache.Pulsar.Common.Util.DateFormatter;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;
	using ByteString = Org.Apache.Pulsar.shaded.com.google.protobuf.v241.ByteString;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class ProducerImpl<T> : ProducerBase<T>, TimerTask, ConnectionHandler.Connection
	{

		// Producer id, used to identify a producer within a single connection
		protected internal readonly long ProducerId;

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
		internal virtual HandlerName {get;}
		private bool userProvidedProducerName = false;

		private string connectionId;
		private string connectedSince;
		private readonly int partitionIndex;

		public virtual Stats {get;}

		private readonly CompressionCodec compressor;

		public virtual LastSequenceId {get;}
		protected internal volatile long LastSequenceIdPushed;

		private MessageCrypto msgCrypto = null;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private io.netty.util.concurrent.ScheduledFuture<?> keyGeneratorTask = null;
		private ScheduledFuture<object> keyGeneratorTask = null;

		private readonly IDictionary<string, string> metadata;

		private Optional<sbyte[]> schemaVersion = null;

		public virtual ConnectionHandler {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") private static final java.util.concurrent.atomic.AtomicLongFieldUpdater<ProducerImpl> msgIdGeneratorUpdater = java.util.concurrent.atomic.AtomicLongFieldUpdater.newUpdater(ProducerImpl.class, "msgIdGenerator");
		private static readonly AtomicLongFieldUpdater<ProducerImpl> msgIdGeneratorUpdater = AtomicLongFieldUpdater.newUpdater(typeof(ProducerImpl), "msgIdGenerator");

		public ProducerImpl(PulsarClientImpl Client, string Topic, ProducerConfigurationData Conf, CompletableFuture<Producer<T>> ProducerCreatedFuture, int PartitionIndex, Schema<T> Schema, ProducerInterceptors Interceptors) : base(Client, Topic, Conf, ProducerCreatedFuture, Schema, Interceptors)
		{
			this.ProducerId = Client.newProducerId();
			this.HandlerName = Conf.ProducerName;
			if (StringUtils.isNotBlank(HandlerName))
			{
				this.userProvidedProducerName = true;
			}
			this.partitionIndex = PartitionIndex;
			this.pendingMessages = Queues.newArrayBlockingQueue(Conf.MaxPendingMessages);
			this.pendingCallbacks = Queues.newArrayBlockingQueue(Conf.MaxPendingMessages);
			this.semaphore = new Semaphore(Conf.MaxPendingMessages, true);

			this.compressor = CompressionCodecProvider.getCompressionCodec(Conf.CompressionType);

			if (Conf.InitialSequenceId != null)
			{
				long InitialSequenceId = Conf.InitialSequenceId;
				this.LastSequenceId = InitialSequenceId;
				this.LastSequenceIdPushed = InitialSequenceId;
				this.msgIdGenerator = InitialSequenceId + 1L;
			}
			else
			{
				this.LastSequenceId = -1L;
				this.LastSequenceIdPushed = -1L;
				this.msgIdGenerator = 0L;
			}

			if (Conf.EncryptionEnabled)
			{
				string LogCtx = "[" + Topic + "] [" + HandlerName + "] [" + ProducerId + "]";
				this.msgCrypto = new MessageCrypto(LogCtx, true);

				// Regenerate data key cipher at fixed interval
				keyGeneratorTask = Client.eventLoopGroup().scheduleWithFixedDelay(() =>
				{
				try
				{
					msgCrypto.addPublicKeyCipher(Conf.EncryptionKeys, Conf.CryptoKeyReader);
				}
				catch (CryptoException E)
				{
					if (!ProducerCreatedFuture.Done)
					{
						log.warn("[{}] [{}] [{}] Failed to add public key cipher.", Topic, HandlerName, ProducerId);
						ProducerCreatedFuture.completeExceptionally(PulsarClientException.wrap(E, string.Format("The producer {0} of the topic {1} " + "adds the public key cipher was failed", HandlerName, Topic)));
					}
				}
				}, 0L, 4L, BAMCIS.Util.Concurrent.TimeUnit.HOURS);

			}

			if (Conf.SendTimeoutMs > 0)
			{
				sendTimeout = Client.timer().newTimeout(this, Conf.SendTimeoutMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
			}

			this.createProducerTimeout = DateTimeHelper.CurrentUnixTimeMillis() + Client.Configuration.OperationTimeoutMs;
			if (Conf.BatchingEnabled)
			{
				BatcherBuilder ContainerBuilder = Conf.BatcherBuilder;
				if (ContainerBuilder == null)
				{
					ContainerBuilder = BatcherBuilderFields.DEFAULT;
				}
				this.batchMessageContainer = (BatchMessageContainerBase)ContainerBuilder.build();
				this.batchMessageContainer.Producer = this;
			}
			else
			{
				this.batchMessageContainer = null;
			}
			if (Client.Configuration.StatsIntervalSeconds > 0)
			{
				Stats = new ProducerStatsRecorderImpl(Client, Conf, this);
			}
			else
			{
				Stats = ProducerStatsDisabled.INSTANCE;
			}

			if (Conf.Properties.Empty)
			{
				metadata = Collections.emptyMap();
			}
			else
			{
				metadata = Collections.unmodifiableMap(new Dictionary<>(Conf.Properties));
			}

			this.ConnectionHandler = new ConnectionHandler(this, new BackoffBuilder()
					.setInitialTime(Client.Configuration.InitialBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).setMax(Client.Configuration.MaxBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).setMandatoryStop(Math.Max(100, Conf.SendTimeoutMs - 100), BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).create(), this);

			GrabCnx();
		}


		private bool BatchMessagingEnabled
		{
			get
			{
				return Conf.BatchingEnabled;
			}
		}

		private bool IsMultiSchemaEnabled(bool AutoEnable)
		{
			if (MultiSchemaMode != Auto)
			{
				return MultiSchemaMode == Enabled;
			}
			if (AutoEnable)
			{
				MultiSchemaMode = Enabled;
				return true;
			}
			return false;
		}


		public override CompletableFuture<MessageId> InternalSendAsync<T1>(Message<T1> Message)
		{

			CompletableFuture<MessageId> Future = new CompletableFuture<MessageId>();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> interceptorMessage = (MessageImpl) beforeSend(message);
			MessageImpl<object> InterceptorMessage = (MessageImpl) BeforeSend(Message);
			//Retain the buffer used by interceptors callback to get message. Buffer will release after complete interceptors.
			InterceptorMessage.DataBuffer.retain();
			if (Interceptors != null)
			{
				InterceptorMessage.Properties;
			}
			SendAsync(InterceptorMessage, new SendCallbackAnonymousInnerClass(this, Future, InterceptorMessage));
			return Future;
		}

		public class SendCallbackAnonymousInnerClass : SendCallback
		{
			private readonly ProducerImpl<T> outerInstance;

			private CompletableFuture<MessageId> future;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private SharpPulsar.impl.MessageImpl<JavaToDotNetGenericWildcard> interceptorMessage;
			private MessageImpl<object> interceptorMessage;

			public SendCallbackAnonymousInnerClass<T1>(ProducerImpl<T> OuterInstance, CompletableFuture<MessageId> Future, MessageImpl<T1> InterceptorMessage)
			{
				this.outerInstance = OuterInstance;
				this.future = Future;
				this.interceptorMessage = InterceptorMessage;
				nextCallback = null;
				nextMsg = null;
				createdAt = System.nanoTime();
			}

			internal SendCallback nextCallback;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> nextMsg;
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

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public MessageImpl<?> getNextMessage()
			public MessageImpl<object> NextMessage
			{
				get
				{
					return nextMsg;
				}
			}

			public void sendComplete(Exception E)
			{
				try
				{
					if (E != null)
					{
						outerInstance.Stats.IncrementSendFailed();
						outerInstance.OnSendAcknowledgement(interceptorMessage, null, E);
						future.completeExceptionally(E);
					}
					else
					{
						outerInstance.OnSendAcknowledgement(interceptorMessage, interceptorMessage.getMessageId(), null);
						future.complete(interceptorMessage.getMessageId());
						outerInstance.Stats.IncrementNumAcksReceived(System.nanoTime() - createdAt);
					}
				}
				finally
				{
					interceptorMessage.DataBuffer.release();
				}

				while (nextCallback != null)
				{
					SendCallback SendCallback = nextCallback;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> msg = nextMsg;
					MessageImpl<object> Msg = nextMsg;
					//Retain the buffer used by interceptors callback to get message. Buffer will release after complete interceptors.
					try
					{
						Msg.DataBuffer.retain();
						if (E != null)
						{
							outerInstance.Stats.IncrementSendFailed();
							outerInstance.OnSendAcknowledgement(Msg, null, E);
							SendCallback.Future.completeExceptionally(E);
						}
						else
						{
							outerInstance.OnSendAcknowledgement(Msg, Msg.getMessageId(), null);
							SendCallback.Future.complete(Msg.getMessageId());
							outerInstance.Stats.IncrementNumAcksReceived(System.nanoTime() - createdAt);
						}
						nextMsg = nextCallback.NextMessage;
						nextCallback = nextCallback.NextSendCallback;
					}
					finally
					{
						Msg.DataBuffer.release();
					}
				}
			}

			public void addCallback<T1>(MessageImpl<T1> Msg, SendCallback Scb)
			{
				nextMsg = Msg;
				nextCallback = Scb;
			}
		}

		public virtual void SendAsync<T1>(Message<T1> Message, SendCallback Callback)
		{
			checkArgument(Message is MessageImpl);

			if (!IsValidProducerState(Callback))
			{
				return;
			}

			if (!CanEnqueueRequest(Callback))
			{
				return;
			}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> msg = (MessageImpl) message;
			MessageImpl<object> Msg = (MessageImpl) Message;
			MessageMetadata.Builder MsgMetadataBuilder = Msg.MessageBuilder;
			ByteBuf Payload = Msg.DataBuffer;

			// If compression is enabled, we are compressing, otherwise it will simply use the same buffer
			int UncompressedSize = Payload.readableBytes();
			ByteBuf CompressedPayload = Payload;
			// Batch will be compressed when closed
			// If a message has a delayed delivery time, we'll always send it individually
			if (!BatchMessagingEnabled || MsgMetadataBuilder.hasDeliverAtTime())
			{
				CompressedPayload = compressor.Encode(Payload);
				Payload.release();

				// validate msg-size (For batching this will be check at the batch completion size)
				int CompressedSize = CompressedPayload.readableBytes();
				if (CompressedSize > ClientCnx.MaxMessageSize)
				{
					CompressedPayload.release();
					string CompressedStr = (!BatchMessagingEnabled && Conf.CompressionType != CompressionType.NONE) ? "Compressed" : "";
					PulsarClientException.InvalidMessageException InvalidMessageException = new PulsarClientException.InvalidMessageException(format("The producer %s of the topic %s sends a %s message with %d bytes that exceeds %d bytes", HandlerName, Topic, CompressedStr, CompressedSize, ClientCnx.MaxMessageSize));
					Callback.sendComplete(InvalidMessageException);
					return;
				}
			}

			if (!Msg.Replicated && MsgMetadataBuilder.hasProducerName())
			{
				PulsarClientException.InvalidMessageException InvalidMessageException = new PulsarClientException.InvalidMessageException(format("The producer %s of the topic %s can not reuse the same message", HandlerName, Topic));
				Callback.sendComplete(InvalidMessageException);
				CompressedPayload.release();
				return;
			}

			if (!PopulateMessageSchema(Msg, Callback))
			{
				CompressedPayload.release();
				return;
			}

			try
			{
				lock (this)
				{
					long SequenceId;
					if (!MsgMetadataBuilder.hasSequenceId())
					{
						SequenceId = msgIdGeneratorUpdater.getAndIncrement(this);
						MsgMetadataBuilder.SequenceId = SequenceId;
					}
					else
					{
						SequenceId = MsgMetadataBuilder.SequenceId;
					}
					if (!MsgMetadataBuilder.hasPublishTime())
					{
						MsgMetadataBuilder.PublishTime = ClientConflict.ClientClock.millis();

						checkArgument(!MsgMetadataBuilder.hasProducerName());

						MsgMetadataBuilder.setProducerName(HandlerName);

						if (Conf.CompressionType != CompressionType.NONE)
						{
							MsgMetadataBuilder.Compression = CompressionCodecProvider.convertToWireProtocol(Conf.CompressionType);
						}
						MsgMetadataBuilder.UncompressedSize = UncompressedSize;
					}
					if (CanAddToBatch(Msg))
					{
						if (CanAddToCurrentBatch(Msg))
						{
							// should trigger complete the batch message, new message will add to a new batch and new batch
							// sequence id use the new message, so that broker can handle the message duplication
							if (SequenceId <= LastSequenceIdPushed)
							{
								if (SequenceId <= LastSequenceId)
								{
									log.warn("Message with sequence id {} is definitely a duplicate", SequenceId);
								}
								else
								{
									log.info("Message with sequence id {} might be a duplicate but cannot be determined at this time.", SequenceId);
								}
								DoBatchSendAndAdd(Msg, Callback, Payload);
							}
							else
							{
								// handle boundary cases where message being added would exceed
								// batch size and/or max message size
								bool IsBatchFull = batchMessageContainer.Add(Msg, Callback);
								lastSendFuture = Callback.Future;
								Payload.release();
								if (IsBatchFull)
								{
									BatchMessageAndSend();
								}
							}
						}
						else
						{
							DoBatchSendAndAdd(Msg, Callback, Payload);
						}
					}
					else
					{
						ByteBuf EncryptedPayload = EncryptMessage(MsgMetadataBuilder, CompressedPayload);
						// When publishing during replication, we need to set the correct number of message in batch
						// This is only used in tracking the publish rate stats
						int NumMessages = Msg.MessageBuilder.hasNumMessagesInBatch() ? Msg.MessageBuilder.NumMessagesInBatch : 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final OpSendMsg op;
						OpSendMsg Op;
						if (Msg.getSchemaState() == MessageImpl.SchemaState.Ready)
						{
							MessageMetadata MsgMetadata = MsgMetadataBuilder.build();
							ByteBufPair Cmd = SendMessage(ProducerId, SequenceId, NumMessages, MsgMetadata, EncryptedPayload);
							Op = OpSendMsg.Create(Msg, Cmd, SequenceId, Callback);
							MsgMetadataBuilder.recycle();
							MsgMetadata.recycle();
						}
						else
						{
							Op = OpSendMsg.Create(Msg, null, SequenceId, Callback);
							Op.RePopulate = () =>
							{
							MessageMetadata MsgMetadata = MsgMetadataBuilder.build();
							Op.Cmd = SendMessage(ProducerId, SequenceId, NumMessages, MsgMetadata, EncryptedPayload);
							MsgMetadataBuilder.recycle();
							MsgMetadata.recycle();
							};
						}
						Op.NumMessagesInBatch = NumMessages;
						Op.BatchSizeByte = EncryptedPayload.readableBytes();
						lastSendFuture = Callback.Future;
						ProcessOpSendMsg(Op);
					}
				}
			}
			catch (PulsarClientException E)
			{
				semaphore.release();
				Callback.sendComplete(E);
			}
			catch (Exception T)
			{
				semaphore.release();
				Callback.sendComplete(new PulsarClientException(T));
			}
		}

		private bool PopulateMessageSchema(MessageImpl Msg, SendCallback Callback)
		{
			MessageMetadata.Builder MsgMetadataBuilder = Msg.MessageBuilder;
			if (Msg.Schema == Schema)
			{
				schemaVersion.ifPresent(v => MsgMetadataBuilder.setSchemaVersion(ByteString.copyFrom(v)));
				Msg.setSchemaState(MessageImpl.SchemaState.Ready);
				return true;
			}
			if (!IsMultiSchemaEnabled(true))
			{
				Callback.sendComplete(new PulsarClientException.InvalidMessageException(format("The producer %s of the topic %s is disabled the `MultiSchema`", HandlerName, Topic)));
				return false;
			}
			SchemaHash SchemaHash = SchemaHash.of(Msg.Schema);
			sbyte[] SchemaVersion = SchemaCache.get(SchemaHash);
			if (SchemaVersion != null)
			{
				MsgMetadataBuilder.SchemaVersion = ByteString.copyFrom(SchemaVersion);
				Msg.setSchemaState(MessageImpl.SchemaState.Ready);
			}
			return true;
		}

		private bool RePopulateMessageSchema(MessageImpl Msg)
		{
			SchemaHash SchemaHash = SchemaHash.of(Msg.Schema);
			sbyte[] SchemaVersion = SchemaCache.get(SchemaHash);
			if (SchemaVersion == null)
			{
				return false;
			}
			Msg.MessageBuilder.SchemaVersion = ByteString.copyFrom(SchemaVersion);
			Msg.setSchemaState(MessageImpl.SchemaState.Ready);
			return true;
		}

		private void TryRegisterSchema(ClientCnx Cnx, MessageImpl Msg, SendCallback Callback)
		{
			if (!ChangeToRegisteringSchemaState())
			{
				return;
			}
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			SchemaInfo SchemaInfo = Optional.ofNullable(Msg.Schema).map(Schema::getSchemaInfo).filter(si => si.Type.Value > 0).orElse(SchemaFields.BYTES.SchemaInfo);
			GetOrCreateSchemaAsync(Cnx, SchemaInfo).handle((v, ex) =>
			{
			if (ex != null)
			{
				Exception T = FutureUtil.unwrapCompletionException(ex);
				log.warn("[{}] [{}] GetOrCreateSchema error", Topic, HandlerName, T);
				if (T is PulsarClientException.IncompatibleSchemaException)
				{
					Msg.setSchemaState(MessageImpl.SchemaState.Broken);
					Callback.sendComplete((PulsarClientException.IncompatibleSchemaException) T);
				}
			}
			else
			{
				log.warn("[{}] [{}] GetOrCreateSchema succeed", Topic, HandlerName);
				SchemaHash SchemaHash = SchemaHash.of(Msg.Schema);
				SchemaCache.putIfAbsent(SchemaHash, v);
				Msg.MessageBuilder.SchemaVersion = ByteString.copyFrom(v);
				Msg.setSchemaState(MessageImpl.SchemaState.Ready);
			}
			Cnx.ctx().channel().eventLoop().execute(() =>
			{
				lock (ProducerImpl.this)
				{
					RecoverProcessOpSendMsgFrom(Cnx, Msg);
				}
			});
			return null;
			});
		}

		private CompletableFuture<sbyte[]> GetOrCreateSchemaAsync(ClientCnx Cnx, SchemaInfo SchemaInfo)
		{
			if (!Commands.peerSupportsGetOrCreateSchema(Cnx.RemoteEndpointProtocolVersion))
			{
				return FutureUtil.failedFuture(new PulsarClientException.NotSupportedException(format("The command `GetOrCreateSchema` is not supported for the protocol version %d. " + "The producer is %s, topic is %s", Cnx.RemoteEndpointProtocolVersion, HandlerName, Topic)));
			}
			long RequestId = ClientConflict.newRequestId();
			ByteBuf Request = Commands.newGetOrCreateSchema(RequestId, Topic, SchemaInfo);
			log.info("[{}] [{}] GetOrCreateSchema request", Topic, HandlerName);
			return Cnx.sendGetOrCreateSchema(Request, RequestId);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected io.netty.buffer.ByteBuf encryptMessage(org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata.Builder msgMetadata, io.netty.buffer.ByteBuf compressedPayload) throws SharpPulsar.api.PulsarClientException
		public virtual ByteBuf EncryptMessage(MessageMetadata.Builder MsgMetadata, ByteBuf CompressedPayload)
		{

			ByteBuf EncryptedPayload = CompressedPayload;
			if (!Conf.EncryptionEnabled || msgCrypto == null)
			{
				return EncryptedPayload;
			}
			try
			{
				EncryptedPayload = msgCrypto.Encrypt(Conf.EncryptionKeys, Conf.CryptoKeyReader, MsgMetadata, CompressedPayload);
			}
			catch (PulsarClientException E)
			{
				// Unless config is set to explicitly publish un-encrypted message upon failure, fail the request
				if (Conf.CryptoFailureAction == ProducerCryptoFailureAction.SEND)
				{
					log.warn("[{}] [{}] Failed to encrypt message {}. Proceeding with publishing unencrypted message", Topic, HandlerName, E.Message);
					return CompressedPayload;
				}
				throw E;
			}
			return EncryptedPayload;
		}

		public virtual ByteBufPair SendMessage(long ProducerId, long SequenceId, int NumMessages, MessageMetadata MsgMetadata, ByteBuf CompressedPayload)
		{
			return Commands.newSend(ProducerId, SequenceId, NumMessages, ChecksumType, MsgMetadata, CompressedPayload);
		}

		public virtual ByteBufPair SendMessage(long ProducerId, long LowestSequenceId, long HighestSequenceId, int NumMessages, MessageMetadata MsgMetadata, ByteBuf CompressedPayload)
		{
			return Commands.newSend(ProducerId, LowestSequenceId, HighestSequenceId, NumMessages, ChecksumType, MsgMetadata, CompressedPayload);
		}

		private Commands.ChecksumType? ChecksumType
		{
			get
			{
				if (ConnectionHandler.ClientCnx == null || ConnectionHandler.ClientCnx.RemoteEndpointProtocolVersion >= BrokerChecksumSupportedVersion())
				{
					return Commands.ChecksumType.Crc32c;
				}
				else
				{
					return Commands.ChecksumType.None;
				}
			}
		}

		private bool CanAddToBatch<T1>(MessageImpl<T1> Msg)
		{
			return Msg.getSchemaState() == MessageImpl.SchemaState.Ready && BatchMessagingEnabled && !Msg.MessageBuilder.hasDeliverAtTime();
		}

		private bool CanAddToCurrentBatch<T1>(MessageImpl<T1> Msg)
		{
			return batchMessageContainer.HaveEnoughSpace(Msg) && (!IsMultiSchemaEnabled(false) || batchMessageContainer.HasSameSchema(Msg));
		}

		private void DoBatchSendAndAdd<T1>(MessageImpl<T1> Msg, SendCallback Callback, ByteBuf Payload)
		{
			if (log.DebugEnabled)
			{
				log.debug("[{}] [{}] Closing out batch to accommodate large message with size {}", Topic, HandlerName, Msg.DataBuffer.readableBytes());
			}
			try
			{
				BatchMessageAndSend();
				batchMessageContainer.Add(Msg, Callback);
				lastSendFuture = Callback.Future;
			}
			finally
			{
				Payload.release();
			}
		}

		private bool IsValidProducerState(SendCallback Callback)
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
				Callback.sendComplete(new PulsarClientException.AlreadyClosedException("Producer already closed"));
				return false;
			case Terminated:
				Callback.sendComplete(new PulsarClientException.TopicTerminatedException("Topic was terminated"));
				return false;
			case Failed:
			case Uninitialized:
			default:
				Callback.sendComplete(new PulsarClientException.NotConnectedException());
				return false;
			}
		}

		private bool CanEnqueueRequest(SendCallback Callback)
		{
			try
			{
				if (Conf.BlockIfQueueFull)
				{
					semaphore.acquire();
				}
				else
				{
					if (!semaphore.tryAcquire())
					{
						Callback.sendComplete(new PulsarClientException.ProducerQueueIsFullError("Producer send queue is full"));
						return false;
					}
				}
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				Callback.sendComplete(new PulsarClientException(E));
				return false;
			}

			return true;
		}

		public sealed class WriteInEventLoopCallback : ThreadStart
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private ProducerImpl<?> producer;
			internal ProducerImpl<object> Producer;
			internal ByteBufPair Cmd;
			internal long SequenceId;
			internal ClientCnx Cnx;

			internal static WriteInEventLoopCallback Create<T1>(ProducerImpl<T1> Producer, ClientCnx Cnx, OpSendMsg Op)
			{
				WriteInEventLoopCallback C = RECYCLER.get();
				C.Producer = Producer;
				C.Cnx = Cnx;
				C.SequenceId = Op.SequenceId;
				C.Cmd = Op.Cmd;
				return C;
			}

			public override void Run()
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] [{}] Sending message cnx {}, sequenceId {}", Producer.topic, Producer.HandlerName, Cnx, SequenceId);
				}

				try
				{
					Cnx.ctx().writeAndFlush(Cmd, Cnx.ctx().voidPromise());
				}
				finally
				{
					Recycle();
				}
			}

			public void Recycle()
			{
				Producer = null;
				Cnx = null;
				Cmd = null;
				SequenceId = -1;
				RecyclerHandle.recycle(this);
			}

			internal readonly Recycler.Handle<WriteInEventLoopCallback> RecyclerHandle;

			public WriteInEventLoopCallback(Recycler.Handle<WriteInEventLoopCallback> RecyclerHandle)
			{
				this.RecyclerHandle = RecyclerHandle;
			}

			internal static readonly Recycler<WriteInEventLoopCallback> RECYCLER = new RecyclerAnonymousInnerClass();

			public class RecyclerAnonymousInnerClass : Recycler<WriteInEventLoopCallback>
			{
				public override WriteInEventLoopCallback newObject(Recycler.Handle<WriteInEventLoopCallback> Handle)
				{
					return new WriteInEventLoopCallback(Handle);
				}
			}
		}

		public override CompletableFuture<Void> CloseAsync()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final State currentState = getAndUpdateState(state ->
			State CurrentState = GetAndUpdateState(state =>
			{
			if (state == State.Closed)
			{
				return state;
			}
			return State.Closing;
			});

			if (CurrentState == State.Closed || CurrentState == State.Closing)
			{
				return CompletableFuture.completedFuture(null);
			}

			Timeout Timeout = sendTimeout;
			if (Timeout != null)
			{
				Timeout.cancel();
				sendTimeout = null;
			}

			Timeout BatchTimeout = batchMessageAndSendTimeout;
			if (BatchTimeout != null)
			{
				BatchTimeout.cancel();
				batchMessageAndSendTimeout = null;
			}

			if (keyGeneratorTask != null && !keyGeneratorTask.Cancelled)
			{
				keyGeneratorTask.cancel(false);
			}

			Stats.CancelStatsTimeout();

			ClientCnx Cnx = cnx();
			if (Cnx == null || CurrentState != State.Ready)
			{
				log.info("[{}] [{}] Closed Producer (not connected)", Topic, HandlerName);
				lock (this)
				{
					State = State.Closed;
					ClientConflict.cleanupProducer(this);
					PulsarClientException Ex = new PulsarClientException.AlreadyClosedException(format("The producer %s of the topic %s was already closed when closing the producers", HandlerName, Topic));
					pendingMessages.forEach(msg =>
					{
					msg.callback.sendComplete(Ex);
					msg.cmd.release();
					msg.recycle();
					});
					pendingMessages.clear();
				}

				return CompletableFuture.completedFuture(null);
			}

			long RequestId = ClientConflict.newRequestId();
			ByteBuf Cmd = Commands.newCloseProducer(ProducerId, RequestId);

			CompletableFuture<Void> CloseFuture = new CompletableFuture<Void>();
			Cnx.sendRequestWithId(Cmd, RequestId).handle((v, exception) =>
			{
			Cnx.removeProducer(ProducerId);
			if (exception == null || !Cnx.ctx().channel().Active)
			{
				lock (ProducerImpl.this)
				{
					log.info("[{}] [{}] Closed Producer", Topic, HandlerName);
					State = State.Closed;
					pendingMessages.forEach(msg =>
					{
						msg.cmd.release();
						msg.recycle();
					});
					pendingMessages.clear();
				}
				CloseFuture.complete(null);
				ClientConflict.cleanupProducer(this);
			}
			else
			{
				CloseFuture.completeExceptionally(exception);
			}
			return null;
			});

			return CloseFuture;
		}

		public override bool Connected
		{
			get
			{
				return ConnectionHandler.ClientCnx != null && (State == State.Ready);
			}
		}

		public virtual bool Writable
		{
			get
			{
				ClientCnx Cnx = ConnectionHandler.ClientCnx;
				return Cnx != null && Cnx.channel().Writable;
			}
		}

		public virtual void Terminated(ClientCnx Cnx)
		{
			State PreviousState = GetAndUpdateState(state => (state == State.Closed ? State.Closed : State.Terminated));
			if (PreviousState != State.Terminated && PreviousState != State.Closed)
			{
				log.info("[{}] [{}] The topic has been terminated", Topic, HandlerName);
				ClientCnx = null;

				FailPendingMessages(Cnx, new PulsarClientException.TopicTerminatedException(format("The topic %s that the producer %s produces to has been terminated", Topic, HandlerName)));
			}
		}

		public virtual void AckReceived(ClientCnx Cnx, long SequenceId, long HighestSequenceId, long LedgerId, long EntryId)
		{
			OpSendMsg Op = null;
			bool Callback = false;
			lock (this)
			{
				Op = pendingMessages.peek();
				if (Op == null)
				{
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] Got ack for timed out msg {} - {}", Topic, HandlerName, SequenceId, HighestSequenceId);
					}
					return;
				}

				if (SequenceId > Op.SequenceId)
				{
					log.warn("[{}] [{}] Got ack for msg. expecting: {} - {} - got: {} - {} - queue-size: {}", Topic, HandlerName, Op.SequenceId, Op.HighestSequenceId, SequenceId, HighestSequenceId, pendingMessages.size());
					// Force connection closing so that messages can be re-transmitted in a new connection
					Cnx.channel().close();
				}
				else if (SequenceId < Op.SequenceId)
				{
					// Ignoring the ack since it's referring to a message that has already timed out.
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] Got ack for timed out msg. expecting: {} - {} - got: {} - {}", Topic, HandlerName, Op.SequenceId, Op.HighestSequenceId, SequenceId, HighestSequenceId);
					}
				}
				else
				{
					// Add check `sequenceId >= highestSequenceId` for backward compatibility.
					if (SequenceId >= HighestSequenceId || HighestSequenceId == Op.HighestSequenceId)
					{
						// Message was persisted correctly
						if (log.DebugEnabled)
						{
							log.debug("[{}] [{}] Received ack for msg {} ", Topic, HandlerName, SequenceId);
						}
						pendingMessages.remove();
						ReleaseSemaphoreForSendOp(Op);
						Callback = true;
						pendingCallbacks.add(Op);
					}
					else
					{
						log.warn("[{}] [{}] Got ack for batch msg error. expecting: {} - {} - got: {} - {} - queue-size: {}", Topic, HandlerName, Op.SequenceId, Op.HighestSequenceId, SequenceId, HighestSequenceId, pendingMessages.size());
						// Force connection closing so that messages can be re-transmitted in a new connection
						Cnx.channel().close();
					}
				}
			}
			if (Callback)
			{
				Op = pendingCallbacks.poll();
				if (Op != null)
				{
					LastSequenceId = Math.Max(LastSequenceId, GetHighestSequenceId(Op));
					Op.setMessageId(LedgerId, EntryId, partitionIndex);
					try
					{
						// Need to protect ourselves from any exception being thrown in the future handler from the
						// application
						Op.Callback.sendComplete(null);
					}
					catch (Exception T)
					{
						log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", Topic, HandlerName, SequenceId, T);
					}
					ReferenceCountUtil.safeRelease(Op.Cmd);
					Op.recycle();
				}
			}
		}

		private long GetHighestSequenceId(OpSendMsg Op)
		{
			return Math.Max(Op.HighestSequenceId, Op.SequenceId);
		}

		private void ReleaseSemaphoreForSendOp(OpSendMsg Op)
		{
			semaphore.release(BatchMessagingEnabled ? Op.NumMessagesInBatchConflict : 1);
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
		public virtual void RecoverChecksumError(ClientCnx Cnx, long SequenceId)
		{
			lock (this)
			{
				OpSendMsg Op = pendingMessages.peek();
				if (Op == null)
				{
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] Got send failure for timed out msg {}", Topic, HandlerName, SequenceId);
					}
				}
				else
				{
					long ExpectedSequenceId = GetHighestSequenceId(Op);
					if (SequenceId == ExpectedSequenceId)
					{
						bool Corrupted = !VerifyLocalBufferIsNotCorrupted(Op);
						if (Corrupted)
						{
							// remove message from pendingMessages queue and fail callback
							pendingMessages.remove();
							ReleaseSemaphoreForSendOp(Op);
							try
							{
								Op.Callback.sendComplete(new PulsarClientException.ChecksumException(format("The checksum of the message which is produced by producer %s to the topic " + "%s is corrupted", HandlerName, Topic)));
							}
							catch (Exception T)
							{
								log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", Topic, HandlerName, SequenceId, T);
							}
							ReferenceCountUtil.safeRelease(Op.Cmd);
							Op.recycle();
							return;
						}
						else
						{
							if (log.DebugEnabled)
							{
								log.debug("[{}] [{}] Message is not corrupted, retry send-message with sequenceId {}", Topic, HandlerName, SequenceId);
							}
						}
        
					}
					else
					{
						if (log.DebugEnabled)
						{
							log.debug("[{}] [{}] Corrupt message is already timed out {}", Topic, HandlerName, SequenceId);
						}
					}
				}
				// as msg is not corrupted : let producer resend pending-messages again including checksum failed message
				ResendMessages(Cnx);
			}
		}

		/// <summary>
		/// Computes checksum again and verifies it against existing checksum. If checksum doesn't match it means that
		/// message is corrupt.
		/// </summary>
		/// <param name="op"> </param>
		/// <returns> returns true only if message is not modified and computed-checksum is same as previous checksum else
		///         return false that means that message is corrupted. Returns true if checksum is not present. </returns>
		public virtual bool VerifyLocalBufferIsNotCorrupted(OpSendMsg Op)
		{
			ByteBufPair Msg = Op.Cmd;

			if (Msg != null)
			{
				ByteBuf HeaderFrame = Msg.First;
				HeaderFrame.markReaderIndex();
				try
				{
					// skip bytes up to checksum index
					HeaderFrame.skipBytes(4); // skip [total-size]
					int CmdSize = (int) HeaderFrame.readUnsignedInt();
					HeaderFrame.skipBytes(CmdSize);
					// verify if checksum present
					if (hasChecksum(HeaderFrame))
					{
						int Checksum = readChecksum(HeaderFrame);
						// msg.readerIndex is already at header-payload index, Recompute checksum for headers-payload
						int MetadataChecksum = computeChecksum(HeaderFrame);
						long ComputedChecksum = resumeChecksum(MetadataChecksum, Msg.Second);
						return Checksum == ComputedChecksum;
					}
					else
					{
						log.warn("[{}] [{}] checksum is not present into message with id {}", Topic, HandlerName, Op.SequenceId);
					}
				}
				finally
				{
					HeaderFrame.resetReaderIndex();
				}
				return true;
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				log.warn("[{}] Failed while casting {} into ByteBufPair", HandlerName, (Op.Cmd == null ? null : Op.Cmd.GetType().FullName));
				return false;
			}
		}

		public sealed class OpSendMsg
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MessageImpl<?> msg;
			internal MessageImpl<object> Msg;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<MessageImpl<?>> msgs;
			internal IList<MessageImpl<object>> Msgs;
			internal ByteBufPair Cmd;
			internal SendCallback Callback;
			internal ThreadStart RePopulate;
			internal long SequenceId;
			internal long CreatedAt;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long BatchSizeByteConflict = 0;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal int NumMessagesInBatchConflict = 1;
			internal long HighestSequenceId;

			internal static OpSendMsg Create<T1>(MessageImpl<T1> Msg, ByteBufPair Cmd, long SequenceId, SendCallback Callback)
			{
				OpSendMsg Op = RECYCLER.get();
				Op.Msg = Msg;
				Op.Cmd = Cmd;
				Op.Callback = Callback;
				Op.SequenceId = SequenceId;
				Op.CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
				return Op;
			}

			internal static OpSendMsg Create<T1>(IList<T1> Msgs, ByteBufPair Cmd, long SequenceId, SendCallback Callback)
			{
				OpSendMsg Op = RECYCLER.get();
				Op.Msgs = Msgs;
				Op.Cmd = Cmd;
				Op.Callback = Callback;
				Op.SequenceId = SequenceId;
				Op.CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
				return Op;
			}

			internal static OpSendMsg Create<T1>(IList<T1> Msgs, ByteBufPair Cmd, long LowestSequenceId, long HighestSequenceId, SendCallback Callback)
			{
				OpSendMsg Op = RECYCLER.get();
				Op.Msgs = Msgs;
				Op.Cmd = Cmd;
				Op.Callback = Callback;
				Op.SequenceId = LowestSequenceId;
				Op.HighestSequenceId = HighestSequenceId;
				Op.CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
				return Op;
			}

			public void Recycle()
			{
				Msg = null;
				Msgs = null;
				Cmd = null;
				Callback = null;
				RePopulate = null;
				SequenceId = -1L;
				CreatedAt = -1L;
				HighestSequenceId = -1L;
				RecyclerHandle.recycle(this);
			}

			public int NumMessagesInBatch
			{
				set
				{
					this.NumMessagesInBatchConflict = value;
				}
			}

			public long BatchSizeByte
			{
				set
				{
					this.BatchSizeByteConflict = value;
				}
			}

			public void SetMessageId(long LedgerId, long EntryId, int PartitionIndex)
			{
				if (Msg != null)
				{
					Msg.setMessageId(new MessageIdImpl(LedgerId, EntryId, PartitionIndex));
				}
				else
				{
					for (int BatchIndex = 0; BatchIndex < Msgs.Count; BatchIndex++)
					{
						Msgs[BatchIndex].setMessageId(new BatchMessageIdImpl(LedgerId, EntryId, PartitionIndex, BatchIndex));
					}
				}
			}

			public OpSendMsg(Recycler.Handle<OpSendMsg> RecyclerHandle)
			{
				this.RecyclerHandle = RecyclerHandle;
			}

			internal readonly Recycler.Handle<OpSendMsg> RecyclerHandle;
			internal static readonly Recycler<OpSendMsg> RECYCLER = new RecyclerAnonymousInnerClass();

			public class RecyclerAnonymousInnerClass : Recycler<OpSendMsg>
			{
				public override OpSendMsg newObject(Recycler.Handle<OpSendMsg> Handle)
				{
					return new OpSendMsg(Handle);
				}
			}
		}

		public override void ConnectionOpened(in ClientCnx Cnx)
		{
			// we set the cnx reference before registering the producer on the cnx, so if the cnx breaks before creating the
			// producer, it will try to grab a new cnx
			ConnectionHandler.ClientCnx = Cnx;
			Cnx.registerProducer(ProducerId, this);

			log.info("[{}] [{}] Creating producer on cnx {}", Topic, HandlerName, Cnx.ctx().channel());

			long RequestId = ClientConflict.newRequestId();

			SchemaInfo SchemaInfo = null;
			if (Schema != null)
			{
				if (Schema.SchemaInfo != null)
				{
					if (Schema.SchemaInfo.Type == SchemaType.JSON)
					{
						// for backwards compatibility purposes
						// JSONSchema originally generated a schema for pojo based of of the JSON schema standard
						// but now we have standardized on every schema to generate an Avro based schema
						if (Commands.peerSupportJsonSchemaAvroFormat(Cnx.RemoteEndpointProtocolVersion))
						{
							SchemaInfo = Schema.SchemaInfo;
						}
						else if (Schema is JSONSchema)
						{
							JSONSchema JsonSchema = (JSONSchema) Schema;
							SchemaInfo = JsonSchema.BackwardsCompatibleJsonSchemaInfo;
						}
						else
						{
							SchemaInfo = Schema.SchemaInfo;
						}
					}
					else if (Schema.SchemaInfo.Type == SchemaType.BYTES || Schema.SchemaInfo.Type == SchemaType.NONE)
					{
						// don't set schema info for Schema.BYTES
						SchemaInfo = null;
					}
					else
					{
						SchemaInfo = Schema.SchemaInfo;
					}
				}
			}

			Cnx.sendRequestWithId(Commands.newProducer(Topic, ProducerId, RequestId, HandlerName, Conf.EncryptionEnabled, metadata, SchemaInfo, ConnectionHandler.EpochConflict, userProvidedProducerName), RequestId).thenAccept(response =>
			{
			string ProducerName = response.ProducerName;
			long LastSequenceId = response.LastSequenceId;
			schemaVersion = Optional.ofNullable(response.SchemaVersion);
			schemaVersion.ifPresent(v => SchemaCache.put(SchemaHash.of(Schema), v));
			lock (ProducerImpl.this)
			{
				if (State == State.Closing || State == State.Closed)
				{
					Cnx.removeProducer(ProducerId);
					Cnx.channel().close();
					return;
				}
				ResetBackoff();
				log.info("[{}] [{}] Created producer on cnx {}", Topic, ProducerName, Cnx.ctx().channel());
				connectionId = Cnx.ctx().channel().ToString();
				connectedSince = DateFormatter.now();
				if (string.ReferenceEquals(this.HandlerName, null))
				{
					this.HandlerName = ProducerName;
				}
				if (this.msgIdGenerator == 0 && Conf.InitialSequenceId == null)
				{
					this.LastSequenceId = LastSequenceId;
					this.msgIdGenerator = LastSequenceId + 1;
				}
				if (!ProducerCreatedFutureConflict.Done && BatchMessagingEnabled)
				{
					ClientConflict.timer().newTimeout(batchMessageAndSendTask, Conf.BatchingMaxPublishDelayMicros, BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS);
				}
				ResendMessages(Cnx);
			}
			}).exceptionally((e) =>
			{
			Exception Cause = e.Cause;
			Cnx.removeProducer(ProducerId);
			if (State == State.Closing || State == State.Closed)
			{
				Cnx.channel().close();
				return null;
			}
			log.error("[{}] [{}] Failed to create producer: {}", Topic, HandlerName, Cause.Message);
			if (Cause is PulsarClientException.ProducerBlockedQuotaExceededException)
			{
				lock (this)
				{
					log.warn("[{}] [{}] Topic backlog quota exceeded. Throwing Exception on producer.", Topic, HandlerName);
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] Pending messages: {}", Topic, HandlerName, pendingMessages.size());
					}
					PulsarClientException Bqe = new PulsarClientException.ProducerBlockedQuotaExceededException(format("The backlog quota of the topic %s that the producer %s produces to is exceeded", Topic, HandlerName));
					FailPendingMessages(cnx(), Bqe);
				}
			}
			else if (Cause is PulsarClientException.ProducerBlockedQuotaExceededError)
			{
				log.warn("[{}] [{}] Producer is blocked on creation because backlog exceeded on topic.", HandlerName, Topic);
			}
			if (Cause is PulsarClientException.TopicTerminatedException)
			{
				State = State.Terminated;
				FailPendingMessages(cnx(), (PulsarClientException) Cause);
				ProducerCreatedFutureConflict.completeExceptionally(Cause);
				ClientConflict.cleanupProducer(this);
			}
			else if (ProducerCreatedFutureConflict.Done || (Cause is PulsarClientException && ConnectionHandler.IsRetriableError((PulsarClientException) Cause) && DateTimeHelper.CurrentUnixTimeMillis() < createProducerTimeout))
			{
				ReconnectLater(Cause);
			}
			else
			{
				State = State.Failed;
				ProducerCreatedFutureConflict.completeExceptionally(Cause);
				ClientConflict.cleanupProducer(this);
			}
			return null;
		});
		}

		public override void ConnectionFailed(PulsarClientException Exception)
		{
			if (DateTimeHelper.CurrentUnixTimeMillis() > createProducerTimeout && ProducerCreatedFutureConflict.completeExceptionally(Exception))
			{
				log.info("[{}] Producer creation failed for producer {}", Topic, ProducerId);
				State = State.Failed;
				ClientConflict.cleanupProducer(this);
			}
		}

		private void ResendMessages(ClientCnx Cnx)
		{
			Cnx.ctx().channel().eventLoop().execute(() =>
			{
			lock (this)
			{
				if (State == State.Closing || State == State.Closed)
				{
					Cnx.channel().close();
					return;
				}
				int MessagesToResend = pendingMessages.size();
				if (MessagesToResend == 0)
				{
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] No pending messages to resend {}", Topic, HandlerName, MessagesToResend);
					}
					if (ChangeToReadyState())
					{
						ProducerCreatedFutureConflict.complete(ProducerImpl.this);
						return;
					}
					else
					{
						Cnx.channel().close();
						return;
					}
				}
				log.info("[{}] [{}] Re-Sending {} messages to server", Topic, HandlerName, MessagesToResend);
				RecoverProcessOpSendMsgFrom(Cnx, null);
			}
			});
		}

		/// <summary>
		/// Strips checksum from <seealso cref="OpSendMsg"/> command if present else ignore it.
		/// </summary>
		/// <param name="op"> </param>
		private void StripChecksum(OpSendMsg Op)
		{
			int TotalMsgBufSize = Op.Cmd.readableBytes();
			ByteBufPair Msg = Op.Cmd;
			if (Msg != null)
			{
				ByteBuf HeaderFrame = Msg.First;
				HeaderFrame.markReaderIndex();
				try
				{
					HeaderFrame.skipBytes(4); // skip [total-size]
					int CmdSize = (int) HeaderFrame.readUnsignedInt();

					// verify if checksum present
					HeaderFrame.skipBytes(CmdSize);

					if (!hasChecksum(HeaderFrame))
					{
						return;
					}

					int HeaderSize = 4 + 4 + CmdSize; // [total-size] [cmd-length] [cmd-size]
					int ChecksumSize = 4 + 2; // [magic-number] [checksum-size]
					int ChecksumMark = (HeaderSize + ChecksumSize); // [header-size] [checksum-size]
					int MetaPayloadSize = (TotalMsgBufSize - ChecksumMark); // metadataPayload = totalSize - checksumMark
					int NewTotalFrameSizeLength = 4 + CmdSize + MetaPayloadSize; // new total-size without checksum
					HeaderFrame.resetReaderIndex();
					int HeaderFrameSize = HeaderFrame.readableBytes();

					HeaderFrame.setInt(0, NewTotalFrameSizeLength); // rewrite new [total-size]
					ByteBuf Metadata = HeaderFrame.slice(ChecksumMark, HeaderFrameSize - ChecksumMark); // sliced only
																										// metadata
					HeaderFrame.writerIndex(HeaderSize); // set headerFrame write-index to overwrite metadata over checksum
					Metadata.readBytes(HeaderFrame, Metadata.readableBytes());
					HeaderFrame.capacity(HeaderFrameSize - ChecksumSize); // reduce capacity by removed checksum bytes
				}
				finally
				{
					HeaderFrame.resetReaderIndex();
				}
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				log.warn("[{}] Failed while casting {} into ByteBufPair", HandlerName, (Op.Cmd == null ? null : Op.Cmd.GetType().FullName));
			}
		}

		public virtual int BrokerChecksumSupportedVersion()
		{
			return ProtocolVersion.v6.Number;
		}


		/// <summary>
		/// Process sendTimeout events
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
		public override void Run(Timeout Timeout)
		{
			if (Timeout.Cancelled)
			{
				return;
			}

			long TimeToWaitMs;

			lock (this)
			{
				// If it's closing/closed we need to ignore this timeout and not schedule next timeout.
				if (State == State.Closing || State == State.Closed)
				{
					return;
				}

				OpSendMsg FirstMsg = pendingMessages.peek();
				if (FirstMsg == null)
				{
					// If there are no pending messages, reset the timeout to the configured value.
					TimeToWaitMs = Conf.SendTimeoutMs;
				}
				else
				{
					// If there is at least one message, calculate the diff between the message timeout and the current
					// time.
					long Diff = (FirstMsg.CreatedAt + Conf.SendTimeoutMs) - DateTimeHelper.CurrentUnixTimeMillis();
					if (Diff <= 0)
					{
						// The diff is less than or equal to zero, meaning that the message has been timed out.
						// Set the callback to timeout on every message, then clear the pending queue.
						log.info("[{}] [{}] Message send timed out. Failing {} messages", Topic, HandlerName, pendingMessages.size());

						PulsarClientException Te = new PulsarClientException.TimeoutException(format("The producer %s can not send message to the topic %s within given timeout", HandlerName, Topic));
						FailPendingMessages(Cnx(), Te);
						Stats.IncrementSendFailed(pendingMessages.size());
						// Since the pending queue is cleared now, set timer to expire after configured value.
						TimeToWaitMs = Conf.SendTimeoutMs;
					}
					else
					{
						// The diff is greater than zero, set the timeout to the diff value
						TimeToWaitMs = Diff;
					}
				}

				sendTimeout = ClientConflict.timer().newTimeout(this, TimeToWaitMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
			}
		}

		/// <summary>
		/// This fails and clears the pending messages with the given exception. This method should be called from within the
		/// ProducerImpl object mutex.
		/// </summary>
		private void FailPendingMessages(ClientCnx Cnx, PulsarClientException Ex)
		{
			if (Cnx == null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger releaseCount = new java.util.concurrent.atomic.AtomicInteger();
				AtomicInteger ReleaseCount = new AtomicInteger();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean batchMessagingEnabled = isBatchMessagingEnabled();
				bool BatchMessagingEnabled = BatchMessagingEnabled;
				pendingMessages.forEach(op =>
				{
				ReleaseCount.addAndGet(BatchMessagingEnabled ? op.numMessagesInBatch: 1);
				try
				{
					op.callback.sendComplete(Ex);
				}
				catch (Exception T)
				{
					log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", Topic, HandlerName, op.sequenceId, T);
				}
				ReferenceCountUtil.safeRelease(op.cmd);
				op.recycle();
				});

				pendingMessages.clear();
				pendingCallbacks.clear();
				semaphore.release(ReleaseCount.get());
				if (BatchMessagingEnabled)
				{
					FailPendingBatchMessages(Ex);
				}

			}
			else
			{
				// If we have a connection, we schedule the callback and recycle on the event loop thread to avoid any
				// race condition since we also write the message on the socket from this thread
				Cnx.ctx().channel().eventLoop().execute(() =>
				{
				lock (ProducerImpl.this)
				{
					FailPendingMessages(null, Ex);
				}
				});
			}
		}

		/// <summary>
		/// fail any pending batch messages that were enqueued, however batch was not closed out
		/// 
		/// </summary>
		private void FailPendingBatchMessages(PulsarClientException Ex)
		{
			if (batchMessageContainer.Empty)
			{
				return;
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int numMessagesInBatch = batchMessageContainer.getNumMessagesInBatch();
			int NumMessagesInBatch = batchMessageContainer.NumMessagesInBatch;
			batchMessageContainer.Discard(Ex);
			semaphore.release(NumMessagesInBatch);
		}

		internal TimerTask batchMessageAndSendTask = new TimerTaskAnonymousInnerClass();

		public class TimerTaskAnonymousInnerClass : TimerTask
		{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
			public override void run(Timeout Timeout)
			{
				if (Timeout.Cancelled)
				{
					return;
				}
				if (log.TraceEnabled)
				{
					log.trace("[{}] [{}] Batching the messages from the batch container from timer thread", outerInstance.Topic, outerInstance.HandlerName);
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
					outerInstance.batchMessageAndSendTimeout = outerInstance.ClientConflict.timer().newTimeout(this, outerInstance.Conf.BatchingMaxPublishDelayMicros, BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS);
				}
			}
		}

		public override CompletableFuture<Void> FlushAsync()
		{
			CompletableFuture<MessageId> LastSendFuture;
			lock (ProducerImpl.this)
			{
				if (BatchMessagingEnabled)
				{
					BatchMessageAndSend();
				}
				LastSendFuture = this.lastSendFuture;
			}
			return LastSendFuture.thenApply(ignored => null);
		}

		public override void TriggerFlush()
		{
			if (BatchMessagingEnabled)
			{
				lock (ProducerImpl.this)
				{
					BatchMessageAndSend();
				}
			}
		}

		// must acquire semaphore before enqueuing
		private void BatchMessageAndSend()
		{
			if (log.TraceEnabled)
			{
				log.trace("[{}] [{}] Batching the messages from the batch container with {} messages", Topic, HandlerName, batchMessageContainer.NumMessagesInBatch);
			}
			if (!batchMessageContainer.Empty)
			{
				try
				{
					IList<OpSendMsg> OpSendMsgs;
					if (batchMessageContainer.MultiBatches)
					{
						OpSendMsgs = batchMessageContainer.CreateOpSendMsgs();
					}
					else
					{
						OpSendMsgs = Collections.singletonList(batchMessageContainer.CreateOpSendMsg());
					}
					batchMessageContainer.Clear();
					foreach (OpSendMsg OpSendMsg in OpSendMsgs)
					{
						ProcessOpSendMsg(OpSendMsg);
					}
				}
				catch (PulsarClientException)
				{
					Thread.CurrentThread.Interrupt();
					semaphore.release(batchMessageContainer.NumMessagesInBatch);
				}
				catch (Exception T)
				{
					semaphore.release(batchMessageContainer.NumMessagesInBatch);
					log.warn("[{}] [{}] error while create opSendMsg by batch message container", Topic, HandlerName, T);
				}
			}
		}

		private void ProcessOpSendMsg(OpSendMsg Op)
		{
			if (Op == null)
			{
				return;
			}
			try
			{
				if (Op.Msg != null && BatchMessagingEnabled)
				{
					BatchMessageAndSend();
				}
				pendingMessages.put(Op);
				if (Op.Msg != null)
				{
					LastSequenceIdPushed = Math.Max(LastSequenceIdPushed, GetHighestSequenceId(Op));
				}
				ClientCnx Cnx = cnx();
				if (Connected)
				{
					if (Op.Msg != null && Op.Msg.getSchemaState() == None)
					{
						TryRegisterSchema(Cnx, Op.Msg, Op.Callback);
						return;
					}
					// If we do have a connection, the message is sent immediately, otherwise we'll try again once a new
					// connection is established
					Op.Cmd.retain();
					Cnx.ctx().channel().eventLoop().execute(WriteInEventLoopCallback.Create(this, Cnx, Op));
					Stats.UpdateNumMsgsSent(Op.NumMessagesInBatchConflict, Op.BatchSizeByteConflict);
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug("[{}] [{}] Connection is not ready -- sequenceId {}", Topic, HandlerName, Op.SequenceId);
					}
				}
			}
			catch (InterruptedException Ie)
			{
				Thread.CurrentThread.Interrupt();
				ReleaseSemaphoreForSendOp(Op);
				if (Op != null)
				{
					Op.Callback.sendComplete(new PulsarClientException(Ie));
				}
			}
			catch (Exception T)
			{
				ReleaseSemaphoreForSendOp(Op);
				log.warn("[{}] [{}] error while closing out batch -- {}", Topic, HandlerName, T);
				if (Op != null)
				{
					Op.Callback.sendComplete(new PulsarClientException(T));
				}
			}
		}

		private void RecoverProcessOpSendMsgFrom(ClientCnx Cnx, MessageImpl From)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean stripChecksum = cnx.getRemoteEndpointProtocolVersion() < brokerChecksumSupportedVersion();
			bool StripChecksum = Cnx.RemoteEndpointProtocolVersion < BrokerChecksumSupportedVersion();
			IEnumerator<OpSendMsg> MsgIterator = pendingMessages.GetEnumerator();
			OpSendMsg PendingRegisteringOp = null;
			while (MsgIterator.MoveNext())
			{
				OpSendMsg Op = MsgIterator.Current;
				if (From != null)
				{
					if (Op.Msg == From)
					{
						From = null;
					}
					else
					{
						continue;
					}
				}
				if (Op.Msg != null)
				{
					if (Op.Msg.getSchemaState() == None)
					{
						if (!RePopulateMessageSchema(Op.Msg))
						{
							PendingRegisteringOp = Op;
							break;
						}
					}
					else if (Op.Msg.getSchemaState() == Broken)
					{
						Op.recycle();
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						MsgIterator.remove();
						continue;
					}
				}
				if (Op.Cmd == null)
				{
					checkState(Op.RePopulate != null);
					Op.RePopulate.run();
				}
				if (StripChecksum)
				{
					stripChecksum(Op);
				}
				Op.Cmd.retain();
				if (log.DebugEnabled)
				{
					log.debug("[{}] [{}] Re-Sending message in cnx {}, sequenceId {}", Topic, HandlerName, Cnx.channel(), Op.SequenceId);
				}
				Cnx.ctx().write(Op.Cmd, Cnx.ctx().voidPromise());
				Stats.UpdateNumMsgsSent(Op.NumMessagesInBatchConflict, Op.BatchSizeByteConflict);
			}
			Cnx.ctx().flush();
			if (!ChangeToReadyState())
			{
				// Producer was closed while reconnecting, close the connection to make sure the broker
				// drops the producer on its side
				Cnx.channel().close();
				return;
			}
			if (PendingRegisteringOp != null)
			{
				TryRegisterSchema(Cnx, PendingRegisteringOp.Msg, PendingRegisteringOp.Callback);
			}
		}

		public virtual long DelayInMillis
		{
			get
			{
				OpSendMsg FirstMsg = pendingMessages.peek();
				if (FirstMsg != null)
				{
					return DateTimeHelper.CurrentUnixTimeMillis() - FirstMsg.CreatedAt;
				}
				return 0L;
			}
		}

		public virtual string ConnectionId
		{
			get
			{
				return Cnx() != null ? connectionId : null;
			}
		}

		public virtual string ConnectedSince
		{
			get
			{
				return Cnx() != null ? connectedSince : null;
			}
		}

		public virtual int PendingQueueSize
		{
			get
			{
				return pendingMessages.size();
			}
		}


		public override string ProducerName
		{
			get
			{
				return HandlerName;
			}
		}

		// wrapper for connection methods
		public virtual ClientCnx Cnx()
		{
			return this.ConnectionHandler.Cnx();
		}

		public virtual void ResetBackoff()
		{
			this.ConnectionHandler.ResetBackoff();
		}

		public virtual void ConnectionClosed(ClientCnx Cnx)
		{
			this.ConnectionHandler.ConnectionClosed(Cnx);
		}

		public virtual ClientCnx ClientCnx
		{
			get
			{
				return this.ConnectionHandler.ClientCnx;
			}
			set
			{
				this.ConnectionHandler.ClientCnx = value;
			}
		}


		public virtual void ReconnectLater(Exception Exception)
		{
			this.ConnectionHandler.ReconnectLater(Exception);
		}

		public virtual void GrabCnx()
		{
			this.ConnectionHandler.GrabCnx();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting Semaphore getSemaphore()
		public virtual Semaphore Semaphore
		{
			get
			{
				return semaphore;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ProducerImpl));
	}

}