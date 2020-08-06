using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.Cluster
{
    internal class BookieClusterRole
    {
        private readonly ClusterRole _config;
        public BookieClusterRole(ClusterRole config)
        {
            _config = config;
        }
        public V1ClusterRole Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-{Values.BookKeeper.ComponentName }-clusterrole")
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .AddRule(new[] {""}, new[] { "pods" }, new[] {"list", "get" });
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
