
using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class PodDisruptionBudget
    {
        private readonly IKubernetes _client;
        private PodDisruptionBudgetBuilder _builder;
        public PodDisruptionBudget(IKubernetes client)
        {
            _client = client;
            _builder = new PodDisruptionBudgetBuilder();
        }
        public PodDisruptionBudgetBuilder Builder()
        {
            return _builder;
        }
        public RunResult Run(PodDisruptionBudgetBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new PodDisruptionBudgetBuilder();
                result.Response = _client.CreateNamespacedPodDisruptionBudget(build.Build(), ns, dryRun);
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
        public async Task<RunResult> RunAsync(PodDisruptionBudgetBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new PodDisruptionBudgetBuilder();
                result.Response = await _client.CreateNamespacedPodDisruptionBudgetAsync(build.Build(), ns, dryRun);
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
