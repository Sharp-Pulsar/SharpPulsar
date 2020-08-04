using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Zoo;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.AutoRecovery
{
    public class AutoRecoveryService
    {
        private readonly Service _service;
        public AutoRecoveryService(Service service)
        {
            _service = service;
        }

        public V1Service Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata($"{Values.ReleaseName}-recovery", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component","recovery" }
                            })
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "http", Port = 8000 }
                })
                .ClusterIp("None")
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component","recovery" }
                            });
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
