using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Akka.Actor;
using Akka.Event;
using BeetleX;
using BeetleX.Buffers;
using BeetleX.Clients;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using AuthData = SharpPulsar.Impl.Auth.AuthData;

namespace SharpPulsar.Akka.Network
{
    public class ClientConnection: ReceiveActor, IWithUnboundedStash
    {
        private readonly IAuthentication _authentication;
        private readonly IActorRef _self;
        private readonly IActorRef _parent;
        private readonly IActorContext _context;
        private readonly ReadOnlySequence<byte> _pong = new ReadOnlySequence<byte>(Commands.NewPong());
        internal Uri RemoteAddress;
        internal int RemoteEndpointProtocolVersion = Enum.GetValues(typeof(ProtocolVersion)).Cast<int>().Max();
        public IActorRef Connection;
        private readonly Queue<Payload> _pendingPayloads;
        private ICancelable _receiveThread;
        private readonly ConcurrentDictionary<long, KeyValuePair<IActorRef, Payload>> _requests = new ConcurrentDictionary<long, KeyValuePair<IActorRef, Payload>>();

        private readonly string _proxyToTargetBrokerAddress;
        private readonly ILoggingAdapter _log;
        private readonly ClientConfigurationData _conf;
        private readonly IActorRef _manager;
        // Added for mutual authentication.
        private IAuthenticationDataProvider _authenticationDataProvider;
        private TcpClient _client;
        private ICancelable _reconnectScheduler;

        public ClientConnection(Uri endPoint, ClientConfigurationData conf, IActorRef manager, string targetBroker = "")
        {
            _pendingPayloads = new Queue<Payload>();
            _context = Context;
            _proxyToTargetBrokerAddress = targetBroker;
            _self = Self;
            RemoteHostName = endPoint.Host;
            _conf = conf;
            _manager = manager;
            Connection = Self;
            RemoteAddress = endPoint;
            _log = _context.System.Log;
            if (conf.MaxLookupRequest < conf.ConcurrentLookupRequest)
                throw new Exception("ConcurrentLookupRequest must be less than MaxLookupRequest");
            _authentication = conf.Authentication;

            _parent = _context.Parent;
           Connect();
           Receive<Payload>(p =>
           {
               _ = _requests.TryAdd(p.RequestId, new KeyValuePair<IActorRef, Payload>(Sender, p));
               Send(p.Bytes, p);
           });
        }

        private void Connect()
        {
            try
            {
                var remoteAddress = RemoteAddress;
                if (!string.IsNullOrWhiteSpace(_conf.ProxyServiceUrl))
                {
                    var proxy = new Uri(_conf.ProxyServiceUrl);
                    //var addresses = Dns.GetHostEntry(proxy.Host);
                    remoteAddress = proxy;
                }

                if (_conf.UseTls)
                {
                    _client = SocketFactory.CreateSslClient<TcpClient>(remoteAddress.Host, remoteAddress.Port, RemoteHostName);
                    _client.CertificateValidationCallback = ValidateServerCertificate;
                }
                else
                    _client = SocketFactory.CreateClient<TcpClient>(remoteAddress.Host, remoteAddress.Port);

                _log.Info($"Opening Connection to: {RemoteAddress}");

                ConnectToServer();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
               _reconnectScheduler = _context.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromSeconds(5),
                   ConnectToServer);
            }
        }

