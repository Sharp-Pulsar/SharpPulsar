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

        public V1StatefulSet Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.PrestoWorker.ComponentName}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.PrestoWorker.ComponentName }
                            })
                .SpecBuilder()
                .ServiceName(Values.PrestoWorker.ServiceName)
                .Replication(Values.PrestoWorker.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.PrestoWorker.ComponentName }
                            })
                .UpdateStrategy(Values.PrestoWorker.UpdateStrategy)
                .PodManagementPolicy(Values.PrestoWorker.PodManagementPolicy)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.PrestoWorker.ComponentName }
                            }, new Dictionary<string, string>
                            {
                                {"prometheus.io/scrape", "false" },
                                {"prometheus.io/port", Values.Ports.PrestoWorker["http"].ToString() }
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.PrestoWorker.Tolerations)
                .NodeSelector(Values.PrestoWorker.NodeSelector)
                .PodAntiAffinity(Helpers.AntiAffinity.AffinityTerms(Values.PrestoWorker))
                .TerminationGracePeriodSeconds(Values.PrestoWorker.GracePeriodSeconds)
                .InitContainers(Values.PrestoWorker.ExtraInitContainers)
                .Containers(Values.PrestoWorker.Containers)
                .Volumes(Values.PrestoWorker.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}