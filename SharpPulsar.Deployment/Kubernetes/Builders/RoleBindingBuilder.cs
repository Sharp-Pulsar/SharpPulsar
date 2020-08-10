using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    internal class RoleBindingBuilder
    {
        private readonly V1RoleBinding _binding;
        public RoleBindingBuilder()
        {
            _binding = new V1RoleBinding
            {
                Metadata = new V1ObjectMeta(),
                RoleRef = new V1RoleRef(),
                Subjects = new List<V1Subject>()
            };
        }
        public RoleBindingBuilder Name(string name)
        {
            _binding.Metadata.Name = name;
            return this;
        }
        public RoleBindingBuilder Labels(IDictionary<string, string> labels)
        {
            _binding.Metadata.Labels = labels;
            return this;
        }
        public RoleBindingBuilder RoleRef(string apiGroup, string kind, string name)
        {
            _binding.RoleRef.ApiGroup = apiGroup;
            _binding.RoleRef.Kind = kind;
            _binding.RoleRef.Name = name;
            return this;
        }
        public RoleBindingBuilder AddSubject(string @namespace, string kind, string name)
        {
            _binding.Subjects.Add(new V1Subject
            {
                Kind = kind,
                Name = name,
                NamespaceProperty = @namespace
            });
            return this;
        }
        public V1RoleBinding Build()
        {
            return _binding;
        }
    }
}
