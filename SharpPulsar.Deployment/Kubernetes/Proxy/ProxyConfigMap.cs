using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Proxy
{
    internal class ProxyConfigMap
    {

        private readonly ConfigMap _config;
        public ProxyConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.Proxy.Name}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Proxy.Name },
                            })
                .Data(Values.ConfigMaps.Proxy);
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
