using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Rbac
{
    internal class IngressServiceAccount
    {
        private readonly ServiceAccount _serviceAccount;

        public IngressServiceAccount(ServiceAccount serviceAccount)
        {
            _serviceAccount = serviceAccount;
        }
        public RunResult Run(string dryRun = default)
        {
            _serviceAccount.Builder()
                .Metadata("ingress-nginx", Values.Namespace)
                .Labels(new Dictionary<string, string> 
                {
                    {"app", Values.App },
                    {"cluster", Values.Cluster },
                    {"release", Values.ReleaseName },
                    {"component", "nginx-ingress-controller" }
                });
            return _serviceAccount.Run(_serviceAccount.Builder(), Values.Namespace, dryRun);
        }
    }
}
