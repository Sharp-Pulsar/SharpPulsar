using k8s;
using SharpPulsar.Deployment.Kubernetes.NetworkCenter.ConfigMaps;
using SharpPulsar.Deployment.Kubernetes.NetworkCenter.Rbac;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.NetworkCenter
{
    internal class NetworkCenterRunner
    {
        private NginxConfiguration _nginxConfiguration;
        private TcpServices _tcpServices;
        private UdpServices _udpServices;

        private CenterRole _centerRole;
        private CenterRoleBinding _centerRoleBinding;
        private CenterClusterRole _centerClusterRole;
        private CenterClusterRoleBinding _centerClusterRoleBinding;
        private CenterServiceAccount _centerServiceAccount;

        private CenterIngress _centerIngress;
        private CenterService _centerService;
        private Deployment _deployment;
        private Dictionary<string, object> _results;
        public NetworkCenterRunner(IKubernetes k8s, ConfigMap configMap, Service service, ServiceAccount serviceAccount, Role role, RoleBinding roleBinding, ClusterRole clusterRole, ClusterRoleBinding clusterRoleBinding)
        {
            _results = new Dictionary<string, object>();
            _deployment = new Deployment(k8s);
            _centerIngress = new CenterIngress(k8s);

            _nginxConfiguration = new NginxConfiguration(configMap);
            _tcpServices = new TcpServices(configMap);
            _udpServices = new UdpServices(configMap);

            _centerRole = new CenterRole(role);
            _centerRoleBinding = new CenterRoleBinding(roleBinding);

            _centerClusterRole = new CenterClusterRole(clusterRole);
            _centerClusterRoleBinding = new CenterClusterRoleBinding(clusterRoleBinding);

            _centerServiceAccount = new CenterServiceAccount(serviceAccount);
            _centerService = new CenterService(service);

        }

        public bool Run(out Dictionary<string, object> results, string dryRun = default)
        {
            if (Values.Ingress.Enabled)
            {

            }
            results = _results;
            return true;
        }
    }
}
