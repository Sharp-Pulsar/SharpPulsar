using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie
{
    internal class BookieService
    {
        private readonly Service _service;
        public BookieService(Service service)
        {
            _service = service;
        }

        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata(Values.Settings.BookKeeper.Service, Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.BookKeeper.Name }
                            })
                .Annotations(new Dictionary<string, string>
                            {
                                {"publishNotReadyAddresses","true" }
                            })
                .Ports(new List<V1ServicePort>
                {
                    new V1ServicePort{Name = "bookie", Port = 3181 },
                    new V1ServicePort{Name = "http", Port = 8000 }
                })
                .ClusterIp("None")
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.BookKeeper.Name }
                            })
                .PublishNotReadyAddresses(true);
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
