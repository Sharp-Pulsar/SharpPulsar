using k8s.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class VolumeMounts
    {
        public static List<V1VolumeMount> Broker()
        {
            var vols = new List<V1VolumeMount>();
            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase) && !Values.Authentication.Vault)
            {
                vols.Add(new V1VolumeMount { Name = "token-keys", MountPath = "/pulsar/keys", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "broker-token", MountPath = "/pulsar/tokens", ReadOnlyProperty = true });
            }
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
            {
                vols.Add(new V1VolumeMount { Name = "broker-certs", MountPath = "/pulsar/certs/broker", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });
            }
            if (Values.Tls.ZooKeeper.Enabled)
                vols.Add(new V1VolumeMount { Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath = "keytool.sh" });

            if (Values.Settings.Broker.EnableFunctionCustomizerRuntime)
                vols.Add(new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.Settings.Broker.Name}-runtime", MountPath = $"/pulsar/{Values.Settings.Broker.PulsarFunctionsExtraClasspath}" });
            
            if (Values.Settings.Broker.Offload.Gcs.Enabled)
                vols.Add(new V1VolumeMount { Name = "gcs-offloader-service-acccount", MountPath = "/pulsar/srvaccts", ReadOnlyProperty = true });
            return vols;
        }
        public static List<V1VolumeMount> RecoveryIntContainer()
        {
            var vols = new List<V1VolumeMount>();
            if(Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                vols.Add(new V1VolumeMount { Name = "autorecovery-certs", MountPath = "/pulsar/certs/autorecovery", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });
                
            }
            if (Values.Tls.ZooKeeper.Enabled)
                vols.Add(new V1VolumeMount { Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath = "keytool.sh" });
            
            return vols;
        }
        public static List<V1VolumeMount> BookieIntContainer()
        {
            var vols = new List<V1VolumeMount>();
            if(Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                vols.Add(new V1VolumeMount { Name = "bookie-certs", MountPath = "/pulsar/certs/bookie", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });
                
            }
            if (Values.Tls.ZooKeeper.Enabled)
                vols.Add(new V1VolumeMount { Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath = "keytool.sh" });
            
            return vols;
        }
        public static List<V1VolumeMount> BookieContainer()
        {
            var vols = new List<V1VolumeMount>
            {
                new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}-journal", MountPath = "/pulsar/data/bookkeeper/journal" },
                new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}-ledger", MountPath = "/pulsar/data/bookkeeper/ledgers" }
            };
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                vols.Add(new V1VolumeMount { Name = "bookie-certs", MountPath = "/pulsar/certs/bookie", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });

            }
            if (Values.Tls.ZooKeeper.Enabled)
                vols.Add(new V1VolumeMount { Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath = "keytool.sh" });

            return vols;
        }
        public static List<V1VolumeMount> BrokerContainer()
        {
            var vols = new List<V1VolumeMount>();
            if (Values.Tls.Enabled || (Values.Tls.Broker.Enabled || Values.Tls.Bookie.Enabled))
            {
                vols.Add(new V1VolumeMount { Name = "broker-certs", MountPath = "/pulsar/certs/broker", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });

            }
            if (Values.Tls.ZooKeeper.Enabled /* || Values.components.kop*/)
                vols.Add(new V1VolumeMount { Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath = "keytool.sh" });

            return vols;
        }
        public static List<V1VolumeMount> ProxyContainer()
        {
            var vols = new List<V1VolumeMount>();
            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase) && !Values.Authentication.Vault)
            {
                vols.Add(new V1VolumeMount { Name = "token-keys", MountPath = "/pulsar/keys", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "proxy-token", MountPath = "/pulsar/tokens", ReadOnlyProperty = true });
            }
            if (Values.Tls.Enabled)
            {
                if(Values.Tls.Proxy.Enabled)
                {
                    vols.Add(new V1VolumeMount { Name = "proxy-certs", MountPath = "/pulsar/certs/proxy", ReadOnlyProperty = true });
                    vols.Add(new V1VolumeMount { Name = "proxy-ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });
                }
                if (Values.Tls.Broker.Enabled)
                {
                    vols.Add(new V1VolumeMount { Name = "broker-ca", MountPath = "/pulsar/certs/broker", ReadOnlyProperty = true });
                }
            }
            return vols;
        }

        public static List<V1VolumeMount> Toolset()
        {
            var vols = new List<V1VolumeMount>();
            if (Values.Tls.Enabled)
            {
                if (Values.Tls.ZooKeeper.Enabled || Values.Tls.Broker.Enabled)
                {
                    vols.Add(new V1VolumeMount { Name = "toolset-certs", MountPath = "/pulsar/certs/toolset", ReadOnlyProperty = true });
                    vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });
                    vols.Add(new V1VolumeMount { Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath = "keytool.sh" });

                }
                if (Values.Tls.Broker.Enabled || Values.Tls.Proxy.Enabled)
                {
                    vols.Add(new V1VolumeMount { Name = "proxy-ca", MountPath = "/pulsar/certs/proxy-ca", ReadOnlyProperty = true });
                }
                if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase) && !Values.Authentication.Vault)
                {
                    vols.Add(new V1VolumeMount { Name = "client-token", MountPath = "/pulsar/tokens", ReadOnlyProperty = true });
                }
            }
            return vols;
        }
        public static List<V1VolumeMount> PrometheusContainer()
        {
            var vols = new List<V1VolumeMount> 
            {
                new V1VolumeMount { Name = "config-volume", MountPath = "/etc/config"},
                new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.Settings.Prometheus.Name}-data", MountPath = "/prometheus"}
            };

            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt"))
                vols.Add(new V1VolumeMount { Name = "client-token", MountPath = "/pulsar/tokens", ReadOnlyProperty = true });

            return vols;
        }
        
        public static List<V1VolumeMount> PrometheusReloadContainer()
        {
            var vols = new List<V1VolumeMount> 
            {
                new V1VolumeMount { Name = "config-volume", MountPath = "/etc/config"}
            };
            foreach (var kv in Values.ConfigmapReloads.Prometheus.ExtraConfigmapMounts)
                vols.Add(new V1VolumeMount { Name = kv.Name, MountPath = kv.MountPath, SubPath = kv.SubPath, ReadOnlyProperty = kv.Readonly });

            return vols;
        }

        public static List<V1VolumeMount> PrestoCoordContainer()
        {
            var vols = new List<V1VolumeMount> 
            { 
                new V1VolumeMount{Name = "config-volume", MountPath ="/pulsar/conf/presto/node.properties", SubPath = "node.properties"},
                new V1VolumeMount{Name = "config-volume", MountPath ="/pulsar/conf/presto/log.properties", SubPath = "log.properties"},
                new V1VolumeMount{Name = "config-volume", MountPath ="/pulsar/conf/presto/jvm.config", SubPath = "jvm.config"},
                new V1VolumeMount{Name = "config-volume", MountPath ="/pulsar/conf/presto/config.properties", SubPath = "config.properties"},
                new V1VolumeMount{Name = "config-volume", MountPath ="/pulsar/conf/presto/catalog/pulsar.properties", SubPath = "pulsar.properties"},
            };
            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase))
            {
                vols.Add(new V1VolumeMount { Name = "client-token", MountPath = "/pulsar/tokens", ReadOnlyProperty = true });
            }
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
            {
                vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });
            }
            return vols;
        }
        public static List<V1VolumeMount> PrestoWorkerContainer()
        {
            var vols = new List<V1VolumeMount> 
            { 
                new V1VolumeMount{Name = "config-volume", MountPath ="/pulsar/conf/presto/node.properties", SubPath = "node.properties"},
                new V1VolumeMount{Name = "config-volume", MountPath ="/pulsar/conf/presto/log.properties", SubPath = "log.properties"},
                new V1VolumeMount{Name = "config-volume", MountPath ="/pulsar/conf/presto/jvm.config", SubPath = "jvm.config"},
                new V1VolumeMount{Name = "config-volume", MountPath ="/pulsar/conf/presto/config.properties", SubPath = "config.properties"},
                new V1VolumeMount{Name = "config-volume", MountPath ="/pulsar/conf/presto/catalog/pulsar.properties", SubPath = "pulsar.properties"},
                new V1VolumeMount{Name = "config-volume", MountPath ="/presto/health_check.sh", SubPath = "health_check.sh"},
            };
            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase))
            {
                vols.Add(new V1VolumeMount { Name = "client-token", MountPath = "/pulsar/tokens", ReadOnlyProperty = true });
            }
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
            {
                vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });
            }
            return vols;
        }
        public static List<V1VolumeMount> ToolsetVolumMount()
        {
            var vols = new List<V1VolumeMount>();
            if (Values.Tls.Enabled && (Values.Tls.ZooKeeper.Enabled || Values.Tls.Broker.Enabled))
            {
                vols.Add(new V1VolumeMount { Name = "toolset-certs", MountPath = "/pulsar/certs/toolset", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });

            }
            if (Values.Tls.ZooKeeper.Enabled || (Values.Tls.Broker.Enabled /*&&.Values.components.kop*/))
                vols.Add(new V1VolumeMount { Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath = "keytool.sh" });
            
            if (Values.Tls.ZooKeeper.Enabled || (Values.Tls.Broker.Enabled && Values.Tls.Proxy.Enabled))
                vols.Add(new V1VolumeMount { Name = "proxy-ca", MountPath = "/pulsar/certs/proxy-ca", ReadOnlyProperty = true });

            return vols;
        }
        public static List<V1VolumeMount> RecoveryContainer()
        {
            var vols = new List<V1VolumeMount>();
            if(Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                vols.Add(new V1VolumeMount { Name = "autorecovery-certs", MountPath = "/pulsar/certs/autorecovery", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });
                
            }
            if (Values.Tls.ZooKeeper.Enabled)
                vols.Add(new V1VolumeMount { Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath = "keytool.sh" });
            
            return vols;
        }

        public static List<V1VolumeMount> ZooKeeper()
        {
            var vols = new List<V1VolumeMount>();
            var useSeparateDiskForTxlog = (bool)Values.ExtraConfigs.ZooKeeper.Holder["UseSeparateDiskForTxlog"];
            if (useSeparateDiskForTxlog)
            {
                vols.Add(new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}-data", MountPath = "/pulsar/data/zookeeper"});
                vols.Add(new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}-dataLog", MountPath = "/pulsar/data/zookeeper-datalog" });                
            }
            else
            {
                vols.Add(new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}-data", MountPath = "/pulsar/data" });
            }
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                vols.Add(new V1VolumeMount { Name = "zookeeper-certs", MountPath = "/pulsar/certs/zookeeper", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath = "keytool.sh" });
            }
            vols.Add(new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}-genzkconf", MountPath = "/pulsar/bin/gen-zk-conf.sh", SubPath = "gen-zk-conf.sh" });
            return vols;
        }
    }
}
