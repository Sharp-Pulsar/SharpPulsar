using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Model;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Concurrent;
using SharpPulsar.Exceptions;
using Akka.Util.Internal;
using SharpPulsar.Messages;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Common.Entity;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Tls;
using SharpPulsar.Messages.Requests;
using System.Net;
using System.Threading.Tasks;
using SharpPulsar.Client.Internal;
using static SharpPulsar.Client.Internal.SocketClientActor;

namespace SharpPulsar.Client
{
    internal sealed class ClientCnx : ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _socketClient;
        private readonly IAuthentication _authentication;
        private State _state;
        private readonly IActorRef _self;
        private IActorRef _sendMessage;
        private IActorRef _sender;

        private readonly Dictionary<long, (ReadOnlySequence<byte> Message, IActorRef Requester)> _pendingRequests = new Dictionary<long, (ReadOnlySequence<byte> Message, IActorRef Requester)>();
        // LookupRequests that waiting in client side.
        private readonly LinkedList<KeyValuePair<long, KeyValuePair<ReadOnlySequence<byte>, LookupDataResult>>> _waitingLookupRequests;
        private readonly ConcurrentDictionary<long, IActorRef> _producers = new ConcurrentDictionary<long, IActorRef>();
        private readonly ConcurrentDictionary<long, IActorRef> _watcher = new ConcurrentDictionary<long, IActorRef>();
        private readonly Dictionary<long, IActorRef> _consumers = new Dictionary<long, IActorRef>();
        private readonly Dictionary<long, IActorRef> _transactionMetaStoreHandlers = new Dictionary<long, IActorRef>();

        private readonly ConcurrentQueue<RequestTime> _requestTimeoutQueue = new ConcurrentQueue<RequestTime>();
        private readonly Dictionary<long, IActorRef> _topicListWatchers = new Dictionary<long, IActorRef>();
        private int _numberOfRejectRequests = 0;

        private int _maxMessageSize;

        private readonly int _maxNumberOfRejectedRequestPerConnection;
        private readonly int _rejectedRequestResetTimeSec = 60;
        private int _protocolVersion;
        private readonly TimeSpan _operationTimeout;

        private readonly ILoggingAdapter _log;

        private readonly string _proxyToTargetBrokerAddress;
        private readonly ReadOnlySequence<byte> _pong;
        private readonly List<byte> _pendingReceive;
        private bool _supportsTopicWatchers;
        private readonly string _remoteHostName;
        private readonly bool _isTlsHostnameVerificationEnable;
        private readonly ClientConfigurationData _clientConfigurationData;
        private readonly TaskCompletionSource<ConnectionOpened> _connectionFuture;

        private readonly TlsHostnameVerifier _hostnameVerifier;

        private ICancelable _timeoutTask;

        private readonly ICancelable _sendPing = default;
        private readonly IActorRef _parent;
        private readonly IScheduler _scheduler;

        // Added for mutual authentication.
        private IAuthenticationDataProvider _authenticationDataProvider;
        public ClientCnx(ClientConfigurationData conf, DnsEndPoint endPoint, TaskCompletionSource<ConnectionOpened> connectionFuture, string targetBroker) : this(conf, endPoint, Commands.CurrentProtocolVersion, connectionFuture, targetBroker)
        {
        }

