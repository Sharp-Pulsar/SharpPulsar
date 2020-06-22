using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Network
{
    public class NetworkManager: ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _manager;
        private readonly ClientConfigurationData _configuration; 
        private readonly PulsarServiceNameResolver _serviceNameResolver = new PulsarServiceNameResolver();
        private readonly Dictionary<string, IActorRef> _hosts = new Dictionary<string, IActorRef>();
        private readonly Random _randomHost = new Random();
		public NetworkManager(IActorRef manager, ClientConfigurationData configuration)
        {
            _serviceNameResolver.UpdateServiceUrl(configuration.ServiceUrl);
            _manager = manager;
            _configuration = configuration;
            
            ReceiveAny(_=> Stash.Stash());
            BecomeCreateConnections();
        }

        private void Stop()
        {
            ReceiveAny(m =>
            {
                Stash.Stash();
            });
           StopAndRestartConnections();
        }
        public void Ready()
        {
            //Context.Parent.Tell(new ServiceReady());
            Receive<UpdateService>(u =>
            {
                _serviceNameResolver.UpdateServiceUrl(u.Service);
                Become(Stop);
            });
            Receive<Payload>(x =>
            {
                var u = _randomHost.Next(0, _hosts.Count);
                var host = _hosts.ToList()[u].Value;
                host.Forward(x);
            });
            Receive<ConnectedServerInfo>(x =>
            {
                Context.Parent.Forward(x);
                if(!_hosts.ContainsKey(x.Name))
                    _hosts.Add(x.Name, Sender);
            });
            try
            {
                Stash.UnstashAll();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void BecomeCreateConnections()
        {
            foreach (var s in _serviceNameResolver.AddressList())
            {
                
                var host = s.Host;
                var hn = s.DnsSafeHost;
                Context.ActorOf(HostManager.Prop(s, _configuration, _manager), Regex.Replace(host, @"[^\w\d]", ""));
            }
            Become(CreateConnections);
        }
        private void CreateConnections()
        {
            Receive<ConnectedServerInfo>(x =>
            {
                Context.Parent.Forward(x);
                if (!_hosts.ContainsKey(x.Name))
                    _hosts.Add(x.Name, Sender);
                Become(Ready);
            });
            ReceiveAny(x =>
            {
                Stash.Stash();
            });
        }

        private void StopAndRestartConnections()
        {
            foreach (var c in Context.GetChildren())
            {
                c.GracefulStop(TimeSpan.FromMilliseconds(1000));
            }
            CreateConnections();
        }
        public static Props Prop(IActorRef manager, ClientConfigurationData configuration)
        {
            return Props.Create(()=> new NetworkManager(manager, configuration));
        }

        public IStash Stash { get; set; }
    }
}
