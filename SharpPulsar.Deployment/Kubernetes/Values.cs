using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Helpers;
using System;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes
{
    public static class Values
    {
        public static List<string> UserProvidedZookeepers { get; set; }
        public static bool Persistence { get; set; } = true;
        public static bool LocalStorage { get; set; } = false;
        public static bool AntiAffinity { get; set; } = false;
        // Flag to control whether to run initialize job
        public static  bool Initialize { get; set; } = true;
        public static string ConfigurationStore { get; set; }
        public static string ConfigurationStoreMetadataPrefix { get; set; }
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

        public static Tls Tls { get; set; } = new Tls();
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
        public static Authentication Authentication { get; set; } = new Authentication();

        public static Probes Probe { get; set; } = new Probes();

        public static Ports Ports { get; set; } = new Ports();
        public static Component AutoRecovery { get; set; } = new Component
        {
            ComponentName = "recovery",
            Replicas = 1,
            ServiceName = $"{ReleaseName}-{AutoRecovery.ComponentName }",
            Enabled = true,
            UpdateStrategy = "RollingUpdate",
            PodManagementPolicy = "Parallel",
            HostName = "${HOSTNAME}." + $"{AutoRecovery.ServiceName}.{Namespace}.svc.cluster.local",
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
                        Args = Args.AutoRecoveryIntContainer(),
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
                        VolumeMounts = VolumeMounts.RecoveryIntContainer()
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
                        Args = Args.AutoRecoveryContainer(),
                        Ports = Helpers.Ports.AutoRecovery(),
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
                        VolumeMounts = VolumeMounts.RecoveryContainer()
                    }
            },
            Volumes = Volumes.Recovery(),
            ConfigData = new Dictionary<string, string>{{"BOOKIE_MEM", "-Xms64m -Xmx64m"}}
        };
        public static Component ZooKeeper { get; set; } = new Component
        {
            Enabled = true,
            Replicas = 3,
            ComponentName = "zookeeper",
            ServiceName = $"{ReleaseName}-{ZooKeeper.ComponentName }",
            UpdateStrategy = "RollingUpdate",
            PodManagementPolicy = "OrderedReady",
            ResourcesRequest = new ResourcesRequest { Memory = "256Mi", Cpu = "0.1" },
            StorageClassName = $"{ReleaseName}-{ZooKeeper.ComponentName}-data",
            ZooConnect = Tls.ZooKeeper.Enabled ? $"{ZooKeeper.ServiceName}:2281" : $"{ZooKeeper.ServiceName}:2181",
            HostName = "${HOSTNAME}." + $"{ZooKeeper.ServiceName}.{Namespace}.svc.cluster.local",
            ExtraConfig = new ExtraConfig
            {
                Holder = new Dictionary<string, object>
                {
                    { "ZkServer", new List<string>{ } },
                    {"PeerType", "participant" },
                    {"InitialMyId", 0 },
                    {"UseSeparateDiskForTxlog", true },
                    {"Reconfig", true }
                }
            },
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
                                    "memory", new ResourceQuantity(ZooKeeper.ResourcesRequest.Memory) 
                                }, 
                                { 
                                    "cpu", new ResourceQuantity(ZooKeeper.ResourcesRequest.Cpu) 
                                } 
                            } 
                        },
                        Command = new []
                        { 
                            "sh", 
                            "-c" 
                        },
                        Args = Args.ZooKeeper(),
                        Ports = Helpers.Ports.ZooKeeper(),
                        Env = EnvVar.ZooKeeper(),
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
                        ReadinessProbe = Helpers.Probe.ExecActionReadiness(Probe.ZooKeeper, "bin/pulsar-zookeeper-ruok.sh"),
                        LivenessProbe = Helpers.Probe.ExecActionLiviness(Probe.ZooKeeper, "bin/pulsar-zookeeper-ruok.sh"),
                        StartupProbe = Helpers.Probe.ExecActionStartup(Probe.ZooKeeper, "bin/pulsar-zookeeper-ruok.sh"),
                        VolumeMounts = VolumeMounts.ZooKeeper()
                    }
                },
            Volumes = Volumes.ZooKeeper(),
            ConfigData = Config.ZooKeeper(),
            PVC = VolumeClaim.ZooKeeper()
        };
        public static Component BookKeeper { get; set; } = new Component
        {
            Enabled = true,
            Replicas = 3,
            ComponentName = "bookie",
            ServiceName = $"{ReleaseName}-{BookKeeper.ComponentName }",
            UpdateStrategy = "RollingUpdate",
            HostName = "${HOSTNAME}." + $"{BookKeeper.ServiceName}.{Namespace}.svc.cluster.local",
            PodManagementPolicy = "Parallel",
            LedgerStorageSize = "50Gi",
            JournalStorageSize = "10Gi",
            ResourcesRequest = new ResourcesRequest { Memory = "512Mi", Cpu = "0.2" },
            ExtraConfig = new ExtraConfig
            {
                ExtraInitContainers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = "wait-zookeeper-ready",
                        Image = $"{Images.Bookie.Repository}:{Images.Bookie.Tag}",
                        ImagePullPolicy = Images.Bookie.PullPolicy ,
                        Command = new []
                        {
                            "sh",
                            "-c"
                        },
                        Args = Args.WaitZooKeeperContainer()
                    }
                },
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{BookKeeper.ComponentName}-init",
                        Image = $"{Images.Bookie.Repository}:{Images.Bookie.Tag}",
                        ImagePullPolicy = Images.Bookie.PullPolicy,
                        Resources = new V1ResourceRequirements
                        {
                            /*Requests = new Dictionary<string, ResourceQuantity>
                            {
                                {
                                    "memory", new ResourceQuantity("4Gi")
                                },
                                {
                                    "cpu", new ResourceQuantity("2")
                                }
                            }*/
                        },
                        Command = new []
                        {
                            "sh",
                            "-c"
                        },
                        Args = Args.BookieExtraInitContainer(),
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{BookKeeper.ComponentName}"
                                }
                            }
                        }
                    }
                },
                Holder = new Dictionary<string, object>
                {
                    {"RackAware", true }
                }
            },
            ExtraInitContainers = new List<V1Container>
            {
                new V1Container
                {
                    Name = "pulsar-bookkeeper-verify-clusterid",
                    Image = $"{Images.Bookie.Repository}:{Images.Bookie.Tag}",
                    ImagePullPolicy = Images.Bookie.PullPolicy,
                    Command = new[]
                    {
                         "sh",
                         "-c"
                    },
                    Args = Args.BookieIntContainer(),
                    EnvFrom = new List<V1EnvFromSource>
                    {
                        new V1EnvFromSource
                        {
                            ConfigMapRef = new V1ConfigMapEnvSource
                            {
                                Name = $"{ReleaseName}-{BookKeeper.ComponentName}"
                            }
                        }
                    },
                    VolumeMounts = VolumeMounts.BookieIntContainer()
                }
            },
            Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{BookKeeper.ComponentName }",
                        Image = $"{Images.Bookie.Repository}:{Images.Bookie.Tag}",
                        ImagePullPolicy = Images.Bookie.PullPolicy,
                        Resources = new V1ResourceRequirements
                        { 
                            Requests = new Dictionary<string, ResourceQuantity>
                            { 
                                { 
                                    "memory", new ResourceQuantity(BookKeeper.ResourcesRequest.Memory) 
                                }, 
                                { 
                                    "cpu", new ResourceQuantity(BookKeeper.ResourcesRequest.Cpu) 
                                } 
                            } 
                        },
                        Command = new []
                        {
                            "bash", 
                            "-c" 
                        },
                        Args = Args.BookieContainer(),
                        Ports = Helpers.Ports.BookKeeper(),
                        Env = EnvVar.BookKeeper(),
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                { 
                                    Name = $"{ReleaseName}-{BookKeeper.ComponentName }"
                                }
                            }
                        },
                        ReadinessProbe = Helpers.Probe.HttpActionReadiness(Probe.Bookie, "/api/v1/bookie/is_ready", Ports.Bookie["http"]),
                        LivenessProbe = Helpers.Probe.HttpActionLiviness(Probe.Bookie, "/api/v1/bookie/state", Ports.Bookie["http"]),
                        StartupProbe = Helpers.Probe.HttpActionStartup(Probe.Bookie, "/api/v1/bookie/is_ready", Ports.Bookie["http"]),
                        VolumeMounts = VolumeMounts.BookieContainer()
                    }
                },
            Volumes = Volumes.Bookie(),
            ConfigData = Config.BookKeeper(),
            PVC = VolumeClaim.BookKeeper()
        };
        public static Component Broker { get; set; } = new Component
        {
            Enabled = true,
            Replicas = 3,
            ComponentName = "broker",
            ServiceName = $"{ReleaseName}-{Broker.ComponentName }",
            HostName = "${HOSTNAME}." + $"{Broker.ServiceName}.{Namespace}.svc.cluster.local",
            ZNode = $"{MetadataPrefix}/loadbalance/brokers/{Broker.HostName}:2181",
            UpdateStrategy = "RollingUpdate",
            PodManagementPolicy = "Parallel",
            ExtraInitContainers = new List<V1Container>
            {
                // This init container will wait for zookeeper to be ready before
                // deploying the bookies
                new V1Container
                {
                    Name = "wait-zookeeper-ready",
                    Image = $"{Images.Broker.Repository}:{Images.Broker.Tag}",
                    ImagePullPolicy = Images.Broker.PullPolicy,
                    Command = new []
                        {
                            "sh",
                            "-c"
                        },
                    Args = new List<string>
                    {
                        $"/pulsar/keytool/keytool.sh broker {Broker.HostName} true;",
                        //$@"until bin/bookkeeper org.apache.zookeeper.ZooKeeperMain -server {ConfigurationStore} get {ConfigurationStoreMetadataPrefix}/admin/clusters/""{Cluster}""; do",
                        $"until bin/bookkeeper org.apache.zookeeper.ZooKeeperMain -server {ZooKeeper.ZooConnect} get {MetadataPrefix}/admin/clusters/{Cluster}; do",
                        $"echo 'pulsar cluster {ReleaseName} isn't initialized yet ... check in 3 seconds ...' && sleep 3; done"
                    },
                    VolumeMounts = new List<V1VolumeMount>{}
                },
                //# This init container will wait for bookkeeper to be ready before
                //# deploying the broker
                new V1Container
                {
                    Name = "wait-bookkeeper-ready",
                    Image = $"{Images.Broker.Repository}:{Images.Broker.Tag}",
                    ImagePullPolicy = Images.Broker.PullPolicy,
                    Command = new []
                        {
                            "sh",
                            "-c"
                        },
                    Args = new List<string>
                    {
                        "bin/apply-config-from-env.py conf/bookkeeper.conf;",
                        $@"until bin/bookkeeper shell whatisinstanceid; do echo ""bookkeeper cluster is not initialized yet. backoff for 3 seconds ...""; sleep 3; done;",
                        "echo 'bookkeeper cluster is already initialized';",
                        $@"bookieServiceNumber=""$(nslookup -timeout=10 {ReleaseName}-{BookKeeper.ComponentName} | grep Name | wc -l)"";",
                        "until [ ${bookieServiceNumber} -ge"+ $" {Broker.ConfigData["managedLedgerDefaultEnsembleSize"]} ]; do"
                        + $@"echo ""bookkeeper cluster {ReleaseName} isn't ready yet ... check in 10 seconds ...""; sleep 10; done",
                        $@"bookieServiceNumber=""$(nslookup -timeout=10 {ReleaseName}-{Broker.ComponentName} | grep Name | wc -l)""; done",
                        @"echo ""bookkeeper cluster is ready"";"
                    }
                }
            },
            Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{Broker.ComponentName }",
                        Image = $"{Images.Broker.Repository}:{Images.Broker.Tag}",
                        ImagePullPolicy = Images.Broker.PullPolicy,
                        Resources = new V1ResourceRequirements
                        { 
                            Requests = new Dictionary<string, ResourceQuantity>
                            { 
                                { 
                                    "memory", new ResourceQuantity("512Mi") 
                                }, 
                                { 
                                    "cpu", new ResourceQuantity("0.2") 
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
                            "bin/apply-config-from-env.py conf/broker.conf;",
                            "bin/gen-yml-from-env.py conf/functions_worker.yml;",
                            @"echo ""OK"" > status;",
                            $"bin/pulsar zookeeper-shell -server {ZooKeeper.ZooConnect} get {Broker.ZNode};",
                            "while [ $? -eq 0 ]; do"
                            +$@"echo ""broker {Broker.HostName} znode still exists ... check in 10 seconds ..."";"
                              +"sleep 10;"
                            +$"bin/pulsar zookeeper-shell -server {ZooKeeper.ZooConnect} get {Broker.ZNode};"
                             +"done;"
                             +"bin/pulsar broker;"
                        },
                        Ports = Helpers.Ports.BrokerPorts(),
                        Env = EnvVar.Broker(advertisedPodIP:false),
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                { 
                                    Name = $"{ReleaseName}-{Broker.ComponentName }"
                                }
                            }
                        },
                        ReadinessProbe = Helpers.Probe.HttpActionReadiness(Probe.Broker, "/status.html", 8080),
                        LivenessProbe = Helpers.Probe.HttpActionLiviness(Probe.Broker, "/status.html", 8080),
                        StartupProbe = Helpers.Probe.HttpActionStartup(Probe.Broker, "/status.html", 8080),
                        VolumeMounts = VolumeMounts.Broker(enableAuth: false, authProvider:"jwt", authVault: false, tls: false, tlsZoo:false)
                    }
                },
            Volumes = new List<V1Volume>
            {
                //new V1Volume{ Name = $"{ReleaseName}-{BookKeeper.ComponentName}-journal", EmptyDir = new V1EmptyDirVolumeSource{ } },
                //new V1Volume{ Name = $"{ReleaseName}-{BookKeeper.ComponentName}-ledger", EmptyDir = new V1EmptyDirVolumeSource{ } },
                /*//{{- if and .Values.tls.enabled (or .Values.tls.bookie.enabled .Values.tls.zookeeper.enabled) }}
                            new V1VolumeMount
                            {
                                Name = "bookie-certs",
                                MountPath = "/pulsar/certs/bookie",
                                ReadOnlyProperty = true
                            },
                            new V1VolumeMount
                            {
                                Name = "ca",
                                MountPath = "/pulsar/certs/ca",
                                ReadOnlyProperty = true
                            },
                            //{{- if .Values.tls.zookeeper.enabled }}
                            new V1VolumeMount
                            {
                                Name = "keytool",
                                MountPath = "/pulsar/keytool/keytool.sh",
                                SubPath = "keytool.sh"
                            }*/
            },
            ConfigData = new Dictionary<string, string>
                        {
                            {"zkServers", $"{ZooKeeper.ServiceName}:2181" },
                            //{"zkServers", $"{ZooKeeper.ServiceName}:2281" },
                            {"zkLedgersRootPath", $"{MetadataPrefix}/ledgers" },
                            {"httpServerEnabled", "true" },
                            {"httpServerPort", "8000" },
                            {"statsProviderClass", "org.apache.bookkeeper.stats.prometheus.PrometheusMetricsProvider" },
                            {"useHostNameAsBookieID", "true" },
                            //disable auto recovery on bookies since we will start AutoRecovery in separated pods
                            //{"autoRecoveryDaemonEnabled", "false" },
                            //Do not retain journal files as it increase the disk utilization
                            {"journalMaxBackups", "0"},
                            {"journalDirectories", "/pulsar/data/bookkeeper/journal"},
                            {"PULSAR_PREFIX_journalDirectories", "/pulsar/data/bookkeeper/journal"},
                            {"ledgerDirectories", "/pulsar/data/bookkeeper/ledgers"},
                            /*{"PULSAR_PREFIX_tlsProviderFactoryClass", "org.apache.bookkeeper.tls.TLSContextFactory"},
                            {"PULSAR_PREFIX_tlsCertificatePath", "/pulsar/certs/bookie/tls.crt"},
                            {"PULSAR_PREFIX_tlsKeyStoreType", "PEM"},
                            {"PULSAR_PREFIX_tlsKeyStore", "/pulsar/certs/bookie/tls.key"},
                            {"PULSAR_PREFIX_tlsTrustStoreType", "PEM"},
                            {"PULSAR_PREFIX_tlsTrustStore", "/pulsar/certs/ca/ca.crt"}*/
                            {"BOOKIE_MEM", "-Xms128m -Xmx256m -XX:MaxDirectMemorySize=256m"},
                            {"PULSAR_MEM", "-Xms128m -Xmx256m -XX:MaxDirectMemorySize=256m"},
                            {"PULSAR_GC", "-XX:+UseG1GC -XX:MaxGCPauseMillis=10 -XX:+ParallelRefProcEnabled -XX:+UnlockExperimentalVMOptions -XX:+AggressiveOpts -XX:+DoEscapeAnalysis -XX:ParallelGCThreads=4 -XX:ConcGCThreads=4 -XX:G1NewSizePercent=50 -XX:+DisableExplicitGC -XX:-ResizePLAB -XX:+ExitOnOutOfMemoryError -XX:+PerfDisableSharedMem -XX:+PrintGCDetails -XX:+PrintGCTimeStamps -XX:+PrintGCApplicationStoppedTime -XX:+PrintHeapAtGC -verbosegc -Xloggc:/var/log/bookie-gc.log -XX:G1LogLevel=finest" }
                        },
            ExtraConfig = new ExtraConfig
            {

                //probably replace this with watch
                ExtraInitContainers = new List<V1Container>
                {
                },
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{BookKeeper.ComponentName}-init",
                        Image = $"{Images.Bookie.Repository}:{Images.Bookie.Tag}",
                        ImagePullPolicy = Images.Bookie.PullPolicy,
                        Resources = new V1ResourceRequirements
                        {
                            /*Requests = new Dictionary<string, ResourceQuantity>
                            {
                                {
                                    "memory", new ResourceQuantity("4Gi")
                                },
                                {
                                    "cpu", new ResourceQuantity("2")
                                }
                            }*/
                        },
                        Command = new []
                        {
                            "sh",
                            "-c"
                        },
                        Args = new List<string>
                        {
                            "bin/apply-config-from-env.py conf/bookkeeper.conf;",
                            //"/pulsar/keytool/keytool.sh toolset {{ template pulsar.toolset.hostname . }} true;",
                            //"if bin/bookkeeper shell whatisinstanceid; then echo ""bookkeeper cluster already initialized"";  else {{- if not (eq .Values.metadataPrefix "") }}  bin/bookkeeper org.apache.zookeeper.ZooKeeperMain -server {{ pulsar.fullname . }} -{{ .Values.zookeeper.component } } create {{ .Values.metadataPrefix }    } 'created for pulsar cluster "{{ template "pulsar.fullname" . }}"' || yes &&   {{- end   }}bin/bookkeeper shell initnewcluster;        fi"";
                        }
                    }
                }
            },
            PVC = new List<V1PersistentVolumeClaim>
            {
                new V1PersistentVolumeClaim
                {
                    Metadata = new V1ObjectMeta{Name = $"{ReleaseName}-{BookKeeper.ComponentName}-journal"},
                    Spec = new V1PersistentVolumeClaimSpec
                    { 
                        AccessModes = new []{"ReadWriteOnce"},
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string,ResourceQuantity >{ { "storage", new ResourceQuantity("10Gi") } }
                        },
                        StorageClassName = $"{ReleaseName}-{BookKeeper.ComponentName}-journal"
                        //StorageClassName = "local-storage"
                    }
                },
                new V1PersistentVolumeClaim
                {
                    Metadata = new V1ObjectMeta{Name =  $"{ReleaseName}-{BookKeeper.ComponentName}-ledger"},
                    Spec = new V1PersistentVolumeClaimSpec
                    { 
                        AccessModes = new []{"ReadWriteOnce"},
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string,ResourceQuantity >{ { "storage", new ResourceQuantity("50Gi") } }
                        },
                        StorageClassName = $"{ReleaseName}-{BookKeeper.ComponentName}-ledger"
                        //StorageClassName = "local-storage"
                    }
                }
                
            }
        };
        
    }
    public sealed class Ports
    {
        public IDictionary<string, int> Broker { get; set; } = new Dictionary<string, int>
        {
            {"http", 8080},
            {"https", 8443},
            {"pulsar", 6650},
            {"pulsarssl", 6651}
        };
        public IDictionary<string, int> Bookie { get; set; } = new Dictionary<string, int>
        {
            {"http", 8000},
            {"bookie", 3181}
        };
        public IDictionary<string, int> AutoRecovery { get; set; } = new Dictionary<string, int>
        {
            {"http", 8000}
        };
        public IDictionary<string, int> ZooKeeper { get; set; } = new Dictionary<string, int>
        {
            {"metrics", 8000},
            {"client", 2181},
            {"clientTls", 2281},
            {"follower", 2888},
            {"leaderElection", 3888}
        };
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
        public sealed class Image
        {
            public string ContainerName { get; set; }
            public string Repository { get; set; } = "apachepulsar/pulsar-all";
            public string Tag { get; set; } = "2.6.0";
            public string PullPolicy { get; set; } = "IfNotPresent";
            public bool HasCommand { get; set; } = false;
        }
    }   
    public sealed class Tls
    {
        public bool Enabled { get; set; } = false;
        //90 days
        public string Duration { get; set; } = "2160h";
        //15 days
        public string RenewBefore { get; set; } = "360h";
        public string Organization { get; set; } = "pulsar";
        public int KeySize { get; set; } = 4096;
        public string KeyAlgorithm { get; set; } = "rsa";
        public string KeyEncoding { get; set; } = "pkcs8";
        public ComponentTls ZooKeeper { get; set; } = new ComponentTls
        {
            CertName = "tls-zookeeper"
        };
        public ComponentTls Proxy { get; set; } = new ComponentTls
        {
            CertName = "tls-proxy"
        };
        public ComponentTls Broker { get; set; } = new ComponentTls 
        { 
            CertName = "tls-broker"
        };
        public ComponentTls Bookie { get; set; } = new ComponentTls 
        { 
            CertName = "tls-bookie"
        };
        public ComponentTls PulsarManager { get; set; } = new ComponentTls 
        { 
            CertName = "tls-pulsar-manager"
        };
        public ComponentTls Presto { get; set; } = new ComponentTls 
        { 
            CertName = "tls-presto"
        };
        public ComponentTls PulsarDetector { get; set; } = new ComponentTls 
        { 
            CertName = "tls-pulsar-detector"
        };
        public ComponentTls AutoRecovery { get; set; } = new ComponentTls 
        { 
            Enabled = true,
            CertName = "tls-recovery"
        };
        public ComponentTls ToolSet { get; set; } = new ComponentTls 
        { 
            Enabled = true, 
            CertName = "tls-toolset"
        };
        public class ComponentTls 
        {
            public bool Enabled { get; set; } = false;
            public string CertName { get; set; }
        }
    }

    public sealed class Authentication
    {
        public bool Enabled { get; set; } = false;
        public string Provider { get; set; } = "jwt";
        // Enable JWT authentication
        // If the token is generated by a secret key, set the usingSecretKey as true.
        // If the token is generated by a private key, set the usingSecretKey as false.
        public bool UsingJwtSecretKey { get; set; } = false;
        public bool Authorization { get; set; } = false;
        public bool Vault { get; set; } = false;
        public sealed class SuperUsers
        {
            // broker to broker communication
            public string Broker { get; set; } = "broker-admin";
            // proxy to broker communication
            public string Proxy { get; set; } = "proxy-admin";
            // pulsar-admin client to broker/proxy communication
            public string Client { get; set; } = "admin";
            //pulsar-manager to broker/proxy communication
            public string PulsarManager { get; set; } = "pulsar-manager-admin";
        }
    }
    public sealed class Probes
    {
        public ComponentProbe Broker { get; set; } = new ComponentProbe
        {
            Liveness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 30,
                PeriodSeconds = 10
            },
            Readiness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 30,
                PeriodSeconds = 10
            },
            Startup = new ProbeOptions
            {
                Enabled = false,
                FailureThreshold = 30,
                InitialDelaySeconds = 60,
                PeriodSeconds = 10
            }
        };
        public ComponentProbe ZooKeeper { get; set; } = new ComponentProbe
        {
            Liveness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Readiness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Startup = new ProbeOptions
            {
                Enabled = false,
                FailureThreshold = 30,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            }
        };
        public ComponentProbe Bookie { get; set; } = new ComponentProbe
        {
            Liveness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 60,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Readiness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 60,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Startup = new ProbeOptions
            {
                Enabled = false,
                FailureThreshold = 30,
                InitialDelaySeconds = 60,
                PeriodSeconds = 30
            }
        };
        public sealed class ComponentProbe
        {
            public ProbeOptions Liveness { get; set; }
            public ProbeOptions Readiness { get; set; }
            public ProbeOptions Startup { get; set; }
        }
        public sealed class ProbeOptions
        {
            public bool Enabled { get; set; } = false;
            public int FailureThreshold { get; set; }
            public int InitialDelaySeconds { get; set; }
            public int PeriodSeconds { get; set; }
        }
    }
    public class Common
    {
        public List<V1Container> ExtraInitContainers { get; set; }
    }
    public class Component
    {
        public ResourcesRequest ResourcesRequest { get; set; }
        public string StorageClassName { get; set; }
        public bool Persistence { get; set; } = true;
        public bool LocalStorage { get; set; } = false;
        public string StorageSize { get; set; }
        public string JournalStorageSize { get; set; }
        public string LedgerStorageSize { get; set; }
        public bool AntiAffinity { get; set; } = false;
        public bool Enabled { get; set; } = false;
        public string ComponentName { get; set; }
        public string ServiceName { get; set; }
        public string ZNode { get; set; }
        public string PodManagementPolicy { get; set; }
        public string UpdateStrategy { get; set; }
        public string StorageProvisioner { get; set; }
        public string ZooConnect { get; set; }
        public int GracePeriodSeconds { get; set; }
        public int Replicas { get; set; }
        public List<V1Container> ExtraInitContainers { get; set; } = new List<V1Container>();
        public List<V1PersistentVolumeClaim> PVC { get; set; } = new List<V1PersistentVolumeClaim>();
        public List<V1Volume> Volumes { get; set; } = new List<V1Volume>();
        public List<V1Container> Containers { get; set; } = new List<V1Container>();
        public List<V1Toleration> Tolerations { get; set; } = new List<V1Toleration>();
        public IDictionary<string, string> ConfigData { get; set; } = new Dictionary<string, string>();
        public ExtraConfig ExtraConfig { get; set; } = new ExtraConfig();
        public V1PodSecurityContext SecurityContext { get; set; } = new V1PodSecurityContext { };
        public IDictionary<string, string> NodeSelector { get; set; } = new Dictionary<string, string>();
        public Tls Tls { get; set; } = new Tls();
        public string HostName { get; set; }
    }
    public sealed class ResourcesRequest
    {
        public string Memory { get; set; }
        public string Cpu { get; set; }

    }
    public class ExtraConfig
    {
        public List<V1Container> ExtraInitContainers { get; set; } = new List<V1Container>();
        public List<V1Container> Containers { get; set; } = new List<V1Container>();
        public IDictionary<string, object> Holder { get; set; } = new Dictionary<string, object>();
    }
}
