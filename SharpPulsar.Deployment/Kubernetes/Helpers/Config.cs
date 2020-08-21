﻿using SharpPulsar.Deployment.Kubernetes.Extensions;
using SharpPulsar.Deployment.Kubernetes.Prometheus;
using System;
using System.Collections.Generic;
using System.Linq;

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
                {"PULSAR_MEM", @"-Xms64m
-Xmx128m"},
                {"PULSAR_GC", @"-XX:+UseG1GC
-XX:MaxGCPauseMillis=10
-Dcom.sun.management.jmxremote
-Djute.maxbuffer=10485760
-XX:+ParallelRefProcEnabled
-XX:+UnlockExperimentalVMOptions
-XX:+AggressiveOpts
-XX:+DoEscapeAnalysis
-XX:+DisableExplicitGC
-XX:+PerfDisableSharedMem
-Dzookeeper.forceSync=no" }
            };
            if ((bool)Values.ExtraConfigs.ZooKeeper.Holder["UseSeparateDiskForTxlog"])
                zk.Add("PULSAR_PREFIX_dataLogDir", "/pulsar/data/zookeeper-datalog");
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                zk.Add("secureClientPort", Values.Ports.ZooKeeper["client-tls"].ToString());
                zk.Add("PULSAR_PREFIX_secureClientPort", Values.Ports.ZooKeeper["client-tls"].ToString());
            }
            if ((bool)Values.ExtraConfigs.ZooKeeper.Holder["Reconfig"])
            {
                zk.Add("PULSAR_PREFIX_reconfigEnabled", "true");
                zk.Add("PULSAR_PREFIX_quorumListenOnAllIPs", "true");
            }
            return zk.AddRange(Values.Settings.ZooKeeper.ExtraConfigMap);
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
                            {"BOOKIE_MEM", @"-Xms128m
-Xmx256m
-XX:MaxDirectMemorySize=256m"},
                            {"PULSAR_MEM", @"-Xms128m
-Xmx256m
-XX:MaxDirectMemorySize=256m"},
                            {"PULSAR_GC", @"-XX:+UseG1GC
-XX:MaxGCPauseMillis=10
-XX:+ParallelRefProcEnabled
-XX:+UnlockExperimentalVMOptions
-XX:+AggressiveOpts
-XX:+DoEscapeAnalysis
-XX:ParallelGCThreads=4
-XX:ConcGCThreads=4
-XX:G1NewSizePercent=50
-XX:+DisableExplicitGC
-XX:-ResizePLAB
-XX:+ExitOnOutOfMemoryError
-XX:+PerfDisableSharedMem
-XX:+PrintGCDetails
-XX:+PrintGCTimeStamps
-XX:+PrintGCApplicationStoppedTime
-XX:+PrintHeapAtGC
-verbosegc
-Xloggc:/var/log/bookie-gc.log
-XX:G1LogLevel=finest" }
                        };
            //disable auto recovery on bookies since we will start AutoRecovery in separated pods
            if (Values.Settings.Autorecovery.Enabled)
                conf.Add("autoRecoveryDaemonEnabled", "false");
            if(Values.Tls.Enabled && Values.Tls.Bookie.Enabled)
            {
                conf.Add("zkServers", $"{Values.Settings.ZooKeeper.Service}:{Values.Ports.ZooKeeper["client-tls"]}");
                conf.Add("PULSAR_PREFIX_tlsProviderFactoryClass", "org.apache.bookkeeper.tls.TLSContextFactory");
                conf.Add("PULSAR_PREFIX_tlsCertificatePath", @"/pulsar/certs/bookie/tls.crt");
                conf.Add("PULSAR_PREFIX_tlsKeyStoreType", "PEM");
                conf.Add("PULSAR_PREFIX_tlsKeyStore", "/pulsar/certs/bookie/tls.key");
                conf.Add("PULSAR_PREFIX_tlsTrustStoreType", "PEM");
                conf.Add("PULSAR_PREFIX_tlsTrustStore", "/pulsar/certs/ca/ca.crt");
            }
            else
                conf.Add("zkServers", $"{Values.Settings.ZooKeeper.Service}:{Values.Ports.ZooKeeper["client"]}");
            return conf.AddRange(Values.Settings.BookKeeper.ExtraConfigMap);
        }
        public static IDictionary<string, string> Broker()
        {
            var conf = new Dictionary<string, string>
                        {
                            {"zookeeperServers", $"{Values.Settings.ZooKeeper.ZooConnect}{Values.MetadataPrefix}"},
                            {"clusterName", $"{Values.Cluster}"},
                            {"exposeTopicLevelMetricsInPrometheus", "true"},
                            {"numHttpServerThreads", "8"},
                            {"zooKeeperSessionTimeoutMillis", "30000"},
                            {"statusFilePath", "/pulsar/status"}
                            
                        };
            conf.Add("PULSAR_MEM", @"
-Xms128m
-Xmx256m
-XX:MaxDirectMemorySize=256m");
            conf.Add("PULSAR_GC", @"
-XX:+UseG1GC
-XX:MaxGCPauseMillis=10
-Dio.netty.leakDetectionLevel=disabled
-Dio.netty.recycler.linkCapacity=1024
-XX:+ParallelRefProcEnabled
-XX:+UnlockExperimentalVMOptions
-XX:+AggressiveOpts
-XX:+DoEscapeAnalysis
-XX:ParallelGCThreads=4
-XX:ConcGCThreads=4
-XX:G1NewSizePercent=50
-XX:+DisableExplicitGC
-XX:-ResizePLAB
-XX:+ExitOnOutOfMemoryError
-XX:+PerfDisableSharedMem");
            conf.Add("AWS_ACCESS_KEY_ID", "[YOUR AWS ACCESS KEY ID]");
            conf.Add("AWS_SECRET_ACCESS_KEY", "[YOUR SECRET]");
            conf.Add("managedLedgerDefaultEnsembleSize", "3");
            conf.Add("managedLedgerDefaultWriteQuorum", "3");
            conf.Add("managedLedgerDefaultAckQuorum", "2");
            conf.Add("subscriptionKeySharedUseConsistentHashing", "true");
            if (Values.Settings.Broker.Offload.Enabled)
            {
                conf.Add("offloadersDirectory", "/pulsar/offloaders");
                conf.Add("managedLedgerOffloadDriver", $"{ Values.Settings.Broker.Offload.ManagedLedgerOffloadDriver }");
                if (Values.Settings.Broker.Offload.Gcs.Enabled)
                {
                    conf.Add("gcsManagedLedgerOffloadRegion", $"{ Values.Settings.Broker.Offload.Gcs.Region} ");
                    conf.Add("gcsManagedLedgerOffloadBucket", $"{Values.Settings.Broker.Offload.Gcs.Bucket }");
                    conf.Add("gcsManagedLedgerOffloadMaxBlockSizeInBytes", $"{ Values.Settings.Broker.Offload.Gcs.MaxBlockSizeInBytes}");
                    conf.Add("gcsManagedLedgerOffloadReadBufferSizeInBytes", $"{ Values.Settings.Broker.Offload.Gcs.ReadBufferSizeInBytes}");
                    // Authentication with GCS
                    conf.Add("gcsManagedLedgerOffloadServiceAccountKeyFile", $"/pulsar/srvaccts/gcs.json");
                                
                }
                if (Values.Settings.Broker.Offload.S3.Enabled)
                {
                     conf.Add("s3ManagedLedgerOffloadRegion", $"{Values.Settings.Broker.Offload.S3.Region}");
                     conf.Add("s3ManagedLedgerOffloadBucket", $"{Values.Settings.Broker.Offload.S3.Bucket}");
                     conf.Add("s3ManagedLedgerOffloadMaxBlockSizeInBytes", $"{Values.Settings.Broker.Offload.S3.MaxBlockSizeInBytes}");
                     conf.Add("s3ManagedLedgerOffloadReadBufferSizeInBytes", $"{Values.Settings.Broker.Offload.S3.ReadBufferSizeInBytes}");                     
                }
            }

            if (Values.Settings.Function.Enabled)
            {
                conf.Add("functionsWorkerEnabled", "true");
                conf.Add("PF_functionRuntimeFactoryClassName", "org.apache.pulsar.functions.runtime.kubernetes.KubernetesRuntimeFactory");
                conf.Add("PF_pulsarFunctionsCluster", $"{Values.ReleaseName}");
                conf.Add("PF_connectorsDirectory", "./connectors");
                conf.Add("PF_containerFactory", "k8s");
                conf.Add("PF_numFunctionPackageReplicas", $"{conf["managedLedgerDefaultEnsembleSize"]}");

                if (Values.Settings.Broker.EnableFunctionCustomizerRuntime)
                {
                    conf.Add("PF_runtimeCustomizerClassName", $"{ Values.Settings.Broker.RuntimeCustomizerClassName}");
                    conf.Add("PULSAR_EXTRA_CLASSPATH", $"/pulsar/{Values.Settings.Broker.PulsarFunctionsExtraClasspath}");
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
                    conf.Add("PF_functionRuntimeFactoryConfigs_pulsarAdminUrl", $"http://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["http"]}/");
                    conf.Add("PF_functionRuntimeFactoryConfigs_pulsarServiceUrl", $"pulsar://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["pulsar"]}/");
                }
                if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
                {
                    conf.Add("PF_functionRuntimeFactoryConfigs_pulsarAdminUrl", $"https://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["https"]}/");
                    conf.Add("PF_functionRuntimeFactoryConfigs_pulsarServiceUrl", $"pulsar+ssl://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["pulsarssl"]}/");
                }
                conf.Add("PF_functionRuntimeFactoryConfigs_changeConfigMap", $"{Values.ReleaseName}-{Values.Settings.Function.Name}-config");
                conf.Add("PF_functionRuntimeFactoryConfigs_changeConfigMapNamespace", $"{Values.Namespace}");

            }
            else
                conf.Add("functionsWorkerEnabled", "false");

            conf.Add("webServicePort", $"{ Values.Ports.Broker["http"] }");
            if(!Values.Tls.Enabled && !Values.Tls.Broker.Enabled)
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
            if (Values.Settings.Kop.Enabled)
            {
                conf.Add("messagingProtocols", "kafka");
                if (Values.Authentication.Enabled)
                {
                    if(Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase))
                        conf.Add("PULSAR_PREFIX_saslAllowedMechanisms", "PLAIN");

                }
                if (Values.Tls.Enabled || Values.Tls.Broker.Enabled)
                {
                    conf.Add("PULSAR_PREFIX_kopSslKeystoreLocation", "/pulsar/broker.keystore.jks");
                    conf.Add("PULSAR_PREFIX_kopSslTruststoreLocation", "/pulsar/broker.truststore.jks");
                }
            }
            
            return conf.AddRange(Values.Settings.Broker.ExtraConfigMap);
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
                if(!string.IsNullOrWhiteSpace(Values.Settings.Proxy.ProxyServiceUrl.BrokerPulsarUrl))
                    conf.Add("brokerServiceURL", Values.Settings.Proxy.ProxyServiceUrl.BrokerPulsarUrl);
                else
                    conf.Add("brokerServiceURL", $"pulsar://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["pulsar"]}");
                
                if(!string.IsNullOrWhiteSpace(Values.Settings.Proxy.ProxyServiceUrl.BrokerHttpUrl))
                    conf.Add("brokerWebServiceURL", Values.Settings.Proxy.ProxyServiceUrl.BrokerHttpUrl);
                else
                    conf.Add("brokerWebServiceURL", $"http://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["http"]}");

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

                if (!string.IsNullOrWhiteSpace(Values.Settings.Proxy.ProxyServiceUrl.BrokerPulsarSslUrl))
                    conf.Add("brokerServiceURLTLS", Values.Settings.Proxy.ProxyServiceUrl.BrokerPulsarSslUrl);
                else
                    conf.Add("brokerServiceURLTLS", $"pulsar+ssl://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["pulsarssl"]}");

                if (!string.IsNullOrWhiteSpace(Values.Settings.Proxy.ProxyServiceUrl.BrokerHttpsUrl))
                    conf.Add("brokerWebServiceURLTLS", Values.Settings.Proxy.ProxyServiceUrl.BrokerHttpsUrl);
                else
                    conf.Add("brokerWebServiceURLTLS", $"https://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["https"]}");

                conf.Add("tlsEnabledWithBroker", "true");
                conf.Add("tlsCertRefreshCheckDurationSec", "300");
                conf.Add("brokerClientTrustCertsFilePath", "/pulsar/certs/ca/ca.crt");
            }
            if (!Values.Tls.Enabled && !Values.Tls.Broker.Enabled)
            {

                if (!string.IsNullOrWhiteSpace(Values.Settings.Proxy.ProxyServiceUrl.BrokerPulsarUrl))
                    conf["brokerServiceURL"] =  Values.Settings.Proxy.ProxyServiceUrl.BrokerPulsarUrl;
                else
                    conf["brokerServiceURL"] = $"pulsar://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["pulsar"]}";

                if (!string.IsNullOrWhiteSpace(Values.Settings.Proxy.ProxyServiceUrl.BrokerHttpUrl))
                    conf["brokerWebServiceURL"] =  Values.Settings.Proxy.ProxyServiceUrl.BrokerHttpUrl;
                else
                    conf["brokerWebServiceURL"] = $"http://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["http"]}";

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

            conf.Add("PULSAR_MEM", @"
-Xms64m
-Xmx64m
-XX:MaxDirectMemorySize=64m");
            conf.Add("PULSAR_GC", @"
-XX:+UseG1GC
-XX:MaxGCPauseMillis=10
-Dio.netty.leakDetectionLevel=disabled
-Dio.netty.recycler.linkCapacity=1024
-XX:+ParallelRefProcEnabled
-XX:+UnlockExperimentalVMOptions
-XX:+AggressiveOpts
-XX:+DoEscapeAnalysis
-XX:ParallelGCThreads=4
-XX:ConcGCThreads=4
-XX:G1NewSizePercent=50
-XX:+DisableExplicitGC
-XX:-ResizePLAB
-XX:+ExitOnOutOfMemoryError
-XX:+PerfDisableSharedMem");
            return conf.AddRange(Values.Settings.Proxy.ExtraConfigMap);
        }
        public static IDictionary<string, string> PrestoCoord(string schedule)
        {
            var conf = new Dictionary<string, string> 
            {
                {
                    "node.properties",@"
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# ""License""); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# ""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#
node.environment=production
node.data-dir=/pulsar/data"
                },
                {
                    "jvm.config",$@"
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# ""License""); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# ""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#
-server
-Xmx{Values.ExtraConfigs.PrestoCoordinator.Holder["memory"]}
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
                    "config.properties", $@"
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# ""License""); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# ""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#
coordinator=true
http-server.http.port={Values.Ports.PrestoCoordinator["http"]}
discovery-server.enabled=true
discovery.uri=http://{Values.Settings.PrestoCoord.Service}:{Values.Ports.PrestoCoordinator["http"]}
query.max-memory={Values.ExtraConfigs.PrestoCoordinator.Holder["maxMemory"]}
query.max-memory-per-node={ Values.ExtraConfigs.PrestoCoordinator.Holder["maxMemoryPerNode"] }
distributed-joins-enabled=true
node-scheduler.include-coordinator={schedule}"
                },
                {
                    "log.properties", $@"
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# ""License""); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# ""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#
com.facebook.presto={Values.ExtraConfigs.PrestoCoordinator.Holder["Log"]}
com.sun.jersey.guice.spi.container.GuiceComponentProviderFactory=WARN
com.ning.http.client=WARN
com.facebook.presto.server.PluginManager={Values.ExtraConfigs.PrestoCoordinator.Holder["Log"]}"
                },
                {
                    "pulsar.properties", $@"
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# ""License""); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# ""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#

# name of the connector to be displayed in the catalog
connector.name=pulsar

# URI of Zookeeper cluster
pulsar.zookeeper-uri={Values.Settings.ZooKeeper.ZooConnect}
# minimum number of entries to read at a single time
pulsar.max-entry-read-batch-size={Values.ExtraConfigs.PrestoCoordinator.Holder["maxEntryReadBatchSize"]}

# default number of splits to use per query
pulsar.target-num-splits={Values.ExtraConfigs.PrestoCoordinator.Holder["targetNumSplits"] }
# max message queue size
pulsar.max-split-message-queue-size={ Values.ExtraConfigs.PrestoCoordinator.Holder["maxSplitMessageQueueSize"] }
# max entry queue size
pulsar.max-split-entry-queue-size={Values.ExtraConfigs.PrestoCoordinator.Holder["maxSplitEntryQueueSize"] }

# Rewrite namespace delimiter
# Warn: avoid using symbols allowed by Namespace (a-zA-Z_0-9 -=:%)
# to prevent erroneous rewriting
pulsar.namespace-delimiter-rewrite-enable={Values.ExtraConfigs.PrestoCoordinator.Holder["namespaceDelimiterRewriteEnable"] }
pulsar.rewrite-namespace-delimiter={Values.ExtraConfigs.PrestoCoordinator.Holder["rewriteNamespaceDelimiter"]}

"
                }
            };
            
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled) 
                conf["pulsar.properties"] += $@"
# the url of Pulsar broker service
pulsar.broker-service-url=https://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["https"]}/";
            else
                conf["pulsar.properties"] += $@"
pulsar.broker-service-url=http://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["http"]}/";

            conf["pulsar.properties"] += @"
#### TIERED STORAGE OFFLOADER CONFIGS ####
## Driver to use to offload old data to long term storage
# pulsar.managed - ledger - offload - driver = ""aws - s3""
## The directory to locate offloaders
# pulsar.offloaders-directory=""/pulsar/offloaders""
## Maximum number of thread pool threads for ledger offloading
# pulsar.managed-ledger-offload-max-threads=""2""
## Properties and configurations related to specific offloader implementation
# pulsar.offloader-properties=""{""s3ManagedLedgerOffloadBucket"": ""offload-bucket"", ""s3ManagedLedgerOffloadRegion"": ""us-west-2"", ""s3ManagedLedgerOffloadServiceEndpoint"": ""http://s3.amazonaws.com""}""

##### AUTHENTICATION CONFIGS #######

";
            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase))
            {
                conf["pulsar.properties"] += $@"
## the authentication plugin to be used to authenticate to Pulsar cluster
pulsar.auth-plugin=org.apache.pulsar.client.impl.auth.AuthenticationToken
pulsar.auth-params=file:///pulsar/tokens/client/token";
            }
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
            {                
                conf["pulsar.properties"] += $@"
## Accept untrusted TLS certificate
pulsar.tls-allow-insecure-connection=false
## Whether to enable hostname verification on TLS connections
pulsar.tls-hostname-verification-enable=false
## Path for the trusted TLS certificate file
pulsar.tls-trust-cert-file-path=/pulsar/certs/ca/ca.crt

##### BOOKKEEPER CONFIGS #####
## Entries read count throttling-limit per seconds, 0 is represents disable the throttle, default is 0.
";
            }           
            conf["pulsar.properties"] += $@"
pulsar.bookkeeper-throttle-value={ Values.ExtraConfigs.PrestoCoordinator.Holder["bookkeeperThrottleValue"] }

# The number of threads used by Netty to handle TCP connections,
# default is 2 * Runtime.getRuntime().availableProcessors().

# pulsar.bookkeeper-num-io-threads =
# The number of worker threads used by bookkeeper client to submit operations,
# default is Runtime.getRuntime().availableProcessors().
# pulsar.bookkeeper-num-worker-threads =

##### MANAGED LEDGER CONFIGS ###### 

# Amount of memory to use for caching data payload in managed ledger. This memory
# is allocated from JVM direct memory and its shared across all the managed ledgers
# running in same sql worker. 0 is represents disable the cache, default is 0.
pulsar.managed-ledger-cache-size-MB={Values.ExtraConfigs.PrestoCoordinator.Holder["managedLedgerCacheSizeMB"]}
# Number of threads to be used for managed ledger tasks dispatching,
# default is Runtime.getRuntime().availableProcessors().
# pulsar.managed-ledger-num-worker-threads =

# Number of threads to be used for managed ledger scheduled tasks,
# default is Runtime.getRuntime().availableProcessors().
# pulsar.managed-ledger-num-scheduler-threads =";
            return conf;
        }
        public static IDictionary<string, string> PrestoWorker()
        {
            var conf = new Dictionary<string, string> 
            {
                {
                    "node.properties",@"
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# ""License""); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# ""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#
node.environment=production
node.data-dir=/pulsar/data"
                },
                {
                    "jvm.config",$@"
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# ""License""); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# ""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#
-server
-Xmx{Values.ExtraConfigs.PrestoWorker.Holder["memory"]}
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
                    "config.properties",$@"
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# ""License""); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# ""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#
coordinator=false
http-server.http.port={Values.Ports.PrestoCoordinator["http"]}
discovery.uri=http://{Values.Settings.PrestoCoord.Service}:{Values.Ports.PrestoCoordinator["http"]}
query.max-memory={Values.ExtraConfigs.PrestoWorker.Holder["maxMemory"]}
query.max-memory-per-node={ Values.ExtraConfigs.PrestoWorker.Holder["maxMemoryPerNode"] }"                        
                },
                {
                    "log.properties",$@"
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# ""License""); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# ""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#
com.facebook.presto={Values.ExtraConfigs.PrestoWorker.Holder["Log"]}
com.sun.jersey.guice.spi.container.GuiceComponentProviderFactory=WARN
com.ning.http.client=WARN
com.facebook.presto.server.PluginManager={Values.ExtraConfigs.PrestoWorker.Holder["Log"]}"
                },
                {
                    "pulsar.properties", $@"

#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# ""License""); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# ""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#
# name of the connector to be displayed in the catalog
connector.name=pulsar"
                }
            };
            
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled) 
                conf["pulsar.properties"] += $@"
# the url of Pulsar broker service
pulsar.broker-service-url=https://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["https"]}/";
            else
                conf["pulsar.properties"] += $@"
pulsar.broker-service-url=http://{Values.ReleaseName}-{Values.Settings.Broker.Name}:{Values.Ports.Broker["http"]}/";

            conf["pulsar.properties"] += $@"
# URI of Zookeeper cluster
pulsar.zookeeper-uri={Values.Settings.ZooKeeper.ZooConnect}
# minimum number of entries to read at a single time
pulsar.max-entry-read-batch-size={Values.ExtraConfigs.PrestoCoordinator.Holder["maxEntryReadBatchSize"]}
# default number of splits to use per query
pulsar.target-num-splits={Values.ExtraConfigs.PrestoCoordinator.Holder["targetNumSplits"] }
# max message queue size
pulsar.max-split-message-queue-size={ Values.ExtraConfigs.PrestoCoordinator.Holder["maxSplitMessageQueueSize"] }
# max entry queue size
pulsar.max-split-entry-queue-size={Values.ExtraConfigs.PrestoCoordinator.Holder["maxSplitEntryQueueSize"] }

# Rewrite namespace delimiter
# Warn: avoid using symbols allowed by Namespace (a-zA-Z_0-9 -=:%)
# to prevent erroneous rewriting
pulsar.namespace-delimiter-rewrite-enable={Values.ExtraConfigs.PrestoCoordinator.Holder["namespaceDelimiterRewriteEnable"] }
pulsar.rewrite-namespace-delimiter={Values.ExtraConfigs.PrestoCoordinator.Holder["rewriteNamespaceDelimiter"]}

###### TIERED STORAGE OFFLOADER CONFIGS #######

## Driver to use to offload old data to long term storage
# pulsar.managed-ledger-offload-driver=""aws-s3""

##The directory to locate offloaders
# pulsar.offloaders-directory=""/pulsar/offloaders""

## Maximum number of thread pool threads for ledger offloading
#pulsar.managed-ledger-offload-max-threads=""2""

## Properties and configurations related to specific offloader implementation
# pulsar.offloader-properties=""{{""s3ManagedLedgerOffloadBucket"": ""offload-bucket"", ""s3ManagedLedgerOffloadRegion"": ""us-west-2"", ""s3ManagedLedgerOffloadServiceEndpoint"": ""http://s3.amazonaws.com""}}""

####### AUTHENTICATION CONFIGS #######";

            if (Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt", StringComparison.OrdinalIgnoreCase))
            {
                
                conf["pulsar.properties"] += $@"
## the authentication plugin to be used to authenticate to Pulsar cluster
pulsar.auth-plugin=org.apache.pulsar.client.impl.auth.AuthenticationToken
pulsar.auth-params=file:///pulsar/tokens/client/token";
            }
            if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
            {
                conf["pulsar.properties"] += $@"
## Accept untrusted TLS certificate
pulsar.tls-allow-insecure-connection=false
## Whether to enable hostname verification on TLS connections
pulsar.tls-hostname-verification-enable=false
## Path for the trusted TLS certificate file
pulsar.tls-trust-cert-file-path=/pulsar/certs/ca/ca.crt";
            }
            conf["pulsar.properties"] += $@"

##### BOOKKEEPER CONFIGS #####

# Entries read count throttling-limit per seconds, 0 is represents disable the throttle, default is 0.
            
pulsar.bookkeeper-throttle-value={ Values.ExtraConfigs.PrestoCoordinator.Holder["bookkeeperThrottleValue"] }
# The number of threads used by Netty to handle TCP connections,
# default is 2 * Runtime.getRuntime().availableProcessors().
# pulsar.bookkeeper-num-io-threads=

# The number of worker threads used by bookkeeper client to submit operations,
# default is Runtime.getRuntime().availableProcessors().
# pulsar.bookkeeper-num-worker-threads =

######## MANAGED LEDGER CONFIGS ########

# Amount of memory to use for caching data payload in managed ledger. This memory
# is allocated from JVM direct memory and its shared across all the managed ledgers
# running in same sql worker. 0 is represents disable the cache, default is 0.
pulsar.managed-ledger-cache-size-MB={Values.ExtraConfigs.PrestoCoordinator.Holder["managedLedgerCacheSizeMB"]}
# Number of threads to be used for managed ledger tasks dispatching,
# default is Runtime.getRuntime().availableProcessors().
# pulsar.managed-ledger-num-worker-threads = 
# Number of threads to be used for managed ledger scheduled tasks,
# default is Runtime.getRuntime().availableProcessors().
# pulsar.managed-ledger-num-scheduler-threads =";
            conf["health_check.sh"] =  $@" 
#!/bin/bash
curl --silent {Values.Settings.PrestoCoord.Service}:{Values.Ports.PrestoCoordinator["http"]}/v1/node | tr "", "" ""\n"" | grep --silent $(hostname -i)";
            return conf;
        }

        public static IDictionary<string, string> PrometheusOld(RuleGroup rules = default)
        {
            var config = new Dictionary<string, string>();
            var yamlDict = new Dictionary<string, object>
            {
                {"global", new Dictionary<string, string>{ { "scrape_interval", "15s" } } }
            };
            if (Values.Monitoring.AlertManager)
            {
                yamlDict.Add("rule_files", new List<string> {"'rules.yml'" });
                yamlDict.Add("alerting", new Dictionary<string, object> 
                {
                    {"alertmanagers", new Dictionary<string, object>
                        {
                            {"static_configs", new Dictionary<string, object> 
                                {
                                    {"targets", $"['{Values.ReleaseName}-{Values.Settings.AlertManager.Name}:{Values.Ports.AlertManager["http"]}']" }
                                } 
                            },
                            {"path_prefix", $"{Values.Monitoring.AlertManagerPath}/"}
                        } 
                    }
                });
            }
            var scrapeConfigs = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"job_name","'prometheus'" },
                    {"static_configs", new Dictionary<string, object>
                        {
                            {"targets", $"['localhost:{Values.Ports.Prometheus["http"]}']" }
                        }
                    },
                    {"metrics_path", ""/*because it will be subdomain*/ }
                },
                new Dictionary<string, object>
                {
                    {"job_name", "'kubernetes-pods'"},
                    {"kubernetes_sd_configs", new Dictionary<string, string>{ { "role", "pod" } } },
                    {"relabel_configs", new List<Dictionary<string, object>> 
                        { 
                            new Dictionary<string, object>
                            {
                                {"source_labels", "[__meta_kubernetes_pod_annotation_prometheus_io_scrape]"  },
                                {"action", "keep" },
                                {"regex", true}
                            },
                            new Dictionary<string, object>
                            {
                                {"source_labels", "[__meta_kubernetes_pod_annotation_prometheus_io_path]" },
                                {"action", "replace" },
                                {"target_label", "__metrics_path__"},
                                {"regex", "(.+)"}
                            },
                            new Dictionary<string, object>
                            {
                                {"source_labels", "[__address__,__meta_kubernetes_pod_annotation_prometheus_io_port]"},
                                {"action", "replace" },
                                {"regex", @"([^:]+)(?::\d+)?;(\d+)"},
                                {"replacement", "$1:$2"},
                                {"target_label", "__address__"}
                            },
                            new Dictionary<string, object>
                            {
                                {"action", "labelmap" },
                                {"regex", "__meta_kubernetes_pod_label_(.+)"}
                            },
                            new Dictionary<string, object>
                            {
                                {"source_labels", "[__meta_kubernetes_namespace]" },
                                {"action", "replace" },
                                {"target_label", "kubernetes_namespace"}
                            },
                            new Dictionary<string, object>
                            {
                                {"source_labels", "[__meta_kubernetes_pod_label_component]" },
                                {"action", "replace" },
                                {"target_label", "job"}
                            },
                            new Dictionary<string, object>
                            {
                                {"source_labels", "[__meta_kubernetes_pod_name]" },
                                {"action", "replace" },
                                {"target_label", "kubernetes_pod_name"}
                            }
                        } 
                    }
                },
                new Dictionary<string, object>
                {
                    {"job_name", "'kubernetes-nodes'"},
                    {"scheme", "https"},
                    {"kubernetes_sd_configs", new Dictionary<string, string>{ { "role", "node" } } },
                    {"tls_config", new Dictionary<string, string>{ { "ca_file", "/var/run/secrets/kubernetes.io/serviceaccount/ca.crt" } } },
                    {"bearer_token_file", "/var/run/secrets/kubernetes.io/serviceaccount/token"},
                    {"relabel_configs", new List<Dictionary<string, object>> 
                        { 
                            new Dictionary<string, object>
                            {
                                {"action", "labelmap" },
                                {"regex", "__meta_kubernetes_node_label_(.+)"}
                            },
                            new Dictionary<string, object>
                            {
                                {"target_label", "__address__"},
                                {"replacement", "kubernetes.default.svc:443"}
                            },
                            new Dictionary<string, object>
                            {
                                {"source_labels", "[__meta_kubernetes_node_name]" },
                                {"regex", "(.+)"},
                                {"target_label", "__metrics_path__"},
                                {"replacement", "/api/v1/nodes/${1}/proxy/metrics"}
                            }
                        } 
                    }
                },
                new Dictionary<string, object>
                {
                    {"job_name", "'kubernetes-cadvisor'"},
                    {"scheme", "https"},
                    {"kubernetes_sd_configs", new Dictionary<string, string>{ { "role", "node" } }  },
                    {"tls_config", new Dictionary<string, string>{ { "ca_file", "/var/run/secrets/kubernetes.io/serviceaccount/ca.crt" } } },
                    {"bearer_token_file", "/var/run/secrets/kubernetes.io/serviceaccount/token"},
                    {"relabel_configs", new List<Dictionary<string, object>> 
                        { 
                            new Dictionary<string, object>
                            {
                                {"action", "labelmap" },
                                {"regex", "__meta_kubernetes_node_label_(.+)"}
                            },
                            new Dictionary<string, object>
                            {
                                {"target_label", "__address__"},
                                {"replacement", "kubernetes.default.svc:443"}
                            },
                            new Dictionary<string, object>
                            {
                                {"source_labels", "[__meta_kubernetes_node_name]" },
                                {"regex", "(.+)"},
                                {"target_label", "__metrics_path__"},
                                {"replacement", "/api/v1/nodes/${1}/proxy/metrics/cadvisor"}
                            }
                        } 
                    }
                }
            };
            if (Values.Settings.PulsarDetector.Enabled)
            {
                var ls = (Dictionary<string, List<string>>)scrapeConfigs.First()["static_configs"];
                ls["targets"].Add($"{Values.ReleaseName}-{Values.Settings.PulsarDetector.Name}:{Values.Ports.PulsarDetector["http"]}");
                scrapeConfigs.First()["static_configs"] = ls;
            }
            if(Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt"))
            {
                scrapeConfigs[1].Add("bearer_token_file", "/pulsar/tokens/client/token");
            }
            yamlDict["scrape_configs"] = scrapeConfigs;
            var serializer = new YamlDotNet.Serialization.Serializer();
            var serialized = serializer.Serialize(yamlDict);
            config.Add("prometheus.yml", serialized);
            if(rules != null)
            {
                var rls = serializer.Serialize(rules.Data);
                config.Add("rules.yml", rls);
            }
            else
                config.Add("rules.yml", "");
            return config;
        }

        public static IDictionary<string,string> Prometheus2()
        {
            var token = Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt") ? "bearer_token_file: /pulsar/tokens/client/token" : "";
            var prometheus = $@"
global:
  scrape_interval: 15s
scrape_configs:
- job_name: 'prometheus'
  static_configs:
    targets: ['localhost:{Values.Ports.Prometheus["http"]}']
  metrics_path: ''
- job_name: 'kubernetes-pods'
  {token}
  kubernetes_sd_configs:
    role: pod
  relabel_configs:
  - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_scrape]
    action: keep
    regex: true
  - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_path]
    action: replace
    target_label: __metrics_path__
    regex: (.+)
  - source_labels: [__address__,__meta_kubernetes_pod_annotation_prometheus_io_port]
    action: replace
    regex: ([^:]+)(?::\d+)?;(\d+)
    replacement: $1:$2
    target_label: __address__
  - action: labelmap
    regex: __meta_kubernetes_pod_label_(.+)
  - source_labels: [__meta_kubernetes_namespace]
    action: replace
    target_label: kubernetes_namespace
  - source_labels: [__meta_kubernetes_pod_label_component]
    action: replace
    target_label: job
  - source_labels: [__meta_kubernetes_pod_name]
    action: replace
    target_label: kubernetes_pod_name
