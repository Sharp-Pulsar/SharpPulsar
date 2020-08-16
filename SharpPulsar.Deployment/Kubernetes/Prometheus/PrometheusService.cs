using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Prometheus
{
    internal class PrometheusService
    {
        private readonly Service _service;
        internal PrometheusService(Service service)
        {
            _service = service;
        }

        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata(Values.Settings.Prometheus.Service, Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Prometheus.Name }
                            })

                .Annotations(Values.Settings.Prometheus.Annotations.Service)
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{ Name = "server", Port = Values.Ports.Prometheus["http"]}
                })
                .ClusterIp("None")
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Prometheus.Name }
                            });
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
