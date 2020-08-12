using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie
{
    internal class BookieConfigMap
    {
        private readonly ConfigMap _config;
        public BookieConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.BookKeeper.Name },
                            })
                .Data(Values.ConfigMaps.BookKeeper);
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
