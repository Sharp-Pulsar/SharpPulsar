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
        public void CreateActor(ActorSystem actorSystem)
        {
            _clusterFailOverActor = actorSystem.ActorOf(AutoClusterFailoverActor.Prop(_builder), "auto-cluster-failover");
        }
        public void Initialize(PulsarClient pulsarClient)
        {            
            _clusterFailOverActor.Tell(new Initialize(pulsarClient));
        }
        public static IAutoClusterFailoverBuilder Builder()
        {
            return new AutoClusterFailoverBuilder();
        }
    }
}
