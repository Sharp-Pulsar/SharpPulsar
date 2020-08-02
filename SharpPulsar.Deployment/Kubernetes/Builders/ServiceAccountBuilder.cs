using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    public class ServiceAccountBuilder
    {
        private V1ServiceAccount _serviceAccount;
        public ServiceAccountBuilder()
        {
            _serviceAccount = new V1ServiceAccount
            {
                Metadata = new V1ObjectMeta()
            };
        }
        public ServiceAccountBuilder Metadata(string name, string @namespace)
        {
            _serviceAccount.Metadata.Name = name;
            _serviceAccount.Metadata.NamespaceProperty = @namespace;
            return this;
        }
        public ServiceAccountBuilder Labels(IDictionary<string, string> labels)
        {
            _serviceAccount.Metadata.Labels = labels;
            return this;
        }
        public V1ServiceAccount Build()
        {
            return _serviceAccount;
        }
    }
}
