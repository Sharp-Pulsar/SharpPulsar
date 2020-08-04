using k8s;
using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    public class ZooKeeperStatefulSet
    {
        private readonly StatefulSet _set;
        public ZooKeeperStatefulSet(StatefulSet set)
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
                .NodeSelector(Values.BookKeeper.NodeSelector)
                .PodAntiAffinity(new List<V1PodAffinityTerm> 
                { 
                    new V1PodAffinityTerm
                    { 
                        LabelSelector = new V1LabelSelector
                        { 
                            MatchExpressions = new List<V1LabelSelectorRequirement>
                            {
                                new V1LabelSelectorRequirement{ Key = "app", OperatorProperty = "In", Values = new List<string>{$"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}" } },
                                new V1LabelSelectorRequirement{ Key = "release", OperatorProperty = "In", Values = new List<string>{$"{Values.ReleaseName}" } },
                                new V1LabelSelectorRequirement{ Key = "component", OperatorProperty = "In", Values = new List<string>{ Values.ZooKeeper.ComponentName }}
                            }
                        },
                        TopologyKey = "kubernetes.io/hostname"
                    }
                })
                .TerminationGracePeriodSeconds(Values.ZooKeeper.GracePeriodSeconds)
                .InitContainers(Values.ZooKeeper.ExtraInitContainers)
                .Containers(Values.ZooKeeper.Containers)
                .Volumes(Values.ZooKeeper.Volumes)                ;
            
            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
