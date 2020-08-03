using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    public class BookieKeeperConfigMap
    {
        private readonly IKubernetes _client;
        public BookieKeeperConfigMap(IKubernetes client)
        {
            _client = client;
        }
        public static ConfigMapBuilder Builder()
        {
            return new ConfigMapBuilder();
        }
        public V1ConfigMap Run(string ns, string dryRun = default)
        {
            return _client.CreateNamespacedConfigMap(Builder().Build(), ns, dryRun);
        }
    }
}
