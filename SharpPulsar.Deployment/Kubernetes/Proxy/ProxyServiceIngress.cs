using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Proxy
{
    public class ProxyServiceIngress
    {
        private readonly Service _service;
        public ProxyServiceIngress(Service service)
        {
            _service = service;
        }

        public V1Service Run(string dryRun = default)
        {
            var anat = new Dictionary<string, string>(Values.Ingress.Proxy.Annotations);
            if (!string.IsNullOrWhiteSpace(Values.Ingress.DomainSuffix))
                anat.Add("external-dns.alpha.kubernetes.io/hostname", $"data.{Values.ReleaseName}.{Values.Ingress.DomainSuffix}");

            _service.Builder()
                .Metadata(Values.Proxy.ServiceName + "-ingress", Values.Namespace)
                
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Proxy.ComponentName }
                            })
                .Selector(new Dictionary<string, string>(Values.Ingress.Proxy.ExtraSpec)
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Proxy.ComponentName }
                            })
                 .ClusterIp(Values.Ingress.Proxy.Type);
            if (!Values.Tls.Enabled || !Values.Tls.Proxy.Enabled)
            {
                _service.Builder()
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "http", Port = Values.Ports.Proxy["http"], Protocol = "TCP" },
                    new V1ServicePort{Name = "pulsar", Port = Values.Ports.Proxy["pulsar"], Protocol = "TCP"  }
                });
            }
            else
            {
                _service.Builder()
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "http", Port = Values.Ports.Proxy["https"], Protocol = "TCP" },
                    new V1ServicePort{Name = "pulsar", Port = Values.Ports.Proxy["pulsarssl"], Protocol = "TCP"  }
                });
            }
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
