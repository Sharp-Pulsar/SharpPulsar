using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Proxy
{
    internal class ProxyService
    {
        private readonly Service _service;
        public ProxyService(Service service)
        {
            _service = service;
        }

        public V1Service Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata(Values.Proxy.ServiceName, Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Proxy.ComponentName }
                            })
                .Selector(new Dictionary<string, string>()
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Proxy.ComponentName }
                            });
            if (Values.Ingress.Proxy.Enabled)
            {
                _service.Builder()
                    .Type("ClusterIP")
                    .ClusterIp("None");
            }
            else
                _service.Builder()
                    .ClusterIp(Values.Ingress.Proxy.Type);
            if(!Values.Tls.Enabled || !Values.Tls.Proxy.Enabled)
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
