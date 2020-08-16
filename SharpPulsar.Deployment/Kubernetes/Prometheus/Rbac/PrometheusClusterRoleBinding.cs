
namespace SharpPulsar.Deployment.Kubernetes.Prometheus.Rbac
{
    internal class PrometheusClusterRoleBinding
    {
        private readonly ClusterRoleBinding _config;
        public PrometheusClusterRoleBinding(ClusterRoleBinding config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-{Values.Rbac.RoleNameBinding}")
                .RoleRef("rbac.authorization.k8s.io", "ClusterRole", $"{Values.ReleaseName}-{Values.Rbac.RoleName }")
                .AddSubject(Values.Namespace, "ServiceAccount", $"{Values.ReleaseName}-{Values.Rbac.RoleName }");
            return _config.Run(_config.Builder(), dryRun);
        }
    }
}
