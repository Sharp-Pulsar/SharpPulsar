using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Presto.Coordinator
{
    internal class PrestoCoordinatorConfigMap
    {
        private readonly ConfigMap _config;
        public PrestoCoordinatorConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.PrestoCoord.Name}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.PrestoCoord.Name },
                            })
                .Data(Values.ConfigMaps.PrestoCoordinator);
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}