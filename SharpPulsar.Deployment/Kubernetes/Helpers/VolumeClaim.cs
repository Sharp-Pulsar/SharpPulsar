using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    internal class VolumeClaim
    {
        public static List<V1PersistentVolumeClaim> ZooKeeper()
        {
            if(Values.Persistence && Values.ZooKeeper.Persistence && !string.IsNullOrWhiteSpace(Values.ZooKeeper.Storage.ClassName))
            {
                var temp = new V1PersistentVolumeClaim
                {
                    Metadata = new V1ObjectMeta
                    {
                        Name = $"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}-data"
                    },
                    Spec = new V1PersistentVolumeClaimSpec
                    {
                        AccessModes = new[] { "ReadWriteOnce" },
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string, ResourceQuantity>
                            {
                                {"storage", new ResourceQuantity(Values.ZooKeeper.Storage.Size) }
                            }
                        },
                        StorageClassName = Values.ZooKeeper.Storage.ClassName
                    }
                };
                return new List<V1PersistentVolumeClaim>() { temp };
            }
            return new List<V1PersistentVolumeClaim>();
        }
        public static List<V1PersistentVolumeClaim> BookKeeper()
        {
            if(Values.Persistence && Values.BookKeeper.Persistence && !string.IsNullOrWhiteSpace(Values.BookKeeper.Storage.ClassName))
            {
                return new List<V1PersistentVolumeClaim>() 
                {
                    new V1PersistentVolumeClaim
                {
                    Metadata = new V1ObjectMeta{Name = $"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}-journal"},
                    Spec = new V1PersistentVolumeClaimSpec
                    {
                        AccessModes = new []{"ReadWriteOnce"},
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string,ResourceQuantity >{ { "storage", new ResourceQuantity(Values.BookKeeper.Storage.JournalSize) } }
                        },
                        StorageClassName = $"{Values.BookKeeper.Storage.ClassName}-journal"
                    }
                },
                new V1PersistentVolumeClaim
                {
                    Metadata = new V1ObjectMeta{Name =  $"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}-ledger"},
                    Spec = new V1PersistentVolumeClaimSpec
                    {
                        AccessModes = new []{"ReadWriteOnce"},
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string,ResourceQuantity >{ { "storage", new ResourceQuantity(Values.BookKeeper.Storage.LedgerSize) } }
                        },
                        StorageClassName = $"{Values.BookKeeper.Storage.ClassName}-ledger"
                    }
                }
                };
            }
            return new List<V1PersistentVolumeClaim>();
        }
    }
}