        public ClientCnx(ClientConfigurationData conf, DnsEndPoint endPoint, int protocolVersion, TaskCompletionSource<ConnectionOpened> connectionFuture, string targetBroker)
        {
            _scheduler = Context.System.Scheduler;
            _pong = Commands.NewPong();
            _maxMessageSize = Commands.DefaultMaxMessageSize;
            _connectionFuture = connectionFuture;
            _parent = Context.Parent;
            _pendingReceive = new List<byte>();
            _log = Context.GetLogger();
            _remoteHostName = endPoint.Host;
            _self = Self;
            _clientConfigurationData = conf;
            _hostnameVerifier = new TlsHostnameVerifier(Context.GetLogger());
            _proxyToTargetBrokerAddress = targetBroker;
            _socketClient = Context.ActorOf(SocketClientActor.Prop(Self, conf, endPoint, endPoint.Host));
            //_socketClient = (SocketClient)SocketClient.CreateClient(conf, endPoint, endPoint.Host, Context.GetLogger());

            //_socketClient.OnDisconnect += OnDisconnected;
            Condition.CheckArgument(conf.MaxLookupRequest > conf.ConcurrentLookupRequest);
            _waitingLookupRequests = new LinkedList<KeyValuePair<long, KeyValuePair<ReadOnlySequence<byte>, LookupDataResult>>>();
            _authentication = conf.Authentication;
            _maxNumberOfRejectedRequestPerConnection = conf.MaxNumberOfRejectedRequestPerConnection;
            _operationTimeout = conf.OperationTimeout;
            _state = State.None;
            _isTlsHostnameVerificationEnable = conf.TlsHostnameVerificationEnable;
            _protocolVersion = protocolVersion;

            Connect().GetAwaiter().GetResult();
            //_subscriber = _socketClient.ReceiveMessageObservable.Subscribe(OnCommandReceived);
            Receives();
        }
        private async ValueTask Connect()
        {
            try
            {
                _sendMessage = await _socketClient.Ask<IActorRef>(SocketClientActor.Connect.Instance);
                _timeoutTask = _scheduler.ScheduleTellOnceCancelable(_operationTimeout, _self, RequestTimeout.Instance, ActorRefs.NoSender);

                //_sendPing = _context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(30), Self, SendPing.Instance, ActorRefs.NoSender);

                if (string.IsNullOrWhiteSpace(_proxyToTargetBrokerAddress))
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"{_remoteHostName} Connected to broker");
                    }
                }
                else
                {
                    _log.Info($"{_remoteHostName} Connected through proxy to target broker at {_proxyToTargetBrokerAddress}");
                }
                // Send CONNECT command
                var s = await _sendMessage.Ask<SendMessage>(new SendMessage(NewConnectCommand()));
                _state = State.SentConnectFrame;
            }
            catch (Exception ex)
            {
                _connectionFuture.TrySetException(ex);
            }
            _socketClient.Tell(Start.Instance);
        }
        public static Props Prop(ClientConfigurationData conf, DnsEndPoint endPoint, TaskCompletionSource<ConnectionOpened> connectionFuture, string targetBroker = "")
        {
            return Props.Create(() => new ClientCnx(conf, endPoint, connectionFuture, targetBroker));
        }
        public static Props Prop(ClientConfigurationData conf, DnsEndPoint endPoint, int protocolVersion, TaskCompletionSource<ConnectionOpened> connectionFuture, string targetBroker)
        {
            return Props.Create(() => new ClientCnx(conf, endPoint, protocolVersion, connectionFuture, targetBroker));
        }
        private void Receives()
        {

            Receive<Payload>(p =>
            {
                _sender = Sender;
                switch (p.Command)
                {
                    case "NewLookup":
                        NewLookup(p.Bytes, p.RequestId);
                        break;
                    case "NewAckForReceipt":
                        NewAckForReceipt(p.Bytes, p.RequestId);
                        break;
                    case "NewGetTopicsOfNamespaceRequest":
                        NewGetTopicsOfNamespace(p.Bytes, p.RequestId);
                        break;
                    case "SendGetLastMessageId":
                        SendGetLastMessageId(p.Bytes, p.RequestId);
                        break;
                    case "SendGetRawSchema":
                        SendGetRawSchema(p.Bytes, p.RequestId);
                        break;
                    case "SendGetOrCreateSchema":
                        SendGetOrCreateSchema(p.Bytes, p.RequestId);
                        break;
                    case "NewCloseConsumer":
                    case "NewCloseProducer":
                        try
                        {
                            _sendMessage.Tell(new SendMessage(p.Bytes));
                            Sender.Tell(new AskResponse());
                        }
                        catch (Exception ex)
                        {
                            Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
                        }
                        break;
                    case "NewAddSubscriptionToTxn":
                    case "NewAddPartitionToTxn":
                    case "NewTxn":
                    case "NewEndTxn":
                    case "NewPartitionMetadataRequest":
                    default:
                        SendRequest(p.Bytes, p.RequestId);
                        break;

                }
            }); ;

            Receive<RegisterProducer>(m =>
            {
                RegisterProducer(m.ProducerId, m.Producer);
            });
            Receive<RegisterConsumer>(m =>
            {
                RegisterConsumer(m.ConsumerId, m.Consumer);
            });
            Receive<RemoveProducer>(m =>
            {

                RemoveProducer(m.ProducerId);
            });
            Receive<Close>(m =>
            {
                _socketClient.GracefulStop(TimeSpan.FromSeconds(5));
            });
            Receive<MaxMessageSize>(_ =>
            {
                Sender.Tell(new MaxMessageSizeResponse(_maxMessageSize));
            });
            Receive<RemoveConsumer>(m =>
            {
                RemoveConsumer(m.ConsumerId);
            });
            Receive<SendPing>(m =>
            {
                _sendMessage.Tell(new SendMessage(_pong));
            });
            Receive<RequestTimeout>(m =>
            {
                CheckRequestTimeout();
            });
            Receive<RegisterTransactionMetaStoreHandler>(h =>
            {
                RegisterTransactionMetaStoreHandler(h.TransactionCoordinatorId, h.Coordinator);
            });
            Receive<SendRequestWithId>(r =>
            {
                _sender = Sender;
                SendRequestWithId(r.Message, r.RequestId, r.NeedsResponse);
            });
            Receive<RemoteEndpointProtocolVersion>(r =>
            {
                Sender.Tell(new RemoteEndpointProtocolVersionResponse(_protocolVersion));
            });//RemoveTopicListWatcher
            Receive<RegisterTopicListWatcher>(w => RegisterTopicListWatcher(w.WatcherId, w.Watcher));
            Receive<RemoveTopicListWatcher>(w => RemoveTopicListWatcher(w.WatcherId));
            Receive<Reader>(r => OnCommandReceived((r.Command, r.Metadata, r.BrokerEntryMetadata, r.Payload, r.HasValidcheckSum, r.HasMagicNumber)));
        }
        private void OnDisconnected()
        {
            _log.Info($"{_remoteHostName} Disconnected");

            if (_connectionFuture.Task.IsCompleted)
            {
                _connectionFuture.TrySetException(new PulsarClientException("Connection already closed"));
            }

            var e = new PulsarClientException("Disconnected from server at " + _remoteHostName);


            // Notify all attached producers/consumers so they have a chance to reconnect
            _producers.ForEach(p => p.Value.Tell(new ConnectionClosed(_self)));
            _consumers.ForEach(c => c.Value.Tell(new ConnectionClosed(_self)));
            _transactionMetaStoreHandlers.ForEach(t => t.Value.Tell(new ConnectionClosed(_self)));
            _topicListWatchers.ForEach(watcher => watcher.Value.Tell(new ConnectionClosed(_self)));
            _pendingRequests.Clear();
            _waitingLookupRequests.Clear();

            _producers.Clear();
            _consumers.Clear();
            _topicListWatchers.Clear();
            _timeoutTask?.Cancel(true);
        }

        private void NewAckForReceipt(ReadOnlySequence<byte> request, long requestId)
        {
            SendRequestAndHandleTimeout(request, requestId, RequestType.AckResponse);
        }
        protected override void PostStop()
        {
            OnDisconnected();
            _timeoutTask?.Cancel();
            _sendPing?.Cancel();
            //_subscriber.Dispose();
            base.PostStop();
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

        private void HandleCommandWatchTopicUpdate(CommandWatchTopicUpdate commandWatchTopicUpdate)
        {
            Condition.CheckArgument(_state == State.Ready);
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"[ctx] Received watchTopicUpdate command from server: {commandWatchTopicUpdate.WatcherId}");
            }
            var watcherId = (long)commandWatchTopicUpdate.WatcherId;
            if (_watcher.TryGetValue(watcherId, out var watcher))
                watcher.Tell(new CommandWatchTopicUpdateResponse(commandWatchTopicUpdate));
            else
            {
                _log.Warning($"[ctx] Received topic list update for unknown watcher from server: {watcherId}");
            }
        }

        private void HandleConnected(CommandConnected connected)
        {
            Condition.CheckArgument(_state == State.SentConnectFrame || _state == State.Connecting);
            if (connected.MaxMessageSize > 0)
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"{connected.MaxMessageSize} Connection has max message size setting");
                }
                _maxMessageSize = connected.MaxMessageSize;
            }
            if (_log.IsDebugEnabled)
            {
                _log.Debug("Connection is ready");
            }
            // set remote protocol version to the correct version before we complete the connection future
            //if(connected.FeatureFlags != null)
            _supportsTopicWatchers = connected.FeatureFlags.SupportsTopicWatchers;

            _protocolVersion = connected.ProtocolVersion;
            _state = State.Ready;
            _connectionFuture.TrySetResult(new ConnectionOpened(_self, connected.MaxMessageSize, _protocolVersion));
        }
        private void RegisterTopicListWatcher(long watcherId, IActorRef watcher)
        {
            _topicListWatchers.Add(watcherId, watcher);

        }
        private void RemoveTopicListWatcher(long watcherId)
        {
            _topicListWatchers.Remove(watcherId);
        }
        private void HandleAuthChallenge(CommandAuthChallenge authChallenge)
        {

            // mutual authn. If auth not complete, continue auth; if auth complete, complete connectionFuture.
            try
            {
                Condition.CheckArgument(authChallenge.Challenge != null);
                Condition.CheckArgument(authChallenge.Challenge.auth_data != null);

                if (Auth.AuthData.RefreshAuthDataBytes.Equals(authChallenge.Challenge.auth_data))
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
                    var authData = _authenticationDataProvider.Authenticate(Auth.AuthData.Of(authChallenge.Challenge.auth_data));
                    if (!authData.IsComplete())
                    {
                        _connectionFuture.TrySetException(new PulsarClientException.UnsupportedAuthenticationException(new ArgumentException()));
                        return;
                    }
                    var auth = new AuthData { auth_data = authData.Bytes };
                    var request = Commands.NewAuthResponse(_authentication.AuthMethodName, auth, _protocolVersion, "3.0.0");

                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"Mutual auth {_authentication.AuthMethodName}");
                    }

                    _sendMessage.Tell(new SendMessage(request));/*.AsTask()
                        .ContinueWith(task =>
                        {
                            if (task.IsFaulted)
                            {
                                _log.Warning($"Failed to send request for mutual auth to broker: {task.Exception}");
                                _connectionFuture.TrySetException(task.Exception);
                            }
                        });*/
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

        private void HandleSendReceipt(CommandSendReceipt sendReceipt)
        {
            Condition.CheckArgument(_state == State.Ready);

            var producerId = (long)sendReceipt.ProducerId;
            var sequenceId = (long)sendReceipt.SequenceId;
            var highestSequenceId = (long)sendReceipt.HighestSequenceId;
            long ledgerId = -1;
            long entryId = -1;
            if (sendReceipt.MessageId != null)
            {
                ledgerId = (long)sendReceipt.MessageId.ledgerId;
                entryId = (long)sendReceipt.MessageId.entryId;
            }

            if (ledgerId == -1 && entryId == -1)
            {
                _log.Warning($"Message has been dropped for non-persistent topic producer-id {producerId}-{sequenceId}");
            }

            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Got receipt for producer: {producerId} -- msg: S[{sequenceId}]:H[{highestSequenceId}] -- id: {ledgerId}:{entryId}");
            }
            if (_producers.TryGetValue(producerId, out var producer))
                producer.Tell(new AckReceived(sequenceId, highestSequenceId, ledgerId, entryId));
        }

        private void HandleMessage(CommandMessage msg, MessageMetadata metadata, BrokerEntryMetadata brokerEntryMetadata, ReadOnlySequence<byte> payload, bool hasValidCheckSum, bool hasMagicNumber)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Received a message from the server: {msg}");
            }
            var id = new MessageIdData
            {
                AckSets = msg.AckSets,
                ledgerId = msg.MessageId.ledgerId,
                entryId = msg.MessageId.entryId,
                Partition = msg.MessageId.Partition,
                BatchSize = msg.MessageId.BatchSize,
                BatchIndex = msg.MessageId.BatchIndex
            };
            var message = new MessageReceived(metadata, brokerEntryMetadata, payload, id, (int)msg.RedeliveryCount, hasValidCheckSum, hasMagicNumber, (long)msg.ConsumerEpoch, msg.ShouldSerializeConsumerEpoch());
            if (_consumers.TryGetValue((long)msg.ConsumerId, out var consumer))
            {
                consumer.Tell(message);
            }
        }

        private void HandleActiveConsumerChange(CommandActiveConsumerChange change)
        {
            Condition.CheckArgument(_state == State.Ready);

            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Received a consumer group change message from the server : {change}");
            }
            if (_consumers.TryGetValue((long)change.ConsumerId, out var consumer))
            {
                consumer.Tell(new ActiveConsumerChanged(change.IsActive));
            }
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

        private void HandleSuccess(CommandSuccess success)
        {
            Condition.CheckArgument(_state == State.Ready);

            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Received success response from server: {success.RequestId}");
            }
            var requestId = (long)success.RequestId;
            if (_pendingRequests.TryGetValue(requestId, out var req))
            {
                _pendingRequests.Remove(requestId);
                req.Requester.Tell(new CommandSuccessResponse(success), _self);
            }
            else
            {
                _log.Warning($"Received unknown request id from server: {success.RequestId}");
            }
        }

        private void HandleGetLastMessageIdSuccess(CommandGetLastMessageIdResponse success)
        {
            Condition.CheckArgument(_state == State.Ready);

            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Received success GetLastMessageId response from server: {success.RequestId}");
            }
            var requestId = (long)success.RequestId;
            if (_pendingRequests.TryGetValue(requestId, out var request))
            {
                var consumer = request.Requester;
                var req = _pendingRequests.Remove(requestId);
                var lid = success.LastMessageId;
                consumer.Tell(new LastMessageIdResponse((long)lid.ledgerId, (long)lid.entryId, lid.Partition, lid.BatchIndex, lid.BatchSize, lid.AckSets, success.ConsumerMarkDeletePosition));
            }
            else
            {
                _log.Warning($"Received unknown request id from server: {requestId}");
            }
        }

        private void HandleProducerSuccess(CommandProducerSuccess success)
        {
            Condition.CheckArgument(_state == State.Ready);
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Received producer success response from server: {success.RequestId} - producer-name: {success.ProducerName}");
            }
            var requestId = (long)success.RequestId;
            if (_pendingRequests.TryGetValue(requestId, out var producer))
            {
                _pendingRequests.Remove(requestId);
                producer.Requester.Tell(new AskResponse(new ProducerResponse(success.ProducerName, success.LastSequenceId, success.SchemaVersion, GetTopicEpoch(success), success.ProducerReady)));
            }
            else
            {
                _log.Warning($"Received unknown request id from server: {success.RequestId}");
            }
        }
        private long? GetTopicEpoch(CommandProducerSuccess success)
        {
            if (success.ShouldSerializeTopicEpoch())
                return (long)success.TopicEpoch;

            return null;
        }
        private void HandleLookupResponse(CommandLookupTopicResponse lookupResult)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Received Broker lookup response: {lookupResult.Response}");
            }

            var requestId = (long)lookupResult.RequestId;
            if (RemovePendingLookupRequest(requestId, out var requester))
            {

                if (CommandLookupTopicResponse.LookupType.Failed.Equals(lookupResult.Response))
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
        private void HandlePing(CommandPing ping)
        {
            // Immediately reply success to ping requests
            if (_log.IsEnabled(LogLevel.DebugLevel))
            {
                _log.Debug($"[{_self.Path}] [{_remoteHostName}] Replying back to ping message");
            }
            _sendMessage.Tell(new SendMessage(_pong));
        }
        private void HandlePartitionResponse(CommandPartitionedTopicMetadataResponse lookupResult)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Received Broker Partition response: {lookupResult.Partitions}");
            }

            var requestId = (long)lookupResult.RequestId;
            if (RemovePendingLookupRequest(requestId, out var requester))
            {
                if (CommandPartitionedTopicMetadataResponse.LookupType.Failed.Equals(lookupResult?.Response))
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
        private void AddPendingLookupRequests(long requestId, ReadOnlySequence<byte> message)
        {
            _pendingRequests.Add(requestId, (message, _sender));
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

        private void HandleSendError(CommandSendError sendError)
        {
            _log.Warning($"Received send error from server: {sendError.Error} : {sendError.Message}");

            var producerId = (long)sendError.ProducerId;
            var sequenceId = (long)sendError.SequenceId;

            switch (sendError.Error)
            {
                case ServerError.ChecksumError:
                    _producers[producerId].Tell(new RecoverChecksumError(_self, sequenceId));
                    break;

                case ServerError.TopicTerminatedError:
                    _producers[producerId].Tell(new Messages.Terminated(_self));
                    break;

                case ServerError.NotAllowedError:
                    _producers[producerId].Tell(new RecoverNotAllowedError(sequenceId, sendError.Message));
                    break;

                default:
                    // By default, for transient error, let the reconnection logic
                    // to take place and re-establish the produce again
                    //_socketClient.Dispose();
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
                _sender?.Tell(response);
                _log.Warning($"Received unknown request id from server: {error.RequestId}");
            }
        }

        private void HandleCloseProducer(CommandCloseProducer closeProducer)
        {
            _log.Info($"[{_remoteHostName}] Broker notification of Closed producer: {closeProducer.ProducerId}");
            var producerId = (long)closeProducer.ProducerId;
            if (_producers.TryGetValue(producerId, out var producer))
            {
                producer.Tell(new ConnectionClosed(_self));
            }
            else
            {
                _log.Warning($"Producer with id {producerId} not found while closing producer ");
            }
        }

        private void HandleCloseConsumer(CommandCloseConsumer closeConsumer)
        {
            _log.Info($"[{_remoteHostName}] Broker notification of Closed consumer: {closeConsumer.ConsumerId}");

            var consumerId = (long)closeConsumer.ConsumerId;
            if (_consumers.TryGetValue(consumerId, out var consumer))
            {
                consumer.Tell(new ConnectionClosed(_self));
            }
            else
            {
                _log.Warning($"Consumer with id {consumerId} not found while closing consumer ");
            }
        }

        public IStash Stash { get; set; }

        private void NewLookup(ReadOnlySequence<byte> request, long requestId)
        {
            try
            {
                _sendMessage.Tell(new SendMessage(request));
                AddPendingLookupRequests(requestId, request);

            }
            catch (Exception ex)
            {
                _sender.Tell(PulsarClientException.Unwrap(ex));
            }
        }

        private void NewGetTopicsOfNamespace(ReadOnlySequence<byte> request, long requestId)
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
                requester.Requester.Tell(new AskResponse(new GetSchemaResponse(commandGetSchemaResponse)));
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

        private void SendRequestWithId(ReadOnlySequence<byte> cmd, long requestId, bool reply)
        {
            SendRequestAndHandleTimeout(cmd, requestId, RequestType.Command);
        }

        private bool SendRequestAndHandleTimeout(ReadOnlySequence<byte> requestMessage, long requestId, RequestType requestType)
        {
            try
            {
                _sendMessage.Tell(new SendMessage(requestMessage));
                _pendingRequests.Add(requestId, (requestMessage, _sender));

                _requestTimeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId, requestType));
                return true;
            }
            catch (Exception ex)
            {
                _sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
            }
            return false;
        }
        private void SendRequest(ReadOnlySequence<byte> requestMessage, long requestId)
        {
            try
            {
                _sendMessage.Tell(new SendMessage(requestMessage));
                if (requestId >= 0)
                    _pendingRequests.Add(requestId, (requestMessage, _sender));
            }
            catch (Exception ex)
            {
                _sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
            }

        }

        private void SendGetLastMessageId(ReadOnlySequence<byte> request, long requestId)
        {
            SendRequestAndHandleTimeout(request, requestId, RequestType.GetLastMessageId);
        }

        private void SendGetRawSchema(ReadOnlySequence<byte> request, long requestId)
        {
            SendRequestAndHandleTimeout(request, requestId, RequestType.GetSchema);
        }

        private void SendGetOrCreateSchema(ReadOnlySequence<byte> request, long requestId)
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
        private void OnCommandReceived((BaseCommand command, MessageMetadata metadata, BrokerEntryMetadata brokerEntryMetadata, ReadOnlySequence<byte> payload, bool hasValidCheckSum, bool hasMagicNumber) args)
        {
            var cmd = args.command;
            switch (cmd.type)
            {
                case BaseCommand.Type.AuthChallenge:
                    var auth = cmd.authChallenge;
                    HandleAuthChallenge(auth);
                    break;
                case BaseCommand.Type.Message:
                    var msg = cmd.Message;
                    HandleMessage(msg, args.metadata, args.brokerEntryMetadata, args.payload, args.hasValidCheckSum, args.hasMagicNumber);
                    break;
                case BaseCommand.Type.GetLastMessageIdResponse:
                    HandleGetLastMessageIdSuccess(cmd.getLastMessageIdResponse);
                    break;
                case BaseCommand.Type.Connected:
                    HandleConnected(cmd.Connected);
                    break;
                case BaseCommand.Type.GetTopicsOfNamespaceResponse:
                    HandleGetTopicsOfNamespaceSuccess(cmd.getTopicsOfNamespaceResponse);
                    break;
                case BaseCommand.Type.Success:
                    HandleSuccess(cmd.Success);
                    break;
                case BaseCommand.Type.TcClientConnectResponse:
                    HandleNewTcClientConnectResponse(cmd.tcClientConnectResponse);
                    break;
                case BaseCommand.Type.SendReceipt:
                    HandleSendReceipt(cmd.SendReceipt);
                    break;
                case BaseCommand.Type.GetOrCreateSchemaResponse:
                    HandleGetOrCreateSchemaResponse(cmd.getOrCreateSchemaResponse);
                    break;
                case BaseCommand.Type.ProducerSuccess:
                    HandleProducerSuccess(cmd.ProducerSuccess);
                    break;
                case BaseCommand.Type.Error:
                    HandleError(cmd.Error);
                    break;
                case BaseCommand.Type.GetSchemaResponse:
                    HandleGetSchemaResponse(cmd.getSchemaResponse);
                    break;
                case BaseCommand.Type.LookupResponse:
                    HandleLookupResponse(cmd.lookupTopicResponse);
                    break;
                case BaseCommand.Type.PartitionedMetadataResponse:
                    HandlePartitionResponse(cmd.partitionMetadataResponse);
                    break;
                case BaseCommand.Type.ActiveConsumerChange:
                    HandleActiveConsumerChange(cmd.ActiveConsumerChange);
                    break;
                case BaseCommand.Type.NewTxnResponse:
                    HandleNewTxnResponse(cmd.newTxnResponse);
                    break;
                case BaseCommand.Type.AddPartitionToTxnResponse:
                    HandleAddPartitionToTxnResponse(cmd.addPartitionToTxnResponse);
                    break;
                case BaseCommand.Type.AddSubscriptionToTxnResponse:
                    HandleAddSubscriptionToTxnResponse(cmd.addSubscriptionToTxnResponse);
                    break;
                case BaseCommand.Type.EndTxnResponse:
                    HandleEndTxnResponse(cmd.endTxnResponse);
                    break;
                case BaseCommand.Type.SendError:
                    HandleSendError(cmd.SendError);
                    break;
                case BaseCommand.Type.Ping:
                    HandlePing(cmd.Ping);
                    break;
                case BaseCommand.Type.CloseProducer:
                    HandleCloseProducer(cmd.CloseProducer);
                    break;
                case BaseCommand.Type.CloseConsumer:
                    HandleCloseConsumer(cmd.CloseConsumer);
                    break;
                case BaseCommand.Type.ReachedEndOfTopic:
                    HandleReachedEndOfTopic(cmd.reachedEndOfTopic);
                    break;
                case BaseCommand.Type.AckResponse:
                    HandleAckResponse(cmd.ackResponse);
                    break;
                case BaseCommand.Type.WatchTopicListSuccess:
                    HandleCommandWatchTopicListSuccess(cmd.watchTopicListSuccess);
                    break;
                case BaseCommand.Type.WatchTopicUpdate:
                    HandleCommandWatchTopicUpdate(cmd.watchTopicUpdate);
                    break;
                default:
                    _log.Info($"Received '{cmd.type}' Message in '{_self.Path}'");
                    break;
            }
        }
        private void HandleAckResponse(CommandAckResponse ackResponse)
        {
            Condition.CheckArgument(_state == State.Ready);
            Condition.CheckArgument(ackResponse.RequestId >= 0);
            var consumerId = (long)ackResponse.ConsumerId;
            if (ackResponse?.Error == ServerError.UnknownError && string.IsNullOrWhiteSpace(ackResponse.Message))
            {
                _consumers[consumerId].Tell(new AckReceipt((long)ackResponse.RequestId));
            }
            else
            {
                _consumers[consumerId].Tell(new AckError((long)ackResponse.RequestId, GetPulsarClientException(ackResponse.Error, ackResponse.Message)));
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
                if (_requestTimeoutQueue.TryDequeue(out request))
                {
                    if (_pendingRequests.Remove(request.RequestId, out var val))
                    {
                        var timeoutMessage = $"{request.RequestId} {request.RequestType.Description} timedout after ms {_operationTimeout.TotalMilliseconds}";
                        _log.Warning(timeoutMessage);
                        val.Requester.Tell(new AskResponse(new PulsarClientException(new Exception(timeoutMessage))));
                    }
                }
            }
            _timeoutTask = Context.System.Scheduler.ScheduleTellOnceCancelable(_operationTimeout, Self, RequestTimeout.Instance, ActorRefs.NoSender);

        }
        public ReadOnlySequence<byte> NewConnectCommand()
        {
            // mutual authentication is to auth between `remoteHostName` and this client for this channel.
            // each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
            // and return authData to server.
            _authenticationDataProvider = _authentication.GetAuthData(_remoteHostName);
            var authData = _authenticationDataProvider.Authenticate(Auth.AuthData.InitAuthData);
            var auth = new AuthData { auth_data = authData.Bytes };
            return Commands.NewConnect(_authentication.AuthMethodName, auth, _protocolVersion, "3.1.0", _proxyToTargetBrokerAddress, string.Empty, null, string.Empty);
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
}
