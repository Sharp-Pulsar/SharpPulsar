using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class VolumeClaim
    {
        public static List<V1PersistentVolumeClaim> ZooKeeper()
        {
            if(Values.Persistence && Values.Settings.ZooKeeper.Persistence && !string.IsNullOrWhiteSpace(Values.Settings.ZooKeeper.Storage.ClassName))
            {
                var temp = new V1PersistentVolumeClaim
                {
                    Metadata = new V1ObjectMeta
                    {
                        Name = $"{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}-data"
                    },
                    Spec = new V1PersistentVolumeClaimSpec
                    {
                        AccessModes = new[] { "ReadWriteOnce" },
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string, ResourceQuantity>
                            {
                                {"storage", new ResourceQuantity(Values.Settings.ZooKeeper.Storage.Size) }
                            }
                        },
                        StorageClassName = Values.Settings.ZooKeeper.Storage.ClassName
                    }
                };
                return new List<V1PersistentVolumeClaim>() { temp };
            }
            return new List<V1PersistentVolumeClaim>();
        }
        public static List<V1PersistentVolumeClaim> BookKeeper()
        {
            if(Values.Persistence && Values.Settings.BookKeeper.Persistence && !string.IsNullOrWhiteSpace(Values.Settings.BookKeeper.Storage.ClassName))
            {
                return new List<V1PersistentVolumeClaim>() 
                {
                    new V1PersistentVolumeClaim
                {
                    Metadata = new V1ObjectMeta{Name = $"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}-journal"},
                    Spec = new V1PersistentVolumeClaimSpec
                    {
                        AccessModes = new []{"ReadWriteOnce"},
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string,ResourceQuantity >{ { "storage", new ResourceQuantity(Values.Settings.BookKeeper.Storage.JournalSize) } }
                        },
                        StorageClassName = $"{Values.Settings.BookKeeper.Storage.ClassName}-journal"
                    }
                },
                new V1PersistentVolumeClaim
                {
                    Metadata = new V1ObjectMeta{Name =  $"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}-ledger"},
                    Spec = new V1PersistentVolumeClaimSpec
                    {
                        AccessModes = new []{"ReadWriteOnce"},
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string,ResourceQuantity >{ { "storage", new ResourceQuantity(Values.Settings.BookKeeper.Storage.LedgerSize) } }
                        },
                        StorageClassName = $"{Values.Settings.BookKeeper.Storage.ClassName}-ledger"
                    }
                }
                };
            }
            return new List<V1PersistentVolumeClaim>();
        }
    }
}
