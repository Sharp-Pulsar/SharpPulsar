using System;
using System.Collections.Immutable;
using System.Net;
using Akka.Actor;
using Akka.Event;
using Akka.IO;
using DotNetty.Buffers;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using ByteString = Akka.IO.ByteString;
using Dns = System.Net.Dns;

namespace SharpPulsar.Akka.Network
{
    public class ClientConnection: UntypedActor
    {
        protected internal readonly IAuthentication Authentication;
        private State _state;
        protected internal EndPoint RemoteAddress;
        protected internal int _remoteEndpointProtocolVersion = (int)ProtocolVersion.V0;
        private readonly long _keepAliveIntervalSeconds;
        public IActorRef Connection;

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
                    Log.Info("Connected to {0}", connected.RemoteAddress);
                    // Register self as connection handler
                    Sender.Tell(new Tcp.Register(Self));
                    HandleTcpConnected();
                    Become(Connected(Sender));
                    break;
                case Tcp.CommandFailed c:
                    Console.WriteLine("Connection failed");//reschule
					Context.Parent.Tell(new TcpFailed(Self.Path.Name));
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        private UntypedReceive Connected(IActorRef connection)
        {
            return message =>
            {
                if (message is Tcp.Received received)  // data received from pulsar server
                {
					HandleTcpReceived(received.Data.ToArray());
                }
                else if (message is Payload p)   // data received from producer/consumer
                {
                    connection.Tell(Tcp.Write.Create(ByteString.FromBytes(p.Bytes)));
                }
                else if (message is IByteBuffer b)   // data received from self
                {
                    connection.Tell(Tcp.Write.Create(ByteString.FromBytes(b.Array)));
                }
                else if (message is Tcp.PeerClosed)
                {
                    Log.Info("Connection closed");
                    _manager.Tell(new TcpClosed());
                }
                else Unhandled(message);
            };
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
                var cmd = BaseCommand.Parser.ParseFrom(buffer);

				if (Log.IsEnabled(LogLevel.DebugLevel))
				{
					Log.Debug("[{}] Received cmd {}", RemoteAddress, cmd.Type);
				}

				switch (cmd.Type)
				{
                    case BaseCommand.Types.Type.GetOrCreateSchemaResponse:
                        var res = cmd.GetOrCreateSchemaResponse;
                        _manager.Tell(new GetOrCreateSchemaServerResponse((long)res.RequestId, res.ErrorMessage, res.ErrorCode, res.SchemaVersion.ToByteArray()));
                        break;
                    case BaseCommand.Types.Type.ProducerSuccess:
                        var p = cmd.ProducerSuccess;
                        _manager.Tell(new ProducerCreated(p.ProducerName, (long)p.RequestId, p.LastSequenceId, p.SchemaVersion.ToByteArray()));
                        break;
                    case BaseCommand.Types.Type.GetSchemaResponse:
                        var schema = cmd.GetSchemaResponse.Schema;
                        _manager.Tell(new SchemaResponse(schema.SchemaData.ToByteArray(), schema.Name, schema.Properties.ToImmutableDictionary(x=> x.Key, x=> x.Value), schema.Type, (long)cmd.GetSchemaResponse.RequestId));
                        break;
                    case BaseCommand.Types.Type.LookupResponse:
                        var m = cmd.LookupTopicResponse;
                        _manager.Tell(new BrokerLookUp(m.Message, m.Authoritative, m.Response, m.BrokerServiceUrl, m.BrokerServiceUrlTls, (long)m.RequestId));
                        break;
                    case BaseCommand.Types.Type.PartitionedMetadataResponse:
                        _manager.Tell(new Partitions((int)cmd.PartitionMetadataResponse.Partitions, (long)cmd.PartitionMetadataResponse.RequestId));
                        break;
                    case BaseCommand.Types.Type.Ping:
						if (cmd.HasPing)
							HandlePing(cmd.Ping);
						break;
                    case BaseCommand.Types.Type.Connect:
                        _manager.Tell(new TcpSuccess(RemoteHostName));
                        break;
					case BaseCommand.Types.Type.Pong:
						if (cmd.HasPong)
							HandlePong(cmd.Pong);
						break;
					default:
						_manager.Tell(new TcpReceived(buffer));
                        break;
                }
			}
			finally
			{
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
            Self.Tell(NewConnectCommand());
            _state = State.SentConnectFrame;
			Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(_keepAliveIntervalSeconds), Self, Commands.NewPing(), ActorRefs.NoSender);

        }
		public IByteBuffer NewConnectCommand()
		{
			// mutual authentication is to auth between `remoteHostName` and this client for this channel.
			// each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
			// and return authData to server.
			AuthenticationDataProvider = Authentication.GetAuthData(_remoteHostName);
			var authData = AuthenticationDataProvider.Authenticate(new Shared.Auth.AuthData(Shared.Auth.AuthData.InitAuthData));

			var auth = AuthData.NewBuilder().SetAuthData(Google.Protobuf.ByteString.CopyFrom((byte[])(object)authData.Bytes)).Build();
			return Commands.NewConnect(Authentication.AuthMethodName, auth, _protocolVersion, null, ProxyToTargetBrokerAddress, string.Empty, null, string.Empty);
		}

		public void HandlePing(CommandPing ping)
		{
			// Immediately reply success to ping requests
			if (Log.IsEnabled(LogLevel.DebugLevel))
			{
				Log.Debug("[{}] Replying back to ping message", RemoteAddress);
			}
			Self.Tell(Commands.NewPong());
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
    }
}
