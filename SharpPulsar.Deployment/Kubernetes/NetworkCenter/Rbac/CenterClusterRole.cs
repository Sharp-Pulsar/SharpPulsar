using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.NetworkCenter.Rbac
{
    internal class CenterClusterRole
    {
        private readonly ClusterRole _config;
        public CenterClusterRole(ClusterRole config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-nginx-ingress-clusterrole")
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .AddRule(new[] { "" }, new[] { "configmaps", "endpoints", "nodes", "pods", "secrets" }, new[] { "list", "watch" })
                .AddRule(new[] { "" }, new[] { "nodes"}, new[] { "get" })
                .AddRule(new[] { "" }, new[] { "services" }, new[] { "get", "list", "watch" })
                .AddRule(new[] { "" }, new[] { "events" }, new[] { "create", "patch" })
                .AddRule(new[] { "extensions", "networking.k8s.io" }, new[] { "ingresses" }, new[] { "get", "list", "watch" })
                .AddRule(new[] { "extensions", "networking.k8s.io" }, new[] { "ingresses/status" }, new[] { "update" });
            return _config.Run(_config.Builder(), dryRun);
        }
    }
}
