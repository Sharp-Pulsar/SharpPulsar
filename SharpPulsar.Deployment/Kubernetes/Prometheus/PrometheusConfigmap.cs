using SharpPulsar.Deployment.Kubernetes.Extensions;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Prometheus
{
    internal class PrometheusConfigmap
    {
        private readonly ConfigMap _config;
        public PrometheusConfigmap(ConfigMap config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.Prometheus.Name}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Prometheus.Name },
                            })
                .Data(Values.ConfigMaps.Prometheus.RemoveRN());
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
