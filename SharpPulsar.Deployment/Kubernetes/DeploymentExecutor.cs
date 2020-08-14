using k8s;
using k8s.Models;
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
            _proxyRunner = new ProxyRunner(configMap, pdb, service, serviceAccount, statefulset);
            _prestoRunner = new PrestoRunner(configMap, statefulset, service);
            _networkCenterRunner = new NetworkCenterRunner(_client, configMap, service, serviceAccount, role, roleBinding, clusterRole, clusterRoleBinding, secret);
            _certRunner = new CertRunner(_client, secret);
        }
        public IEnumerable<RunResult> Run(string dryRun = default)
        {
            if (Values.NamespaceCreate)
            {
                var result = new RunResult();
                try
                {
                    var nsp = new V1Namespace
                    {
                        ApiVersion = "v1",
                        Kind = "Namespace",
                        Metadata = new V1ObjectMeta
                        {
                            Name = Values.Namespace
                        }
                    };
                    result.Response = _client.CreateNamespace(nsp, dryRun);
                    result.Success = true;
                }
                catch (Microsoft.Rest.RestException ex)
                {
                    if (ex is Microsoft.Rest.HttpOperationException e)
                        result.HttpOperationException = e;
                    else
                        result.Exception = ex;
                    result.Success = false;
                }
                yield return result;
            }

            foreach(var cert in _certRunner.Run(dryRun))
                yield return cert;

            foreach(var net in _networkCenterRunner.Run(dryRun))
                yield return net;


            foreach(var zoo in _zooKeeperRunner.Run(dryRun))
                yield return zoo;

            foreach(var bookie in _bookieRunner.Run(dryRun))
                yield return bookie;

            foreach(var brok in _brokerRunner.Run(dryRun))
                yield return brok;

            foreach(var prox in _proxyRunner.Run(dryRun))
                yield return prox;

            foreach(var prest in _prestoRunner.Run(dryRun))
                yield return prest;
        }
        public RunResult Delete(string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                result.Response = _client.DeleteNamespace(Values.Namespace, dryRun:dryRun);
                result.Success = true;
            }
            catch (Microsoft.Rest.RestException ex)
            {
                if (ex is Microsoft.Rest.HttpOperationException e)
                    result.HttpOperationException = e;
                else
                    result.Exception = ex;
                result.Success = false;
            }
            return result;
        }
    }
}
