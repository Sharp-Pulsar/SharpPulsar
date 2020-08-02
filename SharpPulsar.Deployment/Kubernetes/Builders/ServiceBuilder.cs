using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    public class ServiceBuilder
    {
        private V1Service _service;
        public ServiceBuilder()
        {
            _service = new V1Service
            {
                Metadata = new V1ObjectMeta(),
                Spec = new V1ServiceSpec()
            };
        }
        public ServiceBuilder Metadata(string name, string @namespace)
        {
            _service.Metadata.Name = name;
            _service.Metadata.NamespaceProperty = @namespace;
            return this;
        }
        public ServiceBuilder Labels(IDictionary<string, string> labels)
        {
            _service.Metadata.Labels = labels;
            return this;
        }
        public ServiceBuilder Annotations(IDictionary<string, string> annotation)
        {
            _service.Metadata.Annotations = annotation;
            return this;
        }
        public ServiceBuilder ClusterIp(string clusterIp)
        {
            _service.Spec.ClusterIP = clusterIp;
            return this;
        }
        public ServiceBuilder ExternalIPs(IList<string> ips)
        {
            _service.Spec.ExternalIPs = ips;
            return this;
        }
        public ServiceBuilder ExternalName(string externalName)
        {
            _service.Spec.ExternalName = externalName;
            return this;
        }
        public ServiceBuilder ExternalTrafficPolicy(string externalTrafficPolicy)
        {
            _service.Spec.ExternalTrafficPolicy = externalTrafficPolicy;
            return this;
        }
        public ServiceBuilder HealthCheckNodePort(int port)
        {
            _service.Spec.HealthCheckNodePort = port;
            return this;
        }
        public ServiceBuilder IpFamily(string ipFamily)
        {
            _service.Spec.IpFamily = ipFamily;
            return this;
        }
        public ServiceBuilder LoadBalancerIP(string loadBalancerIP)
        {
            _service.Spec.LoadBalancerIP = loadBalancerIP;
            return this;
        }
        public ServiceBuilder LoadBalancerSourceRanges(IList<string> ranges)
        {
            _service.Spec.LoadBalancerSourceRanges = ranges;
            return this;
        }
        public ServiceBuilder Ports(IList<V1ServicePort> ports)
        {
            _service.Spec.Ports = ports;
            return this;
        }
        public ServiceBuilder PublishNotReadyAddresses(bool publishNotReadyAddresses)
        {
            _service.Spec.PublishNotReadyAddresses = publishNotReadyAddresses;
            return this;
        }
        public ServiceBuilder Selector(IDictionary<string, string> selector)
        {
            _service.Spec.Selector = selector;
            return this;
        }
        public ServiceBuilder SessionAffinity(string sessionAffinity)
        {
            _service.Spec.SessionAffinity = sessionAffinity;
            return this;
        }
        public ServiceBuilder SessionAffinityConfig(V1SessionAffinityConfig sessionAffinityConfig)
        {
            _service.Spec.SessionAffinityConfig = sessionAffinityConfig;
            return this;
        }
        public ServiceBuilder TopologyKeys(IList<string> topologyKeys)
        {
            _service.Spec.TopologyKeys = topologyKeys;
            return this;
        }
        public ServiceBuilder Type(string type)
        {
            _service.Spec.Type = type;
            return this;
        }
        public V1Service Build()
        {
            return _service;
        }

    }
}
