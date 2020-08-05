using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public sealed class Ports
    {
        public static List<V1ContainerPort> BrokerPorts()
        {
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
            {
                return new List<V1ContainerPort>
                {
                    new V1ContainerPort{Name = "https", ContainerPort = Values.Ports.Broker["https"] },
                    new V1ContainerPort{Name = "pulsarssl", ContainerPort = Values.Ports.Broker["pulsarssl"] },
                };
            }
            return new List<V1ContainerPort>
                {
                    new V1ContainerPort{Name = "http", ContainerPort = Values.Ports.Broker["http"]},
                    new V1ContainerPort{Name = "pulsar", ContainerPort = Values.Ports.Broker["pulsar"] }
                };
        }
    }
}
