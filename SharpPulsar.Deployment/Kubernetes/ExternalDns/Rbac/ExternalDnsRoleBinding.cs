using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.ExternalDns.Rbac
{
    internal class ExternalDnsRoleBinding
    {
        private readonly ClusterRoleBinding _config;
        public ExternalDnsRoleBinding(ClusterRoleBinding config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name("external-dns-viewer")
                .RoleRef("rbac.authorization.k8s.io", "ClusterRole", "external-dns")
                .AddSubject(Values.Namespace, "ServiceAccount", "external-dns");
            return _config.Run(_config.Builder(), dryRun);
        }
    }
}
