using System;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    internal class Config
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
        public static IDictionary<string, string> Proxy()
        {
            var conf = new Dictionary<string, string>
                        {
                            {"clusterName", $"{Values.Cluster}"},
                            {"httpNumThreads", "8"},
                            {"statusFilePath", "/pulsar/status"}
                            
                        };
            conf.Add("webServicePort", $"{ Values.Ports.Proxy["http"] }");
            if(!Values.Tls.Enabled || !Values.Tls.Proxy.Enabled)
            {
                conf.Add("servicePort", $"{ Values.Ports.Proxy["pulsar"]  }");
                if(!string.IsNullOrWhiteSpace(Values.Proxy.ProxyServiceUrl.BrokerPulsarUrl))
                    conf.Add("brokerServiceURL", Values.Proxy.ProxyServiceUrl.BrokerPulsarUrl);
                else
                    conf.Add("brokerServiceURL", $"pulsar://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["pulsar"]}");
                
                if(!string.IsNullOrWhiteSpace(Values.Proxy.ProxyServiceUrl.BrokerHttpUrl))
                    conf.Add("brokerWebServiceURL", Values.Proxy.ProxyServiceUrl.BrokerHttpUrl);
                else
                    conf.Add("brokerWebServiceURL", $"http://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["http"]}");

            }
            if (Values.Tls.Enabled || Values.Tls.Proxy.Enabled)
            {
                conf.Add("tlsEnabledInProxy", "true");
                conf.Add("servicePortTls", $"{ Values.Ports.Proxy["pulsarssl"]  }");
                conf.Add("webServicePortTls", $"{ Values.Ports.Proxy["https"]  }");
                conf.Add("tlsCertificateFilePath", "/pulsar/certs/proxy/tls.crt");
                conf.Add("tlsKeyFilePath", "/pulsar/certs/proxy/tls.key");
                conf.Add("tlsTrustCertsFilePath", "/pulsar/certs/ca/ca.crt");
            }
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
            {

                if (!string.IsNullOrWhiteSpace(Values.Proxy.ProxyServiceUrl.BrokerPulsarSslUrl))
                    conf.Add("brokerServiceURLTLS", Values.Proxy.ProxyServiceUrl.BrokerPulsarSslUrl);
                else
                    conf.Add("brokerServiceURLTLS", $"pulsar+ssl://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["pulsarssl"]}");

                if (!string.IsNullOrWhiteSpace(Values.Proxy.ProxyServiceUrl.BrokerHttpsUrl))
                    conf.Add("brokerWebServiceURLTLS", Values.Proxy.ProxyServiceUrl.BrokerHttpsUrl);
                else
                    conf.Add("brokerWebServiceURLTLS", $"https://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["https"]}");

                conf.Add("tlsEnabledWithBroker", "true");
                conf.Add("tlsCertRefreshCheckDurationSec", "300");
                conf.Add("brokerClientTrustCertsFilePath", "/pulsar/certs/ca/ca.crt");
            }
            if (!Values.Tls.Enabled && !Values.Tls.Broker.Enabled)
            {

                if (!string.IsNullOrWhiteSpace(Values.Proxy.ProxyServiceUrl.BrokerPulsarUrl))
                    conf["brokerServiceURL"] =  Values.Proxy.ProxyServiceUrl.BrokerPulsarUrl;
                else
                    conf["brokerServiceURL"] = $"pulsar://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["pulsar"]}";

                if (!string.IsNullOrWhiteSpace(Values.Proxy.ProxyServiceUrl.BrokerHttpUrl))
                    conf["brokerWebServiceURL"] =  Values.Proxy.ProxyServiceUrl.BrokerHttpUrl;
                else
                    conf["brokerWebServiceURL"] = $"http://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["http"]}";

            }
            if (Values.Authentication.Enabled)
            {
                conf.Add("authenticationEnabled", "true");
                conf.Add("forwardAuthorizationCredentials", "true");
                if (Values.Authentication.Authorization)
                {
                    conf["authorizationEnabled"] = "false";
                    conf["forwardAuthorizationCredentials"] =  "true";
                    conf.Add("superUserRoles", $"{Values.Authentication.Users.Broker},{Values.Authentication.Users.Proxy},{Values.Authentication.Users.Client},{Values.Authentication.Users.PulsarManager}");
                    conf.Add("proxyRoles", Values.Authentication.Users.Proxy);
                }
                if(Values.Authentication.Provider.Equals("jwt", System.StringComparison.OrdinalIgnoreCase) && !Values.Authentication.Vault)
                {
                    //token authentication configuration
                    conf.Add("authenticationProviders", "org.apache.pulsar.broker.authentication.AuthenticationProviderToken");
                    conf.Add("brokerClientAuthenticationParameters", "file:///pulsar/tokens/proxy/token");
                    conf.Add("brokerClientAuthenticationPlugin", "org.apache.pulsar.client.impl.auth.AuthenticationToken");
                }
                if (Values.Authentication.UsingJwtSecretKey)
                    conf.Add("tokenSecretKey", "file:///pulsar/keys/token/secret.key");
                else
                    conf.Add("tokenPublicKey", "file:///pulsar/keys/token/public.key");
            }

            conf.Add("PULSAR_MEM", "-Xms64m -Xmx64m -XX:MaxDirectMemorySize=64m");
            conf.Add("PULSAR_GC", "-XX:+UseG1GC -XX:MaxGCPauseMillis=10 -Dio.netty.leakDetectionLevel=disabled -Dio.netty.recycler.linkCapacity=1024 -XX:+ParallelRefProcEnabled -XX:+UnlockExperimentalVMOptions -XX:+AggressiveOpts -XX:+DoEscapeAnalysis -XX:ParallelGCThreads=4 -XX:ConcGCThreads=4 -XX:G1NewSizePercent=50 -XX:+DisableExplicitGC -XX:-ResizePLAB -XX:+ExitOnOutOfMemoryError -XX:+PerfDisableSharedMem");
            return conf;
        }
        public static IDictionary<string, string> PrestoCoord(string schedule)
        {
            var conf = new Dictionary<string, string> 
            {
                {
                    "node.properties",
                    @"node.environment=production 
                    node.data-dir=/pulsar/data"
                },
                {
                    "jvm.config",
                    $@"-server 
                    -Xmx{Values.PrestoCoordinator.ExtraConfig.Holder["memory"]} 
                    -XX:+UseG1GC 
                    -XX:+UnlockExperimentalVMOptions 
                    -XX:+AggressiveOpts 
                    -XX:+DoEscapeAnalysis 
                    -XX:ParallelGCThreads=4 
                    -XX:ConcGCThreads=4 
                    -XX:G1NewSizePercent=50 
                    -XX:+DisableExplicitGC 
                    -XX:-ResizePLAB 
                    -XX:+ExitOnOutOfMemoryError 
                    -XX:+PerfDisableSharedMem" 
                },
                {
                    "config.properties",
                    $@"coordinator=true 
                        http-server.http.port={Values.Ports.PrestoCoordinator["http"]} 
                        discovery-server.enabled=true 
                        discovery.uri=http://{Values.PrestoCoordinator.ServiceName}:{Values.Ports.PrestoCoordinator["http"]} 
                        query.max-memory={Values.PrestoCoordinator.ExtraConfig.Holder["maxMemory"]} 
                        query.max-memory-per-node={ Values.PrestoCoordinator.ExtraConfig.Holder["maxMemoryPerNode"] } 
                        distributed-joins-enabled=true 
                        node-scheduler.include-coordinator={schedule} "
                },
                {
                    "log.properties",
                    $@"com.facebook.presto={Values.PrestoCoordinator.ExtraConfig.Holder["Log"]} 
                        com.sun.jersey.guice.spi.container.GuiceComponentProviderFactory=WARN 
                        com.ning.http.client=WARN 
                        com.facebook.presto.server.PluginManager={Values.PrestoCoordinator.ExtraConfig.Holder["Log"]}"
                },
                {
                    "pulsar.properties",
                    $@"// name of the connector to be displayed in the catalog
                        connector.name=pulsar "
                }
            };
            // the url of Pulsar broker service
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled) 
                conf["pulsar.properties"] += $@" pulsar.broker-service-url=https://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["https"]}/";
            else
                conf["pulsar.properties"] += $@" pulsar.broker-service-url=http://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["http"]}/";

            // URI of Zookeeper cluster
            conf["pulsar.properties"] += $@" pulsar.zookeeper-uri={Values.ZooKeeper.ZooConnect}";
            // minimum number of entries to read at a single time
            conf["pulsar.properties"] += $@" pulsar.max-entry-read-batch-size={Values.PrestoCoordinator.ExtraConfig.Holder["maxEntryReadBatchSize"]}";
            // default number of splits to use per query
            conf["pulsar.properties"] +=  $" pulsar.target-num-splits={Values.PrestoCoordinator.ExtraConfig.Holder["targetNumSplits"] }";
            // max message queue size
            conf["pulsar.properties"] += $" pulsar.max-split-message-queue-size={ Values.PrestoCoordinator.ExtraConfig.Holder["maxSplitMessageQueueSize"] }";
            // max entry queue size
            conf["pulsar.properties"] += $" pulsar.max-split-entry-queue-size={Values.PrestoCoordinator.ExtraConfig.Holder["maxSplitEntryQueueSize"] } ";
            // Rewrite namespace delimiter
            // Warn: avoid using symbols allowed by Namespace (a-zA-Z_0-9 -=:%)
            // to prevent erroneous rewriting
            conf["pulsar.properties"] += $" pulsar.namespace-delimiter-rewrite-enable={Values.PrestoCoordinator.ExtraConfig.Holder["namespaceDelimiterRewriteEnable"] }";
            conf["pulsar.properties"] += $" pulsar.rewrite-namespace-delimiter={Values.PrestoCoordinator.ExtraConfig.Holder["rewriteNamespaceDelimiter"]}";
            ///////////// TIERED STORAGE OFFLOADER CONFIGS //////////////

            //// Driver to use to offload old data to long term storage
            //conf["pulsar.properties"] += $@" pulsar.managed-ledger-offload-driver=""aws-s3""";

            //// The directory to locate offloaders
            //conf["pulsar.properties"] += $@" pulsar.offloaders-directory="/pulsar/offloaders";

            //// Maximum number of thread pool threads for ledger offloading
            //conf["pulsar.properties"] += $@" pulsar.managed-ledger-offload-max-threads="2";

            //// Properties and configurations related to specific offloader implementation
            //conf["pulsar.properties"] += $@" pulsar.offloader-properties="{"s3ManagedLedgerOffloadBucket": "offload-bucket", "s3ManagedLedgerOffloadRegion": "us-west-2", "s3ManagedLedgerOffloadServiceEndpoint": "http://s3.amazonaws.com"}";
            ////////////// AUTHENTICATION CONFIGS //////////////

            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase))
            {
                //// the authentication plugin to be used to authenticate to Pulsar cluster
                conf["pulsar.properties"] += $@" pulsar.auth-plugin=org.apache.pulsar.client.impl.auth.AuthenticationToken";
                conf["pulsar.properties"] += $@" pulsar.auth-params=file:///pulsar/tokens/client/token";
            }
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
            {
                //// Accept untrusted TLS certificate
                conf["pulsar.properties"] += $@" pulsar.tls-allow-insecure-connection=false";
                //// Whether to enable hostname verification on TLS connections
                conf["pulsar.properties"] += $@" pulsar.tls-hostname-verification-enable=false";
                //// Path for the trusted TLS certificate file
                conf["pulsar.properties"] += $@" pulsar.tls-trust-cert-file-path=/pulsar/certs/ca/ca.crt";
            }
            ////////////// BOOKKEEPER CONFIGS //////////////

            // Entries read count throttling-limit per seconds, 0 is represents disable the throttle, default is 0.
            conf["pulsar.properties"] += $@" pulsar.bookkeeper-throttle-value={ Values.PrestoCoordinator.ExtraConfig.Holder["bookkeeperThrottleValue"] }";

            // The number of threads used by Netty to handle TCP connections,
            // default is 2 * Runtime.getRuntime().availableProcessors().
            // conf["pulsar.properties"] += $@" pulsar.bookkeeper-num-io-threads =";

            // The number of worker threads used by bookkeeper client to submit operations,
            // default is Runtime.getRuntime().availableProcessors().
            // conf["pulsar.properties"] += $@" pulsar.bookkeeper-num-worker-threads =";

            ////////////// MANAGED LEDGER CONFIGS //////////////

            // Amount of memory to use for caching data payload in managed ledger. This memory
            // is allocated from JVM direct memory and it's shared across all the managed ledgers
            // running in same sql worker. 0 is represents disable the cache, default is 0.
            conf["pulsar.properties"] += $@"npulsar.managed-ledger-cache-size-MB={Values.PrestoCoordinator.ExtraConfig.Holder["managedLedgerCacheSizeMB"]}";
            // Number of threads to be used for managed ledger tasks dispatching,
            // default is Runtime.getRuntime().availableProcessors().
            // conf["pulsar.properties"] += $@" pulsar.managed-ledger-num-worker-threads =";

            // Number of threads to be used for managed ledger scheduled tasks,
            // default is Runtime.getRuntime().availableProcessors().
            // conf["pulsar.properties"] += $@" pulsar.managed-ledger-num-scheduler-threads =";
            return conf;
        }
        public static IDictionary<string, string> PrestoWorker()
        {
            var conf = new Dictionary<string, string> 
            {
                {
                    "node.properties",
                    @"node.environment=production 
                     node.data-dir=/pulsar/data"
                },
                {
                    "jvm.config",
                    $@"-server 
                    -Xmx{Values.PrestoWorker.ExtraConfig.Holder["memory"]} 
                    -XX:+UseG1GC 
                    -XX:+UnlockExperimentalVMOptions 
                    -XX:+AggressiveOpts 
                    -XX:+DoEscapeAnalysis 
                    -XX:ParallelGCThreads=4 
                    -XX:ConcGCThreads=4 
                    -XX:G1NewSizePercent=50 
                    -XX:+DisableExplicitGC 
                    -XX:-ResizePLAB 
                    -XX:+ExitOnOutOfMemoryError 
                    -XX:+PerfDisableSharedMem"
                },
                {
                    "config.properties",
                    $@"coordinator=false 
                        http-server.http.port={Values.Ports.PrestoCoordinator["http"]} 
                         discovery.uri=http://{Values.PrestoCoordinator.ServiceName}:{Values.Ports.PrestoCoordinator["http"]}
                         query.max-memory={Values.PrestoWorker.ExtraConfig.Holder["maxMemory"]}
                         query.max-memory-per-node={ Values.PrestoWorker.ExtraConfig.Holder["maxMemoryPerNode"] }"                        
                },
                {
                    "log.properties",
                    $@"com.facebook.presto={Values.PrestoWorker.ExtraConfig.Holder["Log"]} 
                         com.sun.jersey.guice.spi.container.GuiceComponentProviderFactory=WARN 
                         com.ning.http.client=WARN 
                         com.facebook.presto.server.PluginManager={Values.PrestoWorker.ExtraConfig.Holder["Log"]}"
                },
                {
                    "pulsar.properties",
                    $@"// name of the connector to be displayed in the catalog
                        connector.name=pulsar "
                }
            };
            // the url of Pulsar broker service
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled) 
                conf["pulsar.properties"] += $@" pulsar.broker-service-url=https://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["https"]}/";
            else
                conf["pulsar.properties"] += $@" pulsar.broker-service-url=http://{Values.ReleaseName}-{Values.Broker.ComponentName}:{Values.Ports.Broker["http"]}/";

            // URI of Zookeeper cluster
            conf["pulsar.properties"] += $@" pulsar.zookeeper-uri={Values.ZooKeeper.ZooConnect}";
            // minimum number of entries to read at a single time
            conf["pulsar.properties"] += $@" pulsar.max-entry-read-batch-size={Values.PrestoCoordinator.ExtraConfig.Holder["maxEntryReadBatchSize"]}";
            // default number of splits to use per query
            conf["pulsar.properties"] +=  $" pulsar.target-num-splits={Values.PrestoCoordinator.ExtraConfig.Holder["targetNumSplits"] }";
            // max message queue size
            conf["pulsar.properties"] += $" pulsar.max-split-message-queue-size={ Values.PrestoCoordinator.ExtraConfig.Holder["maxSplitMessageQueueSize"] }";
            // max entry queue size
            conf["pulsar.properties"] += $" pulsar.max-split-entry-queue-size={Values.PrestoCoordinator.ExtraConfig.Holder["maxSplitEntryQueueSize"] } ";
            // Rewrite namespace delimiter
            // Warn: avoid using symbols allowed by Namespace (a-zA-Z_0-9 -=:%)
            // to prevent erroneous rewriting
            conf["pulsar.properties"] += $" pulsar.namespace-delimiter-rewrite-enable={Values.PrestoCoordinator.ExtraConfig.Holder["namespaceDelimiterRewriteEnable"] }";
            conf["pulsar.properties"] += $" pulsar.rewrite-namespace-delimiter={Values.PrestoCoordinator.ExtraConfig.Holder["rewriteNamespaceDelimiter"]}";
            ///////////// TIERED STORAGE OFFLOADER CONFIGS //////////////

            //// Driver to use to offload old data to long term storage
            //conf["pulsar.properties"] += $@" pulsar.managed-ledger-offload-driver=""aws-s3""";

            //// The directory to locate offloaders
            //conf["pulsar.properties"] += $@" pulsar.offloaders-directory="/pulsar/offloaders";

            //// Maximum number of thread pool threads for ledger offloading
            //conf["pulsar.properties"] += $@" pulsar.managed-ledger-offload-max-threads="2";

            //// Properties and configurations related to specific offloader implementation
            //conf["pulsar.properties"] += $@" pulsar.offloader-properties="{"s3ManagedLedgerOffloadBucket": "offload-bucket", "s3ManagedLedgerOffloadRegion": "us-west-2", "s3ManagedLedgerOffloadServiceEndpoint": "http://s3.amazonaws.com"}";
            ////////////// AUTHENTICATION CONFIGS //////////////

            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase))
            {
                //// the authentication plugin to be used to authenticate to Pulsar cluster
                conf["pulsar.properties"] += $@" pulsar.auth-plugin=org.apache.pulsar.client.impl.auth.AuthenticationToken";
                conf["pulsar.properties"] += $@" pulsar.auth-params=file:///pulsar/tokens/client/token";
            }
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
            {
                //// Accept untrusted TLS certificate
                conf["pulsar.properties"] += $@" pulsar.tls-allow-insecure-connection=false";
                //// Whether to enable hostname verification on TLS connections
                conf["pulsar.properties"] += $@" pulsar.tls-hostname-verification-enable=false";
                //// Path for the trusted TLS certificate file
                conf["pulsar.properties"] += $@" pulsar.tls-trust-cert-file-path=/pulsar/certs/ca/ca.crt";
            }
            ////////////// BOOKKEEPER CONFIGS //////////////

            // Entries read count throttling-limit per seconds, 0 is represents disable the throttle, default is 0.
            conf["pulsar.properties"] += $@" pulsar.bookkeeper-throttle-value={ Values.PrestoCoordinator.ExtraConfig.Holder["bookkeeperThrottleValue"] }";

            // The number of threads used by Netty to handle TCP connections,
            // default is 2 * Runtime.getRuntime().availableProcessors().
            // conf["pulsar.properties"] += $@" pulsar.bookkeeper-num-io-threads =";

            // The number of worker threads used by bookkeeper client to submit operations,
            // default is Runtime.getRuntime().availableProcessors().
            // conf["pulsar.properties"] += $@" pulsar.bookkeeper-num-worker-threads =";

            ////////////// MANAGED LEDGER CONFIGS //////////////

            // Amount of memory to use for caching data payload in managed ledger. This memory
            // is allocated from JVM direct memory and it's shared across all the managed ledgers
            // running in same sql worker. 0 is represents disable the cache, default is 0.
            conf["pulsar.properties"] += $@"npulsar.managed-ledger-cache-size-MB={Values.PrestoCoordinator.ExtraConfig.Holder["managedLedgerCacheSizeMB"]}";
            // Number of threads to be used for managed ledger tasks dispatching,
            // default is Runtime.getRuntime().availableProcessors().
            // conf["pulsar.properties"] += $@" pulsar.managed-ledger-num-worker-threads =";

            // Number of threads to be used for managed ledger scheduled tasks,
            // default is Runtime.getRuntime().availableProcessors().
            // conf["pulsar.properties"] += $@" pulsar.managed-ledger-num-scheduler-threads =";
            conf.Add("health_check.sh",
                $@"#!/bin/bash curl --silent {Values.PrestoCoordinator.ServiceName}:{Values.Ports.PrestoCoordinator["http"]}/v1/node | tr "", "" ""\n"" | grep --silent $(hostname -i)");
            return conf;
        }
    }
}
