using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Presto.Coordinator
{
    internal class PrestoCoordinatorStatefulSet
    {
        private readonly StatefulSet _set;
        internal PrestoCoordinatorStatefulSet(StatefulSet set)
        {
            _set = set;
        }

        public V1StatefulSet Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.Settings.PrestoCoord.Name}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.PrestoCoord.Name }
                            })
                .SpecBuilder()
                .ServiceName(Values.Settings.PrestoCoord.Service)
                .Replication(Values.Settings.PrestoCoord.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.PrestoCoord.Name }
                            })
                .UpdateStrategy(Values.Settings.PrestoCoord.UpdateStrategy)
                .PodManagementPolicy(Values.Settings.PrestoCoord.PodManagementPolicy)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.PrestoCoord.Name }
                            }, new Dictionary<string, string>
                            {
                                {"prometheus.io/scrape", "false" },
                                {"prometheus.io/port", Values.Ports.PrestoCoordinator["http"].ToString() }
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.PrestoCoordinator.Tolerations)
                .NodeSelector(Values.PrestoCoordinator.NodeSelector)
                .PodAntiAffinity(Helpers.AntiAffinity.AffinityTerms(Values.Settings.PrestoCoord))
                .TerminationGracePeriodSeconds(Values.Settings.PrestoCoord.GracePeriodSeconds)
                .InitContainers(Values.PrestoCoordinator.ExtraInitContainers)
                .Containers(Values.PrestoCoordinator.Containers)
                .Volumes(Values.PrestoCoordinator.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}