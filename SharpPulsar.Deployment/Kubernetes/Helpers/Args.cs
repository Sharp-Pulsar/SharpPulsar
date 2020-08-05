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
    }
}