- job_name: 'kubernetes-nodes'
  scheme: https
  kubernetes_sd_configs:
    role: node
  tls_config:
    ca_file: /var/run/secrets/kubernetes.io/serviceaccount/ca.crt
  bearer_token_file: /var/run/secrets/kubernetes.io/serviceaccount/token
  relabel_configs:
  - action: labelmap
    regex: __meta_kubernetes_node_label_(.+)
  - target_label: __address__
    replacement: kubernetes.default.svc:443
  - source_labels: [__meta_kubernetes_node_name]
    regex: (.+)
    target_label: __metrics_path__
    replacement: /api/v1/nodes/${{1}}/proxy/metrics
- job_name: 'kubernetes-cadvisor'
  scheme: https
  kubernetes_sd_configs:
    role: node
  tls_config:
    ca_file: /var/run/secrets/kubernetes.io/serviceaccount/ca.crt
  bearer_token_file: /var/run/secrets/kubernetes.io/serviceaccount/token
  relabel_configs:
  - action: labelmap
    regex: __meta_kubernetes_node_label_(.+)
  - target_label: __address__
    replacement: kubernetes.default.svc:443
  - source_labels: [__meta_kubernetes_node_name]
    regex: (.+)
    target_label: __metrics_path__
    replacement: /api/v1/nodes/${{1}}/proxy/metrics/cadvisor"; 
            return new Dictionary<string, string> { { "prometheus.yml", prometheus } };
        }
        public static IDictionary<string,string> Prometheus()
        {
            var token = Values.Authentication.Enabled && Values.Authentication.Provider.Equals("jwt") ? "bearer_token_file: /pulsar/tokens/client/token" : "";
            var prometheus = $@"
    global:
      scrape_interval: 15s
    scrape_configs:
    - job_name: 'prometheus'
      static_configs:
      - targets: ['localhost:9090']
    - job_name: 'kubernetes-pods'
      kubernetes_sd_configs:
      - role: pod
      relabel_configs:
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_scrape]
        action: keep
        regex: true
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_path]
        action: replace
        target_label: __metrics_path__
        regex: (.+)
      - source_labels: [__address__, __meta_kubernetes_pod_annotation_prometheus_io_port]
        action: replace
        regex: ([^:]+)(?::\d+)?;(\d+)
        replacement: $1:$2
        target_label: __address__
      - action: labelmap
        regex: __meta_kubernetes_pod_label_(.+)
      - source_labels: [__meta_kubernetes_namespace]
        action: replace
        target_label: kubernetes_namespace
      - source_labels: [__meta_kubernetes_pod_label_component]
        action: replace
        target_label: job
      - source_labels: [__meta_kubernetes_pod_name]
        action: replace
        target_label: kubernetes_pod_name"; 
            return new Dictionary<string, string> { { "prometheus.yml", prometheus } };
        }

        public static IDictionary<string, string> ToolSet()
        {
            var conf = new Dictionary<string, string>
                        {
                            {"keytool.sh", @"
    #!/bin/bash
    component=$1
    name=$2
    isClient=$3
    crtFile=/pulsar/certs/${component}/tls.crt
    keyFile=/pulsar/certs/${component}/tls.key
    caFile=/pulsar/certs/ca/ca.crt
    p12File=/pulsar/${component}.p12
    keyStoreFile=/pulsar/${component}.keystore.jks
    trustStoreFile=/pulsar/${component}.truststore.jks
    
    function checkFile() {
         local file=$1
        local len=$(wc -c ${file} | awk '{print $1}')
        echo ""processing ${file} : len = ${ len}
            ""
        if [!-f ${ file} ]; then
           echo ""${file} is not found""
            return -1
        fi
        if [ $len - le 0]; then
            echo ""${file} is empty""
            return -1
        fi
    }

    function ensureFileNotEmpty()
        {
            local file =$1
            until checkFile ${ file}; do
                echo ""file isn't initialized yet ... check in 3 seconds ..."" && sleep 3;
            done;
        }

    ensureFileNotEmpty ${crtFile}
    ensureFileNotEmpty ${keyFile}
    ensureFileNotEmpty ${caFile}
    
    export PASSWORD=$(head /dev/urandom | base64 | head -c 24)
    
    openssl pkcs12 \
        -export \
        -in ${crtFile} \
        -inkey ${keyFile} \
        -out ${p12File} \
        -name ${name} \
        -passout ""pass:${ PASSWORD}
            ""


    keytool - importkeystore \
        -srckeystore ${ p12File} \
        -srcstoretype PKCS12 - srcstorepass ""${PASSWORD}"" \
        -alias ${ name} \
        -destkeystore ${ keyStoreFile} \
        -deststorepass ""${PASSWORD}""


    keytool - import \
        -file ${ caFile} \
        -storetype JKS \
        -alias ${ name} \
        -keystore ${ trustStoreFile} \
        -storepass ""${PASSWORD}"" \
        -trustcacerts - noprompt


    ensureFileNotEmpty ${ keyStoreFile}
            ensureFileNotEmpty ${ trustStoreFile}

            if [[""x${isClient}"" == ""xtrue""]]; then
              echo ""update tls client settings ...""
"}
                        };
            if (Values.Tls.ZooKeeper.Enabled)
            {
                conf["keytool.sh"] += @"
        echo $'\n' >> conf/pulsar_env.sh
        echo ""PULSAR_EXTRA_OPTS =\""${PULSAR_EXTRA_OPTS} -Dzookeeper.clientCnxnSocket=org.apache.zookeeper.ClientCnxnSocketNetty -Dzookeeper.client.secure=true -Dzookeeper.ssl.keyStore.location=${keyStoreFile} -Dzookeeper.ssl.keyStore.password=${PASSWORD} -Dzookeeper.ssl.trustStore.location=${trustStoreFile} -Dzookeeper.ssl.trustStore.password=${PASSWORD}\"""" >> conf / pulsar_env.sh
        echo $'\n' >> conf / bkenv.sh
        echo ""BOOKIE_EXTRA_OPTS=\""${BOOKIE_EXTRA_OPTS} -Dzookeeper.clientCnxnSocket=org.apache.zookeeper.ClientCnxnSocketNetty -Dzookeeper.client.secure=true -Dzookeeper.ssl.keyStore.location=${keyStoreFile} -Dzookeeper.ssl.keyStore.password=${PASSWORD} -Dzookeeper.ssl.trustStore.location=${trustStoreFile} -Dzookeeper.ssl.trustStore.password=${PASSWORD}\"""" >> conf / bkenv.sh
        echo $'\n' >> conf / bkenv.sh
";
            }
            if (Values.Tls.Broker.Enabled && Values.Settings.Kop.Enabled)
            {
                conf["keytool.sh"] += @"
        echo $'\n' >> conf/broker.conf
        echo ""kopSslKeystorePassword =${ PASSWORD}
                "" >> conf/broker.conf
        echo $'\n' >> conf / broker.conf
        echo ""kopSslKeyPassword=${PASSWORD}"" >> conf / broker.conf
        echo $'\n' >> conf / broker.conf
        echo ""kopSslTruststorePassword=${PASSWORD}"" >> conf / broker.conf
";
            }
            else
            {
                conf["keytool.sh"] += @"echo ""update tls client settings ...""";
            }
            if (Values.Tls.ZooKeeper.Enabled)
            {
                conf["keytool.sh"] += @"
        echo $'\n' >> conf/pulsar_env.sh
        echo ""PULSAR_EXTRA_OPTS =\""${PULSAR_EXTRA_OPTS} -Dzookeeper.ssl.keyStore.location=${keyStoreFile} -Dzookeeper.ssl.keyStore.password=${PASSWORD} -Dzookeeper.ssl.trustStore.location=${trustStoreFile} -Dzookeeper.ssl.trustStore.password=${PASSWORD}\"""" >> conf / pulsar_env.sh
";
            }
            conf["keytool.sh"] += @"
        fi
        echo ${PASSWORD} > conf/password
";
            return conf;
        }

    }
}
