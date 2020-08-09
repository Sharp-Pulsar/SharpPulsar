using k8s.Models;

namespace SharpPulsar.Deployment.Kubernetes.NetworkCenter.Rbac
{
    internal class CenterServiceAccount
    {
        private readonly ServiceAccount _serviceAccount;

        public CenterServiceAccount(ServiceAccount serviceAccount)
        {
            _serviceAccount = serviceAccount;
        }
        public V1ServiceAccount Run(string dryRun = default)
        {
            _serviceAccount.Builder()
                .Metadata($"{Values.ReleaseName}-nginx-ingress-serviceaccount", Values.Namespace);
            return _serviceAccount.Run(_serviceAccount.Builder(), Values.Namespace, dryRun);
        }
    }
}
