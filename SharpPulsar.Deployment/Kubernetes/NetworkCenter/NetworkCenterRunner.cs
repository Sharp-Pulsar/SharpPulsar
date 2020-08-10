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
        private Dictionary<string, object> _results;
        public NetworkCenterRunner(IKubernetes k8s, ConfigMap configMap, Service service, ServiceAccount serviceAccount, Role role, RoleBinding roleBinding, ClusterRole clusterRole, ClusterRoleBinding clusterRoleBinding, Secret secret)
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

            _secret = new CenterSecret(secret);
            _tcpIngressService = new TcpIngressService(service);

        }

        public bool Run(out Dictionary<string, object> results, string dryRun = default)
        {
            if (Values.Ingress.Enabled)
            {
                _results = new Dictionary<string, object>();

                var nginx = _nginxConfiguration.Run(dryRun);
                _results.Add("NginxConfiguration", nginx);

                var udpConf = _udpServices.Run(dryRun);
                _results.Add("UdpServicesConfiguration", udpConf);

                var cenService = _centerService.Run(dryRun);
                _results.Add("CenterService", cenService);

                var cenSecret = _secret.Run(dryRun);
                _results.Add("CenterSecret", cenSecret);

                if (Values.Ingress.DeployNginxController)
                {
                    var dep = _deployment.Run(dryRun);
                    _results.Add("Deployment", dep);
                }
                if(Values.Ingress.Rbac)
                {
                    var cacct = _centerServiceAccount.Run(dryRun);
                    _results.Add("ServiceAccount", cacct);

                    var cr = _centerClusterRole.Run(dryRun);
                    _results.Add("ClusterRole", cr);

                    var crb = _centerClusterRoleBinding.Run(dryRun);
                    _results.Add("ClusterRoleBinding", crb);

                    var r = _centerRole.Run(dryRun);
                    _results.Add("Role", r);

                    var rb = _centerRoleBinding.Run(dryRun);
                    _results.Add("RoleBinding", rb);
                }
                if (Values.Ingress.Proxy.Enabled)
                {
                    var httpPort = Values.Ports.Proxy["http"].ToString();
                    var ports = new Dictionary<string, string>
                    {
                        [httpPort] = $"{Values.Namespace}/{Values.Proxy.ComponentName}:{httpPort}"
                    };
                    if(Values.Tls.Enabled && Values.Tls.Proxy.Enabled)
                    {
                        var httpsPort = Values.Ports.Proxy["https"].ToString();
                        ports[httpsPort] = $"{Values.Namespace}/{Values.Proxy.ComponentName}:{httpsPort}";

                        var pulsarsslPort = Values.Ports.Proxy["pulsarssl"].ToString();
                        ports[pulsarsslPort] = $"{Values.Namespace}/{Values.Proxy.ComponentName}:{pulsarsslPort}";
                    }
                    else
                    {
                        var pulsarPort = Values.Ports.Proxy["pulsar"].ToString();
                        ports[pulsarPort] = $"{Values.Namespace}/{Values.Proxy.ComponentName}:{pulsarPort}";
                    }
                    var tcpConf = _tcpServices.Run(ports, dryRun);
                    _results.Add("TcpServicesConfiguration", tcpConf);

                    _tcpIngressService.ConfigurePorts(Values.Tls.Proxy, Values.Ports.Proxy);
                    var tcp = _tcpIngressService.Run(dryRun);
                    _results.Add("TcpIngressService", tcp);
                }
                else if (Values.Ingress.Broker.Enabled)
                {
                    var httpPort = Values.Ports.Broker["http"].ToString();
                    var ports = new Dictionary<string, string>
                    {
                        [httpPort] = $"{Values.Namespace}/{Values.Broker.ComponentName}:{httpPort}"
                    };
                    if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
                    {
                        var httpsPort = Values.Ports.Broker["https"].ToString();
                        ports[httpsPort] = $"{Values.Namespace}/{Values.Broker.ComponentName}:{httpsPort}";

                        var pulsarsslPort = Values.Ports.Broker["pulsarssl"].ToString();
                        ports[pulsarsslPort] = $"{Values.Namespace}/{Values.Broker.ComponentName}:{pulsarsslPort}";
                    }
                    else
                    {
                        var pulsarPort = Values.Ports.Broker["pulsar"].ToString();
                        ports[pulsarPort] = $"{Values.Namespace}/{Values.Broker.ComponentName}:{pulsarPort}";
                    }
                    var tcpConf = _tcpServices.Run(ports, dryRun);
                    _results.Add("TcpServicesConfiguration", tcpConf);

                    _tcpIngressService.ConfigurePorts(Values.Tls.Broker, Values.Ports.Broker);
                    var tcp = _tcpIngressService.Run(dryRun);
                    _results.Add("TcpIngressService", tcp);
                }

                var tls = new HashSet<string>();
                foreach (var r in Values.Ingress.HttpRules)
                {
                    if (r.Tls)
                        tls.Add(r.Host);
                    _centerIngress.Rule(r.Host, r.Path, r.ServiceName, r.Port);
                }
                _centerIngress.AddTls(tls.ToArray());
                var cing = _centerIngress.Run(dryRun);
                _results.Add("Ingress", cing);
            }
            results = _results;
            return true;
        }
    }
}
