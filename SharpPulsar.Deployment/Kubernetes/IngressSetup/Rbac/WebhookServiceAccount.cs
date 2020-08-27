using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Rbac
{
    internal class WebhookServiceAccount
    {
        private readonly ServiceAccount _serviceAccount;

        public WebhookServiceAccount(ServiceAccount serviceAccount)
        {
            _serviceAccount = serviceAccount;
        }
        public RunResult Run(string dryRun = default)
        {
            _serviceAccount.Builder()
                .Metadata("ingress-nginx-admission", "ingress-nginx")
                .Labels(new Dictionary<string, string>
                {
                    {"helm.sh/chart", "ingress-nginx-2.11.1"},
                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/instance", "ingress-nginx"},
                    {"app.kubernetes.io/version", "0.34.1"},
                    {"app.kubernetes.io/managed-by", "Helm"},
                    {"app.kubernetes.io/component", "admission-webhook"}
                })
                .Annotation(new Dictionary<string, string> 
                {
                    {"helm.sh/hook", "pre-install,pre-upgrade,post-install,post-upgrade"},
                    {"helm.sh/hook-delete-policy", "before-hook-creation,hook-succeeded"}
                });
            return _serviceAccount.Run(_serviceAccount.Builder(), "ingress-nginx", dryRun);
        }
    }
}
