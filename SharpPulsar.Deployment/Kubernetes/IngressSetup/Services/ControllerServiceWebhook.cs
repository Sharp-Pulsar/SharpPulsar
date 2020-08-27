using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Services
{
    internal class ControllerServiceWebhook
    {
        private readonly Service _service;
        public ControllerServiceWebhook(Service service)
        {
            _service = service;
        }

        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata("ingress-nginx-controller-admission", "ingress-nginx")
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
                .Type("ClusterIP")
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "https-webhook", TargetPort = "webhook", Port = 443}
                });

            return _service.Run(_service.Builder(), "ingress-nginx", dryRun);
        }
    }
}
