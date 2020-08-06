﻿using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

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

            if (Values.Broker.EnableFunctionCustomizerRuntime)
                vols.Add(new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.Broker.ComponentName}-runtime", MountPath = $"/pulsar/{Values.Broker.PulsarFunctionsExtraClasspath}" });
            
            if (Values.Broker.Offload.Gcs.Enabled)
                vols.Add(new V1VolumeMount { Name = "gcs-offloader-service-acccount", MountPath = "/pulsar/srvaccts", ReadOnlyProperty = true });
            return vols;
        }
        public static List<V1VolumeMount> Proxy()
        {
            return new List<V1VolumeMount>();
        }
        public static List<V1VolumeMount> Presto()
        {
            return new List<V1VolumeMount>();
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
                new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}-journal", MountPath = "/pulsar/data/bookkeeper/journal" },
                new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}-ledger", MountPath = "/pulsar/data/bookkeeper/ledgers" }
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
            if (Values.Tls.Enabled || (Values.Tls.Broker.Enabled || (Values.Tls.Bookie.Enabled || Values.Tls.Bookie.Enabled)))
            {
                vols.Add(new V1VolumeMount { Name = "broker-certs", MountPath = "/pulsar/certs/broker", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });

            }
            if (Values.Tls.ZooKeeper.Enabled /* || Values.components.kop*/)
                vols.Add(new V1VolumeMount { Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath = "keytool.sh" });

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
            var useSeparateDiskForTxlog = (bool)Values.ZooKeeper.ExtraConfig.Holder["UseSeparateDiskForTxlog"];
            if (useSeparateDiskForTxlog)
            {
                vols.Add(new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}-data", MountPath = "/pulsar/data/zookeeper"});
                vols.Add(new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}-dataLog", MountPath = "/pulsar/data/zookeeper-datalog" });                
            }
            else
            {
                vols.Add(new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}-data", MountPath = "/pulsar/data" });
            }
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                vols.Add(new V1VolumeMount { Name = "zookeeper-certs", MountPath = "/pulsar/certs/zookeeper", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true });
                vols.Add(new V1VolumeMount { Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath = "keytool.sh" });
            }
            vols.Add(new V1VolumeMount { Name = $"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}-genzkconf", MountPath = "/pulsar/bin/gen-zk-conf.sh", SubPath = "gen-zk-conf.sh" });
            return vols;
        }
    }
}