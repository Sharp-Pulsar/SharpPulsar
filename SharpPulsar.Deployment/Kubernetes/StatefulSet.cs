using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    public class StatefulSet
    {
        private readonly IKubernetes _client;
        public StatefulSet(IKubernetes client)
        {
            _client = client; 
        }
        public StatefulSetBuilder Builder()
        {
            return new StatefulSetBuilder();
        }
        public V1StatefulSet Run(string ns, string dryRun = default)
        {
            return _client.CreateNamespacedStatefulSet(Builder().Build(), ns, dryRun);
        }
    }
}
