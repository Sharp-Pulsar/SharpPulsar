﻿using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.AutoRecovery
{
    public class AutoRecoveryConfigMap
    {
        private readonly ConfigMap _config;
        public AutoRecoveryConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public V1ConfigMap Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-recovery", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component","recovery" },
                            })
                .Data(new Dictionary<string, string>
                        {
                            {"BOOKIE_MEM", "-Xms64m -Xmx64m"}
                        });
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
