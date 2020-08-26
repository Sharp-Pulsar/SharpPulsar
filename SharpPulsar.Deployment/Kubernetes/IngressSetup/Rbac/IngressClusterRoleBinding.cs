using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Rbac
{
    internal class IngressClusterRoleBinding
    {
        private readonly ClusterRoleBinding _config;
        public IngressClusterRoleBinding(ClusterRoleBinding config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-ingress-nginx")
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .RoleRef("rbac.authorization.k8s.io", "ClusterRole", $"{Values.ReleaseName}-ingress-nginx")
                .AddSubject(Values.Namespace, "ServiceAccount", "ingress-nginx");
            return _config.Run(_config.Builder(), dryRun);
        }
    }
}
