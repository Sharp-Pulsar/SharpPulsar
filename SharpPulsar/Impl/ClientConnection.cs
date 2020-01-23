using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using SharpPulsar;
using SharpPulsar.Common.Protocol;
using SharpPulsar.Common.PulsarApi;
using SharpPulsar.Configuration;
using SharpPulsar.Entity;
using SharpPulsar.Exception;
using SharpPulsar.Interface.Auth;
using SharpPulsar.Util;
using SharpPulsar.Util.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static SharpPulsar.Impl.BinaryProtoLookupService;

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
	
	public class ClientConnection
	{

		protected internal readonly IAuthentication _authentication;
		private State _state;

		private readonly ConcurrentLongHashMap<ValueTask<ProducerResponse>> _pendingRequests = new ConcurrentLongHashMap<ValueTask<ProducerResponse>>(16, 1);
		private readonly ConcurrentLongHashMap<ValueTask<LookupDataResult>> _pendingLookupRequests = new ConcurrentLongHashMap<ValueTask<LookupDataResult>>(16, 1);
		// LookupRequests that waiting in client side.
		private readonly LinkedList<KeyValuePair<long, KeyValuePair<IByteBuffer, ValueTask<LookupDataResult>>>> _waitingLookupRequests;
		private readonly ConcurrentLongHashMap<ValueTask<MessageIdData>> _pendingGetLastMessageIdRequests = new ConcurrentLongHashMap<ValueTask<MessageIdData>>(16, 1);
		private readonly ConcurrentLongHashMap<ValueTask<IList<string>>> _pendingGetTopicsRequests = new ConcurrentLongHashMap<ValueTask<IList<string>>>(16, 1);

		private readonly ConcurrentLongHashMap<ValueTask<CommandGetSchemaResponse>> _pendingGetSchemaRequests = new ConcurrentLongHashMap<ValueTask<CommandGetSchemaResponse>>(16, 1);
		private readonly ConcurrentLongHashMap<ValueTask<CommandGetOrCreateSchemaResponse>> _pendingGetOrCreateSchemaRequests = new ConcurrentLongHashMap<ValueTask<CommandGetOrCreateSchemaResponse>>(16, 1);
		private readonly ConcurrentLongHashMap<ConsumerImpl<object>> consumers = new ConcurrentLongHashMap<ConsumerImpl<object>>(16, 1);
		private readonly ConcurrentLongHashMap<TransactionMetaStoreHandler> transactionMetaStoreHandlers = new ConcurrentLongHashMap<TransactionMetaStoreHandler>(16, 1);

		private readonly ValueTask _connectionTask = new ValueTask();
		private readonly ConcurrentQueue<RequestTime> _requestTimeoutQueue = new ConcurrentQueue<RequestTime>();
		private readonly Semaphore _pendingLookupRequestSemaphore;
		private readonly Semaphore _maxLookupRequestSemaphore;
		private readonly IEventLoopGroup _eventLoopGroup;

		private static readonly AtomicIntegerFieldUpdater<ClientConnection> NUMBER_OF_REJECTED_REQUESTS_UPDATER = AtomicIntegerFieldUpdater.newUpdater(typeof(ClientConnection), "numberOfRejectRequests");
		private volatile int _numberOfRejectRequests = 0;


		private static int _maxMessageSize = Commands.DEFAULT_MAX_MESSAGE_SIZE;

		private readonly int _maxNumberOfRejectedRequestPerConnection;
		private readonly int _rejectedRequestResetTimeSec = 60;
		private readonly int _protocolVersion;
		private readonly long _operationTimeoutMs;

		protected internal string _proxyToTargetBrokerAddress = null;
		// Remote hostName with which client is connected
		protected internal string _remoteHostName = null;
		private bool _isTlsHostnameVerificationEnable;

		private static readonly DefaultHostnameVerifier HOSTNAME_VERIFIER = new DefaultHostnameVerifier();
		private TaskScheduler timeoutTask;

		// Added for mutual authentication.
		protected internal IAuthenticationDataProvider authenticationDataProvider;

		internal enum State
		{
			None,
			SentConnectFrame,
			Ready,
			Failed,
			Connecting
		}

		internal class RequestTime
		{
			internal long creationTimeMs;
			internal long requestId;

			public RequestTime(long creationTime, long requestId) : base()
			{
				this.creationTimeMs = creationTime;
				this.requestId = requestId;
			}
		}

		public ClientConnection(ClientConfigurationData conf) : this(conf, Commands.CurrentProtocolVersion)
		{
		}

		public ClientConnection(ClientConfigurationData conf, int protocolVersion) : base(conf.KeepAliveIntervalSeconds, BAMCIS.Util.Concurrent.TimeUnit.SECONDS)
		{
			checkArgument(conf.MaxLookupRequest > conf.ConcurrentLookupRequest);
			_pendingLookupRequestSemaphore = new Semaphore(conf.ConcurrentLookupRequest, conf.MaxLookupRequest);
			_maxLookupRequestSemaphore = new Semaphore(conf.MaxLookupRequest - conf.ConcurrentLookupRequest, conf.MaxLookupRequest);
			_waitingLookupRequests = new LinkedList<KeyValuePair<long, KeyValuePair<IByteBuffer, ValueTask<LookupDataResult>>>>();
			_authentication = conf.Authentication;
			_eventLoopGroup = eventLoopGroup;
			_maxNumberOfRejectedRequestPerConnection = conf.MaxNumberOfRejectedRequestPerConnection;
			_operationTimeoutMs = conf.OperationTimeoutMs;
			_state = State.None;
			_isTlsHostnameVerificationEnable = conf.TlsHostnameVerificationEnable;
			_protocolVersion = protocolVersion;
		}
		public void ChannelActive(IChannelHandlerContext ctx)
		{
			base.ChannelActive(ctx);
			this.timeoutTask = _eventLoopGroup.scheduleAtFixedRate(() => checkRequestTimeout(), operationTimeoutMs, operationTimeoutMs, TimeUnit.MILLISECONDS);

			if (string.ReferenceEquals(_proxyToTargetBrokerAddress, null))
			{
				if (log.DebugEnabled)
				{
					log.debug("{} Connected to broker", ctx.Channel);
				}
			}
			else
			{
				log.info("{} Connected through proxy to target broker at {}", ctx.Channel, _proxyToTargetBrokerAddress);
			}
			// Send CONNECT command
			ctx.WriteAndFlushAsync(NewConnectCommand()).addListener(future =>
			{
				if (future.Success)
				{
					if (log.DebugEnabled)
					{
						log.debug("Complete: {}", future.Success);
					}
					state = State.SentConnectFrame;
				}
				else
				{
					log.warn("Error during handshake", future.cause());
					ctx.close();
				}
			});
		}

		protected internal virtual IByteBuffer NewConnectCommand()
		{
			// mutual authentication is to auth between `remoteHostName` and this client for this channel.
			// each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
			// and return authData to server.
			authenticationDataProvider = _authentication.GetAuthData(_remoteHostName);
			AuthData authData = authenticationDataProvider.Authenticate(AuthData.of(AuthData.INIT_AUTH_DATA));
			return Commands.NewConnect(_authentication.AuthMethodName, authData, _protocolVersion, PulsarVersion.Version, _proxyToTargetBrokerAddress, null, null, null);
		}

		public override void ChannelInactive(IChannelHandlerContext ctx)
		{
			base.ChannelInactive(ctx);
			log.info("{} Disconnected", ctx.Channel);
			if (!connectionFuture_Conflict.Done)
			{
				_connectionTask.completeExceptionally(new PulsarClientException("Connection already closed"));
			}

			PulsarClientException e = new PulsarClientException("Disconnected from server at " + ctx.channel().remoteAddress());

			// Fail out all the pending ops
			_pendingRequests.forEach((key, future) => future.completeExceptionally(e));
			_pendingLookupRequests.forEach((key, future) => future.completeExceptionally(e));
			_waitingLookupRequests.forEach(pair => pair.Right.Right.completeExceptionally(e));
			_pendingGetLastMessageIdRequests.forEach((key, future) => future.completeExceptionally(e));
			_pendingGetTopicsRequests.forEach((key, future) => future.completeExceptionally(e));
			_pendingGetSchemaRequests.forEach((key, future) => future.completeExceptionally(e));

			// Notify all attached producers/consumers so they have a chance to reconnect
			_producers.forEach((id, producer) => producer.connectionClosed(this));
			consumers.forEach((id, consumer) => consumer.connectionClosed(this));
			transactionMetaStoreHandlers.forEach((id, handler) => handler.connectionClosed(this));

			_pendingRequests.clear();
			_pendingLookupRequests.clear();
			_waitingLookupRequests.Clear();
			_pendingGetLastMessageIdRequests.clear();
			_pendingGetTopicsRequests.clear();

			_producers.clear();
			consumers.clear();

			timeoutTask.cancel(true);
		}

		// Command Handlers
		public override void exceptionCaught(ChannelHandlerContext ctx, Exception cause)
		{
			if (state != State.Failed)
			{
				// No need to report stack trace for known exceptions that happen in disconnections
				log.warn("[{}] Got exception {} : {}", remoteAddress, cause.GetType().Name, cause.Message, isKnownException(cause) ? null : cause);
				state = State.Failed;
			}
			else
			{
				// At default info level, suppress all subsequent exceptions that are thrown when the connection has already
				// failed
				if (log.DebugEnabled)
				{
					log.debug("[{}] Got exception: {}", remoteAddress, cause.Message, cause);
				}
			}

			ctx.close();
		}

		public static bool isKnownException(Exception t)
		{
			return t is NativeIoException || t is ClosedChannelException;
		}

		protected internal override void handleConnected(CommandConnected connected)
		{

			if (isTlsHostnameVerificationEnable && !string.ReferenceEquals(remoteHostName, null) && !verifyTlsHostName(remoteHostName, ctx))
			{
				// close the connection if host-verification failed with the broker
				Log.Logger.warn("[{}] Failed to verify hostname of {}", ctx.channel(), remoteHostName);
				ctx.close();
				return;
			}

			checkArgument(state == State.SentConnectFrame || state == State.Connecting);
			if (connected.hasMaxMessageSize())
			{
				if (log.DebugEnabled)
				{
					log.debug("{} Connection has max message size setting, replace old frameDecoder with " + "server frame size {}", ctx.channel(), connected.MaxMessageSize);
				}
				maxMessageSize = connected.MaxMessageSize;
				ctx.pipeline().replace("frameDecoder", "newFrameDecoder", new LengthFieldBasedFrameDecoder(connected.MaxMessageSize + Commands.MESSAGE_SIZE_FRAME_PADDING, 0, 4, 0, 4));
			}
			if (log.DebugEnabled)
			{
				log.debug("{} Connection is ready", ctx.channel());
			}
			// set remote protocol version to the correct version before we complete the connection future
			remoteEndpointProtocolVersion = connected.ProtocolVersion;
			connectionFuture_Conflict.complete(null);
			state = State.Ready;
		}

		protected internal void HandleAuthChallenge(CommandAuthChallenge authChallenge)
		{
			checkArgument(authChallenge.hasChallenge());
			checkArgument(authChallenge.Challenge.hasAuthData());

			// mutual authn. If auth not complete, continue auth; if auth complete, complete connectionFuture.
			try
			{
				var authData = authenticationDataProvider.Authenticate(AuthData.Of(authChallenge.Challenge.auth_data));

				checkState(!authData.Complete);

				ByteBuf request = Commands.newAuthResponse(authentication.AuthMethodName, authData, this.protocolVersion, PulsarVersion.Version);

				if (log.DebugEnabled)
				{
					log.debug("{} Mutual auth {}", ctx.channel(), authentication.AuthMethodName);
				}

				ctx.writeAndFlush(request).addListener(writeFuture =>
				{
				if (!writeFuture.Success)
				{
					log.warn("{} Failed to send request for mutual auth to broker: {}", ctx.channel(), writeFuture.cause().Message);
					connectionFuture_Conflict.completeExceptionally(writeFuture.cause());
				}
				});
				state = State.Connecting;
			}
			catch (Exception e)
			{
				log.error("{} Error mutual verify: {}", ctx.channel(), e);
				connectionFuture_Conflict.completeExceptionally(e);
				return;
			}
		}

		protected internal override void handleSendReceipt(PulsarApi.CommandSendReceipt sendReceipt)
		{
			checkArgument(state == State.Ready);

			long producerId = sendReceipt.ProducerId;
			long sequenceId = sendReceipt.SequenceId;
			long highestSequenceId = sendReceipt.HighestSequenceId;
			long ledgerId = -1;
			long entryId = -1;
			if (sendReceipt.hasMessageId())
			{
				ledgerId = sendReceipt.MessageId.LedgerId;
				entryId = sendReceipt.MessageId.EntryId;
			}

			if (ledgerId == -1 && entryId == -1)
			{
				log.warn("[{}] Message has been dropped for non-persistent topic producer-id {}-{}", ctx.channel(), producerId, sequenceId);
			}

			if (log.DebugEnabled)
			{
				log.debug("{} Got receipt for producer: {} -- msg: {} -- id: {}:{}", ctx.channel(), producerId, sequenceId, ledgerId, entryId);
			}

			producers.get(producerId).ackReceived(this, sequenceId, highestSequenceId, ledgerId, entryId);
		}

		protected internal override void handleMessage(PulsarApi.CommandMessage cmdMessage, ByteBuf headersAndPayload)
		{
			checkArgument(state == State.Ready);

			if (log.DebugEnabled)
			{
				log.debug("{} Received a message from the server: {}", ctx.channel(), cmdMessage);
			}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.get(cmdMessage.getConsumerId());
			ConsumerImpl<object> consumer = consumers.get(cmdMessage.ConsumerId);
			if (consumer != null)
			{
				consumer.messageReceived(cmdMessage.MessageId, cmdMessage.RedeliveryCount, headersAndPayload, this);
			}
		}

		protected internal override void handleActiveConsumerChange(PulsarApi.CommandActiveConsumerChange change)
		{
			checkArgument(state == State.Ready);

			if (log.DebugEnabled)
			{
				log.debug("{} Received a consumer group change message from the server : {}", ctx.channel(), change);
			}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.get(change.getConsumerId());
			ConsumerImpl<object> consumer = consumers.get(change.ConsumerId);
			if (consumer != null)
			{
				consumer.activeConsumerChanged(change.IsActive);
			}
		}

		protected internal override void handleSuccess(PulsarApi.CommandSuccess success)
		{
			checkArgument(state == State.Ready);

			if (log.DebugEnabled)
			{
				log.debug("{} Received success response from server: {}", ctx.channel(), success.RequestId);
			}
			long requestId = success.RequestId;
			CompletableFuture<ProducerResponse> requestFuture = pendingRequests.remove(requestId);
			if (requestFuture != null)
			{
				requestFuture.complete(null);
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", ctx.channel(), success.RequestId);
			}
		}

		protected internal override void handleGetLastMessageIdSuccess(PulsarApi.CommandGetLastMessageIdResponse success)
		{
			checkArgument(state == State.Ready);

			if (log.DebugEnabled)
			{
				log.debug("{} Received success GetLastMessageId response from server: {}", ctx.channel(), success.RequestId);
			}
			long requestId = success.RequestId;
			CompletableFuture<PulsarApi.MessageIdData> requestFuture = pendingGetLastMessageIdRequests.remove(requestId);
			if (requestFuture != null)
			{
				requestFuture.complete(success.LastMessageId);
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", ctx.channel(), success.RequestId);
			}
		}

		protected internal override void handleProducerSuccess(PulsarApi.CommandProducerSuccess success)
		{
			checkArgument(state == State.Ready);

			if (log.DebugEnabled)
			{
				log.debug("{} Received producer success response from server: {} - producer-name: {}", ctx.channel(), success.RequestId, success.ProducerName);
			}
			long requestId = success.RequestId;
			CompletableFuture<ProducerResponse> requestFuture = pendingRequests.remove(requestId);
			if (requestFuture != null)
			{
				requestFuture.complete(new ProducerResponse(success.ProducerName, success.LastSequenceId, success.SchemaVersion.toByteArray()));
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", ctx.channel(), success.RequestId);
			}
		}

		protected internal override void handleLookupResponse(PulsarApi.CommandLookupTopicResponse lookupResult)
		{
			if (log.DebugEnabled)
			{
				log.debug("Received Broker lookup response: {}", lookupResult.Response);
			}

			long requestId = lookupResult.RequestId;
			CompletableFuture<LookupDataResult> requestFuture = getAndRemovePendingLookupRequest(requestId);

			if (requestFuture != null)
			{
				if (requestFuture.CompletedExceptionally)
				{
					if (log.DebugEnabled)
					{
						log.debug("{} Request {} already timed-out", ctx.channel(), lookupResult.RequestId);
					}
					return;
				}
				// Complete future with exception if : Result.response=fail/null
				if (!lookupResult.hasResponse() || PulsarApi.CommandLookupTopicResponse.LookupType.Failed.Equals(lookupResult.Response))
				{
					if (lookupResult.hasError())
					{
						checkServerError(lookupResult.Error, lookupResult.Message);
						requestFuture.completeExceptionally(getPulsarClientException(lookupResult.Error, lookupResult.Message));
					}
					else
					{
						requestFuture.completeExceptionally(new PulsarClientException.LookupException("Empty lookup response"));
					}
				}
				else
				{
					requestFuture.complete(new LookupDataResult(lookupResult));
				}
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", ctx.channel(), lookupResult.RequestId);
			}
		}

		protected internal override void handlePartitionResponse(PulsarApi.CommandPartitionedTopicMetadataResponse lookupResult)
		{
			if (log.DebugEnabled)
			{
				log.debug("Received Broker Partition response: {}", lookupResult.Partitions);
			}

			long requestId = lookupResult.RequestId;
			CompletableFuture<LookupDataResult> requestFuture = getAndRemovePendingLookupRequest(requestId);

			if (requestFuture != null)
			{
				if (requestFuture.CompletedExceptionally)
				{
					if (log.DebugEnabled)
					{
						log.debug("{} Request {} already timed-out", ctx.channel(), lookupResult.RequestId);
					}
					return;
				}
				// Complete future with exception if : Result.response=fail/null
				if (!lookupResult.hasResponse() || PulsarApi.CommandPartitionedTopicMetadataResponse.LookupType.Failed.Equals(lookupResult.Response))
				{
					if (lookupResult.hasError())
					{
						checkServerError(lookupResult.Error, lookupResult.Message);
						requestFuture.completeExceptionally(getPulsarClientException(lookupResult.Error, lookupResult.Message));
					}
					else
					{
						requestFuture.completeExceptionally(new PulsarClientException.LookupException("Empty lookup response"));
					}
				}
				else
				{
					// return LookupDataResult when Result.response = success/redirect
					requestFuture.complete(new LookupDataResult(lookupResult.Partitions));
				}
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", ctx.channel(), lookupResult.RequestId);
			}
		}

		protected internal override void handleReachedEndOfTopic(PulsarApi.CommandReachedEndOfTopic commandReachedEndOfTopic)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long consumerId = commandReachedEndOfTopic.getConsumerId();
			long consumerId = commandReachedEndOfTopic.ConsumerId;

			log.info("[{}] Broker notification reached the end of topic: {}", remoteAddress, consumerId);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.get(consumerId);
			ConsumerImpl<object> consumer = consumers.get(consumerId);
			if (consumer != null)
			{
				consumer.setTerminated();
			}
		}

		// caller of this method needs to be protected under pendingLookupRequestSemaphore
		private void addPendingLookupRequests(long requestId, CompletableFuture<LookupDataResult> future)
		{
			pendingLookupRequests.put(requestId, future);
			eventLoopGroup.schedule(() =>
			{
			if (!future.Done)
			{
				future.completeExceptionally(new TimeoutException(requestId + " lookup request timedout after ms " + operationTimeoutMs));
			}
			}, operationTimeoutMs, TimeUnit.MILLISECONDS);
		}

		private CompletableFuture<LookupDataResult> getAndRemovePendingLookupRequest(long requestId)
		{
			CompletableFuture<LookupDataResult> result = pendingLookupRequests.remove(requestId);
			if (result != null)
			{
				Pair<long, Pair<ByteBuf, CompletableFuture<LookupDataResult>>> firstOneWaiting = waitingLookupRequests.RemoveFirst();
				if (firstOneWaiting != null)
				{
					maxLookupRequestSemaphore.release();
					// schedule a new lookup in.
					eventLoopGroup.submit(() =>
					{
					long newId = firstOneWaiting.Left;
					CompletableFuture<LookupDataResult> newFuture = firstOneWaiting.Right.Right;
					addPendingLookupRequests(newId, newFuture);
					ctx.writeAndFlush(firstOneWaiting.Right.Left).addListener(writeFuture =>
					{
						if (!writeFuture.Success)
						{
							log.warn("{} Failed to send request {} to broker: {}", ctx.channel(), newId, writeFuture.cause().Message);
							getAndRemovePendingLookupRequest(newId);
							newFuture.completeExceptionally(writeFuture.cause());
						}
					});
					});
				}
				else
				{
					pendingLookupRequestSemaphore.release();
				}
			}
			return result;
		}

		protected internal override void handleSendError(PulsarApi.CommandSendError sendError)
		{
			log.warn("{} Received send error from server: {} : {}", ctx.channel(), sendError.Error, sendError.Message);

			long producerId = sendError.ProducerId;
			long sequenceId = sendError.SequenceId;

			switch (sendError.Error)
			{
			case ChecksumError:
				producers.get(producerId).recoverChecksumError(this, sequenceId);
				break;

			case TopicTerminatedError:
				producers.get(producerId).terminated(this);
				break;

			default:
				// By default, for transient error, let the reconnection logic
				// to take place and re-establish the produce again
				ctx.close();
			break;
			}
		}

		protected internal override void handleError(PulsarApi.CommandError error)
		{
			checkArgument(state == State.SentConnectFrame || state == State.Ready);

			log.warn("{} Received error from server: {}", ctx.channel(), error.Message);
			long requestId = error.RequestId;
			if (error.Error == PulsarApi.ServerError.ProducerBlockedQuotaExceededError)
			{
				log.warn("{} Producer creation has been blocked because backlog quota exceeded for producer topic", ctx.channel());
			}
			CompletableFuture<ProducerResponse> requestFuture = pendingRequests.remove(requestId);
			if (requestFuture != null)
			{
				requestFuture.completeExceptionally(getPulsarClientException(error.Error, error.Message));
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", ctx.channel(), error.RequestId);
			}
		}

		protected internal override void handleCloseProducer(PulsarApi.CommandCloseProducer closeProducer)
		{
			log.info("[{}] Broker notification of Closed producer: {}", remoteAddress, closeProducer.ProducerId);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long producerId = closeProducer.getProducerId();
			long producerId = closeProducer.ProducerId;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ProducerImpl<?> producer = producers.get(producerId);
			ProducerImpl<object> producer = producers.get(producerId);
			if (producer != null)
			{
				producer.connectionClosed(this);
			}
			else
			{
				log.warn("Producer with id {} not found while closing producer ", producerId);
			}
		}

		protected internal override void handleCloseConsumer(PulsarApi.CommandCloseConsumer closeConsumer)
		{
			log.info("[{}] Broker notification of Closed consumer: {}", remoteAddress, closeConsumer.ConsumerId);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long consumerId = closeConsumer.getConsumerId();
			long consumerId = closeConsumer.ConsumerId;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.get(consumerId);
			ConsumerImpl<object> consumer = consumers.get(consumerId);
			if (consumer != null)
			{
				consumer.connectionClosed(this);
			}
			else
			{
				log.warn("Consumer with id {} not found while closing consumer ", consumerId);
			}
		}

		protected internal override bool HandshakeCompleted
		{
			get
			{
				return state == State.Ready;
			}
		}

		public virtual CompletableFuture<LookupDataResult> newLookup(ByteBuf request, long requestId)
		{
			CompletableFuture<LookupDataResult> future = new CompletableFuture<LookupDataResult>();

			if (pendingLookupRequestSemaphore.tryAcquire())
			{
				addPendingLookupRequests(requestId, future);
				ctx.writeAndFlush(request).addListener(writeFuture =>
				{
				if (!writeFuture.Success)
				{
					log.warn("{} Failed to send request {} to broker: {}", ctx.channel(), requestId, writeFuture.cause().Message);
					getAndRemovePendingLookupRequest(requestId);
					future.completeExceptionally(writeFuture.cause());
				}
				});
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug("{} Failed to add lookup-request into pending queue", requestId);
				}

				if (maxLookupRequestSemaphore.tryAcquire())
				{
					waitingLookupRequests.AddLast(Pair.of(requestId, Pair.of(request, future)));
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug("{} Failed to add lookup-request into waiting queue", requestId);
					}
					future.completeExceptionally(new PulsarClientException.TooManyRequestsException(string.Format("Requests number out of config: There are {{{0}}} lookup requests outstanding and {{{1}}} requests pending.", pendingLookupRequests.size(), waitingLookupRequests.Count)));
				}
			}
			return future;
		}

		public virtual CompletableFuture<IList<string>> newGetTopicsOfNamespace(ByteBuf request, long requestId)
		{
			CompletableFuture<IList<string>> future = new CompletableFuture<IList<string>>();

			pendingGetTopicsRequests.put(requestId, future);
			ctx.writeAndFlush(request).addListener(writeFuture =>
			{
			if (!writeFuture.Success)
			{
				log.warn("{} Failed to send request {} to broker: {}", ctx.channel(), requestId, writeFuture.cause().Message);
				pendingGetTopicsRequests.remove(requestId);
				future.completeExceptionally(writeFuture.cause());
			}
			});

			return future;
		}

		protected internal override void handleGetTopicsOfNamespaceSuccess(PulsarApi.CommandGetTopicsOfNamespaceResponse success)
		{
			checkArgument(state == State.Ready);

			long requestId = success.RequestId;
			IList<string> topics = success.TopicsList;

			if (log.DebugEnabled)
			{
				log.debug("{} Received get topics of namespace success response from server: {} - topics.size: {}", ctx.channel(), success.RequestId, topics.Count);
			}

			CompletableFuture<IList<string>> requestFuture = pendingGetTopicsRequests.remove(requestId);
			if (requestFuture != null)
			{
				requestFuture.complete(topics);
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", ctx.channel(), success.RequestId);
			}
		}

		protected internal void HandleGetSchemaResponse(PulsarApi.CommandGetSchemaResponse commandGetSchemaResponse)
		{
			checkArgument(state == State.Ready);

			long requestId = commandGetSchemaResponse.RequestId;

			CompletableFuture<PulsarApi.CommandGetSchemaResponse> future = pendingGetSchemaRequests.remove(requestId);
			if (future == null)
			{
				log.warn("{} Received unknown request id from server: {}", ctx.channel(), requestId);
				return;
			}
			future.complete(commandGetSchemaResponse);
		}

		protected internal void HandleGetOrCreateSchemaResponse(PulsarApi.CommandGetOrCreateSchemaResponse commandGetOrCreateSchemaResponse)
		{
			checkArgument(state == State.Ready);
			long requestId = commandGetOrCreateSchemaResponse.RequestId;
			CompletableFuture<PulsarApi.CommandGetOrCreateSchemaResponse> future = pendingGetOrCreateSchemaRequests.remove(requestId);
			if (future == null)
			{
				log.warn("{} Received unknown request id from server: {}", ctx.channel(), requestId);
				return;
			}
			future.complete(commandGetOrCreateSchemaResponse);
		}

		internal virtual Promise<Void> NewPromise()
		{
			return ctx.newPromise();
		}

		public virtual ChannelHandlerContext ctx()
		{
			return ctx;
		}

		internal virtual Channel Channel()
		{
			return ctx.channel();
		}

		internal virtual SocketAddress ServerAddrees()
		{
			return remoteAddress;
		}

		internal virtual CompletableFuture<Void> ConnectionFuture()
		{
			return connectionFuture_Conflict;
		}

		internal virtual CompletableFuture<ProducerResponse> SendRequestWithId(ByteBuf cmd, long requestId)
		{
			CompletableFuture<ProducerResponse> future = new CompletableFuture<ProducerResponse>();
			pendingRequests.put(requestId, future);
			ctx.writeAndFlush(cmd).addListener(writeFuture =>
			{
			if (!writeFuture.Success)
			{
				log.warn("{} Failed to send request to broker: {}", ctx.channel(), writeFuture.cause().Message);
				pendingRequests.remove(requestId);
				future.completeExceptionally(writeFuture.cause());
			}
			});
			requestTimeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			return future;
		}

		public virtual CompletableFuture<PulsarApi.MessageIdData> SendGetLastMessageId(ByteBuf request, long requestId)
		{
			CompletableFuture<PulsarApi.MessageIdData> future = new CompletableFuture<PulsarApi.MessageIdData>();

			pendingGetLastMessageIdRequests.put(requestId, future);

			ctx.writeAndFlush(request).addListener(writeFuture =>
			{
			if (!writeFuture.Success)
			{
				log.warn("{} Failed to send GetLastMessageId request to broker: {}", ctx.channel(), writeFuture.cause().Message);
				pendingGetLastMessageIdRequests.remove(requestId);
				future.completeExceptionally(writeFuture.cause());
			}
			});

			return future;
		}

		public virtual CompletableFuture<Optional<SchemaInfo>> SendGetSchema(ByteBuf request, long requestId)
		{
			return sendGetRawSchema(request, requestId).thenCompose(commandGetSchemaResponse =>
			{
			if (commandGetSchemaResponse.hasErrorCode())
			{
				ServerError rc = commandGetSchemaResponse.ErrorCode;
				if (rc == ServerError.TopicNotFound)
				{
					return CompletableFuture.completedFuture(null);
				}
				else
				{
					return FutureUtil.failedFuture(getPulsarClientException(rc, commandGetSchemaResponse.ErrorMessage));
				}
			}
			else
			{
				return CompletableFuture.completedFuture(SchemaInfoUtil.newSchemaInfo(commandGetSchemaResponse.Schema));
			}
			});
		}

		public virtual ValueTask<PulsarApi.CommandGetSchemaResponse> SendGetRawSchema(ByteBuf request, long requestId)
		{
			
			CompletableFuture<PulsarApi.CommandGetSchemaResponse> future = new CompletableFuture<PulsarApi.CommandGetSchemaResponse>();

			pendingGetSchemaRequests.put(requestId, future);

			ctx.writeAndFlush(request).addListener(writeFuture =>
			{
			if (!writeFuture.Success)
			{
				log.warn("{} Failed to send GetSchema request to broker: {}", ctx.channel(), writeFuture.cause().Message);
				pendingGetLastMessageIdRequests.remove(requestId);
				future.completeExceptionally(writeFuture.cause());
			}
			});

			return future;
		}

		public virtual ValueTask<sbyte[]> SendGetOrCreateSchema(ByteBuf request, long requestId)
		{
			CompletableFuture<PulsarApi.CommandGetOrCreateSchemaResponse> future = new CompletableFuture<PulsarApi.CommandGetOrCreateSchemaResponse>();
			pendingGetOrCreateSchemaRequests.put(requestId, future);
			ctx.writeAndFlush(request).addListener(writeFuture =>
			{
			if (!writeFuture.Success)
			{
				log.warn("{} Failed to send GetOrCreateSchema request to broker: {}", ctx.channel(), writeFuture.cause().Message);
				pendingGetOrCreateSchemaRequests.remove(requestId);
				future.completeExceptionally(writeFuture.cause());
			}
			});
			return future.thenCompose(response =>
			{
			if (response.hasErrorCode())
			{
				ServerError rc = response.ErrorCode;
				if (rc == ServerError.TopicNotFound)
				{
					return CompletableFuture.completedFuture(SchemaVersion.Empty.bytes());
				}
				else
				{
					return FutureUtil.failedFuture(getPulsarClientException(rc, response.ErrorMessage));
				}
			}
			else
			{
				return CompletableFuture.completedFuture(response.SchemaVersion.toByteArray());
			}
			});
		}

		protected internal void HandleNewTxnResponse(PulsarApi.CommandNewTxnResponse command)
		{
			TransactionMetaStoreHandler handler = checkAndGetTransactionMetaStoreHandler(command.TxnidMostBits);
			if (handler != null)
			{
				handler.handleNewTxnResponse(command);
			}
		}

		protected internal void HandleAddPartitionToTxnResponse(PulsarApi.CommandAddPartitionToTxnResponse command)
		{
			TransactionMetaStoreHandler handler = checkAndGetTransactionMetaStoreHandler(command.TxnidMostBits);
			if (handler != null)
			{
				handler.handleAddPublishPartitionToTxnResponse(command);
			}
		}

		protected internal void HandleEndTxnResponse(PulsarApi.CommandEndTxnResponse command)
		{
			TransactionMetaStoreHandler handler = checkAndGetTransactionMetaStoreHandler(command.TxnidMostBits);
			if (handler != null)
			{
				handler.handleEndTxnResponse(command);
			}
		}

		private TransactionMetaStoreHandler CheckAndGetTransactionMetaStoreHandler(long tcId)
		{
			TransactionMetaStoreHandler handler = transactionMetaStoreHandlers.get(tcId);
			if (handler == null)
			{
				channel().close();
				log.warn("Close the channel since can't get the transaction meta store handler, will reconnect later.");
			}
			return handler;
		}

		/// <summary>
		/// check serverError and take appropriate action
		/// <ul>
		/// <li>InternalServerError: close connection immediately</li>
		/// <li>TooManyRequest: received error count is more than maxNumberOfRejectedRequestPerConnection in
		/// #rejectedRequestResetTimeSec</li>
		/// </ul>
		/// </summary>
		/// <param name="error"> </param>
		/// <param name="errMsg"> </param>
		private void CheckServerError(PulsarApi.ServerError error, string errMsg)
		{
			if (PulsarApi.ServerError.ServiceNotReady.Equals(error))
			{
				log.error("{} Close connection because received internal-server error {}", ctx.channel(), errMsg);
				ctx.close();
			}
			else if (PulsarApi.ServerError.TooManyRequests.Equals(error))
			{
				long rejectedRequests = NUMBER_OF_REJECTED_REQUESTS_UPDATER.getAndIncrement(this);
				if (rejectedRequests == 0)
				{
					// schedule timer
					eventLoopGroup.schedule(() => NUMBER_OF_REJECTED_REQUESTS_UPDATER.set(ClientConnection.this, 0), rejectedRequestResetTimeSec, TimeUnit.SECONDS);
				}
				else if (rejectedRequests >= maxNumberOfRejectedRequestPerConnection)
				{
					log.error("{} Close connection because received {} rejected request in {} seconds ", ctx.channel(), NUMBER_OF_REJECTED_REQUESTS_UPDATER.get(ClientConnection.this), rejectedRequestResetTimeSec);
					ctx.close();
				}
			}
		}

		/// <summary>
		/// verifies host name provided in x509 Certificate in tls session
		/// 
		/// it matches hostname with below scenarios
		/// 
		/// <pre>
		///  1. Supports IPV4 and IPV6 host matching
		///  2. Supports wild card matching for DNS-name
		///  eg:
		///     HostName                     CN           Result
		/// 1.  localhost                    localhost    PASS
		/// 2.  localhost                    local*       PASS
		/// 3.  pulsar1-broker.com           pulsar*.com  PASS
		/// </pre>
		/// </summary>
		/// <param name="ctx"> </param>
		/// <returns> true if hostname is verified else return false </returns>
		private bool VerifyTlsHostName(string hostname, ChannelHandlerContext ctx)
		{
			ChannelHandler sslHandler = ctx.channel().pipeline().get("tls");

			SSLSession sslSession = null;
			if (sslHandler != null)
			{
				sslSession = ((SslHandler) sslHandler).engine().Session;
				if (log.DebugEnabled)
				{
					log.debug("Verifying HostName for {}, Cipher {}, Protocols {}", hostname, sslSession.CipherSuite, sslSession.Protocol);
				}
				return HOSTNAME_VERIFIER.verify(hostname, sslSession);
			}
			return false;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: void registerConsumer(final long consumerId, final ConsumerImpl<?> consumer)
		internal virtual void RegisterConsumer<T1>(long consumerId, ConsumerImpl<T1> consumer)
		{
			consumers.put(consumerId, consumer);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: void registerProducer(final long producerId, final ProducerImpl<?> producer)
		internal virtual void RegisterProducer<T1>(long producerId, ProducerImpl<T1> producer)
		{
			producers.put(producerId, producer);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: void registerTransactionMetaStoreHandler(final long transactionMetaStoreId, final TransactionMetaStoreHandler handler)
		internal virtual void RegisterTransactionMetaStoreHandler(long transactionMetaStoreId, TransactionMetaStoreHandler handler)
		{
			transactionMetaStoreHandlers.put(transactionMetaStoreId, handler);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: void removeProducer(final long producerId)
		internal virtual void RemoveProducer(long producerId)
		{
			producers.remove(producerId);
		}

		internal virtual void RemoveConsumer(long consumerId)
		{
			consumers.remove(consumerId);
		}

		internal virtual InetSocketAddress TargetBroker
		{
			set
			{
				this.proxyToTargetBrokerAddress = string.Format("{0}:{1:D}", value.HostString, value.Port);
			}
		}

		 internal virtual string RemoteHostName
		 {
			 set
			 {
				this.remoteHostName = value;
			 }
		 }

		private PulsarClientException GetPulsarClientException(PulsarApi.ServerError error, string errorMsg)
		{
			switch (error)
			{
			case AuthenticationError:
				return new PulsarClientException.AuthenticationException(errorMsg);
			case AuthorizationError:
				return new PulsarClientException.AuthorizationException(errorMsg);
			case ProducerBusy:
				return new PulsarClientException.ProducerBusyException(errorMsg);
			case ConsumerBusy:
				return new PulsarClientException.ConsumerBusyException(errorMsg);
			case MetadataError:
				return new PulsarClientException.BrokerMetadataException(errorMsg);
			case PersistenceError:
				return new PulsarClientException.BrokerPersistenceException(errorMsg);
			case ServiceNotReady:
				return new PulsarClientException.LookupException(errorMsg);
			case TooManyRequests:
				return new PulsarClientException.TooManyRequestsException(errorMsg);
			case ProducerBlockedQuotaExceededError:
				return new PulsarClientException.ProducerBlockedQuotaExceededError(errorMsg);
			case ProducerBlockedQuotaExceededException:
				return new PulsarClientException.ProducerBlockedQuotaExceededException(errorMsg);
			case TopicTerminatedError:
				return new PulsarClientException.TopicTerminatedException(errorMsg);
			case IncompatibleSchema:
				return new PulsarClientException.IncompatibleSchemaException(errorMsg);
			case TopicNotFound:
				return new PulsarClientException.TopicDoesNotExistException(errorMsg);
			case UnknownError:
			default:
				return new PulsarClientException(errorMsg);
			}
		}


		public virtual void Close()
		{
		   if (ctx != null)
		   {
			   ctx.close();
		   }
		}

		private void CheckRequestTimeout()
		{
			while (!requestTimeoutQueue.Empty)
			{
				RequestTime request = requestTimeoutQueue.peek();
				if (request == null || (DateTimeHelper.CurrentUnixTimeMillis() - request.creationTimeMs) < operationTimeoutMs)
				{
					// if there is no request that is timed out then exit the loop
					break;
				}
				request = requestTimeoutQueue.poll();
				CompletableFuture<ProducerResponse> requestFuture = pendingRequests.remove(request.requestId);
				if (requestFuture != null && !requestFuture.Done && requestFuture.completeExceptionally(new PulsarClientException.TimeoutException(request.requestId + " lookup request timedout after ms " + operationTimeoutMs)))
				{
					log.warn("{} request {} timed out after {} ms", ctx.channel(), request.requestId, operationTimeoutMs);
				}
				else
				{
					// request is already completed successfully.
				}
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ClientConnection));
	}

}