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
        public RunResult Run(SecretBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new SecretBuilder();
                result.Response = _client.CreateNamespacedSecret(build.Build(), ns, dryRun);
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
        public async Task<RunResult> RunAsync(SecretBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new SecretBuilder();
                result.Response = await _client.CreateNamespacedSecretAsync(build.Build(), ns, dryRun);
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
