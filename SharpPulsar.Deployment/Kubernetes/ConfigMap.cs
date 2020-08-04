using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes
{
    public class ConfigMap
    {
        private readonly IKubernetes _client;
        private ConfigMapBuilder _builder;
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
            var build = builder;
            _builder = new ConfigMapBuilder();
            return _client.CreateNamespacedConfigMap(build.Build(), ns, dryRun);
        }
    }
}
