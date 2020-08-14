using k8s;
using k8s.Models;
using System.IO;
using YamlDotNet.Serialization;

namespace SharpPulsar.Deployment.Kubernetes.CertManager
{
    internal class InstallCertManagerCRD
    {
        private readonly IKubernetes _client;
        public InstallCertManagerCRD(IKubernetes client)
        {
            _client = client;
        }
        public RunResult Install(string dryRun)
        {
            //create RBAC before deploy
            var result = new RunResult();
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "cert-manager.yaml");
                var yaml = File.ReadAllText(path);
                var stringReader = new StringReader(yaml);
                var deserializer = new Deserializer();
                var crd = deserializer.Deserialize<V1CustomResourceDefinition>(stringReader);
                result.Response = _client.CreateCustomResourceDefinition(crd, dryRun);
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
