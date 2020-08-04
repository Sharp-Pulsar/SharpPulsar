using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    public class StatefulSetBuilder
    {
        private readonly V1StatefulSet _statefulSet;
        private readonly StatefulSetSpecBuilder _statefulSetSpecBuilder;
        public StatefulSetBuilder()
        {
            _statefulSet = new V1StatefulSet
            {
                Metadata = new V1ObjectMeta()
            };
            _statefulSetSpecBuilder = new StatefulSetSpecBuilder();
        }
        public StatefulSetBuilder Name(string name)
        {
            _statefulSet.Metadata.Name = name;
            return this;
        }
        public StatefulSetBuilder Namespace(string @namespace)
        {
            _statefulSet.Metadata.NamespaceProperty = @namespace;
            return this;
        }
        public StatefulSetBuilder Labels(IDictionary<string, string> labels)
        {
            _statefulSet.Metadata.Labels = labels;
            return this;
        }
        public StatefulSetBuilder Annotations(IDictionary<string, string> annotations)
        {
            _statefulSet.Metadata.Annotations = annotations;
            return this;
        }
        public StatefulSetSpecBuilder SpecBuilder()
        {
            return _statefulSetSpecBuilder; ;

        }
        public V1StatefulSet Build()
        {
            _statefulSet.Spec = _statefulSetSpecBuilder.Build();
            return _statefulSet;
        }
        public class StatefulSetSpecBuilder
        {
            private  V1StatefulSetSpec _spec;
            private PodTemplateSpecBuilder _tempBulder;
            public StatefulSetSpecBuilder()
            {
                _tempBulder = new PodTemplateSpecBuilder();
                _spec = new V1StatefulSetSpec();
            }
            public StatefulSetSpecBuilder ServiceName(string serviceName)
            {
                _spec.ServiceName = serviceName;
                return this;
            }
            public StatefulSetSpecBuilder Replication(int replicas)
            {
                _spec.Replicas = replicas;
                return this;
            }
            public StatefulSetSpecBuilder Selector(Dictionary<string, string> matchLabels)
            {
                _spec.Selector = new V1LabelSelector
                {
                    MatchLabels = matchLabels
                };
                return this;
            }
            public StatefulSetSpecBuilder UpdateStrategy(string strategy)
            {
                _spec.UpdateStrategy = new V1StatefulSetUpdateStrategy
                {
                    Type = strategy
                };
                return this;
            }
            public StatefulSetSpecBuilder PodManagementPolicy(string policy)
            {
                _spec.PodManagementPolicy = policy;
                return this;
            }
            public StatefulSetSpecBuilder VolumeClaimTemplates(IList<V1PersistentVolumeClaim> pvc)
            {
                _spec.VolumeClaimTemplates = pvc;
                return this;
            }
            public PodTemplateSpecBuilder TemplateBuilder()
            {
                return _tempBulder;
            }
            public V1StatefulSetSpec Build()
            {
                _spec.Template = _tempBulder.Build();
                return _spec;
            }
        }
    }
}
