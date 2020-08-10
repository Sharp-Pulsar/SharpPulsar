using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class Secret
    {
        private readonly IKubernetes _client;
        private SecretBuilder _builder;
        public Secret(IKubernetes client)
        {
            _client = client;
            _builder = new SecretBuilder();
        }
        public SecretBuilder Builder()
        {
            return _builder;
        }
        public V1Secret Run(SecretBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new SecretBuilder();
            return _client.CreateNamespacedSecret(build.Build(), ns, dryRun);
        }
        public async Task<V1Secret> RunAsync(SecretBuilder builder, string ns, string dryRun = default)
        {
            var build = builder;
            _builder = new SecretBuilder();
            return await _client.CreateNamespacedSecretAsync(build.Build(), ns, dryRun);
        }
    }
}
