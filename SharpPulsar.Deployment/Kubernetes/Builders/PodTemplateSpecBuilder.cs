using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    public class PodTemplateSpecBuilder
    {
        private readonly V1PodTemplateSpec _spec;
        private readonly PodSpecBuilder _podSpecBuilder;
        public PodTemplateSpecBuilder()
        {
            _spec = new V1PodTemplateSpec();
            _podSpecBuilder = new PodSpecBuilder();
        }
        public PodTemplateSpecBuilder Metadata(Dictionary<string,string> labels, Dictionary<string,string> annotations)
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
