using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class RoleBinding
    {
        private readonly IKubernetes _client;
        private RoleBindingBuilder _builder;
        public RoleBinding(IKubernetes client)
        {
            _client = client;
            _builder = new RoleBindingBuilder();
        }
        public RoleBindingBuilder Builder()
        {
            return _builder;
        }
        public V1RoleBinding Run(RoleBindingBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new RoleBindingBuilder();
            return _client.CreateNamespacedRoleBinding(build.Build(), ns, dryRun);
        }
        public async Task<V1RoleBinding> RunAsync(RoleBindingBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new RoleBindingBuilder();
            return await _client.CreateNamespacedRoleBindingAsync(build.Build(), ns, dryRun);
        }
    }
}
