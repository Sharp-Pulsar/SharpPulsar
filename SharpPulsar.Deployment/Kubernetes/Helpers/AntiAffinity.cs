using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class AntiAffinity
    {
        public static List<V1PodAffinityTerm> AffinityTerms(Component component)
        {
            if(Values.AntiAffinity && component.AntiAffinity)
            {
                return new List<V1PodAffinityTerm>
                {
                    new V1PodAffinityTerm
                    {
                        LabelSelector = new V1LabelSelector
                        {
                            MatchExpressions = new List<V1LabelSelectorRequirement>
                            {
                                new V1LabelSelectorRequirement{ Key = "app", OperatorProperty = "In", Values = new List<string>{$"{Values.ReleaseName}-{component.ComponentName}" } },
                                new V1LabelSelectorRequirement{ Key = "release", OperatorProperty = "In", Values = new List<string>{$"{Values.ReleaseName}" } },
                                new V1LabelSelectorRequirement{ Key = "component", OperatorProperty = "In", Values = new List<string>{ component.ComponentName }}
                            }
                        },
                        TopologyKey = "kubernetes.io/hostname"
                    }
                };
            }
            return new List<V1PodAffinityTerm>();
        }
    }
}
