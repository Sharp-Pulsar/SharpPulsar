using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class VolumeClaim
    {
        public static List<V1PersistentVolumeClaim> ZooKeeper()
        {
            if(Values.Persistence && Values.ZooKeeper.Persistence && !string.IsNullOrWhiteSpace(Values.ZooKeeper.StorageClassName))
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
                                {"storage", new ResourceQuantity(Values.ZooKeeper.StorageSize) }
                            }
                        },
                        StorageClassName = Values.ZooKeeper.StorageClassName
                    }
                };
                return new List<V1PersistentVolumeClaim>() { temp };
            }
            return new List<V1PersistentVolumeClaim>();
        }
        public static List<V1PersistentVolumeClaim> BookKeeper()
        {
            if(Values.Persistence && Values.BookKeeper.Persistence && !string.IsNullOrWhiteSpace(Values.BookKeeper.StorageClassName))
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
                            Requests = new Dictionary<string,ResourceQuantity >{ { "storage", new ResourceQuantity(Values.BookKeeper.JournalStorageSize) } }
                        },
                        StorageClassName = $"{Values.BookKeeper.StorageClassName}-journal"
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
                            Requests = new Dictionary<string,ResourceQuantity >{ { "storage", new ResourceQuantity(Values.BookKeeper.LedgerStorageSize) } }
                        },
                        StorageClassName = $"{Values.BookKeeper.StorageClassName}-ledger"
                    }
                }
                };
            }
            return new List<V1PersistentVolumeClaim>();
        }
    }
}
