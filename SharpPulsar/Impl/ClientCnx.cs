using DotNetty.Buffers;
using SharpPulsar.Api;
using SharpPulsar.Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SharpPulsar.Impl.BinaryProtoLookupService;
using SharpPulsar.Protocol.Proto;
using System.Collections.Concurrent;
using System.Threading;
using DotNetty.Transport.Channels;
using SharpPulsar.Impl.Conf;
using System.Net;
using System.Linq;
using DotNetty.Codecs;
using SharpPulsar.Common.Schema;
using SharpPulsar.Protocol.Schema;
using Microsoft.Extensions.Logging;
using System.IO;
using BAMCIS.Util.Concurrent;
using DotNetty.Handlers.Tls;
using Google.Protobuf;
using SharpPulsar.Utils;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;

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

	public class ClientCnx:PulsarHandler
	{

		protected internal readonly IAuthentication Authentication;
		private State _state;

		private readonly ConcurrentDictionary<long, TaskCompletionSource<ProducerResponse>> _pendingRequests = new ConcurrentDictionary<long, TaskCompletionSource<ProducerResponse>>(1, 16);
		private readonly ConcurrentDictionary<long, TaskCompletionSource<LookupDataResult>> _pendingLookupRequests = new ConcurrentDictionary<long, TaskCompletionSource<LookupDataResult>>(1, 16);
		// LookupRequests that waiting in client side.
		private readonly LinkedList<KeyValuePair<long, KeyValuePair<IByteBuffer, TaskCompletionSource<LookupDataResult>>>> _waitingLookupRequests;
		private readonly ConcurrentDictionary<long, TaskCompletionSource<MessageIdData>> _pendingGetLastMessageIdRequests = new ConcurrentDictionary<long, TaskCompletionSource<MessageIdData>>(1, 16);
		private readonly ConcurrentDictionary<long, TaskCompletionSource<IList<string>>> _pendingGetTopicsRequests = new ConcurrentDictionary<long, TaskCompletionSource<IList<string>>>();

		private readonly ConcurrentDictionary<long, TaskCompletionSource<CommandGetSchemaResponse>> _pendingGetSchemaRequests = new ConcurrentDictionary<long, TaskCompletionSource<CommandGetSchemaResponse>>(1, 16);
		private readonly ConcurrentDictionary<long, TaskCompletionSource<CommandGetOrCreateSchemaResponse>> _pendingGetOrCreateSchemaRequests = new ConcurrentDictionary<long, TaskCompletionSource<CommandGetOrCreateSchemaResponse>>(1, 16);


		private readonly ConcurrentDictionary<long, ProducerImpl<object>> _producers = new ConcurrentDictionary<long, ProducerImpl<object>>(1, 16);
		private readonly ConcurrentDictionary<long, ConsumerImpl<object>> _consumers = new ConcurrentDictionary<long, ConsumerImpl<object>>(1, 16);
		private readonly ConcurrentDictionary<long, TransactionMetaStoreHandler> _transactionMetaStoreHandlers = new ConcurrentDictionary<long, TransactionMetaStoreHandler>(1, 16);

		private readonly TaskCompletionSource<ClientCnx> _connectionTask = new TaskCompletionSource<ClientCnx>();
		private readonly ConcurrentQueue<RequestTime> _requestTimeoutQueue = new ConcurrentQueue<RequestTime>();
		private readonly Semaphore _pendingLookupRequestSemaphore;
		private readonly Semaphore _maxLookupRequestSemaphore;
		private readonly MultithreadEventLoopGroup _eventLoopGroup;

		private static readonly ConcurrentDictionary<ClientCnx, long> NumberOfRejectedrequestsUpdater = new ConcurrentDictionary<ClientCnx, long>();
        public volatile int NumberOfRejectRequests = 0;
		public static int MaxMessageSize = Commands.DefaultMaxMessageSize;

		private readonly int _maxNumberOfRejectedRequestPerConnection;
		private readonly int _rejectedRequestResetTimeSec = 60;
		private readonly int _protocolVersion;
		private readonly long _operationTimeoutMs;

		protected internal string ProxyToTargetBrokerAddress;
		private string _remoteHostName;
		private readonly bool _isTlsHostnameVerificationEnable;

		private CancellationTokenSource _timeoutTask;

        private ScheduledThreadPoolExecutor _timeoutSchedule;
		// Added for mutual authentication.
		protected internal IAuthenticationDataProvider AuthenticationDataProvider;

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

			public RequestTime(long creationTime, long requestId)
			{
				CreationTimeMs = creationTime;
				RequestId = requestId;
			}
		}

		public ClientCnx(ClientConfigurationData conf, MultithreadEventLoopGroup eventLoopGroup) : this(conf, Commands.CurrentProtocolVersion, eventLoopGroup)
		{
		}

		public ClientCnx(ClientConfigurationData conf, int protocolVersion, MultithreadEventLoopGroup eventLoopGroup) : base(conf.KeepAliveIntervalSeconds, TimeUnit.SECONDS)
		{
			if (conf.MaxLookupRequest < conf.ConcurrentLookupRequest)
				throw new System.Exception("ConcurrentLookupRequest must be less than MaxLookupRequest");
			_pendingLookupRequestSemaphore = new Semaphore(conf.ConcurrentLookupRequest, conf.MaxLookupRequest);
			_maxLookupRequestSemaphore = new Semaphore(conf.MaxLookupRequest - conf.ConcurrentLookupRequest, conf.MaxLookupRequest);
			_waitingLookupRequests = new LinkedList<KeyValuePair<long, KeyValuePair<IByteBuffer, TaskCompletionSource<LookupDataResult>>>>();
			Authentication = conf.Authentication;
			_timeoutSchedule = new ScheduledThreadPoolExecutor(conf.NumIoThreads);
			_maxNumberOfRejectedRequestPerConnection = conf.MaxNumberOfRejectedRequestPerConnection;
			_operationTimeoutMs = conf.OperationTimeoutMs;
			_state = State.None;
			_isTlsHostnameVerificationEnable = conf.TlsHostnameVerificationEnable;
			_protocolVersion = protocolVersion;
            _eventLoopGroup = eventLoopGroup;
        }
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            base.ChannelActive(ctx);
            _timeoutSchedule.ScheduleAtFixedRate(CheckRequestTimeout, TimeSpan.FromMilliseconds(_operationTimeoutMs), TimeSpan.FromMilliseconds(_operationTimeoutMs));

            if (string.ReferenceEquals(ProxyToTargetBrokerAddress, null))
            {
				if (Log.IsEnabled(LogLevel.Debug))
				{
                    Log.LogDebug("{} Connected to broker", ctx.Channel);
                }
            }
            else
            {
                Log.LogInformation("{} Connected through proxy to target broker at {}", ctx.Channel, ProxyToTargetBrokerAddress);
            }
            // Send CONNECT command
            ctx.WriteAndFlushAsync(NewConnectCommand()).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.LogDebug("Complete: {}", task.Status);
                    }
                    _state = State.SentConnectFrame;
                }
                else
                {
                    Log.LogWarning(task.Exception, "Error during handshake");
                    ctx.CloseAsync();
                }
            });
        }
		public IByteBuffer NewConnectCommand()
		{
			// mutual authentication is to auth between `remoteHostName` and this client for this channel.
			// each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
			// and return authData to server.
			AuthenticationDataProvider = Authentication.GetAuthData(_remoteHostName);
            var authData =  AuthenticationDataProvider.Authenticate(new Shared.Auth.AuthData(Shared.Auth.AuthData.InitAuthData));
            
            var auth = AuthData.NewBuilder().SetAuthData(ByteString.CopyFrom((byte[])(object)authData.Bytes)).Build();
            return Commands.NewConnect(Authentication.AuthMethodName, auth, _protocolVersion, null, ProxyToTargetBrokerAddress, string.Empty, null, string.Empty);
		}

		public override void ChannelInactive(IChannelHandlerContext ctx)
		{
			base.ChannelInactive(ctx);
            _eventLoopGroup.Schedule(CheckRequestTimeout, TimeSpan.FromMilliseconds(_operationTimeoutMs));
			Log.LogInformation("{} Disconnected", ctx.Channel);
			if (_connectionTask.Task.IsFaulted)
			{
				_connectionTask.SetException(new PulsarClientException("Connection already closed"));
			}

			var e = new PulsarClientException("Disconnected from server at " + ctx.Channel.RemoteAddress);

			// Fail out all the pending ops
			_pendingRequests.ToList().ForEach(x => x.Value.SetException(e));
			_pendingLookupRequests.ToList().ForEach(x => x.Value.SetException(e));
			_waitingLookupRequests.ToList().ForEach(pair => pair.Value.Value.SetException(e));
			_pendingGetLastMessageIdRequests.ToList().ForEach(x => x.Value.SetException(e));
			_pendingGetTopicsRequests.ToList().ForEach(x => x.Value.SetException(e));
			_pendingGetSchemaRequests.ToList().ForEach(x => x.Value.SetException(e));

			// Notify all attached producers/consumers so they have a chance to reconnect
			_producers.ToList().ForEach(p => p.Value.ConnectionClosed(this));
			_consumers.ToList().ForEach(c => c.Value.ConnectionClosed(this));
			_transactionMetaStoreHandlers.ToList().ForEach(h => h.Value.ConnectionClosed(this));

			_pendingRequests.Clear();
			_pendingLookupRequests.Clear();
			_waitingLookupRequests.Clear();
			_pendingGetLastMessageIdRequests.Clear();
			_pendingGetTopicsRequests.Clear();

			_producers.Clear();
			_consumers.Clear();
            _timeoutSchedule = null;
            //_timeoutTask.Cancel();
        }

		// Command Handlers
		public new void ExceptionCaught(IChannelHandlerContext ctx, System.Exception cause)
		{
			if (_state != State.Failed)
			{
				// No need to report stack trace for known exceptions that happen in disconnections
				Log.LogWarning("[{}] Got exception {} : {}", ctx.Channel.RemoteAddress, cause.GetType().Name, cause.Message, IsKnownException(cause) ? null : cause);
				_state = State.Failed;
			}
			else
			{
				// At default info level, suppress all subsequent exceptions that are thrown when the connection has already
				// failed
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] Got exception: {}", ctx.Channel.RemoteAddress, cause.Message, cause);
				}
			}

			Channel().CloseAsync();
		}

		public static bool IsKnownException(System.Exception T)
		{
			return T is IOException;
		}

		public override void HandleConnected(CommandConnected connected)
		{

			if (_isTlsHostnameVerificationEnable && !string.IsNullOrWhiteSpace(_remoteHostName) && !VerifyTlsHostName(_remoteHostName, Ctx()))
			{
				// close the connection if host-verification failed with the broker
				Log.LogWarning("[{}] Failed to verify hostname of {}", Channel(), _remoteHostName);
				Channel().CloseAsync();
				return;
			}

			if(_state == State.SentConnectFrame || _state == State.Connecting) 
                if (connected.MaxMessageSize > 0)
			    {
				    if (Log.IsEnabled(LogLevel.Debug))
				    {
					    Log.LogDebug("{} Connection has max message size setting, replace old frameDecoder with " + "server frame size {}", Channel(), connected.MaxMessageSize);
				    }
				    MaxMessageSize = connected.MaxMessageSize;
				    Channel().Pipeline.Replace("frameDecoder", "newFrameDecoder", new LengthFieldBasedFrameDecoder(connected.MaxMessageSize + Commands.MessageSizeFramePadding, 0, 4, 0, 4));
			    }
			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("{} Connection is ready", Channel());
			}
			// set remote protocol version to the correct version before we complete the connection future
			RemoteEndpointProtocolVersion = connected.ProtocolVersion;
			_connectionTask.SetResult(null);
			_state = State.Ready;
		}

		public override void HandleAuthChallenge(CommandAuthChallenge authChallenge)
        {
            if (authChallenge.Challenge == null)
                return;
            if (authChallenge.Challenge.AuthData_ == null)
                return;
			// mutual authn. If auth not complete, continue auth; if auth complete, complete connectionFuture.
			try
			{
				var authData = AuthenticationDataProvider.Authenticate(new Shared.Auth.AuthData(authChallenge.Challenge.AuthData_.ToByteArray()));

                if (!authData.Complete)
                    throw new System.Exception();
                var auth = AuthData.NewBuilder().SetAuthData(ByteString.CopyFrom((byte[])(object)authData.Bytes)).Build();
				var request = Commands.NewAuthResponse(Authentication.AuthMethodName, ByteString.CopyFrom((byte[])(object)authData.Bytes), _protocolVersion, string.Empty);

				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("{} Mutual auth {}", Channel(), Authentication.AuthMethodName);
				}

				Channel().WriteAndFlushAsync(request).ContinueWith(writeTask =>
				{
                    if (!writeTask.IsFaulted) return;
                    Log.LogWarning("{} Failed to send request for mutual auth to broker: {}", Channel(), writeTask.Exception.Message);
                    _connectionTask.SetException(writeTask.Exception);
                });
				_state = State.Connecting;
			}
			catch (System.Exception e)
			{
				Log.LogError("{} Error mutual verify: {}", Channel(), e);
				_connectionTask.SetException(e);
            }
		}

		public override void HandleSendReceipt(CommandSendReceipt sendReceipt)
		{
			if (_state != State.Ready)
				return;

			var producerId = (long)sendReceipt.ProducerId;
			var sequenceId = (long)sendReceipt.SequenceId;
			var highestSequenceId = (long)sendReceipt.HighestSequenceId;
			long ledgerId = -1;
			long entryId = -1;
			if (sendReceipt.MessageId != null)
			{
				ledgerId = (long)sendReceipt.MessageId.LedgerId;
				entryId = (long)sendReceipt.MessageId.EntryId;
			}

			if (ledgerId == -1 && entryId == -1)
			{
				Log.LogWarning("[{}] Message has been dropped for non-persistent topic producer-id {}-{}", Channel(), producerId, sequenceId);
			}

			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("{} Got receipt for producer: {} -- msg: {} -- id: {}:{}", Channel(), producerId, sequenceId, ledgerId, entryId);
			}

			_producers[producerId].AckReceived(this, sequenceId, highestSequenceId, ledgerId, entryId);
		}

		public override void HandleMessage(CommandMessage cmdMessage, IByteBuffer headersAndPayload)
		{
			if(_state != State.Ready)
				return;

			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("{} Received a message from the server: {}", Channel(), cmdMessage);
			}
			var consumer = _consumers[(long)cmdMessage.ConsumerId];
            consumer?.MessageReceived(cmdMessage.MessageId, (int)cmdMessage.RedeliveryCount, headersAndPayload, this);
        }

		public override void HandleActiveConsumerChange(CommandActiveConsumerChange change)
		{
			if (_state != State.Ready)
				return;

			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("{} Received a consumer group change message from the server : {}", Channel(), change);
			}
			var consumer = _consumers[(long)change.ConsumerId];
            consumer?.ActiveConsumerChanged(change.IsActive);
        }

		public override void HandleSuccess(CommandSuccess success)
		{
			if (_state != State.Ready)
				return;

			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("{} Received success response from server: {}", Channel(), success.RequestId);
			}
			var requestId = (long)success.RequestId;
			_pendingRequests.Remove(requestId, out var requestTask);
			if (requestTask != null)
			{
				requestTask.SetResult(null);
			}
			else
			{
				Log.LogWarning("{} Received unknown request id from server: {}", Channel(), success.RequestId);
			}
		}

		public override void HandleGetLastMessageIdSuccess(CommandGetLastMessageIdResponse success)
		{
			if (_state != State.Ready)
				return;

			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("{} Received success GetLastMessageId response from server: {}", Channel(), success.RequestId);
			}
			var requestId = (long)success.RequestId;
			_pendingGetLastMessageIdRequests.Remove(requestId, out var requestTask);
			if (requestTask != null)
			{
				requestTask.SetResult(success.LastMessageId);
			}
			else
			{
				Log.LogWarning("{} Received unknown request id from server: {}", Channel(), success.RequestId);
			}
		}

		public override void HandleProducerSuccess(CommandProducerSuccess success)
		{
			if (_state != State.Ready)
				return;

			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("{} Received producer success response from server: {} - producer-name: {}", Channel(), success.RequestId, success.ProducerName);
			}
			var requestId = (long)success.RequestId;
			_pendingRequests.Remove(requestId, out var requestTask);
			if (requestTask != null)
			{
				requestTask.SetResult(new ProducerResponse(success.ProducerName, success.LastSequenceId, (sbyte[])(object)success.SchemaVersion));
			}
			else
			{
				Log.LogWarning("{} Received unknown request id from server: {}", Channel(), success.RequestId);
			}
		}

		public override void HandleLookupResponse(CommandLookupTopicResponse lookupResult)
		{
			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("Received Broker lookup response: {}", lookupResult.Response);
			}

			var requestId = (long)lookupResult.RequestId;
			var requestTask = GetAndRemovePendingLookupRequest(requestId);

			if (requestTask != null)
			{
				if (requestTask.Task.IsFaulted)
				{
					if (Log.IsEnabled(LogLevel.Debug))
					{
						Log.LogDebug("{} Request {} already timed-out", Channel(), lookupResult.RequestId);
					}
					return;
				}
				// Complete future with exception if : Result.response=fail/null
				if (CommandLookupTopicResponse.Types.LookupType.Failed.Equals(lookupResult.Response))
				{
                    CheckServerError(lookupResult.Error, lookupResult.Message);
                    requestTask.SetException(GetPulsarClientException(lookupResult.Error, lookupResult.Message));
                }
				else
				{
					requestTask.SetResult(new LookupDataResult(lookupResult));
				}
			}
			else
			{
				Log.LogWarning("{} Received unknown request id from server: {}", Channel(), lookupResult.RequestId);
			}
		}

		public override void HandlePartitionResponse(CommandPartitionedTopicMetadataResponse lookupResult)
		{
			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("Received Broker Partition response: {}", lookupResult.Partitions);
			}

			var requestId = (long)lookupResult.RequestId;
			var requestTask = GetAndRemovePendingLookupRequest(requestId);

			if (requestTask != null)
			{
				if (requestTask.Task.IsFaulted)
				{
					if (Log.IsEnabled(LogLevel.Debug))
					{
						Log.LogDebug("{} Request {} already timed-out", Channel(), lookupResult.RequestId);
					}
					return;
				}
				// Complete future with exception if : Result.response=fail/null
				if (CommandPartitionedTopicMetadataResponse.Types.LookupType.Failed.Equals(lookupResult.Response))
				{
                    CheckServerError(lookupResult.Error, lookupResult.Message);
                    requestTask.SetException(GetPulsarClientException(lookupResult.Error, lookupResult.Message));
                }
				else
				{
					// return LookupDataResult when Result.response = success/redirect
					requestTask.SetResult(new LookupDataResult((int)lookupResult.Partitions));
				}
			}
			else
			{
				Log.LogWarning("{} Received unknown request id from server: {}", Channel(), lookupResult.RequestId);
			}
		}

		public override void HandleReachedEndOfTopic(CommandReachedEndOfTopic commandReachedEndOfTopic)
		{
			var consumerId = (long)commandReachedEndOfTopic.ConsumerId;

			Log.LogInformation("[{}] Broker notification reached the end of topic: {}", Channel().RemoteAddress, consumerId);
			var consumer = _consumers[consumerId];
            consumer?.SetTerminated();
        }

		// caller of this method needs to be protected under pendingLookupRequestSemaphore
		private void AddPendingLookupRequests(long requestId, TaskCompletionSource<LookupDataResult> task)
		{
			_pendingLookupRequests.TryAdd(requestId, task);
			_eventLoopGroup.Schedule(x =>
			{
				if (!task.Task.IsCompleted)
				{
					task.SetException(new TimeoutException(requestId + " lookup request timedout after ms " + _operationTimeoutMs));
				}
			}, _operationTimeoutMs, TimeSpan.FromMilliseconds(_operationTimeoutMs));
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
					_eventLoopGroup.Execute(async () =>
					{

                        var newId = firstOneWaiting.Key;
                        var newTask = firstOneWaiting.Value.Value;
						try
                        {
                            AddPendingLookupRequests(newId, newTask);
                            await Channel().WriteAndFlushAsync(firstOneWaiting.Value.Key);
                        }
                        catch (Exception e)
                        {
                            Log.LogWarning("{} Failed to send request {} to broker: {}", Channel(), newId, e.Message);
                            GetAndRemovePendingLookupRequest(newId);
                            newTask.SetException(e);
						}
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

		public override void HandleSendError(CommandSendError sendError)
		{
			Log.LogWarning("{} Received send error from server: {} : {}", Channel(), sendError.Error, sendError.Message);

			var producerId = (long)sendError.ProducerId;
			var sequenceId = (long)sendError.SequenceId;

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
				Channel().CloseAsync();
			break;
			}
		}

		public override void HandleError(CommandError error)
		{
			if (_state != State.SentConnectFrame || _state != State.Ready)
				return;

			Log.LogWarning("{} Received error from server: {}", Channel(), error.Message);
			var requestId = (long)error.RequestId;
			if (error.Error == ServerError.ProducerBlockedQuotaExceededError)
			{
				Log.LogWarning("{} Producer creation has been blocked because backlog quota exceeded for producer topic", Channel());
			}
			_pendingRequests.Remove(requestId, out var requestTask);
			if (requestTask != null)
			{
				requestTask.SetException(GetPulsarClientException(error.Error, error.Message));
			}
			else
			{
				Log.LogWarning("{} Received unknown request id from server: {}", Channel(), error.RequestId);
			}
		}

		public override void HandleCloseProducer(CommandCloseProducer closeProducer)
		{
			Log.LogInformation("[{}] Broker notification of Closed producer: {}", Channel().RemoteAddress, closeProducer.ProducerId);
			var producerId = (long)closeProducer.ProducerId;
			var producer = _producers[producerId];
			if (producer != null)
			{
				producer.ConnectionClosed(this);
			}
			else
			{
				Log.LogWarning("Producer with id {} not found while closing producer ", producerId);
			}
		}

		public override void HandleCloseConsumer(CommandCloseConsumer closeConsumer)
		{
			Log.LogInformation("[{}] Broker notification of Closed consumer: {}", Channel().RemoteAddress, closeConsumer.ConsumerId);
			var consumerId = (long)closeConsumer.ConsumerId;
			var consumer = _consumers[consumerId];
			if (consumer != null)
			{
				consumer.ConnectionClosed(this);
			}
			else
			{
				Log.LogWarning("Consumer with id {} not found while closing consumer ", consumerId);
			}
		}

		public override bool HandshakeCompleted => _state == State.Ready;

        public virtual async ValueTask<LookupDataResult> NewLookup(IByteBuffer request, long requestId)
		{
			var task = new TaskCompletionSource<LookupDataResult>();
            if (_pendingLookupRequestSemaphore.WaitOne())
			{
                try
                {
                    AddPendingLookupRequests(requestId, task);
                    await Channel().WriteAndFlushAsync(request);
                }
                catch (Exception e)
                {
                    Log.LogWarning("{} Failed to send request {} to broker: {}", Channel(), requestId, e.Message);
                    GetAndRemovePendingLookupRequest(requestId);
                    task.SetException(e);
                }
                finally
                {
                    _pendingLookupRequestSemaphore.Release();
				}
            }
			else
			{
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("{} Failed to add lookup-request into pending queue", requestId);
				}

				if (_maxLookupRequestSemaphore.WaitOne())
                {
                    var kv = new KeyValuePair<long, KeyValuePair<IByteBuffer, TaskCompletionSource<LookupDataResult>>>(requestId, new KeyValuePair<IByteBuffer, TaskCompletionSource<LookupDataResult>>(request, task));
                    _waitingLookupRequests.AddLast(kv);
                    _maxLookupRequestSemaphore.Release();
                }
				else
				{
					if (Log.IsEnabled(LogLevel.Debug))
					{
						Log.LogDebug("{} Failed to add lookup-request into waiting queue", requestId);
					}
					task.SetException(new PulsarClientException.TooManyRequestsException(
                        $"Requests number out of config: There are {{{_pendingLookupRequests.Count}}} lookup requests outstanding and {{{_waitingLookupRequests.Count}}} requests pending."));
				}
			}
			return task.Task.Result;
		}

		public virtual TaskCompletionSource<IList<string>> NewGetTopicsOfNamespace(IByteBuffer request, long requestId)
		{
			var listTask = new TaskCompletionSource<IList<string>>();

			_pendingGetTopicsRequests.TryAdd(requestId, listTask);
			Channel().WriteAndFlushAsync(request).ContinueWith(writeTask =>
			{
			if (writeTask.IsFaulted)
			{
				Log.LogWarning("{} Failed to send request {} to broker: {}", Channel(), requestId, writeTask.Exception.Message);
				_pendingGetTopicsRequests.Remove(requestId, out listTask);
				listTask.SetException(writeTask.Exception);
			}
			});

			return listTask;
		}

		public override void HandleGetTopicsOfNamespaceSuccess(CommandGetTopicsOfNamespaceResponse success)
		{
			if (_state != State.Ready)
				return;

			var requestId = (long)success.RequestId;
			var topics = success.Topics;

			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("{} Received get topics of namespace success response from server: {} - topics.size: {}", Channel(), success.RequestId, topics.Count);
			}

			_pendingGetTopicsRequests.Remove(requestId, out var requestTask);
			if (requestTask != null)
			{
				requestTask.SetResult(topics);
			}
			else
			{
				Log.LogWarning("{} Received unknown request id from server: {}", Channel(), success.RequestId);
			}
		}

		public override void HandleGetSchemaResponse(CommandGetSchemaResponse commandGetSchemaResponse)
		{
			if (_state != State.Ready)
				return;

			var requestId = (long)commandGetSchemaResponse.RequestId;

			_pendingGetSchemaRequests.Remove(requestId, out var requestTask);
			if (requestTask == null)
			{
				Log.LogWarning("{} Received unknown request id from server: {}", Channel(), requestId);
				return;
			}
			requestTask.SetResult(commandGetSchemaResponse);
		}

		public override void HandleGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse commandGetOrCreateSchemaResponse)
		{
			if (_state != State.Ready)
				return;
			var requestId = (long)commandGetOrCreateSchemaResponse.RequestId;
			_pendingGetOrCreateSchemaRequests.Remove(requestId, out var requestTask);
			if (requestTask == null)
			{
				Log.LogWarning("{} Received unknown request id from server: {}", Channel(), requestId);
				return;
			}
			requestTask.SetResult(commandGetOrCreateSchemaResponse);
		}


		public virtual IChannelHandlerContext Ctx()
		{
			return Context;
		}

		public virtual IChannel Channel()
		{
			return PulsarChannel;
		}

		public virtual EndPoint ServerAddress()
		{
			return Channel().RemoteAddress;
		}

		public virtual TaskCompletionSource<ClientCnx> ConnectionTask()
		{
			return _connectionTask;
		}

		public virtual async ValueTask<ProducerResponse> SendRequestWithId(IByteBuffer cmd, long requestId)
		{
			var task = new TaskCompletionSource<ProducerResponse>();
			_pendingRequests.TryAdd(requestId, task);
			await Channel().WriteAndFlushAsync(cmd).ContinueWith(writeTask =>
			{
				if (writeTask.IsFaulted)
                {
                    if (writeTask.Exception != null)
                        Log.LogWarning("{} Failed to send request to broker: {}", Channel(),
                            writeTask.Exception.Message);
                    _pendingRequests.TryRemove(requestId, out task);
                }
			});
			_requestTimeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			return task.Task.Result;
		}

		public virtual async ValueTask<MessageIdData> SendGetLastMessageId(IByteBuffer request, long requestId)
		{
			var task = new TaskCompletionSource<MessageIdData>();

			_pendingGetLastMessageIdRequests.TryAdd(requestId, task);

			await Channel().WriteAndFlushAsync(request).ContinueWith(writeTask =>
			{
				if (writeTask.IsFaulted)
				{
					Log.LogWarning("{} Failed to send GetLastMessageId request to broker: {}", Channel(), writeTask.Exception.Message);
					_pendingGetLastMessageIdRequests.TryRemove(requestId, out task);
					task.SetException(writeTask.Exception);
				}
			});

			return task.Task.Result;
		}

		public virtual async ValueTask<SchemaInfo> SendGetSchema(IByteBuffer request, long requestId)
        {
            var result = await SendGetRawSchema(request, requestId);
            {
                var rc = result.ErrorCode;
                if(rc == ServerError.TopicNotFound)
                {
                    throw new NullReferenceException("TopicNotFound");
                }

                throw GetPulsarClientException(rc, result.ErrorMessage);
            }
        }

		public virtual async ValueTask<CommandGetSchemaResponse> SendGetRawSchema(IByteBuffer request, long requestId)
		{
			var task = new TaskCompletionSource<CommandGetSchemaResponse>();

			_pendingGetSchemaRequests.GetOrAdd(requestId, task);

			await Channel().WriteAndFlushAsync(request).ContinueWith(writeTask =>
			{
			if (writeTask.IsFaulted)
			{
				Log.LogWarning("{} Failed to send GetSchema request to broker: {}", Channel(), writeTask.Exception.Message);
				_pendingGetLastMessageIdRequests.TryRemove(requestId, out var t);
				task.SetException(writeTask.Exception);
			}
			});

			return task.Task.Result;
		}

		public virtual async ValueTask<sbyte[]> SendGetOrCreateSchema(IByteBuffer request, long requestId)
		{
			var task = new TaskCompletionSource<CommandGetOrCreateSchemaResponse>();
			_pendingGetOrCreateSchemaRequests.TryAdd(requestId, task);
			await Channel().WriteAndFlushAsync(request).ContinueWith(writeTask =>
			{
				if (writeTask.IsFaulted)
				{
					Log.LogWarning("{} Failed to send GetOrCreateSchema request to broker: {}", Channel(), writeTask.Exception.Message);
					_pendingGetOrCreateSchemaRequests.TryRemove(requestId, out var t);
					task.SetException(writeTask.Exception);
				}
			});
			var response = task.Task.Result;
            {
                var rc = response.ErrorCode;
                if (rc == ServerError.TopicNotFound)
                {
                    return SchemaVersionFields.Empty.Bytes();
                }

                throw GetPulsarClientException(rc, response.ErrorMessage);
            }
        }

		public override void HandleNewTxnResponse(CommandNewTxnResponse command)
		{
			var handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
            handler?.HandleNewTxnResponse(command);
        }

		public override void HandleAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse command)
		{
			var handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
            handler?.HandleAddPublishPartitionToTxnResponse(command);
        }

		public override void HandleEndTxnResponse(CommandEndTxnResponse command)
		{
			var handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
            handler?.HandleEndTxnResponse(command);
        }

		private TransactionMetaStoreHandler CheckAndGetTransactionMetaStoreHandler(long TcId)
		{
			var handler = _transactionMetaStoreHandlers[TcId];
			if (handler == null)
			{
				Channel().CloseAsync();
				Log.LogWarning("Close the channel since can't get the transaction meta store handler, will reconnect later.");
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
            switch (Error)
            {
                case ServerError.ServiceNotReady:
                    Log.LogError("{} Close connection because received internal-server error {}", Channel(), ErrMsg);
                    Channel().CloseAsync();
                    break;
                case ServerError.TooManyRequests:
                {
                    var rejectedRequests = NumberOfRejectedrequestsUpdater[this];
                    if (rejectedRequests == 0)
                    {
                        // schedule timer
                        _eventLoopGroup.Schedule(x => NumberOfRejectedrequestsUpdater.TryAdd(this, 0), _rejectedRequestResetTimeSec, TimeSpan.FromSeconds(_rejectedRequestResetTimeSec));
                    }
                    else if (rejectedRequests >= _maxNumberOfRejectedRequestPerConnection)
                    {
                        Log.LogError("{} Close connection because received {} rejected request in {} seconds ", Channel(),NumberOfRejectedrequestsUpdater[this], _rejectedRequestResetTimeSec);
                        Channel().CloseAsync();
                    }

                    break;
                }
                case ServerError.UnknownError:
                    break;
                case ServerError.MetadataError:
                    break;
                case ServerError.PersistenceError:
                    break;
                case ServerError.AuthenticationError:
                    break;
                case ServerError.AuthorizationError:
                    break;
                case ServerError.ConsumerBusy:
                    break;
                case ServerError.ProducerBlockedQuotaExceededError:
                    break;
                case ServerError.ProducerBlockedQuotaExceededException:
                    break;
                case ServerError.ChecksumError:
                    break;
                case ServerError.UnsupportedVersionError:
                    break;
                case ServerError.TopicNotFound:
                    break;
                case ServerError.SubscriptionNotFound:
                    break;
                case ServerError.ConsumerNotFound:
                    break;
                case ServerError.TopicTerminatedError:
                    break;
                case ServerError.ProducerBusy:
                    break;
                case ServerError.InvalidTopicName:
                    break;
                case ServerError.IncompatibleSchema:
                    break;
                case ServerError.ConsumerAssignError:
                    break;
                case ServerError.TransactionCoordinatorNotFound:
                    break;
                case ServerError.InvalidTxnStatus:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Error), Error, null);
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
		private bool VerifyTlsHostName(string hostname, IChannelHandlerContext ctx)
		{
			var sslHandler = ctx.Channel.Pipeline.Get("tls");

			if (sslHandler != null)
			{
				var sslSession = ((TlsHandler) sslHandler);
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("Verifying HostName for {}, Cipher {}, Protocols {}", hostname);
				}
                return new  DefaultHostNameVerifier().Verify(hostname, sslSession);
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
			set => ProxyToTargetBrokerAddress = $"{value.Address.ToString()}:{value.Port:D}";
        }

		 public virtual string RemoteHostName
		 {
			get => _remoteHostName;
            set => _remoteHostName = value;
         }


		private PulsarClientException GetPulsarClientException(ServerError error, string ErrorMsg)
        {
            return error switch
            {
                ServerError.AuthenticationError => new PulsarClientException.AuthenticationException(ErrorMsg),
                ServerError.AuthorizationError => new PulsarClientException.AuthorizationException(ErrorMsg),
                ServerError.ProducerBusy => new PulsarClientException.ProducerBusyException(ErrorMsg),
                ServerError.ConsumerBusy => new PulsarClientException.ConsumerBusyException(ErrorMsg),
                ServerError.MetadataError => new PulsarClientException.BrokerMetadataException(ErrorMsg),
                ServerError.PersistenceError => new PulsarClientException.BrokerPersistenceException(ErrorMsg),
                ServerError.ServiceNotReady => new PulsarClientException.LookupException(ErrorMsg),
                ServerError.TooManyRequests => new PulsarClientException.TooManyRequestsException(ErrorMsg),
                ServerError.ProducerBlockedQuotaExceededError =>
                new PulsarClientException.ProducerBlockedQuotaExceededError(ErrorMsg),
                ServerError.ProducerBlockedQuotaExceededException =>
                new PulsarClientException.ProducerBlockedQuotaExceededException(ErrorMsg),
                ServerError.TopicTerminatedError => new PulsarClientException.TopicTerminatedException(ErrorMsg),
                ServerError.IncompatibleSchema => new PulsarClientException.IncompatibleSchemaException(ErrorMsg),
                ServerError.TopicNotFound => new PulsarClientException.TopicDoesNotExistException(ErrorMsg),
                ServerError.UnknownError => new PulsarClientException(ErrorMsg),
                _ => new PulsarClientException(ErrorMsg)
            };
        }

		public virtual void Close()
		{
		   if (Channel() != null)
		   {
			   Channel().CloseAsync();
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
				if (requestTask != null && !requestTask.Task.IsCompleted)
				{
					requestTask.SetException(new PulsarClientException.TimeoutException(request.RequestId + " lookup request timedout after ms " + _operationTimeoutMs));
					Log.LogWarning("{} request {} timed out after {} ms", Channel(), request.RequestId, _operationTimeoutMs);
				}
            }
		}
		
		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger<ClientCnx>();

        public void RegisterConsumer<T>(in long consumerId, ConsumerImpl<T> consumer)
        {
            var c = (ConsumerImpl<object>)Convert.ChangeType(consumer, typeof(ConsumerImpl<object>));
            _consumers.TryAdd(consumerId, c);
        }
    }

}