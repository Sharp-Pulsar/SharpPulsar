
namespace SharpPulsar.Deployment.Kubernetes.Certificate.Secrets
{
    internal class BrokerSecret
    {
        private readonly Secret _secret;
        public BrokerSecret(Secret secret)
        {
            _secret = secret;
        }

        public RunResult Run(string dryRun = default)
        {
            _secret.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Tls.Broker.CertName}", Values.Namespace)
                .Type("kubernetes.io/tls")
                .KeyValue("tls.crt", Values.CertificateSecrets.Broker.Public)
                .KeyValue("tls.key", Values.CertificateSecrets.Broker.Private);

            return _secret.Run(_secret.Builder(), Values.Namespace, dryRun);
        }
    }
}
