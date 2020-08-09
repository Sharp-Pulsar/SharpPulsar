using k8s;
using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Certificate
{
    public class ClusterIngress
    {
        private IKubernetes _client;
        private Networkingv1beta1Ingress _ingress;

        public ClusterIngress(IKubernetes client)
        {
            _client = client;
            _ingress = new Networkingv1beta1Ingress
            {
                Metadata = new V1ObjectMeta
                {
                    Annotations = new Dictionary<string, string>
                    {
                        {"kubernetes.io/ingress.class","nginx" },
                        {"ingress.kubernetes.io/ssl-redirect", "true" },
                        {"cert-manager.io/cluster-issuer","letsencrypt" },
                        {"kubernetes.io/tls-acme", "true"}
                    }
                },
                Spec =  new Networkingv1beta1IngressSpec
                {
                    Tls = new List<Networkingv1beta1IngressTLS>(),
                    Rules = new List<Networkingv1beta1IngressRule>()
                }
            };
        }
        public ClusterIngress Name(string name)
        {
            _ingress.Metadata.Name = name;
            return this;
        }
        public ClusterIngress AddTls(IList<string> hosts, string secretName)
        {
            _ingress.Spec.Tls.Add(new Networkingv1beta1IngressTLS
            {
                Hosts = new List<string>(hosts),
                SecretName = secretName
            });
            return this;
        }
        public ClusterIngress AddRule(string host, string path, string service, int port)
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

    }
}
