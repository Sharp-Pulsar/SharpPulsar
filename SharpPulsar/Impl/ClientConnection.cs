using SharpPulsar;
using System;
using System.Collections.Generic;
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
	using Queues = com.google.common.collect.Queues;

	using ByteBuf = io.netty.buffer.ByteBuf;
	using Channel = io.netty.channel.Channel;
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using NativeIoException = io.netty.channel.unix.Errors.NativeIoException;
	using LengthFieldBasedFrameDecoder = io.netty.handler.codec.LengthFieldBasedFrameDecoder;
	using SslHandler = io.netty.handler.ssl.SslHandler;
	using Promise = io.netty.util.concurrent.Promise;


	using Getter = lombok.Getter;
	using Pair = org.apache.commons.lang3.tuple.Pair;
	using DefaultHostnameVerifier = org.apache.http.conn.ssl.DefaultHostnameVerifier;
	using Authentication = org.apache.pulsar.client.api.Authentication;
	using AuthenticationDataProvider = org.apache.pulsar.client.api.AuthenticationDataProvider;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using TimeoutException = org.apache.pulsar.client.api.PulsarClientException.TimeoutException;
	using LookupDataResult = org.apache.pulsar.client.impl.BinaryProtoLookupService.LookupDataResult;
	using ClientConfigurationData = org.apache.pulsar.client.impl.conf.ClientConfigurationData;
	using AuthData = org.apache.pulsar.common.api.AuthData;
	using PulsarApi = org.apache.pulsar.common.api.proto.PulsarApi;
	using Commands = org.apache.pulsar.common.protocol.Commands;
	using PulsarHandler = org.apache.pulsar.common.protocol.PulsarHandler;
	using ServerError = SharpPulsar.ServerError;
	using SchemaVersion = org.apache.pulsar.common.protocol.schema.SchemaVersion;
	using SchemaInfo = Common.Schema.SchemaInfo;
	using SchemaInfoUtil = org.apache.pulsar.common.protocol.schema.SchemaInfoUtil;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
	using ConcurrentLongHashMap = org.apache.pulsar.common.util.collections.ConcurrentLongHashMap;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class ClientConnection : PulsarHandler
	{

		protected internal readonly Authentication authentication;
		private State state;

		private readonly ConcurrentLongHashMap<CompletableFuture<ProducerResponse>> pendingRequests = new ConcurrentLongHashMap<CompletableFuture<ProducerResponse>>(16, 1);
		private readonly ConcurrentLongHashMap<CompletableFuture<LookupDataResult>> pendingLookupRequests = new ConcurrentLongHashMap<CompletableFuture<LookupDataResult>>(16, 1);
		// LookupRequests that waiting in client side.
		private readonly LinkedList<Pair<long, Pair<ByteBuf, CompletableFuture<LookupDataResult>>>> waitingLookupRequests;
		private readonly ConcurrentLongHashMap<CompletableFuture<PulsarApi.MessageIdData>> pendingGetLastMessageIdRequests = new ConcurrentLongHashMap<CompletableFuture<PulsarApi.MessageIdData>>(16, 1);
		private readonly ConcurrentLongHashMap<CompletableFuture<IList<string>>> pendingGetTopicsRequests = new ConcurrentLongHashMap<CompletableFuture<IList<string>>>(16, 1);

		private readonly ConcurrentLongHashMap<CompletableFuture<SharpPulsar.CommandGetSchemaResponse>> pendingGetSchemaRequests = new ConcurrentLongHashMap<CompletableFuture<PulsarApi.CommandGetSchemaResponse>>(16, 1);
		private readonly ConcurrentLongHashMap<CompletableFuture<PulsarApi.CommandGetOrCreateSchemaResponse>> pendingGetOrCreateSchemaRequests = new ConcurrentLongHashMap<CompletableFuture<PulsarApi.CommandGetOrCreateSchemaResponse>>(16, 1);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.apache.pulsar.common.util.collections.ConcurrentLongHashMap<ProducerImpl<?>> producers = new org.apache.pulsar.common.util.collections.ConcurrentLongHashMap<>(16, 1);
		private readonly ConcurrentLongHashMap<ProducerImpl<object>> producers = new ConcurrentLongHashMap<ProducerImpl<object>>(16, 1);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.apache.pulsar.common.util.collections.ConcurrentLongHashMap<ConsumerImpl<?>> consumers = new org.apache.pulsar.common.util.collections.ConcurrentLongHashMap<>(16, 1);
		private readonly ConcurrentLongHashMap<ConsumerImpl<object>> consumers = new ConcurrentLongHashMap<ConsumerImpl<object>>(16, 1);
		private readonly ConcurrentLongHashMap<TransactionMetaStoreHandler> transactionMetaStoreHandlers = new ConcurrentLongHashMap<TransactionMetaStoreHandler>(16, 1);

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly CompletableFuture<Void> connectionFuture_Conflict = new CompletableFuture<Void>();
		private readonly ConcurrentLinkedQueue<RequestTime> requestTimeoutQueue = new ConcurrentLinkedQueue<RequestTime>();
		private readonly Semaphore pendingLookupRequestSemaphore;
		private readonly Semaphore maxLookupRequestSemaphore;
		private readonly EventLoopGroup eventLoopGroup;

		private static readonly AtomicIntegerFieldUpdater<ClientConnection> NUMBER_OF_REJECTED_REQUESTS_UPDATER = AtomicIntegerFieldUpdater.newUpdater(typeof(ClientConnection), "numberOfRejectRequests");
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile int numberOfRejectRequests = 0;
		private volatile int numberOfRejectRequests = 0;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter private static int maxMessageSize = org.apache.pulsar.common.protocol.Commands.DEFAULT_MAX_MESSAGE_SIZE;
		private static int maxMessageSize = Commands.DEFAULT_MAX_MESSAGE_SIZE;

		private readonly int maxNumberOfRejectedRequestPerConnection;
		private readonly int rejectedRequestResetTimeSec = 60;
		private readonly int protocolVersion;
		private readonly long operationTimeoutMs;

		protected internal string proxyToTargetBrokerAddress = null;
		// Remote hostName with which client is connected
		protected internal string remoteHostName = null;
		private bool isTlsHostnameVerificationEnable;

		private static readonly DefaultHostnameVerifier HOSTNAME_VERIFIER = new DefaultHostnameVerifier();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.concurrent.ScheduledFuture<?> timeoutTask;
		private ScheduledFuture<object> timeoutTask;

		// Added for mutual authentication.
		protected internal AuthenticationDataProvider authenticationDataProvider;

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

		public ClientConnection(ClientConfigurationData conf, EventLoopGroup eventLoopGroup) : this(conf, eventLoopGroup, Commands.CurrentProtocolVersion)
		{
		}

		public ClientConnection(ClientConfigurationData conf, EventLoopGroup eventLoopGroup, int protocolVersion) : base(conf.KeepAliveIntervalSeconds, TimeUnit.SECONDS)
		{
			checkArgument(conf.MaxLookupRequest > conf.ConcurrentLookupRequest);
			this.pendingLookupRequestSemaphore = new Semaphore(conf.ConcurrentLookupRequest, false);
			this.maxLookupRequestSemaphore = new Semaphore(conf.MaxLookupRequest - conf.ConcurrentLookupRequest, false);
			this.waitingLookupRequests = Queues.newConcurrentLinkedQueue();
			this.authentication = conf.Authentication;
			this.eventLoopGroup = eventLoopGroup;
			this.maxNumberOfRejectedRequestPerConnection = conf.MaxNumberOfRejectedRequestPerConnection;
			this.operationTimeoutMs = conf.OperationTimeoutMs;
			this.state = State.None;
			this.isTlsHostnameVerificationEnable = conf.TlsHostnameVerificationEnable;
			this.protocolVersion = protocolVersion;
		}
		public void ChannelActive(ChannelHandlerContext ctx)
		{
			base.channelActive(ctx);
			this.timeoutTask = this.eventLoopGroup.scheduleAtFixedRate(() => checkRequestTimeout(), operationTimeoutMs, operationTimeoutMs, TimeUnit.MILLISECONDS);

			if (string.ReferenceEquals(proxyToTargetBrokerAddress, null))
			{
				if (log.DebugEnabled)
				{
					log.debug("{} Connected to broker", ctx.channel());
				}
			}
			else
			{
				log.info("{} Connected through proxy to target broker at {}", ctx.channel(), proxyToTargetBrokerAddress);
			}
			// Send CONNECT command
			ctx.writeAndFlush(newConnectCommand()).addListener(future =>
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected io.netty.buffer.ByteBuf newConnectCommand() throws Exception
		protected internal virtual ByteBuf newConnectCommand()
		{
			// mutual authentication is to auth between `remoteHostName` and this client for this channel.
			// each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
			// and return authData to server.
			authenticationDataProvider = authentication.getAuthData(remoteHostName);
			AuthData authData = authenticationDataProvider.authenticate(AuthData.of(AuthData.INIT_AUTH_DATA));
			return Commands.newConnect(authentication.AuthMethodName, authData, this.protocolVersion, PulsarVersion.Version, proxyToTargetBrokerAddress, null, null, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void channelInactive(io.netty.channel.ChannelHandlerContext ctx) throws Exception
		public override void channelInactive(ChannelHandlerContext ctx)
		{
			base.channelInactive(ctx);
			log.info("{} Disconnected", ctx.channel());
			if (!connectionFuture_Conflict.Done)
			{
				connectionFuture_Conflict.completeExceptionally(new PulsarClientException("Connection already closed"));
			}

			PulsarClientException e = new PulsarClientException("Disconnected from server at " + ctx.channel().remoteAddress());

			// Fail out all the pending ops
			pendingRequests.forEach((key, future) => future.completeExceptionally(e));
			pendingLookupRequests.forEach((key, future) => future.completeExceptionally(e));
			waitingLookupRequests.forEach(pair => pair.Right.Right.completeExceptionally(e));
			pendingGetLastMessageIdRequests.forEach((key, future) => future.completeExceptionally(e));
			pendingGetTopicsRequests.forEach((key, future) => future.completeExceptionally(e));
			pendingGetSchemaRequests.forEach((key, future) => future.completeExceptionally(e));

			// Notify all attached producers/consumers so they have a chance to reconnect
			producers.forEach((id, producer) => producer.connectionClosed(this));
			consumers.forEach((id, consumer) => consumer.connectionClosed(this));
			transactionMetaStoreHandlers.forEach((id, handler) => handler.connectionClosed(this));

			pendingRequests.clear();
			pendingLookupRequests.clear();
			waitingLookupRequests.Clear();
			pendingGetLastMessageIdRequests.clear();
			pendingGetTopicsRequests.clear();

			producers.clear();
			consumers.clear();

			timeoutTask.cancel(true);
		}

		// Command Handlers

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void exceptionCaught(io.netty.channel.ChannelHandlerContext ctx, Throwable cause) throws Exception
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

		protected internal override void handleConnected(PulsarApi.CommandConnected connected)
		{

			if (isTlsHostnameVerificationEnable && !string.ReferenceEquals(remoteHostName, null) && !verifyTlsHostName(remoteHostName, ctx))
			{
				// close the connection if host-verification failed with the broker
				log.warn("[{}] Failed to verify hostname of {}", ctx.channel(), remoteHostName);
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

		protected internal override void handleAuthChallenge(PulsarApi.CommandAuthChallenge authChallenge)
		{
			checkArgument(authChallenge.hasChallenge());
			checkArgument(authChallenge.Challenge.hasAuthData());

			// mutual authn. If auth not complete, continue auth; if auth complete, complete connectionFuture.
			try
			{
				AuthData authData = authenticationDataProvider.authenticate(AuthData.of(authChallenge.Challenge.AuthData.toByteArray()));

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