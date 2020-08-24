using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Helpers;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    internal class ClusterInitializer
    {
        private readonly Job _job;
        public ClusterInitializer(Job job)
        {
            _job = job;
        }
        public RunResult Run(string dryRun = default)
        {
            _job.Builder()
                .Metadata($"{Values.ReleaseName}-cluster-init", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component","pulsar-init" }
                            })
                .TempBuilder()
                .SpecBuilder()
                .InitContainers(Containers.InitializerExtra())
                .Containers(new List<V1Container> 
                {
                    new V1Container
                    {
                        Name = $"{Values.ReleaseName}-pulsar-init",
                        Image = $"{Values.Images.PulsarMetadata.Repository}:{Values.Images.PulsarMetadata.Tag}",
                        ImagePullPolicy = Values.Images.PulsarMetadata.PullPolicy,
                        Command = new[]
                        {
                             "sh",
                             "-c"
                        },
                        Args = new List<string>{ string.Join(" ", Args.PulsarMetadataContainer()) },
                        VolumeMounts = VolumeMounts.ToolsetVolumMount()
                    }
                })
                .Volumes(Volumes.Toolset())
                .RestartPolicy("Never");
            return _job.Run(_job.Builder(), Values.Namespace, dryRun);
        }
    }
}
