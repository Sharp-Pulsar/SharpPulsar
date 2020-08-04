using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Zoo;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.AutoRecovery
{
    public class AutoRecoveryStatefulSet
    {
        private readonly StatefulSet _set;
        public AutoRecoveryStatefulSet(StatefulSet set)
        {
            _set = set;
        }

        public V1StatefulSet Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.AutoRecovery.ComponentName}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.AutoRecovery.ComponentName }
                            })
                .SpecBuilder()
                .ServiceName(Values.AutoRecovery.ServiceName)
                .Replication(Values.AutoRecovery.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component", Values.AutoRecovery.ComponentName }
                            })
                .UpdateStrategy(Values.AutoRecovery.UpdateStrategy)
                .PodManagementPolicy(Values.AutoRecovery.PodManagementPolicy)
                .VolumeClaimTemplates(Values.AutoRecovery.PVC)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.AutoRecovery.ComponentName }
                            }, new Dictionary<string, string>
                            {
                                {"prometheus.io/scrape", "true" },
                                {"prometheus.io/port", "8000" },
                                //{"checksum/config", @"{{ include (print $.Template.BasePath "/bookkeeper/bookkeeper-autorecovery-configmap.yaml") . | sha256sum }}" }
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.AutoRecovery.Tolerations)
                .PodAntiAffinity(new List<V1PodAffinityTerm>
                {
                    new V1PodAffinityTerm
                    {
                        LabelSelector = new V1LabelSelector
                        {
                            MatchExpressions = new List<V1LabelSelectorRequirement>
                            {
                                new V1LabelSelectorRequirement{ Key = "app", OperatorProperty = "In", Values = new List<string>{$"{Values.ReleaseName}-bookie" } },
                                new V1LabelSelectorRequirement{ Key = "release", OperatorProperty = "In", Values = new List<string>{$"{Values.ReleaseName}" } },
                                new V1LabelSelectorRequirement{ Key = "component", OperatorProperty = "In", Values = new List<string>{ "bookie" }}
                            }
                        },
                        TopologyKey = "kubernetes.io/hostname"
                    }
                })
                .TerminationGracePeriodSeconds(Values.AutoRecovery.GracePeriodSeconds)
                .InitContainers(Values.AutoRecovery.ExtraInitContainers)
                .Containers(Values.AutoRecovery.Containers)
                .Volumes(Values.AutoRecovery.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
