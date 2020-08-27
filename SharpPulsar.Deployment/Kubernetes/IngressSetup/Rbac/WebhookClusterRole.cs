using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Rbac
{
    internal class WebhookClusterRole
    {
        private readonly ClusterRole _config;
        public WebhookClusterRole(ClusterRole config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name("ingress-nginx-admission")
                .Labels(new Dictionary<string, string>
                {
                    {"helm.sh/chart", "ingress-nginx-2.11.1"},
                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/instance", "ingress-nginx"},
                    {"app.kubernetes.io/version", "0.34.1"},
                    {"app.kubernetes.io/managed-by", "Helm"},
                    {"app.kubernetes.io/component", "admission-webhook"}
                })
                .Annotations(new Dictionary<string, string>
                {
                    {"helm.sh/hook", "pre-install,pre-upgrade,post-install,post-upgrade"},
                    {"helm.sh/hook-delete-policy", "before-hook-creation,hook-succeeded"}
                })
                .AddRule(new[] { "admissionregistration.k8s.io" }, new[] { "validatingwebhookconfigurations" }, new[] { "get", "update" });
            return _config.Run(_config.Builder(), dryRun);
        }
    }
}
