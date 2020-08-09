using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.NetworkCenter.Rbac
{
    internal class CenterRoleBinding
    {
        private readonly RoleBinding _config;
        public CenterRoleBinding(RoleBinding config)
        {
            _config = config;
        }
        public V1RoleBinding Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-nginx-ingress-role-nisa-binding")
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .RoleRef("rbac.authorization.k8s.io", "Role", $"{Values.ReleaseName}-nginx-ingress-role")
                .AddSubject(Values.Namespace, "ServiceAccount", $"{Values.ReleaseName}-nginx-ingress-serviceaccount");
            return _config.Run(_config.Builder(), dryRun);
        }
    }
}
