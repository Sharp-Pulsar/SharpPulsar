
using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes
{
    public class PodDisruptionBudget
    {
        private readonly IKubernetes _client;
        private readonly PodDisruptionBudgetBuilder _builder;
        public PodDisruptionBudget(IKubernetes client)
        {
            _client = client;
            _builder = new PodDisruptionBudgetBuilder();
        }
        public PodDisruptionBudgetBuilder Builder()
        {
            return _builder;
        }
        public V1beta1PodDisruptionBudget Run(PodDisruptionBudgetBuilder builder, string ns, string dryRun = default)
        {
            return _client.CreateNamespacedPodDisruptionBudget(builder.Build(), ns, dryRun);
        }
    }
}
