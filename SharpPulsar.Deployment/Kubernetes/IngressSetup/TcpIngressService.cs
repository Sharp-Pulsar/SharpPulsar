using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static SharpPulsar.Deployment.Kubernetes.Tls;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup
{
    //https://kubernetes.github.io/ingress-nginx/user-guide/exposing-tcp-udp-services/
    internal class TcpIngressService
    {
        private readonly Service _service;
        /// <summary>
        /// Ingress for non Http ports like 6650, 6651
        /// </summary>
        /// <param name="service"></param>
        public TcpIngressService(Service service)
        {
            _service = service;
        }
        public TcpIngressService ConfigurePorts(ComponentTls tls, IDictionary<string, int> ports)
        {
            var port = new List<V1ServicePort> 
            { 
                new V1ServicePort{Name = "grafana", Port = Values.Ports.Grafana["http"], Protocol = "TCP"},
                new V1ServicePort{Name = "presto", Port = Values.Ports.PrestoCoordinator["http"], Protocol = "TCP"}
            };
            if(Values.Tls.Enabled && tls.Enabled)
            {
                port.AddRange(new[] 
                {   new V1ServicePort{Name = "http", Port = ports["http"], Protocol = "TCP" },
                    new V1ServicePort{Name = "https", Port = ports["https"], Protocol = "TCP" },
                    new V1ServicePort{Name = "pulsarssl", Port = ports["pulsarssl"], Protocol = "TCP" }
                });
            }
            else
            {
                port.AddRange(new[]
                {   new V1ServicePort{Name = "http", Port = ports["http"], Protocol = "TCP" },
                    new V1ServicePort{Name = "pulsar", Port = ports["pulsar"], Protocol = "TCP" }
                });
            }
            _service.Builder()
            .Ports(port);
            return this;
        }
        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata($"{Values.ReleaseName}-tcp-ingress", Values.Namespace)
                
                .Labels(new Dictionary<string, string>
                            {
                                {"app.kubernetes.io/name", "ingress-nginx"},
                                {"app.kubernetes.io/part-of", "ingress-nginx"}
                            })
                .Annotations(new Dictionary<string, string> {
                    {"external-dns.alpha.kubernetes.io/hostname",$"data.{Values.ReleaseName}.{Values.DomainSuffix}" }
                });
            //.Type("LoadBalancer")
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
