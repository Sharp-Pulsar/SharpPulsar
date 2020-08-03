
using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes
{
    public class PodDisruptionBudget
    {
        private readonly IKubernetes _client;
        public PodDisruptionBudget(IKubernetes client)
        {
            _client = client;
        }
        public PodDisruptionBudgetBuilder Builder()
        {
            return new PodDisruptionBudgetBuilder();
        }
        public V1beta1PodDisruptionBudget Run(string ns, string dryRun = default)
        {
            return _client.CreateNamespacedPodDisruptionBudget(Builder().Build(), ns, dryRun);
        }
    }
}
