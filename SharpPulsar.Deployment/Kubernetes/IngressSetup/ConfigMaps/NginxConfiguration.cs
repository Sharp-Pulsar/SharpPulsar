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
                .Metadata($"{Values.ReleaseName}-nginx-configuration", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Proxy.Name },
                            })
                .Data(new Dictionary<string, string> 
                {
                    {"use-forwarded-headers", "true" }
                });

            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
