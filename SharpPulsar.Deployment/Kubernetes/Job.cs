using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;
using System;
using System.IO;
using System.Text.Json;
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
        public RunResult Run(JobBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder.Build();
                File.AppendAllText($"{Guid.NewGuid()}.txt", JsonSerializer.Serialize(build, new JsonSerializerOptions { WriteIndented = true }));
                _builder = new JobBuilder();
                result.Response = _client.CreateNamespacedJob(build, ns, dryRun);
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
        public async Task<RunResult> RunAsync(JobBuilder builder, string ns, string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var build = builder;
                _builder = new JobBuilder();
                result.Response = await _client.CreateNamespacedJobAsync(build.Build(), ns, dryRun);
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
