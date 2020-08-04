using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes.Bookie
{
    public class BookieService
    {
        private readonly IKubernetes _client;
        public BookieService(IKubernetes client)
        {
            _client = client;
        }
        public static ServiceBuilder Builder()
        {
            return new ServiceBuilder();
        }
        public V1Service Run(string ns, string dryRun = default)
        {
            return _client.CreateNamespacedService(Builder().Build(), ns, dryRun);
        }
    }
}
