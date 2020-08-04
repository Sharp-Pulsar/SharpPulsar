using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.Storage
{
    public class Ledger
    {
        private readonly StorageClass _str;
        public Ledger(StorageClass cls)
        {
            _str = cls;
        }
        public V1StorageClass Run(string dryRun = default)
        {
            _str.Builder().
                Metadata($"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}-ledger", Values.Namespace, new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.BookKeeper.ComponentName }
                            })
                .Parameters(new Dictionary<string, string>
                {

                })
                .Provisioner(Values.BookKeeper.StorageProvisioner);
            return _str.Run(_str.Builder(), dryRun);
        }
    }
}
