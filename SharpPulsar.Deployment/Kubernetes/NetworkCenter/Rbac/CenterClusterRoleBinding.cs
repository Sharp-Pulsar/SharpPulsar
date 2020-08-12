using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.NetworkCenter.Rbac
{
    internal class CenterClusterRoleBinding
    {
        private readonly ClusterRoleBinding _config;
        public CenterClusterRoleBinding(ClusterRoleBinding config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-nginx-ingress-clusterrole-nisa-binding")
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .RoleRef("rbac.authorization.k8s.io", "ClusterRole", $"{Values.ReleaseName}-nginx-ingress-clusterrole")
                .AddSubject(Values.Namespace, "ServiceAccount", $"{Values.ReleaseName}-nginx-ingress-serviceaccount");
            return _config.Run(_config.Builder(), dryRun);
        }
    }
}
