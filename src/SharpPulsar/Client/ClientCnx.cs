using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Model;
using SharpPulsar.Common.Precondition;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Akka.Util.Internal;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Common.Entity;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Tls;
using SharpPulsar.Messages.Requests;
using System.Net;
using System.Threading.Tasks;
using SharpPulsar.Client.Internal;
using static SharpPulsar.Client.Internal.SocketClientActor;
using SharpPulsar.API;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Shared.Exceptions;
using DotNetty.Common.Utilities;
using App.Metrics.Concurrency;
using DotNetty.Buffers;
using Pulsar.Proto;
using System.Linq;
using static Pulsar.Proto.CommandTopicMigrated.Types;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Channels;
using SharpPulsar.DotNetty;
using SharpPulsar.Common.Util;
using SharpPulsar.Shared.Buf;
using System.Threading;
using SharpPulsar.Metrics;
using System.Reactive;
using SharpPulsar.Trino.Trino;
using DotNetty.Transport.Channels.Local;
using Microsoft.Extensions.Logging;
using SharpPulsar.Common.Look;
using SharpPulsar.Shared;
using static SharpPulsar.Shared.Exceptions.PulsarClientException;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Policy;
using System.Text;
using System.Threading.Channels;
using SharpPulsar.Common.Protocol.Proto;

namespace SharpPulsar.Client
{
    public class ClientCnx : PulsarHandler
    {
        private readonly IActorRef _socketClient;
        private readonly IAuthentication _authentication;
        private State _state;
        private readonly IActorRef _self;
        private IActorRef _sendMessage;
        private AtomicLong _duplicatedResponseCounter = new AtomicLong(0);
        //private IActorRef _sender;

        protected internal readonly ConcurrentLongHashMap<TimedTaskCompletionSource<object>> _pendingRequests = ConcurrentLongHashMap<TimedTaskCompletionSource<object>>.NewBuilder<TimedTaskCompletionSource<object>>().ExpectedItems(16).ConcurrencyLevel(1).Build();
       
        internal readonly ConcurrentLongHashMap<IActorRef> _producers = ConcurrentLongHashMap<IActorRef>.NewBuilder<IActorRef>().ExpectedItems(16).ConcurrencyLevel(1).Build();
        
        internal readonly ConcurrentLongHashMap<IActorRef> _consumers = ConcurrentLongHashMap<IActorRef>.NewBuilder<IActorRef>().ExpectedItems(16).ConcurrencyLevel(1).Build();
        private readonly ConcurrentLongHashMap<IActorRef> _transactionMetaStoreHandlers = ConcurrentLongHashMap<IActorRef>.NewBuilder<IActorRef>().ExpectedItems(16).ConcurrencyLevel(1).Build();
        
        private readonly ConcurrentLongHashMap<IActorRef> _topicListWatchers = ConcurrentLongHashMap<IActorRef>.NewBuilder<IActorRef>().ExpectedItems(16).ConcurrencyLevel(1).Build();


        //private readonly Dictionary<long, (AbstractByteBuffer Message, IActorRef Requester)> _pendingRequests = new Dictionary<long, (AbstractByteBuffer, IActorRef Requester)>();
        // LookupRequests that waiting in client side.
        private readonly LinkedList<KeyValuePair<long, KeyValuePair<AbstractByteBuffer, TimedTaskCompletionSource<LookupDataResult>>>> _waitingLookupRequests;

        private readonly ConcurrentQueue<RequestTime> _requestTimeoutQueue = new ConcurrentQueue<RequestTime>();

        private readonly Semaphore _pendingLookupRequestSemaphore;
        private readonly Semaphore _maxLookupRequestSemaphore;
        private readonly IEventLoopGroup _eventLoopGroup;

        private static readonly  ClientCnx NumberOfRejectedRequestsUpdate;
        private volatile int _numberOfRejectRequests = 0;

        private int _maxMessageSize = Commands.DefaultMaxMessageSize;
        private readonly int maxNumberOfRejectedRequestPerConnection;
        private readonly int rejectedRequestResetTimeSec = 60;
        protected internal readonly int protocolVersion;
        private readonly long operationTimeoutMs;

        protected internal string _proxyToTargetBrokerAddress = null;
        // Remote hostName with which client is connected
        protected internal string _remoteHostName = null;

        private ScheduledFuture<object> timeoutTask;
        private EndPoint _localAddress;
        private EndPoint _remoteAddress;
        private IChannelHandlerContext _ctx;
        private ClientCnxIdleState _idleState;

        private readonly int _maxNumberOfRejectedRequestPerConnection;
        private readonly int _rejectedRequestResetTimeSec = 60;
        private int _protocolVersion;
        private readonly TimeSpan _operationTimeout;

        private readonly ILoggingAdapter _log;
        private readonly IByteBuffer _pong;
        private readonly List<byte> _pendingReceive;
        private bool _supportsTopicWatchers;
        private readonly bool _isTlsHostnameVerificationEnable;
        private readonly ClientConfigurationData _clientConfigurationData;
        private readonly TaskCompletionSource<ConnectionOpened> _connectionFuture;

        private readonly TlsHostnameVerifier _hostnameVerifier;

        private string _clientVersion;
        private string _originalPrincipal;
        private long _lastDisconnectedTimestamp;
        private bool _brokerSupportsReplDedupByLidAndEid;
        private AtomicCounterLong _connectionsOpenedCounter;
        private AtomicCounterLong _connectionsClosedCounter;
        //private ICancelable _timeoutTask;
        private bool _supportsGetPartitionedMetadataWithoutAutoCreation;
        private readonly ICancelable _sendPing = default;
        private readonly IActorRef _parent;
        private readonly IScheduler _scheduler;

        // Added for mutual authentication.
        private IAuthenticationDataProvider _authenticationDataProvider;
        public ClientCnx(InstrumentProvider instrumentProvider,
                     ClientConfigurationData conf, IEventLoopGroup eventLoopGroup): this(instrumentProvider, conf, eventLoopGroup, Commands.CurrentProtocolVersion)
        {
           
        }
        public ClientCnx(InstrumentProvider instrumentProvider, ClientConfigurationData conf, IEventLoopGroup eventLoopGroup,
                     int protocolVersion): base(conf.KeepAliveIntervalSeconds, TimeUnit.TimeUnit.SECONDS)
        {
            Condition.CheckArgument(conf.MaxLookupRequest > conf.ConcurrentLookupRequest);
            _pendingLookupRequestSemaphore = new Semaphore(conf.ConcurrentLookupRequest, false);
            _maxLookupRequestSemaphore =
                    new Semaphore(conf.MaxLookupRequest - conf.ConcurrentLookupRequest, false);
            _waitingLookupRequests = new LinkedList<KeyValuePair<long, KeyValuePair<AbstractByteBuffer, TimedTaskCompletionSource<LookupDataResult>>>>();
            _authentication = conf.Authentication;
            _eventLoopGroup = eventLoopGroup;
            _maxNumberOfRejectedRequestPerConnection = conf.MaxNumberOfRejectedRequestPerConnection;
            operationTimeoutMs = conf.OperationTimeoutMs;
            _state = State.None;
            this.protocolVersion = protocolVersion;
            _idleState = new ClientCnxIdleState(this);
            _clientVersion = "Pulsar-Java-v" + PulsarVersion.getVersion()
                    + (conf.Description == null ? "" : ("-" + conf.Description));
            _originalPrincipal = conf.OriginalPrincipal;
            _connectionsOpenedCounter =
                    instrumentProvider.NewCounter("pulsar.client.connection.opened", Unit.Connections,
                            "The number of connections opened", null, Attributes.empty());
            _connectionsClosedCounter =
                    instrumentProvider.NewCounter("pulsar.client.connection.closed", Unit.Connections,
                            "The number of connections closed", null, Attributes.empty());

        }
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            base.ChannelActive(ctx);
            _ctx = ctx;
            _connectionsOpenedCounter.Increment();
            _localAddress = ctx.Channel.LocalAddress;
            _remoteAddress = ctx.Channel.RemoteAddress;

            this.timeoutTask = _eventLoopGroup.Schedule.scheduleAtFixedRate(catchingAndLoggingThrowables(this.checkRequestTimeout), operationTimeoutMs, operationTimeoutMs, TimeUnit.MILLISECONDS);

