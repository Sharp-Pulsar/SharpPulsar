
namespace SharpPulsar.Deployment.Kubernetes.ExternalDns.Rbac
{
    internal class ExternalDnsRole
    {
        private readonly ClusterRole _config;
        public ExternalDnsRole(ClusterRole config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name("external-dns")
                .AddRule(new[] { "" }, new[] { "services", "endpoints", "pods" }, new[] { "get", "watch", "list" })
                .AddRule(new[] { "extensions", "networking.k8s.io" }, new[] { "ingresses" }, new[] { "get", "watch", "list" })
                .AddRule(new[] { "" }, new[] { "nodes" }, new[] { "list" });
            return _config.Run(_config.Builder(), dryRun);
        }
    }
}
