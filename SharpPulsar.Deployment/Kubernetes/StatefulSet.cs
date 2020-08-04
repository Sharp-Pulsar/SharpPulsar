using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    public class StatefulSet
    {
        private readonly IKubernetes _client;
        private StatefulSetBuilder _builder;
        public StatefulSet(IKubernetes client)
        {
            _client = client;
            _builder = new StatefulSetBuilder();
        }
        public StatefulSetBuilder Builder()
        {
            return _builder;
        }
        public V1StatefulSet Run(StatefulSetBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new StatefulSetBuilder();
            return _client.CreateNamespacedStatefulSet(build.Build(), ns, dryRun);
        }
        public async Task<V1StatefulSet> RunAsync(StatefulSetBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new StatefulSetBuilder();
            return await _client.CreateNamespacedStatefulSetAsync(build.Build(), ns, dryRun);
        }
    }
}
