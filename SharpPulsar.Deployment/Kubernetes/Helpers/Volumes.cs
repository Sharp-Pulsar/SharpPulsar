using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class Volumes
    {
        public static List<V1Volume> Recovery()
        {
            var vols = new List<V1Volume>();
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                vols.Add(new V1Volume { Name = "autorecovery-certs", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-{Values.Tls.AutoRecovery.CertName}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "tls.crt", Path = "tls.crt" }, new V1KeyToPath { Key = "tls.key", Path = "tls.key" } } } });
                vols.Add(new V1Volume { Name = "ca", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-ca-tls", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "ca.crt", Path = "ca.crt" } } } });

            }
            if (Values.Tls.ZooKeeper.Enabled)
                vols.Add(new V1Volume{ Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-keytool-configmap", DefaultMode = 0755 }});

            return vols;
        }
        public static List<V1Volume> ZooKeeper()
        {
            var vols = new List<V1Volume>();
            if(!Values.Persistence && !Values.ZooKeeper.Persistence)
                vols.Add(new V1Volume { Name = $"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}-data", EmptyDir = new V1EmptyDirVolumeSource() });
            else if((bool)Values.ZooKeeper.ExtraConfig.Holder["UseSeparateDiskForTxlog"])
                vols.Add(new V1Volume { Name = $"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}-dataLog", EmptyDir = new V1EmptyDirVolumeSource() });
            
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                vols.Add(new V1Volume { Name = "zookeeper-certs", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-{Values.Tls.ZooKeeper.CertName}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "tls.crt", Path = "tls.crt" }, new V1KeyToPath { Key = "tls.key", Path = "tls.key" } } } });
                vols.Add(new V1Volume { Name = "ca", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-ca-tls", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "ca.crt", Path = "ca.crt" } } } });
                vols.Add(new V1Volume { Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-keytool-configmap", DefaultMode = 0755 } });
            }
            return vols;
        }
    }
}
