using System;
using System.Collections.Generic;

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

	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;
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
	using Authentication = SharpPulsar.Api.Authentication;
	using AuthenticationDataProvider = SharpPulsar.Api.AuthenticationDataProvider;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using TimeoutException = SharpPulsar.Api.PulsarClientException.TimeoutException;
	using LookupDataResult = SharpPulsar.Impl.BinaryProtoLookupService.LookupDataResult;
	using ClientConfigurationData = SharpPulsar.Impl.Conf.ClientConfigurationData;
	using AuthData = Org.Apache.Pulsar.Common.Api.AuthData;
	using PulsarApi = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi;
	using Commands = Org.Apache.Pulsar.Common.Protocol.Commands;
	using PulsarHandler = Org.Apache.Pulsar.Common.Protocol.PulsarHandler;
	using CommandActiveConsumerChange = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandActiveConsumerChange;
	using CommandAuthChallenge = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandAuthChallenge;
	using CommandCloseConsumer = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandCloseConsumer;
	using CommandCloseProducer = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandCloseProducer;
	using CommandConnected = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandConnected;
	using CommandError = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandError;
	using CommandGetLastMessageIdResponse = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandGetLastMessageIdResponse;
	using CommandGetSchemaResponse = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandGetSchemaResponse;
	using CommandGetOrCreateSchemaResponse = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandGetOrCreateSchemaResponse;
	using CommandGetTopicsOfNamespaceResponse = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandGetTopicsOfNamespaceResponse;
	using CommandLookupTopicResponse = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandLookupTopicResponse;
	using CommandMessage = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandMessage;
	using CommandPartitionedTopicMetadataResponse = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandPartitionedTopicMetadataResponse;
	using CommandProducerSuccess = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandProducerSuccess;
	using CommandReachedEndOfTopic = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandReachedEndOfTopic;
	using CommandSendError = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandSendError;
	using CommandSendReceipt = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandSendReceipt;
	using CommandSuccess = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandSuccess;
	using MessageIdData = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.MessageIdData;
	using ServerError = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.ServerError;
	using SchemaVersion = Org.Apache.Pulsar.Common.Protocol.Schema.SchemaVersion;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaInfoUtil = Org.Apache.Pulsar.Common.Protocol.Schema.SchemaInfoUtil;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;
	using Org.Apache.Pulsar.Common.Util.Collections;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class ClientCnx : PulsarHandler
	{

		protected internal readonly Authentication Authentication;
		private State state;

		private readonly ConcurrentLongHashMap<CompletableFuture<ProducerResponse>> pendingRequests = new ConcurrentLongHashMap<CompletableFuture<ProducerResponse>>(16, 1);
		private readonly ConcurrentLongHashMap<CompletableFuture<LookupDataResult>> pendingLookupRequests = new ConcurrentLongHashMap<CompletableFuture<LookupDataResult>>(16, 1);
		// LookupRequests that waiting in client side.
		private readonly LinkedList<Pair<long, Pair<ByteBuf, CompletableFuture<LookupDataResult>>>> waitingLookupRequests;
		private readonly ConcurrentLongHashMap<CompletableFuture<PulsarApi.MessageIdData>> pendingGetLastMessageIdRequests = new ConcurrentLongHashMap<CompletableFuture<PulsarApi.MessageIdData>>(16, 1);
		private readonly ConcurrentLongHashMap<CompletableFuture<IList<string>>> pendingGetTopicsRequests = new ConcurrentLongHashMap<CompletableFuture<IList<string>>>(16, 1);

		private readonly ConcurrentLongHashMap<CompletableFuture<PulsarApi.CommandGetSchemaResponse>> pendingGetSchemaRequests = new ConcurrentLongHashMap<CompletableFuture<PulsarApi.CommandGetSchemaResponse>>(16, 1);
		private readonly ConcurrentLongHashMap<CompletableFuture<PulsarApi.CommandGetOrCreateSchemaResponse>> pendingGetOrCreateSchemaRequests = new ConcurrentLongHashMap<CompletableFuture<PulsarApi.CommandGetOrCreateSchemaResponse>>(16, 1);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.apache.pulsar.common.util.collections.ConcurrentLongHashMap<ProducerImpl<?>> producers = new org.apache.pulsar.common.util.collections.ConcurrentLongHashMap<>(16, 1);
		private readonly ConcurrentLongHashMap<ProducerImpl<object>> producers = new ConcurrentLongHashMap<ProducerImpl<object>>(16, 1);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.apache.pulsar.common.util.collections.ConcurrentLongHashMap<ConsumerImpl<?>> consumers = new org.apache.pulsar.common.util.collections.ConcurrentLongHashMap<>(16, 1);
		private readonly ConcurrentLongHashMap<ConsumerImpl<object>> consumers = new ConcurrentLongHashMap<ConsumerImpl<object>>(16, 1);
		private readonly ConcurrentLongHashMap<TransactionMetaStoreHandler> transactionMetaStoreHandlers = new ConcurrentLongHashMap<TransactionMetaStoreHandler>(16, 1);

		private readonly CompletableFuture<Void> connectionFuture = new CompletableFuture<Void>();
		private readonly ConcurrentLinkedQueue<RequestTime> requestTimeoutQueue = new ConcurrentLinkedQueue<RequestTime>();
		private readonly Semaphore pendingLookupRequestSemaphore;
		private readonly Semaphore maxLookupRequestSemaphore;
		private readonly EventLoopGroup eventLoopGroup;

		private static readonly AtomicIntegerFieldUpdater<ClientCnx> NUMBER_OF_REJECTED_REQUESTS_UPDATER = AtomicIntegerFieldUpdater.newUpdater(typeof(ClientCnx), "numberOfRejectRequests");
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

		protected internal string ProxyToTargetBrokerAddress = null;
		// Remote hostName with which client is connected
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal string RemoteHostNameConflict = null;
		private bool isTlsHostnameVerificationEnable;

		private static readonly DefaultHostnameVerifier HOSTNAME_VERIFIER = new DefaultHostnameVerifier();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.concurrent.ScheduledFuture<?> timeoutTask;
		private ScheduledFuture<object> timeoutTask;

		// Added for mutual authentication.
		protected internal AuthenticationDataProvider AuthenticationDataProvider;

		public enum State
		{
			None,
			SentConnectFrame,
			Ready,
			Failed,
			Connecting
		}

		public class RequestTime
		{
			internal long CreationTimeMs;
			internal long RequestId;

			public RequestTime(long CreationTime, long RequestId) : base()
			{
				this.CreationTimeMs = CreationTime;
				this.RequestId = RequestId;
			}
		}

		public ClientCnx(ClientConfigurationData Conf, EventLoopGroup EventLoopGroup) : this(Conf, EventLoopGroup, Commands.CurrentProtocolVersion)
		{
		}

		public ClientCnx(ClientConfigurationData Conf, EventLoopGroup EventLoopGroup, int ProtocolVersion) : base(Conf.KeepAliveIntervalSeconds, BAMCIS.Util.Concurrent.TimeUnit.SECONDS)
		{
			checkArgument(Conf.MaxLookupRequest > Conf.ConcurrentLookupRequest);
			this.pendingLookupRequestSemaphore = new Semaphore(Conf.ConcurrentLookupRequest, false);
			this.maxLookupRequestSemaphore = new Semaphore(Conf.MaxLookupRequest - Conf.ConcurrentLookupRequest, false);
			this.waitingLookupRequests = Queues.newConcurrentLinkedQueue();
			this.Authentication = Conf.Authentication;
			this.eventLoopGroup = EventLoopGroup;
			this.maxNumberOfRejectedRequestPerConnection = Conf.MaxNumberOfRejectedRequestPerConnection;
			this.operationTimeoutMs = Conf.OperationTimeoutMs;
			this.state = State.None;
			this.isTlsHostnameVerificationEnable = Conf.TlsHostnameVerificationEnable;
			this.protocolVersion = ProtocolVersion;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void channelActive(io.netty.channel.ChannelHandlerContext ctx) throws Exception
		public override void ChannelActive(ChannelHandlerContext Ctx)
		{
			base.ChannelActive(Ctx);
			this.timeoutTask = this.eventLoopGroup.scheduleAtFixedRate(() => checkRequestTimeout(), operationTimeoutMs, operationTimeoutMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);

			if (string.ReferenceEquals(ProxyToTargetBrokerAddress, null))
			{
				if (log.DebugEnabled)
				{
					log.debug("{} Connected to broker", Ctx.channel());
				}
			}
			else
			{
				log.info("{} Connected through proxy to target broker at {}", Ctx.channel(), ProxyToTargetBrokerAddress);
			}
			// Send CONNECT command
			Ctx.writeAndFlush(NewConnectCommand()).addListener(future =>
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
				Ctx.close();
			}
			});
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected io.netty.buffer.ByteBuf newConnectCommand() throws Exception
		public virtual ByteBuf NewConnectCommand()
		{
			// mutual authentication is to auth between `remoteHostName` and this client for this channel.
			// each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
			// and return authData to server.
			AuthenticationDataProvider = Authentication.getAuthData(RemoteHostNameConflict);
			AuthData AuthData = AuthenticationDataProvider.authenticate(AuthData.of(AuthData.INIT_AUTH_DATA));
			return Commands.newConnect(Authentication.AuthMethodName, AuthData, this.protocolVersion, PulsarVersion.Version, ProxyToTargetBrokerAddress, null, null, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void channelInactive(io.netty.channel.ChannelHandlerContext ctx) throws Exception
		public override void ChannelInactive(ChannelHandlerContext Ctx)
		{
			base.ChannelInactive(Ctx);
			log.info("{} Disconnected", Ctx.channel());
			if (!connectionFuture.Done)
			{
				connectionFuture.completeExceptionally(new PulsarClientException("Connection already closed"));
			}

			PulsarClientException E = new PulsarClientException("Disconnected from server at " + Ctx.channel().remoteAddress());

			// Fail out all the pending ops
			pendingRequests.ForEach((key, future) => future.completeExceptionally(E));
			pendingLookupRequests.ForEach((key, future) => future.completeExceptionally(E));
			waitingLookupRequests.forEach(pair => pair.Right.Right.completeExceptionally(E));
			pendingGetLastMessageIdRequests.ForEach((key, future) => future.completeExceptionally(E));
			pendingGetTopicsRequests.ForEach((key, future) => future.completeExceptionally(E));
			pendingGetSchemaRequests.ForEach((key, future) => future.completeExceptionally(E));

			// Notify all attached producers/consumers so they have a chance to reconnect
			producers.ForEach((id, producer) => producer.connectionClosed(this));
			consumers.ForEach((id, consumer) => consumer.connectionClosed(this));
			transactionMetaStoreHandlers.ForEach((id, handler) => handler.connectionClosed(this));

			pendingRequests.Clear();
			pendingLookupRequests.Clear();
			waitingLookupRequests.Clear();
			pendingGetLastMessageIdRequests.Clear();
			pendingGetTopicsRequests.Clear();

			producers.Clear();
			consumers.Clear();

			timeoutTask.cancel(true);
		}

		// Command Handlers

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void exceptionCaught(io.netty.channel.ChannelHandlerContext ctx, Throwable cause) throws Exception
		public override void ExceptionCaught(ChannelHandlerContext Ctx, Exception Cause)
		{
			if (state != State.Failed)
			{
				// No need to report stack trace for known exceptions that happen in disconnections
				log.warn("[{}] Got exception {} : {}", RemoteAddress, Cause.GetType().Name, Cause.Message, IsKnownException(Cause) ? null : Cause);
				state = State.Failed;
			}
			else
			{
				// At default info level, suppress all subsequent exceptions that are thrown when the connection has already
				// failed
				if (log.DebugEnabled)
				{
					log.debug("[{}] Got exception: {}", RemoteAddress, Cause.Message, Cause);
				}
			}

			Ctx.close();
		}

		public static bool IsKnownException(Exception T)
		{
			return T is NativeIoException || T is ClosedChannelException;
		}

		public override void HandleConnected(PulsarApi.CommandConnected Connected)
		{

			if (isTlsHostnameVerificationEnable && !string.ReferenceEquals(RemoteHostNameConflict, null) && !VerifyTlsHostName(RemoteHostNameConflict, Ctx))
			{
				// close the connection if host-verification failed with the broker
				log.warn("[{}] Failed to verify hostname of {}", Ctx.channel(), RemoteHostNameConflict);
				Ctx.close();
				return;
			}

			checkArgument(state == State.SentConnectFrame || state == State.Connecting);
			if (Connected.hasMaxMessageSize())
			{
				if (log.DebugEnabled)
				{
					log.debug("{} Connection has max message size setting, replace old frameDecoder with " + "server frame size {}", Ctx.channel(), Connected.MaxMessageSize);
				}
				maxMessageSize = Connected.MaxMessageSize;
				Ctx.pipeline().replace("frameDecoder", "newFrameDecoder", new LengthFieldBasedFrameDecoder(Connected.MaxMessageSize + Commands.MESSAGE_SIZE_FRAME_PADDING, 0, 4, 0, 4));
			}
			if (log.DebugEnabled)
			{
				log.debug("{} Connection is ready", Ctx.channel());
			}
			// set remote protocol version to the correct version before we complete the connection future
			RemoteEndpointProtocolVersionConflict = Connected.ProtocolVersion;
			connectionFuture.complete(null);
			state = State.Ready;
		}

		public override void HandleAuthChallenge(PulsarApi.CommandAuthChallenge AuthChallenge)
		{
			checkArgument(AuthChallenge.hasChallenge());
			checkArgument(AuthChallenge.Challenge.hasAuthData());

			// mutual authn. If auth not complete, continue auth; if auth complete, complete connectionFuture.
			try
			{
				AuthData AuthData = AuthenticationDataProvider.authenticate(AuthData.of(AuthChallenge.Challenge.AuthData.toByteArray()));

				checkState(!AuthData.Complete);

				ByteBuf Request = Commands.newAuthResponse(Authentication.AuthMethodName, AuthData, this.protocolVersion, PulsarVersion.Version);

				if (log.DebugEnabled)
				{
					log.debug("{} Mutual auth {}", Ctx.channel(), Authentication.AuthMethodName);
				}

				Ctx.writeAndFlush(Request).addListener(writeFuture =>
				{
				if (!writeFuture.Success)
				{
					log.warn("{} Failed to send request for mutual auth to broker: {}", Ctx.channel(), writeFuture.cause().Message);
					connectionFuture.completeExceptionally(writeFuture.cause());
				}
				});
				state = State.Connecting;
			}
			catch (Exception E)
			{
				log.error("{} Error mutual verify: {}", Ctx.channel(), E);
				connectionFuture.completeExceptionally(E);
				return;
			}
		}

		public override void HandleSendReceipt(PulsarApi.CommandSendReceipt SendReceipt)
		{
			checkArgument(state == State.Ready);

			long ProducerId = SendReceipt.ProducerId;
			long SequenceId = SendReceipt.SequenceId;
			long HighestSequenceId = SendReceipt.HighestSequenceId;
			long LedgerId = -1;
			long EntryId = -1;
			if (SendReceipt.hasMessageId())
			{
				LedgerId = SendReceipt.MessageId.LedgerId;
				EntryId = SendReceipt.MessageId.EntryId;
			}

			if (LedgerId == -1 && EntryId == -1)
			{
				log.warn("[{}] Message has been dropped for non-persistent topic producer-id {}-{}", Ctx.channel(), ProducerId, SequenceId);
			}

			if (log.DebugEnabled)
			{
				log.debug("{} Got receipt for producer: {} -- msg: {} -- id: {}:{}", Ctx.channel(), ProducerId, SequenceId, LedgerId, EntryId);
			}

			producers.Get(ProducerId).ackReceived(this, SequenceId, HighestSequenceId, LedgerId, EntryId);
		}

		public override void HandleMessage(PulsarApi.CommandMessage CmdMessage, ByteBuf HeadersAndPayload)
		{
			checkArgument(state == State.Ready);

			if (log.DebugEnabled)
			{
				log.debug("{} Received a message from the server: {}", Ctx.channel(), CmdMessage);
			}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.get(cmdMessage.getConsumerId());
			ConsumerImpl<object> Consumer = consumers.Get(CmdMessage.ConsumerId);
			if (Consumer != null)
			{
				Consumer.messageReceived(CmdMessage.MessageId, CmdMessage.RedeliveryCount, HeadersAndPayload, this);
			}
		}

		public override void HandleActiveConsumerChange(PulsarApi.CommandActiveConsumerChange Change)
		{
			checkArgument(state == State.Ready);

			if (log.DebugEnabled)
			{
				log.debug("{} Received a consumer group change message from the server : {}", Ctx.channel(), Change);
			}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.get(change.getConsumerId());
			ConsumerImpl<object> Consumer = consumers.Get(Change.ConsumerId);
			if (Consumer != null)
			{
				Consumer.activeConsumerChanged(Change.IsActive);
			}
		}

		public override void HandleSuccess(PulsarApi.CommandSuccess Success)
		{
			checkArgument(state == State.Ready);

			if (log.DebugEnabled)
			{
				log.debug("{} Received success response from server: {}", Ctx.channel(), Success.RequestId);
			}
			long RequestId = Success.RequestId;
			CompletableFuture<ProducerResponse> RequestFuture = pendingRequests.Remove(RequestId);
			if (RequestFuture != null)
			{
				RequestFuture.complete(null);
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", Ctx.channel(), Success.RequestId);
			}
		}

		public override void HandleGetLastMessageIdSuccess(PulsarApi.CommandGetLastMessageIdResponse Success)
		{
			checkArgument(state == State.Ready);

			if (log.DebugEnabled)
			{
				log.debug("{} Received success GetLastMessageId response from server: {}", Ctx.channel(), Success.RequestId);
			}
			long RequestId = Success.RequestId;
			CompletableFuture<PulsarApi.MessageIdData> RequestFuture = pendingGetLastMessageIdRequests.Remove(RequestId);
			if (RequestFuture != null)
			{
				RequestFuture.complete(Success.LastMessageId);
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", Ctx.channel(), Success.RequestId);
			}
		}

		public override void HandleProducerSuccess(PulsarApi.CommandProducerSuccess Success)
		{
			checkArgument(state == State.Ready);

			if (log.DebugEnabled)
			{
				log.debug("{} Received producer success response from server: {} - producer-name: {}", Ctx.channel(), Success.RequestId, Success.ProducerName);
			}
			long RequestId = Success.RequestId;
			CompletableFuture<ProducerResponse> RequestFuture = pendingRequests.Remove(RequestId);
			if (RequestFuture != null)
			{
				RequestFuture.complete(new ProducerResponse(Success.ProducerName, Success.LastSequenceId, Success.SchemaVersion.toByteArray()));
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", Ctx.channel(), Success.RequestId);
			}
		}

		public override void HandleLookupResponse(PulsarApi.CommandLookupTopicResponse LookupResult)
		{
			if (log.DebugEnabled)
			{
				log.debug("Received Broker lookup response: {}", LookupResult.Response);
			}

			long RequestId = LookupResult.RequestId;
			CompletableFuture<LookupDataResult> RequestFuture = GetAndRemovePendingLookupRequest(RequestId);

			if (RequestFuture != null)
			{
				if (RequestFuture.CompletedExceptionally)
				{
					if (log.DebugEnabled)
					{
						log.debug("{} Request {} already timed-out", Ctx.channel(), LookupResult.RequestId);
					}
					return;
				}
				// Complete future with exception if : Result.response=fail/null
				if (!LookupResult.hasResponse() || PulsarApi.CommandLookupTopicResponse.LookupType.Failed.Equals(LookupResult.Response))
				{
					if (LookupResult.hasError())
					{
						CheckServerError(LookupResult.Error, LookupResult.Message);
						RequestFuture.completeExceptionally(GetPulsarClientException(LookupResult.Error, LookupResult.Message));
					}
					else
					{
						RequestFuture.completeExceptionally(new PulsarClientException.LookupException("Empty lookup response"));
					}
				}
				else
				{
					RequestFuture.complete(new LookupDataResult(LookupResult));
				}
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", Ctx.channel(), LookupResult.RequestId);
			}
		}

		public override void HandlePartitionResponse(PulsarApi.CommandPartitionedTopicMetadataResponse LookupResult)
		{
			if (log.DebugEnabled)
			{
				log.debug("Received Broker Partition response: {}", LookupResult.Partitions);
			}

			long RequestId = LookupResult.RequestId;
			CompletableFuture<LookupDataResult> RequestFuture = GetAndRemovePendingLookupRequest(RequestId);

			if (RequestFuture != null)
			{
				if (RequestFuture.CompletedExceptionally)
				{
					if (log.DebugEnabled)
					{
						log.debug("{} Request {} already timed-out", Ctx.channel(), LookupResult.RequestId);
					}
					return;
				}
				// Complete future with exception if : Result.response=fail/null
				if (!LookupResult.hasResponse() || PulsarApi.CommandPartitionedTopicMetadataResponse.LookupType.Failed.Equals(LookupResult.Response))
				{
					if (LookupResult.hasError())
					{
						CheckServerError(LookupResult.Error, LookupResult.Message);
						RequestFuture.completeExceptionally(GetPulsarClientException(LookupResult.Error, LookupResult.Message));
					}
					else
					{
						RequestFuture.completeExceptionally(new PulsarClientException.LookupException("Empty lookup response"));
					}
				}
				else
				{
					// return LookupDataResult when Result.response = success/redirect
					RequestFuture.complete(new LookupDataResult(LookupResult.Partitions));
				}
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", Ctx.channel(), LookupResult.RequestId);
			}
		}

		public override void HandleReachedEndOfTopic(PulsarApi.CommandReachedEndOfTopic CommandReachedEndOfTopic)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long consumerId = commandReachedEndOfTopic.getConsumerId();
			long ConsumerId = CommandReachedEndOfTopic.ConsumerId;

			log.info("[{}] Broker notification reached the end of topic: {}", RemoteAddress, ConsumerId);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.get(consumerId);
			ConsumerImpl<object> Consumer = consumers.Get(ConsumerId);
			if (Consumer != null)
			{
				Consumer.setTerminated();
			}
		}

		// caller of this method needs to be protected under pendingLookupRequestSemaphore
		private void AddPendingLookupRequests(long RequestId, CompletableFuture<LookupDataResult> Future)
		{
			pendingLookupRequests.Put(RequestId, Future);
			eventLoopGroup.schedule(() =>
			{
			if (!Future.Done)
			{
				Future.completeExceptionally(new TimeoutException(RequestId + " lookup request timedout after ms " + operationTimeoutMs));
			}
			}, operationTimeoutMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
		}

		private CompletableFuture<LookupDataResult> GetAndRemovePendingLookupRequest(long RequestId)
		{
			CompletableFuture<LookupDataResult> Result = pendingLookupRequests.Remove(RequestId);
			if (Result != null)
			{
				Pair<long, Pair<ByteBuf, CompletableFuture<LookupDataResult>>> FirstOneWaiting = waitingLookupRequests.RemoveFirst();
				if (FirstOneWaiting != null)
				{
					maxLookupRequestSemaphore.release();
					// schedule a new lookup in.
					eventLoopGroup.submit(() =>
					{
					long NewId = FirstOneWaiting.Left;
					CompletableFuture<LookupDataResult> NewFuture = FirstOneWaiting.Right.Right;
					AddPendingLookupRequests(NewId, NewFuture);
					Ctx.writeAndFlush(FirstOneWaiting.Right.Left).addListener(writeFuture =>
					{
						if (!writeFuture.Success)
						{
							log.warn("{} Failed to send request {} to broker: {}", Ctx.channel(), NewId, writeFuture.cause().Message);
							GetAndRemovePendingLookupRequest(NewId);
							NewFuture.completeExceptionally(writeFuture.cause());
						}
					});
					});
				}
				else
				{
					pendingLookupRequestSemaphore.release();
				}
			}
			return Result;
		}

		public override void HandleSendError(PulsarApi.CommandSendError SendError)
		{
			log.warn("{} Received send error from server: {} : {}", Ctx.channel(), SendError.Error, SendError.Message);

			long ProducerId = SendError.ProducerId;
			long SequenceId = SendError.SequenceId;

			switch (SendError.Error)
			{
			case ChecksumError:
				producers.Get(ProducerId).recoverChecksumError(this, SequenceId);
				break;

			case TopicTerminatedError:
				producers.Get(ProducerId).terminated(this);
				break;

			default:
				// By default, for transient error, let the reconnection logic
				// to take place and re-establish the produce again
				Ctx.close();
			break;
			}
		}

		public override void HandleError(PulsarApi.CommandError Error)
		{
			checkArgument(state == State.SentConnectFrame || state == State.Ready);

			log.warn("{} Received error from server: {}", Ctx.channel(), Error.Message);
			long RequestId = Error.RequestId;
			if (Error.Error == PulsarApi.ServerError.ProducerBlockedQuotaExceededError)
			{
				log.warn("{} Producer creation has been blocked because backlog quota exceeded for producer topic", Ctx.channel());
			}
			CompletableFuture<ProducerResponse> RequestFuture = pendingRequests.Remove(RequestId);
			if (RequestFuture != null)
			{
				RequestFuture.completeExceptionally(GetPulsarClientException(Error.Error, Error.Message));
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", Ctx.channel(), Error.RequestId);
			}
		}

		public override void HandleCloseProducer(PulsarApi.CommandCloseProducer CloseProducer)
		{
			log.info("[{}] Broker notification of Closed producer: {}", RemoteAddress, CloseProducer.ProducerId);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long producerId = closeProducer.getProducerId();
			long ProducerId = CloseProducer.ProducerId;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ProducerImpl<?> producer = producers.get(producerId);
			ProducerImpl<object> Producer = producers.Get(ProducerId);
			if (Producer != null)
			{
				Producer.connectionClosed(this);
			}
			else
			{
				log.warn("Producer with id {} not found while closing producer ", ProducerId);
			}
		}

		public override void HandleCloseConsumer(PulsarApi.CommandCloseConsumer CloseConsumer)
		{
			log.info("[{}] Broker notification of Closed consumer: {}", RemoteAddress, CloseConsumer.ConsumerId);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long consumerId = closeConsumer.getConsumerId();
			long ConsumerId = CloseConsumer.ConsumerId;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.get(consumerId);
			ConsumerImpl<object> Consumer = consumers.Get(ConsumerId);
			if (Consumer != null)
			{
				Consumer.connectionClosed(this);
			}
			else
			{
				log.warn("Consumer with id {} not found while closing consumer ", ConsumerId);
			}
		}

		public override bool HandshakeCompleted
		{
			get
			{
				return state == State.Ready;
			}
		}

		public virtual CompletableFuture<LookupDataResult> NewLookup(ByteBuf Request, long RequestId)
		{
			CompletableFuture<LookupDataResult> Future = new CompletableFuture<LookupDataResult>();

			if (pendingLookupRequestSemaphore.tryAcquire())
			{
				AddPendingLookupRequests(RequestId, Future);
				Ctx.writeAndFlush(Request).addListener(writeFuture =>
				{
				if (!writeFuture.Success)
				{
					log.warn("{} Failed to send request {} to broker: {}", Ctx.channel(), RequestId, writeFuture.cause().Message);
					GetAndRemovePendingLookupRequest(RequestId);
					Future.completeExceptionally(writeFuture.cause());
				}
				});
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug("{} Failed to add lookup-request into pending queue", RequestId);
				}

				if (maxLookupRequestSemaphore.tryAcquire())
				{
					waitingLookupRequests.AddLast(Pair.of(RequestId, Pair.of(Request, Future)));
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug("{} Failed to add lookup-request into waiting queue", RequestId);
					}
					Future.completeExceptionally(new PulsarClientException.TooManyRequestsException(string.Format("Requests number out of config: There are {{{0}}} lookup requests outstanding and {{{1}}} requests pending.", pendingLookupRequests.Size(), waitingLookupRequests.Count)));
				}
			}
			return Future;
		}

		public virtual CompletableFuture<IList<string>> NewGetTopicsOfNamespace(ByteBuf Request, long RequestId)
		{
			CompletableFuture<IList<string>> Future = new CompletableFuture<IList<string>>();

			pendingGetTopicsRequests.Put(RequestId, Future);
			Ctx.writeAndFlush(Request).addListener(writeFuture =>
			{
			if (!writeFuture.Success)
			{
				log.warn("{} Failed to send request {} to broker: {}", Ctx.channel(), RequestId, writeFuture.cause().Message);
				pendingGetTopicsRequests.Remove(RequestId);
				Future.completeExceptionally(writeFuture.cause());
			}
			});

			return Future;
		}

		public override void HandleGetTopicsOfNamespaceSuccess(PulsarApi.CommandGetTopicsOfNamespaceResponse Success)
		{
			checkArgument(state == State.Ready);

			long RequestId = Success.RequestId;
			IList<string> Topics = Success.TopicsList;

			if (log.DebugEnabled)
			{
				log.debug("{} Received get topics of namespace success response from server: {} - topics.size: {}", Ctx.channel(), Success.RequestId, Topics.Count);
			}

			CompletableFuture<IList<string>> RequestFuture = pendingGetTopicsRequests.Remove(RequestId);
			if (RequestFuture != null)
			{
				RequestFuture.complete(Topics);
			}
			else
			{
				log.warn("{} Received unknown request id from server: {}", Ctx.channel(), Success.RequestId);
			}
		}

		public override void HandleGetSchemaResponse(PulsarApi.CommandGetSchemaResponse CommandGetSchemaResponse)
		{
			checkArgument(state == State.Ready);

			long RequestId = CommandGetSchemaResponse.RequestId;

			CompletableFuture<PulsarApi.CommandGetSchemaResponse> Future = pendingGetSchemaRequests.Remove(RequestId);
			if (Future == null)
			{
				log.warn("{} Received unknown request id from server: {}", Ctx.channel(), RequestId);
				return;
			}
			Future.complete(CommandGetSchemaResponse);
		}

		public override void HandleGetOrCreateSchemaResponse(PulsarApi.CommandGetOrCreateSchemaResponse CommandGetOrCreateSchemaResponse)
		{
			checkArgument(state == State.Ready);
			long RequestId = CommandGetOrCreateSchemaResponse.RequestId;
			CompletableFuture<PulsarApi.CommandGetOrCreateSchemaResponse> Future = pendingGetOrCreateSchemaRequests.Remove(RequestId);
			if (Future == null)
			{
				log.warn("{} Received unknown request id from server: {}", Ctx.channel(), RequestId);
				return;
			}
			Future.complete(CommandGetOrCreateSchemaResponse);
		}

		public virtual Promise<Void> NewPromise()
		{
			return Ctx.newPromise();
		}

		public virtual ChannelHandlerContext Ctx()
		{
			return Ctx;
		}

		public virtual Channel Channel()
		{
			return Ctx.channel();
		}

		public virtual SocketAddress ServerAddrees()
		{
			return RemoteAddress;
		}

		public virtual CompletableFuture<Void> ConnectionFuture()
		{
			return connectionFuture;
		}

		public virtual CompletableFuture<ProducerResponse> SendRequestWithId(ByteBuf Cmd, long RequestId)
		{
			CompletableFuture<ProducerResponse> Future = new CompletableFuture<ProducerResponse>();
			pendingRequests.Put(RequestId, Future);
			Ctx.writeAndFlush(Cmd).addListener(writeFuture =>
			{
			if (!writeFuture.Success)
			{
				log.warn("{} Failed to send request to broker: {}", Ctx.channel(), writeFuture.cause().Message);
				pendingRequests.Remove(RequestId);
				Future.completeExceptionally(writeFuture.cause());
			}
			});
			requestTimeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), RequestId));
			return Future;
		}

		public virtual CompletableFuture<PulsarApi.MessageIdData> SendGetLastMessageId(ByteBuf Request, long RequestId)
		{
			CompletableFuture<PulsarApi.MessageIdData> Future = new CompletableFuture<PulsarApi.MessageIdData>();

			pendingGetLastMessageIdRequests.Put(RequestId, Future);

			Ctx.writeAndFlush(Request).addListener(writeFuture =>
			{
			if (!writeFuture.Success)
			{
				log.warn("{} Failed to send GetLastMessageId request to broker: {}", Ctx.channel(), writeFuture.cause().Message);
				pendingGetLastMessageIdRequests.Remove(RequestId);
				Future.completeExceptionally(writeFuture.cause());
			}
			});

			return Future;
		}

		public virtual CompletableFuture<Optional<SchemaInfo>> SendGetSchema(ByteBuf Request, long RequestId)
		{
			return SendGetRawSchema(Request, RequestId).thenCompose(commandGetSchemaResponse =>
			{
			if (commandGetSchemaResponse.hasErrorCode())
			{
				ServerError Rc = commandGetSchemaResponse.ErrorCode;
				if (Rc == ServerError.TopicNotFound)
				{
					return CompletableFuture.completedFuture(null);
				}
				else
				{
					return FutureUtil.failedFuture(GetPulsarClientException(Rc, commandGetSchemaResponse.ErrorMessage));
				}
			}
			else
			{
				return CompletableFuture.completedFuture(SchemaInfoUtil.newSchemaInfo(commandGetSchemaResponse.Schema));
			}
			});
		}

		public virtual CompletableFuture<PulsarApi.CommandGetSchemaResponse> SendGetRawSchema(ByteBuf Request, long RequestId)
		{
			CompletableFuture<PulsarApi.CommandGetSchemaResponse> Future = new CompletableFuture<PulsarApi.CommandGetSchemaResponse>();

			pendingGetSchemaRequests.Put(RequestId, Future);

			Ctx.writeAndFlush(Request).addListener(writeFuture =>
			{
			if (!writeFuture.Success)
			{
				log.warn("{} Failed to send GetSchema request to broker: {}", Ctx.channel(), writeFuture.cause().Message);
				pendingGetLastMessageIdRequests.Remove(RequestId);
				Future.completeExceptionally(writeFuture.cause());
			}
			});

			return Future;
		}

		public virtual CompletableFuture<sbyte[]> SendGetOrCreateSchema(ByteBuf Request, long RequestId)
		{
			CompletableFuture<PulsarApi.CommandGetOrCreateSchemaResponse> Future = new CompletableFuture<PulsarApi.CommandGetOrCreateSchemaResponse>();
			pendingGetOrCreateSchemaRequests.Put(RequestId, Future);
			Ctx.writeAndFlush(Request).addListener(writeFuture =>
			{
			if (!writeFuture.Success)
			{
				log.warn("{} Failed to send GetOrCreateSchema request to broker: {}", Ctx.channel(), writeFuture.cause().Message);
				pendingGetOrCreateSchemaRequests.Remove(RequestId);
				Future.completeExceptionally(writeFuture.cause());
			}
			});
			return Future.thenCompose(response =>
			{
			if (response.hasErrorCode())
			{
				ServerError Rc = response.ErrorCode;
				if (Rc == ServerError.TopicNotFound)
				{
					return CompletableFuture.completedFuture(SchemaVersion.Empty.bytes());
				}
				else
				{
					return FutureUtil.failedFuture(GetPulsarClientException(Rc, response.ErrorMessage));
				}
			}
			else
			{
				return CompletableFuture.completedFuture(response.SchemaVersion.toByteArray());
			}
			});
		}

		public override void HandleNewTxnResponse(PulsarApi.CommandNewTxnResponse Command)
		{
			TransactionMetaStoreHandler Handler = CheckAndGetTransactionMetaStoreHandler(Command.TxnidMostBits);
			if (Handler != null)
			{
				Handler.handleNewTxnResponse(Command);
			}
		}

		public override void HandleAddPartitionToTxnResponse(PulsarApi.CommandAddPartitionToTxnResponse Command)
		{
			TransactionMetaStoreHandler Handler = CheckAndGetTransactionMetaStoreHandler(Command.TxnidMostBits);
			if (Handler != null)
			{
				Handler.handleAddPublishPartitionToTxnResponse(Command);
			}
		}

		public override void HandleEndTxnResponse(PulsarApi.CommandEndTxnResponse Command)
		{
			TransactionMetaStoreHandler Handler = CheckAndGetTransactionMetaStoreHandler(Command.TxnidMostBits);
			if (Handler != null)
			{
				Handler.handleEndTxnResponse(Command);
			}
		}

		private TransactionMetaStoreHandler CheckAndGetTransactionMetaStoreHandler(long TcId)
		{
			TransactionMetaStoreHandler Handler = transactionMetaStoreHandlers.Get(TcId);
			if (Handler == null)
			{
				Channel().close();
				log.warn("Close the channel since can't get the transaction meta store handler, will reconnect later.");
			}
			return Handler;
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
		private void CheckServerError(PulsarApi.ServerError Error, string ErrMsg)
		{
			if (PulsarApi.ServerError.ServiceNotReady.Equals(Error))
			{
				log.error("{} Close connection because received internal-server error {}", Ctx.channel(), ErrMsg);
				Ctx.close();
			}
			else if (PulsarApi.ServerError.TooManyRequests.Equals(Error))
			{
				long RejectedRequests = NUMBER_OF_REJECTED_REQUESTS_UPDATER.getAndIncrement(this);
				if (RejectedRequests == 0)
				{
					// schedule timer
					eventLoopGroup.schedule(() => NUMBER_OF_REJECTED_REQUESTS_UPDATER.set(ClientCnx.this, 0), rejectedRequestResetTimeSec, BAMCIS.Util.Concurrent.TimeUnit.SECONDS);
				}
				else if (RejectedRequests >= maxNumberOfRejectedRequestPerConnection)
				{
					log.error("{} Close connection because received {} rejected request in {} seconds ", Ctx.channel(), NUMBER_OF_REJECTED_REQUESTS_UPDATER.get(ClientCnx.this), rejectedRequestResetTimeSec);
					Ctx.close();
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
		private bool VerifyTlsHostName(string Hostname, ChannelHandlerContext Ctx)
		{
			ChannelHandler SslHandler = Ctx.channel().pipeline().get("tls");

			SSLSession SslSession = null;
			if (SslHandler != null)
			{
				SslSession = ((SslHandler) SslHandler).engine().Session;
				if (log.DebugEnabled)
				{
					log.debug("Verifying HostName for {}, Cipher {}, Protocols {}", Hostname, SslSession.CipherSuite, SslSession.Protocol);
				}
				return HOSTNAME_VERIFIER.verify(Hostname, SslSession);
			}
			return false;
		}

		public virtual void RegisterConsumer<T1>(in long ConsumerId, in ConsumerImpl<T1> Consumer)
		{
			consumers.Put(ConsumerId, Consumer);
		}

		public virtual void RegisterProducer<T1>(in long ProducerId, in ProducerImpl<T1> Producer)
		{
			producers.Put(ProducerId, Producer);
		}

		public virtual void RegisterTransactionMetaStoreHandler(in long TransactionMetaStoreId, in TransactionMetaStoreHandler Handler)
		{
			transactionMetaStoreHandlers.Put(TransactionMetaStoreId, Handler);
		}

		public virtual void RemoveProducer(in long ProducerId)
		{
			producers.Remove(ProducerId);
		}

		public virtual void RemoveConsumer(in long ConsumerId)
		{
			consumers.Remove(ConsumerId);
		}

		public virtual InetSocketAddress TargetBroker
		{
			set
			{
				this.ProxyToTargetBrokerAddress = string.Format("{0}:{1:D}", value.HostString, value.Port);
			}
		}

		 public virtual string RemoteHostName
		 {
			 set
			 {
				this.RemoteHostNameConflict = value;
			 }
		 }

		private PulsarClientException GetPulsarClientException(PulsarApi.ServerError Error, string ErrorMsg)
		{
			switch (Error.innerEnumValue)
			{
			case PulsarApi.ServerError.InnerEnum.AuthenticationError:
				return new PulsarClientException.AuthenticationException(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.AuthorizationError:
				return new PulsarClientException.AuthorizationException(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.ProducerBusy:
				return new PulsarClientException.ProducerBusyException(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.ConsumerBusy:
				return new PulsarClientException.ConsumerBusyException(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.MetadataError:
				return new PulsarClientException.BrokerMetadataException(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.PersistenceError:
				return new PulsarClientException.BrokerPersistenceException(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.ServiceNotReady:
				return new PulsarClientException.LookupException(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.TooManyRequests:
				return new PulsarClientException.TooManyRequestsException(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.ProducerBlockedQuotaExceededError:
				return new PulsarClientException.ProducerBlockedQuotaExceededError(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.ProducerBlockedQuotaExceededException:
				return new PulsarClientException.ProducerBlockedQuotaExceededException(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.TopicTerminatedError:
				return new PulsarClientException.TopicTerminatedException(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.IncompatibleSchema:
				return new PulsarClientException.IncompatibleSchemaException(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.TopicNotFound:
				return new PulsarClientException.TopicDoesNotExistException(ErrorMsg);
			case PulsarApi.ServerError.InnerEnum.UnknownError:
			default:
				return new PulsarClientException(ErrorMsg);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public void close()
		public virtual void Close()
		{
		   if (Ctx != null)
		   {
			   Ctx.close();
		   }
		}

		private void CheckRequestTimeout()
		{
			while (!requestTimeoutQueue.Empty)
			{
				RequestTime Request = requestTimeoutQueue.peek();
				if (Request == null || (DateTimeHelper.CurrentUnixTimeMillis() - Request.CreationTimeMs) < operationTimeoutMs)
				{
					// if there is no request that is timed out then exit the loop
					break;
				}
				Request = requestTimeoutQueue.poll();
				CompletableFuture<ProducerResponse> RequestFuture = pendingRequests.Remove(Request.RequestId);
				if (RequestFuture != null && !RequestFuture.Done && RequestFuture.completeExceptionally(new PulsarClientException.TimeoutException(Request.RequestId + " lookup request timedout after ms " + operationTimeoutMs)))
				{
					log.warn("{} request {} timed out after {} ms", Ctx.channel(), Request.RequestId, operationTimeoutMs);
				}
				else
				{
					// request is already completed successfully.
				}
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ClientCnx));
	}

}