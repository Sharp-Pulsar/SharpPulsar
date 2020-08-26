using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static SharpPulsar.Deployment.Kubernetes.Tls;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Services
{
    internal class PrestoIngressService
    {
        private readonly Service _service;
        /// <summary>
        /// Ingress for non Http ports like 6650, 6651
        /// </summary>
        /// <param name="service"></param>
        public PrestoIngressService(Service service)
        {
            _service = service;
        }
        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata($"{Values.ReleaseName}-presto-ingress", Values.Namespace)
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "presto", Port = Values.Ports.PrestoCoordinator["http"], Protocol = "TCP"}
                })
                .Annotations(new Dictionary<string, string>
                {
                    {"external-dns.alpha.kubernetes.io/hostname",$"presto.{Values.ReleaseName}.{Values.DomainSuffix}" },
                    {"kubernetes.io/ingress.class", "nginx" }
                })
            .Type("ClusterIP");
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
