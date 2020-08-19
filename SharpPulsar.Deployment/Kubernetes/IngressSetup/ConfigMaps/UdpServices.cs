﻿using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.ConfigMaps
{
    internal class UdpServices
    {
        private readonly ConfigMap _config;
        public UdpServices(ConfigMap config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-udp-services", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Proxy.Name },
                            });

            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}