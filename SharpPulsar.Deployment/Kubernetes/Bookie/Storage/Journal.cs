using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.Storage
{
    internal class Journal
    {
        private readonly StorageClass _str;
        public Journal(StorageClass cls)
        {
            _str = cls;
        }
        public RunResult Run(string dryRun = default)
        {
            _str.Builder().
                Metadata($"{Values.Settings.BookKeeper.Storage.ClassName}-journal", Values.Namespace, new Dictionary<string, string>
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
