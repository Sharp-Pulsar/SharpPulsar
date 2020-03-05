using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
    public class ClientConnection: ReceiveActor, IWithUnboundedStash
    {
        protected internal readonly IAuthentication Authentication;
        private PulsarStream _stream;
        private State _state;
        protected internal EndPoint RemoteAddress;
        protected internal int _remoteEndpointProtocolVersion = (int)ProtocolVersion.V15;
        private readonly long _keepAliveIntervalSeconds;
        public IActorRef Connection;
        private Dictionary<long, KeyValuePair<IActorRef, Payload>> _requests = new Dictionary<long, KeyValuePair<IActorRef, Payload>>();

        public  int MaxMessageSize = Commands.DefaultMaxMessageSize;

        protected internal string ProxyToTargetBrokerAddress;
        private string _remoteHostName;

        private ILoggingAdapter Log;
        private ClientConfigurationData _conf;
        private IActorRef _manager;
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
            //Context.System.Tcp().Tell(new Tcp.Connect(endPoint));
            var connector = new Connector(conf);
            _stream = new PulsarStream(connector.Connect((IPEndPoint)endPoint));
            Context.Parent.Tell(new TcpSuccess(conf.ServiceUrl));
            Receive<Payload>(p =>
            {
                _requests.Add(p.RequestId, new KeyValuePair<IActorRef, Payload>(Sender, p));
                _ = _stream.Send(new ReadOnlySequence<byte>(p.Bytes));
            });
            Receive<ConnectionCommand>(p =>
            {
                _ = _stream.Send(new ReadOnlySequence<byte>(p.Command));
            });
            var c = new ConnectionCommand(NewConnectCommand());
            _ =_stream.Send(new ReadOnlySequence<byte>(c.Command));
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(30), Self, new ConnectionCommand(Commands.NewPing()), ActorRefs.NoSender);

        }

        public static Props Prop(EndPoint endPoint, ClientConfigurationData conf, IActorRef manager)
        {
            return Props.Create(() => new ClientConnection(endPoint, conf, manager));
        }

        protected override void PreStart()
        {
            _ = ProcessIncommingFrames();
        }

        protected override void PostStop()
        {
            _stream.DisposeAsync().ConfigureAwait(false);
        }

        protected override void Unhandled(object message)
        {
            Console.WriteLine($"Unhandled {message.GetType()} in {Self.Path}");
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
			AuthenticationDataProvider = Authentication.GetAuthData(_conf.ServiceUrl);
			var authData = AuthenticationDataProvider.Authenticate(new Shared.Auth.AuthDataShared(Shared.Auth.AuthDataShared.InitAuthData));
            var assemblyName = Assembly.GetCallingAssembly().GetName();
            var auth = new AuthData {auth_data = ((byte[]) (object) authData.Bytes)};
            var clientVersion = assemblyName.Name + " " + assemblyName.Version.ToString(3);

            return Commands.NewConnect(Authentication.AuthMethodName, auth, 15, clientVersion, ProxyToTargetBrokerAddress, string.Empty, null, string.Empty);
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
        public async Task ProcessIncommingFrames()
        {
            await Task.Yield();

            try
            {
                await foreach (var frame in _stream.Frames())
                {
                    var commandSize = frame.ReadUInt32(0, true);
                    var cmd = Serializer.Deserialize(frame.Slice(4, commandSize));

                    switch (cmd.type)
                    {
                        case BaseCommand.Type.Connected:
                            var c = cmd.Connected;
                            Console.WriteLine($"Now connected: ServerVersion = {c.ServerVersion}, ProtocolVersion = {c.ProtocolVersion}");
                            break;
                        case BaseCommand.Type.GetTopicsOfNamespaceResponse:
                            var ns = cmd.getTopicsOfNamespaceResponse;
                            var requestid = (long)ns.RequestId;
                            _requests[requestid].Key.Tell(new NamespaceTopics(requestid, ns.Topics.ToList()));
                            _requests.Remove(requestid);
                            break;
                        case BaseCommand.Type.Message:
                            var msg = cmd.Message;
                            //_manager.Tell(new MessageReceived((long)msg.ConsumerId, new MessageIdReceived((long)msg.MessageId.ledgerId, (long)msg.MessageId.entryId, msg.MessageId.BatchIndex, msg.MessageId.Partition), cmd.Message, (int)msg.RedeliveryCount));
                            break;
                        case BaseCommand.Type.Success:
                            var s = cmd.Success;
                            _requests[(long)s.RequestId].Key.Tell(new SubscribeSuccess(s?.Schema, (long)s.RequestId, s.Schema != null));
                            _requests.Remove((long)s.RequestId);
                            break;
                        case BaseCommand.Type.SendReceipt:
                            var send = cmd.SendReceipt;
                            _requests[(long)send.SequenceId].Key.Tell(new SentReceipt((long)send.ProducerId, (long)send.SequenceId, (long)send.MessageId.entryId, (long)send.MessageId.ledgerId, send.MessageId.BatchIndex, send.MessageId.Partition));
                            _requests.Remove((long)send.SequenceId);
                            break;
                        case BaseCommand.Type.GetOrCreateSchemaResponse:
                            var res = cmd.getOrCreateSchemaResponse;
                            _manager.Tell(new GetOrCreateSchemaServerResponse((long)res.RequestId, res.ErrorMessage, res.ErrorCode, res.SchemaVersion));
                            _requests.Remove((long)res.RequestId);
                            break;
                        case BaseCommand.Type.ProducerSuccess:
                            var p = cmd.ProducerSuccess;
                            _requests[(long)p.RequestId].Key.Tell(new ProducerCreated(p.ProducerName, (long)p.RequestId, p.LastSequenceId, p.SchemaVersion));
                            _requests.Remove((long)p.RequestId);
                            break;
                        case BaseCommand.Type.GetSchemaResponse:
                            var schema = cmd.getSchemaResponse.Schema;
                            _requests[(long)cmd.getSchemaResponse.RequestId].Key.Tell(new SchemaResponse(schema.SchemaData, schema.Name, schema.Properties.ToImmutableDictionary(x => x.Key, x => x.Value), schema.type, (long)cmd.getSchemaResponse.RequestId));
                            _requests.Remove((long)cmd.getSchemaResponse.RequestId);
                            break;
                        case BaseCommand.Type.LookupResponse:
                            var m = cmd.lookupTopicResponse;
                            _requests[(long)m.RequestId].Key.Tell(new BrokerLookUp(m.Message, m.Authoritative, m.Response, m.brokerServiceUrl, m.brokerServiceUrlTls, (long)m.RequestId));
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
                            //Context.Parent.Tell(new TcpSuccess(RemoteHostName));
                            break;
                        case BaseCommand.Type.Pong:
                            HandlePong(cmd.Pong);
                            break;
                        default:
                            Console.WriteLine($"{cmd.type.GetType()} Received");
                            break;
                    }
                }
            }
            catch { }
        }
        
        public virtual string RemoteHostName
		{
			get => _remoteHostName;
			set => _remoteHostName = value;
		}

        public IStash Stash { get; set; }
    }
}
