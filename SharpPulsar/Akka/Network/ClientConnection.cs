using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Reflection;
using Akka.Actor;
using Akka.Event;
using Akka.IO;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using ByteString = Akka.IO.ByteString;
using Dns = System.Net.Dns;

namespace SharpPulsar.Akka.Network
{
    public class ClientConnection: UntypedActor, IWithUnboundedStash
    {
        protected internal readonly IAuthentication Authentication;
        private State _state;
        protected internal EndPoint RemoteAddress;
        protected internal int _remoteEndpointProtocolVersion = (int)ProtocolVersion.V0;
        private readonly long _keepAliveIntervalSeconds;
        public IActorRef Connection;
        private Dictionary<long, KeyValuePair<IActorRef, Payload>> _requests = new Dictionary<long, KeyValuePair<IActorRef, Payload>>();

        public  int MaxMessageSize = Commands.DefaultMaxMessageSize;

        protected internal string ProxyToTargetBrokerAddress;
        private string _remoteHostName;

        private ILoggingAdapter Log;
        private ClientConfigurationData _conf;
        private IActorRef _manager;

        private int _protocolVersion;
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
        public ClientConnection(EndPoint endPoint, ClientConfigurationData conf, IActorRef manager)
        {
            _keepAliveIntervalSeconds = conf.KeepAliveIntervalSeconds;
            _protocolVersion = conf.ProtocolVersion;
            _conf = conf;
            _manager = manager;
            Connection = Self;
            RemoteAddress = endPoint;
            Log = Context.System.Log;
            if (conf.MaxLookupRequest < conf.ConcurrentLookupRequest)
                throw new Exception("ConcurrentLookupRequest must be less than MaxLookupRequest");
            Authentication = conf.Authentication;
            _state = State.None;
            //this.keepAliveIntervalSeconds = BAMCIS.Util.Concurrent.TimeUnit.SECONDS.ToSecs(conf.KeepAliveIntervalSeconds);
            Context.System.Tcp().Tell(new Tcp.Connect(endPoint));
        }
        
        public static Props Prop(EndPoint endPoint, ClientConfigurationData conf, IActorRef manager)
        {
            return Props.Create(() => new ClientConnection(endPoint, conf, manager));
        }
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case TcpReconnect _:
                    Context.System.Tcp().Tell(new Tcp.Connect(RemoteAddress));
                    break;
                case Tcp.Connected connected:
                    Log.Info("Connected to {0}", connected.RemoteAddress.ToString());
                    // Register self as connection handler
                    Sender.Tell(new Tcp.Register(Self));
                    Connection = Sender;
                    HandleTcpConnected();
                    Become(Connected());
                    break;
                case Tcp.Closed closed:
                case Tcp.Aborted a:
                case Tcp.ConnectionClosed cl:
                case Tcp.CommandFailed c:
                    Console.WriteLine("Connection lost");
					Context.Parent.Tell(new TcpFailed(Self.Path.Name));
                    Become(Connecting());
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        private UntypedReceive Connecting()
        {
            return message =>
            {
                if (message is TcpReconnect)
                {
                    Context.System.Tcp().Tell(new Tcp.Connect(RemoteAddress));
                }
                else if (message is Tcp.Connected connected)
                {
                    Log.Info("Connected to {0}", connected.RemoteAddress.ToString());
                    // Register self as connection handler
                    Sender.Tell(new Tcp.Register(Self));
                    HandleTcpConnected();
                    Connection = Sender;
                    Become(Connected());
                    Stash.UnstashAll();
                }
                else
                {
                    Stash.Stash();
                }
            };
        }
        private UntypedReceive Connected()
        {
            return message =>
            {
                if (message is Tcp.Received received)  // data received from pulsar server
                {
                    var b = received.Data.ToArray();
                    HandleTcpReceived(received.Data.ToArray());
                }
                else if (message is Payload p)   // data received from producer/consumer
                {
                    var b = p.Bytes;
                    _requests.Add(p.RequestId, new KeyValuePair<IActorRef, Payload>(Sender, p));
                    Connection.Tell(Tcp.Write.Create(ByteString.FromBytes(p.Bytes)));
                }
                else if (message is ConnectionCommand b)   // data received from self
                {
                    Connection.Tell(Tcp.Write.Create(ByteString.FromBytes(b.Command)));
                }
                else if (message is Tcp.PeerClosed)
                {
                    Log.Info("Connection closed");
                    _manager.Tell(new TcpClosed());
                }
                else Unhandled(message);
            };
        }

        protected override void Unhandled(object message)
        {
            Console.WriteLine($"Unhandled {message.GetType()} in {Self.Path}");
        }

