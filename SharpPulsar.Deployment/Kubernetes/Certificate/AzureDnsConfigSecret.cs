using k8s.Models;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes.Certificate
{//https://cert-manager.io/docs/installation/kubernetes/
    internal class AzureDnsConfigSecret
    {
        private readonly Secret _secret;
        public AzureDnsConfigSecret(Secret secret)
        {

            _secret = secret;
        }
        //echo <service principal password> | openssl base64
        public RunResult Run(string password, string dryRun = default)
        {
            _secret.Builder()
                 .Metadata($"{Values.ReleaseName}-azure-dns-secret", Values.Namespace)
                 .KeyValue("password", password);
            return _secret.Run(_secret.Builder(), Values.Namespace, dryRun);
        }
        public async Task<RunResult> RunAsync(string password, string dryRun = default)
        {
            _secret.Builder()
                 .Metadata($"{Values.ReleaseName}-azure-dns-secret", Values.Namespace)
                 .KeyValue("password", password);
            return await _secret.RunAsync(_secret.Builder(), Values.Namespace, dryRun);
        }
    }
}
