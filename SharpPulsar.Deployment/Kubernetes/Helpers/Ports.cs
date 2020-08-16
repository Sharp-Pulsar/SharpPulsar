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
                    new V1ContainerPort{Name = "http", ContainerPort = Values.Ports.Broker["http"]},
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
        public static List<V1ContainerPort> AutoRecovery()
        {
            return new List<V1ContainerPort>
                {
                    new V1ContainerPort{Name = "http", ContainerPort = Values.Ports.AutoRecovery["http"]}
                };
        }
        public static List<V1ContainerPort> ZooKeeper()
        {
            var ports = new List<V1ContainerPort>();
            foreach(var p in Values.Ports.ZooKeeper)
            {
                if (p.Key.Equals("client-tls", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
                        ports.Add(new V1ContainerPort { Name = p.Key, ContainerPort = p.Value});
                }
                else
                {
                    ports.Add(new V1ContainerPort { Name = p.Key, ContainerPort = p.Value });
                }
            }
            return ports;
        }
        public static List<V1ContainerPort> BookKeeper()
        {
            var ports = new List<V1ContainerPort>();
            foreach (var p in Values.Ports.Bookie)
            {
                ports.Add(new V1ContainerPort { Name = p.Key, ContainerPort = p.Value });
            }
            return ports;
        }
        public static List<V1ContainerPort> Proxy()
        {
            var ports = new List<V1ContainerPort>
            {
                new V1ContainerPort { Name = "http", ContainerPort = Values.Ports.Proxy["http"] }
            };
            if (!Values.Tls.Enabled || !Values.Tls.Proxy.Enabled)
            {
                ports.Add(new V1ContainerPort { Name = "pulsar", ContainerPort = Values.Ports.Proxy["pulsar"] });
            }
            else
            {
                ports.Add(new V1ContainerPort { Name = "pulsarssl", ContainerPort = Values.Ports.Proxy["pulsarssl"] });
                ports.Add(new V1ContainerPort { Name = "https", ContainerPort = Values.Ports.Proxy["https"] });
            }
            return ports;
        }

        public static List<V1ContainerPort> Prometheus()
        {
            var ports = new List<V1ContainerPort>
            {
                new V1ContainerPort { Name = "server", ContainerPort = Values.Ports.Prometheus["http"] }
            };
            return ports;
        }

        public static List<V1ContainerPort> PrestoCoord()
        {
            var ports = new List<V1ContainerPort>
            {
                new V1ContainerPort { Name = "http-coord", ContainerPort = Values.Ports.PrestoCoordinator["http"], Protocol = "TCP" }
            };
            return ports;
        }
    }
}
