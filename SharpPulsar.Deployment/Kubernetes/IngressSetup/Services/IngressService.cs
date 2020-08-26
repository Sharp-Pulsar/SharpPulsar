using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Services
{
    internal class IngressService
    {
        private readonly Service _service;
        public IngressService(Service service)
        {
            _service = service;
        }

        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata("ingress-nginx-controller", Values.Namespace)
                .Labels(new Dictionary<string, string>
                {
                    {"app", Values.App },
                    {"cluster", Values.Cluster },
                    {"release", Values.ReleaseName },
                    {"component", "nginx-ingress-controller" }

                })
                .Annotations(new Dictionary<string, string> 
                {
                    {"external-dns.alpha.kubernetes.io/hostname", $"monitor.{Values.ReleaseName}.{Values.DomainSuffix}" }
                })
                .Selector(new Dictionary<string, string>()
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component", "nginx-ingress-controller" }
                            })
                .Type("LoadBalancer")
                .ExternalTrafficPolicy("Local")
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "http", TargetPort = "http", Port = 80, Protocol = "TCP" },
                    new V1ServicePort{Name = "https", TargetPort = "https", Port = 443, Protocol = "TCP" }
                });

            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
