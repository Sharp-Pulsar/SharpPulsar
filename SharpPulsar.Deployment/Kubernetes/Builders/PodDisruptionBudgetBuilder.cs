using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    internal class PodDisruptionBudgetBuilder
    {
        private readonly V1beta1PodDisruptionBudget _pdb;
        public PodDisruptionBudgetBuilder()
        {
            _pdb = new V1beta1PodDisruptionBudget
            {
                Metadata = new V1ObjectMeta(),
                Spec = new V1beta1PodDisruptionBudgetSpec()
                
            };
        }
        public PodDisruptionBudgetBuilder Metadata(string name, string @namespace)
        {
            _pdb.Metadata.Name = name;
            _pdb.Metadata.NamespaceProperty = @namespace;
            return this;
        }
        public PodDisruptionBudgetBuilder Labels(IDictionary<string, string> labels)
        {
            _pdb.Metadata.Labels = labels;
            return this;
        }
        public PodDisruptionBudgetBuilder MatchLabels(IDictionary<string, string> labels)
        {
            _pdb.Spec.Selector.MatchLabels = labels;
            return this;
        }
        public PodDisruptionBudgetBuilder MaxUnavailable(IntstrIntOrString intstrIntOrString)
        {
            _pdb.Spec.MaxUnavailable = intstrIntOrString ;
            return this;
        }
        public V1beta1PodDisruptionBudget Build()
        {
            return _pdb;
        }
    }
}
