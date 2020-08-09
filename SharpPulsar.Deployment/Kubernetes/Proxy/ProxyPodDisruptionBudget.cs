using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Proxy
{
    internal class ProxyPodDisruptionBudget
    {
        private readonly PodDisruptionBudget _pdb;
        public ProxyPodDisruptionBudget(PodDisruptionBudget pdb)
        {
            _pdb = pdb;
        }
        public V1beta1PodDisruptionBudget Run(string dryRun = default)
        {
            _pdb.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Proxy.ComponentName}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Proxy.ComponentName }
                            })
                .MatchLabels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component", Values.Proxy.ComponentName }
                            })
                .MaxUnavailable(new IntstrIntOrString { Value = "1" });
            return _pdb.Run(_pdb.Builder(), Values.Namespace, dryRun);
        }
    }
}
