
using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class PodDisruptionBudget
    {
        private readonly IKubernetes _client;
        private PodDisruptionBudgetBuilder _builder;
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
            var build = builder;
            _builder = new PodDisruptionBudgetBuilder();
            return _client.CreateNamespacedPodDisruptionBudget(build.Build(), ns, dryRun);
        }
        public async Task<V1beta1PodDisruptionBudget> RunAsync(PodDisruptionBudgetBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new PodDisruptionBudgetBuilder();
            return await _client.CreateNamespacedPodDisruptionBudgetAsync(build.Build(), ns, dryRun);
        }
    }
}
