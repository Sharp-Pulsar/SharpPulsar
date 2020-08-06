using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core.Tokens;

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
            if(!Values.Persistence || !Values.ZooKeeper.Persistence)
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
        public static List<V1Volume> Bookie()
        {
            var vols = new List<V1Volume>();
            if(!Values.Persistence || !Values.BookKeeper.Persistence)
            {
                vols.Add(new V1Volume { Name = $"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}-journal", EmptyDir = new V1EmptyDirVolumeSource() });
                vols.Add(new V1Volume { Name = $"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}-ledger", EmptyDir = new V1EmptyDirVolumeSource() });
            }                
           
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                vols.Add(new V1Volume { Name = "bookie-certs", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-{Values.Tls.Bookie.CertName}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "tls.crt", Path = "tls.crt" }, new V1KeyToPath { Key = "tls.key", Path = "tls.key" } } } });
                vols.Add(new V1Volume { Name = "ca", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-ca-tls", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "ca.crt", Path = "ca.crt" } } } });
                vols.Add(new V1Volume { Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-keytool-configmap", DefaultMode = 0755 } });
            }
            return vols;
        }
        public static List<V1Volume> Broker()
        {
            var vols = new List<V1Volume>();
            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase) && !Values.Authentication.Vault)
            {
                var v = new V1Volume { Name = "token-keys" };
                if (Values.Authentication.UsingJwtSecretKey)
                    v.Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName }-token-symmetric-key" };
                else
                    v.Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName }-token-asymmetric-key" };
                if (Values.Authentication.UsingJwtSecretKey)
                    v.Secret.Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "SECRETKEY", Path = "token/secret.key" } };
                else
                    v.Secret.Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "PUBLICKEY", Path = "token/public.key" }, new V1KeyToPath { Key = "PRIVATEKEY", Path = "token/private.key" } };

                vols.Add(new V1Volume { Name = "broker-token", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-token-{Values.Authentication.Users.Broker}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "TOKEN", Path = "broker/token" } } } });
            }              
           
            if (Values.Tls.Enabled && (Values.Tls.Broker.Enabled || (Values.Tls.Bookie.Enabled || Values.Tls.ZooKeeper.Enabled)))
            {
                vols.Add(new V1Volume { Name = "broker-certs", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-{Values.Tls.Broker.CertName}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "tls.crt", Path = "tls.crt" }, new V1KeyToPath { Key = "tls.key", Path = "tls.key" } } } });
                vols.Add(new V1Volume { Name = "ca", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-ca-tls", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "ca.crt", Path = "ca.crt" } } } });
            }
            if(Values.Tls.ZooKeeper.Enabled /*|| .Values.components.kop*/)
                vols.Add(new V1Volume { Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-keytool-configmap", DefaultMode = 0755 } });
            
            if(Values.Broker.EnableFunctionCustomizerRuntime)
                vols.Add(new V1Volume { Name = $"{Values.ReleaseName}-{Values.Broker.ComponentName}-runtime", HostPath = new V1HostPathVolumeSource { Path = "/proc" } });
            
            if(Values.Broker.Offload.Gcs.Enabled)
                vols.Add(new V1Volume { Name = "gcs-offloader-service-acccount", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-gcs-offloader-service-account", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "gcs.json", Path = "gcs.json" } } } });

            return vols;
        }
        public static List<V1Volume> Toolset()
        {
            var vols = new List<V1Volume>();
            if (Values.Tls.Enabled && (Values.Tls.ZooKeeper.Enabled || Values.Tls.Broker.Enabled))
            {
                vols.Add(new V1Volume { Name = "toolset-certs", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-{Values.Tls.ToolSet.CertName}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "tls.crt", Path = "tls.crt" }, new V1KeyToPath { Key = "tls.key", Path = "tls.key" } } } });
                vols.Add(new V1Volume { Name = "ca", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-ca-tls", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "ca.crt", Path = "ca.crt" } } } });                
            }
            if (Values.Tls.ZooKeeper.Enabled || (Values.Tls.Broker.Enabled /*&& Values.components.kop*/))
            {
                vols.Add(new V1Volume { Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-keytool-configmap", DefaultMode = 0755 } });
            }
            return vols;
        }
    }
}
