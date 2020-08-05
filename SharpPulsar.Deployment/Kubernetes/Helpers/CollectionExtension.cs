using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Extensions
{
    public static class CollectionExtension
    {
        public static  List<V1ContainerPort> ContainerPorts(this List<V1ContainerPort> containerPorts, bool tls)
        {
            if (tls)
            {
                return new List<V1ContainerPort>
                {
                    new V1ContainerPort{Name = "https", ContainerPort = 8443 },
                    new V1ContainerPort{Name = "pulsarssl", ContainerPort = 6651 },
                };
            }
            return new List<V1ContainerPort>
                {
                    new V1ContainerPort{Name = "http", ContainerPort = 8080 },
                    new V1ContainerPort{Name = "pulsar", ContainerPort = 6650 }
                };
        }
        
    }
}
