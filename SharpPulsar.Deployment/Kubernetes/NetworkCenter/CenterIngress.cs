﻿using k8s;
using k8s.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPulsar.Deployment.Kubernetes.NetworkCenter
{
    //https://kubernetes.github.io/ingress-nginx/user-guide/exposing-tcp-udp-services/
    internal class CenterIngress
    {
        private IKubernetes _client;
        private Networkingv1beta1Ingress _ingress;
        /// <summary>
        /// Ingress for grafana, pulsar manager and prometheus
        /// </summary>
        /// <param name="client"></param>
        public CenterIngress(IKubernetes client)
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
        public CenterIngress AddTls(params string[] hosts)
        {
            _ingress.Spec.Tls.Add(new Networkingv1beta1IngressTLS
            {
                Hosts = new List<string>(hosts),
                SecretName = $"{Values.ReleaseName}-ingress-secret"
            });
            return this;
        }
        public CenterIngress CreateHostRule(string host, string path, string service, int port)
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
        public CenterIngress AddRule(string host, string path, string service, int port)
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

        public Networkingv1beta1Ingress Run(string dryRun = default)
        {
            return _client.CreateNamespacedIngress1(_ingress, Values.Namespace, dryRun);
        }
    }
}
