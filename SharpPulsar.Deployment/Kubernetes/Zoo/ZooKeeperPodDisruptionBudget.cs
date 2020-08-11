using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    internal class ZooKeeperPodDisruptionBudget
    {
        private readonly PodDisruptionBudget _pdb;
        internal ZooKeeperPodDisruptionBudget(PodDisruptionBudget pdb)
        {
            _pdb = pdb;
        }
        public V1beta1PodDisruptionBudget Run(string dryRun = default)
        {
            _pdb.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.ZooKeeper.Name }
                            })
                .MatchLabels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.ZooKeeper.Name }
                            })
                .MaxUnavailable(new IntstrIntOrString { Value = "1" });
            return _pdb.Run(_pdb.Builder(), Values.Namespace, dryRun);
        }
    }
}
