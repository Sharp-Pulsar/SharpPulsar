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
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Configuration;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using AuthData = SharpPulsar.Auth.AuthData;

namespace SharpPulsar.Akka.Network
{
    //https://github.com/kubernetes-sigs/external-dns/blob/master/docs/tutorials/azure-private-dns.md#manifest-for-clusters-with-rbac-enabled-cluster-access
    //https://noobient.com/2018/04/10/free-wildcard-certificates-using-azure-dns-lets/
    //https://cert-manager.io/docs/configuration/acme/dns01/azuredns/
    //https://dev.to/mimetis/using-dns01-challenge-and-let-s-encrypt-to-secure-your-aks-kubernetes-cluster-5g42s
    //http://www.wahidsaleemi.com/2020/01/get-a-free-domain-for-you-azure-labs/
    //https://github.com/kubernetes-sigs/external-dns/blob/7505f29e4cec80ca20468b38c03b660a8481277d/docs/tutorials/azure.md
    //https://thorsten-hans.com/custom-domains-in-azure-kubernetes-with-nginx-ingress-azure-cli
    public class ClientConnection: ReceiveActor, IWithUnboundedStash
    {
        //private uint? _frameSize;
        //private AsyncTcpClient _client;
        private readonly IAuthentication _authentication;
        private PulsarStream _stream;
        private readonly IActorRef _self;
        private readonly IActorRef _parent;
        private readonly IActorContext _context;
        private readonly ReadOnlySequence<byte> _pong = new ReadOnlySequence<byte>(Commands.NewPong());
        internal Uri RemoteAddress;
        internal int RemoteEndpointProtocolVersion = Enum.GetValues(typeof(ProtocolVersion)).Cast<int>().Max();
        public IActorRef Connection;
        private readonly Queue<Payload> _pendingPayloads;
        private readonly ConcurrentDictionary<long, KeyValuePair<IActorRef, Payload>> _requests = new ConcurrentDictionary<long, KeyValuePair<IActorRef, Payload>>();

        private readonly string _proxyToTargetBrokerAddress;
        private readonly ILoggingAdapter _log;
        private readonly ClientConfigurationData _conf;
        private readonly IActorRef _manager;

        private readonly CancellationTokenSource _cancellationToken;
        // Added for mutual authentication.
        private IAuthenticationDataProvider _authenticationDataProvider;

        public ClientConnection(Uri endPoint, ClientConfigurationData conf, IActorRef manager, string targetBroker = "")
        {
            _cancellationToken = new CancellationTokenSource();
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
               Send(new ReadOnlySequence<byte>(p.Bytes));
           });
           _ = ProcessIncommingFrames();
        }

        private void Connect()
        {
            try
            {
                var connector = new Connector(_conf);
                var connect = connector.Connect(RemoteAddress, RemoteHostName);
                _context.System.Log.Info($"Opening Connection to: {RemoteAddress}");
                _stream = new PulsarStream(connect, _log);
                Send(new ReadOnlySequence<byte>(NewConnectCommand()));

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
                _cancellationToken.Cancel();
                _stream.DisposeAsync().ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                _log.Error(ex.ToString());
            }
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
            _ = _stream.Send(_pong);
        }
        
        private async Task ProcessIncommingFrames()
        {
            await Task.Yield();
            try
            {
                
                await foreach (var frame in _stream.Frames(_cancellationToken.Token))
                {
                    try
                    {

                        var commandSize = frame.ReadUInt32(0, true);
                        var cmd = Serializer.Deserialize(frame.Slice(4, commandSize));
                        var t = cmd.type;


                        switch (cmd.type)
                        {
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

}
