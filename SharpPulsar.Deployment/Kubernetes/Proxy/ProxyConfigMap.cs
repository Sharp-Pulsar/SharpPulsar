﻿using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Proxy
{
    public class ProxyConfigMap
    {

        private readonly ConfigMap _config;
        public ProxyConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public V1ConfigMap Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Proxy.ComponentName}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Proxy.ComponentName },
                            })
                .Data(Values.Proxy.ConfigData);
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}