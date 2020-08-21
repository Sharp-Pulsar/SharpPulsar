using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Toolset
{
    internal class ToolsetStateful
    {
        private readonly StatefulSet _set;
        internal ToolsetStateful(StatefulSet set)
        {
            _set = set;
        }

        public RunResult Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.Settings.Toolset.Name}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Toolset.Name }
                            })
                .SpecBuilder()
                .ServiceName(Values.Settings.Toolset.Service)
                .Replication(Values.Settings.Toolset.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Toolset.Name }
                            })
                .UpdateStrategy(Values.Settings.Toolset.UpdateStrategy)
                .PodManagementPolicy(Values.Settings.Toolset.PodManagementPolicy)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Toolset.Name }
                            },
                            new Dictionary<string, string>
                            {
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.Toolset.Tolerations)
                .NodeSelector(Values.Toolset.NodeSelector)
                .PodAntiAffinity(Helpers.AntiAffinity.AffinityTerms(Values.Settings.Toolset))
                .TerminationGracePeriodSeconds(Values.Settings.Toolset.GracePeriodSeconds)
                .Containers(Values.Toolset.Containers)
                .Volumes(Values.Toolset.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
