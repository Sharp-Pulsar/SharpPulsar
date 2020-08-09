using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class ServiceAccount
    {
        private readonly IKubernetes _client;
        private ServiceAccountBuilder _builder;
        public ServiceAccount(IKubernetes client)
        {
            _client = client;
            _builder = new ServiceAccountBuilder();
        }
        public ServiceAccountBuilder Builder()
        {
            return _builder;
        }
        public V1ServiceAccount Run(ServiceAccountBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new ServiceAccountBuilder();
            return _client.CreateNamespacedServiceAccount(build.Build(), ns, dryRun);
        }
        public async Task<V1ServiceAccount> RunAsync(ServiceAccountBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new ServiceAccountBuilder();
            return await _client.CreateNamespacedServiceAccountAsync(build.Build(), ns, dryRun);
        }
    }
}
