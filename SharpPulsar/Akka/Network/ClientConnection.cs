using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
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
        private PulsarStream _stream;
        private readonly IActorRef _self;
        private readonly IActorRef _parent;
        private readonly ActorSystem _system;
        private readonly ReadOnlySequence<byte> _pong = new ReadOnlySequence<byte>(Commands.NewPong());
        internal Uri RemoteAddress;
        internal int RemoteEndpointProtocolVersion = Enum.GetValues(typeof(ProtocolVersion)).Cast<int>().Max();
        public IActorRef Connection;
        private readonly ConcurrentDictionary<long, KeyValuePair<IActorRef, Payload>> _requests = new ConcurrentDictionary<long, KeyValuePair<IActorRef, Payload>>();

        private readonly string _proxyToTargetBrokerAddress;

        private readonly ILoggingAdapter _log;
        private readonly ClientConfigurationData _conf;
        private readonly IActorRef _manager;
        // Added for mutual authentication.
        private IAuthenticationDataProvider _authenticationDataProvider;

        public ClientConnection(Uri endPoint, ClientConfigurationData conf, IActorRef manager, string targetBroker = "")
        {
            _proxyToTargetBrokerAddress = targetBroker;
            _system = Context.System;
            _self = Self;
            RemoteHostName = endPoint.Host;
            _conf = conf;
            _manager = manager;
            Connection = Self;
            RemoteAddress = endPoint;
            _log = Context.System.Log;
            if (conf.MaxLookupRequest < conf.ConcurrentLookupRequest)
                throw new Exception("ConcurrentLookupRequest must be less than MaxLookupRequest");
            _authentication = conf.Authentication;

            _parent = Context.Parent;
           Connect();
           Receive<Payload>(p =>
           {
               _ = _requests.TryAdd(p.RequestId, new KeyValuePair<IActorRef, Payload>(Sender, p));
               Send(new ReadOnlySequence<byte>(p.Bytes));
           });
            Receive<ConnectionCommand>(p =>
           {
               Send(new ReadOnlySequence<byte>(p.Command));
           });
            _ = ProcessIncommingFrames();
        }

        private void Connect()
        {
            try
            {
                var connector = new Connector(_conf);
                var connect = connector.Connect(RemoteAddress, RemoteHostName);
                Context.System.Log.Info($"Opening Connection to: {RemoteAddress}");
                _stream = new PulsarStream(connect);
                Send(new ReadOnlySequence<byte>(NewConnectCommand()));
                Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromSeconds(10), Self, new ConnectionCommand(Commands.NewPing()), ActorRefs.NoSender);

            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                Thread.Sleep(5000);
                Connect();
            }
        }
        
        private void Send(ReadOnlySequence<byte> cmd)
        {
            try
            {
                _ = _stream.Send(cmd);
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
            try
            {
                _stream.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
                
            }
        }

        protected override void Unhandled(object message)
        {
            Console.WriteLine($"C -Unhandled {message.GetType().Name} in {Self.Path}");
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
                AuthData authData = _authenticationDataProvider.Authenticate(new AuthData(authChallenge.Challenge.auth_data));
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
        private sealed class ConnectionCommand
        {
            public ConnectionCommand(byte[] command)
            {
                Command = command;
            }

            public byte[] Command { get; }
        }
		public void HandlePing(CommandPing ping)
		{
			// Immediately reply success to ping requests
			if (_log.IsEnabled(LogLevel.DebugLevel))
			{
				//Log.Debug($"[{RemoteAddress}] Replying back to ping message");
			}
            _ = _stream.Send(_pong);
		}
        
        public async Task ProcessIncommingFrames()
        {
            await Task.Yield();
            try
            {
                
                await foreach (var frame in _stream.Frames())
                {
                    try
                    {

                        var commandSize = frame.ReadUInt32(0, true);
                        var cmd = Serializer.Deserialize(frame.Slice(4, commandSize));
                        var t = cmd.type;

                        
                        switch (cmd.type)
                        {
                            case BaseCommand.Type.AuthChallenge:
                                var auth = cmd.authChallenge;
                                var authen = HandleAuthChallenge(auth);
                                Send(new ReadOnlySequence<byte>(authen));
                                break;
                            case BaseCommand.Type.GetLastMessageIdResponse:
                                var mid = cmd.getLastMessageIdResponse.LastMessageId;
                                var rquestid = (long)cmd.getLastMessageIdResponse.RequestId;
                                _requests[rquestid].Key.Tell(new LastMessageIdResponse((long)mid.ledgerId,
                                    (long)mid.entryId, mid.Partition, mid.BatchIndex));
                                _requests.TryRemove(rquestid, out var ut);
                                break;
                            case BaseCommand.Type.Connected:
                                var c = cmd.Connected;
                                _parent.Tell(
                                    new ConnectedServerInfo(c.MaxMessageSize, c.ProtocolVersion, c.ServerVersion,
                                        RemoteHostName), _self);
                                _log.Info($"Now connected: Host = {RemoteHostName}, ProtocolVersion = {c.ProtocolVersion}");
                                break;
                            case BaseCommand.Type.GetTopicsOfNamespaceResponse:
                                var ns = cmd.getTopicsOfNamespaceResponse;
                                var requestid = (long)ns.RequestId;
                                _requests[requestid].Key.Tell(new NamespaceTopics(requestid, ns.Topics.ToList()));
                                _requests.TryRemove(requestid, out var u);
                                break;
                            case BaseCommand.Type.Message:
                                var msg = cmd.Message;
                                _manager.Tell(new MessageReceived((long)msg.ConsumerId, new MessageIdReceived((long)msg.MessageId.ledgerId, (long)msg.MessageId.entryId, msg.MessageId.BatchIndex, msg.MessageId.Partition, msg.MessageId.AckSets), frame.Slice(commandSize + 4), (int)msg.RedeliveryCount));
                                break;
                            case BaseCommand.Type.Success:
                                var s = cmd.Success;
                                _requests[(long)s.RequestId].Key
                                    .Tell(new SubscribeSuccess(s?.Schema, (long)s.RequestId, s.Schema != null));
                                _requests.TryRemove((long)s.RequestId, out var rt);
                                break;
                            case BaseCommand.Type.SendReceipt:
                                try
                                {
                                    var send = cmd.SendReceipt;
                                    _requests[(long)send.SequenceId].Key.Tell(new SentReceipt((long)send.ProducerId,
                                        (long)send.SequenceId, (long)send.HighestSequenceId, (long)send.MessageId.entryId, (long)send.MessageId.ledgerId,
                                        send.MessageId.BatchIndex, send.MessageId.Partition));
                                    _requests.TryRemove((long)send.SequenceId, out var ou);
                                }
                                catch (Exception exception)
                                {
                                    var send = cmd.SendReceipt;
                                    _parent.Tell(new SentReceipt((long)send.ProducerId,
                                        (long)send.SequenceId, (long)send.HighestSequenceId, (long)send.MessageId.entryId, (long)send.MessageId.ledgerId,
                                        send.MessageId.BatchIndex, send.MessageId.Partition));
                                    _log.Error(exception.ToString());
                                }
                                break;
                            case BaseCommand.Type.GetOrCreateSchemaResponse:
                                var res = cmd.getOrCreateSchemaResponse;
                                _requests[(long)res.RequestId].Key
                                    .Tell(new GetOrCreateSchemaServerResponse((long)res.RequestId, res.ErrorMessage,
                                        res.ErrorCode, res.SchemaVersion));
                                _requests.TryRemove((long)res.RequestId, out var g);
                                break;
                            case BaseCommand.Type.ProducerSuccess:
                                var p = cmd.ProducerSuccess;
                                _requests[(long)p.RequestId].Key.Tell(new ProducerCreated(p.ProducerName,
                                    (long)p.RequestId, p.LastSequenceId, p.SchemaVersion));
                                _requests.TryRemove((long)p.RequestId, out var pr);
                                break;
                            case BaseCommand.Type.Error:
                                var er = cmd.Error;
                                _requests[(long)er.RequestId].Key.Tell(new PulsarError(er.Message, er.Error.ToString()));
                                _requests.TryRemove((long)er.RequestId, out var err);
                                break;
                            case BaseCommand.Type.GetSchemaResponse:
                                var schema = cmd.getSchemaResponse.Schema;
                                var a = _requests[(long)cmd.getSchemaResponse.RequestId].Key;
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
                                _requests[(long)m.RequestId].Key.Tell(new BrokerLookUp(m.Message, m.Authoritative,
                                    m.Response, m.brokerServiceUrl, m.brokerServiceUrlTls, (long)m.RequestId, m.ProxyThroughServiceUrl));
                                _requests.TryRemove((long)m.RequestId, out var lk);
                                break;
                            case BaseCommand.Type.PartitionedMetadataResponse:
                                var part = cmd.partitionMetadataResponse;
                                var rPay = _requests[(long)part.RequestId];
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
                                _parent.Tell(new ProducerClosed((long)cmd.CloseProducer.ProducerId));
                                break;
                            case BaseCommand.Type.CloseConsumer:
                                _parent.Tell(new ConsumerClosed((long)cmd.CloseConsumer.ConsumerId));
                                break;
                            default:
                                _log.Info($"Received '{cmd.type}' Message in '{_self.Path}'");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }
        
        public string RemoteHostName { get; set; }


        public IStash Stash { get; set; }
    }
    public sealed class OpenConnection
    {

    }

}
