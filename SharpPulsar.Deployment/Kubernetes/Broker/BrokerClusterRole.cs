using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    internal class BrokerClusterRole
    {
        private readonly ClusterRole _config;
        public BrokerClusterRole(ClusterRole config)
        {
            _config = config;
        }
        public V1ClusterRole Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-{Values.Broker.ComponentName }-clusterrole")
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .AddRule(new[] { "" }, new[] { "configmap", "configmaps" }, new[] { "list", "get", "watch" })
                .AddRule(new[] { "", "extensions", "apps" }, new[] { "pods", "services", "deployments", "secrets", "statefulsets" }, new[] { "list", "watch", "get", "update", "create", "delete", "patch" });
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
