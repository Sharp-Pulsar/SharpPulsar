using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
