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
        
        public RunResult Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.ZooKeeper.Name }
                            })
                .SpecBuilder()
                .ServiceName(Values.Settings.ZooKeeper.Service)
                .Replication(Values.Settings.ZooKeeper.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.ZooKeeper.Name }
                            })
                .UpdateStrategy(Values.Settings.ZooKeeper.UpdateStrategy)
                .PodManagementPolicy(Values.Settings.ZooKeeper.PodManagementPolicy)
                .VolumeClaimTemplates(Values.ZooKeeper.PVC)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.ZooKeeper.Name }
                            }, 
                            new Dictionary<string, string>
                            {
                                {"prometheus.io/scrape", "true" },
                                {"prometheus.io/port", "8000" }
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.ZooKeeper.Tolerations)
                .NodeSelector(Values.ZooKeeper.NodeSelector)
                .PodAntiAffinity(Helpers.AntiAffinity.AffinityTerms(Values.Settings.ZooKeeper))
                .TerminationGracePeriodSeconds(Values.Settings.ZooKeeper.GracePeriodSeconds)
                .InitContainers(Values.ZooKeeper.ExtraInitContainers)
                .Containers(Values.ZooKeeper.Containers)
                .Volumes(Values.ZooKeeper.Volumes);
            
            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
