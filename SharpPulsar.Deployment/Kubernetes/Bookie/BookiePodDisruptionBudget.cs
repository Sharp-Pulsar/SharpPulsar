using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie
{
    internal class BookiePodDisruptionBudget
    {
        private readonly PodDisruptionBudget _pdb;
        public BookiePodDisruptionBudget(PodDisruptionBudget pdb)
        {
            _pdb = pdb;
        }
        public V1beta1PodDisruptionBudget Run(string dryRun = default)
        {
            _pdb.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.BookKeeper.Name }
                            })
                .MatchLabels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.BookKeeper.Name }
                            })
                .MaxUnavailable(new IntstrIntOrString { Value = "1" });
            return _pdb.Run(_pdb.Builder(), Values.Namespace, dryRun);
        }
    }
}
