using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Presto
{
    //https://medium.com/walmartglobaltech/presto-on-azure-c9bb8357a50a
    internal class PrestoService
    {
        private readonly Service _service;
        internal PrestoService(Service service)
        {
            _service = service;
        }

        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata(Values.Settings.PrestoCoord.Service, Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "http-coord", Port = Values.Ports.PrestoCoordinator["http"], TargetPort = Values.Ports.PrestoCoordinator["http"], Protocol = "TCP" }
                })                
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.PrestoCoord.Name }
                            });
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}