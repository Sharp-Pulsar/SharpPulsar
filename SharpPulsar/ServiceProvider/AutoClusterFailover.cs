using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Builder;
using SharpPulsar.Common;
using SharpPulsar.Interfaces;
using SharpPulsar.ServiceProvider.Messages;
using SharpPulsar.User;

namespace SharpPulsar.ServiceProvider
{
    public class AutoClusterFailover: IServiceUrlProvider
    {
        private IActorRef _clusterFailOverActor;
        private readonly AutoClusterFailoverBuilder _builder;
        /// <summary>
        /// Build the ServiceUrlProvider instance.
        /// <para>Example:
        /// 
        /// <pre>{@code
        /// ServiceUrlProvider failover = 
        /// AutoClusterFailover.Builder()
        /// .Primary("pulsar+ssl://broker.active.com:6651/")
        /// .Secondary(new List<string>{"pulsar+ssl://broker.standby.com:6651"})
        /// .FailoverDelay(TimeSpan.FromSeconds(30))
        /// .SwitchBackDelay(TimeSpan.FromSeconds(60))
        /// .CheckInterval(TimeSpan.FromSeconds(1))
        /// .SecondaryAuthentication(secondaryAuth);
        /// }</pre>
        /// 
        /// </para>
        /// @return
        /// </summary>
        public AutoClusterFailover(AutoClusterFailoverBuilder builder)
        {
            builder.Validate();
            _builder = builder; 
        }

        public string ServiceUrl => ServiceUrlAsync().GetAwaiter().GetResult();

        public async ValueTask<string> ServiceUrlAsync() => await _clusterFailOverActor
            .Ask<string>(GetServiceUrl.Instance)
            .ConfigureAwait(false);

        public void Close()
        {
            _clusterFailOverActor.Tell(PoisonPill.Instance);
        }
        public string Primary => _builder.primary;  
        public IList<string> Secondary => _builder.secondary;
        public TimeSpan FailoverDelayNs => _builder.FailoverDelayNs;
        public TimeSpan SwitchBackDelayNs => _builder.SwitchBackDelayNs;
        public TimeSpan IntervalMs => _builder.CheckIntervalMs;
        public IDictionary<string, string> SecondaryTlsTrustCertsFilePaths => _builder.SecondaryTlsTrustCertsFilePaths;
        public IDictionary<string, IAuthentication> SecondaryAuthentications => _builder.SecondaryAuthentications;
        public void Initialize(PulsarClient pulsarClient)
        {
            _clusterFailOverActor = pulsarClient
                .ActorSystem
                .ActorOf(AutoClusterFailoverActor.Prop(_builder), "auto-cluster-failover");
            _clusterFailOverActor.Tell(new Initialize(pulsarClient));
        }
        public static IAutoClusterFailoverBuilder Builder()
        {
            return new AutoClusterFailoverBuilder();
        }
    }
}
