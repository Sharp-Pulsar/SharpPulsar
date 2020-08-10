using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    internal class StorageClassBuilder
    {
        private readonly V1StorageClass _zooStorage;
        public StorageClassBuilder()
        {
            _zooStorage = new V1StorageClass();
        }
        public StorageClassBuilder Metadata(string name, string @namespace, IDictionary<string, string> labels)
        {
            _zooStorage.Metadata = new V1ObjectMeta
            {
                Name = name,
                NamespaceProperty = @namespace,
                Labels = labels
            };
            return this;
        }
        public StorageClassBuilder Provisioner(string provisioner)
        {
            _zooStorage.Provisioner = provisioner;
            return this;
        }
        public StorageClassBuilder Parameters(IDictionary<string, string> parameters)
        {
            _zooStorage.Parameters = parameters;
            return this;
        }
        public V1StorageClass Build()
        {
            return _zooStorage;
        }
    }
    internal class DataLogStorageClass
    {
        private readonly V1StorageClass _zooStorage;
        public DataLogStorageClass()
        {
            _zooStorage = new V1StorageClass();
        }
        public DataLogStorageClass Metadata(string name, string @namespace, IDictionary<string, string> labels)
        {
            _zooStorage.Metadata = new V1ObjectMeta
            {
                Name = name,
                NamespaceProperty = @namespace,
                Labels = labels
            };
            return this;
        }
        public DataLogStorageClass Provisioner(string provisioner)
        {
            _zooStorage.Provisioner = provisioner;
            return this;
        }
        public DataLogStorageClass Parameters(IDictionary<string, string> parameters)
        {
            _zooStorage.Parameters = parameters;
            return this;
        }
        public V1StorageClass Build()
        {
            return _zooStorage;
        }
    }
}
