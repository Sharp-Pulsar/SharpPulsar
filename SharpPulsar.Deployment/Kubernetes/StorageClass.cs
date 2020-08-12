using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class StorageClass
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
        public RunResult Run(StorageClassBuilder builder, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new StorageClassBuilder();
                result.Response = _client.CreateStorageClass(build.Build(), dryRun);
                result.Success = true;
            }
            catch (Microsoft.Rest.RestException ex)
            {
                if (ex is Microsoft.Rest.HttpOperationException e)
                    result.HttpOperationException = e;
                else
                    result.Exception = ex;
                result.Success = false;
            }
            return result;
        }
        public async Task<RunResult> RunAsync(StorageClassBuilder builder, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new StorageClassBuilder();
                result.Response = await _client.CreateStorageClassAsync(build.Build(), dryRun);
                result.Success = true;
            }
            catch (Microsoft.Rest.RestException ex)
            {
                if (ex is Microsoft.Rest.HttpOperationException e)
                    result.HttpOperationException = e;
                else
                    result.Exception = ex;
                result.Success = false;
            }
            return result;
        }
    }
}
