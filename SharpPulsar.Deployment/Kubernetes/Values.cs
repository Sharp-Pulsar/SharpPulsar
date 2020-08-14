using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Extensions;
using SharpPulsar.Deployment.Kubernetes.Helpers;
using System;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes
{
    public class Values
    {
        public Values()
        {
            ResourcesRequests = new ResourcesRequests
            {
                ZooKeeper = new ResourcesRequest { Memory = "256Mi", Cpu = "0.1" },
                Broker = new ResourcesRequest { Memory = "512Mi", Cpu = "0.2" },
                BookKeeper = new ResourcesRequest { Memory = "512Mi", Cpu = "0.2" },
                AutoRecovery = new ResourcesRequest { Memory = "64Mi", Cpu= "0.05"}
            };
            Authentication = new Authentication
            {
                Enabled = false
            };
            Tls = new Tls
            {
                Enabled = false
            };
            Namespace = "pulsar";
            Cluster = "pulsar";
            ReleaseName = "pulsar";
            App = "pulsar";
            UserProvidedZookeepers = new List<string>();
            Persistence = true;
            LocalStorage = false;
            AntiAffinity = true;
            Initialize = true;
            ConfigurationStore = "";
            ConfigurationStoreMetadataPrefix = "";
            Namespace = "pulsar";
            NamespaceCreate = true;
            MetadataPrefix = "";
            Monitoring = new Monitoring();
            Images = new Images();
            Probe = new Probes();
            Ports = new Ports();
            Settings = new ComponentSettings
            {
                Autorecovery = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 1,
                    Name = "recovery",
                    Service = $"{ReleaseName}-recovery",
                    Host = "${HOSTNAME}." + $"{ReleaseName}-recovery.{Namespace}.svc.cluster.local",
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "Parallel"
                },
                ZooKeeper = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 3,
                    Name = "zookeeper",
                    Service = $"{ReleaseName}-zookeeper",
                    Host = "${HOSTNAME}." + $"{ReleaseName}-zookeeper.{Namespace}.svc.cluster.local",
                    Storage = new Storage
                    {
                        ClassName = "default",//Each AKS cluster includes four pre-created storage classes(default,azurefile,azurefile-premium,managed-premium)
                        Size = "50Gi"
                    },
                    ZooConnect = Tls.ZooKeeper.Enabled ? $"{ReleaseName}-zookeeper:2281" : $"{ReleaseName}-zookeeper:2181",
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "OrderedReady",
                },
                Broker = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 3,
                    Name = "broker",
                    Service = $"{ReleaseName}-broker",
                    Host = "${HOSTNAME}." + $"{ReleaseName}-broker.{Namespace}.svc.cluster.local",
                    ZNode = $"{MetadataPrefix}/loadbalance/brokers/"+ "${HOSTNAME}." + $"{ReleaseName}-broker.{Namespace}.svc.cluster.local:2181",
                    UpdateStrategy = "RollingUpdate",
                    EnableFunctionCustomizerRuntime = false,
                    PulsarFunctionsExtraClasspath = "extraLibs",
                    RuntimeCustomizerClassName = "org.apache.pulsar.functions.runtime.kubernetes.BasicKubernetesManifestCustomizer",

                    PodManagementPolicy = "Parallel",
                },
                BookKeeper = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 3,
                    Name = "bookie",
                    Service = $"{ReleaseName}-bookie",
                    Host = "${HOSTNAME}." + $"{ReleaseName}-bookie.{Namespace}.svc.cluster.local",
                    Storage = new Storage
                    {
                        LedgerSize = "50Gi",
                        JournalSize = "10Gi",
                    },
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "Parallel"
                },
                Proxy = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 3,
                    Name = "proxy",
                    Service = $"{ReleaseName}-proxy",
                    Host = "${HOSTNAME}." + $"{ReleaseName}-proxy.{Namespace}.svc.cluster.local",
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "Parallel",
                },
                PrestoCoord = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 1,
                    Name = "presto-coordinator",
                    Service = $"{ReleaseName}-presto-coordinator",
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "Parallel",
                },
                PrestoWorker = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 2,
                    Name = "presto-work",
                    Service = $"{ReleaseName}-presto-worker",
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "Parallel"
                },
                Kop = new ComponentSetting
                {
                    Enabled = false
                },
                Function = new ComponentSetting
                {
                    Enabled = true,
                    Name = "functions-worker"
                }
            };
            ExtraConfigs = new ExtraConfigs
            {
                ZooKeeper = new ExtraConfig
                {
                    Holder = new Dictionary<string, object>
                    {
                        { "ZkServer", new List<string> { } },
                        { "PeerType", "participant" },
                        { "InitialMyId", 0 },
                        { "UseSeparateDiskForTxlog", false },
                        { "Reconfig", false }
                    }
                },
                Broker = new ExtraConfig
                {
                    Holder = new Dictionary<string, object>
                    {
                        {"AdvertisedPodIP", false }
                    }
                },
                Bookie  = new ExtraConfig
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
                            Args = new List<string>{string.Join(" ", Args.WaitZooKeeperContainer()) }
                        }
                    },
                    Containers = new List<V1Container>
                    {
                        new V1Container
                        {
                            Name = $"{ReleaseName}-{Settings.BookKeeper.Name}-init",
                            Image = $"{Images.Bookie.Repository}:{Images.Bookie.Tag}",
                            ImagePullPolicy = Images.Bookie.PullPolicy,
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string>{string.Join(" ", Args.BookieExtraInitContainer()) },
                            EnvFrom = new List<V1EnvFromSource>
                            {
                                new V1EnvFromSource
                                {
                                    ConfigMapRef = new V1ConfigMapEnvSource
                                    {
                                        Name = $"{ReleaseName}-{Settings.BookKeeper.Name}"
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
                PrestoCoordinator = new ExtraConfig
                {
                    Holder = new Dictionary<string, object>
                    {
                        { "memory", "2G"},{"maxMemory","1GB" },{"maxMemoryPerNode", "128MB"},
                        {"Log", "DEBUG" }, {"maxEntryReadBatchSize", "100"},{ "targetNumSplits", "16"},
                        {"maxSplitMessageQueueSize", "10000"}, {"maxSplitEntryQueueSize", "1000"},
                        {"namespaceDelimiterRewriteEnable", "true" },{ "rewriteNamespaceDelimiter", "/"},
                        {"bookkeeperThrottleValue", "0" }, {"managedLedgerCacheSizeMB", "0"}
                    }
                },
                PrestoWorker = new ExtraConfig
                {
                    Holder = new Dictionary<string, object>
                    {
                        { "memory", "2G"},{"maxMemory","1GB" },{"maxMemoryPerNode", "128MB"},
                        {"Log", "DEBUG" }, {"maxEntryReadBatchSize", "100"},{ "targetNumSplits", "16"},
                        {"maxSplitMessageQueueSize", "10000"}, {"maxSplitEntryQueueSize", "1000"},
                        {"namespaceDelimiterRewriteEnable", "true" },{ "rewriteNamespaceDelimiter", "/"},
                        {"bookkeeperThrottleValue", "0" }, {"managedLedgerCacheSizeMB", "0"}
                    }
                }
            };
            ConfigMaps = new ConfigMaps
            {
                AutoRecovery = new Dictionary<string, string> { { "BOOKIE_MEM", "-Xms64m -Xmx64m" } },
                ZooKeeper = Config.ZooKeeper().RemoveRN(),
                Broker = Config.Broker().RemoveRN(),
                BookKeeper = Config.BookKeeper().RemoveRN(),
                Proxy = Config.Proxy().RemoveRN(),
                PrestoCoordinator = Config.PrestoCoord(Settings.PrestoCoord.Replicas > 0 ? "false" : "true").RemoveRN(),
                PrestoWorker = Config.PrestoWorker().RemoveRN()

            };
            //Dependencies order
            ZooKeeper = ZooKeeperComponent();
            BookKeeper = BookKeeperComponent();
            AutoRecovery = AutoRecoveryComponent();
            Broker = BrokerComponent();
            Proxy = ProxyComponent();
            PrestoCoordinator = PrestoCoordinatorComponent();
            PrestoWorker = PrestoWorkComponent();
            Toolset = new Component();
            Kop = new Component();
            Functions = new Component();
            AutoRecovery = AutoRecoveryComponent();
            Ingress = new Ingress 
            { 
                Enabled = false,
                Proxy =new Ingress.IngressSetting
                {
                    Type = "LoadBalancer"
                }
            };

        }
        public static List<string> UserProvidedZookeepers { get; set; }
        public static bool Persistence { get; set; }
        public static bool LocalStorage { get; set; }
        public static bool AntiAffinity { get; set; }
        // Flag to control whether to run initialize job
        public static bool Initialize { get; set; }
        public static string ConfigurationStore { get; set; }
        public static string ConfigurationStoreMetadataPrefix { get; set; }
        //Namespace to deploy pulsar
        public static string Namespace { get; set; }
        public static string Cluster { get; set; }
        public static string ReleaseName { get; set; }
        public static string App { get; set; }
        public static bool NamespaceCreate { get; set; }
        //// Pulsar Metadata Prefix
        ////
        //// By default, pulsar stores all the metadata at root path.
        //// You can configure to have a prefix (e.g. "/my-pulsar-cluster").
        //// If you do so, all the pulsar and bookkeeper metadata will
        //// be stored under the provided path
        public static string MetadataPrefix { get; set; }

        public static Tls Tls { get; set; }
        //// Monitoring Components
        ////
        //// Control what components of the monitoring stack to deploy for the cluster
        public static Monitoring Monitoring { get; set; }
        //// Images
        ////
        //// Control what images to use for each component
        public static Images Images { get; set; }
        //// TLS
        //// templates/tls-certs.yaml
        ////
        //// The chart is using cert-manager for provisioning TLS certs for
        //// brokers and proxies.
        ///
        public static ResourcesRequests ResourcesRequests {get;set;}
        public static Authentication Authentication { get; set; }

        public static ExtraConfigs ExtraConfigs { get; set; }

        public static ConfigMaps ConfigMaps { get; set; }

        public static Probes Probe { get; set; }
        public static ComponentSettings Settings { get; set; }
        public static Ports Ports { get; set; }
        public static Component Toolset { get; set; }
        public static Component AutoRecovery { get; set; } 
        public static Component ZooKeeper { get; set; }
        public static Component BookKeeper { get; set; } 
        public static Component Broker { get; set; }
        public static Component Proxy { get; set; } 

        public static Component PrestoCoordinator { get; set; }
        public static Component PrestoWorker { get; set; } 
        public static Component Functions { get; set; }
        public static Component Kop { get; set; }
        public static Ingress Ingress { get; set; }

        private Component AutoRecoveryComponent()
        {
            return new Component
            {
                
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
                            Args = new List<string>{ string.Join(" ", Args.AutoRecoveryIntContainer()) },
                            EnvFrom = new List<V1EnvFromSource>
                            {
                                new V1EnvFromSource
                                {
                                    ConfigMapRef = new V1ConfigMapEnvSource
                                    {
                                         Name = $"{ReleaseName}-{Settings.BookKeeper.Name}"
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
                            Name = $"{ReleaseName}-{Settings.Autorecovery.Name}",
                            Image = $"{Images.Autorecovery.Repository}:{Images.Autorecovery.Tag}",
                            ImagePullPolicy = Images.Autorecovery.PullPolicy,
                            Resources = new V1ResourceRequirements
                            {
                                Requests = new Dictionary<string, ResourceQuantity>
                                {
                                    {
                                        "memory", new ResourceQuantity(ResourcesRequests.AutoRecovery.Memory)
                                    },
                                    {
                                        "cpu", new ResourceQuantity(ResourcesRequests.AutoRecovery.Cpu)
                                    }
                                }
                            },
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string>{ string.Join(" ", Args.AutoRecoveryContainer()) },
                            Ports = Helpers.Ports.AutoRecovery(),
                            EnvFrom = new List<V1EnvFromSource>
                            {
                                new V1EnvFromSource
                                {
                                    ConfigMapRef = new V1ConfigMapEnvSource
                                    {
                                        Name = $"{ReleaseName}-{Settings.BookKeeper.Name}"
                                    }
                                }
                            },
                            VolumeMounts = VolumeMounts.RecoveryContainer()
                        }
                },
                Volumes = Volumes.Recovery()
            };
        }
        private Component ZooKeeperComponent()
        {
            return new Component
            {
                Containers = new List<V1Container>
                    {
                        new V1Container
                        {
                            Name = $"{ReleaseName}-{Settings.ZooKeeper.Name}",
                            Image = $"{Images.ZooKeeper.Repository}:{Images.ZooKeeper.Tag}",
                            ImagePullPolicy = Images.ZooKeeper.PullPolicy,
                            Resources = new V1ResourceRequirements
                            {
                                Requests = new Dictionary<string, ResourceQuantity>
                                {
                                    {
                                        "memory", new ResourceQuantity(ResourcesRequests.ZooKeeper.Memory)
                                    },
                                    {
                                        "cpu", new ResourceQuantity(ResourcesRequests.ZooKeeper.Cpu)
                                    }
                                }
                            },
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string>{ string.Join(" ", Args.ZooKeeper()) },
                            Ports = Helpers.Ports.ZooKeeper(),
                            Env = EnvVar.ZooKeeper(),
                            EnvFrom = new List<V1EnvFromSource>
                            {
                                new V1EnvFromSource
                                {
                                    ConfigMapRef = new V1ConfigMapEnvSource
                                    {
                                        Name = $"{ReleaseName}-zookeeper"
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
                PVC = VolumeClaim.ZooKeeper()
            };
        }
        private Component BrokerComponent()
        {
            return new Component
            {                
                
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
                        Args = new List<string>{ string.Join(" ", Args.BrokerZooIntContainer()) },
                        VolumeMounts = VolumeMounts.BrokerContainer()
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
                        Args = new List<string>{string.Join(" ", Args.BrokerBookieIntContainer()) },
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.BookKeeper.Name}"
                                }
                            }
                        },
                        VolumeMounts = VolumeMounts.BrokerContainer()
                    }
                },
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{Settings.Broker.Name }",
                        Image = $"{Images.Broker.Repository}:{Images.Broker.Tag}",
                        ImagePullPolicy = Images.Broker.PullPolicy,
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string, ResourceQuantity>
                            {
                                {
                                    "memory", new ResourceQuantity(ResourcesRequests.Broker.Memory)
                                },
                                {
                                    "cpu", new ResourceQuantity(ResourcesRequests.Broker.Cpu)
                                }
                            }
                        },
                        Command = new []
                        {
                            "sh",
                            "-c"
                        },
                        Args = new List<string>{string.Join(" ", Args.BrokerContainer()) },
                        Ports = Helpers.Ports.BrokerPorts(),
                        Env = EnvVar.Broker(),
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.Broker.Name}"
                                }
                            }
                        },
                        ReadinessProbe = Helpers.Probe.HttpActionReadiness(Probe.Broker, "/status.html", Ports.Broker["http"]),
                        LivenessProbe = Helpers.Probe.HttpActionLiviness(Probe.Broker, "/status.html", Ports.Broker["http"]),
                        StartupProbe = Helpers.Probe.HttpActionStartup(Probe.Broker, "/status.html", Ports.Broker["http"]),
                        VolumeMounts = VolumeMounts.Broker()
                    }
                },
                Volumes = Volumes.Broker()
            };
        }
        private Component BookKeeperComponent()
        {
            return new Component
            {
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
                        Args = new List<string>{string.Join(" ", Args.BookieIntContainer()) },
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.BookKeeper.Name}"
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
                        Name = $"{ReleaseName}-{Settings.BookKeeper.Name }",
                        Image = $"{Images.Bookie.Repository}:{Images.Bookie.Tag}",
                        ImagePullPolicy = Images.Bookie.PullPolicy,
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string, ResourceQuantity>
                            {
                                {
                                    "memory", new ResourceQuantity(ResourcesRequests.BookKeeper.Memory)
                                },
                                {
                                    "cpu", new ResourceQuantity(ResourcesRequests.BookKeeper.Cpu)
                                }
                            }
                        },
                        Command = new []
                        {
                            "bash",
                            "-c"
                        },
                        Args = new List<string>{ string.Join(" ", Args.BookieContainer()) },
                        Ports = Helpers.Ports.BookKeeper(),
                        Env = EnvVar.BookKeeper(),
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.BookKeeper.Name }"
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
                PVC = VolumeClaim.BookKeeper()
            };
        }
        private Component ProxyComponent()
        {
            return new Component
            {
                ExtraInitContainers = new List<V1Container>
                {
                    new V1Container
                        {
                            Name = "wait-zookeeper-ready",
                            Image = $"{Images.Proxy.Repository}:{Images.Proxy.Tag}",
                            ImagePullPolicy = Images.Proxy.PullPolicy ,
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string> { string.Join(" ", Args.WaitZooKeeperContainer()) }
                        },
                        new V1Container
                        {
                            Name = "wait-broker-ready",
                            Image = $"{Images.Proxy.Repository}:{Images.Proxy.Tag}",
                            ImagePullPolicy = Images.Proxy.PullPolicy ,
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string> { string.Join(" ", Args.WaitBrokerContainer()) } 
                        }
                },
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{Settings.Proxy.Name }",
                        Image = $"{Images.Proxy.Repository}:{Images.Proxy.Tag}",
                        ImagePullPolicy = Images.Proxy.PullPolicy,
                        Command = new []
                        {
                            "bash",
                            "-c"
                        },
                        Args = new List<string>{ string.Join(" ", Args.ProxyContainer()) },
                        Ports = Helpers.Ports.Proxy(),
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.Proxy.Name }"
                                }
                            }
                        },
                        ReadinessProbe = Helpers.Probe.HttpActionReadiness(Probe.Proxy, "/status.html", Ports.Proxy["http"]),
                        LivenessProbe = Helpers.Probe.HttpActionLiviness(Probe.Proxy, "/status.html", Ports.Proxy["http"]),
                        StartupProbe = Helpers.Probe.HttpActionStartup(Probe.Proxy, "/status.html", Ports.Proxy["http"]),
                        VolumeMounts = VolumeMounts.ProxyContainer()
                    }
                },
                Volumes = Volumes.Proxy()
            };
        }
        private Component PrestoCoordinatorComponent()
        {
            return new Component
            {
                /*ExtraInitContainers = new List<V1Container> 
                {
                        new V1Container
                        {
                            Name = "wait-broker-ready",
                            Image = $"{Images.Broker.Repository}:{Images.Broker.Tag}",
                            ImagePullPolicy = Images.Broker.PullPolicy ,
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string> { string.Join(" ", Args.WaitBrokerContainer()) }
                        }
                },*/
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{Settings.PrestoCoord.Name }",
                        Image = $"{Images.Presto.Repository}:{Images.Presto.Tag}",
                        ImagePullPolicy = Images.Presto.PullPolicy,
                        Command = new []
                        {
                            "bash",
                            "-c"
                        },
                        Args = new List<string>{ string.Join(" ", Args.PrestoCoordContainer()) },
                        Ports = Helpers.Ports.PrestoCoord(),
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.PrestoCoord.Name }"
                                }
                            }
                        },
                        ReadinessProbe = Helpers.Probe.HttpActionReadiness(Probe.Presto, "/v1/cluster", Ports.PrestoCoordinator["http"]),
                        LivenessProbe = Helpers.Probe.HttpActionLiviness(Probe.Presto, "/v1/cluster", Ports.PrestoCoordinator["http"]),
                        StartupProbe = Helpers.Probe.HttpActionStartup(Probe.Presto, "/v1/cluster", Ports.PrestoCoordinator["http"]),
                        VolumeMounts = VolumeMounts.PrestoCoordContainer()//here
                    }
                },
                Volumes = Volumes.PrestoCoord()
            };
        }
        private Component PrestoWorkComponent()
        {
            return new Component
            {
                /*ExtraInitContainers = new List<V1Container>
                {
                        new V1Container
                        {
                            Name = "wait-presto-coord-ready",
                            Image = $"{Images.Presto.Repository}:{Images.Presto.Tag}",
                            ImagePullPolicy = Images.Presto.PullPolicy ,
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string> { string.Join(" ", Args.WaitPrestoCoordContainer()) }
                        }
                },*/
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{Settings.PrestoWorker.Name }",
                        Image = $"{Images.Presto.Repository}:{Images.Presto.Tag}",
                        ImagePullPolicy = Images.Presto.PullPolicy,
                        Command = new []
                        {
                            "bash",
                            "-c"
                        },
                        Args =  new List<string>{ string.Join(" ", Args.PrestoWorker()) },
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.PrestoWorker.Name }"
                                }
                            }
                        },
                        ReadinessProbe = Helpers.Probe.ExecActionReadiness(Probe.PrestoWorker, "/bin/bash", "/presto/health_check.sh"),
                        LivenessProbe = Helpers.Probe.ExecActionLiviness(Probe.PrestoWorker, "/bin/bash", "/presto/health_check.sh"),
                        VolumeMounts = VolumeMounts.PrestoWorkerContainer()
                    }
                },
                Volumes = Volumes.PrestoWorker()
            };
        }
    }
    public sealed class ProxyServiceUrl
    {
        public string BrokerHttpUrl { get; set; }
        public string BrokerHttpsUrl { get; set; }
        public string BrokerPulsarUrl { get; set; }
        public string BrokerPulsarSslUrl { get; set; }
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
        public IDictionary<string, int> Proxy { get; set; } = new Dictionary<string, int>
        {
            {"http", 8080},
            {"https", 8443},
            {"pulsar", 6650},
            {"pulsarssl", 6651}
        };
        public IDictionary<string, int> PrestoCoordinator { get; set; } = new Dictionary<string, int>
        {
            {"http", 8081},
            {"https", 4431}
        };
        public IDictionary<string, int> PrestoWorker { get; set; } = new Dictionary<string, int>
        {
            {"http", 8081},
            {"https", 4431}
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
            {"client-tls", 2281},
            {"follower", 2888},
            {"leader-election", 3888}
        };
    }
    public  sealed class Monitoring
    {
        // monitoring - prometheus
        public bool Prometheus { get; set; } = false;
        // monitoring - grafana
        public bool Grafana { get; set; } = false;
        // alerting - alert-manager
        public bool AlertManager { get; set; } = false;
    }
    public  sealed class Images 
    {
        public Image ZooKeeper { get; set; } = new Image();
        public Image Bookie { get; set; } = new Image();
        public Image Presto { get; set; } = new Image();
        public Image Autorecovery { get; set; } = new Image();
        public Image Broker { get; set; } = new Image();
        public Image PulsarMetadata { get; set; } = new Image();
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
        public Image Ingress { get; set; } = new Image
        {
            Repository = "us.gcr.io/k8s-artifacts-prod/ingress-nginx/controller",
            Tag = "v0.34.1"
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
    
    public sealed class ComponentSettings
    {
        public ComponentSetting Broker { get; set; }
        public ComponentSetting ZooKeeper { get; set; }
        public ComponentSetting BookKeeper { get; set; }
        public ComponentSetting Autorecovery { get; set; }
        public ComponentSetting Proxy { get; set; }
        public ComponentSetting PrestoCoord { get; set; }
        public ComponentSetting PrestoWorker { get; set; }
        public ComponentSetting Function { get; set; }
        public ComponentSetting Toolset { get; set; }
        public ComponentSetting Kop { get; set; }
    }
    public sealed class ComponentSetting
    {
        public bool Enabled { get; set; }
        public bool AntiAffinity { get; set; } = true;
        public int Replicas { get; set; }
        public string Name { get; set; }
        public string Service { get; set; }
        public string Host { get; set; }
        public Offload Offload { get; set; } = new Offload();

        public ProxyServiceUrl ProxyServiceUrl { get; set; } = new ProxyServiceUrl();
        public bool UsePolicyPodDisruptionBudget { get; set; }
        public bool EnableFunctionCustomizerRuntime { get; set; } = false;
        public string PulsarFunctionsExtraClasspath { get; set; }
        public string RuntimeCustomizerClassName { get; set; }
        public string PodManagementPolicy { get; set; }
        public string UpdateStrategy { get; set; }
        public int GracePeriodSeconds { get; set; }
        public bool Persistence { get; set; } = true;
        public bool LocalStorage { get; set; } = false;
        public string ZooConnect { get; set; }
        public string ZNode { get; set; }
        public Storage Storage { get; set; }
    }
    public sealed class Ingress 
    { 
        public bool Rbac { get; set; }
        public bool Enabled { get; set; }
        public int Replicas { get; set; } = 1;
        public int GracePeriodSeconds { get; set; } = 30;

        public bool DeployNginxController { get; set; } = true;

        public IngressSetting Proxy { get; set; } = new IngressSetting();
        public IngressSetting Presto { get; set; } = new IngressSetting();
        public IngressSetting Broker { get; set; } = new IngressSetting();
        public string DomainSuffix { get; set; }
        public List<HttpRule> HttpRules { get; set; } = new List<HttpRule>();
        public sealed class IngressSetting
        {
            public bool Enabled { get; set; }
            public bool Tls { get; set; }
            public string Type { get; set; }
            public IDictionary<string,string> Annotations { get; set; }
            public IDictionary<string,string> ExtraSpec { get; set; }
        }
        public sealed class HttpRule
        {
            public bool Tls { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public string Path { get; set; }
            public string ServiceName { get; set; }
        }
    }
    public sealed class Tls
    {
        //echo <service principal password> | openssl base64
        public string SecretPassword { get; set; }
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
        public SuperUsers Users { get; set; } = new SuperUsers();
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
    public sealed class ExtraConfigs
    {
        public ExtraConfig ZooKeeper { get; set; }
        public ExtraConfig Proxy { get; set; }
        public ExtraConfig Broker { get; set; }
        public ExtraConfig Bookie { get; set; }
        public ExtraConfig PrestoCoordinator { get; set; }
        public ExtraConfig PrestoWorker { get; set; }
        public ExtraConfig AutoRecovery { get; set; }
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

        public ComponentProbe Presto { get; set; } = new ComponentProbe
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
        public ComponentProbe PrestoWorker { get; set; } = new ComponentProbe
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
        public ComponentProbe Proxy { get; set; } = new ComponentProbe
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
        public List<V1Container> ExtraInitContainers { get; set; } = new List<V1Container>();
        public List<V1PersistentVolumeClaim> PVC { get; set; } = new List<V1PersistentVolumeClaim>();
        public List<V1Volume> Volumes { get; set; } = new List<V1Volume>();
        public List<V1Container> Containers { get; set; } = new List<V1Container>();
        public List<V1Toleration> Tolerations { get; set; } = new List<V1Toleration>();
        public V1PodSecurityContext SecurityContext { get; set; } = new V1PodSecurityContext { };
        public IDictionary<string, string> NodeSelector { get; set; } = new Dictionary<string, string>();
    }
    public sealed class ResourcesRequests
    {
        public ResourcesRequest AutoRecovery { get; set; }
        public ResourcesRequest ZooKeeper { get; set; }
        public ResourcesRequest BookKeeper { get; set; }
        public ResourcesRequest Broker { get; set; }
        public ResourcesRequest Proxy { get; set; }
        public ResourcesRequest PrestoCoordinator { get; set; }
        public ResourcesRequest PrestoWorker { get; set; }

    }

    public sealed class ResourcesRequest
    {
        public string Memory { get; set; }
        public string Cpu { get; set; }

    }
    public sealed class Offload
    {
        public bool Enabled { get; set; }
        public string ManagedLedgerOffloadDriver { get; set; }
        public OffloadSetting Gcs { get; set; } = new OffloadSetting();
        public OffloadSetting Azure { get; set; } = new OffloadSetting();
        public OffloadSetting S3 { get; set; } = new OffloadSetting();
        public sealed class OffloadSetting
        {
            public bool Enabled { get; set; }
            public string Region { get; set; }
            public string Bucket { get; set; }
            public long MaxBlockSizeInBytes { get; set; }
            public long ReadBufferSizeInBytes { get; set; }
        }
    }
    public sealed class Storage
    {
        public string ClassName { get; set; } = "default";//Each AKS cluster includes four pre-created storage classes(default,azurefile,azurefile-premium,managed-premium)
        public string Provisioner { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
        public string Size { get; set; }
        public string JournalSize { get; set; }
        public string LedgerSize { get; set; }
    }
    public class ExtraConfig
    {
        public List<V1Container> ExtraInitContainers { get; set; } = new List<V1Container>();
        public List<V1Container> Containers { get; set; } = new List<V1Container>();
        public IDictionary<string, object> Holder { get; set; } = new Dictionary<string, object>();
    }
    public sealed class ConfigMaps
    {
        public IDictionary<string, string> ZooKeeper { get; set; }
        public IDictionary<string, string> BookKeeper { get; set; }
        public IDictionary<string, string> Broker { get; set; }
        public IDictionary<string, string> PrestoCoordinator { get; set; }
        public IDictionary<string, string> PrestoWorker { get; set; }
        public IDictionary<string, string> Proxy { get; set; }
        public IDictionary<string, string> AutoRecovery { get; set; }
        public IDictionary<string, string> Functions { get; set; }
        public IDictionary<string, string> Toolset { get; set; }
    }

}
