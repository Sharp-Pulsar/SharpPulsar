
namespace SharpPulsar.Deployment.Kubernetes.Certificate.Secrets
{
    internal class TlsCa
    {
        private readonly Secret _secret;
        public TlsCa(Secret secret)
        {
            _secret = secret;
        }

        public RunResult Run(string dryRun = default)
        {
            _secret.Builder()
                .Metadata($"{Values.ReleaseName}-ca-tls", Values.Namespace)
                .KeyValue("ca.crt", Values.CertificateSecrets.CertificateAuthority);

            return _secret.Run(_secret.Builder(), Values.Namespace, dryRun);
        }
    }
}
