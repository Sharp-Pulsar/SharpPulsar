using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    internal class BrokerStatefulset
    {
        private readonly StatefulSet _set;
        public BrokerStatefulset(StatefulSet set)
        {
            _set = set;
        }

        public V1StatefulSet Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.Settings.Broker.Name}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Broker.Name }
                            })
                .SpecBuilder()
                .ServiceName(Values.Settings.Broker.Service)
                .Replication(Values.Settings.Broker.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Broker.Name }
                            })
                .UpdateStrategy(Values.Settings.Broker.UpdateStrategy)
                .PodManagementPolicy(Values.Settings.Broker.PodManagementPolicy)
                .VolumeClaimTemplates(Values.Broker.PVC)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Broker.Name }
                            }, new Dictionary<string, string>
                            {
                                {"prometheus.io/scrape", "true" },
                                {"prometheus.io/port", "8000" }
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.Broker.Tolerations)
                .SecurityContext(Values.Broker.SecurityContext)
                .ServiceAccountName($"{Values.ReleaseName}-{Values.Settings.Broker.Name}-acct")
                .NodeSelector(Values.Broker.NodeSelector)
                .PodAntiAffinity(Helpers.AntiAffinity.AffinityTerms(Values.Settings.Broker))
                .TerminationGracePeriodSeconds(Values.Settings.Broker.GracePeriodSeconds)
                .InitContainers(Values.Broker.ExtraInitContainers)
                .Containers(Values.Broker.Containers)
                .Volumes(Values.Broker.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
