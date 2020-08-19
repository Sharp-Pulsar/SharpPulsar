using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.NetworkCenter.Rbac
{
    internal class CenterRole
    {
        private readonly Role _config;
        public CenterRole(Role config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}--nginx-ingress-role", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .AddRule(new[] { "" }, new[] { "configmaps", "pods", "secrets" }, new[] { "get" }, new[] {"" })
                .AddRule(new[] { "" }, new[] { "configmaps" }, new[] { "get", "update" }, new[] 
                {
                    //Defaults to "<election-id>-<ingress-class>"
                    // Here: "<ingress-controller-leader>-<nginx>"
                    // This has to be adapted if you change either parameter
                    // when launching the nginx-ingress-controller.
                    "ingress-controller-leader-nginx"
                })
                .AddRule(new[] { "" }, new[] { "configmaps" }, new[] { "create" }, new[] { ""})
                .AddRule(new[] { "" }, new[] { "endpoints" }, new[] { "get" }, new[] { "" });
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
