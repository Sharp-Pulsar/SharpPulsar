using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class StatefulSet
    {
        private readonly IKubernetes _client;
        private StatefulSetBuilder _builder;
        public StatefulSet(IKubernetes client)
        {
            _client = client;
            _builder = new StatefulSetBuilder();
        }
        public StatefulSetBuilder Builder()
        {
            return _builder;
        }
        public RunResult Run(StatefulSetBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new StatefulSetBuilder();
                result.Response = _client.CreateNamespacedStatefulSet(build.Build(), ns, dryRun);
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
        public async Task<RunResult> RunAsync(StatefulSetBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new StatefulSetBuilder();
                result.Response = await _client.CreateNamespacedStatefulSetAsync(build.Build(), ns, dryRun);
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
