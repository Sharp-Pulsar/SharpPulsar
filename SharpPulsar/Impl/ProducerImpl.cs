using DotNetty.Buffers;
using DotNetty.Common;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Optional;
using Org.BouncyCastle.Crypto;
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
using System.Globalization;
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

	public sealed class ProducerImpl<T> : ProducerBase<T>, ITimerTask, IConnection
	{

		// Producer id, used to identify a producer within a single connection
        internal readonly long ProducerId;

		// Variable is used through the atomic updater
		public static ConcurrentDictionary<ProducerImpl<T>, long> MsgIdGenerator = new ConcurrentDictionary<ProducerImpl<T>, long>();

		private readonly BlockingQueue<OpSendMsg<T>> _pendingMessages;
		private readonly BlockingQueue<OpSendMsg<T>> _pendingCallbacks;
		private readonly Semaphore _semaphore;
		private  ITimeout _sendTimeout;
		private ITimeout _batchMessageAndSendTimeout = null;
		private readonly long _createProducerTimeout;
		private readonly BatchMessageContainerBase<T> _batchMessageContainer;
		private TaskCompletionSource<IMessageId> _lastSendTask = new TaskCompletionSource<IMessageId>();
		private readonly bool _userProvidedProducerName = false;

		private string _connectionId;
		private string _connectedSince;
		private readonly int _partitionIndex;

		private readonly CompressionCodec _compressor;

        internal long LastSequenceIdPushed;

		private readonly MessageCrypto _msgCrypto = null;
		private readonly IScheduledTask _keyGeneratorTask = null;

		private readonly IDictionary<string, string> _metadata;

		private sbyte[] _schemaVersion;

		public  ConnectionHandler ConnectionHandler;


		//private static readonly AtomicLongFieldUpdater<ProducerImpl> msgIdGeneratorUpdater = AtomicLongFieldUpdater.newUpdater(typeof(ProducerImpl), "msgIdGenerator");

		public ProducerImpl(PulsarClientImpl client, string topic, ProducerConfigurationData conf, TaskCompletionSource<IProducer<T>> producerCreatedTask, int partitionindex, ISchema<T> schema, ProducerInterceptors interceptors) : base(client, topic, conf, producerCreatedTask, schema, interceptors)
		{
			ProducerId = Client.NewProducerId();
			HandlerName = conf.ProducerName;
			if (!string.IsNullOrWhiteSpace(HandlerName))
			{
				_userProvidedProducerName = true;
			}
			_partitionIndex = partitionindex;
			_pendingMessages = new BlockingQueue<OpSendMsg<T>>(conf.MaxPendingMessages);
			_pendingCallbacks = new BlockingQueue<OpSendMsg<T>>(conf.MaxPendingMessages);
			_semaphore = new Semaphore(0, conf.MaxPendingMessages);

			_compressor = CompressionCodecProvider.GetCompressionCodec(Conf.CompressionType);
			if (Conf.InitialSequenceId != null)
			{
				var initialSequenceId = (long)Conf.InitialSequenceId;
				LastSequenceId = initialSequenceId;
				LastSequenceIdPushed = initialSequenceId;
				MsgIdGenerator.TryAdd(this, initialSequenceId + 1L);
			}
			else
			{
				LastSequenceId = -1L;
				LastSequenceIdPushed = -1L;
				MsgIdGenerator.TryAdd(this, 0L);
			}

			if (Conf.EncryptionEnabled)
			{
				var logCtx = "[" + Topic + "] [" + HandlerName + "] [" + ProducerId + "]";
				_msgCrypto = new MessageCrypto(logCtx, true);

				// Regenerate data key cipher at fixed interval
				_keyGeneratorTask = Client.EventLoopGroup.Schedule(() =>
				{
					try
					{
						_msgCrypto.AddPublicKeyCipher(Conf.EncryptionKeys, Conf.CryptoKeyReader);
					}
					catch (CryptoException e)
					{
						if (!producerCreatedTask.Task.IsCompletedSuccessfully)
						{
							Log.LogWarning("[{}] [{}] [{}] Failed to add public key cipher.", Topic, HandlerName, ProducerId);
							producerCreatedTask.SetException(PulsarClientException.Wrap(e,
                                $"The producer {HandlerName} of the topic {Topic} " +
                                "adds the public key cipher was failed"));
						}
					}
				}, TimeSpan.FromSeconds(30));

			}

			if (Conf.SendTimeoutMs > 0)
            {
                _sendTimeout = Client.Timer.NewTimeout(this, TimeSpan.FromTicks(Conf.SendTimeoutMs));
            }

			_createProducerTimeout = DateTimeHelper.CurrentUnixTimeMillis() + Client.Configuration.OperationTimeoutMs;
			if (Conf.BatchingEnabled)
			{
				var containerBuilder = Conf.BatcherBuilder ?? DefaultImplementation.NewDefaultBatcherBuilder<T>();
                _batchMessageContainer = (BatchMessageContainerBase<T>)containerBuilder.Build<T>();
				_batchMessageContainer.Producer = this;
			}
			else
			{
				_batchMessageContainer = null;
			}
			Stats = Client.Configuration.StatsIntervalSeconds > 0 ? new ProducerStatsRecorderImpl<T>(Client, Conf, this) : ProducerStatsDisabled<T>.Instance;

			if (!Conf.Properties.Any())
			{
				_metadata = new Dictionary<string,string>();
			}
			else
			{
				_metadata = new SortedDictionary<string, string>(Conf.Properties);
			}

			ConnectionHandler = new ConnectionHandler(this, new BackoffBuilder()
					.SetInitialTime(Client.Configuration.InitialBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).SetMax(Client.Configuration.MaxBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).SetMandatoryStop(Math.Max(100, Conf.SendTimeoutMs - 100), BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).Create(), this);

			GrabCnx();
		}


		private bool BatchMessagingEnabled => Conf.BatchingEnabled;

        private bool IsMultiSchemaEnabled(bool autoEnable)
		{
			if (MultiSchemaMode != MultiSchemaMode.Auto)
			{
				return MultiSchemaMode == MultiSchemaMode.Enabled;
			}

            if (!autoEnable) return false;
            MultiSchemaMode = MultiSchemaMode.Enabled;
            return true;
        }


		public override TaskCompletionSource<IMessageId> InternalSendAsync(Api.IMessage<T> message)
		{

			var task = new TaskCompletionSource<IMessageId>();
			var interceptorMessage = (MessageImpl<T>) BeforeSend(message);
			//Retain the buffer used by interceptors callback to get message. Buffer will release after complete interceptors.
			interceptorMessage.DataBuffer.Retain();
			if (Interceptors != null)
			{
				//InterceptorMessage.Properties;
			}
			SendAsync(interceptorMessage, new SendCallbackAnonymousInnerClass<T>(this, task, interceptorMessage));
			return task;
		}

		public class SendCallbackAnonymousInnerClass<TS> : SendCallback
		{
			private readonly ProducerImpl<TS> _outerInstance;

            private TaskCompletionSource<IMessageId> _task;
			private readonly MessageImpl<TS> _interceptorMessage;

			public SendCallbackAnonymousInnerClass(ProducerImpl<TS> outerInstance, TaskCompletionSource<IMessageId> task, MessageImpl<TS> interceptorMessage)
			{
				_outerInstance = outerInstance;
				_task = task;
				_interceptorMessage = interceptorMessage;
				NextCallback = null;
				NextMsg = null;
				CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
			}

			internal SendCallback NextCallback;
			internal MessageImpl<TS> NextMsg;
			internal long CreatedAt;

			public TaskCompletionSource<IMessageId> Task => _task;

            public SendCallback NextSendCallback => NextCallback;

            public MessageImpl<object> NextMessage => NextMsg;

            public void SendComplete(System.Exception e)
			{
				try
				{
					if (e != null)
					{
						//outerInstance.Stats.NumSendFailed++;
						_outerInstance.OnSendAcknowledgement(_interceptorMessage, null, e);
						_task.SetException(e);
					}
					else
					{
						_outerInstance.OnSendAcknowledgement(_interceptorMessage, _interceptorMessage.GetMessageId(), null);
						_task.SetResult(_interceptorMessage.MessageId);
						_outerInstance.Stats.IncrementNumAcksReceived(DateTime.Now.Millisecond - CreatedAt);
					}
				}
				finally
				{
					_interceptorMessage.DataBuffer.Release();
				}

				while (NextCallback != null)
				{
					var sendCallback = NextCallback;
					var msg = NextMsg;
					//Retain the buffer used by interceptors callback to get message. Buffer will release after complete interceptors.
					try
					{
						msg.DataBuffer.Retain();
						if (e != null)
						{
							_outerInstance.Stats.IncrementSendFailed();
							_outerInstance.OnSendAcknowledgement(msg, null, e);
							sendCallback.Task.SetException(e);
						}
						else
						{
							_outerInstance.OnSendAcknowledgement(msg, msg.MessageId, null);
							sendCallback.Task.SetResult(msg.MessageId);
							_outerInstance.Stats.IncrementNumAcksReceived(DateTime.Now.Millisecond - CreatedAt);
						}
						NextMsg = NextCallback.NextMessage;
						NextCallback = NextCallback.NextSendCallback;
					}
					finally
					{
						msg.DataBuffer.Release();
					}
				}
			}

			public void AddCallback<T1>(MessageImpl<T1> msg, SendCallback scb)
			{
				NextMsg = (MessageImpl<object>)msg;
				NextCallback = scb;
			}
		}

		public static implicit operator ProducerImpl<T>(ProducerImpl<object> v)
		{
			return (ProducerImpl<T>)Convert.ChangeType(v, typeof(ProducerImpl<T>));
		}
		public static implicit operator ProducerImpl<object>(ProducerImpl<T> v)
		{
			return (ProducerImpl<object>)Convert.ChangeType(v, typeof(ProducerImpl<object>));
		}
		public void SendAsync(Api.IMessage<T> message, SendCallback callback)
		{
			if (message is MessageImpl<T>)
				return;

			if (!IsValidProducerState(callback))
			{
				return;
			}

			if (!CanEnqueueRequest(callback))
			{
				return;
			}

			var msg = (MessageImpl<T>) message;
			var msgMetadataBuilder = msg.MessageBuilder;
			var payload = msg.DataBuffer;

			// If compression is enabled, we are compressing, otherwise it will simply use the same buffer
			var uncompressedSize = payload.ReadableBytes;
			var compressedPayload = payload;
			// Batch will be compressed when closed
			// If a message has a delayed delivery time, we'll always send it individually
			if (!BatchMessagingEnabled || msgMetadataBuilder.HasDeliverAtTime())
			{
				compressedPayload = _compressor.Encode(payload);
				payload.Release();

				// validate msg-size (For batching this will be check at the batch completion size)
				var compressedSize = compressedPayload.ReadableBytes;
				if (compressedSize > ClientCnx.MaxMessageSize)
				{
					compressedPayload.Release();
					var compressedStr = (!BatchMessagingEnabled && Conf.CompressionType != ICompressionType.None) ? "Compressed" : "";
					var invalidMessageException = new PulsarClientException.InvalidMessageException(string.Format("The producer %s of the topic %s sends a %s message with %d bytes that exceeds %d bytes", HandlerName, Topic, compressedStr, compressedSize, ClientCnx.MaxMessageSize));
					callback.SendComplete(invalidMessageException);
					return;
				}
			}

			if (!msg.Replicated && msgMetadataBuilder.HasProducerName())
			{
				var invalidMessageException = new PulsarClientException.InvalidMessageException(string.Format("The producer %s of the topic %s can not reuse the same message", HandlerName, Topic));
				callback.SendComplete(invalidMessageException);
				compressedPayload.Release();
				return;
			}

			if (!PopulateMessageSchema(msg, callback))
			{
				compressedPayload.Release();
				return;
			}

			try
			{
				lock (this)
				{
					long sequenceId;
					if (!msgMetadataBuilder.HasSequenceId())
					{
						sequenceId = MsgIdGenerator[this]; //msgIdGeneratorUpdater.getAndIncrement(this);
						MsgIdGenerator[this]++;
						msgMetadataBuilder.SetSequenceId(sequenceId);
					}
					else
					{
						sequenceId = msgMetadataBuilder.GetSequenceId();
					}
					if (!msgMetadataBuilder.HasPublishTime())
					{
						msgMetadataBuilder.SetPublishTime(Client.ClientClock.Millisecond);

						if(msgMetadataBuilder.HasProducerName())
							msgMetadataBuilder.SetProducerName(HandlerName);

						if (Conf.CompressionType != ICompressionType.None)
						{
							msgMetadataBuilder.SetCompression(CompressionCodecProvider.ConvertToWireProtocol(Conf.CompressionType));
						}
						msgMetadataBuilder.SetUncompressedSize(uncompressedSize);
					}
					if (CanAddToBatch(msg))
					{
						if (CanAddToCurrentBatch(msg))
						{
							// should trigger complete the batch message, new message will add to a new batch and new batch
							// sequence id use the new message, so that broker can handle the message duplication
							if (sequenceId <= LastSequenceIdPushed)
							{
								if (sequenceId <= LastSequenceId)
								{
									Log.LogWarning("Message with sequence id {} is definitely a duplicate", sequenceId);
								}
								else
								{
									Log.LogInformation("Message with sequence id {} might be a duplicate but cannot be determined at this time.", sequenceId);
								}
								DoBatchSendAndAdd(msg, callback, payload);
							}
							else
							{
								// handle boundary cases where message being added would exceed
								// batch size and/or max message size
								var isBatchFull = _batchMessageContainer.Add(msg, callback);
								_lastSendTask = callback.Task;
								payload.Release();
								if (isBatchFull)
								{
									BatchMessageAndSend();
								}
							}
						}
						else
						{
							DoBatchSendAndAdd(msg, callback, payload);
						}
					}
					else
					{
						var encryptedPayload = EncryptMessage(msgMetadataBuilder, compressedPayload);
						// When publishing during replication, we need to set the correct number of message in batch
						// This is only used in tracking the publish rate stats
						var numMessages = msg.MessageBuilder.HasNumMessagesInBatch() ? msg.MessageBuilder.NumMessagesInBatch : 1;
						OpSendMsg<T> op;
						var schemaState = (int)msg.GetSchemaState();
						if (schemaState  == 1)
						{
							var msgMetadata = msgMetadataBuilder.Build();
							var cmd = SendMessage(ProducerId, sequenceId, numMessages, msgMetadata, encryptedPayload);
							op = OpSendMsg<T>.Create(msg, cmd, sequenceId, callback);
							msgMetadataBuilder.Recycle();
							msgMetadata.Recycle();
						}
						else
						{
							op = OpSendMsg<T>.Create(msg, null, sequenceId, callback);
							op.RePopulate = () =>
							{
								var msgMetadata = msgMetadataBuilder.Build();
								op.Cmd = SendMessage(ProducerId, sequenceId, numMessages, msgMetadata, encryptedPayload);
								msgMetadataBuilder.Recycle();
								msgMetadata.Recycle();
							};
						}
						op.NumMessagesInBatch = numMessages;
						op.BatchSizeByte = encryptedPayload.ReadableBytes;
						_lastSendTask = callback.Task;
						ProcessOpSendMsg(op);
					}
				}
			}
			catch (PulsarClientException e)
			{
				_semaphore.Release();
				callback.SendComplete(e);
			}
			catch (System.Exception T)
			{
				_semaphore.Release();
				callback.SendComplete(new PulsarClientException(T.Message));
			}
		}

		private bool PopulateMessageSchema(MessageImpl<object> msg, SendCallback callback)
		{
			var msgMetadataBuilder = msg.MessageBuilder;
            var schemaHash = SchemaHash.Of(msg.Schema);
            var schemaVersion = SchemaCache[schemaHash];
			if (msg.Schema == Schema)
			{
				if(schemaVersion != null)
                    msgMetadataBuilder.SetSchemaVersion(ByteString.CopyFrom((byte[])(object)schemaVersion));
				msg.SetSchemaState(MessageImpl<object>.SchemaState.Ready);
				return true;
			}
			if (!IsMultiSchemaEnabled(true))
			{
				callback.SendComplete(new PulsarClientException.InvalidMessageException(string.Format("The producer %s of the topic %s is disabled the `MultiSchema`", HandlerName, Topic)));
				return false;
			}

            if (schemaVersion == null) return true;
            msgMetadataBuilder.SetSchemaVersion(ByteString.CopyFrom((byte[])(object)schemaVersion));
            msg.SetSchemaState(MessageImpl<object>.SchemaState.Ready);
            return true;
		}

		private bool RePopulateMessageSchema(MessageImpl<T> msg)
		{
			var schemaHash = SchemaHash.Of(msg.Schema);
			var schemaVersion = SchemaCache[schemaHash];
			if (schemaVersion == null)
			{
				return false;
			}
			msg.MessageBuilder.SetSchemaVersion(ByteString.CopyFrom((byte[])(object)schemaVersion));
			msg.SetSchemaState(MessageImpl<T>.SchemaState.Ready);
			return true;
		}

		private void TryRegisterSchema(ClientCnx cnx, MessageImpl<T> msg, SendCallback callback)
		{
			if (!ChangeToRegisteringSchemaState())
			{
				return;
			}

            SchemaInfo schemaInfo = null;
            if (msg.Schema != null && msg.Schema.SchemaInfo.Type.Value > 0)
            {
                schemaInfo = (SchemaInfo) msg.Schema.SchemaInfo;
            }
            else
            {
                schemaInfo = (SchemaInfo)SchemaFields.Bytes.SchemaInfo;

            }
            
			GetOrCreateSchemaAsync(cnx, schemaInfo).AsTask().ContinueWith(task =>
			{
				if (task.Exception != null)
				{
					Log.LogWarning("[{}] [{}] GetOrCreateSchema error", Topic, HandlerName, task.Exception);
                }
				else
				{
					Log.LogWarning("[{}] [{}] GetOrCreateSchema succeed", Topic, HandlerName);
					var schemaHash = SchemaHash.Of(msg.Schema);
                    if (!SchemaCache.ContainsKey(schemaHash))
                    {
                        SchemaCache[schemaHash] = task.Result;

                    }
					msg.MessageBuilder.SetSchemaVersion(ByteString.CopyFrom((byte[])(object)task.Result));
					msg.SetSchemaState(MessageImpl<T>.SchemaState.Ready);
				}
			});
			cnx.Ctx().Channel.EventLoop.Execute(() =>
			{
				lock (this)
				{
					RecoverProcessOpSendMsgFrom(cnx, msg);
				}
			});
		}

		private ValueTask<sbyte[]> GetOrCreateSchemaAsync(ClientCnx cnx, SchemaInfo schemaInfo)
		{
			if (!Commands.PeerSupportsGetOrCreateSchema(cnx.RemoteEndpointProtocolVersion))
			{
				return new ValueTask<sbyte[]>(Task.FromException<sbyte[]>(new PulsarClientException.NotSupportedException(string.Format("The command `GetOrCreateSchema` is not supported for the protocol version %d. " + "The producer is %s, topic is %s", cnx.RemoteEndpointProtocolVersion, HandlerName, Topic))));
			}
			var requestId = Client.NewRequestId();
			var request = Commands.NewGetOrCreateSchema(requestId, Topic, schemaInfo);
			Log.LogInformation("[{}] [{}] GetOrCreateSchema request", Topic, HandlerName);
			return cnx.SendGetOrCreateSchema(request, requestId);
		}

		public IByteBuffer EncryptMessage(MessageMetadata.Builder msgMetadata, IByteBuffer compressedPayload)
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
                if (Conf.CryptoFailureAction != ProducerCryptoFailureAction.Send) throw e;
                Log.LogWarning("[{}] [{}] Failed to encrypt message {}. Proceeding with publishing unencrypted message", Topic, HandlerName, e.Message);
                return compressedPayload;
            }
			return encryptedPayload;
		}

		public ByteBufPair SendMessage(long producerId, long sequenceId, int numMessages, MessageMetadata msgMetadata, IByteBuffer compressedPayload)
		{
			return Commands.NewSend(producerId, sequenceId, numMessages, ChecksumType, msgMetadata, compressedPayload);
		}

		public ByteBufPair SendMessage(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, MessageMetadata msgMetadata, IByteBuffer compressedPayload)
		{
			return Commands.NewSend(producerId, lowestSequenceId, highestSequenceId, numMessages, ChecksumType, msgMetadata, compressedPayload);
		}

		private Commands.ChecksumType ChecksumType
		{
			get
			{
				if (ConnectionHandler.ClientCnx == null || ConnectionHandler.ClientCnx.RemoteEndpointProtocolVersion >= BrokerChecksumSupportedVersion())
				{
					return Commands.ChecksumType.Crc32C;
				}
				else
				{
					return Commands.ChecksumType.None;
				}
			}
		}

		private bool CanAddToBatch(MessageImpl<T> msg)
		{
			return msg.GetSchemaState() == MessageImpl<T>.SchemaState.Ready && BatchMessagingEnabled && !msg.MessageBuilder.HasDeliverAtTime();
		}

		private bool CanAddToCurrentBatch(MessageImpl<T> msg)
		{
			return _batchMessageContainer.HaveEnoughSpace(msg) && (!IsMultiSchemaEnabled(false) || _batchMessageContainer.HasSameSchema(msg));
		}

		private void DoBatchSendAndAdd(MessageImpl<T> msg, SendCallback callback, IReferenceCounted payload)
		{
			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("[{}] [{}] Closing out batch to accommodate large message with size {}", Topic, HandlerName, msg.DataBuffer.ReadableBytes);
			}
			try
			{
				BatchMessageAndSend();
				_batchMessageContainer.Add(msg, callback);
				_lastSendTask = callback.Task;
			}
			finally
			{
				payload.Release();
			}
		}

		private bool IsValidProducerState(SendCallback callback)
		{
			switch (GetState())
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
					callback.SendComplete(new PulsarClientException.AlreadyClosedException("Producer already closed"));
					return false;
				case State.Terminated:
					callback.SendComplete(new PulsarClientException.TopicTerminatedException("Topic was terminated"));
					return false;
				case State.Failed:
				case State.Uninitialized:
				default:
					callback.SendComplete(new PulsarClientException.NotConnectedException());
					return false;
			}
		}

		private bool CanEnqueueRequest(SendCallback callback)
		{
			try
			{
				if (Conf.BlockIfQueueFull)
				{
					_semaphore.WaitOne();
				}
				else
				{
					if (!_semaphore.WaitOne())
					{
						callback.SendComplete(new PulsarClientException.ProducerQueueIsFullError("Producer send queue is full"));
						return false;
					}
				}
				_semaphore.Release();
			}
			catch (System.Exception e)
			{
				Thread.CurrentThread.Interrupt();
				callback.SendComplete(e);
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

			internal static WriteInEventLoopCallback Create<T1>(ProducerImpl<T1> producer, ClientCnx cnx, OpSendMsg<T> op)
			{
				var c = Pool.Take();
				c.Producer = producer;
				c.Cnx = cnx;
				c.SequenceId = op.SequenceId;
				c.Cmd = op.Cmd;
				return c;
			}

			public void Run()
			{
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] [{}] Sending message cnx {}, sequenceId {}", Producer.Topic, Producer.HandlerName, Cnx, SequenceId);
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
				Handle.Release(this);
			}

			internal static ThreadLocalPool<WriteInEventLoopCallback> Pool = new ThreadLocalPool<WriteInEventLoopCallback>(handle => new WriteInEventLoopCallback(handle), 1, true);

			internal ThreadLocalPool.Handle Handle;
			private WriteInEventLoopCallback(ThreadLocalPool.Handle handle)
			{
				Handle = handle;
			}
		}

		public override ValueTask CloseAsync()
		{
			State? currentState = GetAndUpdateState(state1 => state1 == State.Closed ? state1 : State.Closing);

			if (currentState == State.Closed || currentState == State.Closing)
			{
				return new ValueTask(Task.CompletedTask);
			}

			var timeout = _sendTimeout;
			if (timeout != null)
			{
				timeout.Cancel();
				_sendTimeout = null;
			}

			var batchTimeout = _batchMessageAndSendTimeout;
			if (batchTimeout != null)
			{
				batchTimeout.Cancel();
				_batchMessageAndSendTimeout = null;
			}

			if (_keyGeneratorTask != null && !_keyGeneratorTask.Cancel())
			{
				_keyGeneratorTask.Cancel();
			}

			Stats.CancelStatsTimeout();

			var cnx = ConnectionHandler.Cnx();
			if (cnx == null || currentState != State.Ready)
			{
				Log.LogInformation("[{}] [{}] Closed Producer (not connected)", Topic, HandlerName);
				lock (this)
				{
					ChangeToState(State.Closed);
					Client.CleanupProducer(this);
					PulsarClientException ex = new PulsarClientException.AlreadyClosedException(string.Format("The producer %s of the topic %s was already closed when closing the producers", HandlerName, Topic));
					_pendingMessages.ToList().ForEach(msg =>
					{
						msg.Callback.SendComplete(ex);
						msg.Cmd.Release();
						msg.Recycle();
					});
					_pendingMessages.Clear();
				}

				return new ValueTask(Task.CompletedTask);
			}

			var requestId = Client.NewRequestId();
			var cmd = Commands.NewCloseProducer(ProducerId, requestId);

			var closeTask = new TaskCompletionSource();
			cnx.SendRequestWithId(cmd, requestId).AsTask().ContinueWith((v) =>
			{
				cnx.RemoveProducer(ProducerId);
				if (v.Exception == null || !cnx.Ctx().Channel.Active)
				{
					lock (this)
					{
						Log.LogInformation("[{}] [{}] Closed Producer", Topic, HandlerName);
                        ChangeToState(State.Closed);
						_pendingMessages.ToList().ForEach(msg =>
						{
							msg.Cmd.Release();
							msg.Recycle();
						});
						_pendingMessages.Clear();
					}
					closeTask.SetResult(-1);
						Client.CleanupProducer(this);
				}
				else
				{
					closeTask.SetException(v.Exception);
				}
			});

			return new ValueTask(closeTask.Task);
		}

		public override bool Connected
        {
            get => ConnectionHandler.ClientCnx != null && (GetState() == State.Ready);
            set => ChangeToState(State.Closed);
        }

        public bool Writable
		{
			get
			{
				var cnx = ConnectionHandler.ClientCnx;
				return cnx != null && cnx.Channel().IsWritable;
			}
		}

		public void Terminated(ClientCnx cnx)
		{
			var previousState = GetAndUpdateState(state1 => (state1 == State.Closed ? State.Closed : State.Terminated));
			if (previousState != State.Terminated && previousState != State.Closed)
			{
				Log.LogInformation("[{}] [{}] The topic has been terminated", Topic, HandlerName);
				ClientCnx = null;

				FailPendingMessages(cnx, new PulsarClientException.TopicTerminatedException(string.Format("The topic %s that the producer %s produces to has been terminated", Topic, HandlerName)));
			}
		}

		public void AckReceived(ClientCnx cnx, long sequenceId, long highestSequenceId, long ledgerId, long entryId)
		{
			OpSendMsg<T> op = null;
			var callback = false;
			lock (this)
			{
				op = _pendingMessages.Peek();
				if (op == null)
				{
					if (Log.IsEnabled(LogLevel.Debug))
					{
						Log.LogDebug("[{}] [{}] Got ack for timed out msg {} - {}", Topic, HandlerName, sequenceId, highestSequenceId);
					}
					return;
				}

				if (sequenceId > op.SequenceId)
				{
					Log.LogWarning("[{}] [{}] Got ack for msg. expecting: {} - {} - got: {} - {} - queue-size: {}", Topic, HandlerName, op.SequenceId, op.HighestSequenceId, sequenceId, highestSequenceId, _pendingMessages.Count);
					// Force connection closing so that messages can be re-transmitted in a new connection
					cnx.Channel().CloseAsync();
				}
				else if (sequenceId < op.SequenceId)
				{
					// Ignoring the ack since it's referring to a message that has already timed out.
					if (Log.IsEnabled(LogLevel.Debug))
					{
						Log.LogDebug("[{}] [{}] Got ack for timed out msg. expecting: {} - {} - got: {} - {}", Topic, HandlerName, op.SequenceId, op.HighestSequenceId, sequenceId, highestSequenceId);
					}
				}
				else
				{
					// Add check `sequenceId >= highestSequenceId` for backward compatibility.
					if (sequenceId >= highestSequenceId || highestSequenceId == op.HighestSequenceId)
					{
						// Message was persisted correctly
						if (Log.IsEnabled(LogLevel.Debug))
						{
							Log.LogDebug("[{}] [{}] Received ack for msg {} ", Topic, HandlerName, sequenceId);
						}
						_pendingMessages.Dequeue();
						ReleaseSemaphoreForSendOp(op);
						callback = true;
						_pendingCallbacks.Enqueue(op);
					}
					else
					{
						Log.LogWarning("[{}] [{}] Got ack for batch msg error. expecting: {} - {} - got: {} - {} - queue-size: {}", Topic, HandlerName, op.SequenceId, op.HighestSequenceId, sequenceId, highestSequenceId, _pendingMessages.Count);
						// Force connection closing so that messages can be re-transmitted in a new connection
						cnx.Channel().CloseAsync();
					}
				}
			}
			if (callback)
			{
				op = _pendingCallbacks.Peek();
                if (op == null) return;
                LastSequenceId = Math.Max(LastSequenceId, GetHighestSequenceId(op));
                op.SetMessageId(ledgerId, entryId, _partitionIndex);
                try
                {
                    // Need to protect ourselves from any exception being thrown in the future handler from the
                    // application
                    op.Callback.SendComplete(null);
                }
                catch (System.Exception T)
                {
                    Log.LogWarning("[{}] [{}] Got exception while completing the callback for msg {}:", Topic, HandlerName, sequenceId, T);
                }
                op.Cmd.SafeRelease();
                op.Recycle();
            }
		}

		private long GetHighestSequenceId(OpSendMsg<T> op)
		{
			return Math.Max(op.HighestSequenceId, op.SequenceId);
		}

		private void ReleaseSemaphoreForSendOp(OpSendMsg<T> op)
		{
			_semaphore.Release(BatchMessagingEnabled ? op.NumMessagesInBatch : 1);
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
		public void RecoverChecksumError(ClientCnx cnx, long sequenceId)
		{
			lock (this)
			{
				var op = _pendingMessages.Peek();
				if (op == null)
				{
					if (Log.IsEnabled(LogLevel.Debug))
					{
						Log.LogDebug("[{}] [{}] Got send failure for timed out msg {}", Topic, HandlerName, sequenceId);
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
							_pendingMessages.Dequeue();
							ReleaseSemaphoreForSendOp(op);
							try
							{
								op.Callback.SendComplete(new PulsarClientException.ChecksumException(string.Format("The checksum of the message which is produced by producer %s to the topic " + "%s is corrupted", HandlerName, Topic)));
							}
							catch (System.Exception T)
							{
								Log.LogWarning("[{}] [{}] Got exception while completing the callback for msg {}:", Topic, HandlerName, sequenceId, T);
							}
							op.Cmd.SafeRelease();
							op.Recycle();
							return;
						}
						else
						{
							if (Log.IsEnabled(LogLevel.Debug))
							{
								Log.LogDebug("[{}] [{}] Message is not corrupted, retry send-message with sequenceId {}", Topic, HandlerName, sequenceId);
							}
						}
        
					}
					else
					{
						if (Log.IsEnabled(LogLevel.Debug))
						{
							Log.LogDebug("[{}] [{}] Corrupt message is already timed out {}", Topic, HandlerName, sequenceId);
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
		public bool VerifyLocalBufferIsNotCorrupted(OpSendMsg<T> op)
		{
			var msg = op.Cmd;

			if (msg != null)
			{
				var headerFrame = msg.First;
				headerFrame.MarkReaderIndex();
				try
				{
					// skip bytes up to checksum index
					headerFrame.SkipBytes(4); // skip [total-size]
					var cmdSize = (int) headerFrame.ReadUnsignedInt();
					headerFrame.SkipBytes(cmdSize);
					// verify if checksum present
					if (Commands.HasChecksum(headerFrame))
					{
						var checksum = Commands.ReadChecksum(headerFrame);
						// msg.readerIndex is already at header-payload index, Recompute checksum for headers-payload
						int metadataChecksum = Commands.ComputeChecksum(headerFrame);
						long computedChecksum = Commands.ResumeChecksum(metadataChecksum, msg.Second);
						return checksum == computedChecksum;
					}
					else
					{
						Log.LogWarning("[{}] [{}] checksum is not present into message with id {}", Topic, HandlerName, op.SequenceId);
					}
				}
				finally
				{
					headerFrame.ResetReaderIndex();
				}
				return true;
			}
			else
			{
				Log.LogWarning("[{}] Failed while casting {} into ByteBufPair", HandlerName, (op.Cmd == null ? null : op.Cmd.GetType().FullName));
				return false;
			}
		}

        public void ConnectionOpened(ClientCnx cnx)
		{
			// we set the cnx reference before registering the producer on the cnx, so if the cnx breaks before creating the
			// producer, it will try to grab a new cnx
			ConnectionHandler.ClientCnx = cnx;
			cnx.RegisterProducer(ProducerId, this);

			Log.LogInformation("[{}] [{}] Creating producer on cnx {}", Topic, HandlerName, cnx.Ctx().Channel);

			var requestId = Client.NewRequestId();

			SchemaInfo schemaInfo = null;
            if (Schema?.SchemaInfo != null)
            {
                if (Schema.SchemaInfo.Type == SchemaType.Json)
                {
                    // for backwards compatibility purposes
                    // JSONSchema originally generated a schema for pojo based of of the JSON schema standard
                    // but now we have standardized on every schema to generate an Avro based schema
                    if (Commands.PeerSupportJsonSchemaAvroFormat(cnx.RemoteEndpointProtocolVersion))
                    {
                        schemaInfo = (SchemaInfo)Schema.SchemaInfo;
                    }
                    else if (Schema is JsonSchema<T> jsonSchema)
                    {
                        schemaInfo = jsonSchema.BackwardsCompatibleJsonSchemaInfo;
                    }
                    else
                    {
                        schemaInfo = (SchemaInfo)Schema.SchemaInfo;
                    }
                }
                else if (Schema.SchemaInfo.Type == SchemaType.BYTES || Schema.SchemaInfo.Type == SchemaType.NONE)
                {
                    // don't set schema info for Schema.BYTES
                    schemaInfo = null;
                }
                else
                {
                    schemaInfo = (SchemaInfo)Schema.SchemaInfo;
                }
            }

            cnx.SendRequestWithId(Commands.NewProducer(Topic, ProducerId, requestId, HandlerName, Conf.EncryptionEnabled, _metadata, schemaInfo, ConnectionHandler.Epoch, _userProvidedProducerName), requestId)
		    .AsTask().ContinueWith(task =>
			{
                if (task.IsFaulted)
                {
                    System.Exception cause = task.Exception;
                    cnx.RemoveProducer(ProducerId);
                    if (GetState() == State.Closing || GetState() == State.Closed)
                    {
                        cnx.Channel().CloseAsync();
                        //return null;
                    }
                    Log.LogError("[{}] [{}] Failed to create producer: {}", Topic, HandlerName, cause.Message);
                    if (cause is PulsarClientException.ProducerBlockedQuotaExceededException)
                    {
                        lock (this)
                        {
                            Log.LogWarning("[{}] [{}] Topic backlog quota exceeded. Throwing Exception on producer.", Topic, HandlerName);
                            if (Log.IsEnabled(LogLevel.Debug))
                            {
                                Log.LogDebug("[{}] [{}] Pending messages: {}", Topic, HandlerName, _pendingMessages.Count);
                            }
                            PulsarClientException bqe = new PulsarClientException.ProducerBlockedQuotaExceededException(string.Format("The backlog quota of the topic %s that the producer %s produces to is exceeded", Topic, HandlerName));
                            FailPendingMessages(cnx, bqe);
                        }
                    }
                    else if (cause is PulsarClientException.ProducerBlockedQuotaExceededError)
                    {
                        Log.LogWarning("[{}] [{}] Producer is blocked on creation because backlog exceeded on topic.", HandlerName, Topic);
                    }
                    if (cause is PulsarClientException.TopicTerminatedException)
                    {
                        ChangeToState(State.Terminated);
                        FailPendingMessages(cnx, (PulsarClientException)cause);
                        ProducerCreatedTask.SetException(cause);
                        Client.CleanupProducer(this);
                    }
                    else if (ProducerCreatedTask.Task.IsCompletedSuccessfully || (cause is PulsarClientException exception && ConnectionHandler.IsRetriableError(exception) && DateTimeHelper.CurrentUnixTimeMillis() < _createProducerTimeout))
                    {
                        ReconnectLater(cause);
                    }
                    else
                    {
                        ChangeToState(State.Failed);
                        ProducerCreatedTask.SetException(cause);
                        Client.CleanupProducer(this);
                    }
                    return;
				}
				var response = task.Result;
				var producerName = response.ProducerName;
				var lastSequenceId = response.LastSequenceId;
				var schemaVersion = response.SchemaVersion;
				if(schemaVersion != null)
				{
					SchemaCache.TryAdd(SchemaHash.Of(Schema), schemaVersion);
				}
				
				lock (this)
				{
					if (GetState() == State.Closing || GetState() == State.Closed)
					{
						cnx.RemoveProducer(ProducerId);
						cnx.Channel().CloseAsync();
						return;
					}
					ResetBackoff();
					Log.LogInformation("[{}] [{}] Created producer on cnx {}", Topic, producerName, cnx.Ctx().Channel);
					_connectionId = cnx.Ctx().Channel.ToString();
					_connectedSince = DateTime.Now.ToString(CultureInfo.InvariantCulture);
					if (HandlerName is null)
					{
					
						HandlerName = producerName;
					}
					if (MsgIdGenerator[this] == 0 && Conf.InitialSequenceId == null)
					{
						LastSequenceId = lastSequenceId;
						MsgIdGenerator[this] = LastSequenceId + 1;
					}
					if (!ProducerCreatedTask.Task.IsCompletedSuccessfully && BatchMessagingEnabled)
                    {
                        Client.Timer.NewTimeout(new TimerTaskAnonymousInnerClass(this), TimeSpan.FromTicks(Conf.BatchingMaxPublishDelayMicros));
                    }
					ResendMessages(cnx);
				}
			});
		}
	
		public void ConnectionFailed(PulsarClientException exception)
		{
            if (DateTimeHelper.CurrentUnixTimeMillis() <= _createProducerTimeout ||
                !ProducerCreatedTask.TrySetException(exception)) return;
            Log.LogInformation("[{}] Producer creation failed for producer {}", Topic, ProducerId);
           ChangeToState(State.Failed);
            Client.CleanupProducer(this);
        }

		private void ResendMessages(ClientCnx cnx)
		{
			cnx.Ctx().Channel.EventLoop.Execute(() =>
			{
			lock (this)
			{
				if (GetState() == State.Closing || GetState() == State.Closed)
				{
					cnx.Channel().CloseAsync();
					return;
				}
				var messagesToResend = _pendingMessages.Count();
				if (messagesToResend == 0)
				{
					if (Log.IsEnabled(LogLevel.Debug))
					{
						Log.LogDebug("[{}] [{}] No pending messages to resend {}", Topic, HandlerName, messagesToResend);
					}
					if (ChangeToReadyState())
					{
						ProducerCreatedTask.SetResult(this);
						return;
					}
					else
					{
						cnx.Channel().CloseAsync();
						return;
					}
				}
				Log.LogInformation("[{}] [{}] Re-Sending {} messages to server", Topic, HandlerName, messagesToResend);
				RecoverProcessOpSendMsgFrom(cnx, null);
			}
			});
		}

		/// <summary>
		/// Strips checksum from <seealso cref="OpSendMsg"/> command if present else ignore it.
		/// </summary>
		/// <param name="op"> </param>
		private void StripChecksum(OpSendMsg<T> op)
		{
			var totalMsgBufSize = op.Cmd.ReadableBytes();
			var msg = op.Cmd;
			if (msg != null)
			{
				var headerFrame = msg.First;
				headerFrame.MarkReaderIndex();
				try
				{
					headerFrame.SkipBytes(4); // skip [total-size]
					var cmdSize = (int) headerFrame.ReadUnsignedInt();

					// verify if checksum present
					headerFrame.SkipBytes(cmdSize);

					if (!Commands.HasChecksum(headerFrame))
					{
						return;
					}

					var headerSize = 4 + 4 + cmdSize; // [total-size] [cmd-length] [cmd-size]
					var checksumSize = 4 + 2; // [magic-number] [checksum-size]
					var checksumMark = (headerSize + checksumSize); // [header-size] [checksum-size]
					var metaPayloadSize = (totalMsgBufSize - checksumMark); // metadataPayload = totalSize - checksumMark
					var newTotalFrameSizeLength = 4 + cmdSize + metaPayloadSize; // new total-size without checksum
					headerFrame.ResetReaderIndex();
					var headerFrameSize = headerFrame.ReadableBytes;

					headerFrame.SetInt(0, newTotalFrameSizeLength); // rewrite new [total-size]
					var metadata = headerFrame.Slice(checksumMark, headerFrameSize - checksumMark); // sliced only
																										// metadata
					headerFrame.SetWriterIndex(headerSize); // set headerFrame write-index to overwrite metadata over checksum
					metadata.ReadBytes(headerFrame, metadata.ReadableBytes);
					headerFrame.AdjustCapacity(headerFrameSize - checksumSize); // reduce capacity by removed checksum bytes
				}
				finally
				{
					headerFrame.ResetReaderIndex();
				}
			}
			else
			{

				Log.LogWarning("[{}] Failed while casting {} into ByteBufPair", HandlerName, (op.Cmd == null ? null : op.Cmd.GetType().FullName));
			}
		}

		public int BrokerChecksumSupportedVersion()
		{
			return (int)ProtocolVersion.V6;
		}


		/// <summary>
		/// Process sendTimeout events
		/// </summary>
		public void Run(ITimeout timeout)
		{
			if (timeout.Canceled)
			{
				return;
			}

			long timeToWaitMs;

			lock (this)
			{
				// If it's closing/closed we need to ignore this timeout and not schedule next timeout.
				if (GetState() == State.Closing || GetState() == State.Closed)
				{
					return;
				}

				var firstMsg = _pendingMessages.Peek();
				if (firstMsg == null)
				{
					// If there are no pending messages, reset the timeout to the configured value.
					timeToWaitMs = Conf.SendTimeoutMs;
				}
				else
				{
					// If there is at least one message, calculate the diff between the message timeout and the current
					// time.
					var diff = (firstMsg.CreatedAt + Conf.SendTimeoutMs) - DateTimeHelper.CurrentUnixTimeMillis();
					if (diff <= 0)
					{
						// The diff is less than or equal to zero, meaning that the message has been timed out.
						// Set the callback to timeout on every message, then clear the pending queue.
						Log.LogInformation("[{}] [{}] Message send timed out. Failing {} messages", Topic, HandlerName, _pendingMessages.Count);

						PulsarClientException te = new PulsarClientException.TimeoutException(string.Format("The producer %s can not send message to the topic %s within given timeout", HandlerName, Topic));
						FailPendingMessages(Cnx(), te);
						Stats.IncrementSendFailed(_pendingMessages.Count);
						// Since the pending queue is cleared now, set timer to expire after configured value.
						timeToWaitMs = Conf.SendTimeoutMs;
					}
					else
					{
						// The diff is greater than zero, set the timeout to the diff value
						timeToWaitMs = diff;
					}
				}

				_sendTimeout = Client.Timer.NewTimeout(this, TimeSpan.FromTicks(timeToWaitMs));
			}
		}

		/// <summary>
		/// This fails and clears the pending messages with the given exception. This method should be called from within the
		/// ProducerImpl object mutex.
		/// </summary>
		private void FailPendingMessages(ClientCnx cnx, PulsarClientException ex)
		{
			if (cnx == null)
			{
				var releaseCount = new AtomicInt();
				var batchMessagingEnabled = BatchMessagingEnabled;
				_pendingMessages.ToList().ForEach(op =>
				{
						releaseCount.AddAndGet(batchMessagingEnabled ? op.NumMessagesInBatch: 1);
						try
						{
							op.Callback.SendComplete(ex);
						}
						catch (System.Exception T)
						{
							Log.LogWarning("[{}] [{}] Got exception while completing the callback for msg {}:", Topic, HandlerName, op.SequenceId, T);
						}
						op.Cmd.SafeRelease();
						op.Recycle();
				});

				_pendingMessages.Clear();
				_pendingCallbacks.Clear();
				_semaphore.Release(releaseCount.Get());
				if (batchMessagingEnabled)
				{
					FailPendingBatchMessages(ex);
				}

			}
			else
			{
				// If we have a connection, we schedule the callback and recycle on the event loop thread to avoid any
				// race condition since we also write the message on the socket from this thread
				cnx.Ctx().Channel.EventLoop.Execute(() =>
				{
					lock(this)
					{
						FailPendingMessages(null, ex);
					}
				});
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
			_semaphore.Release(numMessagesInBatch);
		}

		public class TimerTaskAnonymousInnerClass: ITimerTask
        {
            private readonly ProducerImpl<T> _outerInstance;

            public TimerTaskAnonymousInnerClass(ProducerImpl<T> outerInstance)
            {
                _outerInstance = outerInstance;
            }
			
			public void Run(ITimeout timeout)
			{
                if (timeout.Canceled)
                {
                    return;
                }
                if (Log.IsEnabled(LogLevel.Trace))
                {
                    Log.LogTrace("[{}] [{}] Batching the messages from the batch container from timer thread", _outerInstance.Topic, _outerInstance.HandlerName);
                }
                // semaphore acquired when message was enqueued to container
                lock (_outerInstance)
                {
                    // If it's closing/closed we need to ignore the send batch timer and not schedule next timeout.
                    if (_outerInstance.GetState() == State.Closing || _outerInstance.GetState() == State.Closed)
                    {
                        return;
                    }

                    _outerInstance.BatchMessageAndSend();
                    // schedule the next batch message task
                    _outerInstance._batchMessageAndSendTimeout = _outerInstance.Client.Timer.NewTimeout(this, TimeSpan.FromTicks(_outerInstance.Conf.BatchingMaxPublishDelayMicros));
                }
			}
		}

		public override ValueTask FlushAsync()
		{
			lock (this)
			{
				if (BatchMessagingEnabled)
				{
					BatchMessageAndSend();
				}
				//LastSend = lastSendTask;
			}
			//return LastSendTask.thenApply(ignored => null);
			return new ValueTask(Task.CompletedTask);
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
			if (Log.IsEnabled(LogLevel.Trace))
			{
				Log.LogTrace("[{}] [{}] Batching the messages from the batch container with {} messages", Topic, HandlerName, _batchMessageContainer.NumMessagesInBatch);
			}
			if (!_batchMessageContainer.Empty)
			{
				try
				{
					IList<OpSendMsg<T>> opSendMsgs;
					opSendMsgs = _batchMessageContainer.MultiBatches ? _batchMessageContainer.CreateOpSendMsgs() : new List<OpSendMsg<T>>{ _batchMessageContainer.CreateOpSendMsg()};
					_batchMessageContainer.Clear();
					foreach (var opSendMsg in opSendMsgs)
					{
						ProcessOpSendMsg(opSendMsg);
					}
				}
				catch (PulsarClientException)
				{
					Thread.CurrentThread.Interrupt();
					_semaphore.Release(_batchMessageContainer.NumMessagesInBatch);
				}
				catch (System.Exception T)
				{
					_semaphore.Release(_batchMessageContainer.NumMessagesInBatch);
					Log.LogWarning("[{}] [{}] error while create opSendMsg by batch message container", Topic, HandlerName, T);
				}
			}
		}

		private void ProcessOpSendMsg(OpSendMsg<T> op)
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
				_pendingMessages.Enqueue(op);
				if (op.Msg != null)
				{
					LastSequenceIdPushed = Math.Max(LastSequenceIdPushed, GetHighestSequenceId(op));
				}
				var cnx = ConnectionHandler.Cnx();
				if (Connected)
				{
					if (op.Msg != null && op.Msg.GetSchemaState() == 0)
					{
						TryRegisterSchema(cnx, op.Msg, op.Callback);
						return;
					}
					// If we do have a connection, the message is sent immediately, otherwise we'll try again once a new
					// connection is established
					op.Cmd.Retain();
					cnx.Ctx().Channel.EventLoop.Execute(WriteInEventLoopCallback.Create(this, cnx, op));
					Stats.UpdateNumMsgsSent(op.NumMessagesInBatch, op.BatchSizeByte);
				}
				else
				{
					if (Log.IsEnabled(LogLevel.Debug))
					{
						Log.LogDebug("[{}] [{}] Connection is not ready -- sequenceId {}", Topic, HandlerName, op.SequenceId);
					}
				}
			}
			catch (ThreadInterruptedException ie)
			{
				Thread.CurrentThread.Interrupt();
				ReleaseSemaphoreForSendOp(op);
                op.Callback.SendComplete(new PulsarClientException(ie.Message));
            }
			catch (System.Exception T)
			{
				ReleaseSemaphoreForSendOp(op);
				Log.LogWarning("[{}] [{}] error while closing out batch -- {}", Topic, HandlerName, T);
                op.Callback.SendComplete(new PulsarClientException(T.Message));
            }
		}

		private void RecoverProcessOpSendMsgFrom(ClientCnx cnx, MessageImpl<object> @from)
		{
			var stripChecksum = cnx.RemoteEndpointProtocolVersion < BrokerChecksumSupportedVersion();
            using var msgIterator = _pendingMessages.GetEnumerator();
			OpSendMsg<T> pendingRegisteringOp = null;
			while (msgIterator.MoveNext())
			{
				var op = msgIterator.Current;
				if (@from != null)
				{
					if (op != null && op.Msg == (MessageImpl<T>) @from)
					{
						@from = null;
					}
					else
					{
						continue;
					}
				}
				if (op?.Msg != null)
				{
					if (op.Msg.GetSchemaState() == 0)
					{
						if (!RePopulateMessageSchema(op.Msg))
						{
							pendingRegisteringOp = op;
							break;
						}
					}
					else if (op != null && (int)op.Msg.GetSchemaState() ==  2)
					{
						op.Recycle();
						msgIterator.Remove(msgIterator.Current);
						continue;
					}
				}
				if (op.Cmd == null)
				{
					if(op.RePopulate == null)
                    {
                        op.RePopulate?.Invoke();
                    }
                }
				if (stripChecksum)
				{
					StripChecksum(op);
				}

                if (op.Cmd != null)
                {
                    op.Cmd.Retain();
                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.LogDebug("[{}] [{}] Re-Sending message in cnx {}, sequenceId {}", Topic, HandlerName,
                            cnx.Channel(), op.SequenceId);
                    }

                    cnx.Ctx().WriteAsync(op.Cmd);
                }

                Stats.UpdateNumMsgsSent(op.NumMessagesInBatch, op.BatchSizeByte);
			}
			cnx.Ctx().Flush();
			if (!ChangeToReadyState())
			{
				// Producer was closed while reconnecting, close the connection to make sure the broker
				// drops the producer on its side
				cnx.Channel().CloseAsync();
				return;
			}
			if (pendingRegisteringOp != null)
			{
				TryRegisterSchema(cnx, pendingRegisteringOp.Msg, pendingRegisteringOp.Callback);
			}
		}

		public long DelayInMillis
		{
			get
			{
				var firstMsg = _pendingMessages.Peek();
				if (firstMsg != null)
				{
					return DateTimeHelper.CurrentUnixTimeMillis() - firstMsg.CreatedAt;
				}
				return 0L;
			}
		}

		public string ConnectionId => Cnx() != null ? _connectionId : null;

        public string ConnectedSince => Cnx() != null ? _connectedSince : null;

        public int PendingQueueSize => _pendingMessages.Count;


        public override string ProducerName
        {
            get => HandlerName;
            set => HandlerName = value;
        }

        // wrapper for connection methods
		public ClientCnx Cnx()
		{
			return ConnectionHandler.Cnx();
		}

		public void ResetBackoff()
		{
			ConnectionHandler.ResetBackoff();
		}

		public void ConnectionClosed(ClientCnx cnx)
		{
			ConnectionHandler.ConnectionClosed(cnx);
		}

		public ClientCnx ClientCnx
		{
			get => ConnectionHandler.ClientCnx;
            set => ConnectionHandler.ClientCnx = value;
        }


		public void ReconnectLater(System.Exception exception)
		{
			ConnectionHandler.ReconnectLater(exception);
		}

		public void GrabCnx()
		{
			ConnectionHandler.GrabCnx();
		}

		public Semaphore Semaphore => _semaphore;

        private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(ProducerImpl<T>));
	}

}