using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes
{
    public class StorageClass
    {
        private readonly IKubernetes _client;
        public StorageClass(IKubernetes client)
        {
            _client = client;
        }
        public StorageClassBuilder Builder()
        {
            return new StorageClassBuilder();
        }
        public V1StorageClass Run(string dryRun = default)
        {
            return _client.CreateStorageClass(Builder().Build(), dryRun);
        }
    }
    public class ZooKeeperDataLogStorageClass
    {
        private readonly IKubernetes _client;
        public ZooKeeperDataLogStorageClass(IKubernetes client)
        {
            _client = client;
        }
        public static StorageClassBuilder Builder()
        {
            return new StorageClassBuilder();
        }
        public V1StorageClass Run(string dryRun = default)
        {
            return _client.CreateStorageClass(Builder().Build(), dryRun);
        }
    }
}
