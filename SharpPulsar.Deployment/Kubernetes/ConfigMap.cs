using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class ConfigMap
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
        public async Task<V1ConfigMap> RunAsync(ConfigMapBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new ConfigMapBuilder();
            return await _client.CreateNamespacedConfigMapAsync(build.Build(), ns, dryRun);
        }
    }
}
