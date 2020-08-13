using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class Containers
    {
        public static List<V1Container> InitializerExtra()
        {
            var extras = new List<V1Container>();
            if (!string.IsNullOrWhiteSpace(Values.ConfigurationStore))
            {
                extras.Add(new V1Container
                {
                    Name = "wait-cs-ready",
                    Image = $"{Values.Images.PulsarManager.Repository}:{Values.Images.PulsarManager.Tag}",
                    ImagePullPolicy = Values.Images.PulsarManager.PullPolicy,
                    Command = new[] { "sh", "-c" },
                    Args = new List<string>
                    {
                        $@"until nslookup {Values.ConfigurationStore}; do  sleep 3; done;"
                    }
                });
            }
            extras.Add(
            new V1Container
            {
                Name = "wait-zookeeper-ready",
                Image = $"{Values.Images.PulsarMetadata.Repository}:{Values.Images.PulsarMetadata.Tag}",
                ImagePullPolicy = Values.Images.PulsarMetadata.PullPolicy,
                Command = new[]
                        {
                            "sh",
                            "-c"
                        },
                Args = new List<string> { string.Join(" ", Args.WaitZooKeeperContainer()) }
            });
            extras.Add(new V1Container
            {
                Name = "pulsar-bookkeeper-verify-clusterid",
                Image = $"{Values.Images.PulsarMetadata.Repository}:{Values.Images.PulsarMetadata.Tag}",
                ImagePullPolicy = Values.Images.PulsarMetadata.PullPolicy,
                Command = new[]
                        {
                             "sh",
                             "-c"
                        },
                Args = new List<string> { string.Join(" ", Args.MetadataBookieContainer()) },
                EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}"
                                }
                            }
                        },
                VolumeMounts = VolumeMounts.ToolsetVolumMount()
            });
            return extras;
        }
    }
}
