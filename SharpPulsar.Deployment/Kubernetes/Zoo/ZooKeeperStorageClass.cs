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
        public V1StorageClass Run(string dryRun = default)
        {
            _str.Builder().
                Metadata($"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}-data", Values.Namespace, new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.ZooKeeper.ComponentName }
                            })
                .Parameters(ZooTemp.StorageParameters)
                .Provisioner(ZooTemp.StorageProvisioner);
            return _str.Run(_str.Builder(), dryRun);
        }
    }
    public class ZooKeeperDataLogStorageClass
    {
        private readonly StorageClass _str;
        public ZooKeeperDataLogStorageClass(StorageClass cls)
        {
            _str = cls;
        }
        public V1StorageClass Run(string dryRun = default)
        {
            _str.Builder().
                Metadata($"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}-data-log", Values.Namespace, new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.ZooKeeper.ComponentName }
                            })
                .Parameters(ZooTemp.DatalogParameters)
                .Provisioner(ZooTemp.DatalogProvisioner);
            return _str.Run(_str.Builder(), dryRun);
        }
    }
}
