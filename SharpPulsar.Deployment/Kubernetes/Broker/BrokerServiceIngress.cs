using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    internal class BrokerServiceIngress
    {
        private readonly Service _service;
        public BrokerServiceIngress(Service service)
        {
            _service = service;
        }

        public V1Service Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata($"{Values.Broker.ServiceName}-ingress", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Broker.ComponentName }
                            })
                .Annotations(new Dictionary<string, string>
                {
                    //{"publishNotReadyAddresses","true" }
                })
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "http", Port = 8080 },
                   // new V1ServicePort{Name = "https", Port = 8443 },
                    new V1ServicePort{Name = "pulsar", Port = 6650 }
                    //new V1ServicePort{Name = "pulsarssl", Port = 6651 }
                })
                .Type("LoadBalancer")
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Broker.ComponentName }
                            })
                .PublishNotReadyAddresses(true);
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
