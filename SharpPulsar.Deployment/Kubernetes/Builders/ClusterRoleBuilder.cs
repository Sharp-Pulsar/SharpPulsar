using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    public class ClusterRoleBuilder
    {
        private V1ClusterRole _cluster;
        public ClusterRoleBuilder()
        {
            _cluster = new V1ClusterRole
            {
                Rules = new List<V1PolicyRule>(),
                Metadata = new V1ObjectMeta()
            };
        }
        public ClusterRoleBuilder Name(string name)
        {
            _cluster.Metadata.Name = name;
            return this;
        }
        public ClusterRoleBuilder Labels(IDictionary<string, string> labels)
        {
            _cluster.Metadata.Labels = labels;
            return this;
        }
        public ClusterRoleBuilder AddRule(string[] apiGroups, string[] resources, string[] verbs)
        {
            _cluster.Rules.Add(new V1PolicyRule 
            { 
                ApiGroups = apiGroups,
                Resources = resources,
                Verbs = verbs
            });
            return this;
        }
        public V1ClusterRole Build()
        {
            return _cluster;
        }
    }
}
