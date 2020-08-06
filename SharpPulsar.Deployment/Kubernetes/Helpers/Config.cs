using System.Collections.Generic;
using YamlDotNet.Core.Tokens;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class Config
    {
        public static IDictionary<string, string> ZooKeeper()
        {
            var zk = new Dictionary<string, string>
            {
                {"dataDir", "/pulsar/data/zookeeper" },
                {"PULSAR_PREFIX_serverCnxnFactory", "org.apache.zookeeper.server.NettyServerCnxnFactory"},
                {"serverCnxnFactory", "org.apache.zookeeper.server.NettyServerCnxnFactory"},
                {"PULSAR_PREFIX_peerType", "participant" },
                {"PULSAR_MEM", "-Xms64m -Xmx128m"},
                {"PULSAR_GC", "-XX:+UseG1GC -XX:MaxGCPauseMillis=10 -Dcom.sun.management.jmxremote -Djute.maxbuffer=10485760 -XX:+ParallelRefProcEnabled -XX:+UnlockExperimentalVMOptions -XX:+AggressiveOpts -XX:+DoEscapeAnalysis -XX:+DisableExplicitGC -XX:+PerfDisableSharedMem -Dzookeeper.forceSync=no" }
            };
            if ((bool)Values.ZooKeeper.ExtraConfig.Holder["UseSeparateDiskForTxlog"])
                zk.Add("PULSAR_PREFIX_dataLogDir", "/pulsar/data/zookeeper-datalog");
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                zk.Add("secureClientPort", Values.Ports.ZooKeeper["clientTls"].ToString());
                zk.Add("PULSAR_PREFIX_secureClientPort", Values.Ports.ZooKeeper["clientTls"].ToString());
            }
            if ((bool)Values.ZooKeeper.ExtraConfig.Holder["Reconfig"])
            {
                zk.Add("PULSAR_PREFIX_reconfigEnabled", "true");
                zk.Add("PULSAR_PREFIX_quorumListenOnAllIPs", "true");
            }
            return zk;
        }

        public static IDictionary<string, string> BookKeeper()
        {
            var conf = new Dictionary<string, string>
                        {
                            {"zkLedgersRootPath", $"{Values.MetadataPrefix}/ledgers" },
                            {"httpServerEnabled", "true" },
                            {"httpServerPort", "8000" },
                            {"statsProviderClass", "org.apache.bookkeeper.stats.prometheus.PrometheusMetricsProvider" },
                            {"useHostNameAsBookieID", "true" },
                            //Do not retain journal files as it increase the disk utilization
                            {"journalMaxBackups", "0"},
                            {"journalDirectories", "/pulsar/data/bookkeeper/journal"},
                            {"PULSAR_PREFIX_journalDirectories", "/pulsar/data/bookkeeper/journal"},
                            {"ledgerDirectories", "/pulsar/data/bookkeeper/ledgers"},
                            {"BOOKIE_MEM", "-Xms128m -Xmx256m -XX:MaxDirectMemorySize=256m"},
                            {"PULSAR_MEM", "-Xms128m -Xmx256m -XX:MaxDirectMemorySize=256m"},
                            {"PULSAR_GC", "-XX:+UseG1GC -XX:MaxGCPauseMillis=10 -XX:+ParallelRefProcEnabled -XX:+UnlockExperimentalVMOptions -XX:+AggressiveOpts -XX:+DoEscapeAnalysis -XX:ParallelGCThreads=4 -XX:ConcGCThreads=4 -XX:G1NewSizePercent=50 -XX:+DisableExplicitGC -XX:-ResizePLAB -XX:+ExitOnOutOfMemoryError -XX:+PerfDisableSharedMem -XX:+PrintGCDetails -XX:+PrintGCTimeStamps -XX:+PrintGCApplicationStoppedTime -XX:+PrintHeapAtGC -verbosegc -Xloggc:/var/log/bookie-gc.log -XX:G1LogLevel=finest" }
                        };
            //disable auto recovery on bookies since we will start AutoRecovery in separated pods
            if (Values.AutoRecovery.Enabled)
                conf.Add("autoRecoveryDaemonEnabled", "false");
            if(Values.Tls.Enabled && Values.Tls.Bookie.Enabled)
            {
                conf.Add("zkServers", $"{Values.ZooKeeper.ServiceName}:{Values.Ports.ZooKeeper["clientTls"]}");
                conf.Add("PULSAR_PREFIX_tlsProviderFactoryClass", "org.apache.bookkeeper.tls.TLSContextFactory");
                conf.Add("PULSAR_PREFIX_tlsCertificatePath", @"/pulsar/certs/bookie/tls.crt");
                conf.Add("PULSAR_PREFIX_tlsKeyStoreType", "PEM");
                conf.Add("PULSAR_PREFIX_tlsKeyStore", "/pulsar/certs/bookie/tls.key");
                conf.Add("PULSAR_PREFIX_tlsTrustStoreType", "PEM");
                conf.Add("PULSAR_PREFIX_tlsTrustStore", "/pulsar/certs/ca/ca.crt");
            }
            else
                conf.Add("zkServers", $"{Values.ZooKeeper.ServiceName}:{Values.Ports.ZooKeeper["client"]}");
            return conf;
        }
        public static IDictionary<string, string> Broker()
        {
            var conf = new Dictionary<string, string>
                        {
                            {"zookeeperServers", $"{Values.ZooKeeper.ZooConnect}{Values.MetadataPrefix}"},
                            {"clusterName", $"{Values.Cluster}"},
                            {"exposeTopicLevelMetricsInPrometheus", "true"},
                            {"numHttpServerThreads", "8"},
                            {"zooKeeperSessionTimeoutMillis", "30000"},
                            {"statusFilePath", "/pulsar/status"}
                            
                        };
            if (Values.Broker.Offload.Enabled)
            {
                conf.Add("offloadersDirectory", "/pulsar/offloaders");
                conf.Add("managedLedgerOffloadDriver", $"{ Values.Broker.Offload.ManagedLedgerOffloadDriver }");
                if (Values.Broker.Offload.Gcs.Enabled)
                {
                    conf.Add("gcsManagedLedgerOffloadRegion", $"{ Values.Broker.Offload.Gcs.Region} ");
                    conf.Add("gcsManagedLedgerOffloadBucket", $"{Values.Broker.Offload.Gcs.Bucket }");
                    conf.Add("gcsManagedLedgerOffloadMaxBlockSizeInBytes", $"{ Values.Broker.Offload.Gcs.MaxBlockSizeInBytes}");
                    conf.Add("gcsManagedLedgerOffloadReadBufferSizeInBytes", $"{ Values.Broker.Offload.Gcs.ReadBufferSizeInBytes}");
                    // Authentication with GCS
                    conf.Add("gcsManagedLedgerOffloadServiceAccountKeyFile", $"/pulsar/srvaccts/gcs.json");
                                
                }
                if (Values.Broker.Offload.S3.Enabled)
                {
                     conf.Add("s3ManagedLedgerOffloadRegion", $"{Values.Broker.Offload.S3.Region}");
                     conf.Add("s3ManagedLedgerOffloadBucket", $"{Values.Broker.Offload.S3.Bucket}");
                     conf.Add("s3ManagedLedgerOffloadMaxBlockSizeInBytes", $"{Values.Broker.Offload.S3.MaxBlockSizeInBytes}");
                     conf.Add("s3ManagedLedgerOffloadReadBufferSizeInBytes", $"{Values.Broker.Offload.S3.ReadBufferSizeInBytes}");                     
                }
            }

            if (Values.Functions.Enabled)
            {
                conf.Add("functionsWorkerEnabled", "true");
                conf.Add("PF_functionRuntimeFactoryClassName", "org.apache.pulsar.functions.runtime.kubernetes.KubernetesRuntimeFactory");
                conf.Add("PF_pulsarFunctionsCluster", $"{Values.ReleaseName}");
                conf.Add("PF_connectorsDirectory", "./connectors");
                conf.Add("PF_containerFactory", "k8s");
                conf.Add("PF_numFunctionPackageReplicas", $"{Values.Broker.ConfigData["managedLedgerDefaultEnsembleSize"]}");

                if (Values.Broker.EnableFunctionCustomizerRuntime)
                {
                    conf.Add("PF_runtimeCustomizerClassName", $"{ Values.Broker.RuntimeCustomizerClassName}");
                    conf.Add("PULSAR_EXTRA_CLASSPATH", $"/pulsar/{Values.Broker.PulsarFunctionsExtraClasspath}");
                }
                //support version >= 2.5.0
                conf.Add("PF_functionRuntimeFactoryConfigs_pulsarRootDir", "/pulsar");
                conf.Add("PF_kubernetesContainerFactory_pulsarRootDir", "/pulsar");
                conf.Add("PF_functionRuntimeFactoryConfigs_pulsarDockerImageName", $"{Values.Images.Functions.Repository}:{Values.Images.Functions.Tag}");
                conf.Add("PF_functionRuntimeFactoryConfigs_submittingInsidePod", "true");
                conf.Add("PF_functionRuntimeFactoryConfigs_installUserCodeDependencies", "true");
                conf.Add("PF_functionRuntimeFactoryConfigs_jobNamespace", $"{Values.Namespace}");
                conf.Add("PF_functionRuntimeFactoryConfigs_expectedMetricsCollectionInterval", "30");
                if (!Values.Tls.Enabled && !Values.Tls.Broker.Enabled)
                {
                    conf.Add("PF_functionRuntimeFactoryConfigs_pulsarAdminUrl", $"http://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["http"]}/");
                    conf.Add("PF_functionRuntimeFactoryConfigs_pulsarServiceUrl", $"pulsar://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["pulsar"]}/");
                }
                if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
                {
                    conf.Add("PF_functionRuntimeFactoryConfigs_pulsarAdminUrl", $"https://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["https"]}/");
                    conf.Add("PF_functionRuntimeFactoryConfigs_pulsarServiceUrl", $"pulsar+ssl://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["pulsarssl"]}/");
                }
                conf.Add("PF_functionRuntimeFactoryConfigs_changeConfigMap", $"{Values.ReleaseName}-{Values.Functions.ComponentName}-config");
                conf.Add("PF_functionRuntimeFactoryConfigs_changeConfigMapNamespace", $"{Values.Namespace}");

            }
            else
                conf.Add("functionsWorkerEnabled", "false");

            conf.Add("webServicePort", $"{ Values.Ports.Broker["http"] }");
            if(!Values.Tls.Enabled || !Values.Tls.Broker.Enabled)
                conf.Add("brokerServicePort", $"{ Values.Ports.Broker["pulsar"]  }");
            if (Values.Tls.Enabled || Values.Tls.Broker.Enabled)
            {
                conf.Add("brokerServicePort", $"{ Values.Ports.Broker["pulsarssl"]  }");
                conf.Add("webServicePortTls", $"{ Values.Ports.Broker["https"]  }");
                conf.Add("tlsCertificateFilePath", "/pulsar/certs/broker/tls.crt");
                conf.Add("tlsKeyFilePath", "/pulsar/certs/broker/tls.key");
                conf.Add("tlsTrustCertsFilePath", "/pulsar/certs/ca/ca.crt");
            }
            if (Values.Authentication.Enabled)
            {
                conf.Add("authenticationEnabled", "true");
                conf.Add("authenticateOriginalAuthData", "true");
                if (Values.Authentication.Authorization)
                {
                    conf.Add("authorizationEnabled", "true");
                    conf.Add("superUserRoles", $"{Values.Authentication.Users.Broker},{Values.Authentication.Users.Proxy},{Values.Authentication.Users.Client},{Values.Authentication.Users.PulsarManager}");
                    conf.Add("proxyRoles", Values.Authentication.Users.Proxy);
                }
                if(Values.Authentication.Provider.Equals("jwt", System.StringComparison.OrdinalIgnoreCase) && !Values.Authentication.Vault)
                {
                    //token authentication configuration
                    conf.Add("authenticationProviders", "org.apache.pulsar.broker.authentication.AuthenticationProviderToken");
                    conf.Add("brokerClientAuthenticationParameters", "file:///pulsar/tokens/broker/token");
                    conf.Add("brokerClientAuthenticationPlugin", "org.apache.pulsar.client.impl.auth.AuthenticationToken");
                }
                if (Values.Authentication.UsingJwtSecretKey)
                    conf.Add("tokenSecretKey", "file:///pulsar/keys/token/secret.key");
                else
                    conf.Add("tokenPublicKey", "file:///pulsar/keys/token/public.key");
            }

            if(Values.Tls.Enabled && Values.Tls.Bookie.Enabled)
            {
                //bookkeeper tls settings
                conf.Add("bookkeeperTLSClientAuthentication", "true");
                conf.Add("bookkeeperTLSKeyFileType", "PEM");
                conf.Add("bookkeeperTLSKeyFilePath", "/pulsar/certs/broker/tls.key");
                conf.Add("bookkeeperTLSCertificateFilePath", "/pulsar/certs/broker/tls.crt");
                conf.Add("bookkeeperTLSTrustCertsFilePath", "/pulsar/certs/ca/ca.crt");
                conf.Add("bookkeeperTLSTrustCertTypes", "PEM");
                conf.Add("PULSAR_PREFIX_bookkeeperTLSClientAuthentication", "true");
                conf.Add("PULSAR_PREFIX_bookkeeperTLSKeyFileType", "PEM");
                conf.Add("PULSAR_PREFIX_bookkeeperTLSKeyFilePath", "/pulsar/certs/broker/tls.key");
                conf.Add("PULSAR_PREFIX_bookkeeperTLSCertificateFilePath", "/pulsar/certs/broker/tls.crt");
                conf.Add("PULSAR_PREFIX_bookkeeperTLSTrustCertsFilePath", "/pulsar/certs/ca/ca.crt");
                conf.Add("PULSAR_PREFIX_bookkeeperTLSTrustCertTypes", "PEM");
                //https://github.com/apache/bookkeeper/pull/2300
                conf.Add("bookkeeperUseV2WireProtocol", "false");
            }
            if (Values.Kop.Enabled)
            {
                conf.Add("messagingProtocols", "kafka");
                if (Values.Authentication.Enabled)
                {
                    if(Values.Authentication.Provider.Equals("jwt", System.StringComparison.OrdinalIgnoreCase))
                        conf.Add("PULSAR_PREFIX_saslAllowedMechanisms", "PLAIN");

                }
                if (Values.Tls.Enabled || Values.Tls.Broker.Enabled)
                {
                    conf.Add("PULSAR_PREFIX_kopSslKeystoreLocation", "/pulsar/broker.keystore.jks");
                    conf.Add("PULSAR_PREFIX_kopSslTruststoreLocation", "/pulsar/broker.truststore.jks");
                }
            }
            conf.Add("PULSAR_MEM", "-Xms128m -Xmx256m -XX:MaxDirectMemorySize=256m");
            conf.Add("PULSAR_GC", "-XX:+UseG1GC -XX:MaxGCPauseMillis=10 -Dio.netty.leakDetectionLevel=disabled -Dio.netty.recycler.linkCapacity=1024 -XX:+ParallelRefProcEnabled -XX:+UnlockExperimentalVMOptions -XX:+AggressiveOpts -XX:+DoEscapeAnalysis -XX:ParallelGCThreads=4 -XX:ConcGCThreads=4 -XX:G1NewSizePercent=50 -XX:+DisableExplicitGC -XX:-ResizePLAB -XX:+ExitOnOutOfMemoryError -XX:+PerfDisableSharedMem");
            conf.Add("AWS_ACCESS_KEY_ID", "[YOUR AWS ACCESS KEY ID]");
            conf.Add("AWS_SECRET_ACCESS_KEY", "[YOUR SECRET]");
            conf.Add("managedLedgerDefaultEnsembleSize", "3");
            conf.Add("managedLedgerDefaultWriteQuorum", "3");
            conf.Add("managedLedgerDefaultAckQuorum", "2");
            conf.Add("subscriptionKeySharedUseConsistentHashing", "true");
            return conf;
        }
    }
}
