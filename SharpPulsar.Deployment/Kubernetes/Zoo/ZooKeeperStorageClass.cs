using k8s;
using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    public class ZooKeeperStorageClass
    {
        private readonly StorageClass _str;
        public ZooKeeperStorageClass(StorageClass cls)
        {
            _str = cls;
        }
        public V1StorageClass Run()
        {
            _str.Builder().
                Metadata($"{Values.ReleaseName}-zookeeper-data", Values.Namespace, new Dictionary<string, string>
                            {
                                {"app", Values.ReleaseName },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component","zookeeper" }
                            })
                .Parameters(ZooTemp.StorageParameters)
                .Provisioner(ZooTemp.StorageProvisioner);
            return _str.Run(Values.Namespace);
        }
    }
    public class ZooKeeperDataLogStorageClass
    {
        private readonly StorageClass _str;
        public ZooKeeperDataLogStorageClass(StorageClass cls)
        {
            _str = cls;
        }
        public V1StorageClass Run()
        {
            _str.Builder().
                Metadata($"{Values.ReleaseName}-zookeeper-data-log", Values.Namespace, new Dictionary<string, string>
                            {
                                {"app", Values.ReleaseName },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component","zookeeper" }
                            })
                .Parameters(ZooTemp.DatalogParameters)
                .Provisioner(ZooTemp.DatalogProvisioner);
            return _str.Run(Values.Namespace);
        }
    }
}
