using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class ClusterRoleBinding
    {
        private readonly IKubernetes _client;
        private ClusterRoleBindingBuilder _builder;
        public ClusterRoleBinding(IKubernetes client)
        {
            _client = client;
            _builder = new ClusterRoleBindingBuilder();
        }
        public ClusterRoleBindingBuilder Builder()
        {
            return _builder;
        }
        public RunResult Run(ClusterRoleBindingBuilder builder, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new ClusterRoleBindingBuilder();
                result.Response = _client.CreateClusterRoleBinding(build.Build(), dryRun);
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
        public async Task<RunResult> RunAsync(ClusterRoleBindingBuilder builder, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new ClusterRoleBindingBuilder();
                result.Response = await _client.CreateClusterRoleBindingAsync(build.Build(), dryRun);
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
