using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    public class ClusterInitializer
    {
        private readonly Job _job;
        public ClusterInitializer(Job job)
        {
            _job = job;
        }
        public V1Job Run(string dryRun = default)
        {
            _job.Builder()
                .Metadata($"{Values.ReleaseName}-pulsar-init", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component","pulsar-init" }
                            })
                .TempBuilder()
                .SpecBuilder()
                .InitContainers(new List<V1Container> 
                { 
                    Containers.WaitCsReady(),
                    new V1Container
                    {
                        Name = "wait-zookeeper-ready",
                        Image = $"{Values.Images.PulsarMetadata.Repository}:{Values.Images.PulsarMetadata.Tag}",
                        ImagePullPolicy = Values.Images.PulsarMetadata.PullPolicy ,
                        Command = new []
                        {
                            "sh",
                            "-c"
                        },
                        Args = Args.WaitZooKeeperContainer()
                    },
                    new V1Container
                    {
                        Name = "pulsar-bookkeeper-verify-clusterid",
                        Image = $"{Values.Images.PulsarMetadata.Repository}:{Values.Images.PulsarMetadata.Tag}",
                        ImagePullPolicy = Values.Images.PulsarMetadata.PullPolicy,
                        Command = new[]
                        {
                             "sh",
                             "-c"
                        },
                        Args = Args.MetadataBookieContainer(),
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}"
                                }
                            }
                        },
                        VolumeMounts = VolumeMounts.ToolsetVolumMount()
                    }
                })
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
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string, ResourceQuantity>
                            {

                            }
                        },
                        Args = Args.PulsarMetadataContainer(),
                        VolumeMounts = VolumeMounts.ToolsetVolumMount()
                    }
                })
                .Volumes(Volumes.Toolset())
                .RestartPolicy("Never");
            return _job.Run(_job.Builder(), Values.Namespace, dryRun);
        }
    }
}
