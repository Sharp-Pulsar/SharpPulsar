using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    public class BrokerClusterRoleBinding
    {
        private readonly ClusterRoleBinding _config;
        public BrokerClusterRoleBinding(ClusterRoleBinding config)
        {
            _config = config;
        }
        public V1ClusterRoleBinding Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-{Values.Broker.ComponentName }-clusterrolebinding")
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .RoleRef("rbac.authorization.k8s.io", "ClusterRole", $"{Values.ReleaseName}-{Values.Broker.ComponentName }-clusterrole")
                .AddSubject(Values.Namespace, "ServiceAccount", $"{Values.ReleaseName}-{Values.Broker.ComponentName }-acct");
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
