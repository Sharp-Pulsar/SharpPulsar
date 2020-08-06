using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class Containers
    {
        public static V1Container WaitCsReady()
        {
            if (!string.IsNullOrWhiteSpace(Values.ConfigurationStore))
            {
                return new V1Container
                {
                    Name = "wait-cs-ready",
                    Image= $"{Values.Images.PulsarManager.Repository}:{Values.Images.PulsarManager.Tag}",
                    ImagePullPolicy = Values.Images.PulsarManager.PullPolicy,
                    Command = new[] { "sh", "-c" },
                    Args = new List<string>
                    {
                        $@"until nslookup {Values.ConfigurationStore}; do
                            sleep 3;
                        done;"
                    }
                };
            }
            return new V1Container();
        }
    }
}
