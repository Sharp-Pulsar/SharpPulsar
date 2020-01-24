using DotNetty.Buffers;
using SharpPulsar.Api;
using SharpPulsar.Protocol;
using SharpPulsar.Util.Collections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SharpPulsar.Impl.BinaryProtoLookupService;
using SharpPulsar.Protocol.Proto;
using System.Collections.Concurrent;
using System.Threading;
using DotNetty.Transport.Channels;
using SharpPulsar.Util.Atomic;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Exception;
using System.Net;
using System.Linq;
using DotNetty.Codecs;
using Optional;
using SharpPulsar.Common.Schema;
using SharpPulsar.Protocol.Schema;
using Microsoft.Extensions.Logging;
using DotNetty.Handlers.Tls;

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

	public class ClientCnx : PulsarHandler
	{

		protected internal readonly IAuthentication Authentication;
		private State state;

		private readonly ConcurrentDictionary<long, TaskCompletionSource<ProducerResponse>> _pendingRequests = new ConcurrentDictionary<long, TaskCompletionSource<ProducerResponse>>(1, 16);
		private readonly ConcurrentDictionary<long, TaskCompletionSource<LookupDataResult>> _pendingLookupRequests = new ConcurrentDictionary<long, TaskCompletionSource<LookupDataResult>>(1, 16);
		// LookupRequests that waiting in client side.
		private readonly LinkedList<KeyValuePair<long, KeyValuePair<IByteBuffer, TaskCompletionSource<LookupDataResult>>>> _waitingLookupRequests;
		private readonly ConcurrentDictionary<long, TaskCompletionSource<MessageIdData>> _pendingGetLastMessageIdRequests = new ConcurrentDictionary<long, TaskCompletionSource<MessageIdData>>(1, 16);
		private readonly ConcurrentQueue<Task<IList<string>>> _pendingGetTopicsRequests = new ConcurrentQueue<Task<IList<string>>>();

		private readonly ConcurrentDictionary<long, TaskCompletionSource<CommandGetSchemaResponse>> _pendingGetSchemaRequests = new ConcurrentDictionary<long, TaskCompletionSource<CommandGetSchemaResponse>>(1, 16);
		private readonly ConcurrentDictionary<long, TaskCompletionSource<CommandGetOrCreateSchemaResponse>> _pendingGetOrCreateSchemaRequests = new ConcurrentDictionary<long, TaskCompletionSource<CommandGetOrCreateSchemaResponse>>(1, 16);


		private readonly ConcurrentDictionary<long, ProducerImpl<object>> _producers = new ConcurrentDictionary<long, ProducerImpl<object>>(1, 16);
		private readonly ConcurrentDictionary<long, ConsumerImpl<object>> _consumers = new ConcurrentDictionary<long, ConsumerImpl<object>>(1, 16);
		private readonly ConcurrentDictionary<long, TransactionMetaStoreHandler> _transactionMetaStoreHandlers = new ConcurrentDictionary<long, TransactionMetaStoreHandler>(1, 16);

		private  ValueTask _connectionTask = new ValueTask();
		private readonly ConcurrentQueue<RequestTime> _requestTimeoutQueue = new ConcurrentQueue<RequestTime>();
		private readonly Semaphore _pendingLookupRequestSemaphore;
		private readonly Semaphore _maxLookupRequestSemaphore;
		private readonly IEventLoopGroup _eventLoopGroup;

		private static readonly ConcurrentDictionary<ClientCnx, long> _numberOfRejectedrequestsUpdater = new ConcurrentDictionary<ClientCnx, long>();
		private volatile int _numberOfRejectRequests = 0;
		private static int _maxMessageSize = Commands.DefaultMaxMessageSize;

		private readonly int _maxNumberOfRejectedRequestPerConnection;
		private readonly int _rejectedRequestResetTimeSec = 60;
		private readonly int _protocolVersion;
		private readonly long _operationTimeoutMs;

		protected internal string _proxyToTargetBrokerAddress;
		protected internal string _remoteHostName;
		private bool _isTlsHostnameVerificationEnable;

		private static readonly DefaultHostnameVerifier _hostNameVerifier = new DefaultHostnameVerifier();
		private TaskScheduler _timeoutTask;

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

			public RequestTime(long creationTime, long requestId) : base()
			{
				CreationTimeMs = creationTime;
				RequestId = requestId;
			}
		}

		public ClientCnx(ClientConfigurationData Conf, IEventLoopGroup EventLoopGroup) : this(Conf, EventLoopGroup, Commands.CurrentProtocolVersion)
		{
		}

		public ClientCnx(ClientConfigurationData Conf, IEventLoopGroup EventLoopGroup, int ProtocolVersion) : base(Conf.KeepAliveIntervalSeconds, BAMCIS.Util.Concurrent.TimeUnit.SECONDS)
		{
			if (Conf.MaxLookupRequest < Conf.ConcurrentLookupRequest)
				throw new System.Exception("ConcurrentLookupRequest must be less than MaxLookupRequest");
			_pendingLookupRequestSemaphore = new Semaphore(Conf.ConcurrentLookupRequest, Conf.MaxLookupRequest);
			_maxLookupRequestSemaphore = new Semaphore(Conf.MaxLookupRequest - Conf.ConcurrentLookupRequest, Conf.MaxLookupRequest);
			_waitingLookupRequests = new LinkedList<KeyValuePair<long, KeyValuePair<IByteBuffer, Task<LookupDataResult>>>>();
			Authentication = Conf.Authentication;
			_eventLoopGroup = EventLoopGroup;
			_maxNumberOfRejectedRequestPerConnection = Conf.MaxNumberOfRejectedRequestPerConnection;
			_operationTimeoutMs = Conf.OperationTimeoutMs;
			state = State.None;
			_isTlsHostnameVerificationEnable = Conf.TlsHostnameVerificationEnable;
			_protocolVersion = ProtocolVersion;
		}

		public void ChannelActive(IChannelHandlerContext Ctx)
		{
			base.ChannelActive(Ctx);
			_timeoutTask = _eventLoopGroup.Schedule(() => CheckRequestTimeout(), _operationTimeoutMs);

			if (_proxyToTargetBrokerAddress is null)
			{
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("{} Connected to broker", Ctx.Channel);
				}
			}
			else
			{
				log.LogInformation("{} Connected through proxy to target broker at {}", Ctx.Channel, _proxyToTargetBrokerAddress);
			}
			// Send CONNECT command
			Ctx.WriteAndFlushAsync(NewConnectCommand()).ContinueWith(task =>
			{
				if (task.IsCompleted)
				{
					if (log.IsEnabled(LogLevel.Debug))
					{
						log.LogDebug("Complete: {}", task.IsCompleted);
					}
					state = State.SentConnectFrame;
				}
				else
				{
					log.LogWarning("Error during handshake", task.Exception);
					Ctx.CloseAsync();
				}
			});
		}

		public virtual IByteBuffer NewConnectCommand()
		{
			// mutual authentication is to auth between `remoteHostName` and this client for this channel.
			// each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
			// and return authData to server.
			AuthenticationDataProvider = Authentication.GetAuthData(_remoteHostName);
			AuthData AuthData = AuthenticationDataProvider.Authenticate(AuthData.of(AuthData.auth_data));
			return Commands.NewConnect(Authentication.AuthMethodName, AuthData, _protocolVersion, PulsarVersion.Version, _proxyToTargetBrokerAddress, null, null, null);
		}

		public void ChannelInactive(IChannelHandlerContext Ctx)
		{
			base.ChannelInactive(Ctx);
			log.LogInformation("{} Disconnected", Ctx.Channel);
			if (!_connectionTask.IsCompleted)
			{
				_connectionTask = new ValueTask(Task.FromException(new PulsarClientException("Connection already closed")));
			}

			PulsarClientException E = new PulsarClientException("Disconnected from server at " + Ctx.Channel.RemoteAddress);

			// Fail out all the pending ops
			_pendingRequests.ToList().ForEach(x => x.Value = new ValueTask(T));
			_pendingLookupRequests.ForEach((key, future) => future.completeExceptionally(E));
			_waitingLookupRequests.forEach(pair => pair.Right.Right.completeExceptionally(E));
			_pendingGetLastMessageIdRequests.ForEach((key, future) => future.completeExceptionally(E));
			_pendingGetTopicsRequests.ForEach((key, future) => future.completeExceptionally(E));
			_pendingGetSchemaRequests.ForEach((key, future) => future.completeExceptionally(E));

			// Notify all attached producers/consumers so they have a chance to reconnect
			_producers.ForEach((id, producer) => producer.connectionClosed(this));
			_consumers.ForEach((id, consumer) => consumer.connectionClosed(this));
			_transactionMetaStoreHandlers.ForEach((id, handler) => handler.connectionClosed(this));

			_pendingRequests.Clear();
			_pendingLookupRequests.Clear();
			_waitingLookupRequests.Clear();
			_pendingGetLastMessageIdRequests.Clear();
			_pendingGetTopicsRequests.Clear();

			_producers.Clear();
			_consumers.Clear();

			_timeoutTask.cancel(true);
		}

		// Command Handlers
		public void ExceptionCaught(IChannelHandlerContext ctx, System.Exception cause)
		{
			if (state != State.Failed)
			{
				// No need to report stack trace for known exceptions that happen in disconnections
				log.LogWarning("[{}] Got exception {} : {}", ctx.Channel.RemoteAddress, cause.GetType().Name, cause.Message, IsKnownException(Cause) ? null : Cause);
				state = State.Failed;
			}
			else
			{
				// At default info level, suppress all subsequent exceptions that are thrown when the connection has already
				// failed
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("[{}] Got exception: {}", ctx.Channel.RemoteAddress, cause.Message, cause);
				}
			}

			Ctx().CloseAsync();
		}

		public static bool IsKnownException(System.Exception T)
		{
			return T is NativeIoException || T is ClosedChannelException;
		}

		public  void HandleConnected(CommandConnected connected)
		{

			if (_isTlsHostnameVerificationEnable && !string.IsNullOrWhiteSpace(_remoteHostName) && !VerifyTlsHostName(_remoteHostName, Ctx()))
			{
				// close the connection if host-verification failed with the broker
				log.LogWarning("[{}] Failed to verify hostname of {}", Ctx().Channel, _remoteHostName);
				Ctx().CloseAsync();
				return;
			}

			if(state == State.SentConnectFrame || state == State.Connecting)
			if (connected.MaxMessageSize > 0)
			{
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("{} Connection has max message size setting, replace old frameDecoder with " + "server frame size {}", Ctx().Channel, connected.MaxMessageSize);
				}
				_maxMessageSize = connected.MaxMessageSize;
				Ctx().Channel.Pipeline.Replace("frameDecoder", "newFrameDecoder", new LengthFieldBasedFrameDecoder(connected.MaxMessageSize + Commands.MessageSizeFramePadding, 0, 4, 0, 4));
			}
			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("{} Connection is ready", Ctx().Channel);
			}
			// set remote protocol version to the correct version before we complete the connection future
			RemoteEndpointProtocolVersion = connected.ProtocolVersion;
			connectionFuture.complete(null);
			state = State.Ready;
		}

		public void HandleAuthChallenge(CommandAuthChallenge AuthChallenge)
		{
			if(AuthChallenge.Challenge != null)
			if(AuthChallenge.Challenge.auth_data != null)
			// mutual authn. If auth not complete, continue auth; if auth complete, complete connectionFuture.
			try
			{
				AuthData authData = AuthenticationDataProvider.Authenticate(AuthData.of(AuthChallenge.Challenge.auth_data));

						if (!authData.Complete)
							throw new System.Exception();
					IByteBuffer request = Commands.NewAuthResponse(Authentication.AuthMethodName, authData, _protocolVersion, PulsarVersion.Version);

				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("{} Mutual auth {}", Ctx().Channel, Authentication.AuthMethodName);
				}

				Ctx().WriteAndFlushAsync(request).ContinueWith(writeTask =>
				{
					if (writeTask.IsFaulted)
					{
						log.LogWarning("{} Failed to send request for mutual auth to broker: {}", Ctx().Channel, writeTask.Exception.Message);
						connectionFuture.completeExceptionally(writeTask.Exception);
					}
					});
				state = State.Connecting;
			}
			catch (System.Exception E)
			{
				log.LogError("{} Error mutual verify: {}", Ctx().Channel, E);
				connectionFuture.completeExceptionally(E);
				return;
			}
		}

		public void HandleSendReceipt(CommandSendReceipt sendReceipt)
		{
			if (state != State.Ready)
				return;

			long ProducerId = (long)sendReceipt.ProducerId;
			long SequenceId = (long)sendReceipt.SequenceId;
			long HighestSequenceId = (long)sendReceipt.HighestSequenceId;
			long LedgerId = -1;
			long EntryId = -1;
			if (sendReceipt.MessageId != null)
			{
				LedgerId = (long)sendReceipt.MessageId.ledgerId;
				EntryId = (long)sendReceipt.MessageId.entryId;
			}

			if (LedgerId == -1 && EntryId == -1)
			{
				log.LogWarning("[{}] Message has been dropped for non-persistent topic producer-id {}-{}", Ctx().Channel, ProducerId, SequenceId);
			}

			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("{} Got receipt for producer: {} -- msg: {} -- id: {}:{}", Ctx().Channel, ProducerId, SequenceId, LedgerId, EntryId);
			}

			_producers[ProducerId].AckReceived(this, SequenceId, HighestSequenceId, LedgerId, EntryId);
		}

		public void HandleMessage(CommandMessage CmdMessage, IByteBuffer HeadersAndPayload)
		{
			if(state != State.Ready)
				return;

			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("{} Received a message from the server: {}", Ctx().Channel, CmdMessage);
			}
			ConsumerImpl<object> consumer = _consumers[(long)CmdMessage.ConsumerId];
			if (consumer != null)
			{
				consumer.MessageReceived(CmdMessage.MessageId, CmdMessage.RedeliveryCount, HeadersAndPayload, this);
			}
		}

		public void HandleActiveConsumerChange(CommandActiveConsumerChange change)
		{
			if (state != State.Ready)
				return;

			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("{} Received a consumer group change message from the server : {}", Ctx().Channel, change);
			}
			ConsumerImpl<object> consumer = _consumers[(long)change.ConsumerId];
			if (consumer != null)
			{
				consumer.ActiveConsumerChanged(change.IsActive);
			}
		}

		public void HandleSuccess(CommandSuccess success)
		{
			if (state != State.Ready)
				return;

			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("{} Received success response from server: {}", Ctx().Channel, success.RequestId);
			}
			long requestId = (long)success.RequestId;
			_pendingRequests.Remove(requestId, out var requestTask);
			if (requestTask != null)
			{
				requestTask.SetResult(null);
			}
			else
			{
				log.LogWarning("{} Received unknown request id from server: {}", Ctx().Channel, success.RequestId);
			}
		}

		public void HandleGetLastMessageIdSuccess(CommandGetLastMessageIdResponse success)
		{
			if (state != State.Ready)
				return;

			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("{} Received success GetLastMessageId response from server: {}", Ctx().Channel, success.RequestId);
			}
			long RequestId = (long)success.RequestId;
			_pendingGetLastMessageIdRequests.Remove(RequestId, out var requestTask);
			if (requestTask != null)
			{
				requestTask.SetResult(success.LastMessageId);
			}
			else
			{
				log.LogWarning("{} Received unknown request id from server: {}", Ctx().Channel, success.RequestId);
			}
		}

		public void HandleProducerSuccess(CommandProducerSuccess success)
		{
			if (state != State.Ready)
				return;

			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("{} Received producer success response from server: {} - producer-name: {}", Ctx().Channel, success.RequestId, success.ProducerName);
			}
			long requestId = (long)success.RequestId;
			_pendingRequests.Remove(requestId, out var requestTask);
			if (requestTask != null)
			{
				requestTask.SetResult(new ProducerResponse(success.ProducerName, success.LastSequenceId, (sbyte[])(object)success.SchemaVersion));
			}
			else
			{
				log.LogWarning("{} Received unknown request id from server: {}", Ctx().Channel, success.RequestId);
			}
		}

		public void HandleLookupResponse(CommandLookupTopicResponse lookupResult)
		{
			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("Received Broker lookup response: {}", lookupResult.Response);
			}

			long requestId = (long)lookupResult.RequestId;
			var requestTask = GetAndRemovePendingLookupRequest(requestId);

			if (requestTask != null)
			{
				if (requestTask.IsFaulted)
				{
					if (log.IsEnabled(LogLevel.Debug))
					{
						log.LogDebug("{} Request {} already timed-out", Ctx().Channel, lookupResult.RequestId);
					}
					return;
				}
				// Complete future with exception if : Result.response=fail/null
				if (lookupResult.Response != null || CommandLookupTopicResponse.LookupType.Failed.Equals(lookupResult.Response))
				{
					if (lookupResult.Error != null)
					{
						CheckServerError(lookupResult.Error, lookupResult.Message);
						requestTask.se(GetPulsarClientException(LookupResult.Error, LookupResult.Message));
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
				log.LogWarning("{} Received unknown request id from server: {}", Ctx().Channel, LookupResult.RequestId);
			}
		}

		public void HandlePartitionResponse(CommandPartitionedTopicMetadataResponse LookupResult)
		{
			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("Received Broker Partition response: {}", LookupResult.Partitions);
			}

			long RequestId = LookupResult.RequestId;
			Task<LookupDataResult> RequestFuture = GetAndRemovePendingLookupRequest(RequestId);

			if (RequestFuture != null)
			{
				if (RequestFuture.CompletedExceptionally)
				{
					if (log.IsEnabled(LogLevel.Debug))
					{
						log.LogDebug("{} Request {} already timed-out", Ctx().Channel, LookupResult.RequestId);
					}
					return;
				}
				// Complete future with exception if : Result.response=fail/null
				if (!LookupResult.hasResponse() || CommandPartitionedTopicMetadataResponse.LookupType.Failed.Equals(LookupResult.Response))
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
				log.LogWarning("{} Received unknown request id from server: {}", Ctx().Channel, LookupResult.RequestId);
			}
		}

		public void HandleReachedEndOfTopic(CommandReachedEndOfTopic CommandReachedEndOfTopic)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long consumerId = commandReachedEndOfTopic.getConsumerId();
			long ConsumerId = CommandReachedEndOfTopic.ConsumerId;

			log.LogInformation("[{}] Broker notification reached the end of topic: {}", RemoteAddress, ConsumerId);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.get(consumerId);
			ConsumerImpl<object> Consumer = consumers.Get(ConsumerId);
			if (Consumer != null)
			{
				Consumer.setTerminated();
			}
		}

		// caller of this method needs to be protected under pendingLookupRequestSemaphore
		private void AddPendingLookupRequests(long requestId, TaskCompletionSource<LookupDataResult> task)
		{
			_pendingLookupRequests.TryAdd(requestId, task);
			_eventLoopGroup.Schedule(() =>
			{
			if (!Future.Done)
			{
				Future.completeExceptionally(new TimeoutException(RequestId + " lookup request timedout after ms " + operationTimeoutMs));
			}
			}, operationTimeoutMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
		}

		private TaskCompletionSource<LookupDataResult> GetAndRemovePendingLookupRequest(long requestId)
		{
			_pendingLookupRequests.Remove(requestId, out var requestTask);
			if (requestTask != null)
			{
				var firstOneWaiting = _waitingLookupRequests.First();
				if (firstOneWaiting.Key > 1)
				{
					_maxLookupRequestSemaphore.Release();
					// schedule a new lookup in.
					_eventLoopGroup.Execute(() =>
					{
					long newId = firstOneWaiting.Key;
					var newTask = firstOneWaiting.Value.Value;
					AddPendingLookupRequests(newId, newTask);
					Ctx().WriteAndFlushAsync(firstOneWaiting.Value.Key).ContinueWith(writeTask =>
					{
						if (writeTask.IsFaulted)
						{
							log.LogWarning("{} Failed to send request {} to broker: {}", Ctx().Channel, newId, writeTask.Exception.Message);
							GetAndRemovePendingLookupRequest(newId);
							newTask.SetException(writeTask.Exception);
						}
					});
					});
					_waitingLookupRequests.RemoveFirst();
				}
				else
				{
					_pendingLookupRequestSemaphore.Release();
				}
			}
			return requestTask;
		}

		public void HandleSendError(CommandSendError sendError)
		{
			log.LogWarning("{} Received send error from server: {} : {}", Ctx().Channel, sendError.Error, sendError.Message);

			long producerId = (long)sendError.ProducerId;
			long sequenceId = (long)sendError.SequenceId;

			switch (sendError.Error)
			{
			case ServerError.ChecksumError:
				_producers[producerId].RecoverChecksumError(this, sequenceId);
				break;

			case ServerError.TopicTerminatedError:
					_producers[producerId].Terminated(this);
				break;

			default:
				// By default, for transient error, let the reconnection logic
				// to take place and re-establish the produce again
				Ctx().CloseAsync();
			break;
			}
		}

		public void HandleError(CommandError error)
		{
			if (state != State.SentConnectFrame || state != State.Ready)
				return;

			log.LogWarning("{} Received error from server: {}", Ctx().Channel, error.Message);
			long requestId = (long)error.RequestId;
			if (error.Error == ServerError.ProducerBlockedQuotaExceededError)
			{
				log.LogWarning("{} Producer creation has been blocked because backlog quota exceeded for producer topic", Ctx().Channel);
			}
			_pendingRequests.Remove(requestId, out var requestTask);
			if (requestTask != null)
			{
				requestTask.SetException(GetPulsarClientException(error.Error, error.Message));
			}
			else
			{
				log.LogWarning("{} Received unknown request id from server: {}", Ctx().Channel, error.RequestId);
			}
		}

		public void HandleCloseProducer(CommandCloseProducer closeProducer)
		{
			log.LogInformation("[{}] Broker notification of Closed producer: {}", Ctx().Channel.RemoteAddress, closeProducer.ProducerId);
			long ProducerId = (long)closeProducer.ProducerId;
			ProducerImpl<object> producer = _producers[ProducerId];
			if (producer != null)
			{
				producer.ConnectionClosed(this);
			}
			else
			{
				log.LogWarning("Producer with id {} not found while closing producer ", ProducerId);
			}
		}

		public void HandleCloseConsumer(CommandCloseConsumer closeConsumer)
		{
			log.LogInformation("[{}] Broker notification of Closed consumer: {}", Ctx().Channel.RemoteAddress, closeConsumer.ConsumerId);
			long consumerId = (long)closeConsumer.ConsumerId;
			ConsumerImpl<object> consumer = _consumers[consumerId];
			if (consumer != null)
			{
				consumer.ConnectionClosed(this);
			}
			else
			{
				log.LogWarning("Consumer with id {} not found while closing consumer ", consumerId);
			}
		}

		public override bool HandshakeCompleted
		{
			get
			{
				return state == State.Ready;
			}
		}

		public virtual Task<LookupDataResult> NewLookup(IByteBuffer request, long requestId)
		{
			TaskCompletionSource<LookupDataResult> task = new TaskCompletionSource<LookupDataResult>();

			if (_pendingLookupRequestSemaphore.WaitOne())
			{
				AddPendingLookupRequests(requestId, task);
				Ctx().WriteAndFlushAsync(request).ContinueWith(writeTask =>
				{
				if (writeTask.IsFaulted)
				{
					log.LogWarning("{} Failed to send request {} to broker: {}", Ctx().Channel, requestId, writeTask.Exception.Message);
					GetAndRemovePendingLookupRequest(requestId);
					task.SetException(writeTask.Exception);
				}
				});
			}
			else
			{
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("{} Failed to add lookup-request into pending queue", requestId);
				}

				if (_maxLookupRequestSemaphore.WaitOne())
				{
					
					_waitingLookupRequests.AddLast(new LinkedListNode<KeyValuePair<long, KeyValuePair<IByteBuffer, TaskCompletionSource<LookupDataResult>>>> (requestId, new KeyValuePair<IByteBuffer, TaskCompletionSource<LookupDataResult>>(request, task)));
				}
				else
				{
					if (log.IsEnabled(LogLevel.Debug))
					{
						log.LogDebug("{} Failed to add lookup-request into waiting queue", requestId);
					}
					task.SetException(new PulsarClientException.TooManyRequestsException(string.Format("Requests number out of config: There are {{{0}}} lookup requests outstanding and {{{1}}} requests pending.", _pendingLookupRequests.Count, _waitingLookupRequests.Count)));
				}
			}
			return task;
		}

		public virtual Task<IList<string>> NewGetTopicsOfNamespace(ByteBuf Request, long requestId)
		{
			Task<IList<string>> Future = new Task<IList<string>>();

			pendingGetTopicsRequests.Put(RequestId, Future);
			Ctx().WriteAndFlushAsync(request).ContinueWith(writeTask =>
			{
			if (!writeTask.Success)
			{
				log.LogWarning("{} Failed to send request {} to broker: {}", Ctx().Channel, RequestId, writeTask.Exception.Message);
				pendingGetTopicsRequests.Remove(RequestId);
				Future.completeExceptionally(writeTask.Exception);
			}
			});

			return Future;
		}

		public void HandleGetTopicsOfNamespaceSuccess(CommandGetTopicsOfNamespaceResponse Success)
		{
			checkArgument(state == State.Ready);

			long RequestId = Success.RequestId;
			IList<string> Topics = Success.TopicsList;

			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("{} Received get topics of namespace success response from server: {} - topics.size: {}", Ctx().Channel, Success.RequestId, Topics.Count);
			}

			Task<IList<string>> RequestFuture = pendingGetTopicsRequests.Remove(RequestId);
			if (RequestFuture != null)
			{
				RequestFuture.complete(Topics);
			}
			else
			{
				log.LogWarning("{} Received unknown request id from server: {}", Ctx().Channel, Success.RequestId);
			}
		}

		public void HandleGetSchemaResponse(CommandGetSchemaResponse CommandGetSchemaResponse)
		{
			checkArgument(state == State.Ready);

			long RequestId = CommandGetSchemaResponse.RequestId;

			Task<CommandGetSchemaResponse> Future = pendingGetSchemaRequests.Remove(RequestId);
			if (Future == null)
			{
				log.LogWarning("{} Received unknown request id from server: {}", Ctx().Channel, RequestId);
				return;
			}
			Future.complete(CommandGetSchemaResponse);
		}

		public void HandleGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse CommandGetOrCreateSchemaResponse)
		{
			checkArgument(state == State.Ready);
			long RequestId = CommandGetOrCreateSchemaResponse.RequestId;
			Task<CommandGetOrCreateSchemaResponse> Future = pendingGetOrCreateSchemaRequests.Remove(RequestId);
			if (Future == null)
			{
				log.LogWarning("{} Received unknown request id from server: {}", Ctx().Channel, RequestId);
				return;
			}
			Future.complete(CommandGetOrCreateSchemaResponse);
		}

		public virtual Promise<Void> NewPromise()
		{
			return Ctx.newPromise();
		}

		public virtual IChannelHandlerContext Ctx()
		{
			return Ctx;
		}

		public virtual Channel Channel()
		{
			return Ctx().Channel;
		}

		public virtual SocketAddress ServerAddrees()
		{
			return RemoteAddress;
		}

		public virtual ValueTask ConnectionTask()
		{
			return _connectionTask;
		}

		public virtual async ValueTask<ProducerResponse> SendRequestWithId(IByteBuffer cmd, long requestId)
		{
			ValueTask<ProducerResponse> task = new ValueTask<ProducerResponse>();
			_pendingRequests.TryAdd(requestId, task);
			await Ctx().WriteAndFlushAsync(cmd).ContinueWith(writeTask =>
			{
				if (writeTask.IsFaulted)
				{
					log.LogWarning("{} Failed to send request to broker: {}", Ctx().Channel, writeTask.Exception.Message);
					_pendingRequests.TryRemove(requestId, out task);
				}
			});
			_requestTimeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			return task.Result;
		}

		public virtual async ValueTask<MessageIdData> SendGetLastMessageId(IByteBuffer request, long requestId)
		{
			ValueTask<MessageIdData> task = new ValueTask<MessageIdData>();

			_pendingGetLastMessageIdRequests.TryAdd(requestId, task);

			await Ctx().WriteAndFlushAsync(request).ContinueWith(writeTask =>
			{
				if (writeTask.IsFaulted)
				{
					log.LogWarning("{} Failed to send GetLastMessageId request to broker: {}", Ctx().Channel, writeTask.Exception.Message);
					_pendingGetLastMessageIdRequests.TryRemove(requestId, out task);
					Future.completeExceptionally(writeTask.Exception);
				}
			});

			return task.Result;
		}

		public virtual async ValueTask<SchemaInfo> SendGetSchema(IByteBuffer request, long requestId)
		{
			CommandGetSchemaResponse result = await SendGetRawSchema(request, requestId);
			if(result.ErrorCode != null)
			{
				var rc = result.ErrorCode;
				if(rc == ServerError.TopicNotFound)
				{
					throw new NullReferenceException("TopicNotFound");
				}
				else
				{
					throw GetPulsarClientException(rc, result.ErrorMessage);
				}
			}
			else
			{
				return SchemaInfoUtil.NewSchemaInfo(result.Schema);
			}
		}

		public virtual ValueTask<CommandGetSchemaResponse> SendGetRawSchema(IByteBuffer request, long requestId)
		{
			ValueTask<CommandGetSchemaResponse> task = new ValueTask<CommandGetSchemaResponse>();

			_pendingGetSchemaRequests.GetOrAdd(requestId, task);

			Ctx().WriteAndFlushAsync(request).ContinueWith(writeTask =>
			{
			if (writeTask.IsFaulted)
			{
				log.LogWarning("{} Failed to send GetSchema request to broker: {}", Ctx().Channel, writeTask.Exception.Message);
				_pendingGetLastMessageIdRequests.TryRemove(requestId, out var t);
				Future.completeExceptionally(writeTask.Exception);
			}
			});

			return task;
		}

		public virtual Task<sbyte[]> SendGetOrCreateSchema(IByteBuffer request, long requestId)
		{
			ValueTask<CommandGetOrCreateSchemaResponse> task = new ValueTask<CommandGetOrCreateSchemaResponse>();
			_pendingGetOrCreateSchemaRequests.TryAdd(requestId, task);
			Ctx().WriteAndFlushAsync(request).ContinueWith(writeTask =>
			{
				if (writeTask.IsFaulted)
				{
					log.LogWarning("{} Failed to send GetOrCreateSchema request to broker: {}", Ctx().Channel, writeTask.Exception.Message);
					_pendingGetOrCreateSchemaRequests.TryRemove(requestId, out var t);
					Future.completeExceptionally(writeTask.Exception);
				}
			});
			return Future.thenCompose(response =>
			{
			if (response.hasErrorCode())
			{
				ServerError Rc = response.ErrorCode;
				if (Rc == ServerError.TopicNotFound)
				{
					return Task.completedFuture(SchemaVersion.Empty.bytes());
				}
				else
				{
					return FutureUtil.failedFuture(GetPulsarClientException(Rc, response.ErrorMessage));
				}
			}
			else
			{
				return Task.completedFuture(response.SchemaVersion.toByteArray());
			}
			});
		}

		public void HandleNewTxnResponse(CommandNewTxnResponse command)
		{
			TransactionMetaStoreHandler handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
			if (handler != null)
			{
				handler.HandleNewTxnResponse(command);
			}
		}

		public void HandleAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse command)
		{
			TransactionMetaStoreHandler handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
			if (handler != null)
			{
				handler.HandleAddPublishPartitionToTxnResponse(command);
			}
		}

		public void HandleEndTxnResponse(CommandEndTxnResponse command)
		{
			TransactionMetaStoreHandler handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
			if (handler != null)
			{
				handler.HandleEndTxnResponse(command);
			}
		}

		private TransactionMetaStoreHandler CheckAndGetTransactionMetaStoreHandler(long TcId)
		{
			TransactionMetaStoreHandler handler = _transactionMetaStoreHandlers[TcId];
			if (handler == null)
			{
				Channel().close();
				log.LogWarning("Close the channel since can't get the transaction meta store handler, will reconnect later.");
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
		private void CheckServerError(ServerError Error, string ErrMsg)
		{
			if (ServerError.ServiceNotReady.Equals(Error))
			{
				log.LogError("{} Close connection because received internal-server error {}", Ctx().Channel, ErrMsg);
				Ctx().CloseAsync();
			}
			else if (ServerError.TooManyRequests.Equals(Error))
			{
				long rejectedRequests = _numberOfRejectedrequestsUpdater[this];
				if (rejectedRequests == 0)
				{
					// schedule timer
					_eventLoopGroup.Schedule(x => _numberOfRejectedrequestsUpdater.TryAdd(this, 0), _rejectedRequestResetTimeSec, BAMCIS.Util.Concurrent.TimeUnit.SECONDS);
				}
				else if (rejectedRequests >= _maxNumberOfRejectedRequestPerConnection)
				{
					log.LogError("{} Close connection because received {} rejected request in {} seconds ", Ctx().Channel,_numberOfRejectedrequestsUpdater[this], _rejectedRequestResetTimeSec);
					Ctx().CloseAsync();
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
		private bool VerifyTlsHostName(string Hostname, IChannelHandlerContext Ctx)
		{
			IChannelHandler SslHandler = Ctx.Channel.Pipeline.Get("tls");

			TlsSession SslSession = null;
			if (SslHandler != null)
			{
				SslSession = ((TlsHandler) SslHandler);
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("Verifying HostName for {}, Cipher {}, Protocols {}", Hostname, SslSession.CipherSuite, SslSession.Protocol);
				}
				return HOSTNAME_VERIFIER.verify(Hostname, SslSession);
			}
			return false;
		}

		public virtual void RegisterConsumer(in long consumerId, in ConsumerImpl<object> consumer)
		{
			_consumers.TryAdd(consumerId, consumer);
		}

		public virtual void RegisterProducer(in long producerId, in ProducerImpl<object> producer)
		{
			_producers.TryAdd(producerId, producer);
		}

		public virtual void RegisterTransactionMetaStoreHandler(in long transactionMetaStoreId, in TransactionMetaStoreHandler handler)
		{
			_transactionMetaStoreHandlers.TryAdd(transactionMetaStoreId, handler);
		}

		public virtual void RemoveProducer(in long producerId)
		{
			_producers.Remove(producerId, out var p);
		}

		public virtual void RemoveConsumer(in long consumerId)
		{
			_consumers.Remove(consumerId, out var c);
		}

		public virtual IPEndPoint TargetBroker
		{
			set
			{
				_proxyToTargetBrokerAddress = string.Format("{0}:{1:D}", value.Address.ToString(), value.Port);
			}
		}

		 public virtual string RemoteHostName
		 {
			 set
			 {
				_remoteHostName = value;
			 }
		 }


		private PulsarClientException GetPulsarClientException(ServerError error, string ErrorMsg)
		{
			switch (error)
			{
				case ServerError.AuthenticationError:
					return new PulsarClientException.AuthenticationException(ErrorMsg);
				case ServerError.AuthorizationError:
					return new PulsarClientException.AuthorizationException(ErrorMsg);
				case ServerError.ProducerBusy:
					return new PulsarClientException.ProducerBusyException(ErrorMsg);
				case ServerError.ConsumerBusy:
					return new PulsarClientException.ConsumerBusyException(ErrorMsg);
				case ServerError.MetadataError:
					return new PulsarClientException.BrokerMetadataException(ErrorMsg);
				case ServerError.PersistenceError:
					return new PulsarClientException.BrokerPersistenceException(ErrorMsg);
				case ServerError.ServiceNotReady:
					return new PulsarClientException.LookupException(ErrorMsg);
				case ServerError.TooManyRequests:
					return new PulsarClientException.TooManyRequestsException(ErrorMsg);
				case ServerError.ProducerBlockedQuotaExceededError:
					return new PulsarClientException.ProducerBlockedQuotaExceededError(ErrorMsg);
				case ServerError.ProducerBlockedQuotaExceededException:
					return new PulsarClientException.ProducerBlockedQuotaExceededException(ErrorMsg);
				case ServerError.TopicTerminatedError:
					return new PulsarClientException.TopicTerminatedException(ErrorMsg);
				case ServerError.IncompatibleSchema:
					return new PulsarClientException.IncompatibleSchemaException(ErrorMsg);
				case ServerError.TopicNotFound:
					return new PulsarClientException.TopicDoesNotExistException(ErrorMsg);
				case ServerError.UnknownError:
				default:
					return new PulsarClientException(ErrorMsg);
			}
		}

		public virtual void Close()
		{
		   if (Ctx() != null)
		   {
			   Ctx().CloseAsync();
		   }
		}

		private void CheckRequestTimeout()
		{
			while (!_requestTimeoutQueue.IsEmpty)
			{
				_requestTimeoutQueue.TryPeek(out var request);
				if (request == null || (DateTimeHelper.CurrentUnixTimeMillis() - request.CreationTimeMs) < _operationTimeoutMs)
				{
					// if there is no request that is timed out then exit the loop
					break;
				}
			    _requestTimeoutQueue.TryDequeue(out request);
				_pendingRequests.Remove(request.RequestId, out var requestTask);
				if (requestTask != null && !requestTask.IsCompleted && requestTask.IsFaulted)
				{
					log.LogWarning("{} request {} timed out after {} ms", Ctx().Channel, request.RequestId, _operationTimeoutMs);
				}
				else
				{
					// request is already completed successfully.
				}
			}
		}

		private static readonly ILogger log = new LoggerFactory().CreateLogger<ClientCnx>();
	}

}