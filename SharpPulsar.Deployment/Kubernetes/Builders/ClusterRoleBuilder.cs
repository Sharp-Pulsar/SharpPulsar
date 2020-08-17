using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    internal class ClusterRoleBuilder
    {
        private readonly V1ClusterRole _cluster;
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
        public ClusterRoleBuilder AddRule(string[] apiGroups, string[] resources, string[] verbs, string[] nonResourceURLs = null)
        {
           if(nonResourceURLs == null)
            {
                _cluster.Rules.Add(new V1PolicyRule
                {
                    ApiGroups = apiGroups,
                    Resources = resources,
                    Verbs = verbs
                });
            }
            else
            {
                _cluster.Rules.Add(new V1PolicyRule
                {
                    ApiGroups = apiGroups,
                    Verbs = verbs,
                    NonResourceURLs = nonResourceURLs
                });
            }
            return this;
        }
        public V1ClusterRole Build()
        {
            return _cluster;
        }
    }
}
