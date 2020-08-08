using k8s;
using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    internal class ZooKeeperStatefulSet
    {
        private readonly StatefulSet _set;
        internal ZooKeeperStatefulSet(StatefulSet set)
        {
            _set = set;
        }
        
        public V1StatefulSet Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.ZooKeeper.ComponentName }
                            })
                .SpecBuilder()
                .ServiceName(Values.ZooKeeper.ServiceName)
                .Replication(Values.ZooKeeper.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.ZooKeeper.ComponentName }
                            })
                .UpdateStrategy(Values.ZooKeeper.UpdateStrategy)
                .PodManagementPolicy(Values.ZooKeeper.PodManagementPolicy)
                .VolumeClaimTemplates(Values.ZooKeeper.PVC)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.ZooKeeper.ComponentName }
                            }, new Dictionary<string, string>
                            {
                                {"prometheus.io/scrape", "true" },
                                {"prometheus.io/port", "8000" }
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.ZooKeeper.Tolerations)
                .NodeSelector(Values.ZooKeeper.NodeSelector)
                .PodAntiAffinity(Helpers.AntiAffinity.AffinityTerms(Values.ZooKeeper))
                .TerminationGracePeriodSeconds(Values.ZooKeeper.GracePeriodSeconds)
                .InitContainers(Values.ZooKeeper.ExtraInitContainers)
                .Containers(Values.ZooKeeper.Containers)
                .Volumes(Values.ZooKeeper.Volumes);
            
            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
