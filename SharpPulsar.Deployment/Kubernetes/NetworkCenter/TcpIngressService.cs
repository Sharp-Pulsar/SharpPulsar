using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static SharpPulsar.Deployment.Kubernetes.Tls;

namespace SharpPulsar.Deployment.Kubernetes.NetworkCenter
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
        public TcpIngressService ConfigurePorts(ComponentTls tls, IDictionary<string, string> ports)
        {
            _service.Builder()
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "http", TargetPort = ports["http"], Protocol = "TCP" }
                });
            if(Values.Tls.Enabled && tls.Enabled)
            {
                _service.Builder()
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "https", TargetPort = ports["https"], Protocol = "TCP" },
                    new V1ServicePort{Name = "pulsarssl", TargetPort = ports["pulsarssl"], Protocol = "TCP" }
                    //new V1ServicePort{Name = "kafkassl", TargetPort = Values.Ports.Kop["ssl"], Protocol = "TCP" }
                });
            }
            else
            {
                _service.Builder()
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "pulsar", TargetPort = ports["pulsar"], Protocol = "TCP" }
                    //new V1ServicePort{Name = "kafkaPlainText", TargetPort = Values.Ports.Kop["kafkaplain"], Protocol = "TCP" }
                });
            }
            return this;
        }
        public V1Service Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Namespace}-tcp-ingress", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"app.kubernetes.io/name", "ingress-nginx"},
                                {"app.kubernetes.io/part-of", "ingress-nginx" }
                            })
                .Selector(new Dictionary<string, string>()
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"app.kubernetes.io/name", "ingress-nginx"},
                                {"app.kubernetes.io/part-of", "ingress-nginx" }
                            })
                .Type("LoadBalancer");
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
