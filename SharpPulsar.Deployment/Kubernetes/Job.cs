using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes
{
    internal class Job
    {
        private readonly IKubernetes _client;
        private JobBuilder _builder;
        public Job(IKubernetes client)
        {
            _client = client;
            _builder = new JobBuilder();
        }
        public JobBuilder Builder()
        {
            return _builder;
        }
        public V1Job Run(JobBuilder builder, string ns, string dryRun = default)
        {
            try
            {
                var build = builder;
                _builder = new JobBuilder();
                return _client.CreateNamespacedJob(build.Build(), ns, dryRun);
            }
            catch(Microsoft.Rest.ValidationException vEx)
            {
                throw new System.Exception(vEx.Details.ToString());
            }
            catch (Microsoft.Rest.HttpOperationException ex)
            {
                throw new System.Exception(ex.Response.Content);
            }
        }
        public async Task<V1Job> RunAsync(JobBuilder builder, string ns, string dryRun = default)
        {
            try
            {
                var build = builder;
                _builder = new JobBuilder();
                return await _client.CreateNamespacedJobAsync(build.Build(), ns, dryRun);
            }
            catch (Microsoft.Rest.ValidationException vEx)
            {
                throw new System.Exception(vEx.Details.ToString());
            }
            catch (Microsoft.Rest.HttpOperationException ex)
            {
                throw new System.Exception(ex.Response.Content);
            }
        }
    }
}
