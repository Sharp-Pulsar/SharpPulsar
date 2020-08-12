using k8s;
using SharpPulsar.Deployment.Kubernetes.NetworkCenter.ConfigMaps;
using SharpPulsar.Deployment.Kubernetes.NetworkCenter.Rbac;
using System.Collections.Generic;
using System.Linq;

namespace SharpPulsar.Deployment.Kubernetes.NetworkCenter
{
    internal class NetworkCenterRunner
    {
        private readonly NginxConfiguration _nginxConfiguration;
        private readonly TcpServices _tcpServices;
        private readonly UdpServices _udpServices;

        private readonly CenterRole _centerRole;
        private readonly CenterRoleBinding _centerRoleBinding;
        private readonly CenterClusterRole _centerClusterRole;
        private readonly CenterClusterRoleBinding _centerClusterRoleBinding;
        private readonly CenterServiceAccount _centerServiceAccount;

        private readonly CenterIngress _centerIngress;
        private readonly CenterService _centerService;
        private readonly Deployment _deployment;

        private readonly CenterSecret _secret;
        private readonly TcpIngressService _tcpIngressService;
        public NetworkCenterRunner(IKubernetes k8s, ConfigMap configMap, Service service, ServiceAccount serviceAccount, Role role, RoleBinding roleBinding, ClusterRole clusterRole, ClusterRoleBinding clusterRoleBinding, Secret secret)
        {
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

            _secret = new CenterSecret(secret);
            _tcpIngressService = new TcpIngressService(service);

        }

        public IEnumerable<object> Run(string dryRun = default)
        {
            object result;
            if (Values.Ingress.Enabled)
            {
                result = _nginxConfiguration.Run(dryRun);
                yield return result;

                result = _udpServices.Run(dryRun);
                yield return result;

                result = _centerService.Run(dryRun);
                yield return result;

                result = _secret.Run(dryRun);
                yield return result;

                if (Values.Ingress.DeployNginxController)
                {
                    result = _deployment.Run(dryRun);
                    yield return result;
                }
                if(Values.Ingress.Rbac)
                {
                    result = _centerServiceAccount.Run(dryRun);
                    yield return result;

                    result = _centerClusterRole.Run(dryRun);
                    yield return result;

                    result = _centerClusterRoleBinding.Run(dryRun);
                    yield return result;

                    result = _centerRole.Run(dryRun);
                    yield return result;

                    result = _centerRoleBinding.Run(dryRun);
                    yield return result;
                }
                if (Values.Ingress.Proxy.Enabled)
                {
                    var httpPort = Values.Ports.Proxy["http"].ToString();
                    var ports = new Dictionary<string, string>
                    {
                        [httpPort] = $"{Values.Namespace}/{Values.Settings.Proxy.Name}:{httpPort}"
                    };
                    if(Values.Tls.Enabled && Values.Tls.Proxy.Enabled)
                    {
                        var httpsPort = Values.Ports.Proxy["https"].ToString();
                        ports[httpsPort] = $"{Values.Namespace}/{Values.Settings.Proxy.Name}:{httpsPort}";

                        var pulsarsslPort = Values.Ports.Proxy["pulsarssl"].ToString();
                        ports[pulsarsslPort] = $"{Values.Namespace}/{Values.Settings.Proxy.Name}:{pulsarsslPort}";
                    }
                    else
                    {
                        var pulsarPort = Values.Ports.Proxy["pulsar"].ToString();
                        ports[pulsarPort] = $"{Values.Namespace}/{Values.Settings.Proxy.Name}:{pulsarPort}";
                    }
                    result = _tcpServices.Run(ports, dryRun);
                    yield return result;

                    _tcpIngressService.ConfigurePorts(Values.Tls.Proxy, Values.Ports.Proxy);
                    result = _tcpIngressService.Run(dryRun);
                    yield return result;
                }
                else if (Values.Ingress.Broker.Enabled)
                {
                    var httpPort = Values.Ports.Broker["http"].ToString();
                    var ports = new Dictionary<string, string>
                    {
                        [httpPort] = $"{Values.Namespace}/{Values.Settings.Broker.Name}:{httpPort}"
                    };
                    if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
                    {
                        var httpsPort = Values.Ports.Broker["https"].ToString();
                        ports[httpsPort] = $"{Values.Namespace}/{Values.Settings.Broker.Name}:{httpsPort}";

                        var pulsarsslPort = Values.Ports.Broker["pulsarssl"].ToString();
                        ports[pulsarsslPort] = $"{Values.Namespace}/{Values.Settings.Broker.Name}:{pulsarsslPort}";
                    }
                    else
                    {
                        var pulsarPort = Values.Ports.Broker["pulsar"].ToString();
                        ports[pulsarPort] = $"{Values.Namespace}/{Values.Settings.Broker.Name}:{pulsarPort}";
                    }
                    result = _tcpServices.Run(ports, dryRun);
                    yield return result;

                    _tcpIngressService.ConfigurePorts(Values.Tls.Broker, Values.Ports.Broker);
                    result = _tcpIngressService.Run(dryRun);
                    yield return result;
                }

                var tls = new HashSet<string>();
                foreach (var r in Values.Ingress.HttpRules)
                {
                    if (r.Tls)
                        tls.Add(r.Host);
                    _centerIngress.Rule(r.Host, r.Path, r.ServiceName, r.Port);
                }
                _centerIngress.AddTls(tls.ToArray());
                result = _centerIngress.Run(dryRun);
                yield return result;
            }
        }
    }
}
