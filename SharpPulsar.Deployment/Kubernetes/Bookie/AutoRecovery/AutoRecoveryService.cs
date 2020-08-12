using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.AutoRecovery
{
    internal class AutoRecoveryService
    {
        private readonly Service _service;
        public AutoRecoveryService(Service service)
        {
            _service = service;
        }

        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata(Values.Settings.Autorecovery.Service, Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Autorecovery.Name  }
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
                                {"component",Values.Settings.Autorecovery.Name  }
                            });
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
