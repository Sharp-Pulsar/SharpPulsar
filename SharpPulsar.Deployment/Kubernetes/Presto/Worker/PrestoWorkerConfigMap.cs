﻿using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Presto.Worker
{
    internal class PrestoWorkerConfigMap
    {
        private readonly ConfigMap _config;
        public PrestoWorkerConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public V1ConfigMap Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.PrestoWorker.Name}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.PrestoWorker.Name },
                            })
                .Data(Values.ConfigMaps.PrestoWorker);
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}