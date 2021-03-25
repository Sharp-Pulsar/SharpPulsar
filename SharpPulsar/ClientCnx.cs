using Akka.Actor;
using Akka.Event;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Model;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
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
using SharpPulsar.Extension;
using SharpPulsar.SocketImpl;

namespace SharpPulsar
{
	internal sealed class ClientCnx : ReceiveActor
	{
		private readonly SocketClient _socketClient;
		private readonly IAuthentication _authentication;
		private State _state;
		private readonly IActorRef _self;

		private readonly Dictionary<long, (ReadOnlySequence<byte> Message, IActorRef Requester)> _pendingRequests = new Dictionary<long, (ReadOnlySequence<byte> Message, IActorRef Requester)>();
		// LookupRequests that waiting in client side.
		private readonly LinkedList<KeyValuePair<long, KeyValuePair<byte[], LookupDataResult>>> _waitingLookupRequests;

		private readonly ConcurrentDictionary<long, IActorRef> _producers = new ConcurrentDictionary<long, IActorRef>();

		private readonly Dictionary<long, IActorRef> _consumers = new Dictionary<long, IActorRef>();
		private readonly Dictionary<long, IActorRef> _transactionMetaStoreHandlers = new Dictionary<long, IActorRef>();

		private readonly ConcurrentQueue<RequestTime> _requestTimeoutQueue = new ConcurrentQueue<RequestTime>();

		private volatile int _numberOfRejectRequests = 0;

		private static int _maxMessageSize = Commands.DefaultMaxMessageSize;

		private readonly int _maxNumberOfRejectedRequestPerConnection;
		private readonly int _rejectedRequestResetTimeSec = 60;
		private int _protocolVersion;
		private readonly long _operationTimeoutMs;

		private readonly ILoggingAdapter _log;

		private string _proxyToTargetBrokerAddress;
		private readonly byte[] _pong = new Commands().NewPong();
		private List<byte> _pendingReceive;

		private string _remoteHostName;
		private bool _isTlsHostnameVerificationEnable;
		private readonly ClientConfigurationData _clientConfigurationData;

		private readonly TlsHostnameVerifier _hostnameVerifier;

		private ICancelable _timeoutTask;

		private ICancelable _sendPing;
		private readonly IActorRef _parent;

		// Added for mutual authentication.
		private IAuthenticationDataProvider _authenticationDataProvider;
		public ClientCnx(ClientConfigurationData conf, DnsEndPoint endPoint, string targetBroker = "") : this(conf, endPoint, new Commands().CurrentProtocolVersion, targetBroker)
		{
		}