        private void Disconnected()
        {
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                if(_client.IsConnected)
                    return;
                _client.Connect(out var connected);
                if (connected)
                    Connected();
                else
                    _reconnectScheduler = _context.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromSeconds(5), ConnectToServer);
            }
            catch (Exception e)
            {
                _log.Error(e.ToString());
                _reconnectScheduler = _context.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromSeconds(5), ConnectToServer);
                throw;
            }
        }
        private void Connected()
        {
            var context = Context;
            Send(NewConnectCommand());
            if (_receiveThread == null)
            {
                _receiveThread = context.System.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(30), ReceiveIfAvailable);
            } 
        }

        private void OnReceive(PipeStream pipestream, int size)
        {
            var block = pipestream.ReadBytes(size);
            var sequence = new ReadOnlySequence<byte>(block.Data);
            var frameSize = sequence.ReadUInt32(0, true);
            var totalSize = frameSize + 4;
            if (block.Count < totalSize)
                return;
            ProcessIncommingFrames(sequence.Slice(4, frameSize));

        }

        private void ReceiveIfAvailable()
        {
            try
            {
                var client = _client;
                var available = client.Socket.Available;
                if (available <= 0) return;
                var stream = _client.Receive();
                OnReceive(stream, available);
            }
            catch (Exception e)
            {
                _log.Error(e.ToString());
            }
        }
        private void Send(IEnumerable cmd, Payload payload = null)
        {
            try
            {
                if(!_client.IsConnected && payload != null)
                    _pendingPayloads.Enqueue(payload);
                else 
                    _client.SendMessage(cmd);
            }
            catch (Exception e)
            {
                _log.Error(e.ToString());
            }
        }
        public static Props Prop(Uri endPoint, ClientConfigurationData conf, IActorRef manager, string targetBroker = "")
        {
            return Props.Create(() => new ClientConnection(endPoint, conf, manager, targetBroker));
        }
        
        protected override void PostStop()
        {
            _reconnectScheduler?.Cancel();
            _receiveThread?.Cancel();
            _client.DisConnect();
        }

        protected override void Unhandled(object message)
        {
           _log.Info($"C -Unhandled {message.GetType().Name} in {Self.Path}");
        }
        
        public byte[] NewConnectCommand()
		{
			// mutual authentication is to auth between `remoteHostName` and this client for this channel.
			// each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
			// and return authData to server.
			_authenticationDataProvider = _authentication.GetAuthData(RemoteHostName);
			var authData = _authenticationDataProvider.Authenticate(_authentication.AuthMethodName.ToLower() == "sts"? null: new AuthData(AuthData.InitAuthData));
            var assemblyName = Assembly.GetCallingAssembly().GetName();
            var auth = new Protocol.Proto.AuthData { auth_data = ((byte[]) (object) authData.Bytes)};
            var clientVersion = assemblyName.Name + " " + assemblyName.Version.ToString(3);

            return Commands.NewConnect(_authentication.AuthMethodName, auth, RemoteEndpointProtocolVersion, clientVersion, _proxyToTargetBrokerAddress, string.Empty, null, string.Empty);
		}
        private byte[] HandleAuthChallenge(CommandAuthChallenge authChallenge)
        {
            try
            {
                var assemblyName = Assembly.GetCallingAssembly().GetName();
                var authData = _authenticationDataProvider.Authenticate(new AuthData(authChallenge.Challenge.auth_data));
                var clientVersion = assemblyName.Name + " " + assemblyName.Version.ToString(3);
                var auth = new Protocol.Proto.AuthData { auth_data = ((byte[])(object)authData.Bytes) };
                
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("{} Mutual auth {}", RemoteAddress, _authentication.AuthMethodName);
                }
                return Commands.NewAuthResponse(_authentication.AuthMethodName, auth, 15, clientVersion);

            }
            catch (Exception e)
            {
                _log.Error(e.ToString());
                return null;
            }
        }
        
		public void HandlePing(CommandPing ping)
		{
			// Immediately reply success to ping requests
			if (_log.IsEnabled(LogLevel.DebugLevel))
			{
				_log.Debug($"[{_self.Path}] [{RemoteAddress}] Replying back to ping message");
			}
            _client.SendMessage(_pong.ToArray());
        }
        
        private void ProcessIncommingFrames(ReadOnlySequence<byte> frame)
        {
            var log = _log;
            var requests = _requests;
            var parent = _parent;
            var manager = _manager;
            try
            {
                var commandSize = frame.ReadUInt32(0, true);
                var cmd = Serializer.Deserialize(frame.Slice(4, commandSize));
                
                switch (cmd.type)
                {
                    case BaseCommand.Type.AuthChallenge:
                        var auth = cmd.authChallenge;
                        var authen = HandleAuthChallenge(auth);
                        Send(authen);
                        break;
                    case BaseCommand.Type.GetLastMessageIdResponse:
                        var mid = cmd.getLastMessageIdResponse.LastMessageId;
                        var rquestid = (long)cmd.getLastMessageIdResponse.RequestId;
                        requests[rquestid].Key.Tell(new LastMessageIdResponse((long)mid.ledgerId, (long)mid.entryId, mid.Partition, mid.BatchIndex));
                        _requests.TryRemove(rquestid, out var ut);
                        break;
                    case BaseCommand.Type.Connected:
                    {
                        if(_pendingPayloads.Count > 0)
                        {
                                for(var i = 0; i < _pendingPayloads.Count; i++)
                                {
                                    var payload = _pendingPayloads.Dequeue();
                                    Send(payload.Bytes);
                                }
                        }

                        var c = cmd.Connected;
                        parent.Tell(new ConnectedServerInfo(c.MaxMessageSize, c.ProtocolVersion, c.ServerVersion,
                            RemoteHostName), _self);
                        log.Info($"Now connected: Host = {RemoteHostName}, ProtocolVersion = {c.ProtocolVersion}");
                        }
                        break;
                    case BaseCommand.Type.GetTopicsOfNamespaceResponse:
                        var ns = cmd.getTopicsOfNamespaceResponse;
                        var requestid = (long)ns.RequestId;
                        requests[requestid].Key.Tell(new NamespaceTopics(requestid, ns.Topics.ToList()));
                        _requests.TryRemove(requestid, out var u);
                        break;
                    case BaseCommand.Type.Message:
                        var msg = cmd.Message;
                        manager.Tell(new MessageReceived((long)msg.ConsumerId, new MessageIdReceived((long)msg.MessageId.ledgerId, (long)msg.MessageId.entryId, msg.MessageId.BatchIndex, msg.MessageId.Partition, msg.MessageId.AckSets), frame.Slice(commandSize + 4), (int)msg.RedeliveryCount));
                        break;
                    case BaseCommand.Type.Success:
                        var s = cmd.Success;
                        requests[(long)s.RequestId].Key
                            .Tell(new SubscribeSuccess(s?.Schema, (long)s.RequestId, s.Schema != null));
                        _requests.TryRemove((long)s.RequestId, out var rt);
                        break;
                    case BaseCommand.Type.SendReceipt:
                        try
                        {
                            var send = cmd.SendReceipt;
                            requests[(long)send.SequenceId].Key.Tell(new SentReceipt((long)send.ProducerId,
                                (long)send.SequenceId, (long)send.HighestSequenceId, (long)send.MessageId.entryId, (long)send.MessageId.ledgerId,
                                send.MessageId.BatchIndex, send.MessageId.Partition));
                            _requests.TryRemove((long)send.SequenceId, out var ou);
                        }
                        catch (Exception exception)
                        {
                            var send = cmd.SendReceipt;
                            parent.Tell(new SentReceipt((long)send.ProducerId,
                                (long)send.SequenceId, (long)send.HighestSequenceId, (long)send.MessageId.entryId, (long)send.MessageId.ledgerId,
                                send.MessageId.BatchIndex, send.MessageId.Partition));
                            log.Error(exception.ToString());
                        }
                        break;
                    case BaseCommand.Type.GetOrCreateSchemaResponse:
                        var res = cmd.getOrCreateSchemaResponse;
                        requests[(long)res.RequestId].Key
                            .Tell(new GetOrCreateSchemaServerResponse((long)res.RequestId, res.ErrorMessage,
                                res.ErrorCode, res.SchemaVersion));
                        _requests.TryRemove((long)res.RequestId, out var g);
                        break;
                    case BaseCommand.Type.ProducerSuccess:
                        var p = cmd.ProducerSuccess;
                        requests[(long)p.RequestId].Key.Tell(new ProducerCreated(p.ProducerName,
                            (long)p.RequestId, p.LastSequenceId, p.SchemaVersion));
                        _requests.TryRemove((long)p.RequestId, out var pr);
                        break;
                    case BaseCommand.Type.Error:
                        var er = cmd.Error;
                        requests[(long)er.RequestId].Key.Tell(new PulsarError(er.Message, er.Error.ToString()));
                        _requests.TryRemove((long)er.RequestId, out var err);
                        break;
                    case BaseCommand.Type.GetSchemaResponse:
                        var schema = cmd.getSchemaResponse.Schema;
                        var a = requests[(long)cmd.getSchemaResponse.RequestId].Key;
                        if (schema == null)
                            a.Tell(new NullSchema());
                        else
                            a.Tell(new SchemaResponse(schema.SchemaData, schema.Name,
                                schema.Properties.ToImmutableDictionary(x => x.Key, x => x.Value), schema.type,
                                (long)cmd.getSchemaResponse.RequestId));
                        _requests.TryRemove((long)cmd.getSchemaResponse.RequestId, out var sch);
                        break;
                    case BaseCommand.Type.LookupResponse:
                        var m = cmd.lookupTopicResponse;
                        requests[(long)m.RequestId].Key.Tell(new BrokerLookUp(m.Message, m.Authoritative,
                            m.Response, m.brokerServiceUrl, m.brokerServiceUrlTls, (long)m.RequestId, m.ProxyThroughServiceUrl));
                        _requests.TryRemove((long)m.RequestId, out var lk);
                        break;
                    case BaseCommand.Type.PartitionedMetadataResponse:
                        var part = cmd.partitionMetadataResponse;
                        var rPay = requests[(long)part.RequestId];
                        rPay.Key.Tell(
                            new Partitions((int)part.Partitions, (long)part.RequestId, rPay.Value.Topic));
                        _requests.TryRemove((long)part.RequestId, out var pa);
                        break;
                    case BaseCommand.Type.SendError:
                        var e = cmd.SendError;
                        break;
                    case BaseCommand.Type.Ping:
                        HandlePing(cmd.Ping);
                        break;
                    case BaseCommand.Type.CloseProducer:
                        parent.Tell(new ProducerClosed((long)cmd.CloseProducer.ProducerId));
                        break;
                    case BaseCommand.Type.CloseConsumer:
                        parent.Tell(new ConsumerClosed((long)cmd.CloseConsumer.ConsumerId));
                        break;
                    default:
                        log.Info($"Received '{cmd.type}' Message in '{_self.Path}'");
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }
        
        public string RemoteHostName { get; set; }


        public IStash Stash { get; set; }
        private bool ValidateServerCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
            {
                var certServerName = cert.Subject.Substring(cert.Subject.IndexOf('=') + 1);

                // Verify that target server name matches subject in the certificate
                if (RemoteHostName.Length > certServerName.Length)
                {
                    return false;
                }
                if (RemoteHostName.Length == certServerName.Length)
                {
                    // Both strings have the same Length, so targetServerName must be a FQDN
                    if (!RemoteHostName.Equals(certServerName, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                else
                {
                    if (string.Compare(RemoteHostName, 0, certServerName, 0, RemoteHostName.Length, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        return false;
                    }

                    // Server name matches cert name for its whole Length, so ensure that the
                    // character following the server name is a '.'. This will avoid
                    // having server name "ab" match "abc.corp.company.com"
                    // (Names have different lengths, so the target server can't be a FQDN.)
                    if (certServerName[RemoteHostName.Length] != '.')
                    {
                        return false;
                    }
                }
            }
            else
            {
                // Fail all other SslPolicy cases besides RemoteCertificateNameMismatch
                return false;
            }
            return true;
        }

    }

    public sealed class KeepAlive
    {
        public static KeepAlive Instance = new KeepAlive();
        public readonly byte[] Cmd = Commands.NewPing();
    }
    
}
