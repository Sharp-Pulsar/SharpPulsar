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
        public RunResult Run(string dryRun = default)
        {
            _pdb.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.Proxy.Name}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Proxy.Name }
                            })
                .MatchLabels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Proxy.Name }
                            })
                .MaxUnavailable(new IntstrIntOrString { Value = "1" });
            return _pdb.Run(_pdb.Builder(), Values.Namespace, dryRun);
        }
    }
}
