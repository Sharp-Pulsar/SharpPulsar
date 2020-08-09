using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    internal class PodSpecBuilder
    {
        private V1PodSpec _spec;
        public PodSpecBuilder()
        {
            _spec = new V1PodSpec
            {
                Affinity = new V1Affinity
                {
                    PodAntiAffinity = new V1PodAntiAffinity(),
                    PodAffinity = new V1PodAffinity(),
                    NodeAffinity = new V1NodeAffinity()
                }
            };
        }
        public PodSpecBuilder SecurityContext(V1PodSecurityContext context)
        {
            _spec.SecurityContext = context;
            return this;
        }
        public PodSpecBuilder ServiceAccountName(string serviceAccountName)
        {
            _spec.ServiceAccountName = serviceAccountName;
            return this;
        }
        public PodSpecBuilder NodeSelector(IDictionary<string,string> selector)
        {
            _spec.NodeSelector = selector;
            return this;
        }
        public PodSpecBuilder Tolerations(IList<V1Toleration> tolerations)
        {
            _spec.Tolerations = tolerations;
            return this;
        }
        public PodSpecBuilder PodAntiAffinity(IList<V1PodAffinityTerm> terms)
        {
            _spec.Affinity.PodAntiAffinity.RequiredDuringSchedulingIgnoredDuringExecution = terms;
            return this;
        }
        public PodSpecBuilder PodAntiAffinity(IList<V1WeightedPodAffinityTerm> weight)
        {
            _spec.Affinity.PodAntiAffinity.PreferredDuringSchedulingIgnoredDuringExecution = weight;
            return this;
        }
        public PodSpecBuilder PodAffinity(IList<V1PodAffinityTerm> terms)
        {
            _spec.Affinity.PodAffinity.RequiredDuringSchedulingIgnoredDuringExecution = terms;
            return this;
        }
        public PodSpecBuilder PodAffinity(IList<V1WeightedPodAffinityTerm> weight)
        {
            _spec.Affinity.PodAffinity.PreferredDuringSchedulingIgnoredDuringExecution = weight;
            return this;
        }
        public PodSpecBuilder NodeAffinity(IList<V1PreferredSchedulingTerm> weight)
        {
            _spec.Affinity.NodeAffinity.PreferredDuringSchedulingIgnoredDuringExecution = weight;
            return this;
        }
        public PodSpecBuilder NodeAffinity(V1NodeSelector selector)
        {
            _spec.Affinity.NodeAffinity.RequiredDuringSchedulingIgnoredDuringExecution = selector;
            return this;
        }
        public PodSpecBuilder TerminationGracePeriodSeconds(long period)
        {
            _spec.TerminationGracePeriodSeconds = period;
            return this;
        }
        public PodSpecBuilder InitContainers(IList<V1Container> containers)
        {
            _spec.InitContainers = containers;
            return this;
        }
        public PodSpecBuilder Containers(IList<V1Container> containers)
        {
            _spec.Containers = containers;
            return this;
        }
        public PodSpecBuilder Volumes(IList<V1Volume> volumes)
        {
            _spec.Volumes = volumes;
            return this;
        }
        public PodSpecBuilder RestartPolicy(string policy)
        {
            _spec.RestartPolicy = policy;
            return this;
        }
        public V1PodSpec Build()
        {
            return _spec;
        }
    }
}
