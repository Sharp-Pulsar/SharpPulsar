using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class RoleBinding
    {
        private readonly IKubernetes _client;
        private RoleBindingBuilder _builder;
        public RoleBinding(IKubernetes client)
        {
            _client = client;
            _builder = new RoleBindingBuilder();
        }
        public RoleBindingBuilder Builder()
        {
            return _builder;
        }
        public RunResult Run(RoleBindingBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new RoleBindingBuilder();
                result.Response = _client.CreateNamespacedRoleBinding(build.Build(), ns, dryRun);
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
        public async Task<RunResult> RunAsync(RoleBindingBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new RoleBindingBuilder();
                result.Response = await _client.CreateNamespacedRoleBindingAsync(build.Build(), ns, dryRun);
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
