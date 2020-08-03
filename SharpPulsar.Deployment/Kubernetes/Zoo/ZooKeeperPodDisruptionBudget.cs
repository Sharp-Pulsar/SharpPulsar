using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    public class BookieKeeperPodDisruptionBudget
    {
        private readonly PodDisruptionBudget _pdb;
        public BookieKeeperPodDisruptionBudget(PodDisruptionBudget pdb)
        {
            _pdb = pdb;
        }
        public V1beta1PodDisruptionBudget Run()
        {
            _pdb.Builder()
                .Metadata($"{Values.ReleaseName}-zookeeper", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.ReleaseName },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component","zookeeper" }
                            })
                .MatchLabels(new Dictionary<string, string>
                            {
                                {"app", Values.ReleaseName },
                                {"release", Values.ReleaseName },
                                {"component","zookeeper" }
                            })
                .MaxUnavailable(new IntstrIntOrString { Value = "1" })                ;
            return _pdb.Run(_pdb.Builder(), Values.Namespace);
        }
    }
}
