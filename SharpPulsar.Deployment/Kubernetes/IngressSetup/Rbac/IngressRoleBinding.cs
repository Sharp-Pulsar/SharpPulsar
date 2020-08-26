using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Rbac
{
    internal class IngressRoleBinding
    {
        private readonly RoleBinding _config;
        public IngressRoleBinding(RoleBinding config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-ingress-nginx")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                {
                    {"app", Values.App },
                    {"cluster", Values.Cluster },
                    {"release", Values.ReleaseName },
                    {"component", "nginx-ingress-controller" }
                })
                .RoleRef("rbac.authorization.k8s.io", "Role", $"{Values.ReleaseName}-ingress-nginx")
                .AddSubject(Values.Namespace, "ServiceAccount", "ingress-nginx");
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
