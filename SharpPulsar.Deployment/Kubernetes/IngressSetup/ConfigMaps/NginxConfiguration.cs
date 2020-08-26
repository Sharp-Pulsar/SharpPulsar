using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.ConfigMaps
{
    internal class NginxConfiguration
    {
        private readonly ConfigMap _config;
        public NginxConfiguration(ConfigMap config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-nginx-configuration", "ingress-nginx")
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", "ingress-nginx" },
                            })
                .Data(new Dictionary<string, string> 
                {
                    {"use-forwarded-headers", "true" }
                });

            return _config.Run(_config.Builder(), "ingress-nginx", dryRun);
        }
    }
}
