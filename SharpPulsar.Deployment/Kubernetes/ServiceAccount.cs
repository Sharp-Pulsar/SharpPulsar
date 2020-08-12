using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class ServiceAccount
    {
        private readonly IKubernetes _client;
        private ServiceAccountBuilder _builder;
        public ServiceAccount(IKubernetes client)
        {
            _client = client;
            _builder = new ServiceAccountBuilder();
        }
        public ServiceAccountBuilder Builder()
        {
            return _builder;
        }
        public RunResult Run(ServiceAccountBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new ServiceAccountBuilder();
                result.Response = _client.CreateNamespacedServiceAccount(build.Build(), ns, dryRun);
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
        public async Task<RunResult> RunAsync(ServiceAccountBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new ServiceAccountBuilder();
                result.Response = await _client.CreateNamespacedServiceAccountAsync(build.Build(), ns, dryRun);
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
