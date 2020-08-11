using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.Storage
{
    internal class Ledger
    {
        private readonly StorageClass _str;
        public Ledger(StorageClass cls)
        {
            _str = cls;
        }
        public V1StorageClass Run(string dryRun = default)
        {
            _str.Builder().
                Metadata($"{Values.Settings.BookKeeper.Storage.ClassName}-ledger", Values.Namespace, new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.BookKeeper.Name }
                            })
                .Parameters(Values.Settings.BookKeeper.Storage.Parameters)
                .Provisioner(Values.Settings.BookKeeper.Storage.Provisioner);
            return _str.Run(_str.Builder(), dryRun);
        }
    }
}
