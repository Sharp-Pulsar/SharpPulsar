using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    public class BrokerStatefulset
    {
        private readonly StatefulSet _set;
        public BrokerStatefulset(StatefulSet set)
        {
            _set = set;
        }

        public V1StatefulSet Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.Broker.ComponentName}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Broker.ComponentName }
                            })
                .SpecBuilder()
                .ServiceName(Values.Broker.ServiceName)
                .Replication(Values.Broker.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Broker.ComponentName }
                            })
                .UpdateStrategy(Values.Broker.UpdateStrategy)
                .PodManagementPolicy(Values.Broker.PodManagementPolicy)
                .VolumeClaimTemplates(Values.Broker.PVC)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Broker.ComponentName }
                            }, new Dictionary<string, string>
                            {
                                {"prometheus.io/scrape", "true" },
                                {"prometheus.io/port", "8000" }
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.Broker.Tolerations)
                .SecurityContext(Values.Broker.SecurityContext)
                .ServiceAccountName($"{Values.ReleaseName}-{Values.Broker.ComponentName}-acct")
                .NodeSelector(Values.Broker.NodeSelector)
                .PodAntiAffinity(new List<V1PodAffinityTerm>
                {
                    new V1PodAffinityTerm
                    {
                        LabelSelector = new V1LabelSelector
                        {
                            MatchExpressions = new List<V1LabelSelectorRequirement>
                            {
                                new V1LabelSelectorRequirement{ Key = "app", OperatorProperty = "In", Values = new List<string>{$"{Values.ReleaseName}-{Values.Broker.ComponentName}" } },
                                new V1LabelSelectorRequirement{ Key = "release", OperatorProperty = "In", Values = new List<string>{$"{Values.ReleaseName}" } },
                                new V1LabelSelectorRequirement{ Key = "component", OperatorProperty = "In", Values = new List<string>{ Values.Broker.ComponentName }}
                            }
                        },
                        TopologyKey = "kubernetes.io/hostname"
                    }
                })
                .TerminationGracePeriodSeconds(Values.Broker.GracePeriodSeconds)
                .InitContainers(Values.Broker.ExtraInitContainers)
                .Containers(Values.Broker.Containers)
                .Volumes(Values.Broker.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
