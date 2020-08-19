﻿using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup
{
    internal class IngressService
    {
        private readonly Service _service;
        public IngressService(Service service)
        {
            _service = service;
        }

        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata($"{Values.ReleaseName}-ingress-controller", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Proxy.Name }
                            })
                .Selector(new Dictionary<string, string>()
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Proxy.Name }
                            })
                .Type("LoadBalancer");
            if (!Values.Tls.Enabled)
            {
                _service.Builder()
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "http", Port = 80, Protocol = "TCP" }
                });
            }
            else
            {
                _service.Builder()
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "https", Port = 443, Protocol = "TCP" }
                });
            }
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}