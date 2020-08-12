using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.Cluster
{
    internal class BookieClusterRoleBinding
    {
        private readonly ClusterRoleBinding _config;
        public BookieClusterRoleBinding(ClusterRoleBinding config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name }-clusterrolebinding")
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .RoleRef("rbac.authorization.k8s.io", "ClusterRole", $"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name }-clusterrole")
                .AddSubject(Values.Namespace, "ServiceAccount", $"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name }-acct");
            return _config.Run(_config.Builder(), dryRun);
        }
    }
}
