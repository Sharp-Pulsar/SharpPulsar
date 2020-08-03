using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes
{
    public class ConfigMap
    {
        private readonly IKubernetes _client;
        public ConfigMap(IKubernetes client)
        {
            _client = client;
        }
        public ConfigMapBuilder Builder()
        {
            return new ConfigMapBuilder();
        }
        public V1ConfigMap Run(string ns, string dryRun = default)
        {
            return _client.CreateNamespacedConfigMap(Builder().Build(), ns, dryRun);
        }
    }
}
