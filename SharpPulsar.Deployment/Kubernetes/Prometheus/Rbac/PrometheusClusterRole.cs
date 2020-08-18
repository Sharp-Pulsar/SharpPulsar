using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Prometheus.Rbac
{
    internal class PrometheusClusterRole
    {
        private readonly ClusterRole _config;
        public PrometheusClusterRole(ClusterRole config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-{Values.Rbac.RoleName }")
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .AddRule(new[] { "" }, new[] { "nodes", "nodes/proxy", "services", "endpoints", "pods" }, new[] { "get", "list", "watch" })
                .AddRule(new[] { "/metrics" }, new[] { "get"})
                .AddRule(new[] { "" }, new[] { "namespaces", "persistentvolumes", "persistentvolumeclaims" }, new[] { "list", "watch", "get", "create" })
                .AddRule(new[] { "", "extensions", "apps" }, new[] { "pods", "deployments", "ingresses", "secrets", "statefulsets" }, new[] { "list", "watch", "get", "update", "create", "delete", "patch" })
                .AddRule(new[] { ""}, new[] { "replicasets" }, new[] { "list", "watch", "get"})
                .AddRule(new[] { ""}, new[] { "events" }, new[] { "list", "watch", "get"})
                .AddRule(new[] { "rbac.authorization.k8s.io" }, new[] { "clusterrolebindings", "clusterroles" }, new[] { "*"});
            return _config.Run(_config.Builder(), dryRun);
        }
    }
}
