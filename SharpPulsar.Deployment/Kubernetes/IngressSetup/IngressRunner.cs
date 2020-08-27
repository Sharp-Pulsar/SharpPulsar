using k8s;
using SharpPulsar.Deployment.Kubernetes.IngressSetup.ConfigMaps;
using SharpPulsar.Deployment.Kubernetes.IngressSetup.Jobs;
using SharpPulsar.Deployment.Kubernetes.IngressSetup.Rbac;
using SharpPulsar.Deployment.Kubernetes.IngressSetup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
        private readonly IngressNamespace _ingressNamespace;

        private readonly ControllerConfigMap _controllerConfigMap;
        private readonly ValidatingWebhookConfiguration _validatingWebhookConfiguration;
        private readonly TcpServices _tcpServices;
        private readonly UdpServices _udpServices;

        private readonly AdmissionCreate _admissionCreate;
        private readonly AdmissionPatch _admissionPatch;
        
        private readonly Ingress _centerIngress;
        private readonly TcpIngressService _tcpIngressService;

        private readonly IngressRole _ingressRole;
        private readonly IngressRoleBinding _ingressRoleBinding;
        private readonly IngressClusterRole _ingressClusterRole;
        private readonly IngressClusterRoleBinding _ingressClusterRoleBinding;
        private readonly IngressServiceAccount _ingressServiceAccount;

        private readonly WebhookRole _webhookRole;
        private readonly WebhookRoleBinding _webhookRoleBinding;
        private readonly WebhookClusterRole _webhookClusterRole;
        private readonly WebhookClusterRoleBinding _webhookClusterRoleBinding;
        private readonly WebhookServiceAccount _webhookServiceAccount;

        private readonly ControllerService _ingressService;
        private readonly ControllerServiceWebhook _controllerServiceWebhook;
        private readonly Deployment _deployment;
        public IngressRunner(IKubernetes k8s, ConfigMap configMap, Service service, ServiceAccount serviceAccount, Role role, RoleBinding roleBinding, ClusterRole clusterRole, ClusterRoleBinding clusterRoleBinding, Secret secret, Job job)
        {
            _ingressNamespace = new IngressNamespace(k8s);

            _centerIngress = new Ingress(k8s);
            _controllerConfigMap = new ControllerConfigMap(configMap);
            _tcpServices = new TcpServices(configMap);
            _udpServices = new UdpServices(configMap);
            _tcpIngressService = new TcpIngressService(service);
            _validatingWebhookConfiguration = new ValidatingWebhookConfiguration(k8s);

            _admissionCreate = new AdmissionCreate(job);
            _admissionPatch = new AdmissionPatch(job);

            _ingressClusterRole = new IngressClusterRole(clusterRole);
            _ingressClusterRoleBinding = new IngressClusterRoleBinding(clusterRoleBinding);
            _ingressRole = new IngressRole(role);
            _ingressRoleBinding = new IngressRoleBinding(roleBinding);

            _webhookClusterRole = new WebhookClusterRole(clusterRole);
            _webhookClusterRoleBinding = new WebhookClusterRoleBinding(clusterRoleBinding);
            _webhookRole = new WebhookRole(role);
            _webhookRoleBinding = new WebhookRoleBinding(roleBinding);
            _webhookServiceAccount = new WebhookServiceAccount(serviceAccount);

            _ingressServiceAccount = new IngressServiceAccount(serviceAccount);
            _ingressService = new ControllerService(service);
            _controllerServiceWebhook = new ControllerServiceWebhook(service);

            _deployment = new Deployment(k8s);

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
                result =  _ingressNamespace.Run(dryRun);
                yield return result;
                
                result =  _validatingWebhookConfiguration.Run(dryRun);
                yield return result;
                
                result = _controllerConfigMap.Run(dryRun);
                yield return result;

                result = _ingressServiceAccount.Run(dryRun);
                yield return result;

                result = _udpServices.Run(dryRun);
                yield return result;

                if (Values.Ingress.Proxy.Enabled)
                {
                    var httpPort = Values.Ports.Proxy["http"].ToString();
                    var ports = new Dictionary<string, string>
                    {
                        [httpPort] = $"{Values.Namespace}/{Values.Settings.Proxy.Service}:{httpPort}",
                        ["8081"] = $"{Values.Namespace}/{Values.Settings.PrestoCoord.Service}:8081"
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
                        [httpPort] = $"{Values.Namespace}/{Values.Settings.Broker.Service}:{httpPort}",
                        ["8081"] = $"{Values.Namespace}/{Values.Settings.PrestoCoord.Service}:8081"
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

                result = _ingressClusterRole.Run(dryRun);
                yield return result;

                result = _ingressClusterRoleBinding.Run(dryRun);
                yield return result;

                result = _ingressRole.Run(dryRun);
                yield return result;

                result = _ingressRoleBinding.Run(dryRun);
                yield return result;

                result = _ingressService.Run(dryRun);
                yield return result;

                result = _deployment.Run(dryRun);
                yield return result;

                result = _webhookClusterRole.Run(dryRun);
                yield return result;

                result = _webhookClusterRoleBinding.Run(dryRun);
                yield return result;

                result = _webhookRole.Run(dryRun);
                yield return result;

                result = _webhookRoleBinding.Run(dryRun);
                yield return result;

                result = _webhookServiceAccount.Run(dryRun);
                yield return result;

                result = _controllerServiceWebhook.Run(dryRun);
                yield return result;

                result = _admissionCreate.Run(dryRun);
                yield return result;              

                result = _admissionPatch.Run(dryRun);
                yield return result;

                var tls = new HashSet<string>();
                foreach (var r in Values.Ingress.HttpRules)
                {
                    if (r.Tls)
                        tls.Add(r.Host);
                    _centerIngress.Rule(r.Host, r.Path, r.ServiceName, r.Port);
                }
                _centerIngress.AddTls(tls.ToArray());
                //So we wait: ingress-nginx-admission-patch needs to patch ingress-nginx-admission secret else ingress deployment fails
                Thread.Sleep(TimeSpan.FromSeconds(30));
                result = _centerIngress.Run(dryRun);
                yield return result;
            }
        }
    }
}
