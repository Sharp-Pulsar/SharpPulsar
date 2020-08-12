using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Proxy
{
    internal class ProxyServiceAccount
    {
        private readonly ServiceAccount _serviceAccount;

        public ProxyServiceAccount(ServiceAccount serviceAccount)
        {
            _serviceAccount = serviceAccount;
        }
        public RunResult Run(string dryRun = default)
        {
            _serviceAccount.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.Proxy.Name}-acct", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Proxy.Name }
                            });
            return _serviceAccount.Run(_serviceAccount.Builder(), Values.Namespace, dryRun);
        }
    }
}
