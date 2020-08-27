using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    internal class PodTemplateSpecBuilder
    {
        private readonly V1PodTemplateSpec _spec;
        private readonly PodSpecBuilder _podSpecBuilder;
        public PodTemplateSpecBuilder()
        {
            _spec = new V1PodTemplateSpec {
                Metadata = new V1ObjectMeta()
            };
            _podSpecBuilder = new PodSpecBuilder();
        }
        public PodTemplateSpecBuilder Name(string name)
        {
            _spec.Metadata.Name = name;
            return this;
        }
        public PodTemplateSpecBuilder Labels(IDictionary<string,string> labels)
        {
            _spec.Metadata.Labels = labels;
            return this;
        }
        public PodTemplateSpecBuilder Annotations(IDictionary<string,string> annotations)
        {
            _spec.Metadata.Annotations = annotations;
            return this;
        }
        public PodTemplateSpecBuilder Metadata(IDictionary<string, string> labels, IDictionary<string, string> annotations)
        {
            _spec.Metadata = new V1ObjectMeta
            {
                Labels = labels,
                Annotations = annotations
            };
            return this;
        }
        public PodSpecBuilder SpecBuilder()
        {
            return _podSpecBuilder;
        }
        public V1PodTemplateSpec Build()
        {
            _spec.Spec = _podSpecBuilder.Build();
            return _spec;
        }
    }
}
