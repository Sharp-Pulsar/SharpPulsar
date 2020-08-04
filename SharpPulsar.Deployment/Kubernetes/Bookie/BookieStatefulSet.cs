using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie
{
    public class BookieStatefulSet
    {
        private readonly StatefulSet _set;
        public BookieStatefulSet(StatefulSet set)
        {
            _set = set;
        }

        public V1StatefulSet Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.BookKeeper.ComponentName }
                            })
                .SpecBuilder()
                .ServiceName(Values.BookKeeper.ServiceName)
                .Replication(Values.BookKeeper.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.BookKeeper.ComponentName }
                            })
                .UpdateStrategy(Values.BookKeeper.UpdateStrategy)
                .PodManagementPolicy(Values.BookKeeper.PodManagementPolicy)
                .VolumeClaimTemplates(Values.BookKeeper.PVC)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.BookKeeper.ComponentName }
                            }, new Dictionary<string, string>
                            {
                                {"prometheus.io/scrape:", "true" },
                                {"prometheus.io/port", "8000" },
                                {"checksum/config", @"{{ include (print $.Template.BasePath ""/bookkeeper/bookkeeper-configmap.yaml"") . | sha256sum }}" }
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.BookKeeper.Tolerations)
                .SecurityContext(Values.BookKeeper.SecurityContext)
                .ServiceAccountName($"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}-acct")
                .NodeSelector(Values.BookKeeper.NodeSelector)
                .PodAntiAffinity(new List<V1PodAffinityTerm>
                {
                    new V1PodAffinityTerm
                    {
                        LabelSelector = new V1LabelSelector
                        {
                            MatchExpressions = new List<V1LabelSelectorRequirement>
                            {
                                new V1LabelSelectorRequirement{ Key = "app", OperatorProperty = "In", Values = new List<string>{$"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}" } },
                                new V1LabelSelectorRequirement{ Key = "release", OperatorProperty = "In", Values = new List<string>{$"{Values.ReleaseName}" } },
                                new V1LabelSelectorRequirement{ Key = "component", OperatorProperty = "In", Values = new List<string>{ Values.BookKeeper.ComponentName }}
                            }
                        },
                        TopologyKey = "kubernetes.io/hostname"
                    }
                })
                .TerminationGracePeriodSeconds(Values.BookKeeper.GracePeriodSeconds)
                .InitContainers(Values.BookKeeper.ExtraInitContainers)
                .Containers(Values.BookKeeper.Containers)
                .Volumes(Values.BookKeeper.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