		public ClientCnx(ClientConfigurationData conf, DnsEndPoint endPoint, int protocolVersion, string targetBroker = "")
		{
			_parent = Context.Parent;
			_pendingReceive = new List<byte>();
			_log = Context.GetLogger();
			_remoteHostName = endPoint.Host;
			_self = Self;
			_clientConfigurationData = conf;
			_hostnameVerifier = new TlsHostnameVerifier(Context.GetLogger());
			_proxyToTargetBrokerAddress = targetBroker;
			_socketClient = (SocketClient)SocketClient.CreateClient(conf, endPoint, endPoint.Host, Context.System.Log);
			_socketClient.OnConnect += OnConnected;
			_socketClient.OnDisconnect += OnDisconnected;
			Condition.CheckArgument(conf.MaxLookupRequest > conf.ConcurrentLookupRequest);
			_waitingLookupRequests = new LinkedList<KeyValuePair<long, KeyValuePair<byte[], LookupDataResult>>>();
			_authentication = conf.Authentication;
			_maxNumberOfRejectedRequestPerConnection = conf.MaxNumberOfRejectedRequestPerConnection;
			_operationTimeoutMs = conf.OperationTimeoutMs;
			_state = State.None;
			_isTlsHostnameVerificationEnable = conf.TlsHostnameVerificationEnable;
			_protocolVersion = protocolVersion;
			_socketClient.Connect();
			Receive<Payload>(p =>
			{
				switch (p.Command)
				{
					case "NewLookup":
						NewLookup(p.Bytes, p.RequestId);
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
					case "NewAddSubscriptionToTxn":
					case "NewAddPartitionToTxn":
					case "NewTxn":
					case "NewEndTxn":
						_socketClient.SendMessage(p.Bytes);
						break;
					default:
						SendRequest(p.Bytes, p.RequestId);
						break;

				}
			});

			Receive<RegisterProducer>(m => {
				RegisterProducer(m.ProducerId, m.Producer);
			});
			Receive<RegisterConsumer>(m => {
				RegisterConsumer(m.ConsumerId, m.Consumer);
			});
			Receive<RemoveProducer>(m => {

				RemoveProducer(m.ProducerId);
			});
			Receive<MaxMessageSize>(_ => {

				Sender.Tell(new MaxMessageSizeResponse(_maxMessageSize));
			});
			Receive<RemoveConsumer>(m => {
				RemoveConsumer(m.ConsumerId);
			});
			Receive<SendPing>(m => {
				_socketClient.SendMessage(_pong);
			});
			Receive<RequestTimeout>(m => {
				CheckRequestTimeout();
			});
			Receive<RegisterTransactionMetaStoreHandler>(h => {
				RegisterTransactionMetaStoreHandler(h.TransactionCoordinatorId, h.Coordinator);
			});
			Receive<SendRequestWithId>(r => {
				SendRequestWithId(r.Message, r.RequestId, r.NeedsResponse);
			});
			Receive<RemoteEndpointProtocolVersion>(r => {
				Sender.Tell(new RemoteEndpointProtocolVersionResponse(_protocolVersion));
			});
		}
		private void OnConnected()
		{
			_timeoutTask = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(_operationTimeoutMs), Self, RequestTimeout.Instance, ActorRefs.NoSender);

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
			_socketClient.SendMessage(NewConnectCommand());
			_state = State.SentConnectFrame;
			_socketClient.ReceiveMessageObservable.Subscribe(a => OnCommandReceived(a));
		}
		private void OnDisconnected()
		{
			_log.Info($"{_remoteHostName} Disconnected");
			PulsarClientException e = new PulsarClientException("Disconnected from server at " + _remoteHostName);


			// Notify all attached producers/consumers so they have a chance to reconnect
			_producers.ForEach(p => p.Value.Tell(new ConnectionClosed(_self)));
			_consumers.ForEach(c => c.Value.Tell(new ConnectionClosed(_self)));
			_transactionMetaStoreHandlers.ForEach(t => t.Value.Tell(new ConnectionClosed(_self)));

			_pendingRequests.Clear();
			_waitingLookupRequests.Clear();

			_producers.Clear();
			_consumers.Clear();

			_timeoutTask.Cancel(true);
		}
		private void ExceptionCaught(Exception cause)
		{
			if (_state != State.Failed)
			{
				// No need to report stack trace for known exceptions that happen in disconnections
				_log.Warning($"[{_remoteHostName}] Got exception {cause.StackTrace}");
				_state = State.Failed;
			}
			else
			{
				// At default info level, suppress all subsequent exceptions that are thrown when the connection has already
				// failed
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"[{_remoteHostName}] Got exception: {cause}");
				}
			}

		}

		protected override void PostStop()
		{
			_timeoutTask?.Cancel();
			_sendPing?.Cancel();
			base.PostStop();
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
			_protocolVersion = connected.ProtocolVersion;
			_state = State.Ready;
			_parent.Tell(new ConnectionOpened(_self, connected.MaxMessageSize, _protocolVersion));
		}

		private void HandleAuthChallenge(CommandAuthChallenge authChallenge)
		{
			// mutual authn. If auth not complete, continue auth; if auth complete, complete connectionFuture.
			try
			{
				var assemblyName = Assembly.GetCallingAssembly().GetName();
				var authData = _authenticationDataProvider.Authenticate(new Auth.AuthData(authChallenge.Challenge.auth_data));
				var auth = new AuthData { auth_data = (authData.Bytes.ToBytes()) };
				var clientVersion = assemblyName.Name + " " + assemblyName.Version.ToString(3);
				var request = new Commands().NewAuthResponse(_authentication.AuthMethodName, auth, _protocolVersion, clientVersion);

				if (_log.IsDebugEnabled)
				{
					_log.Debug($"Mutual auth {_authentication.AuthMethodName}");
				}

				_socketClient.SendMessage(request);
				if (_state == State.SentConnectFrame)
				{
					_state = State.Connecting;
				}
			}
			catch (Exception e)
			{
				_log.Error($"Error mutual verify: {e}");
			}
		}

		private void HandleSendReceipt(CommandSendReceipt sendReceipt)
		{
			Condition.CheckArgument(_state == State.Ready);

			long producerId = (long)sendReceipt.ProducerId;
			long sequenceId = (long)sendReceipt.SequenceId;
			long highestSequenceId = (long)sendReceipt.HighestSequenceId;
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

		private void HandleMessage(CommandMessage msg, MessageMetadata metadata, byte[] payload, bool checkSum, short magicNumber)
		{
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Received a message from the server: {msg}");
			}
			MessageIdData id = new MessageIdData
			{
				AckSets = msg.AckSets,
				ledgerId = msg.MessageId.ledgerId,
				entryId = msg.MessageId.entryId,
				Partition = msg.MessageId.Partition,
				BatchSize = msg.MessageId.BatchSize,
				BatchIndex = msg.MessageId.BatchIndex
			};
			var message = new MessageReceived(metadata, payload, id, (int)msg.RedeliveryCount, checkSum, magicNumber);
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

		private void HandleSuccess(CommandSuccess success)
		{
			Condition.CheckArgument(_state == State.Ready);

			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Received success response from server: {success.RequestId}");
			}
			long requestId = (long)success.RequestId;
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
			long requestId = (long)success.RequestId;
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
			long requestId = (long)success.RequestId;
			if (_pendingRequests.TryGetValue(requestId, out var producer))
			{
				_pendingRequests.Remove(requestId);
				producer.Requester.Tell(new ProducerResponse(success.ProducerName, success.LastSequenceId, success.SchemaVersion));
			}
			else
			{
				_log.Warning($"Received unknown request id from server: {success.RequestId}");
			}
		}

		private void HandleLookupResponse(CommandLookupTopicResponse lookupResult)
		{
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Received Broker lookup response: {lookupResult.Response}");
			}

			long requestId = (long)lookupResult.RequestId;
			if (RemovePendingLookupRequest(requestId, out var requester))
			{

				if (CommandLookupTopicResponse.LookupType.Failed.Equals(lookupResult.Response))
				{
					if (lookupResult?.Error != null)
					{
						CheckServerError(lookupResult.Error, lookupResult.Message);
						var ex = GetPulsarClientException(lookupResult.Error, lookupResult.Message);
						requester.Tell(new Failure { Exception = ex });
					}
					else
					{
						var ex = new PulsarClientException.LookupException("Empty lookup response");
						requester.Tell(new Failure { Exception = ex });
					}
				}
				else
				{
					requester.Tell(new LookupDataResult(lookupResult));
				}
			}
			else
			{
				_log.Warning($"Received unknown request id from server: {lookupResult.RequestId}");
			}
		}
		private void HandlePing(CommandPing ping)
		{
			// Immediately reply success to ping requests
			if (_log.IsEnabled(LogLevel.DebugLevel))
			{
				_log.Debug($"[{_self.Path}] [{_remoteHostName}] Replying back to ping message");
			}
			_socketClient.SendMessage(_pong);
		}
		private void HandlePartitionResponse(CommandPartitionedTopicMetadataResponse lookupResult)
		{
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Received Broker Partition response: {lookupResult.Partitions}");
			}

			long requestId = (long)lookupResult.RequestId;
			if (RemovePendingLookupRequest(requestId, out var requester))
			{
				if (CommandPartitionedTopicMetadataResponse.LookupType.Failed.Equals(lookupResult.Response))
				{
					if (lookupResult?.Error != null)
					{
						CheckServerError(lookupResult.Error, lookupResult.Message);
						var ex = GetPulsarClientException(lookupResult.Error, lookupResult.Message);
						requester.Tell(new ClientExceptions(ex));
					}
					else
					{
						var ex = new PulsarClientException.LookupException("Empty lookup response");
						requester.Tell(ex);
					}
				}
				else
				{
					// return LookupDataResult when Result.response = success/redirect
					requester.Tell(new LookupDataResult((int)lookupResult.Partitions));
				}
			}
			else
			{
				_log.Warning($"Received unknown request id from server: {lookupResult.RequestId}");
			}
		}

		private void HandleReachedEndOfTopic(CommandReachedEndOfTopic commandReachedEndOfTopic)
		{
			long consumerId = (long)commandReachedEndOfTopic.ConsumerId;

			_log.Info($"[{_remoteHostName}] Broker notification reached the end of topic: {consumerId}");
			if (_consumers.TryGetValue(consumerId, out var consumer))
			{
				consumer.Tell(SetTerminated.Instance);
			}
		}

		// caller of this method needs to be protected under pendingLookupRequestSemaphore
		private void AddPendingLookupRequests(long requestId, ReadOnlySequence<byte> message)
		{
			_pendingRequests.Add(requestId, (message, Sender));
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

			long producerId = (long)sendError.ProducerId;
			long sequenceId = (long)sendError.SequenceId;

			switch (sendError.Error)
			{
				case ServerError.ChecksumError:
					_producers[producerId].Tell(new RecoverChecksumError(Self, sequenceId));
					break;

				case ServerError.TopicTerminatedError:
					_producers[producerId].Tell(new Messages.Terminated(Self));
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
			long requestId = (long)error.RequestId;

			if (_pendingRequests.TryGetValue(requestId, out var request))
			{
				if (error.Error == ServerError.ProducerBlockedQuotaExceededError)
				{
					_log.Warning($"Producer creation has been blocked because backlog quota exceeded for producer topic");
					request.Requester.Tell(new ClientExceptions(new PulsarClientException.AuthenticationException("Producer creation has been blocked because backlog quota exceeded for producer topic")));
				}
				else if (error.Error == ServerError.AuthenticationError)
				{
					request.Requester.Tell(new ClientExceptions(new PulsarClientException.AuthenticationException(error.Message)));
					_log.Error("Failed to authenticate the client");
				}
				else
					request.Requester.Tell(new ClientExceptions(GetPulsarClientException(error.Error, error.Message)));
			}
			else
			{
				_log.Warning($"Received unknown request id from server: {error.RequestId}");
			}
		}

		private void HandleCloseProducer(CommandCloseProducer closeProducer)
		{
			_log.Info($"[{_remoteHostName}] Broker notification of Closed producer: {closeProducer.ProducerId}");
			long producerId = (long)closeProducer.ProducerId;
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

			long consumerId = (long)closeConsumer.ConsumerId;
			if (_consumers.TryGetValue(consumerId, out var consumer))
			{
				consumer.Tell(new ConnectionClosed(_self));
			}
			else
			{
				_log.Warning($"Consumer with id {consumerId} not found while closing consumer ");
			}
		}

		private bool HandshakeCompleted
		{
			get
			{
				return _state == State.Ready;
			}
		}

		private void NewLookup(byte[] request, long requestId)
		{
			AddPendingLookupRequests(requestId, new ReadOnlySequence<byte>(request));
			_socketClient.SendMessage(request);
		}

		private void NewGetTopicsOfNamespace(byte[] request, long requestId)
		{
			_ = SendRequestAndHandleTimeout(request, requestId, RequestType.GetTopics);
		}

		private void HandleGetTopicsOfNamespaceSuccess(CommandGetTopicsOfNamespaceResponse success)
		{
			Condition.CheckArgument(_state == State.Ready);

			long requestId = (long)success.RequestId;

			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Received get topics of namespace success response from server: {success.RequestId} - topics.size: {success.Topics.Count}");
			}

			if (_pendingRequests.TryGetValue(requestId, out var requester))
			{
				requester.Requester.Tell(new GetTopicsOfNamespaceResponse(success));
			}
			else
			{
				_log.Warning($"Received unknown request id from server: {success.RequestId}");
			}
		}

		private void HandleGetSchemaResponse(CommandGetSchemaResponse commandGetSchemaResponse)
		{
			Condition.CheckArgument(_state == State.Ready);
			long requestId = (long)commandGetSchemaResponse.RequestId;

			if (_pendingRequests.TryGetValue(requestId, out var requester))
			{
				requester.Requester.Tell(new GetSchemaResponse(commandGetSchemaResponse));
			}
			else
				_log.Warning($"Received unknown request id from server: {requestId}");
		}

		private void HandleGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse commandGetOrCreateSchemaResponse)
		{
			Condition.CheckArgument(_state == State.Ready);
			long requestId = (long)commandGetOrCreateSchemaResponse.RequestId;
			if (_pendingRequests.TryGetValue(requestId, out var requester))
			{
				requester.Requester.Tell(new GetOrCreateSchemaResponse(commandGetOrCreateSchemaResponse));
			}
			else
				_log.Warning($"Received unknown request id from server: {requestId}");
		}

		private void SendRequestWithId(byte[] cmd, long requestId, bool reply)
		{
			var sent = SendRequestAndHandleTimeout(cmd, requestId, RequestType.Command);
			if (reply)
				Sender.Tell(sent);
		}

		private bool SendRequestAndHandleTimeout(byte[] requestMessage, long requestId, RequestType requestType)
		{
			_pendingRequests.Add(requestId, (new ReadOnlySequence<byte>(requestMessage), Sender));
			_socketClient.SendMessage(requestMessage);
			/*var task = _socketClient.Execute(requestMessage); 
			if (task.IsFaulted)
			{
				_log.Warning($"Failed to send {requestType.Description} to broker: {task.Exception}");
				_pendingRequests.Remove(requestId);
				return false;
			}*/
			_requestTimeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId, requestType));
			return true;
		}
		private void SendRequest(byte[] requestMessage, long requestId)
		{
			if (requestId >= 0)
				_pendingRequests.Add(requestId, (new ReadOnlySequence<byte>(requestMessage), Sender));

			_socketClient.SendMessage(requestMessage);
		}

		private void SendGetLastMessageId(byte[] request, long requestId)
		{
			_ = SendRequestAndHandleTimeout(request, requestId, RequestType.GetLastMessageId);
		}

		private void SendGetRawSchema(byte[] request, long requestId)
		{
			_ = SendRequestAndHandleTimeout(request, requestId, RequestType.GetSchema);
		}

		private void SendGetOrCreateSchema(byte[] request, long requestId)
		{
			_ = SendRequestAndHandleTimeout(request, requestId, RequestType.GetOrCreateSchema);
		}

		private void HandleNewTxnResponse(CommandNewTxnResponse command)
		{
			var handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
			if (handler != null)
			{
				handler.Tell(new NewTxnResponse(command));
			}
		}

		private void HandleAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse command)
		{
			var handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
			if (handler != null)
			{
				handler.Tell(new AddPublishPartitionToTxnResponse(command));
			}
		}

		private void HandleAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse command)
		{
			var handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
			if (handler != null)
			{
				handler.Tell(new AddSubscriptionToTxnResponse(command));
			}
		}


		private void HandleEndTxnResponse(CommandEndTxnResponse command)
		{
			var handler = CheckAndGetTransactionMetaStoreHandler((long)command.TxnidMostBits);
			if (handler != null)
			{
				handler.Tell(new EndTxnResponse(command));
			}
		}

		private IActorRef CheckAndGetTransactionMetaStoreHandler(long tcId)
		{
			if (!_transactionMetaStoreHandlers.TryGetValue(tcId, out var handler))
			{

				_socketClient.Dispose();
				_log.Warning("Close the channel since can't get the transaction meta store handler, will reconnect later.");
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
		private void CheckServerError(ServerError error, string errMsg)
		{
			if (ServerError.ServiceNotReady.Equals(error))
			{
				_log.Error($"Close connection because received internal-server error {errMsg}");
				_socketClient.Dispose();
			}
			else if (ServerError.TooManyRequests.Equals(error))
			{
				long rejectedRequests = _numberOfRejectRequests++;
				if (rejectedRequests >= _maxNumberOfRejectedRequestPerConnection)
				{
					_log.Error($"Close connection because received {this} rejected request in {_rejectedRequestResetTimeSec} seconds ");

					_socketClient.Dispose();
				}
			}
		}
		private void OnCommandReceived((BaseCommand command, MessageMetadata metadata, byte[] payload, bool checkSum, short magicNumber) args)
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
					HandleMessage(msg, args.metadata, args.payload, args.checkSum, args.magicNumber);
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
				default:
					_log.Info($"Received '{cmd.type}' Message in '{_self.Path}'");
					break;
			}
		}
		private void HandleAckResponse(CommandAckResponse ackResponse)
		{
			Condition.CheckArgument(_state == State.Ready);
			Condition.CheckArgument(ackResponse.RequestId >= 0);
			long consumerId = (long)ackResponse.ConsumerId;
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
				case ServerError.UnknownError:
				default:
					return new PulsarClientException(errorMsg);
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
				if (!req || (DateTimeHelper.CurrentUnixTimeMillis() - request.CreationTimeMs) < _operationTimeoutMs)
				{
					// if there is no request that is timed out then exit the loop
					break;
				}
				req = _requestTimeoutQueue.TryDequeue(out request);
				if (_pendingRequests.Remove(request.RequestId))
				{
					string timeoutMessage = string.Format("{0:D} {1} timedout after ms {2:D}", request.RequestId, request.RequestType.Description, _operationTimeoutMs);
					_log.Warning(timeoutMessage);
				}
			}
			_timeoutTask = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(_operationTimeoutMs), Self, RequestTimeout.Instance, ActorRefs.NoSender);

		}
		public byte[] NewConnectCommand()
		{
			// mutual authentication is to auth between `remoteHostName` and this client for this channel.
			// each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
			// and return authData to server.
			_authenticationDataProvider = _authentication.GetAuthData(_remoteHostName);
			var authData = _authenticationDataProvider.Authenticate(_authentication.AuthMethodName.ToLower() == "sts" ? null : new Auth.AuthData(Auth.AuthData.InitAuthData));
			var assemblyName = Assembly.GetCallingAssembly().GetName();
			var auth = new AuthData { auth_data = (authData.Bytes.ToBytes()) };
			var clientVersion = assemblyName.Name + " " + assemblyName.Version.ToString(3);

			return new Commands().NewConnect(_authentication.AuthMethodName, auth, _protocolVersion, clientVersion, _proxyToTargetBrokerAddress, string.Empty, null, string.Empty);
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

			private static readonly List<RequestType> valueList = new List<RequestType>();

			static RequestType()
			{
				valueList.Add(Command);
				valueList.Add(GetLastMessageId);
				valueList.Add(GetTopics);
				valueList.Add(GetSchema);
				valueList.Add(GetOrCreateSchema);
			}

			public enum InnerEnum
			{
				Command,
				GetLastMessageId,
				GetTopics,
				GetSchema,
				GetOrCreateSchema
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
				foreach (RequestType enumInstance in valueList)
				{
					if (enumInstance.nameValue == name)
					{
						return enumInstance;
					}
				}
				throw new System.ArgumentException(name);
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
