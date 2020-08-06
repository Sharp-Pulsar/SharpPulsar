using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    internal class ZooKeeperStorageClass
    {
        private readonly StorageClass _str;
        internal ZooKeeperStorageClass(StorageClass cls)
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
                .Parameters(Values.ZooKeeper.Storage.Parameters)
                .Provisioner(Values.ZooKeeper.Storage.Provisioner);
            return _str.Run(_str.Builder(), dryRun);
        }
    }
    internal class ZooKeeperDataLogStorageClass
    {
        private readonly StorageClass _str;
        internal ZooKeeperDataLogStorageClass(StorageClass cls)
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
                .Parameters(Values.ZooKeeper.Storage.Parameters)
                .Provisioner(Values.ZooKeeper.Storage.Provisioner);
            return _str.Run(_str.Builder(), dryRun);
        }
    }
}
