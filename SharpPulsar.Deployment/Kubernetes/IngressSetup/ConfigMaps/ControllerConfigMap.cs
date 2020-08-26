using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.ConfigMaps
{
    internal class ControllerConfigMap
    {
        private readonly ConfigMap _config;
        public ControllerConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata("ingress-nginx-controller", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"component", "ingress-nginx" },
                            });

            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}

