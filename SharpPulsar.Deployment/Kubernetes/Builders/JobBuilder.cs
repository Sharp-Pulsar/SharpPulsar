using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    internal class JobBuilder
    {
        private readonly V1Job _job;
        private readonly PodTemplateSpecBuilder _tempBuilder;
        public JobBuilder()
        {
            _job = new V1Job
            {
                Metadata = new V1ObjectMeta(),
                Spec = new V1JobSpec()
            };
            _tempBuilder = new PodTemplateSpecBuilder();
        }
        public JobBuilder Metadata(string name, string @namespace)
        {
            _job.Metadata.Name = name;
            _job.Metadata.NamespaceProperty = @namespace;
            return this;
        }
        public JobBuilder Labels(IDictionary<string, string> labels)
        {
            _job.Metadata.Labels = labels;
            return this;
        }
        public PodTemplateSpecBuilder TempBuilder()
        {
            return _tempBuilder;
        }
        public V1Job Build()
        {
            _job.Spec.Template = _tempBuilder.Build();
            return _job;
        }
    }
}
