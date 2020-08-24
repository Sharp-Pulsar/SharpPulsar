using k8s;
using SharpPulsar.Deployment.Kubernetes.IngressSetup.ConfigMaps;
using System.Collections.Generic;
using System.Linq;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup
{
    //https://docs.nginx.com/nginx-ingress-controller/installation/installation-with-helm/
    //https://pixelrobots.co.uk/2019/09/update-your-azure-aks-service-principal-credentials/
    //https://kubernetes.github.io/ingress-nginx/user-guide/cli-arguments/
    //https://dzone.com/articles/nginx-ingress-controller-configuration-in-aks
    //https://docs.microsoft.com/en-us/azure/aks/ingress-tls
    //https://thorsten-hans.com/custom-domains-in-azure-kubernetes-with-nginx-ingress-azure-cli
    //https://kubernetes.io/docs/concepts/services-networking/ingress/
    //https://docs.nginx.com/nginx-ingress-controller/configuration/ingress-resources/advanced-configuration-with-annotations/
    internal class IngressRunner
    {
        private readonly NginxConfiguration _nginxConfiguration;
        private readonly TcpServices _tcpServices;
        private readonly UdpServices _udpServices;
        
        private readonly Ingress _centerIngress;
        private readonly TcpIngressService _tcpIngressService;
        public IngressRunner(IKubernetes k8s, ConfigMap configMap, Service service, ServiceAccount serviceAccount, Role role, RoleBinding roleBinding, ClusterRole clusterRole, ClusterRoleBinding clusterRoleBinding, Secret secret)
        {
            _centerIngress = new Ingress(k8s);

            _nginxConfiguration = new NginxConfiguration(configMap);
            _tcpServices = new TcpServices(configMap);
            _udpServices = new UdpServices(configMap);
            _tcpIngressService = new TcpIngressService(service);

        }
        /// <summary>
        /// https://medium.com/faun/wildcard-k8s-4998173b16c8
        /// https://docs.microsoft.com/en-us/azure/aks/ingress-tls
        /// First of all =>>>> kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v0.34.1/deploy/static/provider/cloud/deploy.yaml
        /// https://kubernetes.github.io/ingress-nginx/deploy/
        /// https://cert-manager.io/docs/configuration/acme/dns01/azuredns/
        /// https://github.com/fbeltrao/aks-letsencrypt/blob/6fd10fc92e5cc53588fe0c47be8cd0fcb17edaf7/setup-wildcard-certificates-with-azure-dns.md
        /// https://cert-manager.io/docs/tutorials/acme/ingress/
        /// https://cert-manager.io/docs/installation/kubernetes/
        /// </summary>
        /// <param name="dryRun"></param>
        /// <returns></returns>
        public IEnumerable<RunResult> Run(string dryRun = default)
        {
            RunResult result;
            if (Values.Ingress.Enabled)
            {
                result = _nginxConfiguration.Run(dryRun);
                yield return result;

                result = _udpServices.Run(dryRun);
                yield return result;

                if (Values.Ingress.Proxy.Enabled)
                {
                    var httpPort = Values.Ports.Proxy["http"].ToString();
                    var ports = new Dictionary<string, string>
                    {
                        [httpPort] = $"{Values.Namespace}/{Values.Settings.Proxy.Service}:{httpPort}"
                    };
                    if(Values.Tls.Enabled && Values.Tls.Proxy.Enabled)
                    {
                        var httpsPort = Values.Ports.Proxy["https"].ToString();
                        ports[httpsPort] = $"{Values.Namespace}/{Values.Settings.Proxy.Service}:{httpsPort}";

                        var pulsarsslPort = Values.Ports.Proxy["pulsarssl"].ToString();
                        ports[pulsarsslPort] = $"{Values.Namespace}/{Values.Settings.Proxy.Service}:{pulsarsslPort}";
                    }
                    else
                    {
                        var pulsarPort = Values.Ports.Proxy["pulsar"].ToString();
                        ports[pulsarPort] = $"{Values.Namespace}/{Values.Settings.Proxy.Service}:{pulsarPort}";
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
                        [httpPort] = $"{Values.Namespace}/{Values.Settings.Broker.Service}:{httpPort}"
                    };
                    if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
                    {
                        var httpsPort = Values.Ports.Broker["https"].ToString();
                        ports[httpsPort] = $"{Values.Namespace}/{Values.Settings.Broker.Service}:{httpsPort}";

                        var pulsarsslPort = Values.Ports.Broker["pulsarssl"].ToString();
                        ports[pulsarsslPort] = $"{Values.Namespace}/{Values.Settings.Broker.Service}:{pulsarsslPort}";
                    }
                    else
                    {
                        var pulsarPort = Values.Ports.Broker["pulsar"].ToString();
                        ports[pulsarPort] = $"{Values.Namespace}/{Values.Settings.Broker.Service}:{pulsarPort}";
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
