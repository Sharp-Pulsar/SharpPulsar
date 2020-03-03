using System;
using System.Collections.Generic;
using System.Net;
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
		public NetworkManager(IActorRef manager, ClientConfigurationData configuration)
        {
            _serviceNameResolver.UpdateServiceUrl(configuration.ServiceUrl);
            _manager = manager;
            _configuration = configuration;
            Console.WriteLine("Become CreateConnections");
            Become(CreateConnections);
        }

        private void Start()
        {
            ReceiveAny(m =>
            {
                Stash.Stash();
            });
            CreateConnections();
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
            try
            {
                Console.WriteLine("Become Ready Unstashing");
                Stash.UnstashAll();
                Context.Parent.Tell(new ServiceReady());
                Receive<UpdateService>(u =>
                {
                    _serviceNameResolver.UpdateServiceUrl(u.Service);
                    Become(Stop);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        private void CreateConnections()
        {
            var dnsResolver = new DefaultNameResolver();
            foreach (var s in _serviceNameResolver.AddressList())
            {
                var service = s;
                if (!dnsResolver.IsResolved(s))
                    service = (IPEndPoint)dnsResolver.ResolveAsync(s).GetAwaiter().GetResult();
                var host = Dns.GetHostEntry(service.Address).HostName;
                Context.ActorOf(HostManager.Prop(service, _configuration, _manager), "HostManager");
            }
            Console.WriteLine("Become Ready");
            Become(Ready);
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
