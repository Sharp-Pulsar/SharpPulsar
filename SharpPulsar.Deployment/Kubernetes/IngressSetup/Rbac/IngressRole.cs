using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Rbac
{
    internal class IngressRole
    {
        private readonly Role _config;
        public IngressRole(Role config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name("ingress-nginx", "ingress-nginx")
                .Labels(new Dictionary<string, string>
                {
                    {"helm.sh/chart", "ingress-nginx-2.11.1"},
                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/instance", "ingress-nginx"},
                    {"app.kubernetes.io/version", "0.34.1"},
                    {"app.kubernetes.io/managed-by", "Helm"},
                    {"app.kubernetes.io/component", "controller"}
                })
                .AddRule(new[] { "" }, new[] { "namespaces" }, new[] { "get" }, new[] {"" })
                .AddRule(new[] { "" }, new[] { "configmaps", "pods", "secrets", "endpoints" }, new[] { "get", "list", "watch" }, new[] {"" })
                
                .AddRule(new[] { "" }, new[] { "services" }, new[] { "get", "list", "update", "watch" }, new[] {"" })

                .AddRule(new[] { "extensions", "networking.k8s.io" }, new[] { "ingresses" }, new[] { "get", "list", "watch" }, new[] {""})
                .AddRule(new[] { "extensions", "networking.k8s.io" }, new[] { "ingresses/status" }, new[] { "update" }, new[] {""})
                .AddRule(new[] { "networking.k8s.io" }, new[] { "ingressclasses" }, new[] { "get", "list", "watch" }, new[] {""})

                .AddRule(new[] { "" }, new[] { "configmaps" }, new[] { "get", "update" }, new[] {"ingress-controller-leader-nginx"})

                .AddRule(new[] { "" }, new[] { "configmaps" }, new[] { "create" }, new[] { ""})
                .AddRule(new[] { "" }, new[] { "endpoints" }, new[] { "create", "get", "update" }, new[] { "" })
                .AddRule(new[] { "" }, new[] { "events" }, new[] { "create", "patch" }, new[] { "" });
            return _config.Run(_config.Builder(), "ingress-nginx", dryRun);
        }
    }
}
