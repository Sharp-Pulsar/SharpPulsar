using k8s;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Grafana
{
    internal class GrafanaRunner
    {
        private readonly GrafanaAdminSecret _grafanaAdminSecret;
        private readonly GrafanaDeployment _grafanaDeployment;
        private readonly GrafanaService _grafanaService;
        public GrafanaRunner(IKubernetes client, Secret secret, Service service)
        {
            _grafanaAdminSecret = new GrafanaAdminSecret(secret);
            _grafanaDeployment = new GrafanaDeployment(client);
            _grafanaService = new GrafanaService(service);
     
        }
        public IEnumerable<RunResult> Run(string dryRun = default)
        {
            RunResult result;
            if (Values.Monitoring.Grafana)
            {
                result = _grafanaAdminSecret.Run(dryRun);
                yield return result;

                result = _grafanaService.Run(dryRun);
                yield return result;

                result = _grafanaDeployment.Run(dryRun);
                yield return result;
            }
        }
    }
}
