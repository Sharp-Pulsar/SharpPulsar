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
            _service.Builder();
            if(Values.Tls.Enabled && tls.Enabled)
            {
                _service.Builder()
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "http", Port = ports["http"], Protocol = "TCP" },
                    new V1ServicePort{Name = "https", Port = ports["https"], Protocol = "TCP" },
                    new V1ServicePort{Name = "pulsarssl", Port = ports["pulsarssl"], Protocol = "TCP" }
                    //new V1ServicePort{Name = "kafkassl", TargetPort = Values.Ports.Kop["ssl"], Protocol = "TCP" }
                });
            }
            else
            {
                _service.Builder()
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "http", Port = ports["http"], Protocol = "TCP" },
                    new V1ServicePort{Name = "pulsar", Port = ports["pulsar"], Protocol = "TCP" }
                    //new V1ServicePort{Name = "kafkaPlainText", TargetPort = Values.Ports.Kop["kafkaplain"], Protocol = "TCP" }
                });
            }
            return this;
        }
        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Namespace}-tcp-ingress", Values.Namespace)
                .Annotations(new Dictionary<string, string> {
                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/part-of", "ingress-nginx"}
                })
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Proxy.Name }
                            })
                .Selector(new Dictionary<string, string>()
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Proxy.Name }
                            })
                .Type("LoadBalancer");
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
