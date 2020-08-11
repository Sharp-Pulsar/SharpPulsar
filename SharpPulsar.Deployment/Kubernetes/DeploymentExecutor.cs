using k8s;
using SharpPulsar.Deployment.Kubernetes.Bookie;
using SharpPulsar.Deployment.Kubernetes.Broker;
using SharpPulsar.Deployment.Kubernetes.Certificate;
using SharpPulsar.Deployment.Kubernetes.NetworkCenter;
using SharpPulsar.Deployment.Kubernetes.Presto;
using SharpPulsar.Deployment.Kubernetes.Proxy;
using SharpPulsar.Deployment.Kubernetes.Zoo;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes
{
    public class DeploymentExecutor
    {
        private readonly IKubernetes _client;
        private ZooKeeperRunner _zooKeeperRunner;
        private BrokerRunner _brokerRunner;
        private BookieRunner _bookieRunner;
        private ProxyRunner _proxyRunner;
        private PrestoRunner _prestoRunner;
        private NetworkCenterRunner _networkCenterRunner;
        private readonly CertRunner _certRunner;

        public DeploymentExecutor(KubernetesClientConfiguration conf = default)
        {
            var config = conf ?? KubernetesClientConfiguration.BuildDefaultConfig();
            _client = new k8s.Kubernetes(config);
            var configMap = new ConfigMap(_client);
            var clusterRole = new ClusterRole(_client);
            var clusterRoleBinding = new ClusterRoleBinding(_client);
            var job = new Job(_client);
            var pdb = new PodDisruptionBudget(_client);
            var role = new Role(_client);
            var roleBinding = new RoleBinding(_client);
            var secret = new Secret(_client);
            var service = new Service(_client);
            var serviceAccount = new ServiceAccount(_client);
            var statefulset = new StatefulSet(_client);
            var storage = new StorageClass(_client);
            _zooKeeperRunner = new ZooKeeperRunner(job, configMap, pdb, service, statefulset, storage);
            _brokerRunner = new BrokerRunner(configMap, pdb, service, serviceAccount, statefulset, clusterRole, clusterRoleBinding);
            _bookieRunner = new BookieRunner(job, configMap, pdb, service, serviceAccount, statefulset, clusterRole, clusterRoleBinding, storage);
            _proxyRunner = new ProxyRunner(configMap, pdb, service, statefulset);
            _prestoRunner = new PrestoRunner(configMap, statefulset, service);
            _networkCenterRunner = new NetworkCenterRunner(_client, configMap, service, serviceAccount, role, roleBinding, clusterRole, clusterRoleBinding, secret);
            _certRunner = new CertRunner(_client, secret);
        }
        public IEnumerable<Dictionary<string, object>> Run(string dryRun = default)
        {
            var output = new Dictionary<string, object>();
            if (Values.NamespaceCreate)
            {
                var ns =_client.CreateNamespace(new k8s.Models.V1Namespace
                {
                    Metadata = new k8s.Models.V1ObjectMeta
                    {
                        Name = Values.Namespace
                    }
                }, dryRun);
                output["Namespace"] = ns;
                yield return output;
                output.Clear();
            }

            _certRunner.Run(out output, dryRun);
            yield return output;
            output.Clear();

            _networkCenterRunner.Run(out output, dryRun);
            yield return output;
            output.Clear();

            _zooKeeperRunner.Run(out output, dryRun);
            yield return output;
            output.Clear();

            _bookieRunner.Run(out output, dryRun);
            yield return output;
            output.Clear();

            _brokerRunner.Run(out output, dryRun);
            yield return output;
            output.Clear();

            _proxyRunner.Run(out output, dryRun);
            yield return output;
            output.Clear();

            _prestoRunner.Run(out output, dryRun);
            yield return output;
            output.Clear();
        }
    }
}
