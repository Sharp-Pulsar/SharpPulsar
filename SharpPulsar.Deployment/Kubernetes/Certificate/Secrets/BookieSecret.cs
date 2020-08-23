
namespace SharpPulsar.Deployment.Kubernetes.Certificate.Secrets
{
    internal class BookieSecret
    {
        private readonly Secret _secret;
        public BookieSecret(Secret secret)
        {
            _secret = secret;
        }

        public RunResult Run(string dryRun = default)
        {
            _secret.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Tls.Bookie.CertName}", Values.Namespace)
                .Type("kubernetes.io/tls")
                .KeyValue("tls.crt", Values.CertificateSecrets.Bookie.Public)
                .KeyValue("tls.key", Values.CertificateSecrets.Bookie.Private);

            return _secret.Run(_secret.Builder(), Values.Namespace, dryRun);
        }
    }
}
