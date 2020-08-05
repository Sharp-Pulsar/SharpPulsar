using k8s;
using SharpPulsar.Deployment.Kubernetes.Bookie;

namespace SharpPulsar.Deployment.Kubernetes
{
    public class DeploymentExecutor
    {
        private readonly IKubernetes _client;
        private readonly ConfigMap _configMap;
        public DeploymentExecutor(KubernetesClientConfiguration conf = default)
        {
            var config = conf ?? KubernetesClientConfiguration.BuildDefaultConfig();
            _client = new k8s.Kubernetes(config);
            _configMap = new ConfigMap(_client);
        }
        public void ApplyZooKeeper()
        {
            var zkC = new ZooKeeperConfigMap(_configMap);
            var genZkConf = zkC.GenZkConf();
            var zkRun = zkC.Run();
        }
    }
}
