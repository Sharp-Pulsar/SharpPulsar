using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Grafana
{
    internal class GrafanaService
    {
        private readonly Service _service;
        public GrafanaService(Service service)
        {
            _service = service;
        }

        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.Grafana.Name}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Grafana.Name }
                            })
                .Annotations(Values.Settings.Grafana.Annotations.Service)
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "server", Port = Values.Ports.Grafana["http"], TargetPort = Values.Ports.Grafana["targetPort"], Protocol = "TCP" }
                })
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Grafana.Name }
                            });
            if (!Values.Ingress.Enabled)
                _service.Builder()
                    .Type("LoadBalancer");
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }

    }
}
