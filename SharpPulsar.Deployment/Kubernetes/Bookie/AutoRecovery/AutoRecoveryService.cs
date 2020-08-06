using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Zoo;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.AutoRecovery
{
    internal class AutoRecoveryService
    {
        private readonly Service _service;
        public AutoRecoveryService(Service service)
        {
            _service = service;
        }

        public V1Service Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata(Values.AutoRecovery.ServiceName, Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.AutoRecovery.ComponentName  }
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
                                {"component",Values.AutoRecovery.ComponentName  }
                            });
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
