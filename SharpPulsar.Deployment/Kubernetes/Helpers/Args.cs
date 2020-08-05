using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class Args
    {
        public static IList<string> AutoRecoveryIntContainer()
        {
            var args = new List<string>
            {
                "bin/apply-config-from-env.py conf/bookkeeper.conf;"
            };
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
                args.Add($"/pulsar/keytool/keytool.sh autorecovery {Values.AutoRecovery.HostName} true;");

            args.Add("until bin/bookkeeper shell whatisinstanceid; do"
                            + "  sleep 3;"
                            + "done; ");
            return args;
        }
        public static IList<string> AutoRecoveryContainer()
        {
            var args = new List<string>
            {
                "bin/apply-config-from-env.py conf/bookkeeper.conf;"
            };
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
                args.Add($"/pulsar/keytool/keytool.sh autorecovery {Values.AutoRecovery.HostName} true;");

            args.Add("bin/bookkeeper autorecovery");
            return args;
        }
        public static IList<string> ZooKeeper()
        {
            var args = new List<string>
            {
                "bin/apply-config-from-env.py conf/zookeeper.conf"
            };
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
                args.Add($"/pulsar/keytool/keytool.sh zookeeper {Values.ZooKeeper.HostName} false;");

            if(Values.ZooKeeper.ExtraConfig.Holder.TryGetValue("ZkServer", out var zk))
            {
                var zks = (List<string>)zk;
                foreach(var z in zks)
                {
                    args.Add($@"echo ""{z}"" >> conf/zookeeper.conf;");
                }
            }
            args.Add($"bin/gen-zk-conf.sh conf/zookeeper.conf {Values.ZooKeeper.ExtraConfig.Holder["InitialMyId"]} {Values.ZooKeeper.ExtraConfig.Holder["PeerType"]};");
            args.Add("cat conf/zookeeper.conf;");
            args.Add("bin/pulsar zookeeper;");
            return args;
        }
    }
}
