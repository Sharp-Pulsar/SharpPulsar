using k8s;
using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup
{
    internal class IngressNamespace
    {
        private readonly IKubernetes _client;
        public IngressNamespace(IKubernetes client)
        {
            _client = client;
        }
        public RunResult Run(string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                var ns = new V1Namespace
                {
                    Metadata = new V1ObjectMeta
                    {
                        Name = "ingress-nginx",
                        Labels = new Dictionary<string, string> 
                        {
                            {"app.kubernetes.io/name", "ingress-nginx"},
                            {"app.kubernetes.io/instance", "ingress-nginx"}
                        }
                    }
                };
                result.Response = _client.CreateNamespace(ns, dryRun);
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
