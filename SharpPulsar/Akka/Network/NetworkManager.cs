using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Akka.Actor;
using DotNetty.Transport.Bootstrapping;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Network
{
    public class NetworkManager: ReceiveActor, IWithUnboundedStash
    {
        private IActorRef _manager;
        private ClientConfigurationData _configuration; 
        private PulsarServiceNameResolver _serviceNameResolver = new PulsarServiceNameResolver();
        private Dictionary<string, IActorRef> _hosts = new Dictionary<string, IActorRef>();
        private Random _randomHost = new Random();
		public NetworkManager(IActorRef manager, ClientConfigurationData configuration)
        {
            _serviceNameResolver.UpdateServiceUrl(configuration.ServiceUrl);
            _manager = manager;
            _configuration = configuration;
            var dnsResolver = new DefaultNameResolver();
            foreach (var s in _serviceNameResolver.AddressList())
            {
                var service = s;
                if (!dnsResolver.IsResolved(s))
                    service = (IPEndPoint)dnsResolver.ResolveAsync(s).GetAwaiter().GetResult();
                var host = Dns.GetHostEntry(service.Address).HostName;
                var h = Context.ActorOf(HostManager.Prop(service, _configuration, _manager), Regex.Replace(host, @"[^\w\d]", ""));
                _hosts.Add(host, h);
            }
            Become(CreateConnections);
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
                _hosts.ToList()[u].Value.Forward(x);
            });
            Receive<ConnectedServerInfo>(x =>
            {
                Context.Parent.Forward(x);
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
        
        private void CreateConnections()
        {
            Receive<ConnectedServerInfo>(x =>
            {
                Context.Parent.Forward(x);
                Become(Ready);
            });
            ReceiveAny(x =>
            {
                Console.WriteLine($"Stashing {x.GetType()}");
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
