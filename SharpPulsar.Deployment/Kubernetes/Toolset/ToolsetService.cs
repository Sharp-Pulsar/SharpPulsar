using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Toolset
{
    internal class ToolsetService
    {
        private readonly Service _service;
        internal ToolsetService(Service service)
        {
            _service = service;
        }

        public RunResult Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata(Values.Settings.Toolset.Service, Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .ClusterIp("None")
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Toolset.Name }
                            });
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}
