using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.ConfigMaps
{
    internal class UdpServices
    {
        private readonly ConfigMap _config;
        public UdpServices(ConfigMap config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata("udp-services", "ingress-nginx")
                .Labels(new Dictionary<string, string>
                {
                    {"helm.sh/chart", "ingress-nginx-2.11.1"},
                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/instance", "ingress-nginx"},
                    {"app.kubernetes.io/version", "0.34.1"},
                    {"app.kubernetes.io/managed-by", "Helm"},
                    {"app.kubernetes.io/component", "controller"}
                });
            return _config.Run(_config.Builder(), "ingress-nginx", dryRun);
        }
    }
}
