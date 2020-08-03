
using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes.Bookie
{
    public class BookieKeeperPodDisruptionBudget
    {
        private readonly IKubernetes _client;
        public BookieKeeperPodDisruptionBudget(IKubernetes client)
        {
            _client = client;
        }
        public static PodDisruptionBudgetBuilder Builder()
        {
            return new PodDisruptionBudgetBuilder();
        }
        public V1beta1PodDisruptionBudget Run(string ns, string dryRun = default)
        {
            return _client.CreateNamespacedPodDisruptionBudget(Builder().Build(), ns, dryRun);
        }
    }
}
