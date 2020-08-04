using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

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
    }
}
