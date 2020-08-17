using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Grafana
{
    internal class GrafanaAdminSecret
    {
        private readonly Secret _secret;
        public GrafanaAdminSecret(Secret secret)
        {

            _secret = secret;
        }
        public RunResult Run(string dryRun = default)
        {
            _secret.Builder()
                 .Metadata($"{Values.ReleaseName}-{Values.Settings.Grafana.Name}-secret", Values.Namespace)
                 .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Grafana.Name }
                            })
                 .Type("Opaque")
                 .KeyValue("GRAFANA_ADMIN_PASSWORD", Values.ExtraConfigs.Grafana.Holder["Password"].ToString())
                 .KeyValue("GRAFANA_ADMIN_USER", Values.ExtraConfigs.Grafana.Holder["Username"].ToString());
            return _secret.Run(_secret.Builder(), Values.Namespace, dryRun);
        }
    }
}
