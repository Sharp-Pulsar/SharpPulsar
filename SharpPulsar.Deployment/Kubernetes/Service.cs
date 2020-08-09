using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class Service
    {
        private readonly IKubernetes _client;
        private ServiceBuilder _builder;
        public Service(IKubernetes client)
        {
            _client = client;
            _builder = new ServiceBuilder();
        }
        public ServiceBuilder Builder()
        {
            return _builder;
        }
        public V1Service Run(ServiceBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new ServiceBuilder();
            return _client.CreateNamespacedService(build.Build(), ns, dryRun);
        }
        public async Task<V1Service> RunAsync(ServiceBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new ServiceBuilder();
            return await _client.CreateNamespacedServiceAsync(build.Build(), ns, dryRun);
        }
    }
}
