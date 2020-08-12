using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Presto.Worker
{
    internal class PrestoWorkerStatefulSet
    {
        private readonly StatefulSet _set;
        internal PrestoWorkerStatefulSet(StatefulSet set)
        {
            _set = set;
        }

        public RunResult Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.Settings.PrestoWorker.Name}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.PrestoWorker.Name }
                            })
                .SpecBuilder()
                .ServiceName(Values.Settings.PrestoWorker.Service)
                .Replication(Values.Settings.PrestoWorker.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.PrestoWorker.Name }
                            })
                .UpdateStrategy(Values.Settings.PrestoWorker.UpdateStrategy)
                .PodManagementPolicy(Values.Settings.PrestoWorker.PodManagementPolicy)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.PrestoWorker.Name }
                            }, new Dictionary<string, string>
                            {
                                {"\"prometheus.io/scrape\"", "false" },
                                {"\"prometheus.io/port\"", Values.Ports.PrestoWorker["http"].ToString() }
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.PrestoWorker.Tolerations)
                .NodeSelector(Values.PrestoWorker.NodeSelector)
                .PodAntiAffinity(Helpers.AntiAffinity.AffinityTerms(Values.Settings.PrestoWorker))
                .TerminationGracePeriodSeconds(Values.Settings.PrestoWorker.GracePeriodSeconds)
                .InitContainers(Values.PrestoWorker.ExtraInitContainers)
                .Containers(Values.PrestoWorker.Containers)
                .Volumes(Values.PrestoWorker.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}