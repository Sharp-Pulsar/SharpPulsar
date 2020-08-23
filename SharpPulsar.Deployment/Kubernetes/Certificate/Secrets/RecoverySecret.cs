
namespace SharpPulsar.Deployment.Kubernetes.Certificate.Secrets
{
    internal class RecoverySecret
    {
        private readonly Secret _secret;
        public RecoverySecret(Secret secret)
        {
            _secret = secret;
        }

        public RunResult Run(string dryRun = default)
        {
            _secret.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Tls.AutoRecovery.CertName}", Values.Namespace)
                .Type("kubernetes.io/tls")
                .KeyValue("tls.crt", Values.CertificateSecrets.Recovery.Public)
                .KeyValue("tls.key", Values.CertificateSecrets.Recovery.Private);

            return _secret.Run(_secret.Builder(), Values.Namespace, dryRun);
        }
    }
}
