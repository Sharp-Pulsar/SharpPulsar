using k8s.Models;
using System.Collections.Generic;
using System.IO;

namespace SharpPulsar.Deployment.Kubernetes.Bookie
{
    public class ZooKeeperConfigMap
    {
        private readonly ConfigMap _config;
        public ZooKeeperConfigMap(ConfigMap config)
        {
            _config = config;
        } 
        public V1ConfigMap GenZkConf()
        {
            var zk = File.ReadAllText(@"\Kubernetes\Zoo\gen-zk-conf.txt");
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-zookeeper", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.ReleaseName },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .Data(new Dictionary<string, string>
                        {
                            {"gen-zk-conf.sh",zk} 
                });
            return _config.Run(Values.Namespace);
        }
        public V1ConfigMap Run()
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-zookeeper", Values.Namespace)                
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.ReleaseName },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component","zookeeper" },
                            })
                .Data(new Dictionary<string, string>
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
                        });
            return _config.Run(Values.Namespace);
        }
    }
}
