using System.Collections.Generic;
using System.Net;
using Akka.Actor;
using DotNetty.Transport.Bootstrapping;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Network
{
    public class NetworkManager: ReceiveActor
    {
        private List<IPEndPoint> _endPoints;
        private IActorRef _outActorRef;
        private ClientConfigurationData _configuration;
        private int _protocolVersion;
		public NetworkManager(List<IPEndPoint> endPoints, IActorRef outActorRef, ClientConfigurationData configuration, int protocolVersion)
        {
            _endPoints = endPoints;
            _outActorRef = outActorRef;
            _configuration = configuration;
            _protocolVersion = protocolVersion;
        }

        protected override void PreStart()
        {
            var dnsResolver = new DefaultNameResolver();
            foreach (var s in _endPoints)
            {
                var service = s;
                if (!dnsResolver.IsResolved(s))
                    service = (IPEndPoint) dnsResolver.ResolveAsync(s).GetAwaiter().GetResult();
                var host = Dns.GetHostEntry(service.Address).HostName;
                Context.ActorOf(HostManager.Prop(service, _configuration.ConnectionsPerBroker, _configuration, _protocolVersion, _outActorRef), host);
            }
        }

        public static Props Prop(List<IPEndPoint> endPoints, IActorRef outActorRef, ClientConfigurationData configuration, int protocolVersion)
        {
            return Props.Create(()=> new NetworkManager(endPoints, outActorRef, configuration, protocolVersion));
        }
    }
}
