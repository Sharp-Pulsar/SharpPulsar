using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    internal class BrokerClusterRoleBinding
    {
        private readonly ClusterRoleBinding _config;
        public BrokerClusterRoleBinding(ClusterRoleBinding config)
        {
            _config = config;
        }
        public V1ClusterRoleBinding Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-{Values.Settings.Broker.Name }-clusterrolebinding")
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .RoleRef("rbac.authorization.k8s.io", "ClusterRole", $"{Values.ReleaseName}-{Values.Settings.Broker.Name }-clusterrole")
                .AddSubject(Values.Namespace, "ServiceAccount", $"{Values.ReleaseName}-{Values.Settings.Broker.Name }-acct");
            return _config.Run(_config.Builder(), dryRun);
        }
    }
}
