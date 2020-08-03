using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    public class StatefulSet
    {
        private readonly IKubernetes _client;
        private readonly StatefulSetBuilder _builder;
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
            return _client.CreateNamespacedStatefulSet(builder.Build(), ns, dryRun);
        }
    }
}
