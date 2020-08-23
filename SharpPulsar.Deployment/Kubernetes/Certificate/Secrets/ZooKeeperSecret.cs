
namespace SharpPulsar.Deployment.Kubernetes.Certificate.Secrets
{
    internal class ZooKeeperSecret
    {
        private readonly Secret _secret;
        public ZooKeeperSecret(Secret secret)
        {
            _secret = secret;
        }

        public RunResult Run(string dryRun = default)
        {
            _secret.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Tls.ZooKeeper.CertName}", Values.Namespace)
                .Type("kubernetes.io/tls")
                .KeyValue("tls.crt", Values.CertificateSecrets.Zoo.Public)
                .KeyValue("tls.key", Values.CertificateSecrets.Zoo.Private);

            return _secret.Run(_secret.Builder(), Values.Namespace, dryRun);
        }
    }
}