            if (_proxyToTargetBrokerAddress == null)
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("{} Connected to broker", ctx.Channel);
                }
            }
            else
            {
                _log.Info("{} Connected through proxy to target broker at {}", ctx.Channel, _proxyToTargetBrokerAddress);
            }
            // Send CONNECT command
            ctx.WriteAndFlushAsync(NewConnectCommand()).ContinueWith(future =>
            {
                if (future.IsCompletedSuccessfully)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug("Complete: {}", future.IsCompletedSuccessfully);
                    }
                    _state = State.SentConnectFrame;
                }
                else
                {
                    _log.Warning("Error during handshake", future.Exception);
                    ctx.CloseAsync();
                }
            });
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            base.ChannelInactive(ctx);
            _connectionsClosedCounter.increment();
            _lastDisconnectedTimestamp = DateTimeHelper.CurrentUnixTimeMillis();
            _log.Info("{} Disconnected", ctx.Channel);
            if (!_connectionFuture.isDone())
            {
                connectionFuture.completeExceptionally(new PulsarClientException("Connection already closed"));
            }

            ConnectException e = new ConnectException("Disconnected from server at " + ctx.Channel.RemoteAddress);

            // Fail out all the pending ops
            _pendingRequests.ForEach((key, future) =>
            {
                if (pendingRequests.remove(key, future) && !future.isDone())
                {
                    future.completeExceptionally(e);
                }
            });
            _waitingLookupRequests.ForEach(pair => pair.getRight().getRight().completeExceptionally(e));

            // Notify all attached producers/consumers so they have a chance to reconnect
            _producers.ForEach(p => p.Value.Tell(new ConnectionClosed(_self, 0, null)));
            _consumers.ForEach(c => c.Value.Tell(new ConnectionClosed(_self, 0, null)));
            _transactionMetaStoreHandlers.ForEach(t => t.Value.Tell(new ConnectionClosed(_self, 0, null)));
            _topicListWatchers.ForEach(watcher => watcher.Value.Tell(new ConnectionClosed(_self, 0, null)));

            _waitingLookupRequests.Clear();

            _producers.Clear();
            _consumers.Clear();
            _topicListWatchers.Clear();
            Timers.Cancel(RequestTimeout.Instance);

            /* // Notify all attached producers/consumers so they have a chance to reconnect
            producers.forEach((id, producer) => producer.connectionClosed(this, null, null));
            consumers.forEach((id, consumer) => consumer.connectionClosed(this, null, null));
            transactionMetaStoreHandlers.forEach((id, handler) => handler.connectionClosed(this));
            topicListWatchers.forEach((__, watcher) => watcher.connectionClosed(this));

            waitingLookupRequests.clear();

            producers.clear();
            consumers.clear();
            topicListWatchers.clear();

            timeoutTask.cancel(true);*/
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            if (_state != State.Failed)
            {
                // No need to report stack trace for known exceptions that happen in disconnections
                _log.Warning("[{}] Got exception {}", _remoteAddress, ClientCnx.IsKnownException(cause) ? cause : ExceptionUtils.getStackTrace(cause));
                _state = State.Failed;
            }
            else
            {
                // At default info level, suppress all subsequent exceptions that are thrown when the connection has already
                // failed
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("[{}] Got exception: {}", remoteAddress, cause.Message, cause);
                }
            }

            ctx.CloseAsync().GetAwaiter();
        }
        public static bool IsKnownException(Exception t)
        {
            return t is NativeIoException || t is ClosedChannelException;
        }

        public virtual long DuplicatedResponseCount
        {
            get
            {
                return _duplicatedResponseCounter.GetValue();
            }
        }

        protected internal override void HandleConnected(CommandConnected connected)
        {                        
            Condition.CheckArgument(_state == State.SentConnectFrame || _state == State.Connecting);
            if (connected.HasMaxMessageSize)
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("{} Connection has max message size setting, replace old frameDecoder with " + "server frame size {}", _ctx.Channel, connected.MaxMessageSize);
                }
                _maxMessageSize = connected.MaxMessageSize;
                FrameDecoderUtil.ReplaceFrameDecoder(_ctx.Channel.Pipeline, connected.MaxMessageSize);
            }
            if (_log.IsDebugEnabled)
            {
                _log.Debug("{} Connection is ready", _ctx.Channel);
            }

            // set remote protocol version to the correct version before we complete the connection future
            _protocolVersion = connected.ProtocolVersion;
            _state = State.Ready;
            _connectionFuture.TrySetResult(new ConnectionOpened(_self, connected.MaxMessageSize, _protocolVersion));
        }

        protected internal override void HandleAuthChallenge(CommandAuthChallenge authChallenge)
        {
            // mutual authn. If auth not complete, continue auth; if auth complete, complete connectionFuture.
            try
            {
                Condition.CheckArgument(authChallenge.Challenge != null);
                Condition.CheckArgument(authChallenge.Challenge.AuthData_ != null);

                if (Shared.AuthData.RefreshAuthDataBytes.Equals(authChallenge.Challenge.AuthData_))
                {
                    try
                    {
                        _authenticationDataProvider = _authentication.GetAuthData(_remoteHostName);
                    }
                    catch (PulsarClientException e)
                    {
                        _log.Error($"Error when refreshing authentication data provider: {e}");
                        _connectionFuture.TrySetException(e);
                        return;
                    }
                }
                try
                {
                    var authData = _authenticationDataProvider.Authenticate(Shared.AuthData.Of(authChallenge.Challenge.AuthData_.ToArray()));
                    if (!authData.IsComplete())
                    {
                        _connectionFuture.TrySetException(new PulsarClientException.UnsupportedAuthenticationException(new ArgumentException()));
                        return;
                    }
                    var request = Commands.NewAuthResponse(_authentication.AuthMethodName, authData, _protocolVersion, "4.1.1");

                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"Mutual auth {_authentication.AuthMethodName}");
                    }

                    _ctx.WriteAndFlushAsync(request).ContinueWith(writeFuture =>
                    {
                        if (!writeFuture.IsCompletedSuccessfully)
                        {
                            log.warn("{} Failed to send request for mutual auth to broker: {}", _ctx.Channel, writeFuture.Exception.Message);
                            _connectionFuture.TrySetException(writeFuture.Exception);
                        }
                    });

                    if (_state == State.SentConnectFrame)
                    {
                        _state = State.Connecting;
                    }
                }
                catch (Exception ex)
                {
                    _connectionFuture.TrySetException(ex);
                }

            }
            catch (Exception e)
            {
                _log.Error($"Error mutual verify: {e}");
                _connectionFuture.TrySetException(e);
            }
            
        }

        protected internal override void HandleSendReceipt(CommandSendReceipt sendReceipt)
        {
            Condition.CheckArgument(_state == State.Ready);

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

            var producer = _producers.Get(producerId);
            if (ledgerId == -1 && entryId == -1)
            {
                if (producer == null)
                {
                    _log.Warning("{} Message with sequence-id {}-{} published by producer [id:{}, name:{}] has been dropped", _ctx.Channel, sequenceId, highestSequenceId, producerId, "null");
                }
                else
                {
                    producer.Tell(new PrintWarnLogWhenCanNotDetermineDeduplication(_ctx.Channel, sequenceId, highestSequenceId));
                }

            }
            else
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("{} Got receipt for producer: [id:{}, name:{}] -- sequence-id: {}-{} -- entry-id: {}:{}", _ctx.Channel, producerId, producer.Ask<string>(new ProducerName()), sequenceId, highestSequenceId, ledgerId, entryId);
                }
            }

            if (producer != null)
            {
                producer.Tell(new AckReceived(sequenceId, highestSequenceId, ledgerId, entryId));
            }
            else
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Producer is {} already closed, ignore published message [{}-{}]", producerId, ledgerId, entryId);
                }
            }
        }

        protected internal override void HandleAckResponse(CommandAckResponse ackResponse)
        {
            Condition.CheckArgument(_state == State.Ready);
            Condition.CheckArgument(ackResponse.RequestId >= 0);
            var consumerId = (long)ackResponse.ConsumerId;
            var completableFuture = _pendingRequests.Remove((long)ackResponse.RequestId);

            if (completableFuture != null && !completableFuture.TrySetResult(ackResponse))
            {
                if (!ackResponse.HasError)
                {
                    completableFuture.TrySetResult(null);
                }
                else
                {
                    completableFuture.SetException(GetPulsarClientException(ackResponse.Error, BuildError((long)ackResponse.RequestId, ackResponse.Message)));
                }
            }
            else
            {
                _duplicatedResponseCounter.GetAndIncrement();
                _log.Warning("AckResponse has complete when receive response! requestId : {}, consumerId : {}", ackResponse.RequestId, ackResponse.HasConsumerId);
            }
            
        }

        protected internal override void HandleMessage(CommandMessage cmdMessage, AbstractByteBuffer headersAndPayload)
        {
            Condition.CheckArgument(_state == State.Ready);

            if (_log.IsDebugEnabled)
            {
                _log.Debug("{} Received a message from the server: {}", _ctx.Channel, cmdMessage);
            }
            
            var message = new MessageReceived(cmdMessage, headersAndPayload, this);
            var consumer = _consumers.Get((long)cmdMessage.ConsumerId);
            if (consumer != null)
            {
                consumer.Tell(message);
            }
        }

        protected internal override void HandleActiveConsumerChange(CommandActiveConsumerChange change)
        {
            Condition.CheckArgument(_state == State.Ready);

            if (_log.IsDebugEnabled)
            {
                _log.Debug("{} Received a consumer group change message from the server : {}", _ctx.Channel, change);
            }
            
            var consumer = _consumers.Get((long)change.ConsumerId);
            if (consumer != null)
            {
                consumer.Tell(new ActiveConsumerChanged(change.IsActive));
            }
        }

        protected internal override void HandleSuccess(CommandSuccess success)
        {
            Condition.CheckArgument(_state == State.Ready);

            if (_log.IsDebugEnabled)
            {
                _log.Debug($"{_ctx.Channel} Received success response from server: {success.RequestId}");
            }
            var requestId = (long)success.RequestId;
            var requestFuture = _pendingRequests.Remove(requestId);
            if (requestFuture != null)
            {
                requestFuture.SetResult(null);
            }
            else
            {
                _duplicatedResponseCounter.GetAndIncrement();
                _log.Warning($"{_ctx.Channel} Received unknown request id from server: {success.RequestId}");
            }
        }

        protected internal override void HandleGetLastMessageIdSuccess(CommandGetLastMessageIdResponse success)
        {
            Condition.CheckArgument(_state == State.Ready);

            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Received success GetLastMessageId response from server: {success.RequestId}");
            }
            var requestId = (long)success.RequestId;
            var req = (TaskCompletionSource<object>)_pendingRequests.Remove(requestId);
            if (req != null)
            {
                req.SetResult(success);
            }
            else
            {
                _duplicatedResponseCounter.GetAndIncrement();
                _log.Warning($"Received unknown request id from server: {requestId}");
            }
            
        }

        protected internal override void HandleProducerSuccess(CommandProducerSuccess success)
        {
            Condition.CheckArgument(_state == State.Ready);
            if (_log.IsDebugEnabled)
            {
                _log.Debug($" {_ctx.Channel} Received producer success response from server: {success.RequestId} - producer-name: {success.ProducerName}");
            }
            var requestId = (long)success.RequestId;
            if (!success.ProducerReady)
            {
                // We got a success operation but the producer is not ready. This means that the producer has been queued up
                // in broker. We need to leave the future pending until we get the final confirmation. We just mark that
                // we have received a response, in order to avoid the timeout.
                var requestFuture = _pendingRequests.Get(requestId);
                if (requestFuture != null)
                {
                    _log.Info($"{_ctx.Channel} Producer {success.ProducerName} has been queued up at broker. request: {requestId}");
                    requestFuture.MarkAsResponded();
                }
                return;
            }
            var requestFuture = _pendingRequests.Remove(requestId);
            if (requestFuture != null)
            {
                ProducerResponse pr = new ProducerResponse(success.getProducerName(), success.getLastSequenceId(), success.getSchemaVersion(), success.hasTopicEpoch() ? success.getTopicEpoch() : null);
                requestFuture.complete(pr);
            }
            else if (_pendingRequests.TryGetValue(requestId, out var producer))
            {
                _pendingRequests.Remove(requestId);
                producer.Requester.Tell(new AskResponse(new ProducerResponse(success.ProducerName, success.LastSequenceId, success.SchemaVersion.ToArray(), GetTopicEpoch(success), success.ProducerReady)));
            }
            else
            {
                _duplicatedResponseCounter.GetAndIncrement();
                _log.Warning($"Received unknown request id from server: {success.RequestId}");
            }
            
        }

        protected internal override void HandleLookupResponse(CommandLookupTopicResponse lookupResult)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Received Broker lookup response: {lookupResult.Response}");
            }

            var requestId = (long)lookupResult.RequestId;
            if (RemovePendingLookupRequest(requestId, out var requester))
            {

                if (CommandLookupTopicResponse.Types.LookupType.Failed.Equals(lookupResult.Response))
                {
                    if (lookupResult.Error != ServerError.UnknownError)
                    {
                        CheckServerError(lookupResult.Error, lookupResult.Message);
                        var ex = GetPulsarClientException(lookupResult.Error, lookupResult.Message);
                        requester.Tell(new AskResponse(ex));
                    }
                    else
                    {
                        var ex = new PulsarClientException.LookupException("Empty lookup response");
                        requester.Tell(new AskResponse(ex));
                    }
                }
                else
                {
                    requester.Tell(new AskResponse(new LookupDataResult(lookupResult)));
                }
            }
            else
            {
                var msg = $"Received unknown request id from server: {lookupResult.RequestId}";

                _log.Warning(msg);
                var ex = new PulsarClientException.LookupException(msg);
                requester.Tell(new AskResponse(ex));
            }
        }

        protected internal override void HandlePartitionResponse(CommandPartitionedTopicMetadataResponse lookupResult)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Received Broker Partition response: {lookupResult.Partitions}");
            }

            var requestId = (long)lookupResult.RequestId;
            if (RemovePendingLookupRequest(requestId, out var requester))
            {
                if (CommandPartitionedTopicMetadataResponse.Types.LookupType.Failed.Equals(lookupResult?.Response))
                {
                    if (lookupResult != null && lookupResult.Error != ServerError.UnknownError)
                    {
                        CheckServerError(lookupResult.Error, lookupResult.Message);
                        var ex = GetPulsarClientException(lookupResult.Error, lookupResult.Message);
                        requester.Tell(new AskResponse(ex));
                    }
                    else
                    {
                        var ex = new PulsarClientException.LookupException("Empty lookup response");
                        requester.Tell(new AskResponse(ex));
                    }
                }
                else
                {
                    // return LookupDataResult when Result.response = success/redirect
                    requester.Tell(new AskResponse(new LookupDataResult((int)lookupResult.Partitions)));
                }
            }
            else
            {
                var msg = $"Received unknown request id from server: {lookupResult.RequestId}";

                _log.Warning(msg);
                var ex = new PulsarClientException.LookupException(msg);
                requester.Tell(new AskResponse(ex));
            }
            if (_log.IsDebugEnabled)
            {
                CommandPartitionedTopicMetadataResponse.Types.LookupType? response = lookupResult.HasResponse ? lookupResult.Response : null;
                int partitions = lookupResult.HasPartitions ? (int)lookupResult.Partitions : -1;
                _log.Debug($"Received Broker Partition response: {lookupResult.RequestId} {response} {partitions}");
            }

            long requestId = (long)lookupResult.RequestId;
            var requestFuture = GetAndRemovePendingLookupRequest(requestId);

            if (requestFuture != null)
            {
                if (requestFuture.IsFaulted)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"{_ctx.Channel} Request {lookupResult.RequestId} already timed-out");
                    }
                    return;
                }
                // Complete future with exception if : Result.response=fail/null
                if (!lookupResult.HasResponse || CommandPartitionedTopicMetadataResponse.Types.LookupType.Failed.Equals(lookupResult.Response))
                {
                    if (lookupResult.HasError)
                    {
                        string message = BuildError((long)lookupResult.RequestId, lookupResult.HasMessage ? lookupResult.Message : null);
                        CheckServerError(lookupResult.Error, message);
                        requestFuture.completeExceptionally(GetPulsarClientException(lookupResult.Error, message));
                    }
                    else
                    {
                        requestFuture.completeExceptionally(new PulsarClientException.LookupException("Empty lookup response"));
                    }
                }
                else
                {
                    // return LookupDataResult when Result.response = success/redirect
                    requestFuture.complete(new LookupDataResult(lookupResult.getPartitions()));
                }
            }
            else
            {
                _log.Warning($"{_ctx.Channel} Received unknown request id from server: {lookupResult.RequestId}");
            }
        }

        protected internal override void handleReachedEndOfTopic(CommandReachedEndOfTopic commandReachedEndOfTopic)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final long consumerId = commandReachedEndOfTopic.getConsumerId();
            long consumerId = commandReachedEndOfTopic.getConsumerId();

            log.info("[{}] Broker notification reached the end of topic: {}", remoteAddress, consumerId);

            //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
            //ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.get(consumerId);
            ConsumerImpl<object> consumer = consumers.get(consumerId);
            if (consumer != null)
            {
                consumer.setTerminated();
            }
        }

        protected internal override void handleTopicMigrated(CommandTopicMigrated commandTopicMigrated)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final long resourceId = commandTopicMigrated.getResourceId();
            long resourceId = commandTopicMigrated.getResourceId();
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final String serviceUrl = commandTopicMigrated.hasBrokerServiceUrl() ? commandTopicMigrated.getBrokerServiceUrl() : null;
            string serviceUrl = commandTopicMigrated.hasBrokerServiceUrl() ? commandTopicMigrated.getBrokerServiceUrl() : null;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final String serviceUrlTls = commandTopicMigrated.hasBrokerServiceUrlTls() ? commandTopicMigrated.getBrokerServiceUrlTls() : null;
            string serviceUrlTls = commandTopicMigrated.hasBrokerServiceUrlTls() ? commandTopicMigrated.getBrokerServiceUrlTls() : null;
            HandlerState resource = commandTopicMigrated.getResourceType() == ResourceType.Producer ? producers.get(resourceId) : consumers.get(resourceId);
            log.info("{} is migrated to {}/{}", commandTopicMigrated.getResourceType().name(), serviceUrl, serviceUrlTls);
            if (resource != null)
            {
                try
                {
                    resource.setRedirectedClusterURI(serviceUrl, serviceUrlTls);
                }
                catch (URISyntaxException)
                {
                    log.info("[{}] Invalid redirect url {}/{} for {}", remoteAddress, serviceUrl, serviceUrlTls, resourceId);
                }
            }
        }

        // caller of this method needs to be protected under pendingLookupRequestSemaphore
        private void AddPendingLookupRequests(long requestId, TimedTaskCompletionSource<LookupDataResult> future)
        {
            _pendingRequests.Put(requestId, future);
            _requestTimeoutQueue.Enqueue(new RequestTime(requestId, RequestType.Lookup));
        }

        private ValueTask<LookupDataResult> GetAndRemovePendingLookupRequest(long requestId)
        {
            var result = (ValueTask<LookupDataResult>)_pendingRequests.Remove(requestId);
            if (result != null)
            {
                Pair<long, Pair<ByteBuf, TimedCompletableFuture<LookupDataResult>>> firstOneWaiting = waitingLookupRequests.poll();
                if (firstOneWaiting != null)
                {
                    maxLookupRequestSemaphore.release();
                    // schedule a new lookup in.
                    eventLoopGroup.execute(() =>
                    {
                        long newId = firstOneWaiting.getLeft();
                        TimedCompletableFuture<LookupDataResult> newFuture = firstOneWaiting.getRight().getRight();
                        addPendingLookupRequests(newId, newFuture);
                        ctx.writeAndFlush(firstOneWaiting.getRight().getLeft()).addListener(writeFuture =>
                        {
                            if (!writeFuture.isSuccess())
                            {
                                log.warn("{} Failed to send request {} to broker: {}", ctx.channel(), newId, writeFuture.cause().getMessage());
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
            else
            {
                duplicatedResponseCounter.incrementAndGet();
            }
            return result;
        }

        protected internal override void handleSendError(CommandSendError sendError)
        {
            log.warn("{} Received send error from server: {} : {}", ctx.channel(), sendError.getError(), sendError.getMessage());

            long producerId = sendError.getProducerId();
            long sequenceId = sendError.getSequenceId();

            //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
            //ORIGINAL LINE: ProducerImpl<?> producer = producers.get(producerId);
            ProducerImpl<object> producer = producers.get(producerId);
            if (producer == null)
            {
                log.warn("{} Producer with id {} not found while handling send error", ctx.channel(), producerId);
                return;
            }

            switch (sendError.getError())
            {
                case ChecksumError:
                    producer.recoverChecksumError(this, sequenceId);
                    break;
                case TopicTerminatedError:
                    producer.terminated(this);
                    break;
                case NotAllowedError:
                    producer.recoverNotAllowedError(sequenceId, sendError.getMessage());
                    break;
                default:
                    // don't close this ctx, otherwise it will close all consumers and producers which use this ctx
                    producer.connectionClosed(this, null, null);
                    break;
            }
        }

        protected internal override void handleError(CommandError error)
        {
            checkArgument(state == State.SentConnectFrame || state == State.Ready);

            log.warn("{} Received error from server: {}", ctx.channel(), error.getMessage());
            long requestId = error.getRequestId();
            if (error.getError() == ServerError.ProducerBlockedQuotaExceededError)
            {
                log.warn("{} Producer creation has been blocked because backlog quota exceeded for producer topic", ctx.channel());
            }
            if (error.getError() == ServerError.AuthenticationError)
            {
                connectionFuture.completeExceptionally(new PulsarClientException.AuthenticationException(error.getMessage()));
                log.error("{} Failed to authenticate the client", ctx.channel());
            }
            if (error.getError() == ServerError.NotAllowedError)
            {
                log.error("Get not allowed error, {}", error.getMessage());
                connectionFuture.completeExceptionally(new PulsarClientException.NotAllowedException(error.getMessage()));
            }
            //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
            //ORIGINAL LINE: CompletableFuture<?> requestFuture = pendingRequests.remove(requestId);
            CompletableFuture<object> requestFuture = pendingRequests.remove(requestId);
            if (requestFuture != null)
            {
                requestFuture.completeExceptionally(getPulsarClientException(error.getError(), buildError(error.getRequestId(), error.getMessage())));
            }
            else
            {
                duplicatedResponseCounter.incrementAndGet();
                log.warn("{} Received unknown request id from server: {}", ctx.channel(), error.getRequestId());
            }
        }

        protected internal override void handleCloseProducer(CommandCloseProducer closeProducer)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final long producerId = closeProducer.getProducerId();
            long producerId = closeProducer.getProducerId();
            log.info("[{}] Broker notification of closed producer: {}, assignedBrokerUrl: {}, assignedBrokerUrlTls: {}", remoteAddress, producerId, closeProducer.hasAssignedBrokerServiceUrl() ? closeProducer.getAssignedBrokerServiceUrl() : null, closeProducer.hasAssignedBrokerServiceUrlTls() ? closeProducer.getAssignedBrokerServiceUrlTls() : null);
            //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
            //ORIGINAL LINE: ProducerImpl<?> producer = producers.remove(producerId);
            ProducerImpl<object> producer = producers.remove(producerId);
            if (producer != null)
            {
                string brokerServiceUrl = getBrokerServiceUrl(closeProducer, producer);
                Optional<URI> hostUri = parseUri(brokerServiceUrl, closeProducer.hasRequestId() ? closeProducer.getRequestId() : null);
                long? initialConnectionDelayMs = hostUri.map(__ => 0L);
                producer.connectionClosed(this, initialConnectionDelayMs, hostUri);
            }
            else
            {
                log.warn("[{}] Producer with id {} not found while closing producer", remoteAddress, producerId);
            }
        }

        private static string getBrokerServiceUrl<T1>(CommandCloseProducer closeProducer, ProducerImpl<T1> producer)
        {
            if (producer.getClient().getConfiguration().isUseTls())
            {
                if (closeProducer.hasAssignedBrokerServiceUrlTls())
                {
                    return closeProducer.getAssignedBrokerServiceUrlTls();
                }
            }
            else if (closeProducer.hasAssignedBrokerServiceUrl())
            {
                return closeProducer.getAssignedBrokerServiceUrl();
            }
            return null;
        }

        protected internal override void handleCloseConsumer(CommandCloseConsumer closeConsumer)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final long consumerId = closeConsumer.getConsumerId();
            long consumerId = closeConsumer.getConsumerId();
            log.info("[{}] Broker notification of closed consumer: {}, assignedBrokerUrl: {}, assignedBrokerUrlTls: {}", remoteAddress, consumerId, closeConsumer.hasAssignedBrokerServiceUrl() ? closeConsumer.getAssignedBrokerServiceUrl() : null, closeConsumer.hasAssignedBrokerServiceUrlTls() ? closeConsumer.getAssignedBrokerServiceUrlTls() : null);
            //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
            //ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.remove(consumerId);
            ConsumerImpl<object> consumer = consumers.remove(consumerId);
            if (consumer != null)
            {
                string brokerServiceUrl = getBrokerServiceUrl(closeConsumer, consumer);
                Optional<URI> hostUri = parseUri(brokerServiceUrl, closeConsumer.hasRequestId() ? closeConsumer.getRequestId() : null);
                long? initialConnectionDelayMs = hostUri.map(__ => 0L);
                consumer.connectionClosed(this, initialConnectionDelayMs, hostUri);
            }
            else
            {
                log.warn("[{}] Consumer with id {} not found while closing consumer", remoteAddress, consumerId);
            }
        }

        private static string getBrokerServiceUrl<T1>(CommandCloseConsumer closeConsumer, ConsumerImpl<T1> consumer)
        {
            if (consumer.getClient().getConfiguration().isUseTls())
            {
                if (closeConsumer.hasAssignedBrokerServiceUrlTls())
                {
                    return closeConsumer.getAssignedBrokerServiceUrlTls();
                }
            }
            else if (closeConsumer.hasAssignedBrokerServiceUrl())
            {
                return closeConsumer.getAssignedBrokerServiceUrl();
            }
            return null;
        }

        private Optional<URI> parseUri(string url, long? requestId)
        {
            try
            {
                if (!string.ReferenceEquals(url, null))
                {
                    return (new URI(url));
                }
            }
            catch (URISyntaxException e)
            {
                log.warn("[{}] Invalid redirect URL {}, requestId {}: ", remoteAddress, url, requestId, e);
            }
            return null;
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
            TimedCompletableFuture<LookupDataResult> future = new TimedCompletableFuture<LookupDataResult>();

            if (pendingLookupRequestSemaphore.tryAcquire())
            {
                future.whenComplete((lookupDataResult, throwable) =>
                {
                    if (throwable is ConnectException || throwable is PulsarClientException.LookupException)
                    {
                        pendingLookupRequestSemaphore.release();
                    }
                });
                addPendingLookupRequests(requestId, future);
                ctx.writeAndFlush(request).addListener(writeFuture =>
                {
                    if (!writeFuture.isSuccess())
                    {
                        log.warn("{} Failed to send request {} to broker: {}", ctx.channel(), requestId, writeFuture.cause().getMessage());
                        getAndRemovePendingLookupRequest(requestId);
                        future.completeExceptionally(writeFuture.cause());
                    }
                });
            }
            else
            {
                if (log.isDebugEnabled())
                {
                    log.debug("{} Failed to add lookup-request into pending queue", requestId);
                }

                if (maxLookupRequestSemaphore.tryAcquire())
                {
                    waitingLookupRequests.add(Pair.of(requestId, Pair.of(request, future)));
                }
                else
                {
                    request.release();
                    if (log.isDebugEnabled())
                    {
                        log.debug("{} Failed to add lookup-request into waiting queue", requestId);
                    }
                    future.completeExceptionally(new PulsarClientException.TooManyRequestsException(string.Format("Requests number out of config: There are {{{0}}} lookup requests outstanding and {{{1}}} requests" + " pending.", pendingLookupRequestSemaphore.getQueueLength(), waitingLookupRequests.size())));
                }
            }
            return future;
        }

        public virtual CompletableFuture<GetTopicsResult> newGetTopicsOfNamespace(ByteBuf request, long requestId)
        {
            return sendRequestAndHandleTimeout(request, requestId, RequestType.GetTopics, true);
        }

        public virtual CompletableFuture<Void> newAckForReceipt(ByteBuf request, long requestId)
        {
            return sendRequestAndHandleTimeout(request, requestId, RequestType.AckResponse, true);
        }

        public virtual void newAckForReceiptWithFuture(ByteBuf request, long requestId, TimedCompletableFuture<Void> future)
        {
            sendRequestAndHandleTimeout(request, requestId, RequestType.AckResponse, false, future);
        }

        protected internal override void handleGetTopicsOfNamespaceSuccess(CommandGetTopicsOfNamespaceResponse success)
        {
            checkArgument(state == State.Ready);

            long requestId = success.getRequestId();
            IList<string> topics = success.getTopicsList();


            if (log.isDebugEnabled())
            {
                log.debug("{} Received get topics of namespace success response from server: {} - topics.size: {}", ctx.channel(), success.getRequestId(), topics.Count);
            }

            CompletableFuture<GetTopicsResult> requestFuture = (CompletableFuture<GetTopicsResult>)pendingRequests.remove(requestId);
            if (requestFuture != null)
            {
                requestFuture.complete(new GetTopicsResult(topics, success.hasTopicsHash() ? success.getTopicsHash() : null, success.isFiltered(), success.isChanged()));
            }
            else
            {
                duplicatedResponseCounter.incrementAndGet();
                log.warn("{} Received unknown request id from server: {}", ctx.channel(), success.getRequestId());
            }
        }

        protected internal override void handleGetSchemaResponse(CommandGetSchemaResponse commandGetSchemaResponse)
        {
            checkArgument(state == State.Ready);

            long requestId = commandGetSchemaResponse.getRequestId();

            CompletableFuture<CommandGetSchemaResponse> future = (CompletableFuture<CommandGetSchemaResponse>)pendingRequests.remove(requestId);
            if (future == null)
            {
                duplicatedResponseCounter.incrementAndGet();
                log.warn("{} Received unknown request id from server: {}", ctx.channel(), requestId);
                return;
            }
            future.complete((new CommandGetSchemaResponse()).copyFrom(commandGetSchemaResponse));
        }

        protected internal override void handleGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse commandGetOrCreateSchemaResponse)
        {
            checkArgument(state == State.Ready);
            long requestId = commandGetOrCreateSchemaResponse.getRequestId();
            CompletableFuture<CommandGetOrCreateSchemaResponse> future = (CompletableFuture<CommandGetOrCreateSchemaResponse>)pendingRequests.remove(requestId);
            if (future == null)
            {
                duplicatedResponseCounter.incrementAndGet();
                log.warn("{} Received unknown request id from server: {}", ctx.channel(), requestId);
                return;
            }
            future.complete((new CommandGetOrCreateSchemaResponse()).copyFrom(commandGetOrCreateSchemaResponse));
        }

        internal virtual Promise<Void> newPromise()
        {
            return ctx.newPromise();
        }

        public virtual ChannelHandlerContext ctx()
        {
            return ctx;
        }

        //JAVA TO C# CONVERTER TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @VisibleForTesting protected Channel channel()
        protected internal virtual Channel channel()
        {
            return ctx.channel();
        }

        internal virtual CompletableFuture<Void> connectionFuture()
        {
            return connectionFuture;
        }

        internal virtual CompletableFuture<ProducerResponse> sendRequestWithId(ByteBuf cmd, long requestId)
        {
            return sendRequestAndHandleTimeout(cmd, requestId, RequestType.Command, true);
        }

        private void sendRequestAndHandleTimeout<T>(ByteBuf requestMessage, long requestId, RequestType requestType, bool flush, TimedCompletableFuture<T> future)
        {
            pendingRequests.put(requestId, future);
            if (flush)
            {
                ctx.writeAndFlush(requestMessage).addListener(writeFuture =>
                {
                    if (!writeFuture.isSuccess())
                    {
                        if (pendingRequests.remove(requestId, future) && !future.isDone())
                        {
                            log.warn("{} Failed to send {} to broker: {}", ctx.channel(), requestType.getDescription(), writeFuture.cause().getMessage());
                            future.completeExceptionally(writeFuture.cause());
                        }
                    }
                });
            }
            else
            {
                ctx.write(requestMessage, ctx().voidPromise());
            }
            requestTimeoutQueue.add(new RequestTime(requestId, requestType));
        }

        private CompletableFuture<T> sendRequestAndHandleTimeout<T>(ByteBuf requestMessage, long requestId, RequestType requestType, bool flush)
        {
            TimedCompletableFuture<T> future = new TimedCompletableFuture<T>();
            sendRequestAndHandleTimeout(requestMessage, requestId, requestType, flush, future);
            return future;
        }

        public virtual CompletableFuture<CommandGetLastMessageIdResponse> sendGetLastMessageId(ByteBuf request, long requestId)
        {
            return sendRequestAndHandleTimeout(request, requestId, RequestType.GetLastMessageId, true);
        }

        public virtual CompletableFuture<Optional<SchemaInfo>> sendGetSchema(ByteBuf request, long requestId)
        {
            return sendGetRawSchema(request, requestId).thenCompose(commandGetSchemaResponse =>
            {
                if (commandGetSchemaResponse.hasErrorCode())
                {
                    // Request has failed
                    ServerError rc = commandGetSchemaResponse.getErrorCode();
                    if (rc == ServerError.TopicNotFound)
                    {
                        return CompletableFuture.completedFuture(null);
                    }
                    else
                    {
                        return FutureUtil.failedFuture(getPulsarClientException(rc, buildError(requestId, commandGetSchemaResponse.getErrorMessage())));
                    }
                }
                else
                {
                    return CompletableFuture.completedFuture(SchemaInfoUtil.newSchemaInfo(commandGetSchemaResponse.getSchema()));
                }
            });
        }

        public virtual CompletableFuture<CommandGetSchemaResponse> sendGetRawSchema(ByteBuf request, long requestId)
        {
            return sendRequestAndHandleTimeout(request, requestId, RequestType.GetSchema, true);
        }

        public virtual CompletableFuture<sbyte[]> sendGetOrCreateSchema(ByteBuf request, long requestId)
        {
            CompletableFuture<CommandGetOrCreateSchemaResponse> future = sendRequestAndHandleTimeout(request, requestId, RequestType.GetOrCreateSchema, true);
            return future.thenCompose(response =>
            {
                if (response.hasErrorCode())
                {
                    // Request has failed
                    ServerError rc = response.getErrorCode();
                    if (rc == ServerError.TopicNotFound)
                    {
                        return CompletableFuture.completedFuture(SchemaVersion.Empty.bytes());
                    }
                    else
                    {
                        return FutureUtil.failedFuture(getPulsarClientException(rc, buildError(requestId, response.getErrorMessage())));
                    }
                }
                else
                {
                    return CompletableFuture.completedFuture(response.getSchemaVersion());
                }
            });
        }

        protected internal override void handleNewTxnResponse(CommandNewTxnResponse command)
        {
            TransactionMetaStoreHandler handler = checkAndGetTransactionMetaStoreHandler(command.getTxnidMostBits());
            if (handler != null)
            {
                handler.handleNewTxnResponse(command);
            }
        }

        protected internal override void handleAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse command)
        {
            TransactionMetaStoreHandler handler = checkAndGetTransactionMetaStoreHandler(command.getTxnidMostBits());
            if (handler != null)
            {
                handler.handleAddPublishPartitionToTxnResponse(command);
            }
        }

        protected internal override void handleAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse command)
        {
            TransactionMetaStoreHandler handler = checkAndGetTransactionMetaStoreHandler(command.getTxnidMostBits());
            if (handler != null)
            {
                handler.handleAddSubscriptionToTxnResponse(command);
            }
        }

        protected internal override void handleEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse command)
        {
            TransactionBufferHandler handler = checkAndGetTransactionBufferHandler();
            if (handler != null)
            {
                handler.handleEndTxnOnTopicResponse(command.getRequestId(), command);
            }
        }

        protected internal override void handleEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse command)
        {
            TransactionBufferHandler handler = checkAndGetTransactionBufferHandler();
            if (handler != null)
            {
                handler.handleEndTxnOnSubscriptionResponse(command.getRequestId(), command);
            }
        }

        protected internal override void handleEndTxnResponse(CommandEndTxnResponse command)
        {
            TransactionMetaStoreHandler handler = checkAndGetTransactionMetaStoreHandler(command.getTxnidMostBits());
            if (handler != null)
            {
                handler.handleEndTxnResponse(command);
            }
        }

        protected internal override void handleTcClientConnectResponse(CommandTcClientConnectResponse response)
        {
            checkArgument(state == State.Ready);

            if (log.isDebugEnabled())
            {
                log.debug("{} Received tc client connect response " + "from server: {}", ctx.channel(), response.getRequestId());
            }
            long requestId = response.getRequestId();
            //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
            //ORIGINAL LINE: CompletableFuture<?> requestFuture = pendingRequests.remove(requestId);
            CompletableFuture<object> requestFuture = pendingRequests.remove(requestId);

            if (requestFuture != null && !requestFuture.isDone())
            {
                if (!response.hasError())
                {
                    requestFuture.complete(null);
                }
                else
                {
                    ServerError error = response.getError();
                    log.error("Got tc client connect response for request: {}, error: {}, errorMessage: {}", response.getRequestId(), response.getError(), response.getMessage());
                    requestFuture.completeExceptionally(getExceptionByServerError(error, response.getMessage()));
                }
            }
            else
            {
                duplicatedResponseCounter.incrementAndGet();
                log.warn("Tc client connect command has been completed and get response for request: {}", response.getRequestId());
            }
        }

        private TransactionMetaStoreHandler checkAndGetTransactionMetaStoreHandler(long tcId)
        {
            TransactionMetaStoreHandler handler = transactionMetaStoreHandlers.get(tcId);
            if (handler == null)
            {
                channel().close();
                log.warn("Close the channel since can't get the transaction meta store handler, will reconnect later.");
            }
            return handler;
        }

        private TransactionBufferHandler checkAndGetTransactionBufferHandler()
        {
            if (transactionBufferHandler == null)
            {
                channel().close();
                log.warn("Close the channel since can't get the transaction buffer handler.");
            }
            return transactionBufferHandler;
        }

        public virtual CompletableFuture<CommandWatchTopicListSuccess> newWatchTopicList(BaseCommand commandWatchTopicList, long requestId)
        {
            if (!supportsTopicWatchers)
            {
                return FutureUtil.failedFuture(new PulsarClientException.NotAllowedException("Broker does not allow broker side pattern evaluation."));
            }
            return sendRequestAndHandleTimeout(Commands.serializeWithSize(commandWatchTopicList), requestId, RequestType.Command, true);
        }

        public virtual CompletableFuture<CommandSuccess> newWatchTopicListClose(BaseCommand commandWatchTopicListClose, long requestId)
        {
            return sendRequestAndHandleTimeout(Commands.serializeWithSize(commandWatchTopicListClose), requestId, RequestType.Command, true);
        }

        protected internal override void handleCommandWatchTopicListSuccess(CommandWatchTopicListSuccess commandWatchTopicListSuccess)
        {
            checkArgument(state == State.Ready);

            if (log.isDebugEnabled())
            {
                log.debug("{} Received watchTopicListSuccess response from server: {}", ctx.channel(), commandWatchTopicListSuccess.getRequestId());
            }
            long requestId = commandWatchTopicListSuccess.getRequestId();
            CompletableFuture<CommandWatchTopicListSuccess> requestFuture = (CompletableFuture<CommandWatchTopicListSuccess>)pendingRequests.remove(requestId);
            if (requestFuture != null)
            {
                requestFuture.complete(commandWatchTopicListSuccess);
            }
            else
            {
                duplicatedResponseCounter.incrementAndGet();
                log.warn("{} Received unknown request id from server: {}", ctx.channel(), commandWatchTopicListSuccess.getRequestId());
            }
        }

        protected internal override void HandleCommandWatchTopicUpdate(CommandWatchTopicUpdate commandWatchTopicUpdate)
        {
            Condition.CheckArgument(_state == State.Ready);
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"{_ctx.Channel} Received watchTopicUpdate command from server: {commandWatchTopicUpdate.WatcherId}");
            }

            var watcherId = (long)commandWatchTopicUpdate.WatcherId;
            var watcher = _topicListWatchers.Get(watcherId);
            if (watcher != null)
            {
                watcher.Tell(new CommandWatchTopicUpdateResponse(commandWatchTopicUpdate)); 
            }
            else
            {
                _log.Warning("{} Received topic list update for unknown watcher from server: {}", _ctx.Channel, watcherId);
            }
            
        }

        /// <summary>
        /// check serverError and take appropriate action.
        /// <ul>
        /// <li>InternalServerError: close connection immediately</li>
        /// <li>TooManyRequest: received error count is more than maxNumberOfRejectedRequestPerConnection in
        /// #rejectedRequestResetTimeSec</li>
        /// </ul>
        /// </summary>
        /// <param name="error"> </param>
        /// <param name="errMsg"> </param>
        private void CheckServerError(ServerError error, string errMsg)
        {
            if (ServerError.ServiceNotReady.Equals(error))
            {
                log.error("{} Close connection because received internal-server error {}", ctx.channel(), errMsg);
                _ctx.CloseAsync().GetAwaiter();
            }
            else if (ServerError.TooManyRequests.Equals(error))
            {
                IncrementRejectsAndMaybeClose();
            }
        }

        private void IncrementRejectsAndMaybeClose()
        {
            long rejectedRequests = NUMBER_OF_REJECTED_REQUESTS_UPDATER.getAndIncrement(this);
            if (rejectedRequests == 0)
            {
                // schedule timer
                eventLoopGroup.schedule(() => NUMBER_OF_REJECTED_REQUESTS_UPDATER.set(ClientCnx.this, 0), rejectedRequestResetTimeSec, TimeUnit.SECONDS);
            }
            else if (rejectedRequests >= maxNumberOfRejectedRequestPerConnection)
            {
                log.error("{} Close connection because received {} rejected request in {} seconds ", ctx.channel(), NUMBER_OF_REJECTED_REQUESTS_UPDATER.get(ClientCnx.this), rejectedRequestResetTimeSec);
                ctx.close();
            }
        }

        internal virtual void RegisterConsumer<T1>(in long consumerId, in IActorRef consumer)
        {
            _consumers.Put(consumerId, consumer);
        }

        internal virtual void RegisterProducer<T1>(long producerId, IActorRef producer)
        {
            _producers.Put(producerId, producer);
        }

        internal virtual void RegisterTransactionMetaStoreHandler(in long transactionMetaStoreId, in IActorRef handler)
        {
            _transactionMetaStoreHandlers.Put(transactionMetaStoreId, handler);
        }

        internal virtual void RegisterTopicListWatcher(long watcherId, IActorRef watcher)
        {
            _topicListWatchers.Put(watcherId, watcher);

        }

        internal virtual void RemoveProducer(in long producerId)
        {
            _producers.Remove(producerId);
        }

        internal virtual void RemoveConsumer(in long consumerId)
        {
            _consumers.Remove(consumerId);
        }

        internal virtual void RemoveTopicListWatcher(in long watcherId)
        {
            _topicListWatchers.Remove(watcherId);
        }

        internal virtual  SocketAddress TargetBroker
        {
            set
            {
                _proxyToTargetBrokerAddress = string.Format("{0}:{1:D}", value.ToString(), value.AddressFamil.getPort());
            }
        }

        internal virtual string RemoteHostName
        {
            set
            {
                _remoteHostName = value;
            }
        }

        private string BuildError(long requestId, string errorMsg)
        {
            return (new StringBuilder()).Append("{\"errorMsg\":\"").Append(errorMsg).Append("\",\"reqId\":").Append(requestId).Append(", \"remote\":\"").Append(remoteAddress).Append("\", \"local\":\"").Append(_localAddress).Append("\"}").ToString();
        }

        public static PulsarClientException GetPulsarClientException(ServerError error, string errorMsg)
        {
            switch (error)
            {
                case ServerError.AuthenticationError:
                    return new PulsarClientException.AuthenticationException(errorMsg);
                case ServerError.AuthorizationError:
                    return new PulsarClientException.AuthorizationException(errorMsg);
                case ServerError.ProducerBusy:
                    return new PulsarClientException.ProducerBusyException(errorMsg);
                case ServerError.ConsumerBusy:
                    return new PulsarClientException.ConsumerBusyException(errorMsg);
                case ServerError.MetadataError:
                    return new PulsarClientException.BrokerMetadataException(errorMsg);
                case ServerError.PersistenceError:
                    return new PulsarClientException.BrokerPersistenceException(errorMsg);
                case ServerError.ServiceNotReady:
                    return new PulsarClientException.ServiceNotReadyException(errorMsg);
                case ServerError.TooManyRequests:
                    return new PulsarClientException.TooManyRequestsException(errorMsg);
                case ServerError.ProducerBlockedQuotaExceededError:
                    return new PulsarClientException.ProducerBlockedQuotaExceededError(errorMsg);
                case ServerError.ProducerBlockedQuotaExceededException:
                    return new PulsarClientException.ProducerBlockedQuotaExceededException(errorMsg);
                case ServerError.TopicTerminatedError:
                    return new PulsarClientException.TopicTerminatedException(errorMsg);
                case ServerError.IncompatibleSchema:
                    return new PulsarClientException.IncompatibleSchemaException(errorMsg);
                case ServerError.TopicNotFound:
                    return new PulsarClientException.TopicDoesNotExistException(errorMsg);
                case ServerError.SubscriptionNotFound:
                    return new PulsarClientException.SubscriptionNotFoundException(errorMsg);
                case ServerError.ConsumerAssignError:
                    return new PulsarClientException.ConsumerAssignException(errorMsg);
                case ServerError.NotAllowedError:
                    return new PulsarClientException.NotAllowedException(errorMsg);
                case ServerError.TransactionConflict:
                    return new PulsarClientException.TransactionConflictException(errorMsg);
                case ServerError.ProducerFenced:
                    return new PulsarClientException.ProducerFencedException(errorMsg);
                case ServerError.UnknownError:
                default:
                    return new PulsarClientException(errorMsg);
            }
        }

        public static ServerError revertClientExToErrorCode(PulsarClientException ex)
        {
            if (ex is PulsarClientException.AuthenticationException)
            {
                return ServerError.AuthenticationError;
            }
            else if (ex is PulsarClientException.AuthorizationException)
            {
                return ServerError.AuthorizationError;
            }
            else if (ex is PulsarClientException.ProducerBusyException)
            {
                return ServerError.ProducerBusy;
            }
            else if (ex is PulsarClientException.ConsumerBusyException)
            {
                return ServerError.ConsumerBusy;
            }
            else if (ex is PulsarClientException.BrokerMetadataException)
            {
                return ServerError.MetadataError;
            }
            else if (ex is PulsarClientException.BrokerPersistenceException)
            {
                return ServerError.PersistenceError;
            }
            else if (ex is PulsarClientException.TooManyRequestsException)
            {
                return ServerError.TooManyRequests;
            }
            else if (ex is PulsarClientException.LookupException)
            {
                return ServerError.ServiceNotReady;
            }
            else if (ex is PulsarClientException.ProducerBlockedQuotaExceededError)
            {
                return ServerError.ProducerBlockedQuotaExceededError;
            }
            else if (ex is PulsarClientException.ProducerBlockedQuotaExceededException)
            {
                return ServerError.ProducerBlockedQuotaExceededException;
            }
            else if (ex is PulsarClientException.TopicTerminatedException)
            {
                return ServerError.TopicTerminatedError;
            }
            else if (ex is PulsarClientException.IncompatibleSchemaException)
            {
                return ServerError.IncompatibleSchema;
            }
            else if (ex is PulsarClientException.TopicDoesNotExistException)
            {
                return ServerError.TopicNotFound;
            }
            else if (ex is PulsarClientException.SubscriptionNotFoundException)
            {
                return ServerError.SubscriptionNotFound;
            }
            else if (ex is PulsarClientException.ConsumerAssignException)
            {
                return ServerError.ConsumerAssignError;
            }
            else if (ex is PulsarClientException.NotAllowedException)
            {
                return ServerError.NotAllowedError;
            }
            else if (ex is PulsarClientException.TransactionConflictException)
            {
                return ServerError.TransactionConflict;
            }
            else if (ex is PulsarClientException.ProducerFencedException)
            {
                return ServerError.ProducerFenced;
            }
            return ServerError.UnknownError;
        }

        public virtual void Close()
        {
            if (_ctx != null)
            {
                _ctx.CloseAsync();
            }
        }

        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            if (evt is TlsHandshakeCompletionEvent)
            {
                TlsHandshakeCompletionEvent sslHandshakeCompletionEvent = (TlsHandshakeCompletionEvent)evt;
                if (sslHandshakeCompletionEvent.Exception != null)
                {
                    _log.Warning($"{ctx.Channel} Got ssl handshake exception {sslHandshakeCompletionEvent}");
                }
            }
            ctx.FireUserEventTriggered(evt);
        }

        protected internal virtual void CloseWithException(Exception e)
        {
            if (_ctx != null)
            {
                _connectionFuture.SetException(e);
                _ctx.CloseAsync().GetAwaiter();
            }
        }

        private void CheckRequestTimeout()
        {
            while (!_requestTimeoutQueue.IsEmpty)
            {
                _requestTimeoutQueue.TryPeek(out var request);
                if (request == null || !request.IsTimedOut(operationTimeoutMs))
                {
                    // if there is no request that is timed out then exit the loop
                    break;
                }
                if (!_requestTimeoutQueue.Remove(request))
                {
                    // the request has been removed by another thread
                    continue;
                }
                
                var requestFuture = _pendingRequests.Get(request.RequestId);
                if (requestFuture != null && !requestFuture.HasGotResponse())
                {
                    _pendingRequests.Remove(request.RequestId, requestFuture);
                    if (!requestFuture.isDone())
                    {
                        string timeoutMessage = string.Format("{0} timeout {{'durationMs': '{1:D}', 'reqId':'{2:D}', 'remote':'{3}', 'local':'{4}'}}", request.requestType.getDescription(), operationTimeoutMs, request.requestId, remoteAddress, localAddress);
                        if (requestFuture.completeExceptionally(new TimeoutException(timeoutMessage)))
                        {
                            if (request.RequestType == RequestType.Lookup)
                            {
                                IncrementRejectsAndMaybeClose();
                            }
                            _log.Warning($"{_ctx.Channel} {timeoutMessage}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check client connection is now free. This method will not change the state to idle. </summary>
        /// <returns> true if the connection is eligible. </returns>
        public virtual bool IdleCheck()
        {
            if (_pendingRequests != null && !_pendingRequests.Empty)
            {
                return false;
            }
            if (_waitingLookupRequests != null && _waitingLookupRequests.Count == 0)
            {
                return false;
            }
            if (!_consumers.Empty)
            {
                return false;
            }
            if (!_producers.Empty)
            {
                return false;
            }
            if (!_transactionMetaStoreHandlers.Empty)
            {
                return false;
            }
            if (!_topicListWatchers.Empty)
            {
                return false;
            }
            return true;
        }


        // OLDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
        private void NewAckForReceipt(AbstractByteBuffer request, long requestId)
        {
            SendRequestAndHandleTimeout(request, requestId, RequestType.AckResponse);
        }
        

        private void HandleCommandWatchTopicListSuccess(CommandWatchTopicListSuccess commandWatchTopicListSuccess)
        {
            Condition.CheckArgument(_state == State.Ready);
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[ctx] Received watchTopicListSuccess response from server: {commandWatchTopicListSuccess.RequestId}");
            }
            var requestId = (long)commandWatchTopicListSuccess.RequestId;
            if (_pendingRequests.TryGetValue(requestId, out var req))
            {
                _pendingRequests.Remove(requestId);
                req.Requester.Tell(new CommandWatchTopicListSuccessResponse(commandWatchTopicListSuccess), _self);
            }
            else
            {
                _log.Warning($"Received unknown request id from server: {commandWatchTopicListSuccess.RequestId}");
            }
        }

        private void RegisterTopicListWatcher(long watcherId, IActorRef watcher)
        {
            _topicListWatchers.Add(watcherId, watcher);

        }
        private void RemoveTopicListWatcher(long watcherId)
        {
            _topicListWatchers.Remove(watcherId);
        }
        
        private void HandleNewTcClientConnectResponse(CommandTcClientConnectResponse response)
        {
            var requestId = (long)response.RequestId;
            if (_pendingRequests.TryGetValue(requestId, out var req))
            {
                _pendingRequests.Remove(requestId);
                if (response.Error != ServerError.UnknownError)
                {
                    CheckServerError(response.Error, response.Message);
                    var ex = GetPulsarClientException(response.Error, response.Message);
                    req.Requester.Tell(new AskResponse(ex));
                }
                else
                    req.Requester.Tell(new AskResponse());
            }
        }

        private long? GetTopicEpoch(CommandProducerSuccess success)
        {
            if (success.HasTopicEpoch)
                return (long)success.TopicEpoch;

            return null;
        }
        
        private void HandlePing(CommandPing ping)
        {
            // Immediately reply success to ping requests
            if (_log.IsEnabled(LogLevel.DebugLevel))
            {
                _log.Debug($"[{_self.Path}] [{_remoteHostName}] Replying back to ping message");
            }
            _sendMessage.Tell(new SendMessage((AbstractByteBuffer)_pong));
        }
        
        private void HandleReachedEndOfTopic(CommandReachedEndOfTopic commandReachedEndOfTopic)
        {
            var consumerId = (long)commandReachedEndOfTopic.ConsumerId;

            _log.Info($"[{_remoteHostName}] Broker notification reached the end of topic: {consumerId}");
            if (_consumers.TryGetValue(consumerId, out var consumer))
            {
                consumer.Tell(SetTerminated.Instance);
            }
        }

        // caller of this method needs to be protected under pendingLookupRequestSemaphore
        private void AddPendingLookupRequests(long requestId, AbstractByteBuffer message)
        {
            _pendingRequests.Add(requestId, (message, Sender));
            _requestTimeoutQueue.Enqueue(new RequestTime(requestId, RequestType.Lookup));
        }

        private bool RemovePendingLookupRequest(long requestId, out IActorRef actor)
        {
            actor = ActorRefs.Nobody;
            if (_pendingRequests.TryGetValue(requestId, out var request))
            {
                actor = request.Requester;
                return _pendingRequests.Remove(requestId);
            }
            return false;
        }
        private void HandleTopicMigrated(CommandTopicMigrated commandTopicMigrated)
        {
            long resourceId = (long)commandTopicMigrated.ResourceId;
            string serviceUrl = commandTopicMigrated.HasBrokerServiceUrl ? commandTopicMigrated.BrokerServiceUrl : null;
            string serviceUrlTls = commandTopicMigrated.HasBrokerServiceUrlTls ? commandTopicMigrated.BrokerServiceUrlTls : null;
            var resource = commandTopicMigrated.ResourceType == ResourceType.Producer ? _producers[resourceId] : _consumers[resourceId];
            _log.Info($"{commandTopicMigrated.ResourceType} is migrated to {serviceUrl}/{serviceUrlTls}");
            if (resource != null)
            {
                try
                {
                    resource.Tell(new RedirectedClusterURI(serviceUrl, serviceUrlTls));
                }
                catch (Exception)
                {
                    _log.Info($"[{_remoteAddress}] Invalid redirect url {serviceUrl}/{serviceUrlTls} for {resourceId}");
                }
            }
        }

        private void HandleSendError(CommandSendError sendError)
        {
            _log.Warning($"Received send error from server: {sendError.Error} : {sendError.Message}");

            var producerId = (long)sendError.ProducerId;
            var sequenceId = (long)sendError.SequenceId;

            var producer = _producers[producerId];
            if (producer == null)
            {
                _log.Warning($"Producer with id {producerId} not found while handling send error");
                return;
            }

            switch (sendError.Error)
            {
                case ServerError.ChecksumError:
                    producer.Tell(new RecoverChecksumError(_self, sequenceId));
                    break;

                case ServerError.TopicTerminatedError:
                    producer.Tell(new Messages.Terminated(_self));
                    break;

                case ServerError.NotAllowedError:
                    producer.Tell(new RecoverNotAllowedError(sequenceId, sendError.Message));
                    break;

                default:
                    // don't close this ctx, otherwise it will close all consumers and producers which use this ctx
                    producer.Tell(new ConnectionClosed(Self, 0, null));
                    break;
            }
        }

        private void HandleError(CommandError error)
        {
            Condition.CheckArgument(_state == State.SentConnectFrame || _state == State.Ready);

            _log.Warning($"Received error from server: {error.Message}");
            var requestId = (long)error.RequestId;
            AskResponse response;
            if (error.Error == ServerError.ProducerBlockedQuotaExceededError)
            {
                _log.Warning($"Producer creation has been blocked because backlog quota exceeded for producer topic");
                response = new AskResponse(new PulsarClientException.AuthenticationException("Producer creation has been blocked because backlog quota exceeded for producer topic"));
            }
            else if (error.Error == ServerError.AuthenticationError)
            {
                _connectionFuture.TrySetException(new PulsarClientException.AuthenticationException(error.Message));
                _log.Error("Failed to authenticate the client");
                return;
            }
            else if (error.Error == ServerError.NotAllowedError)
            {
                _log.Error($"Get not allowed error, {error.Message}");
                _connectionFuture.TrySetException(new PulsarClientException.NotAllowedException(error.Message));
                return;
            }
            else
                response = new AskResponse(GetPulsarClientException(error.Error, error.Message));

            if (_pendingRequests.TryGetValue(requestId, out var request))
            {
                request.Requester.Tell(response);
            }
            else
            {
                _duplicatedResponseCounter.GetAndIncrement();
                Sender?.Tell(response);
                _log.Warning($"Received unknown request id from server: {error.RequestId}");
            }
        }

        private void HandleCloseProducer(CommandCloseProducer closeProducer)
        {
            var producerId = (long)closeProducer.ProducerId;

            var url = closeProducer.HasAssignedBrokerServiceUrl ? closeProducer.AssignedBrokerServiceUrl : null;
            var tls = closeProducer.HasAssignedBrokerServiceUrlTls ? closeProducer.AssignedBrokerServiceUrlTls : null;

            _log.Info($"[{_remoteAddress}] Broker notification of closed producer: {producerId}, assignedBrokerUrl: " +
                $"{url}, assignedBrokerUrlTls: {tls}");

            if (_producers.TryGetValue(producerId, out var producer))
            {
                _producers.Remove(producerId, out var p);
                string brokerServiceUrl = GetBrokerServiceUrl(closeProducer, producer);
                var hostUri = ParseUri(brokerServiceUrl, closeProducer.HasRequestId ? (long)closeProducer.RequestId : 0);
                long? initialConnectionDelayMs = hostUri.map(__ => 0L);

                p.Tell(new ConnectionClosed(_self, (long)initialConnectionDelayMs, hostUri));
            }
            else
            {
                _log.Warning($"Producer with id {producerId} not found while closing producer ");
            }
        }
        private Uri ParseUri(string url, long requestId)
        {
            try
            {
                if (!string.ReferenceEquals(url, null))
                {
                    return (new Uri(url));
                }
            }
            catch (Exception e)
            {
                _log.Warning($"[{_remoteAddress}] Invalid redirect URL {url}, requestId {requestId}:"+ e);
            }
            return null;
        }

        private string GetBrokerServiceUrl(CommandCloseProducer closeProducer, IActorRef producer)
        {
            if (_clientConfigurationData.UseTls)
            {
                if (closeProducer.HasAssignedBrokerServiceUrlTls)
                {
                    return closeProducer.AssignedBrokerServiceUrlTls;
                }
            }
            else if (closeProducer.HasAssignedBrokerServiceUrl)
            {
                return closeProducer.AssignedBrokerServiceUrl;
            }
            return null;
        }
        private string GetBrokerServiceUrl(CommandCloseConsumer closeConsumer, IActorRef consumer)
        {
            if (_clientConfigurationData.UseTls)
            {
                if (closeConsumer.HasAssignedBrokerServiceUrlTls)
                {
                    return closeConsumer.AssignedBrokerServiceUrlTls;
                }
            }
            else if (closeConsumer.HasAssignedBrokerServiceUrl)
            {
                return closeConsumer.AssignedBrokerServiceUrl;
            }
            return null;
        }
        private void HandleCloseConsumer(CommandCloseConsumer closeConsumer)
        {
            var consumerId = (long)closeConsumer.ConsumerId; 

            _log.Info($"[{_remoteAddress}] Broker notification of closed consumer: {consumerId}, assignedBrokerUrl:" +
                $" {(closeConsumer.HasAssignedBrokerServiceUrl ? closeConsumer.AssignedBrokerServiceUrl : null)}, assignedBrokerUrlTls: " +
                $"{(closeConsumer.HasAssignedBrokerServiceUrlTls ? closeConsumer.AssignedBrokerServiceUrlTls : null)}");
            
            if (_consumers.TryGetValue(consumerId, out var consumer))
            {
                var brokerServiceUrl = GetBrokerServiceUrl(closeConsumer, consumer);
                var hostUri = ParseUri(brokerServiceUrl, closeConsumer.HasRequestId ? (long)closeConsumer.RequestId : 0);
                var initialConnectionDelayMs = hostUri.Map(__-> 0L);
                consumer.Tell(new ConnectionClosed(_self, initialConnectionDelayMs, hostUri));
            }
            else
            {
                _log.Warning($"Consumer with id {consumerId} not found while closing consumer ");
            }
        }

        public IStash Stash { get; set; }
        public ITimerScheduler Timers { get; set; }
        private bool IsHandshakeCompleted()
        {
            return _state == State.Ready;
        }
        private void NewLookup(AbstractByteBuffer request, long requestId)
        {
            try
            {
                _sendMessage.Tell(new SendMessage(request));
                AddPendingLookupRequests(requestId, request);

            }
            catch (Exception ex)
            {
                Sender.Tell(PulsarClientException.Unwrap(ex));
            }
        }

        private void NewGetTopicsOfNamespace(AbstractByteBuffer request, long requestId)
        {
            SendRequestAndHandleTimeout(request, requestId, RequestType.GetTopics);
        }

        private void HandleGetTopicsOfNamespaceSuccess(CommandGetTopicsOfNamespaceResponse success)
        {
            Condition.CheckArgument(_state == State.Ready);

            var requestId = (long)success.RequestId;

            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Received get topics of namespace success response from server: {success.RequestId} - topics.size: {success.Topics.Count}");
            }

            if (_pendingRequests.TryGetValue(requestId, out var requester))
            {
                requester.Requester.Tell(new AskResponse(new GetTopicsOfNamespaceResponse(success)));
            }
            else
            {
                var msg = $"Received unknown request id from server: {success.RequestId}";

                _log.Warning(msg);
            }
        }

        private void HandleGetSchemaResponse(CommandGetSchemaResponse commandGetSchemaResponse)
        {
            Condition.CheckArgument(_state == State.Ready);
            var requestId = (long)commandGetSchemaResponse.RequestId;

            if (_pendingRequests.TryGetValue(requestId, out var requester))
            {
                requester.Requester.Tell(new AskResponse(new SharpPulsar.Messages.GetSchemaResponse(commandGetSchemaResponse)));
            }
            else
            {
                var msg = $"Received unknown request id from server: {requestId}";
                _log.Warning(msg);
                requester.Requester.Tell(new AskResponse(PulsarClientException.Unwrap(new Exception(msg))));
            }

        }

        private void HandleGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse commandGetOrCreateSchemaResponse)
        {
            Condition.CheckArgument(_state == State.Ready);
            var requestId = (long)commandGetOrCreateSchemaResponse.RequestId;
            if (_pendingRequests.TryGetValue(requestId, out var requester))
            {
                requester.Requester.Tell(new GetOrCreateSchemaResponse(commandGetOrCreateSchemaResponse));
            }
            else
                _log.Warning($"Received unknown request id from server: {requestId}");
        }

        private void SendRequestWithId(AbstractByteBuffer cmd, long requestId, bool reply)
        {
            SendRequestAndHandleTimeout(cmd, requestId, RequestType.Command);
        }

        private bool SendRequestAndHandleTimeout(AbstractByteBuffer requestMessage, long requestId, RequestType requestType)
        {
            try
            {
                _sendMessage.Tell(new SendMessage(requestMessage));
                _pendingRequests.Add(requestId, (requestMessage, Sender));

                _requestTimeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId, requestType));
                return true;
            }
            catch (Exception ex)
            {
                Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
            }
            return false;
        }
        private void SendRequest(AbstractByteBuffer requestMessage, long requestId)
        {
            try
            {
                _sendMessage.Tell(new SendMessage(requestMessage));
                if (requestId >= 0)
                    _pendingRequests.Add(requestId, (requestMessage, Sender));
            }
            catch (Exception ex)
            {
                Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
            }

        }

        private void SendGetLastMessageId(AbstractByteBuffer request, long requestId)
        {
            SendRequestAndHandleTimeout(request, requestId, RequestType.GetLastMessageId);
        }

        private void SendGetRawSchema(AbstractByteBuffer request, long requestId)
        {
            SendRequestAndHandleTimeout(request, requestId, RequestType.GetSchema);
        }

        private void SendGetOrCreateSchema(AbstractByteBuffer request, long requestId)
        {
            SendRequestAndHandleTimeout(request, requestId, RequestType.GetOrCreateSchema);
        }

        private void HandleNewTxnResponse(CommandNewTxnResponse command)
        {
            var requestId = (long)command.RequestId;
            if (_pendingRequests.TryGetValue(requestId, out var req))
            {
                _pendingRequests.Remove(requestId);
                req.Requester.Tell(new NewTxnResponse(command, GetExceptionByServerError(command.Error, command.Message)));
            }
        }

        private void HandleAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse command)
        {
            var requestId = (long)command.RequestId;
            if (_pendingRequests.TryGetValue(requestId, out var req))
            {
                _pendingRequests.Remove(requestId);
                req.Requester.Tell(new AddPublishPartitionToTxnResponse(command));
            }
            /*var handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
			if (handler != null)
			{
				handler.Tell(new AddPublishPartitionToTxnResponse(command));
			}*/
        }

        private void HandleAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse command)
        {
            var requestId = (long)command.RequestId;
            if (_pendingRequests.TryGetValue(requestId, out var req))
            {
                _pendingRequests.Remove(requestId);
                req.Requester.Tell(new AddSubscriptionToTxnResponse(command));
            }
            /*var handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
			if (handler != null)
			{
				handler.Tell(new AddSubscriptionToTxnResponse(command));
			}*/
        }


        private void HandleEndTxnResponse(CommandEndTxnResponse command)
        {
            var requestId = (long)command.RequestId;
            if (_pendingRequests.TryGetValue(requestId, out var req))
            {
                _pendingRequests.Remove(requestId);
                req.Requester.Tell(new EndTxnResponse(command));
            }
            /*var handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
			if (handler != null)
			{
				handler.Tell(new EndTxnResponse(command));
			}*/
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
        private void CheckServerError(ServerError error, string errMsg)
        {
            if (ServerError.ServiceNotReady.Equals(error))
            {
                _log.Error($"Close connection because received internal-server error {errMsg}");
                //_socketClient.Dispose();
            }
            else if (ServerError.TooManyRequests.Equals(error))
            {
                long rejectedRequests = _numberOfRejectRequests++;
                if (rejectedRequests >= _maxNumberOfRejectedRequestPerConnection)
                {
                    _log.Error($"Close connection because received {this} rejected request in {_rejectedRequestResetTimeSec} seconds ");

                    //_socketClient.Dispose();
                }
            }
        }
        private void OnCommandReceived((BaseCommand command, MessageMetadata metadata, BrokerEntryMetadata brokerEntryMetadata, AbstractByteBuffer payload, bool hasValidCheckSum, bool hasMagicNumber) args)
        {
            var cmd = args.command;
            switch (cmd.Type)
            {
                case BaseCommand.Types.Type.AuthChallenge:
                    var auth = cmd.AuthChallenge;
                    HandleAuthChallenge(auth);
                    break;
                case BaseCommand.Types.Type.Message:
                    var msg = cmd.Message;
                    HandleMessage(msg, payload);
                    break;
                case BaseCommand.Types.Type.GetLastMessageIdResponse:
                    HandleGetLastMessageIdSuccess(cmd.GetLastMessageIdResponse);
                    break;
                case BaseCommand.Types.Type.Connected:
                    HandleConnected(cmd.Connected);
                    break;
                case BaseCommand.Types.Type.GetTopicsOfNamespaceResponse:
                    HandleGetTopicsOfNamespaceSuccess(cmd.GetTopicsOfNamespaceResponse);
                    break;
                case BaseCommand.Types.Type.Success:
                    HandleSuccess(cmd.Success);
                    break;
                case BaseCommand.Types.Type.TcClientConnectResponse:
                    HandleNewTcClientConnectResponse(cmd.TcClientConnectResponse);
                    break;
                case BaseCommand.Types.Type.SendReceipt:
                    HandleSendReceipt(cmd.SendReceipt);
                    break;
                case BaseCommand.Types.Type.GetOrCreateSchemaResponse:
                    HandleGetOrCreateSchemaResponse(cmd.GetOrCreateSchemaResponse);
                    break;
                case BaseCommand.Types.Type.ProducerSuccess:
                    HandleProducerSuccess(cmd.ProducerSuccess);
                    break;
                case BaseCommand.Types.Type.Error:
                    HandleError(cmd.Error);
                    break;
                case BaseCommand.Types.Type.GetSchemaResponse:
                    HandleGetSchemaResponse(cmd.GetSchemaResponse);
                    break;
                case BaseCommand.Types.Type.LookupResponse:
                    HandleLookupResponse(cmd.LookupTopicResponse);
                    break;
                case BaseCommand.Types.Type.PartitionedMetadataResponse:
                    HandlePartitionResponse(cmd.PartitionMetadataResponse);
                    break;
                case BaseCommand.Types.Type.ActiveConsumerChange:
                    HandleActiveConsumerChange(cmd.ActiveConsumerChange);
                    break;
                case BaseCommand.Types.Type.NewTxnResponse:
                    HandleNewTxnResponse(cmd.NewTxnResponse);
                    break;
                case BaseCommand.Types.Type.AddPartitionToTxnResponse:
                    HandleAddPartitionToTxnResponse(cmd.AddPartitionToTxnResponse);
                    break;
                case BaseCommand.Types.Type.AddSubscriptionToTxnResponse:
                    HandleAddSubscriptionToTxnResponse(cmd.AddSubscriptionToTxnResponse);
                    break;
                case BaseCommand.Types.Type.EndTxnResponse:
                    HandleEndTxnResponse(cmd.EndTxnResponse);
                    break;
                case BaseCommand.Types.Type.SendError:
                    HandleSendError(cmd.SendError);
                    break;
                case BaseCommand.Types.Type.Ping:
                    HandlePing(cmd.Ping);
                    break;
                case BaseCommand.Types.Type.CloseProducer:
                    HandleCloseProducer(cmd.CloseProducer);
                    break;
                case BaseCommand.Types.Type.CloseConsumer:
                    HandleCloseConsumer(cmd.CloseConsumer);
                    break;
                case BaseCommand.Types.Type.ReachedEndOfTopic:
                    HandleReachedEndOfTopic(cmd.ReachedEndOfTopic);
                    break;
                case BaseCommand.Types.Type.AckResponse:
                    HandleAckResponse(cmd.AckResponse);
                    break;
                case BaseCommand.Types.Type.WatchTopicListSuccess:
                    HandleCommandWatchTopicListSuccess(cmd.WatchTopicListSuccess);
                    break;
                case BaseCommand.Types.Type.WatchTopicUpdate:
                    HandleCommandWatchTopicUpdate(cmd.WatchTopicUpdate);
                    break;
                default:
                    _log.Info($"Received '{cmd.Type}' Message in '{_self.Path}'");
                    break;
            }
        }
        
        private void RegisterConsumer(long consumerId, IActorRef consumer)
        {
            if (_consumers.ContainsKey(consumerId))
                _consumers.Remove(consumerId);

            _consumers.Add(consumerId, consumer);
        }
        private PulsarClientException GetPulsarClientException(ServerError error, string errorMsg)
        {
            switch (error)
            {
                case ServerError.AuthenticationError:
                    return new PulsarClientException.AuthenticationException(errorMsg);
                case ServerError.AuthorizationError:
                    return new PulsarClientException.AuthorizationException(errorMsg);
                case ServerError.ProducerBusy:
                    return new PulsarClientException.ProducerBusyException(errorMsg);
                case ServerError.ConsumerBusy:
                    return new PulsarClientException.ConsumerBusyException(errorMsg);
                case ServerError.MetadataError:
                    return new PulsarClientException.BrokerMetadataException(errorMsg);
                case ServerError.PersistenceError:
                    return new PulsarClientException.BrokerPersistenceException(errorMsg);
                case ServerError.ServiceNotReady:
                    return new PulsarClientException.LookupException(errorMsg);
                case ServerError.TooManyRequests:
                    return new PulsarClientException.TooManyRequestsException(errorMsg);
                case ServerError.ProducerBlockedQuotaExceededError:
                    return new PulsarClientException.ProducerBlockedQuotaExceededError(errorMsg);
                case ServerError.ProducerBlockedQuotaExceededException:
                    return new PulsarClientException.ProducerBlockedQuotaExceededException(errorMsg);
                case ServerError.TopicTerminatedError:
                    return new PulsarClientException.TopicTerminatedException(errorMsg);
                case ServerError.IncompatibleSchema:
                    return new PulsarClientException.IncompatibleSchemaException(errorMsg);
                case ServerError.TopicNotFound:
                    return new PulsarClientException.TopicDoesNotExistException(errorMsg);
                case ServerError.ConsumerAssignError:
                    return new PulsarClientException.ConsumerAssignException(errorMsg);
                case ServerError.NotAllowedError:
                    return new PulsarClientException.NotAllowedException(errorMsg);
                case ServerError.TransactionConflict:
                    return new PulsarClientException.TransactionConflictException(errorMsg);
                case ServerError.ProducerFenced:
                    return new PulsarClientException.ProducerFencedException(errorMsg);
                case ServerError.TransactionCoordinatorNotFound:
                    return new PulsarClientException.TransactionCoordinatorNotFoundException(errorMsg);
                case ServerError.UnknownError:
                    return null;
                default:
                    return new PulsarClientException(errorMsg);
            }
        }

        private TransactionCoordinatorClientException GetExceptionByServerError(ServerError serverError, string msg)
        {
            switch (serverError)
            {
                case ServerError.TransactionCoordinatorNotFound:
                    return new TransactionCoordinatorClientException.CoordinatorNotFoundException(msg);
                case ServerError.InvalidTxnStatus:
                    return new TransactionCoordinatorClientException.InvalidTxnStatusException(msg);
                case ServerError.TransactionNotFound:
                    return new TransactionCoordinatorClientException.TransactionNotFoundException(msg);
                case ServerError.UnknownError:
                    return new TransactionCoordinatorClientException.NoException();
                default:
                    return new TransactionCoordinatorClientException(msg);
            }
        }
        private void RegisterProducer(long producerId, IActorRef producer)
        {
            _producers.TryAdd(producerId, producer);
        }
        private void RegisterTransactionMetaStoreHandler(long transactionMetaStoreId, IActorRef handler)
        {
            if(!_transactionMetaStoreHandlers.ContainsKey(transactionMetaStoreId))
                  _transactionMetaStoreHandlers.Add(transactionMetaStoreId, handler);
        }
        private void RemoveProducer(long producerId)
        {
            _producers.TryRemove(producerId, out var r);
        }

        private void RemoveConsumer(long consumerId)
        {
            _consumers.Remove(consumerId);
        }
        private long _reqId = 0;
        private void CheckRequestTimeout()
        {

            while (!_requestTimeoutQueue.IsEmpty)
            {
                var req = _requestTimeoutQueue.TryPeek(out var request);
                
                if (!req || DateTimeHelper.CurrentUnixTimeMillis() - request.CreationTimeMs < _operationTimeout.TotalMilliseconds)
                {
                    // if there is no request that is timed out then exit the loop
                    break;
                }
                if (!_requestTimeoutQueue.TryDequeue(out request))
                {
                    // the request has been removed by another thread
                    continue;
                }
                if (_pendingRequests.Remove(request.RequestId, out var val))
                {
                    var timeoutMessage = $"{request.RequestId} {request.RequestType.Description} timedout after ms {_operationTimeout.TotalMilliseconds}";
                    //_log.Warning(timeoutMessage);
                    //_log.Info(val.Requester.Path.ToString());
                    val.Requester.Tell(new AskResponse(new PulsarClientException(new Exception(timeoutMessage))));
                }
                _reqId = request.RequestId + 1;    
            }
            Timers.StartSingleTimer($"{_reqId}", RequestTimeout.Instance, _operationTimeout);
            //_timeoutTask = Context.System.Scheduler.ScheduleTellOnceCancelable(_operationTimeout, Self, RequestTimeout.Instance, ActorRefs.NoSender);

        }
        
        public AbstractByteBuffer NewConnectCommand()
        {
            // mutual authentication is to auth between `remoteHostName` and this client for this channel.
            // each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
            // and return authData to server.
            _authenticationDataProvider = _authentication.GetAuthData(_remoteHostName);
            var authData = _authenticationDataProvider.Authenticate(Shared.AuthData.InitAuthData);
            return Commands.NewConnect(_authentication.AuthMethodName, authData, _protocolVersion, _clientVersion, _proxyToTargetBrokerAddress, _originalPrincipal, null, null);
        }
        #region privates
        internal enum State
        {
            None,
            SentConnectFrame,
            Ready,
            Failed,
            Connecting
        }

        private class RequestTime
        {
            internal readonly long CreationTimeMs;
            internal readonly long RequestId;
            internal readonly RequestType RequestType;

            internal RequestTime(long creationTime, long requestId, RequestType requestType)
            {
                CreationTimeMs = creationTime;
                RequestId = requestId;
                RequestType = requestType;
            }
            internal RequestTime(long requestId, RequestType requestType)
            {
                RequestId = requestId;
                RequestType = requestType;
            }
        }

        private sealed class RequestType
        {
            public static readonly RequestType Command = new RequestType("Command", InnerEnum.Command);
            public static readonly RequestType GetLastMessageId = new RequestType("GetLastMessageId", InnerEnum.GetLastMessageId);
            public static readonly RequestType GetTopics = new RequestType("GetTopics", InnerEnum.GetTopics);
            public static readonly RequestType GetSchema = new RequestType("GetSchema", InnerEnum.GetSchema);
            public static readonly RequestType GetOrCreateSchema = new RequestType("GetOrCreateSchema", InnerEnum.GetOrCreateSchema);
            public static readonly RequestType AckResponse = new RequestType("AckResponse", InnerEnum.AckResponse);
            public static readonly RequestType Lookup = new RequestType("Lookup", InnerEnum.Lookup);

            private static readonly List<RequestType> valueList = new List<RequestType>();

            static RequestType()
            {
                valueList.Add(Command);
                valueList.Add(GetLastMessageId);
                valueList.Add(GetTopics);
                valueList.Add(GetSchema);
                valueList.Add(GetOrCreateSchema);
                valueList.Add(AckResponse);
                valueList.Add(Lookup);
            }

            public enum InnerEnum
            {
                Command,
                GetLastMessageId,
                GetTopics,
                GetSchema,
                GetOrCreateSchema,
                AckResponse,
                Lookup
            }

            public readonly InnerEnum innerEnumValue;
            private readonly string nameValue;
            private readonly int ordinalValue;
            private static int nextOrdinal = 0;

            private RequestType(string name, InnerEnum innerEnum)
            {
                nameValue = name;
                ordinalValue = nextOrdinal++;
                innerEnumValue = innerEnum;
            }

            internal string Description
            {
                get
                {
                    if (this == Command)
                    {
                        return "request";
                    }
                    else
                    {
                        return nameValue + " request";
                    }
                }
            }

            public static RequestType[] Values()
            {
                return valueList.ToArray();
            }

            public int Ordinal()
            {
                return ordinalValue;
            }

            public override string ToString()
            {
                return nameValue;
            }

            public static RequestType ValueOf(string name)
            {
                foreach (var enumInstance in valueList)
                {
                    if (enumInstance.nameValue == name)
                    {
                        return enumInstance;
                    }
                }
                throw new ArgumentException(name);
            }
        }

        #endregion


    }

    internal sealed class RequestTimeout
    {
        public static RequestTimeout Instance = new RequestTimeout();
    }
    internal sealed class SendPing
    {
        public static SendPing Instance = new SendPing();
    }
    internal record IsSupportsGetPartitionedMetadataWithoutAutoCreation()
    {
        internal static IsSupportsGetPartitionedMetadataWithoutAutoCreation Instance = new IsSupportsGetPartitionedMetadataWithoutAutoCreation();
    }
}
