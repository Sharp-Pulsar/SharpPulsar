using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class Role
    {
        private readonly IKubernetes _client;
        private RoleBuilder _builder;
        public Role(IKubernetes client)
        {
            _client = client;
            _builder = new RoleBuilder();
        }
        public RoleBuilder Builder()
        {
            return _builder;
        }
        public V1Role Run(RoleBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new RoleBuilder();
            return _client.CreateNamespacedRole(build.Build(), ns, dryRun);
        }
        public async Task<V1Role> RunAsync(RoleBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new RoleBuilder();
            return await _client.CreateNamespacedRoleAsync(build.Build(), ns, dryRun);
        }

    }
}
