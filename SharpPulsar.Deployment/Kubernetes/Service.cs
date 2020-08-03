using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    public class Service
    {
        private readonly IKubernetes _client;
        private readonly ServiceBuilder _builder;
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
            return _client.CreateNamespacedService(builder.Build(), ns, dryRun);
        }
    }
}
