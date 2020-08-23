
namespace SharpPulsar.Deployment.Kubernetes.Certificate.Secrets
{
    internal class AzureDnsSecret
    {
        private readonly Secret _secret;
        public AzureDnsSecret(Secret secret)
        {
            _secret = secret;
        }

        public RunResult Run(string dryRun = default)
        {
            _secret.Builder()
                .Metadata("secret-azuredns-config", Values.Namespace)
                .KeyValue("password", Values.CertificateSecrets.AzureDnsPassword);

            return _secret.Run(_secret.Builder(), Values.Namespace, dryRun);
        }
    }
}
