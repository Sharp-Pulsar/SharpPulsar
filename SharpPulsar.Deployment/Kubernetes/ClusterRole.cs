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
        public RunResult Run(ClusterRoleBuilder builder, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new ClusterRoleBuilder();
                result.Response = _client.CreateClusterRole(build.Build(), dryRun);
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
        public async Task<RunResult> RunAsync(ClusterRoleBuilder builder, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new ClusterRoleBuilder();
                result.Response = await _client.CreateClusterRoleAsync(build.Build(), dryRun);
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
