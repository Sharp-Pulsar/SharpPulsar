using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Prometheus
{
    internal class PrometheusStatefulset
    {
        private readonly StatefulSet _set;
        public PrometheusStatefulset(StatefulSet set)
        {
            _set = set;
        }

        public RunResult Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.Settings.Prometheus.Name}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Prometheus.Name }
                            })
                .SpecBuilder()
                .ServiceName(Values.Settings.Prometheus.Service)
                .Replication(Values.Settings.Prometheus.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Prometheus.Name }
                            })                
                .UpdateStrategy(Values.Settings.Prometheus.UpdateStrategy)
                .PodManagementPolicy(Values.Settings.Prometheus.PodManagementPolicy)
                .VolumeClaimTemplates(Values.Prometheus.PVC)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Prometheus.Name }
                            }, 
                            Values.Settings.Prometheus.Annotations.Template
                 )
                .SpecBuilder()
                .Tolerations(Values.Prometheus.Tolerations)
                .SecurityContext(Values.Prometheus.SecurityContext)
                .ServiceAccountName($"{Values.ReleaseName}-{Values.Rbac.RoleName}")
                .NodeSelector(Values.Prometheus.NodeSelector)
                .PodAntiAffinity(Helpers.AntiAffinity.AffinityTerms(Values.Settings.Prometheus))
                .TerminationGracePeriodSeconds(Values.Settings.Prometheus.GracePeriodSeconds)
                .InitContainers(Values.Prometheus.ExtraInitContainers)
                .Containers(Values.Prometheus.Containers)
                .Volumes(Values.Prometheus.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
