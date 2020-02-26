using System;
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

        private readonly int _protocolVersion;

        protected internal string ProxyToTargetBrokerAddress;
        private string _remoteHostName;

        private ILoggingAdapter Log;

        private IActorRef _outerActor;
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
        public ClientConnection(EndPoint endPoint, ClientConfigurationData conf, int protocolVersion, IActorRef outerActor)
        {
            _outerActor = outerActor;
            Connection = Self;
            RemoteAddress = endPoint;
            Log = Context.System.Log;
            if (conf.MaxLookupRequest < conf.ConcurrentLookupRequest)
                throw new Exception("ConcurrentLookupRequest must be less than MaxLookupRequest");
            Authentication = conf.Authentication;
            _state = State.None;
            _protocolVersion = protocolVersion;
            //this.keepAliveIntervalSeconds = BAMCIS.Util.Concurrent.TimeUnit.SECONDS.ToSecs(conf.KeepAliveIntervalSeconds);
            Context.System.Tcp().Tell(new Tcp.Connect(endPoint));
        }

        public static Props Prop(EndPoint endPoint, ClientConfigurationData conf, int protocolVersion, IActorRef outerActor)
        {
            return Props.Create(() => new ClientConnection(endPoint, conf, protocolVersion, outerActor));
        }
        protected override void OnReceive(object message)
        {
            switch (message)
            {
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

					case BaseCommand.Types.Type.Ping:
						if (cmd.HasPing)
							HandlePing(cmd.Ping);
						cmd.Ping.Recycle();
						break;

					case BaseCommand.Types.Type.Pong:
						if (cmd.HasPong)
							HandlePong(cmd.Pong);
						cmd.Pong.Recycle();
						break;
					default:
						_outerActor.Tell(new TcpReceived(buffer));
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
