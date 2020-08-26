using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.ConfigMaps
{
    internal class TcpServices
    {
        private readonly ConfigMap _config;
        public TcpServices(ConfigMap config)
        {
            _config = config;
        }
        public RunResult Run(Dictionary<string, string> ports, string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-tcp-services", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", "ingress-nginx" },
                            })
                .Data(ports);

            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
