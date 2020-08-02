using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    public class StatefulSet
    {
        public V1StatefulSet CreateSet()
        {
            return new V1StatefulSet
            {
                ApiVersion = "apps/v1",
                Kind = "StatefulSet",
                Metadata = new V1ObjectMeta
                {
                    Name = "",
                    NamespaceProperty = "",
                    Labels = new Dictionary<string, string>
                    {
                        {"component", "zookeeper" }
                    }
                },
                Spec = new V1StatefulSetSpec
                {
                    ServiceName = "",
                    Replicas = 0,
                    Selector = new V1LabelSelector
                    {
                        MatchLabels = new Dictionary<string, string>
                        {
                             {"component", "zookeeper" }
                        }
                    },
                    UpdateStrategy = new V1StatefulSetUpdateStrategy
                    {
                        Type = "RollingUpdate"
                    },
                    PodManagementPolicy = "OrderedReady",
                    Template = new V1PodTemplateSpec
                    {
                        Metadata = new V1ObjectMeta
                        {
                            Labels = new Dictionary<string, string>
                            {
                                {"component", "zookeeper" }
                            },
                            Annotations = new Dictionary<string, string>
                            {
                                {"component", "zookeeper" }
                            },
                        },
                        Spec = new V1PodSpec
                        {
                            SecurityContext = new V1PodSecurityContext
                            {
                                
                            },
                            NodeSelector = new Dictionary<string, string>
                            {
                                
                            },
                            Tolerations = new List<V1Toleration>(),
                            Affinity = new V1Affinity
                            {
                                PodAntiAffinity = new V1PodAntiAffinity
                                {
                                    RequiredDuringSchedulingIgnoredDuringExecution = new List<V1PodAffinityTerm> { new V1PodAffinityTerm
                                    {
                                        
                                    } }
                                }
                            },
                            TerminationGracePeriodSeconds = 0L,
                            InitContainers = new List<V1Container>(),
                            Containers = new List<V1Container>
                            {
                                new V1Container
                                {
                                    Command = new string[]{ "sh", "-c" },
                                    Args = new string[]
                                    {
                                        "bin/apply-config-from-env.py conf/zookeeper.conf;"
                                    },
                                    Ports = new V1ContainerPort[]
                                    {

                                    },
                                    Env = new V1EnvVar[]
                                    {

                                    },
                                    EnvFrom = new V1EnvFromSource[]
                                    {

                                    },
                                    ReadinessProbe = new V1Probe
                                    {
                                        Exec = new V1ExecAction
                                        {
                                            Command = new string[]
                                            {
                                                "bin/pulsar-zookeeper-ruok.sh"
                                            }                                            
                                        },
                                        InitialDelaySeconds = 0,
                                        PeriodSeconds = 0,
                                        FailureThreshold = 0
                                    },
                                    LivenessProbe = new V1Probe
                                    {
                                        Exec = new V1ExecAction
                                        {
                                            Command = new string[]
                                            {
                                                "bin/pulsar-zookeeper-ruok.sh"
                                            }
                                        },
                                        InitialDelaySeconds = 0,
                                        PeriodSeconds = 0,
                                        FailureThreshold = 0
                                    },
                                    StartupProbe = new V1Probe
                                    {
                                        Exec = new V1ExecAction
                                        {
                                            Command = new string[]
                                            {
                                                "bin/pulsar-zookeeper-ruok.sh"
                                            }
                                        },
                                        InitialDelaySeconds = 0,
                                        PeriodSeconds = 0,
                                        FailureThreshold = 0
                                    },
                                    VolumeMounts = new V1VolumeMount[]
                                    {

                                    }
                                },
                                
                            },
                            Volumes = new V1Volume[]
                            {

                            }
                        },
                        
                    },
                    VolumeClaimTemplates = new V1PersistentVolumeClaim[]
                    {

                    }
                }
            };
        }
    }
}
