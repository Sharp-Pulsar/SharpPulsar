using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie
{
    internal class BookieStatefulSet
    {
        private readonly StatefulSet _set;
        public BookieStatefulSet(StatefulSet set)
        {
            _set = set;
        }

        public RunResult Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.BookKeeper.Name }
                            })
                .SpecBuilder()
                .ServiceName(Values.Settings.BookKeeper.Service)
                .Replication(Values.Settings.BookKeeper.Replicas)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.BookKeeper.Name }
                            })
                .UpdateStrategy(Values.Settings.BookKeeper.UpdateStrategy)
                .PodManagementPolicy(Values.Settings.BookKeeper.PodManagementPolicy)
                .VolumeClaimTemplates(Values.BookKeeper.PVC)
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.BookKeeper.Name }
                            }, new Dictionary<string, string>
                            {
                                {"prometheus.io/scrape:", "true" },
                                {"prometheus.io/port", "8000" }
                            }
                 )
                .SpecBuilder()
                .Tolerations(Values.BookKeeper.Tolerations)
                .SecurityContext(Values.BookKeeper.SecurityContext)
                .ServiceAccountName($"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}-acct")
                .NodeSelector(Values.BookKeeper.NodeSelector)
                .PodAntiAffinity(Helpers.AntiAffinity.AffinityTerms(Values.Settings.BookKeeper))
                .TerminationGracePeriodSeconds(Values.Settings.BookKeeper.GracePeriodSeconds)
                .InitContainers(Values.BookKeeper.ExtraInitContainers)
                .Containers(Values.BookKeeper.Containers)
                .Volumes(Values.BookKeeper.Volumes);

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
