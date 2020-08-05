using System.Collections.Generic;

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
    }
}
