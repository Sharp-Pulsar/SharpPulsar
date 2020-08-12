using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.AutoRecovery
{
    internal class AutoRecoveryStatefulSet
    {
        private readonly StatefulSet _set;
        public AutoRecoveryStatefulSet(StatefulSet set)
        {
            _set = set;
        }

        public RunResult Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.Settings.Autorecovery.Name}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Autorecovery.Name }
                            })
                .SpecBuilder()
                .ServiceName(Values.Settings.Autorecovery.Service)
                .Replication(Values.Settings.Autorecovery.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Autorecovery.Name }
                            })
                .UpdateStrategy(Values.Settings.Autorecovery.UpdateStrategy)
                .PodManagementPolicy(Values.Settings.Autorecovery.PodManagementPolicy)
                .VolumeClaimTemplates(Values.AutoRecovery.PVC)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Autorecovery.Name }
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
                .TerminationGracePeriodSeconds(Values.Settings.Autorecovery.GracePeriodSeconds)
                .InitContainers(Values.AutoRecovery.ExtraInitContainers)
                .Containers(Values.AutoRecovery.Containers)
                .Volumes(Values.AutoRecovery.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
