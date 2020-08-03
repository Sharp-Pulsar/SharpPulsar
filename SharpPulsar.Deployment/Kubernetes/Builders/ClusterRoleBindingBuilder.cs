using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    public class ClusterRoleBindingBuilder
    {
        private V1ClusterRoleBinding _binding;
        public ClusterRoleBindingBuilder()
        {
            _binding = new V1ClusterRoleBinding
            {
                Metadata = new V1ObjectMeta(),
                RoleRef = new V1RoleRef(),
                Subjects = new List<V1Subject>()
            };
        }
        public ClusterRoleBindingBuilder Name(string name)
        {
            _binding.Metadata.Name = name;
            return this;
        }
        public ClusterRoleBindingBuilder Labels(IDictionary<string, string> labels)
        {
            _binding.Metadata.Labels = labels;
            return this;
        }
        public ClusterRoleBindingBuilder RoleRef(string apiGroup, string kind, string name)
        {
            _binding.RoleRef.ApiGroup = apiGroup;
            _binding.RoleRef.Kind = kind;
            _binding.RoleRef.Name = name;
            return this;
        }
        public ClusterRoleBindingBuilder AddSubject(string @namespace, string kind, string name)
        {
            _binding.Subjects.Add(new V1Subject
            {
                Kind = kind,
                Name = name,
                NamespaceProperty = @namespace
            });
            return this;
        }
        public V1ClusterRoleBinding Build()
        {
            return _binding;
        }
    }
}
