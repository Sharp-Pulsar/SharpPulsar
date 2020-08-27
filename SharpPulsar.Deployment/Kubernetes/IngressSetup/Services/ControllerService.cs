using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Services
{
    internal class ControllerService
    {
        private readonly Service _service;
        public ControllerService(Service service)
        {
            _service = service;
        }

        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata("ingress-nginx-controller", "ingress-nginx")
                .Labels(new Dictionary<string, string>
                {
                    {"helm.sh/chart", "ingress-nginx-2.11.1"},
                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/instance", "ingress-nginx"},
                    {"app.kubernetes.io/version", "0.34.1"},
                    {"app.kubernetes.io/managed-by", "Helm"},
                    {"app.kubernetes.io/component", "controller"}
                })
                .Selector(new Dictionary<string, string>()
                {

                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/instance", "ingress-nginx"},
                    {"app.kubernetes.io/component", "controller"}
                })
                .Type("LoadBalancer")
                .ExternalTrafficPolicy("Local")
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "http", TargetPort = "http", Port = 80},
                    new V1ServicePort{Name = "https", TargetPort = "https", Port = 443}
                });

            return _service.Run(_service.Builder(), "ingress-nginx", dryRun);
        }
    }
}
