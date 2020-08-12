using k8s;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class ConfigMap
    {
        private readonly IKubernetes _client;
        private ConfigMapBuilder _builder;
        public ConfigMap(IKubernetes client)
        {
            _client = client;
            _builder = new ConfigMapBuilder();
        }
        public ConfigMapBuilder Builder()
        {
            return _builder;
        }
        public RunResult Run(ConfigMapBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new ConfigMapBuilder();
                result.Response = _client.CreateNamespacedConfigMap(build.Build(), ns, dryRun);
                result.Success = true;
            }
            catch(Microsoft.Rest.RestException ex)
            {
                if (ex is Microsoft.Rest.HttpOperationException e)
                    result.HttpOperationException = e;
                else
                    result.Exception = ex;
                result.Success = false;
            }
            return result;
        }
        public async Task<RunResult> RunAsync(ConfigMapBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new ConfigMapBuilder();
                result.Response = await _client.CreateNamespacedConfigMapAsync(build.Build(), ns, dryRun);
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
