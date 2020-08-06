using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Proxy
{
    public class ProxyStatefulset
    {
        private readonly StatefulSet _set;
        public ProxyStatefulset(StatefulSet set)
        {
            _set = set;
        }

        public V1StatefulSet Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.Proxy.ComponentName}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Proxy.ComponentName }
                            })
                .SpecBuilder()
                .ServiceName(Values.Proxy.ServiceName)
                .Replication(Values.Proxy.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Proxy.ComponentName }
                            })
                .UpdateStrategy(Values.Proxy.UpdateStrategy)
                .PodManagementPolicy(Values.Proxy.PodManagementPolicy)
                .VolumeClaimTemplates(Values.Proxy.PVC)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Proxy.ComponentName }
                            }, new Dictionary<string, string>
                            {
                                {"prometheus.io/scrape", "true" },
                                {"prometheus.io/port", Values.Ports.Proxy["http"].ToString() }
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.Proxy.Tolerations)
                .SecurityContext(Values.Proxy.SecurityContext)
                .ServiceAccountName($"{Values.ReleaseName}-{Values.Proxy.ComponentName}-acct")
                .NodeSelector(Values.Proxy.NodeSelector)
                .PodAntiAffinity(Helpers.AntiAffinity.AffinityTerms(Values.Proxy))
                .TerminationGracePeriodSeconds(Values.Proxy.GracePeriodSeconds)
                .InitContainers(Values.Proxy.ExtraInitContainers)
                .Containers(Values.Proxy.Containers)
                .Volumes(Values.Proxy.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
