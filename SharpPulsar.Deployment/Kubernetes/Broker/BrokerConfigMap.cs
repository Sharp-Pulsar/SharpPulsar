using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    public class BrokerConfigMap
    {
        private readonly ConfigMap _config;
        public BrokerConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public V1ConfigMap Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Broker.ComponentName}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Broker.ComponentName },
                            })
                .Data(Values.Broker.ConfigData);
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
