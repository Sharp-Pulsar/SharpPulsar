using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.Cluster
{
    public class ClusterInitialize
    {
        private readonly Job _job;
        public ClusterInitialize(Job job)
        {
            _job = job;
        }

        public V1Job Run(string dryRun = default)
        {
            _job.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}-init", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.BookKeeper.ComponentName }
                            })
                .TempBuilder()
                .SpecBuilder()
                .InitContainers(Values.BookKeeper.ExtraConfig.ExtraInitContainers)
                .Containers(Values.BookKeeper.ExtraConfig.Containers)
                .Volumes(new List<V1Volume>())
                .RestartPolicy("Never");
            return _job.Run(_job.Builder(), Values.Namespace, dryRun);
        }
    }
}
