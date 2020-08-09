using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class ClusterRole
    {
        private readonly IKubernetes _client;
        private ClusterRoleBuilder _builder;
        public ClusterRole(IKubernetes client)
        {
            _client = client;
            _builder = new ClusterRoleBuilder();
        }
        public ClusterRoleBuilder Builder()
        {
            return _builder;
        }
        public V1ClusterRole Run(ClusterRoleBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new ClusterRoleBuilder();
            return _client.CreateClusterRole(build.Build(), ns, dryRun);
        }
        public async Task<V1ClusterRole> RunAsync(ClusterRoleBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new ClusterRoleBuilder();
            return await _client.CreateClusterRoleAsync(build.Build(), ns, dryRun);
        }
    }
}
