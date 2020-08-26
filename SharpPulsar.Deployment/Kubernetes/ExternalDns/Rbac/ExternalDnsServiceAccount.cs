namespace SharpPulsar.Deployment.Kubernetes.ExternalDns.Rbac
{
    internal class ExternalDnsServiceAccount
    {
        private readonly ServiceAccount _serviceAccount;

        public ExternalDnsServiceAccount(ServiceAccount serviceAccount)
        {
            _serviceAccount = serviceAccount;
        }
        public RunResult Run(string dryRun = default)
        {
            _serviceAccount.Builder()
                .Metadata("external-dns", "cert-manager");
            return _serviceAccount.Run(_serviceAccount.Builder(), "cert-manager", dryRun);
        }
    }
}
