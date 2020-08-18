using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Proxy
{
    internal class ProxyStatefulset
    {
        private readonly StatefulSet _set;
        public ProxyStatefulset(StatefulSet set)
        {
            _set = set;
        }

        public RunResult Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.Settings.Proxy.Name}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Proxy.Name }
                            })
                .SpecBuilder()
                .ServiceName(Values.Settings.Proxy.Service)
                .Replication(Values.Settings.Proxy.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Proxy.Name }
                            })
                .UpdateStrategy(Values.Settings.Proxy.UpdateStrategy)
                .PodManagementPolicy(Values.Settings.Proxy.PodManagementPolicy)
                .VolumeClaimTemplates(Values.Proxy.PVC)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Proxy.Name }
                            }, new Dictionary<string, string>
                            {
                                {"prometheus.io/scrape", "true" },
                                {"prometheus.io/port", "8080" }
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.Proxy.Tolerations)
                .SecurityContext(Values.Proxy.SecurityContext)
                .ServiceAccountName($"{Values.ReleaseName}-{Values.Settings.Proxy.Name}-acct")
                .NodeSelector(Values.Proxy.NodeSelector)
                .PodAntiAffinity(Helpers.AntiAffinity.AffinityTerms(Values.Settings.Proxy))
                .TerminationGracePeriodSeconds(Values.Settings.Proxy.GracePeriodSeconds)
                .InitContainers(Values.Proxy.ExtraInitContainers)
                .Containers(Values.Proxy.Containers)
                .Volumes(Values.Proxy.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
