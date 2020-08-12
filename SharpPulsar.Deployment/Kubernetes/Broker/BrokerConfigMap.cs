using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    internal class BrokerConfigMap
    {
        private readonly ConfigMap _config;
        public BrokerConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.Broker.Name}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Broker.Name },
                            })
                .Data(Values.ConfigMaps.Broker);
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
