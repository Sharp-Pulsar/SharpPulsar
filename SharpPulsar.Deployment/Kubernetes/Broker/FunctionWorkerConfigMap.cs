using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    public class FunctionWorkerConfigMap
    {
        private readonly ConfigMap _config;
        public FunctionWorkerConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public V1ConfigMap Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-functions-worker-config", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", "functions-worker" },
                            })
                .Data(new Dictionary<string, string> 
                {
                    {"pulsarDockerImageName", $"{Values.Images.Functions.Repository}:{Values.Images.Functions.Tag}" }
                });
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
