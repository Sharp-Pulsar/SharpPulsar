using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    public class StorageClass
    {
        private readonly IKubernetes _client;
        private StorageClassBuilder _builder;
        public StorageClass(IKubernetes client)
        {
            _client = client;
            _builder = new StorageClassBuilder();
        }
        public StorageClassBuilder Builder()
        {
            return _builder;
        }
        public V1StorageClass Run(StorageClassBuilder builder, string dryRun = default)
        {
            var build = builder;
            _builder = new StorageClassBuilder();
            return _client.CreateStorageClass(build.Build(), dryRun);
        }
        public async Task<V1StorageClass> RunAsync(StorageClassBuilder builder, string dryRun = default)
        {
            var build = builder;
            _builder = new StorageClassBuilder();
            return await _client.CreateStorageClassAsync(build.Build(), dryRun);
        }
    }
}
