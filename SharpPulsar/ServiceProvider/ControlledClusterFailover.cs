using System;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Builder;
using SharpPulsar.Common;
using SharpPulsar.ServiceProvider.Messages;
using SharpPulsar.User;

namespace SharpPulsar.ServiceProvider
{
    public class ControlledClusterFailover: IServiceUrlProvider
    {
        private IActorRef _clusterFailOverActor;
        private readonly ControlledClusterFailoverBuilder _builder;
        /// <summary>
        /// Build the ServiceUrlProvider instance.
        /// <para>Example:
        /// 
        /// <pre>{@code
        /// Dictionary<string, string> header = new Dictionary<string, string>();
        /// header.Add("clusterA", "<credentials-for-urlProvider>");
        /// 
        /// ServiceUrlProvider failover = 
        /// ControlledClusterFailover.Builder()
        /// .DefaultServiceUrl("pulsar+ssl://broker.active.com:6651/")
        /// .CheckInterval(TimeSpan.FromMinutes(1))
        /// .UrlProvider("http://failover-notification-service:8080/check")
        /// .UrlProviderHeader(header);
        /// }</pre>
        /// 
        /// </para>
        /// @return
        /// </summary>
        public ControlledClusterFailover(ControlledClusterFailoverBuilder builder)
        {
            _builder = builder;
        }

        public string ServiceUrl => ServiceUrlAsync().GetAwaiter().GetResult();

        public async ValueTask<string> ServiceUrlAsync() => await _clusterFailOverActor
            .Ask<string>(GetServiceUrl.Instance, TimeSpan.FromSeconds(5))
            .ConfigureAwait(false);

        public void Close()
        {
            _clusterFailOverActor.Tell(PoisonPill.Instance);
        }
        public void CreateActor(ActorSystem actorSystem)
        {
            _clusterFailOverActor = actorSystem.ActorOf(ControlledClusterFailoverActor.Prop(_builder), "controlled-cluster-failover");
        }
        public void Initialize(PulsarClient pulsarClient)
        {           
            _clusterFailOverActor.Tell(new Initialize(pulsarClient));
        }
    }
}
