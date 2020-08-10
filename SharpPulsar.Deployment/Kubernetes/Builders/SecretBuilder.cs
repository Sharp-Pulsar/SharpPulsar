using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    internal class SecretBuilder
    {
        private V1Secret _secret;
        public SecretBuilder()
        {
            _secret = new V1Secret
            {
                Metadata = new V1ObjectMeta
                {

                },
                StringData = new Dictionary<string, string> { }
            };
        }
        public SecretBuilder Metadata(string name, string @namespace)
        {
            _secret.Metadata.Name = name;
            _secret.Metadata.NamespaceProperty = @namespace;
            return this;
        }
        public SecretBuilder KeyValue(string key, string value)
        {
            _secret.StringData[key] = value;
            return this;
        }
        public V1Secret Build()
        {
            return _secret;
        }
    }
}
