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
        public V1ConfigMap GenZkConf(string dryRun = default)
        {
            var zk = File.ReadAllText(@"\Kubernetes\Zoo\gen-zk-conf.txt");
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .Data(new Dictionary<string, string>
                        {
                            {"gen-zk-conf.sh",zk} 
                });
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
        public V1ConfigMap Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}", Values.Namespace)                
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.ZooKeeper.ComponentName },
                            })
                .Data(Values.ZooKeeper.ConfigData);
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
