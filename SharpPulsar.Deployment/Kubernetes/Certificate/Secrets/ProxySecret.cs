
namespace SharpPulsar.Deployment.Kubernetes.Certificate.Secrets
{
    internal class ProxySecret
    {
        private readonly Secret _secret;
        public ProxySecret(Secret secret)
        {
            _secret = secret;
        }

        public RunResult Run(string dryRun = default)
        {
            _secret.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Tls.Proxy.CertName}", Values.Namespace)
                .Type("kubernetes.io/tls")
                .KeyValue("tls.crt", Values.CertificateSecrets.Proxy.Public)
                .KeyValue("tls.key", Values.CertificateSecrets.Proxy.Private);

            return _secret.Run(_secret.Builder(), Values.Namespace, dryRun);
        }
    }
}
