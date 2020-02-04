using DotNetty.Buffers;
using DotNetty.Common;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Optional;
using SharpPulsar.Api;
using SharpPulsar.Common.Compression;
using SharpPulsar.Common.Schema;
using SharpPulsar.Exception;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Shared;
using SharpPulsar.Util;
using SharpPulsar.Util.Atomic;
using SharpPulsar.Util.Atomic.Collections.Concurrent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

	public class ProducerImpl<T> : ProducerBase<T>, IConnection
	{

		// Producer id, used to identify a producer within a single connection
		protected internal readonly long ProducerId;

		// Variable is used through the atomic updater
		private ConcurrentDictionary<ProducerImpl<T>, long> msgIdGenerator = new ConcurrentDictionary<ProducerImpl<T>, long>();

		private readonly BlockingQueue<OpSendMsg> pendingMessages;
		private readonly BlockingQueue<OpSendMsg> pendingCallbacks;
		private readonly Semaphore semaphore;
		public HandlerState.State state;
		private  Timer sendTimeout;
		private  Timer batchMessageAndSendTimeout = null;
		private long createProducerTimeout;
		private readonly BatchMessageContainerBase batchMessageContainer;
		private TaskCompletionSource<IMessageId> lastSendTask = new TaskCompletionSource<IMessageId>();
		// Globally unique producer name
		internal string _handlerName;
		private bool _userProvidedProducerName = false;

		private string connectionId;
		private string connectedSince;
		private readonly int partitionIndex;

		private readonly CompressionCodec compressor;

		protected internal long LastSequenceIdPushed;

		private MessageCrypto msgCrypto = null;
		private ScheduledFuture<object> keyGeneratorTask = null;

		private readonly IDictionary<string, string> metadata;

		private Option<sbyte[]> _schemaVersion;

		public  ConnectionHandler ConnectionHandler;


		//private static readonly AtomicLongFieldUpdater<ProducerImpl> msgIdGeneratorUpdater = AtomicLongFieldUpdater.newUpdater(typeof(ProducerImpl), "msgIdGenerator");

		public ProducerImpl(PulsarClientImpl client, string topic, ProducerConfigurationData conf, TaskCompletionSource<IProducer<T>> producerCreatedTask, int partitionIndex, ISchema<T> schema, ProducerInterceptors Interceptors) : base(client, topic, conf, producerCreatedTask, schema, Interceptors)
		{
			ProducerId = Client.NewProducerId();
			_handlerName = conf.ProducerName;
			if (!string.IsNullOrWhiteSpace(_handlerName))
			{
				_userProvidedProducerName = true;
			}
			partitionIndex = partitionIndex;
			pendingMessages = new BlockingQueue<OpSendMsg>(conf.MaxPendingMessages);
			pendingCallbacks = new BlockingQueue<OpSendMsg>(conf.MaxPendingMessages);
			semaphore = new Semaphore(0, conf.MaxPendingMessages);

			compressor = CompressionCodecProvider.GetCompressionCodec(Conf.CompressionType);

			if (Conf.InitialSequenceId != null)
			{
				long InitialSequenceId = (long)Conf.InitialSequenceId;
				LastSequenceId = InitialSequenceId;
				LastSequenceIdPushed = InitialSequenceId;
				msgIdGenerator.TryAdd(this,InitialSequenceId + 1L);
			}
			else
			{
				LastSequenceId = -1L;
				LastSequenceIdPushed = -1L;
				msgIdGenerator.TryAdd(this, 0L);
			}

			if (Conf.EncryptionEnabled)
			{
				string LogCtx = "[" + Topic + "] [" + HandlerName + "] [" + ProducerId + "]";
				msgCrypto = new MessageCrypto(LogCtx, true);

				// Regenerate data key cipher at fixed interval
				keyGeneratorTask = Client.EventLoopGroup().scheduleWithFixedDelay(() =>
				{
					try
					{
						msgCrypto.AddPublicKeyCipher(Conf.EncryptionKeys, Conf.CryptoKeyReader);
					}
					catch (CryptoException e)
					{
						if (!ProducerCreatedFuture.Done)
						{
							log.LogWarning("[{}] [{}] [{}] Failed to add public key cipher.", Topic, HandlerName, ProducerId);
							ProducerCreatedFuture.completeExceptionally(PulsarClientException.wrap(E, string.Format("The producer {0} of the topic {1} " + "adds the public key cipher was failed", HandlerName, Topic)));
						}
					}
				}, 0L, 4L, BAMCIS.Util.Concurrent.TimeUnit.HOURS);

			}

			if (Conf.SendTimeoutMs > 0)
			{
				sendTimeout = Client.Timer = new Timer(_=> { var t = this; }, null, (int)Conf.SendTimeoutMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.ToMillis(Conf.SendTimeoutMs));
			}

			createProducerTimeout = DateTimeHelper.CurrentUnixTimeMillis() + Client.Configuration.OperationTimeoutMs;
			if (Conf.BatchingEnabled)
			{
				BatcherBuilder ContainerBuilder = Conf.BatcherBuilder;
				if (ContainerBuilder == null)
				{
					ContainerBuilder = BatcherBuilderFields.DEFAULT;
				}
				batchMessageContainer = (BatchMessageContainerBase)ContainerBuilder.Build();
				batchMessageContainer.Producer = this;
			}
			else
			{
				batchMessageContainer = null;
			}
			if (Client.Configuration.StatsIntervalSeconds > 0)
			{
				Stats = new ProducerStatsRecorderImpl<T>(Client, Conf, this);
			}
			else
			{
				Stats = ProducerStatsDisabled.INSTANCE;
			}

			if (!Conf.Properties.Any())
			{
				metadata = new Dictionary<string,string>();
			}
			else
			{
				metadata = new SortedDictionary<string, string>(Conf.Properties);
			}

			ConnectionHandler = new ConnectionHandler(this, new BackoffBuilder()
					.SetInitialTime(Client.Configuration.InitialBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).setMax(Client.Configuration.MaxBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).setMandatoryStop(Math.Max(100, Conf.SendTimeoutMs - 100), BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).create(), this);

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
			if (MultiSchemaMode != MultiSchemaMode.Auto)
			{
				return MultiSchemaMode == MultiSchemaMode.Enabled;
			}
			if (AutoEnable)
			{
				MultiSchemaMode = MultiSchemaMode.Enabled;
				return true;
			}
			return false;
		}


		public Task<IMessageId> InternalSendAsync(Message<T> Message)
		{

			TaskCompletionSource<IMessageId> task = new TaskCompletionSource<IMessageId>();
			MessageImpl<T> InterceptorMessage = (MessageImpl<T>) BeforeSend(Message);
			//Retain the buffer used by interceptors callback to get message. Buffer will release after complete interceptors.
			InterceptorMessage.DataBuffer.Retain();
			if (Interceptors != null)
			{
				//InterceptorMessage.Properties;
			}
			SendAsync(InterceptorMessage, new SendCallbackAnonymousInnerClass<T>(this, task, InterceptorMessage));
			return task.Task;
		}

		public class SendCallbackAnonymousInnerClass<S> : SendCallback
		{
			private readonly ProducerImpl<S> outerInstance;

			private TaskCompletionSource<IMessageId> task;
			private MessageImpl<S> interceptorMessage;

			public SendCallbackAnonymousInnerClass(ProducerImpl<S> outerInstance, TaskCompletionSource<IMessageId> task, MessageImpl<S> interceptorMessage)
			{
				outerInstance = outerInstance;
				task = task;
				interceptorMessage = interceptorMessage;
				nextCallback = null;
				nextMsg = null;
				createdAt = DateTimeHelper.CurrentUnixTimeMillis();
			}

			internal SendCallback nextCallback;
			internal MessageImpl<S> nextMsg;
			internal long createdAt;

			public TaskCompletionSource<IMessageId> Task
			{
				get
				{
					return task;
				}
			}

			public SendCallback NextSendCallback
			{
				get
				{
					return nextCallback;
				}
			}

			public MessageImpl<S> NextMessage
			{
				get
				{
					return nextMsg;
				}
			}

			public void SendComplete(System.Exception e)
			{
				try
				{
					if (e != null)
					{
						//outerInstance.Stats.NumSendFailed++;
						outerInstance.OnSendAcknowledgement(interceptorMessage, null, e);
						task.SetException(e);
					}
					else
					{
						outerInstance.OnSendAcknowledgement(interceptorMessage, interceptorMessage.getMessageId(), null);
						task.SetResult(interceptorMessage.MessageId);
						outerInstance.Stats.IncrementNumAcksReceived(System.nanoTime() - createdAt);
					}
				}
				finally
				{
					interceptorMessage.DataBuffer.Release();
				}

				while (nextCallback != null)
				{
					SendCallback SendCallback = nextCallback;
					var Msg = nextMsg;
					//Retain the buffer used by interceptors callback to get message. Buffer will release after complete interceptors.
					try
					{
						Msg.DataBuffer.Retain();
						if (e != null)
						{
							outerInstance.Stats.IncrementSendFailed();
							outerInstance.OnSendAcknowledgement(Msg, null, e);
							SendCallback.Task.SetException(e);
						}
						else
						{
							outerInstance.OnSendAcknowledgement(Msg, Msg.MessageId, null);
							SendCallback.Task.SetResult(Msg.MessageId);
							outerInstance.Stats.IncrementNumAcksReceived(System.nanoTime() - createdAt);
						}
						nextMsg = nextCallback.NextMessage;
						nextCallback = nextCallback.NextSendCallback;
					}
					finally
					{
						Msg.DataBuffer.Release();
					}
				}
			}

			public void AddCallback<T1>(MessageImpl<T1> Msg, SendCallback Scb)
			{
				nextMsg = (MessageImpl<object>)Msg;
				nextCallback = Scb;
			}

		public static implicit operator ProducerImpl<T>(ProducerImpl<T> v)
		{
			throw new NotImplementedException();
		}
	}

		public virtual void SendAsync<T1>(Message<T1> Message, SendCallback Callback)
		{
			if (Message is MessageImpl<T1>)
				return;

			if (!IsValidProducerState(Callback))
			{
				return;
			}

			if (!CanEnqueueRequest(Callback))
			{
				return;
			}

			MessageImpl<object> Msg = (MessageImpl) Message;
			MessageMetadata.Builder MsgMetadataBuilder = Msg.MessageBuilder;
			IByteBuffer Payload = Msg.DataBuffer;

			// If compression is enabled, we are compressing, otherwise it will simply use the same buffer
			int UncompressedSize = Payload.ReadableBytes;
			IByteBuffer CompressedPayload = Payload;
			// Batch will be compressed when closed
			// If a message has a delayed delivery time, we'll always send it individually
			if (!BatchMessagingEnabled || MsgMetadataBuilder.HasDeliverAtTime())
			{
				CompressedPayload = compressor.Encode(Payload);
				Payload.Release();

				// validate msg-size (For batching this will be check at the batch completion size)
				int CompressedSize = CompressedPayload.ReadableBytes;
				if (CompressedSize > ClientCnx.MaxMessageSize)
				{
					CompressedPayload.Release();
					string CompressedStr = (!BatchMessagingEnabled && Conf.CompressionType != ICompressionType.NONE) ? "Compressed" : "";
					PulsarClientException.InvalidMessageException InvalidMessageException = new PulsarClientException.InvalidMessageException(format("The producer %s of the topic %s sends a %s message with %d bytes that exceeds %d bytes", HandlerName, Topic, CompressedStr, CompressedSize, ClientCnx.MaxMessageSize));
					Callback.SendComplete(InvalidMessageException);
					return;
				}
			}

			if (!Msg.Replicated && MsgMetadataBuilder.HasProducerName())
			{
				PulsarClientException.InvalidMessageException InvalidMessageException = new PulsarClientException.InvalidMessageException(format("The producer %s of the topic %s can not reuse the same message", HandlerName, Topic));
				Callback.SendComplete(InvalidMessageException);
				CompressedPayload.Release();
				return;
			}

			if (!PopulateMessageSchema(Msg, Callback))
			{
				CompressedPayload.Release();
				return;
			}

			try
			{
				lock (this)
				{
					long SequenceId;
					if (!MsgMetadataBuilder.HasSequenceId())
					{
						SequenceId = msgIdGeneratorUpdater.getAndIncrement(this);
						MsgMetadataBuilder.SetSequenceId(SequenceId);
					}
					else
					{
						SequenceId = MsgMetadataBuilder.GetSequenceId();
					}
					if (!MsgMetadataBuilder.HasPublishTime())
					{
						MsgMetadataBuilder.SetPublishTime(Client.ClientClock.Millisecond);

						checkArgument(!MsgMetadataBuilder.hasProducerName());

						MsgMetadataBuilder.SetProducerName(HandlerName);

						if (Conf.CompressionType != ICompressionType.NONE)
						{
							MsgMetadataBuilder.SetCompression(CompressionCodecProvider.ConvertToWireProtocol(Conf.CompressionType));
						}
						MsgMetadataBuilder.SetUncompressedSize(UncompressedSize);
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
									log.LogWarning("Message with sequence id {} is definitely a duplicate", SequenceId);
								}
								else
								{
									log.LogInformation("Message with sequence id {} might be a duplicate but cannot be determined at this time.", SequenceId);
								}
								DoBatchSendAndAdd(Msg, Callback, Payload);
							}
							else
							{
								// handle boundary cases where message being added would exceed
								// batch size and/or max message size
								bool IsBatchFull = batchMessageContainer.Add(Msg, Callback);
								lastSendTask = Callback.Future;
								Payload.Release();
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
						IByteBuffer EncryptedPayload = EncryptMessage(MsgMetadataBuilder, CompressedPayload);
						// When publishing during replication, we need to set the correct number of message in batch
						// This is only used in tracking the publish rate stats
						int NumMessages = Msg.MessageBuilder.HasNumMessagesInBatch() ? Msg.MessageBuilder.NumMessagesInBatch : 1;
						OpSendMsg Op;
						if (Msg.GetSchemaState() == MessageImpl.SchemaState.Ready)
						{
							MessageMetadata MsgMetadata = MsgMetadataBuilder.Build();
							ByteBufPair Cmd = SendMessage(ProducerId, SequenceId, NumMessages, MsgMetadata, EncryptedPayload);
							Op = OpSendMsg.Create(Msg, Cmd, SequenceId, Callback);
							MsgMetadataBuilder.Recycle();
							MsgMetadata.Recycle();
						}
						else
						{
							Op = OpSendMsg.Create(Msg, null, SequenceId, Callback);
							Op.RePopulate = () =>
							{
							MessageMetadata MsgMetadata = MsgMetadataBuilder.Build();
							Op.Cmd = SendMessage(ProducerId, SequenceId, NumMessages, MsgMetadata, EncryptedPayload);
							MsgMetadataBuilder.Recycle();
							MsgMetadata.Recycle();
							};
						}
						Op.NumMessagesInBatch = NumMessages;
						Op.BatchSizeByte = EncryptedPayload.ReadableBytes;
						lastSendTask = Callback.Future;
						ProcessOpSendMsg(Op);
					}
				}
			}
			catch (PulsarClientException E)
			{
				semaphore.Release();
				Callback.SendComplete(E);
			}
			catch (System.Exception T)
			{
				semaphore.Release();
				Callback.SendComplete(new PulsarClientException(T.Message));
			}
		}

		private bool PopulateMessageSchema(MessageImpl Msg, SendCallback Callback)
		{
			MessageMetadata.Builder MsgMetadataBuilder = Msg.MessageBuilder;
			if (Msg.Schema == Schema)
			{
				schemaVersion.ifPresent(v => MsgMetadataBuilder.setSchemaVersion(ByteString.copyFrom(v)));
				Msg.SetSchemaState(MessageImpl.SchemaState.Ready);
				return true;
			}
			if (!IsMultiSchemaEnabled(true))
			{
				Callback.SendComplete(new PulsarClientException.InvalidMessageException(string.Format("The producer %s of the topic %s is disabled the `MultiSchema`", HandlerName, Topic)));
				return false;
			}
			SchemaHash SchemaHash = SchemaHash.of(Msg.Schema);
			sbyte[] SchemaVersion = SchemaCache.get(SchemaHash);
			if (SchemaVersion != null)
			{
				MsgMetadataBuilder.SchemaVersion = ByteString.CopyFrom(SchemaVersion);
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
			SchemaInfo SchemaInfo = Optional.ofNullable(Msg.Schema).map(Schema.getSchemaInfo).filter(si => si.Type.Value > 0).orElse(SchemaFields.BYTES.SchemaInfo);
			GetOrCreateSchemaAsync(Cnx, SchemaInfo).handle((v, ex) =>
			{
				if (ex != null)
				{
					Exception T = FutureUtil.unwrapCompletionException(ex);
					log.LogWarning("[{}] [{}] GetOrCreateSchema error", Topic, HandlerName, T);
					if (T is PulsarClientException.IncompatibleSchemaException)
					{
						Msg.setSchemaState(MessageImpl.SchemaState.Broken);
						Callback.sendComplete((PulsarClientException.IncompatibleSchemaException)T);
					}
				}
				else
				{
					log.LogWarning("[{}] [{}] GetOrCreateSchema succeed", Topic, HandlerName);
					SchemaHash SchemaHash = SchemaHash.of(Msg.Schema);
					SchemaCache.putIfAbsent(SchemaHash, v);
					Msg.MessageBuilder.SchemaVersion = ByteString.copyFrom(v);
					Msg.setSchemaState(MessageImpl.SchemaState.Ready);
				}
			});
			Cnx.Ctx().Channel.EventLoop.Execute(() =>
			{
				lock (this)
				{
					RecoverProcessOpSendMsgFrom(Cnx, Msg);
				}
			});
		}

		private ValueTask<sbyte[]> GetOrCreateSchemaAsync(ClientCnx Cnx, SchemaInfo SchemaInfo)
		{
			if (!Commands.PeerSupportsGetOrCreateSchema(Cnx.RemoteEndpointProtocolVersion))
			{
				return FutureUtil.failedFuture(new PulsarClientException.NotSupportedException(string.Format("The command `GetOrCreateSchema` is not supported for the protocol version %d. " + "The producer is %s, topic is %s", Cnx.RemoteEndpointProtocolVersion, HandlerName, Topic)));
			}
			long RequestId = ClientConflict.newRequestId();
			IByteBuffer Request = Commands.NewGetOrCreateSchema(RequestId, Topic, SchemaInfo);
			log.LogInformation("[{}] [{}] GetOrCreateSchema request", Topic, HandlerName);
			return Cnx.SendGetOrCreateSchema(Request, RequestId);
		}

		public virtual IByteBuffer EncryptMessage(MessageMetadata.Builder MsgMetadata, IByteBuffer CompressedPayload)
		{

			IByteBuffer EncryptedPayload = CompressedPayload;
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
					log.LogWarning("[{}] [{}] Failed to encrypt message {}. Proceeding with publishing unencrypted message", Topic, HandlerName, E.Message);
					return CompressedPayload;
				}
				throw E;
			}
			return EncryptedPayload;
		}

		public virtual ByteBufPair SendMessage(long ProducerId, long SequenceId, int NumMessages, MessageMetadata MsgMetadata, IByteBuffer CompressedPayload)
		{
			return Commands.NewSend(ProducerId, SequenceId, NumMessages, ChecksumType, MsgMetadata, CompressedPayload);
		}

		public virtual ByteBufPair SendMessage(long ProducerId, long LowestSequenceId, long HighestSequenceId, int NumMessages, MessageMetadata MsgMetadata, IByteBuffer CompressedPayload)
		{
			return Commands.NewSend(ProducerId, LowestSequenceId, HighestSequenceId, NumMessages, ChecksumType, MsgMetadata, CompressedPayload);
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
			return Msg.GetSchemaState() == MessageImpl.SchemaState.Ready && BatchMessagingEnabled && !Msg.MessageBuilder.hasDeliverAtTime();
		}

		private bool CanAddToCurrentBatch<T1>(MessageImpl<T1> Msg)
		{
			return batchMessageContainer.HaveEnoughSpace(Msg) && (!IsMultiSchemaEnabled(false) || batchMessageContainer.HasSameSchema(Msg));
		}

		private void DoBatchSendAndAdd<T1>(MessageImpl<T1> Msg, SendCallback Callback, IByteBuffer Payload)
		{
			if (log.IsEnabled(LogLevel.Debug))
			{
				log.log.LogDebug("[{}] [{}] Closing out batch to accommodate large message with size {}", Topic, HandlerName, Msg.DataBuffer.readableBytes());
			}
			try
			{
				BatchMessageAndSend();
				batchMessageContainer.Add(Msg, Callback);
				lastSendTask = Callback.Task;
			}
			finally
			{
				Payload.Release();
			}
		}

		private bool IsValidProducerState(SendCallback Callback)
		{
			switch (state)
			{
			case State.Ready:
				// OK
			case State.Connecting:
				// We are OK to queue the messages on the client, it will be sent to the broker once we get the connection
			case State.RegisteringSchema:
				// registering schema
				return true;
			case State.Closing:
			case State.Closed:
				Callback.SendComplete(new PulsarClientException.AlreadyClosedException("Producer already closed"));
				return false;
			case State.Terminated:
				Callback.SendComplete(new PulsarClientException.TopicTerminatedException("Topic was terminated"));
				return false;
			case State.Failed:
			case State.Uninitialized:
			default:
				Callback.SendComplete(new PulsarClientException.NotConnectedException());
				return false;
			}
		}

		private bool CanEnqueueRequest(SendCallback Callback)
		{
			try
			{
				if (Conf.BlockIfQueueFull)
				{
					semaphore.WaitOne();
				}
				else
				{
					if (!semaphore.WaitOne())
					{
						Callback.SendComplete(new PulsarClientException.ProducerQueueIsFullError("Producer send queue is full"));
						return false;
					}
				}
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				Callback.SendComplete(new PulsarClientException(E));
				return false;
			}

			return true;
		}

		public sealed class WriteInEventLoopCallback: IRunnable
		{
			internal ProducerImpl<object> Producer;
			internal ByteBufPair Cmd;
			internal long SequenceId;
			internal ClientCnx Cnx;

			internal static WriteInEventLoopCallback Create<T1>(ProducerImpl<T1> Producer, ClientCnx Cnx, OpSendMsg Op)
			{
				WriteInEventLoopCallback C = _pool.Take();
				C.Producer = Producer;
				C.Cnx = Cnx;
				C.SequenceId = Op.SequenceId;
				C.Cmd = Op.Cmd;
				return C;
			}

			public void Run()
			{
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("[{}] [{}] Sending message cnx {}, sequenceId {}", Producer.Topic, Producer.HandlerName, Cnx, SequenceId);
				}

				try
				{
					Cnx.Ctx().WriteAndFlushAsync(Cmd);
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
				_handle.Release(this);
			}

			internal static ThreadLocalPool<WriteInEventLoopCallback> _pool = new ThreadLocalPool<WriteInEventLoopCallback>(handle => new WriteInEventLoopCallback(handle), 1, true);

			internal ThreadLocalPool.Handle _handle;
			private WriteInEventLoopCallback(ThreadLocalPool.Handle handle)
			{
				_handle = handle;
			}
		}

		public ValueTask CloseAsync()
		{
			State? CurrentState = GetAndUpdateState(state =>
			{
				if (state == State.Closed)
				{
					return state;
				}
				return State.Closing;
			});

			if (CurrentState == State.Closed || CurrentState == State.Closing)
			{
				return new ValueTask(Task.CompletedTask);
			}

			Timer Timeout = sendTimeout;
			if (Timeout != null)
			{
				Timeout.Cancel();
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
				log.LogInformation("[{}] [{}] Closed Producer (not connected)", Topic, HandlerName);
				lock (this)
				{
					state = State.Closed;
					Client.CleanupProducer(this);
					PulsarClientException Ex = new PulsarClientException.AlreadyClosedException(string.Format("The producer %s of the topic %s was already closed when closing the producers", HandlerName, Topic));
					pendingMessages.ToList().ForEach(msg =>
					{
						msg.Callback.SendComplete(Ex);
						msg.Cmd.Release();
						msg.Recycle();
					});
					pendingMessages.Clear();
				}

				return new ValueTask(Task.CompletedTask);
			}

			long RequestId = Client.NewRequestId();
			IByteBuffer Cmd = Commands.NewCloseProducer(ProducerId, RequestId);

			TaskCompletionSource closeTask = new TaskCompletionSource();
			Cnx.SendRequestWithId(Cmd, RequestId).handle((v, exception) =>
			{
				Cnx.removeProducer(ProducerId);
				if (exception == null || !Cnx.ctx().channel().Active)
				{
					lock (this)
					{
						log.LogInformation("[{}] [{}] Closed Producer", Topic, HandlerName);
						State = State.Closed;
						pendingMessages.ToList().ForEach(msg =>
						{
							msg.Cmd.Release();
							msg.Recycle();
						});
						pendingMessages.Clear();
					}
					closeTask.SetResult(null);
					ClientConflict.cleanupProducer(this);
			}
			else
			{
				CloseFuture.completeExceptionally(exception);
			}
			return null;
			});

			return new ValueTask(closeTask.Task);
		}

		public override bool Connected
		{
			get
			{
				return ConnectionHandler.ClientCnx != null && (state == State.Ready);
			}
		}

		public virtual bool Writable
		{
			get
			{
				ClientCnx Cnx = ConnectionHandler.ClientCnx;
				return Cnx != null && Cnx.Channel().IsWritable;
			}
		}

		public virtual void Terminated(ClientCnx Cnx)
		{
			State PreviousState = GetAndUpdateState(state => (state == State.Closed ? State.Closed : State.Terminated));
			if (PreviousState != State.Terminated && PreviousState != State.Closed)
			{
				log.LogInformation("[{}] [{}] The topic has been terminated", Topic, HandlerName);
				ClientCnx = null;

				FailPendingMessages(Cnx, new PulsarClientException.TopicTerminatedException(string.Format("The topic %s that the producer %s produces to has been terminated", Topic, HandlerName)));
			}
		}

		public virtual void AckReceived(ClientCnx Cnx, long SequenceId, long HighestSequenceId, long LedgerId, long EntryId)
		{
			OpSendMsg Op = null;
			bool Callback = false;
			lock (this)
			{
				Op = pendingMessages.Peek();
				if (Op == null)
				{
					if (log.IsEnabled(LogLevel.Debug))
					{
						log.LogDebug("[{}] [{}] Got ack for timed out msg {} - {}", Topic, HandlerName, SequenceId, HighestSequenceId);
					}
					return;
				}

				if (SequenceId > Op.SequenceId)
				{
					log.LogWarning("[{}] [{}] Got ack for msg. expecting: {} - {} - got: {} - {} - queue-size: {}", Topic, HandlerName, Op.SequenceId, Op.HighestSequenceId, SequenceId, HighestSequenceId, pendingMessages.size());
					// Force connection closing so that messages can be re-transmitted in a new connection
					Cnx.Channel().CloseAsync();
				}
				else if (SequenceId < Op.SequenceId)
				{
					// Ignoring the ack since it's referring to a message that has already timed out.
					if (log.IsEnabled(LogLevel.Debug))
					{
						log.LogDebug("[{}] [{}] Got ack for timed out msg. expecting: {} - {} - got: {} - {}", Topic, HandlerName, Op.SequenceId, Op.HighestSequenceId, SequenceId, HighestSequenceId);
					}
				}
				else
				{
					// Add check `sequenceId >= highestSequenceId` for backward compatibility.
					if (SequenceId >= HighestSequenceId || HighestSequenceId == Op.HighestSequenceId)
					{
						// Message was persisted correctly
						if (log.IsEnabled(LogLevel.Debug))
						{
							log.LogDebug("[{}] [{}] Received ack for msg {} ", Topic, HandlerName, SequenceId);
						}
						pendingMessages.Dequeue();
						ReleaseSemaphoreForSendOp(Op);
						Callback = true;
						pendingCallbacks.Enqueue(Op);
					}
					else
					{
						log.LogWarning("[{}] [{}] Got ack for batch msg error. expecting: {} - {} - got: {} - {} - queue-size: {}", Topic, HandlerName, Op.SequenceId, Op.HighestSequenceId, SequenceId, HighestSequenceId, pendingMessages.size());
						// Force connection closing so that messages can be re-transmitted in a new connection
						Cnx.Channel().CloseAsync();
					}
				}
			}
			if (Callback)
			{
				Op = pendingCallbacks.Peek();
				if (Op != null)
				{
					LastSequenceId = Math.Max(LastSequenceId, GetHighestSequenceId(Op));
					Op.SetMessageId(LedgerId, EntryId, partitionIndex);
					try
					{
						// Need to protect ourselves from any exception being thrown in the future handler from the
						// application
						Op.Callback.SendComplete(null);
					}
					catch (System.Exception T)
					{
						log.LogWarning("[{}] [{}] Got exception while completing the callback for msg {}:", Topic, HandlerName, SequenceId, T);
					}
					ReferenceCountUtil.SafeRelease(Op.Cmd);
					Op.Recycle();
				}
			}
		}

		private long GetHighestSequenceId(OpSendMsg Op)
		{
			return Math.Max(Op.HighestSequenceId, Op.SequenceId);
		}

		private void ReleaseSemaphoreForSendOp(OpSendMsg Op)
		{
			semaphore.Release(BatchMessagingEnabled ? Op.NumMessagesInBatchConflict : 1);
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
				OpSendMsg Op = pendingMessages.Peek();
				if (Op == null)
				{
					if (log.IsEnabled(LogLevel.Debug))
					{
						log.LogDebug("[{}] [{}] Got send failure for timed out msg {}", Topic, HandlerName, SequenceId);
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
							pendingMessages.Dequeue();
							ReleaseSemaphoreForSendOp(Op);
							try
							{
								Op.Callback.SendComplete(new PulsarClientException.ChecksumException(string.Format("The checksum of the message which is produced by producer %s to the topic " + "%s is corrupted", HandlerName, Topic)));
							}
							catch (System.Exception T)
							{
								log.LogWarning("[{}] [{}] Got exception while completing the callback for msg {}:", Topic, HandlerName, SequenceId, T);
							}
							ReferenceCountUtil.SafeRelease(Op.Cmd);
							Op.Recycle();
							return;
						}
						else
						{
							if (log.IsEnabled(LogLevel.Debug))
							{
								log.LogDebug("[{}] [{}] Message is not corrupted, retry send-message with sequenceId {}", Topic, HandlerName, SequenceId);
							}
						}
        
					}
					else
					{
						if (log.IsEnabled(LogLevel.Debug))
						{
							log.LogDebug("[{}] [{}] Corrupt message is already timed out {}", Topic, HandlerName, SequenceId);
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
				IByteBuffer HeaderFrame = Msg.First;
				HeaderFrame.MarkReaderIndex();
				try
				{
					// skip bytes up to checksum index
					HeaderFrame.SkipBytes(4); // skip [total-size]
					int CmdSize = (int) HeaderFrame.ReadUnsignedInt();
					HeaderFrame.SkipBytes(CmdSize);
					// verify if checksum present
					if (Commands.HasChecksum(HeaderFrame))
					{
						int Checksum = Commands.ReadChecksum(HeaderFrame);
						// msg.readerIndex is already at header-payload index, Recompute checksum for headers-payload
						int MetadataChecksum = Commands.ComputeChecksum(HeaderFrame);
						long ComputedChecksum = ResumeChecksum(MetadataChecksum, Msg.Second);
						return Checksum == ComputedChecksum;
					}
					else
					{
						log.LogWarning("[{}] [{}] checksum is not present into message with id {}", Topic, HandlerName, Op.SequenceId);
					}
				}
				finally
				{
					HeaderFrame.ResetReaderIndex();
				}
				return true;
			}
			else
			{
				log.LogWarning("[{}] Failed while casting {} into ByteBufPair", HandlerName, (Op.Cmd == null ? null : Op.Cmd.GetType().FullName));
				return false;
			}
		}

		public sealed class OpSendMsg
		{
			internal MessageImpl<object> Msg;
			internal IList<MessageImpl<object>> Msgs;
			internal ByteBufPair Cmd;
			internal SendCallback Callback;
			internal ThreadStart RePopulate;
			internal long SequenceId;
			internal long CreatedAt;
			internal long BatchSizeByteConflict = 0;
			internal int NumMessagesInBatchConflict = 1;
			internal long HighestSequenceId;

			internal static OpSendMsg Create<T1>(MessageImpl<T1> Msg, ByteBufPair Cmd, long SequenceId, SendCallback Callback)
			{
				OpSendMsg Op = _pool.Take();
				Op.Msg = Msg;
				Op.Cmd = Cmd;
				Op.Callback = Callback;
				Op.SequenceId = SequenceId;
				Op.CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
				return Op;
			}

			internal static OpSendMsg Create<T1>(IList<T1> Msgs, ByteBufPair Cmd, long SequenceId, SendCallback Callback)
			{
				OpSendMsg Op = _pool.Take();
				Op.Msgs = Msgs;
				Op.Cmd = Cmd;
				Op.Callback = Callback;
				Op.SequenceId = SequenceId;
				Op.CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
				return Op;
			}

			internal static OpSendMsg Create<T1>(IList<T1> Msgs, ByteBufPair Cmd, long LowestSequenceId, long HighestSequenceId, SendCallback Callback)
			{
				OpSendMsg Op = _pool.Take();
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
				_handle.Release(this);
			}

			public int NumMessagesInBatch
			{
				set
				{
					NumMessagesInBatchConflict = value;
				}
			}

			public long BatchSizeByte
			{
				set
				{
					BatchSizeByteConflict = value;
				}
			}

			public void SetMessageId(long LedgerId, long EntryId, int PartitionIndex)
			{
				if (Msg != null)
				{
					Msg.SetMessageId(new MessageIdImpl(LedgerId, EntryId, PartitionIndex));
				}
				else
				{
					for (int BatchIndex = 0; BatchIndex < Msgs.Count; BatchIndex++)
					{
						Msgs[BatchIndex].SetMessageId(new BatchMessageIdImpl(LedgerId, EntryId, PartitionIndex, BatchIndex));
					}
				}
			}

			internal static ThreadLocalPool<OpSendMsg> _pool = new ThreadLocalPool<OpSendMsg>(handle => new OpSendMsg(handle), 1, true);

			internal ThreadLocalPool.Handle _handle;
			private OpSendMsg(ThreadLocalPool.Handle handle)
			{
				_handle = handle;
			}
		}

		public void ConnectionOpened(in ClientCnx cnx)
		{
			var Cnx = cnx;
			// we set the cnx reference before registering the producer on the cnx, so if the cnx breaks before creating the
			// producer, it will try to grab a new cnx
			ConnectionHandler.ClientCnx = Cnx;
			Cnx.RegisterProducer(ProducerId, this);

			log.LogInformation("[{}] [{}] Creating producer on cnx {}", Topic, HandlerName, Cnx.Ctx().Channel);

			long RequestId = Client.NewRequestId();

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
						if (Commands.PeerSupportJsonSchemaAvroFormat(Cnx.RemoteEndpointProtocolVersion))
						{
							SchemaInfo = Schema.SchemaInfo;
						}
						else if (Schema is JSONSchema<T>)
						{
							JSONSchema<T> JsonSchema = (JSONSchema<T>) Schema;
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

			Cnx.SendRequestWithId(Commands.NewProducer(Topic, ProducerId, RequestId, HandlerName, Conf.EncryptionEnabled, metadata, SchemaInfo, ConnectionHandler.Epoch, _userProvidedProducerName), RequestId)
		    .AsTask().ContinueWith(task =>
			{
				var response = task.Result;
				string ProducerName = response.ProducerName;
				long LastSequenceId = response.LastSequenceId;
				var schemaVersion = response.SchemaVersion;
				if(schemaVersion != null)
				{
					SchemaCache.TryAdd(SchemaHash.Of(Schema), schemaVersion);
				}
				
				lock (this)
				{
					if (state == State.Closing || state == State.Closed)
					{
						Cnx.RemoveProducer(ProducerId);
						Cnx.Channel().CloseAsync();
						return;
					}
					ResetBackoff();
					log.LogInformation("[{}] [{}] Created producer on cnx {}", Topic, ProducerName, Cnx.Ctx().Channel);
					connectionId = Cnx.Ctx().Channel.ToString();
					connectedSince = DateFormatter.now();
					if (string.ReferenceEquals(HandlerName, null))
					{
						HandlerName = ProducerName;
					}
					if (msgIdGenerator == 0 && Conf.InitialSequenceId == null)
					{
						LastSequenceId = LastSequenceId;
						msgIdGenerator = LastSequenceId + 1;
					}
					if (!ProducerCreatedFutureConflict.Done && BatchMessagingEnabled)
					{
						ClientConflict.timer().newTimeout(batchMessageAndSendTask, Conf.BatchingMaxPublishDelayMicros, BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS);
					}
					ResendMessages(Cnx);
				}
			}).exceptionally(e =>
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
						log.LogWarning("[{}] [{}] Topic backlog quota exceeded. Throwing Exception on producer.", Topic, HandlerName);
						if (log.IsEnabled(LogLevel.Debug))
						{
							log.LogDebug("[{}] [{}] Pending messages: {}", Topic, HandlerName, pendingMessages.size());
						}
						PulsarClientException Bqe = new PulsarClientException.ProducerBlockedQuotaExceededException(format("The backlog quota of the topic %s that the producer %s produces to is exceeded", Topic, HandlerName));
						FailPendingMessages(cnx(), Bqe);
					}
				}
				else if (Cause is PulsarClientException.ProducerBlockedQuotaExceededError)
				{
					log.LogWarning("[{}] [{}] Producer is blocked on creation because backlog exceeded on topic.", HandlerName, Topic);
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
	
		public void ConnectionFailed(PulsarClientException Exception)
		{
			if (DateTimeHelper.CurrentUnixTimeMillis() > createProducerTimeout && ProducerCreatedFutureConflict.completeExceptionally(Exception))
			{
				log.LogInformation("[{}] Producer creation failed for producer {}", Topic, ProducerId);
				State = State.Failed;
				ClientConflict.cleanupProducer(this);
			}
		}

		private void ResendMessages(ClientCnx Cnx)
		{
			Cnx.Ctx().Channel.EventLoop.Execute(() =>
			{
			lock (this)
			{
				if (state == State.Closing || State == State.Closed)
				{
					Cnx.Channel().CloseAsync();
					return;
				}
				int MessagesToResend = pendingMessages.Count();
				if (MessagesToResend == 0)
				{
					if (log.IsEnabled(LogLevel.Debug))
					{
						log.LogDebug("[{}] [{}] No pending messages to resend {}", Topic, HandlerName, MessagesToResend);
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
				log.LogInformation("[{}] [{}] Re-Sending {} messages to server", Topic, HandlerName, MessagesToResend);
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
			int TotalMsgBufSize = Op.Cmd.ReadableBytes();
			ByteBufPair Msg = Op.Cmd;
			if (Msg != null)
			{
				IByteBuffer HeaderFrame = Msg.First;
				HeaderFrame.MarkReaderIndex();
				try
				{
					HeaderFrame.SkipBytes(4); // skip [total-size]
					int CmdSize = (int) HeaderFrame.ReadUnsignedInt();

					// verify if checksum present
					HeaderFrame.SkipBytes(CmdSize);

					if (!Commands.HasChecksum(HeaderFrame))
					{
						return;
					}

					int HeaderSize = 4 + 4 + CmdSize; // [total-size] [cmd-length] [cmd-size]
					int ChecksumSize = 4 + 2; // [magic-number] [checksum-size]
					int ChecksumMark = (HeaderSize + ChecksumSize); // [header-size] [checksum-size]
					int MetaPayloadSize = (TotalMsgBufSize - ChecksumMark); // metadataPayload = totalSize - checksumMark
					int NewTotalFrameSizeLength = 4 + CmdSize + MetaPayloadSize; // new total-size without checksum
					HeaderFrame.ResetReaderIndex();
					int HeaderFrameSize = HeaderFrame.ReadableBytes;

					HeaderFrame.SetInt(0, NewTotalFrameSizeLength); // rewrite new [total-size]
					IByteBuffer Metadata = HeaderFrame.Slice(ChecksumMark, HeaderFrameSize - ChecksumMark); // sliced only
																										// metadata
					HeaderFrame.SetWriterIndex(HeaderSize); // set headerFrame write-index to overwrite metadata over checksum
					Metadata.ReadBytes(HeaderFrame, Metadata.ReadableBytes);
					HeaderFrame.AdjustCapacity(HeaderFrameSize - ChecksumSize); // reduce capacity by removed checksum bytes
				}
				finally
				{
					HeaderFrame.ResetReaderIndex();
				}
			}
			else
			{

				log.LogWarning("[{}] Failed while casting {} into ByteBufPair", HandlerName, (Op.Cmd == null ? null : Op.Cmd.GetType().FullName));
			}
		}

		public virtual int BrokerChecksumSupportedVersion()
		{
			return (int)ProtocolVersion.V6;
		}


		/// <summary>
		/// Process sendTimeout events
		/// </summary>
		public void Run()
		{
			if (Timeout.Cancelled)
			{
				return;
			}

			long TimeToWaitMs;

			lock (this)
			{
				// If it's closing/closed we need to ignore this timeout and not schedule next timeout.
				if (state == State.Closing || state == State.Closed)
				{
					return;
				}

				OpSendMsg FirstMsg = pendingMessages.Peek();
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
						log.LogInformation("[{}] [{}] Message send timed out. Failing {} messages", Topic, HandlerName, pendingMessages.size());

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
				AtomicInt ReleaseCount = new AtomicInt();
				bool BatchMessagingEnabled = BatchMessagingEnabled;
				pendingMessages.ToList().ForEach(op =>
				{
						ReleaseCount.AddAndGet(BatchMessagingEnabled ? op.NumMessagesInBatch: 1);
						try
						{
							op.Callback.SendComplete(Ex);
						}
						catch (System.Exception T)
						{
							log.LogWarning("[{}] [{}] Got exception while completing the callback for msg {}:", Topic, HandlerName, op.sequenceId, T);
						}
						ReferenceCountUtil.SafeRelease(op.Cmd);
						op.Recycle();
				});

				pendingMessages.Clear();
				pendingCallbacks.Clear();
				semaphore.Release(ReleaseCount.Get());
				if (BatchMessagingEnabled)
				{
					FailPendingBatchMessages(Ex);
				}

			}
			else
			{
				// If we have a connection, we schedule the callback and recycle on the event loop thread to avoid any
				// race condition since we also write the message on the socket from this thread
				Cnx.Ctx().Channel.EventLoop.Execute(() =>
				{
					lock(this)
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

			int NumMessagesInBatch = batchMessageContainer.NumMessagesInBatch;
			batchMessageContainer.Discard(Ex);
			semaphore.Release(NumMessagesInBatch);
		}

		internal Timer batchMessageAndSendTask = new TimerTaskAnonymousInnerClass();

		public class TimerTaskAnonymousInnerClass: IRunnable
		{

			public void Run()
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

		public ValueTask FlushAsync()
		{
			ValueTask<IMessageId> lastSendTask;
			lock (this)
			{
				if (BatchMessagingEnabled)
				{
					BatchMessageAndSend();
				}
				LastSendFuture = lastSendTask;
			}
			return LastSendTask.thenApply(ignored => null);
		}

		public override void TriggerFlush()
		{
			if (BatchMessagingEnabled)
			{
				lock (this)
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
						OpSendMsgs = new List<OpSendMsg>(batchMessageContainer.CreateOpSendMsg());
						//OpSendMsgs.Add()
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
					semaphore.Release(batchMessageContainer.NumMessagesInBatch);
				}
				catch (System.Exception T)
				{
					semaphore.Release(batchMessageContainer.NumMessagesInBatch);
					log.LogWarning("[{}] [{}] error while create opSendMsg by batch message container", Topic, HandlerName, T);
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
				pendingMessages.Enqueue(Op);
				if (Op.Msg != null)
				{
					LastSequenceIdPushed = Math.Max(LastSequenceIdPushed, GetHighestSequenceId(Op));
				}
				ClientCnx Cnx = Cnx();
				if (Connected)
				{
					if (Op.Msg != null && Op.Msg.GetSchemaState() == None)
					{
						TryRegisterSchema(Cnx, Op.Msg, Op.Callback);
						return;
					}
					// If we do have a connection, the message is sent immediately, otherwise we'll try again once a new
					// connection is established
					Op.Cmd.Retain();
					Cnx.Ctx().Channel.EventLoop.Execute(WriteInEventLoopCallback.Create(this, Cnx, Op));
					Stats.UpdateNumMsgsSent(Op.NumMessagesInBatchConflict, Op.BatchSizeByteConflict);
				}
				else
				{
					if (log.IsEnabled(LogLevel.Debug))
					{
						log.LogDebug("[{}] [{}] Connection is not ready -- sequenceId {}", Topic, HandlerName, Op.SequenceId);
					}
				}
			}
			catch (InterruptedException Ie)
			{
				Thread.CurrentThread.Interrupt();
				ReleaseSemaphoreForSendOp(Op);
				if (Op != null)
				{
					Op.Callback.SendComplete(new PulsarClientException(Ie));
				}
			}
			catch (Exception T)
			{
				ReleaseSemaphoreForSendOp(Op);
				log.LogWarning("[{}] [{}] error while closing out batch -- {}", Topic, HandlerName, T);
				if (Op != null)
				{
					Op.Callback.SendComplete(new PulsarClientException(T));
				}
			}
		}

		private void RecoverProcessOpSendMsgFrom(ClientCnx Cnx, MessageImpl From)
		{
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
					if (Op.Msg.GetSchemaState() == None)
					{
						if (!RePopulateMessageSchema(Op.Msg))
						{
							PendingRegisteringOp = Op;
							break;
						}
					}
					else if (Op.Msg.GetSchemaState() ==  Broken)
					{
						Op.Recycle();
						MsgIterator.Remove();
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
				Op.Cmd.Retain();
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("[{}] [{}] Re-Sending message in cnx {}, sequenceId {}", Topic, HandlerName, Cnx.Channel(), Op.SequenceId);
				}
				Cnx.Ctx().WriteAsync(Op.Cmd);
				Stats.UpdateNumMsgsSent(Op.NumMessagesInBatchConflict, Op.BatchSizeByteConflict);
			}
			Cnx.Ctx().Flush();
			if (!ChangeToReadyState())
			{
				// Producer was closed while reconnecting, close the connection to make sure the broker
				// drops the producer on its side
				Cnx.Channel().CloseAsync();
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
				OpSendMsg FirstMsg = pendingMessages.Peek();
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
				return pendingMessages.Count;
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
			return ConnectionHandler.Cnx();
		}

		public virtual void ResetBackoff()
		{
			ConnectionHandler.ResetBackoff();
		}

		public virtual void ConnectionClosed(ClientCnx Cnx)
		{
			ConnectionHandler.ConnectionClosed(Cnx);
		}

		public virtual ClientCnx ClientCnx
		{
			get
			{
				return ConnectionHandler.ClientCnx;
			}
			set
			{
				ConnectionHandler.ClientCnx = value;
			}
		}


		public virtual void ReconnectLater(System.Exception Exception)
		{
			ConnectionHandler.ReconnectLater(Exception);
		}

		public virtual void GrabCnx()
		{
			ConnectionHandler.GrabCnx();
		}

		public virtual Semaphore Semaphore
		{
			get
			{
				return semaphore;
			}
		}

		private static readonly ILogger log = new LoggerFactory().CreateLogger(typeof(ProducerImpl<T>));
	}

}