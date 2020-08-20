using k8s;
using k8s.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup
{
    //https://kubernetes.github.io/ingress-nginx/user-guide/exposing-tcp-udp-services/
    internal class Ingress
    {
        private readonly IKubernetes _client;
        private readonly Networkingv1beta1Ingress _ingress;
        /// <summary>
        /// Ingress for grafana, pulsar manager and prometheus
        /// </summary>
        /// <param name="client"></param>
        public Ingress(IKubernetes client)
        {
            _client = client;
            _ingress = new Networkingv1beta1Ingress
            {
                Metadata = new V1ObjectMeta
                {
                    Name = $"{Values.ReleaseName}-{Values.Namespace}-ingress",
                    Annotations = new Dictionary<string, string>
                    {
                        {"kubernetes.io/ingress.class","nginx" },
                        {"ingress.kubernetes.io/ssl-redirect", "true" },
                        {"cert-manager.io/cluster-issuer","letsencrypt" },
                        {"kubernetes.io/tls-acme", "true"}
                    }
                },
                Spec = new Networkingv1beta1IngressSpec
                {
                    Tls = new List<Networkingv1beta1IngressTLS>(),
                    Rules = new List<Networkingv1beta1IngressRule>()
                }
            };
        }
        public Ingress AddTls(params string[] hosts)
        {
            if(hosts.Count() > 0)
            {
                _ingress.Spec.Tls.Add(new Networkingv1beta1IngressTLS
                {
                    Hosts = new List<string>(hosts),
                    SecretName = "wildcard"
                });
            }
            return this;
        }
        public Ingress Rule(string host, string path, string service, int port)
        {
            var hosts =_ingress.Spec.Rules.Where(x => x.Host.Equals(host, StringComparison.OrdinalIgnoreCase));
            if (hosts.Count() == 0)
                return CreateHostRule(host, path, service, port);

            return AddRule(host, path, service, port); 
        }
        private Ingress CreateHostRule(string host, string path, string service, int port)
        {
            _ingress.Spec.Rules.Add(new Networkingv1beta1IngressRule
            {
                Host = host,
                Http = new Networkingv1beta1HTTPIngressRuleValue
                {
                    Paths = new List<Networkingv1beta1HTTPIngressPath>
                    {
                        new Networkingv1beta1HTTPIngressPath
                        {
                            Path = path,
                            Backend = new Networkingv1beta1IngressBackend
                            {
                                ServiceName = service,
                                ServicePort = port
                            }
                        }
                    }
                }
            });
            return this;
        }
        private Ingress AddRule(string host, string path, string service, int port)
        {
            _ingress.Spec.Rules.First(x => x.Host.Equals(host, StringComparison.OrdinalIgnoreCase))
                .Http.Paths.Add(new Networkingv1beta1HTTPIngressPath
                {
                    Path = path,
                    Backend = new Networkingv1beta1IngressBackend
                    {
                        ServiceName = service,
                        ServicePort = port
                    }
                });
            return this;
        }

        public RunResult Run(string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                result.Response = _client.CreateNamespacedIngress1(_ingress, Values.Namespace, dryRun);
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