        public int RemoteEndpointProtocolVersion
        {
            set => _remoteEndpointProtocolVersion = value;
            get => _remoteEndpointProtocolVersion;
        }
		public void HandleTcpReceived(byte[] buffer)
		{
            try
			{
                // De-serialize the command
                var cmd = Stole.Serializer.Deserialize(buffer);
                if (Log.IsEnabled(LogLevel.DebugLevel))
				{
					Log.Debug($"{RemoteAddress} Received cmd {cmd.type.ToString()}");
				}
                
				switch (cmd.type)
				{
                    case BaseCommand.Type.Connected:
                        var c = cmd.Connected;
                        Console.WriteLine($"Now connected: ServerVersion = {c.ServerVersion}, ProtocolVersion = {c.ProtocolVersion}");
                        break;
                    case BaseCommand.Type.GetTopicsOfNamespaceResponse:
                        var ns = cmd.getTopicsOfNamespaceResponse;
                        var requestid = (long) ns.RequestId;
                        _requests[requestid].Key.Tell(new NamespaceTopics(requestid, ns.Topics.ToList()));
                        _requests.Remove(requestid);
                        break;
                    case BaseCommand.Type.Message:
                        var msg = cmd.Message;
                        _manager.Tell(new MessageReceived((long)msg.ConsumerId, new MessageIdReceived((long)msg.MessageId.ledgerId, (long)msg.MessageId.entryId, msg.MessageId.BatchIndex, msg.MessageId.Partition), buffer, (int)msg.RedeliveryCount));
                        break;
                    case BaseCommand.Type.Success:
                        var s = cmd.Success;
                        _manager.Tell(new SubscribeSuccess(s?.Schema, (long)s.RequestId, s.Schema != null));
                        _requests.Remove((long)s.RequestId);
                        break;
                    case BaseCommand.Type.SendReceipt:
                        var send = cmd.SendReceipt;
                        _manager.Tell(new SentReceipt((long)send.ProducerId, (long)send.SequenceId, (long)send.MessageId.entryId, (long)send.MessageId.ledgerId, send.MessageId.BatchIndex, send.MessageId.Partition));
                        break;
                    case BaseCommand.Type.GetOrCreateSchemaResponse:
                        var res = cmd.getOrCreateSchemaResponse;
                        _manager.Tell(new GetOrCreateSchemaServerResponse((long)res.RequestId, res.ErrorMessage, res.ErrorCode, res.SchemaVersion));
                        _requests.Remove((long)res.RequestId);
                        break;
                    case BaseCommand.Type.ProducerSuccess:
                        var p = cmd.ProducerSuccess;
                        _manager.Tell(new ProducerCreated(p.ProducerName, (long)p.RequestId, p.LastSequenceId, p.SchemaVersion));
                        _requests.Remove((long)p.RequestId);
                        break;
                    case BaseCommand.Type.GetSchemaResponse:
                        var schema = cmd.getSchemaResponse.Schema;
                        _manager.Tell(new SchemaResponse(schema.SchemaData, schema.Name, schema.Properties.ToImmutableDictionary(x=> x.Key, x=> x.Value), schema.type, (long)cmd.getSchemaResponse.RequestId));
                        _requests.Remove((long)cmd.getSchemaResponse.RequestId);
                        break;
                    case BaseCommand.Type.LookupResponse:
                        var m = cmd.lookupTopicResponse;
                        _manager.Tell(new BrokerLookUp(m.Message, m.Authoritative, m.Response, m.brokerServiceUrl, m.brokerServiceUrlTls, (long)m.RequestId));
                        _requests.Remove((long)m.RequestId);
                        break;
                    case BaseCommand.Type.PartitionedMetadataResponse:
                        var part = cmd.partitionMetadataResponse;
                        var rPay = _requests[(long)part.RequestId];
                        rPay.Key.Tell(new Partitions((int)part.Partitions, (long)part.RequestId, rPay.Value.Topic));
                        _requests.Remove((long)part.RequestId);
                        break;
                    case BaseCommand.Type.Ping:
							HandlePing(cmd.Ping);
						break;
                    case BaseCommand.Type.Connect:
                        Context.Parent.Tell(new TcpSuccess(RemoteHostName));
                        break;
					case BaseCommand.Type.Pong:
							HandlePong(cmd.Pong);
						break;
					default:
						Console.WriteLine($"{cmd.type.GetType()} Received");
                        break;
                }
			}
			catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
		}
       
		private void HandlePong(CommandPong cmdPong)
		{
			
		}
        
		public void HandleTcpConnected()
		{
			RemoteHostName = Dns.GetHostEntry(((IPEndPoint)RemoteAddress).Address).HostName;
			if (Log.IsEnabled(LogLevel.DebugLevel))
			{
				Log.Debug("[{}] Scheduling keep-alive task every {} s", RemoteAddress, _keepAliveIntervalSeconds);
			}
			
			if (ReferenceEquals(ProxyToTargetBrokerAddress, null))
			{
				if (Log.IsEnabled(LogLevel.DebugLevel))
				{
					Log.Debug("{} Connected to broker", RemoteAddress);
				}
			}
			else
			{
				Log.Info("{} Connected through proxy to target broker at {}", RemoteAddress, ProxyToTargetBrokerAddress);
			}
			// Send CONNECT command
            var c = new ConnectionCommand(NewConnectCommand());
            Self.Tell(c);
            _state = State.SentConnectFrame;
			Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(30), Self, new ConnectionCommand(Commands.NewPing()) , ActorRefs.NoSender);

        }
		public byte[] NewConnectCommand()
		{
			// mutual authentication is to auth between `remoteHostName` and this client for this channel.
			// each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
			// and return authData to server.
			AuthenticationDataProvider = Authentication.GetAuthData(_remoteHostName);
			var authData = AuthenticationDataProvider.Authenticate(new Shared.Auth.AuthDataShared(Shared.Auth.AuthDataShared.InitAuthData));
            var assemblyName = Assembly.GetCallingAssembly().GetName();
            var auth = AuthData.NewBuilder().SetAuthData((byte[])(object)authData.Bytes).Build();
            var clientVersion = assemblyName.Name + " " + assemblyName.Version.ToString(3);

            return Commands.NewConnect(Authentication.AuthMethodName, auth, 14, clientVersion, ProxyToTargetBrokerAddress, string.Empty, null, string.Empty);
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
			if (Log.IsEnabled(LogLevel.DebugLevel))
			{
				Log.Debug("[{}] Replying back to ping message", RemoteAddress);
			}
			Self.Tell(new ConnectionCommand(Commands.NewPong()));
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

        public IStash Stash { get; set; }
    }
}
