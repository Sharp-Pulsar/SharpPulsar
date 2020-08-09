using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class ClusterRoleBinding
    {
        private readonly IKubernetes _client;
        private ClusterRoleBindingBuilder _builder;
        public ClusterRoleBinding(IKubernetes client)
        {
            _client = client;
            _builder = new ClusterRoleBindingBuilder();
        }
        public ClusterRoleBindingBuilder Builder()
        {
            return _builder;
        }
        public V1ClusterRoleBinding Run(ClusterRoleBindingBuilder builder, string dryRun = default)
        {
            var build = builder;
            _builder = new ClusterRoleBindingBuilder();
            return _client.CreateClusterRoleBinding(build.Build(), dryRun);
        }
        public async Task<V1ClusterRoleBinding> RunAsync(ClusterRoleBindingBuilder builder, string dryRun = default)
        {
            var build = builder;
            _builder = new ClusterRoleBindingBuilder();
            return await _client.CreateClusterRoleBindingAsync(build.Build(), dryRun);
        }
    }
}
