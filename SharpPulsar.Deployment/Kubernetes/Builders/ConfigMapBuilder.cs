using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    public class ConfigMapBuilder
    {
        private readonly V1ConfigMap _configMap;
        public ConfigMapBuilder()
        {
            _configMap = new V1ConfigMap
            {
                Metadata = new V1ObjectMeta()
            };
        }
        public ConfigMapBuilder Metadata(string name, string @namespace)
        {
            _configMap.Metadata.Name = name;
            _configMap.Metadata.NamespaceProperty = @namespace;
            return this;
        }
        public ConfigMapBuilder Labels(IDictionary<string, string> labels)
        {
            _configMap.Metadata.Labels = labels;
            return this;
        }
        public ConfigMapBuilder Data(IDictionary<string, string> data)
        {
            _configMap.Data = data;
            return this;
        }
        public V1ConfigMap Build()
        {
            return _configMap;
        }
    }
}
