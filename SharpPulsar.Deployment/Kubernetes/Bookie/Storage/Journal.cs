using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.Storage
{
    internal class Journal
    {
        private readonly StorageClass _str;
        public Journal(StorageClass cls)
        {
            _str = cls;
        }
        public V1StorageClass Run(string dryRun = default)
        {
            _str.Builder().
                Metadata($"{Values.BookKeeper.Storage.ClassName}-journal", Values.Namespace, new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.BookKeeper.ComponentName }
                            })
                .Parameters(Values.BookKeeper.Storage.Parameters)
                .Provisioner(Values.BookKeeper.Storage.Provisioner);
            return _str.Run(_str.Builder(), dryRun);
        }
    }
}
