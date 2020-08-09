using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    internal class BookieConfigMap
    {
        private readonly ConfigMap _config;
        public BookieConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public V1ConfigMap Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.BookKeeper.ComponentName },
                            })
                .Data(Values.BookKeeper.ConfigData);
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
