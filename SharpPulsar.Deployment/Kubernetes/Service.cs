using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class Service
    {
        private readonly IKubernetes _client;
        private ServiceBuilder _builder;
        public Service(IKubernetes client)
        {
            _client = client;
            _builder = new ServiceBuilder();
        }
        public ServiceBuilder Builder()
        {
            return _builder;
        }
        public RunResult Run(ServiceBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            var build = builder;
            try
            {
                _builder = new ServiceBuilder();
                result.Response = _client.CreateNamespacedService(build.Build(), ns, dryRun);
                result.Success = true;
                
            }
            catch (Microsoft.Rest.RestException ex)
            {
                if (ex is Microsoft.Rest.HttpOperationException e)
                {
                    //if (e.Response.Content.Contains("AlreadyExists"))
                        //return Patch(build.Build(), ns, dryRun);

                    result.HttpOperationException = e;
                }
                else
                    result.Exception = ex;
                result.Success = false;
            }
            return result;
        }
        public async Task<RunResult> RunAsync(ServiceBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            var build = builder;
            try
            {                
                _builder = new ServiceBuilder();
                result.Response = await _client.CreateNamespacedServiceAsync(build.Build(), ns, dryRun);
                result.Success = true;
            }
            catch (Microsoft.Rest.RestException ex)
            {
                if (ex is Microsoft.Rest.HttpOperationException e)
                {
                    if (e.Response.Content.Contains("AlreadyExists"))
                        return await PatchAsync(build.Build(), ns, dryRun);                    
                    result.HttpOperationException = e;
                }                    
                else
                    result.Exception = ex;
                result.Success = false;
            }
            return result;
        }
        private async Task<RunResult> PatchAsync(V1Service service, string ns, string dryRun)
        {
            var result = new RunResult();
            try
            {
                var patch = new V1Patch(service);
                result.Response = await _client.PatchNamespacedServiceAsync(patch, ns, dryRun);
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
        private RunResult Patch(V1Service service, string ns, string dryRun)
        {
            var result = new RunResult();
            try
            {
                var patch = new V1Patch(service);
                result.Response = _client.PatchNamespacedService(patch, ns, dryRun);
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
