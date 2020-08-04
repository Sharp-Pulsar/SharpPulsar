using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes
{
    public static class Values
    {
        // Flag to control whether to run initialize job
        public static  bool Initialize { get; set; } = true;
        //Namespace to deploy pulsar
        public static string Namespace { get; set; } = "pulsar";
        public static string Cluster { get; set; } = "pulsar";
        public static string ReleaseName { get; set; } = "pulsar";
        public static string App { get; set; } = "pulsar";
        public static bool NamespaceCreate { get; set; } = false;
        //// Pulsar Metadata Prefix
        ////
        //// By default, pulsar stores all the metadata at root path.
        //// You can configure to have a prefix (e.g. "/my-pulsar-cluster").
        //// If you do so, all the pulsar and bookkeeper metadata will
        //// be stored under the provided path
        public static string MetadataPrefix { get; set; } = "";
        //// Monitoring Components
        ////
        //// Control what components of the monitoring stack to deploy for the cluster
        public static Monitoring Monitoring { get; set; } = new Monitoring();
        //// Images
        ////
        //// Control what images to use for each component
        public static Images Images { get; set; } = new Images();
        //// TLS
        //// templates/tls-certs.yaml
        ////
        //// The chart is using cert-manager for provisioning TLS certs for
        //// brokers and proxies.
        ///

        public static Component AutoRecovery { get; set; } = new Component
        {
            ComponentName = "recovery",
            Replicas = 1,
            ServiceName = $"{ReleaseName}-{AutoRecovery.ComponentName }",
            Enabled = true,
            UpdateStrategy = "RollingUpdate",
            PodManagementPolicy = "Parallel",
            ExtraInitContainers = new List<V1Container>
            {
                new V1Container
                    {
                        Name = "pulsar-bookkeeper-verify-clusterid",
                        Image = $"{Images.Autorecovery.Repository}:{Images.Autorecovery.Tag}",
                        ImagePullPolicy = Images.Autorecovery.PullPolicy,
                        Command = new []
                        { 
                            "sh", 
                            "-c" 
                        },
                        Args = new List<string>
                        {
                            "bin/apply-config-from-env.py conf/bookkeeper.conf;",
                            "until bin/bookkeeper shell whatisinstanceid; do  sleep 3; done; "
                        },
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-recovery"
                                }
                            }
                        },
                        VolumeMounts = new List<V1VolumeMount>
                        {
                            //{{- if and .Values.tls.enabled .Values.tls.zookeeper.enabled }}
                            // new V1VolumeMount{ Name = "autorecovery-certs", MountPath = "/pulsar/certs/autorecovery", ReadOnlyProperty = true},
                            // new V1VolumeMount{ Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true},
                            //{{- if .Values.tls.zookeeper.enabled }}
                            // new V1VolumeMount{ Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath = "keytool.sh"}
                        }
                    }
            },
            Containers = new List<V1Container>
            {
                new V1Container
                    {
                        Name = $"{ReleaseName}-{AutoRecovery.ComponentName}",
                        Image = $"{Images.Autorecovery.Repository}:{Images.Autorecovery.Tag}",
                        ImagePullPolicy = Images.Autorecovery.PullPolicy,
                        Resources = new V1ResourceRequirements
                        { 
                            Requests = new Dictionary<string, ResourceQuantity>
                            { 
                                { 
                                    "memory", new ResourceQuantity("64Mi") 
                                }, 
                                {
                                    "cpu", new ResourceQuantity("0.05")
                                } 
                            } 
                        },
                        Command = new []
                        { 
                            "sh",
                            "-c" 
                        },
                        Args = new List<string>
                        {
                            "bin/apply-config-from-env.py conf/bookkeeper.conf;",
                            //"/pulsar/keytool/keytool.sh autorecovery {{ template pulsar.autorecovery.hostname . }} true;",
                            "bin/bookkeeper autorecovery"
                        },
                        Ports = new List<V1ContainerPort>
                        {
                            new V1ContainerPort
                            {
                                Name = "http", 
                                ContainerPort = 8000 
                            }
                        },
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                { 
                                    Name = $"{ReleaseName}-{AutoRecovery.ComponentName}"
                                }
                            }
                        },
                        VolumeMounts = new List<V1VolumeMount>
                        {
                            new V1VolumeMount{Name = "pulsar-zookeeper-data", MountPath = "/pulsar/data"},
                            new V1VolumeMount{Name = "pulsar-zookeeper-genzkconf", MountPath = "/pulsar/bin/gen-zk-conf.sh", SubPath = "gen-zk-conf.sh"},
                            new V1VolumeMount{Name = "pulsar-zookeeper-log4j2", MountPath = "/pulsar/conf/log4j2.yaml", SubPath = "og4j2.yaml"},
                            /*new V1VolumeMount{Name = "zookeeper-certs", MountPath = "/pulsar/certs/zookeeper", ReadOnlyProperty = true},
                            new V1VolumeMount{Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true},
                            new V1VolumeMount{Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath= "keytool.sh"}*/
                        }
                    }
            },
            Volumes = new List<V1Volume>
            {
                /*new V1Volume {Name = "zookeeper-certs", Secret = new V1SecretVolumeSource{SecretName ="{{ .Release.Name }}-{{ .Values.tls.zookeeper.cert_name }}", Items = new List<V1KeyToPath>{ new V1KeyToPath {Key = "tls.crt", Path = "tls.crt" }, new V1KeyToPath { Key = "tls.key", Path = "tls.key" } } }},
                new V1Volume {Name = "ca", Secret = new V1SecretVolumeSource{SecretName ="{{ .Release.Name }}-ca-tls", Items = new List<V1KeyToPath>{ new V1KeyToPath {Key = "ca.crt", Path = "ca.crt" } } }},
                new V1Volume{Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource{Name = "{{ template pulsar.fullname . }}-keytool-configmap", DefaultMode = 0755}}*/
            },
            ConfigData = new Dictionary<string, string>
                        {
                            {"BOOKIE_MEM", "-Xms64m -Xmx64m"}
                        }
        };
        public static Component ZooKeeper { get; set; } = new Component
        {
            Enabled = true,
            Replicas = 3,
            ComponentName = "zookeeper",
            ServiceName = $"{ReleaseName}-{ZooKeeper.ComponentName }",
            UpdateStrategy = "RollingUpdate",
            PodManagementPolicy = "OrderedReady",
            Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{ZooKeeper.ComponentName }",
                        Image = $"{Images.ZooKeeper.Repository}:{Images.ZooKeeper.Tag}",
                        ImagePullPolicy = Images.ZooKeeper.PullPolicy,
                        Resources = new V1ResourceRequirements
                        { 
                            Requests = new Dictionary<string, ResourceQuantity>
                            { 
                                { 
                                    "memory", new ResourceQuantity("256Mi") 
                                }, 
                                { 
                                    "cpu", new ResourceQuantity("0.1") 
                                } 
                            } 
                        },
                        Command = new []
                        { 
                            "sh", 
                            "-c" 
                        },
                        Args = new List<string>
                        {
                            "bin/apply-config-from-env.py conf/zookeeper.conf;",
                            "bin/gen-zk-conf.sh conf/zookeeper.conf 0 participant;",
                            "cat conf/zookeeper.conf;",
                            "bin/pulsar zookeeper;"
                        },
                        Ports = new List<V1ContainerPort>
                        {
                            new V1ContainerPort{Name = "metrics", ContainerPort = 8000 },
                            new V1ContainerPort{Name = "client", ContainerPort = 2181 },
                            //new V1ContainerPort{Name = "client-tls", ContainerPort = 2281 },
                            new V1ContainerPort{Name = "follower", ContainerPort = 2888 },
                            new V1ContainerPort{Name = "leader-election", ContainerPort = 3888 }
                        },
                        Env = new List<V1EnvVar>
                        {
                            new V1EnvVar
                            { 
                                Name = "ZOOKEEPER_SERVERS", 
                                Value ="pulsar-zookeeper-0,pulsar-zookeeper-1,pulsar-zookeeper-2"
                            }
                        },
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                { 
                                    Name = $"{ReleaseName}-{ZooKeeper.ComponentName }"
                                }
                            }
                        },
                        ReadinessProbe = new V1Probe
                        {
                            Exec = new V1ExecAction
                            {
                                Command = new List<string>{ "bin/pulsar-zookeeper-ruok.sh" }
                            },
                            InitialDelaySeconds = 10,
                            FailureThreshold = 10,
                            PeriodSeconds = 30
                        },
                        LivenessProbe = new V1Probe
                        {
                            Exec = new V1ExecAction
                            {
                                Command = new List<string>{ "bin/pulsar-zookeeper-ruok.sh" }
                            },
                            InitialDelaySeconds = 10,
                            FailureThreshold = 10,
                            PeriodSeconds = 30
                        }/*,
                        StartupProbe = new V1Probe
                        {
                            Exec = new V1ExecAction
                            {
                                Command = new List<string>{ "bin/pulsar-zookeeper-ruok.sh" }
                            },
                            InitialDelaySeconds = 10,
                            FailureThreshold = 30,
                            PeriodSeconds = 30
                        }*/,
                        VolumeMounts = new List<V1VolumeMount>
                        {
                            new V1VolumeMount{Name = "pulsar-zookeeper-data", MountPath = "/pulsar/data"},
                            new V1VolumeMount{Name = "pulsar-zookeeper-genzkconf", MountPath = "/pulsar/bin/gen-zk-conf.sh", SubPath = "gen-zk-conf.sh"},
                            new V1VolumeMount{Name = "pulsar-zookeeper-log4j2", MountPath = "/pulsar/conf/log4j2.yaml", SubPath = "og4j2.yaml"},
                            /*new V1VolumeMount{Name = "zookeeper-certs", MountPath = "/pulsar/certs/zookeeper", ReadOnlyProperty = true},
                            new V1VolumeMount{Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true},
                            new V1VolumeMount{Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath= "keytool.sh"}*/
                        }
                    }
                },
            Volumes = new List<V1Volume>
            {
                /*new V1Volume {Name = "zookeeper-certs", Secret = new V1SecretVolumeSource{SecretName ="{{ .Release.Name }}-{{ .Values.tls.zookeeper.cert_name }}", Items = new List<V1KeyToPath>{ new V1KeyToPath {Key = "tls.crt", Path = "tls.crt" }, new V1KeyToPath { Key = "tls.key", Path = "tls.key" } } }},
                    new V1Volume {Name = "ca", Secret = new V1SecretVolumeSource{SecretName ="{{ .Release.Name }}-ca-tls", Items = new List<V1KeyToPath>{ new V1KeyToPath {Key = "ca.crt", Path = "ca.crt" } } }},
                    new V1Volume{Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource{Name = "{{ template pulsar.fullname . }}-keytool-configmap", DefaultMode = 0755}}*/
            },
            ConfigData = new Dictionary<string, string>
                        {
                            {"dataDir", "/pulsar/data/zookeeper" },
                            //{"PULSAR_PREFIX_dataLogDir", "/pulsar/data/zookeeper-datalog" },
                            {"PULSAR_PREFIX_serverCnxnFactory", "org.apache.zookeeper.server.NettyServerCnxnFactory"},
                            {"serverCnxnFactory", "org.apache.zookeeper.server.NettyServerCnxnFactory"},
                            //if tls enabled
                            //{"secureClientPort", "Values.zookeeper.ports.clientTls"},
                            //{"PULSAR_PREFIX_secureClientPort", "Values.zookeeper.ports.clientTls"},
                            //if reconfig enabled }}
                            //{"PULSAR_PREFIX_reconfigEnabled", "true"},
                            //{"PULSAR_PREFIX_quorumListenOnAllIPs", "true"},
                            {"PULSAR_PREFIX_peerType", "participant" },
                            {"PULSAR_MEM", "-Xms64m -Xmx128m"},
                            {"PULSAR_GC", "-XX:+UseG1GC -XX:MaxGCPauseMillis=10 -Dcom.sun.management.jmxremote -Djute.maxbuffer=10485760 -XX:+ParallelRefProcEnabled -XX:+UnlockExperimentalVMOptions -XX:+AggressiveOpts -XX:+DoEscapeAnalysis -XX:+DisableExplicitGC -XX:+PerfDisableSharedMem -Dzookeeper.forceSync=no" }
                        }
        };

    }
    public  sealed class Volume
    {
        public bool Persistence { get; set; } = true;
        // configure the components to use local persistent volume
        // the local provisioner should be installed prior to enable local persistent volume
        public  bool LocalStorage { get; set; } = false;
    }
    public  sealed class Components
    {
        // zookeeper
        public bool Zookeeper { get; set; } = true;
        // bookkeeper
        public bool Bookkeeper { get; set; } = true;
        // bookkeeper - autorecovery
        public bool Autorecovery { get; set; } = true;
        // broker
        public  bool Broker { get; set; } = true;
        // functions
        public bool Functions { get; set; } = true;
        // proxy
        public bool Proxy { get; set; } = true;
        // toolset
        public bool Toolset { get; set; } = false;
        // pulsar manager
        public bool PulsarManager { get; set; } = false;
        // pulsar sql
        public bool SqlWorker { get; set; } = true;
        // kop
        public bool Kop { get; set; } = false;
        // pulsar detector
        public bool PulsarDetector { get; set; } = false;
    }
    public  sealed class Monitoring
    {
        // monitoring - prometheus
        public bool Prometheus { get; set; } = false;
        // monitoring - grafana
        public bool Grafana { get; set; } = false;
        // monitoring - node_exporter
        public bool NodeExporter { get; set; } = false;
        // alerting - alert-manager
        public bool AlertManager { get; set; } = false;
        // monitoring - loki
        public bool Loki { get; set; } = false;
        // monitoring - datadog
        public bool Datadog { get; set; } = false;
    }
    public  sealed class Images 
    {
        public Image ZooKeeper { get; set; } = new Image();
        public Image Bookie { get; set; } = new Image();
        public Image Presto { get; set; } = new Image();
        public Image Autorecovery { get; set; } = new Image();
        public Image Broker { get; set; } = new Image();
        public Image Proxy { get; set; } = new Image();
        public Image PulsarDetector { get; set; } = new Image();
        public Image Functions { get; set; } = new Image();
        public Image Prometheus { get; set; } = new Image 
        { 
            Repository = "prom/prometheus",
            Tag = "v2.17.2"
        };
        public Image AlertManager { get; set; } = new Image
        {
            Repository = "prom/alertmanager",
            Tag = "v0.20.0"
        };
        public Image Grafana { get; set; } = new Image
        {
            Repository = "streamnative/apache-pulsar-grafana-dashboard-k8s",
            Tag = "0.0.8"
        };
        public Image PulsarManager { get; set; } = new Image
        {
            Repository = "streamnative/pulsar-manager",
            Tag = "0.3.0"
        };
        public Image NodeExporter { get; set; } = new Image
        {
            Repository = "prom/node-exporter",
            Tag = "0.16.0"
        };
        public Image NginxIngressController { get; set; } = new Image
        {
            Repository = "quay.io/kubernetes-ingress-controller/nginx-ingress-controller",
            Tag = "0.26.2"
        };
    }
    public  sealed class Image
    {
        public string ContainerName { get; set; }
        public string Repository { get; set; } = "apachepulsar/pulsar-all";
        public string Tag { get; set; } = "2.6.0";
        public string PullPolicy { get; set; } = "IfNotPresent";
        public bool HasCommand { get; set; } = false;
    }
    public class Common
    {
        public List<V1Container> ExtraInitContainers { get; set; }
    }
    public class Component
    {
        public bool Enabled { get; set; } = false;
        public string ComponentName { get; set; }
        public string ServiceName { get; set; }
        public string PodManagementPolicy { get; set; }
        public string UpdateStrategy { get; set; }
        public int GracePeriodSeconds { get; set; }
        public int Replicas { get; set; }
        public List<V1Container> ExtraInitContainers { get; set; } = new List<V1Container>();
        public List<V1PersistentVolumeClaim> PVC { get; set; } = new List<V1PersistentVolumeClaim>();
        public List<V1Volume> Volumes { get; set; } = new List<V1Volume>();
        public List<V1Container> Containers { get; set; } = new List<V1Container>();
        public List<V1Toleration> Tolerations { get; set; } = new List<V1Toleration>();
        public IDictionary<string, string> ConfigData { get; set; } = new Dictionary<string, string>();
    }
}
