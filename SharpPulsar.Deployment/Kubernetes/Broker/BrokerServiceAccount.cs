using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    internal class BrokerServiceAccount
    {
        private readonly ServiceAccount _serviceAccount;

        public BrokerServiceAccount(ServiceAccount serviceAccount)
        {
            _serviceAccount = serviceAccount;
        }
        public V1ServiceAccount Run(string dryRun = default)
        {
            _serviceAccount.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Broker.ComponentName}-acct", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Broker.ComponentName }
                            });
            return _serviceAccount.Run(_serviceAccount.Builder(), Values.Namespace, dryRun);
        }
    }
}
