using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.AutoRecovery
{
    internal class AutoRecoveryConfigMap
    {
        private readonly ConfigMap _config;
        public AutoRecoveryConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public V1ConfigMap Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.Autorecovery.Name }", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Autorecovery.Name },
                            })
                .Data(Values.ConfigMaps.AutoRecovery);
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
