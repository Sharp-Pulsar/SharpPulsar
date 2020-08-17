using System.Collections.Generic;

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
                args.Add($"/pulsar/keytool/keytool.sh autorecovery {Values.Settings.Autorecovery.Host} true;");

            args.Add("until bin/bookkeeper shell whatisinstanceid; do sleep 3; done; ");
            return args;
        }
        public static IList<string> BrokerZooIntContainer()
        {
            var args = new List<string>();
            if (Values.Tls.Enabled && (Values.Tls.ZooKeeper.Enabled || (Values.Tls.Broker.Enabled /*&& Values.Tls.Kop.Enabled*/)))
                args.Add($"/pulsar/keytool/keytool.sh broker {Values.Settings.Broker.Host} true;");

            if (!string.IsNullOrWhiteSpace(Values.ConfigurationStore))
                args.Add($@"until bin/bookkeeper org.apache.zookeeper.ZooKeeperMain -server {Values.ConfigurationStore} get {Values.ConfigurationStoreMetadataPrefix}/admin/clusters/""{ Values.Namespace }""; do");
            else
                args.Add($@"until bin/bookkeeper org.apache.zookeeper.ZooKeeperMain -server {Values.Settings.ZooKeeper.ZooConnect} get {Values.MetadataPrefix}/admin/clusters/{Values.Namespace}; do");
            
            args.Add($@"echo ""pulsar cluster { Values.ReleaseName } isn't initialized yet ... check in 3 seconds ..."" && sleep 3; done; ");
            return args;
        }
        public static IList<string> BrokerBookieIntContainer()
        {
            var args = new List<string>();
            if (Values.Tls.Enabled && (Values.Tls.ZooKeeper.Enabled || (Values.Tls.Broker.Enabled /*&& Values.Tls.Kop.Enabled*/)))
                args.Add($"/pulsar/keytool/keytool.sh broker {Values.Settings.Broker.Host} true;");

            args.Add("bin/apply-config-from-env.py conf/bookkeeper.conf;");
            args.Add($@"until bin/bookkeeper shell whatisinstanceid; do echo ""bookkeeper cluster is not initialized yet. Backoff for 3 seconds...""; sleep 3; done; ");
            args.Add(@"echo ""bookkeeper cluster is already initialized"";");
            args.Add($@"bookieServiceNumber=""$(nslookup -timeout=10 { Values.ReleaseName }-{ Values.Settings.BookKeeper.Name } | grep Name | wc -l)"";");
            args.Add($@"until [ "+"${bookieServiceNumber}"+$@" -ge {Values.ConfigMaps.Broker["managedLedgerDefaultEnsembleSize"]} ]; do echo ""bookkeeper cluster { Values.ReleaseName }  isn't ready yet ... check in 10 seconds ...""; sleep 10; bookieServiceNumber=""$(nslookup -timeout=10 {Values.ReleaseName}-{Values.Settings.BookKeeper.Name} | grep Name | wc -l)""; done; ");

            args.Add(@"echo ""bookkeeper cluster is ready"";");
            return args;
        }
        public static IList<string> BrokerContainer()
        {
            var args = new List<string>
            {
                "bin/apply-config-from-env.py conf/broker.conf;",
                "bin/gen-yml-from-env.py conf/functions_worker.yml;",
                @"echo ""OK"" > status;"
            };

            if (Values.Tls.Enabled && (Values.Tls.ZooKeeper.Enabled || (Values.Tls.Broker.Enabled /*&& Values.Tls.Kop.Enabled*/)))
                args.Add($"/pulsar/keytool/keytool.sh broker {Values.Settings.Broker.Host} true;");
            
            args.Add($"bin/pulsar zookeeper-shell -server {Values.Settings.ZooKeeper.ZooConnect} get {Values.Settings.Broker.ZNode};");
            
            args.Add($@"while [ $? -eq 0 ]; do  echo ""broker { Values.Settings.Broker.Host } znode still exists... check in 10 seconds...""; sleep 10; bin/pulsar zookeeper - shell - server { Values.Settings.ZooKeeper.ZooConnect }  get { Values.Settings.Broker.ZNode };  done; ");

            args.Add(@"bin/pulsar broker;");
            return args;
        }
        public static IList<string> AutoRecoveryContainer()
        {
            var args = new List<string>
            {
                "bin/apply-config-from-env.py conf/bookkeeper.conf;"
            };
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
                args.Add($"/pulsar/keytool/keytool.sh autorecovery {Values.Settings.Autorecovery.Host} true;");

            args.Add("bin/bookkeeper autorecovery");
            return args;
        }
        public static IList<string> BookieExtraInitContainer()
        {
            var arg = @"if bin/bookkeeper shell whatisinstanceid; then echo ""bookkeeper cluster already initialized"";  else ";
            var args = new List<string> 
            {
                "bin/apply-config-from-env.py conf/bookkeeper.conf;"
            };
            if (!string.IsNullOrWhiteSpace(Values.MetadataPrefix))
            {
                arg += $@"bin/bookkeeper org.apache.zookeeper.ZooKeeperMain - server { Values.ReleaseName }-{ Values.Settings.ZooKeeper.Name } create { Values.MetadataPrefix } 'created for pulsar cluster ""{Values.ReleaseName}""' || yes && ";
            }
            arg += @"bin/bookkeeper shell initnewcluster; fi";
            args.Add(arg);
            return args;
        }
        public static IList<string> WaitZooKeeperContainer()
        {
            var prefix = string.IsNullOrWhiteSpace(Values.MetadataPrefix) ? "/": Values.MetadataPrefix;
            var args = new List<string>();
            if(Values.UserProvidedZookeepers.Count > 0)
                args.Add($@"until bin/pulsar zookeeper-shell -server {Values.UserProvidedZookeepers} ls {prefix}; do echo ""user provided zookeepers {Values.UserProvidedZookeepers} are unreachable... check in 3 seconds ..."" && sleep 3; done; ");
            else
            args.Add($@"until nslookup {Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}-{Values.Settings.ZooKeeper.Replicas - 1}.{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}.{Values.Namespace}; do sleep 3; done;");
            return args;
        }
        public static IList<string> WaitBrokerContainer()
        {
            var args = new List<string> 
            {
                "set -e; ",
                $@"brokerServiceNumber=""$(nslookup -timeout=10 {Values.ReleaseName}-{Values.Settings.Broker.Name} | grep Name | wc -l)"";",
                $@"until [ "+"${brokerServiceNumber} -ge 1 ]; do"+ $@" echo ""pulsar cluster {Values.ReleaseName} isn't initialized yet ... check in 10 seconds ..."";  sleep 10; brokerServiceNumber=""$(nslookup -timeout=10 {Values.ReleaseName}-{Values.Settings.Broker.Name} | grep Name | wc -l)"";  done;"  };
            return args;
        }
        public static IList<string> WaitPrestoCoordContainer()
        {
            var args = new List<string>
            {
                "set -e; ",
                $@"prestoServiceNumber=""$(nslookup -timeout=10 {Values.ReleaseName}-{Values.Settings.PrestoCoord.Name} | grep Name | wc -l)"";",
                $@"until [ "+"${prestoServiceNumber} -ge 1 ]; do"+ $@" echo ""presto coordinator {Values.ReleaseName} isn't initialized yet ... check in 10 seconds ..."";  sleep 10; prestoServiceNumber=""$(nslookup -timeout=10 {Values.ReleaseName}-{Values.Settings.PrestoCoord.Name} | grep Name | wc -l)"";  done;"  };
            return args;
        }
        public static IList<string> ZooKeeper()
        {
            var args = new List<string>
            {
                "bin/apply-config-from-env.py conf/zookeeper.conf;"
            };
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
                args.Add($"/pulsar/keytool/keytool.sh zookeeper {Values.Settings.ZooKeeper.Host} false;");

            if(Values.ExtraConfigs.ZooKeeper.Holder.TryGetValue("ZkServer", out var zk))
            {
                var zks = (List<string>)zk;
                foreach(var z in zks)
                {
                    args.Add($@"echo ""{z}"" >> conf/zookeeper.conf;");
                }
            }
            args.Add($"/pulsar/bin/gen-zk-conf.sh conf/zookeeper.conf {Values.ExtraConfigs.ZooKeeper.Holder["InitialMyId"]} {Values.ExtraConfigs.ZooKeeper.Holder["PeerType"]};");
            args.Add("cat conf/zookeeper.conf;");
            args.Add("bin/pulsar zookeeper;");
            return args;
        }

        public static IList<string> BookieIntContainer()
        {
            var args = new List<string>();
            if(!Values.Persistence || !Values.Settings.BookKeeper.Persistence)
            {
                args.Add("bin/apply-config-from-env.py conf/bookkeeper.conf;");
                if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
                    args.Add($"/pulsar/keytool/keytool.sh bookie {Values.Settings.BookKeeper.Host} true;");

                args.Add("until bin/bookkeeper shell whatisinstanceid; do sleep 3; done; ");
                args.Add("bin/bookkeeper shell bookieformat -nonInteractive -force -deleteCookie || true");
            }
            else if(Values.Persistence && Values.Settings.BookKeeper.Persistence)
            {
                args.Add("set -e;");
                args.Add("bin/apply-config-from-env.py conf/bookkeeper.conf;");
                if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
                    args.Add($"/pulsar/keytool/keytool.sh bookie {Values.Settings.BookKeeper.Host} true;");

                args.Add("until bin/bookkeeper shell whatisinstanceid; do sleep 3; done; ");
            }
            return args;
        }
        public static IList<string> BookieContainer()
        {
            var args = new List<string>
            {
                "bin/apply-config-from-env.py conf/bookkeeper.conf;"
            }; 
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
                args.Add($"/pulsar/keytool/keytool.sh bookie {Values.Settings.BookKeeper.Host} true;");

            args.Add("bin/pulsar bookie;");

            return args;
        }
        public static IList<string> PrestoWorker()
        {
            var args = new List<string>
            {
                "bin/pulsar sql-worker run --etc-dir=/pulsar/conf/presto --data-dir=/pulsar/data; "
            }; 
            return args;
        }
        
        public static IList<string> ProxyContainer()
        {
            var args = new List<string>
            {
                "pwd;",
                "ls .;",
                "ls conf;",
                "bin/apply-config-from-env.py conf/proxy.conf;",
                @"echo ""OK"" > status;",
                "bin/pulsar proxy;"
            }; 
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
                args.Add($"/pulsar/keytool/keytool.sh bookie {Values.Settings.BookKeeper.Host} true;");

            args.Add("bin/pulsar bookie;");

            return args;
        }

        public static IList<string> PrometheusContainer()
        {
            var args = new List<string>
            {
                "--config.file=/etc/config/prometheus.yml",
                "--storage.tsdb.path=/prometheus",
                "--web.console.libraries=/etc/prometheus/console_libraries",
                "--web.console.templates=/etc/prometheus/consoles",
                "--web.enable-lifecycle"
            };
            if(!string.IsNullOrWhiteSpace(Values.ExtraConfigs.Prometheus.Holder["PrometheusArgsRetention"].ToString()))
                args.Add($"--storage.tsdb.retention.time={Values.ExtraConfigs.Prometheus.Holder["PrometheusArgsRetention"]}");
            if (Values.Ingress.Enabled)
            {
                if (Values.Tls.Enabled)
                {
                    var url = $"https://prometheus.{Values.Ingress.DomainSuffix}";
                    Values.ExtraConfigs.Prometheus.Holder["Url"] = url;
                    args.Add($"--web.external-url={url}/");
                }
                else
                {
                    var url = $"http://prometheus.{Values.Ingress.DomainSuffix}";
                    Values.ExtraConfigs.Prometheus.Holder["Url"] = url;
                    args.Add($"--web.external-url={url}/");
                }
            }
            else
            {
                args.Add($"--web.external-url=/prometheus");
            }

            return args;
        }
        public static IList<string> PrometheusReloadContainer()
        {
            var args = new List<string>
            {
                "--volume-dir=/etc/config",
                $"--webhook-url=http://127.0.0.1:{Values.Ports.Prometheus["http"]}/-/reload"                
            };
            foreach (var kv in Values.ConfigmapReloads.Prometheus.ExtraArgs)
                args.Add($"--{kv.Key}={kv.Value}");

            foreach (var d in Values.ConfigmapReloads.Prometheus.ExtraVolumeDirs)
                args.Add($"--volume-dir={d}");

            return args;
        }
        public static IList<string> PrestoCoordContainer()
        {
            var args = new List<string>
            {
                @"bin/pulsar sql-worker run --etc-dir=/pulsar/conf/presto --data-dir=/pulsar/data; "
            }; 
            return args;
        }
        public static IList<string> MetadataBookieContainer()
        {
            var args = new List<string>
            {
                "bin/apply-config-from-env.py conf/bookkeeper.conf;"
            }; 
            if (Values.Tls.Enabled && (Values.Tls.ZooKeeper.Enabled || (Values.Tls.Bookie.Enabled /*&& Values.Tls.Kop.Enabled*/)))
                args.Add($"/pulsar/keytool/keytool.sh toolset {Values.Settings.Toolset.Host} true;");

            args.Add(@"until bin/bookkeeper shell whatisinstanceid; do sleep 3; done;");

            return args;
        }
        public static IList<string> PulsarMetadataContainer()
        {
            var arg = $@"bin/pulsar initialize-cluster-metadata --cluster {Values.Cluster} --zookeeper {Values.Settings.ZooKeeper.ZooConnect}{Values.MetadataPrefix} ";
            if (!string.IsNullOrWhiteSpace(Values.ConfigurationStore))
                arg += $@"--configuration-store {Values.ConfigurationStore}{Values.ConfigurationStoreMetadataPrefix} ";
            else
                arg += $@"--configuration-store {Values.Settings.ZooKeeper.ZooConnect}{Values.MetadataPrefix} ";
            arg += $@"--web-service-url http://{Values.ReleaseName}-{Values.Settings.Broker.Name}.{Values.Namespace}.svc.cluster.local:8080/ ";
            arg += $@"--web-service-url-tls https://{Values.ReleaseName}-{Values.Settings.Broker.Name}.{Values.Namespace}.svc.cluster.local:8443/ ";
            arg += $@"--broker-service-url pulsar://{Values.ReleaseName}-{Values.Settings.Broker.Name}.{Values.Namespace}.svc.cluster.local:6650/ ";
            arg += $@"--broker-service-url-tls pulsar+ssl://{Values.ReleaseName}-{Values.Settings.Broker.Name}.{Values.Namespace}.svc.cluster.local:6651/ ";

            var args = new List<string>(); 
            if (Values.Tls.Enabled && (Values.Tls.ZooKeeper.Enabled || (Values.Tls.Bookie.Enabled /*&& Values.Tls.Kop.Enabled*/)))
                args.Add($"/pulsar/keytool/keytool.sh toolset {Values.Settings.Toolset.Host} true;");

            args.Add(arg);

            return args;
        }
    }
}
