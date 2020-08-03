using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes
{
    public class ConfigMap
    {
        private readonly IKubernetes _client;
        private readonly ConfigMapBuilder _builder;
        public ConfigMap(IKubernetes client)
        {
            _client = client;
            _builder = new ConfigMapBuilder();
        }
        public ConfigMapBuilder Builder()
        {
            return _builder;
        }
        public V1ConfigMap Run(ConfigMapBuilder builder, string ns, string dryRun = default)
        {
            return _client.CreateNamespacedConfigMap(builder.Build(), ns, dryRun);
        }
    }
}
