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
                .Metadata("tcp-services", "ingress-nginx")
                .Labels(new Dictionary<string, string>
                {
                    {"helm.sh/chart", "ingress-nginx-2.11.1"},
                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/instance", "ingress-nginx"},
                    {"app.kubernetes.io/version", "0.34.1"},
                    {"app.kubernetes.io/managed-by", "Helm"},
                    {"app.kubernetes.io/component", "controller"}
                });
                //.Data(ports);

            return _config.Run(_config.Builder(), "ingress-nginx", dryRun);
        }
    }
}
