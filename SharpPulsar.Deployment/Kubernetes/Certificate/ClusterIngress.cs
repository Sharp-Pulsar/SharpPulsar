using k8s;
using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Certificate
{
    public class ClusterIngress
    {
        private IKubernetes _client;
        private Extensionsv1beta1Ingress _ingress;

        public ClusterIngress(IKubernetes client)
        {
            _client = client;
        }
    }
}
