using k8s.Models;
using System;
using System.Collections.Generic;

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
                vols.Add(new V1Volume{ Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-keytool-configmap", DefaultMode = Convert.ToInt32("0775", 8)/*to octal*/ } });

            return vols;
        }
        public static List<V1Volume> ZooKeeper()
        {
            var vols = new List<V1Volume>();
            if(!Values.Persistence && !Values.Settings.ZooKeeper.Persistence)
                vols.Add(new V1Volume { Name = $"{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}-data", EmptyDir = new V1EmptyDirVolumeSource() });
            else if((bool)Values.ExtraConfigs.ZooKeeper.Holder["UseSeparateDiskForTxlog"])
                vols.Add(new V1Volume { Name = $"{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}-dataLog", EmptyDir = new V1EmptyDirVolumeSource() });
            
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                vols.Add(new V1Volume { Name = "zookeeper-certs", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-{Values.Tls.ZooKeeper.CertName}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "tls.crt", Path = "tls.crt" }, new V1KeyToPath { Key = "tls.key", Path = "tls.key" } } } });
                vols.Add(new V1Volume { Name = "ca", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-ca-tls", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "ca.crt", Path = "ca.crt" } } } });
                vols.Add(new V1Volume { Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-keytool-configmap", DefaultMode = Convert.ToInt32("0775", 8)/*to octal*/ } });
            }
            vols.Add(new V1Volume
            {
                Name = $"{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}-genzkconf",
                ConfigMap = new V1ConfigMapVolumeSource
                {
                    Name = $"{Values.ReleaseName}-genzkconf-configmap",
                    DefaultMode = Convert.ToInt32("0775", 8) //to octal
                }
            });
            return vols;
        }
        public static List<V1Volume> Bookie()
        {
            var vols = new List<V1Volume>();
            if(!Values.Persistence || !Values.Settings.BookKeeper.Persistence)
            {
                vols.Add(new V1Volume { Name = $"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}-journal", EmptyDir = new V1EmptyDirVolumeSource() });
                vols.Add(new V1Volume { Name = $"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}-ledger", EmptyDir = new V1EmptyDirVolumeSource() });
            }                
           
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                vols.Add(new V1Volume { Name = "bookie-certs", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-{Values.Tls.Bookie.CertName}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "tls.crt", Path = "tls.crt" }, new V1KeyToPath { Key = "tls.key", Path = "tls.key" } } } });
                vols.Add(new V1Volume { Name = "ca", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-ca-tls", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "ca.crt", Path = "ca.crt" } } } });
                vols.Add(new V1Volume { Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-keytool-configmap", DefaultMode = Convert.ToInt32("0775", 8)/*to octal*/ } });
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
           
            if (Values.Tls.Enabled)
            {
                vols.Add(new V1Volume { Name = "broker-certs", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-{Values.Tls.Broker.CertName}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "tls.crt", Path = "tls.crt" }, new V1KeyToPath { Key = "tls.key", Path = "tls.key" } } } });
                vols.Add(new V1Volume { Name = "ca", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-ca-tls", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "ca.crt", Path = "ca.crt" } } } });
            }
            if(Values.Tls.ZooKeeper.Enabled /*|| .Values.components.kop*/)
                vols.Add(new V1Volume { Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-keytool-configmap", DefaultMode = Convert.ToInt32("0775", 8)/*to octal*/ } });
            
            if(Values.Settings.Broker.EnableFunctionCustomizerRuntime)
                vols.Add(new V1Volume { Name = $"{Values.ReleaseName}-{Values.Settings.Broker.Name}-runtime", HostPath = new V1HostPathVolumeSource { Path = "/proc" } });
            
            if(Values.Settings.Broker.Offload.Gcs.Enabled)
                vols.Add(new V1Volume { Name = "gcs-offloader-service-acccount", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-gcs-offloader-service-account", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "gcs.json", Path = "gcs.json" } } } });

            return vols;
        }
        public static List<V1Volume> Proxy()
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

                vols.Add(new V1Volume { Name = "proxy-token", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-token-{Values.Authentication.Users.Proxy}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "TOKEN", Path = "proxy/token" } } } });
            }              
           
            if (Values.Tls.Enabled && Values.Tls.Proxy.Enabled)
            {
                vols.Add(new V1Volume { Name = "proxy-certs", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-{Values.Tls.Proxy.CertName}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "tls.crt", Path = "tls.crt" }, new V1KeyToPath { Key = "tls.key", Path = "tls.key" } } } });
                vols.Add(new V1Volume { Name = "proxy-ca", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-ca-tls", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "ca.crt", Path = "ca.crt" } } } });
            }
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
            {
                vols.Add(new V1Volume { Name = "broker-ca", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-ca-tls", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "ca.crt", Path = "ca.crt" } } } });
            }
            return vols;
        }
        public static List<V1Volume> Prometheus()
        {
            var vols = new List<V1Volume> 
            { 
                new V1Volume{ Name = "config-volume", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-{Values.Settings.Prometheus.Name}"} }
            };
            foreach (var vm in Values.ConfigmapReloads.Prometheus.ExtraConfigmapMounts)
                vols.Add(new V1Volume { Name = $"{Values.ConfigmapReloads.Prometheus.Name}-{vm.Name}", ConfigMap = vm.ConfigMap });

            if (!Values.Persistence && !Values.Settings.Prometheus.Persistence)
                vols.Add(new V1Volume { Name = $"{Values.ReleaseName}-{Values.Settings.Prometheus.Name}-data", EmptyDir = new V1EmptyDirVolumeSource() });

            if (Values.Persistence && Values.Settings.Prometheus.Persistence)
                vols.Add(new V1Volume { Name = $"{Values.ReleaseName}-{Values.Settings.Prometheus.Name}-data", PersistentVolumeClaim = new V1PersistentVolumeClaimVolumeSource { ClaimName = $"{Values.ReleaseName}-{Values.Settings.Prometheus.Name}-data" } });

            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt"))
                vols.Add(new V1Volume { Name = "client-token", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-token-{Values.Authentication.Users.Client}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "TOKEN", Path = "client/token" } } } });
            return vols;
        }
        public static List<V1Volume> PrestoCoord()
        {
            var vols = new List<V1Volume>();
            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase))
            {
                vols.Add(new V1Volume { Name = "client-token", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName }-token-{Values.Authentication.Users.Client}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "TOKEN", Path = "client/token" } } } });
            }              
           
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
            {
                vols.Add(new V1Volume { Name = "ca", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-ca-tls", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "ca.crt", Path = "ca.crt" } } } });
            }
            vols.Add(new V1Volume { Name = "config-volume", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-{Values.Settings.PrestoCoord.Name}" } });
            return vols;
        }
        public static List<V1Volume> PrestoWorker()
        {
            var vols = new List<V1Volume>();
            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase))
            {
                vols.Add(new V1Volume { Name = "client-token", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName }-token-{Values.Authentication.Users.Client}", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "TOKEN", Path = "client/token" } } } });
            }              
           
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
            {
                vols.Add(new V1Volume { Name = "ca", Secret = new V1SecretVolumeSource { SecretName = $"{Values.ReleaseName}-ca-tls", Items = new List<V1KeyToPath> { new V1KeyToPath { Key = "ca.crt", Path = "ca.crt" } } } });
            }
            vols.Add(new V1Volume { Name = "config-volume", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-{Values.Settings.PrestoWorker.Name}" } });
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
                vols.Add(new V1Volume { Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource { Name = $"{Values.ReleaseName}-keytool-configmap", DefaultMode = Convert.ToInt32("0775", 8)/*to octal*/ } });
            }
            return vols;
        }
    }
}
