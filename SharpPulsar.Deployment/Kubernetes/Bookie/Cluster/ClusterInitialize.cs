using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.Cluster
{
    internal class ClusterInitialize
    {
        private readonly Job _job;
        public ClusterInitialize(Job job)
        {
            _job = job;
        }

        public RunResult Run(string dryRun = default)
        {
            _job.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}-init", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.BookKeeper.Name }
                            })
                .TempBuilder()
                .SpecBuilder()
                .InitContainers(Values.ExtraConfigs.Bookie.ExtraInitContainers)
                .Containers(Values.ExtraConfigs.Bookie.Containers)
                .Volumes(new List<V1Volume>())
                .RestartPolicy("Never");
            return _job.Run(_job.Builder(), Values.Namespace, dryRun);
        }
    }
}
