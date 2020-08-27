using k8s;
using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup
{
    internal class Deployment
    {
        private readonly IKubernetes _client;
        private readonly V1Deployment _deployment;
        public Deployment(IKubernetes client)
        {
            _client = client;
            _deployment = new V1Deployment
            {
                Metadata = new V1ObjectMeta
                {
                    Name = "ingress-nginx-controller",
                    NamespaceProperty = Values.Namespace,
                    Labels = new Dictionary<string, string>
                    {
                        {"helm.sh/chart", "ingress-nginx-2.11.1"},
                        {"app.kubernetes.io/name", "ingress-nginx"},
                        {"app.kubernetes.io/instance", "ingress-nginx"},
                        {"app.kubernetes.io/version", "0.34.1"},
                        {"app.kubernetes.io/managed-by", "Helm"},
                        {"app.kubernetes.io/component", "controller"}
                    }
                },
                Spec = new V1DeploymentSpec
                {
                    Replicas = Values.Ingress.Replicas,
                    Selector = new V1LabelSelector
                    {
                        MatchLabels = new Dictionary<string, string>
                            {
                                {"app.kubernetes.io/name", "ingress-nginx"},
                                {"app.kubernetes.io/instance", "ingress-nginx"},
                                {"app.kubernetes.io/component", "controller"}
                            }
                    },
                    RevisionHistoryLimit = 10,
                    MinReadySeconds = 0,
                    Template = new V1PodTemplateSpec
                    {
                        Metadata = new V1ObjectMeta
                        {
                            Labels = new Dictionary<string, string>
                            {
                                {"app.kubernetes.io/name", "ingress-nginx"},
                                {"app.kubernetes.io/instance", "ingress-nginx"},
                                {"app.kubernetes.io/component", "controller"}
                            }
                        },
                        Spec = new V1PodSpec
                        {
                            DnsPolicy = "ClusterFirst",
                            ServiceAccount = "ingress-nginx",
                            TerminationGracePeriodSeconds = 300,
                            Containers = new List<V1Container>
                            {
                                new V1Container
                                {
                                    Name = "controller",
                                    Image = $"{Values.Images.Ingress.Repository}:{Values.Images.Ingress.Tag}",
                                    ImagePullPolicy = Values.Images.Ingress.PullPolicy,
                                    Args = new List<string>
                                    {
                                        @"/nginx-ingress-controller",
                                        "--tcp-services-configmap=$(POD_NAMESPACE)/tcp-services",
                                        "--udp-services-configmap=$(POD_NAMESPACE)/udp-services",
                                        "--publish-service=$(POD_NAMESPACE)/ingress-nginx-controller",
                                        "--election-id=ingress-controller-leader",
                                        "--configmap=$(POD_NAMESPACE)/ingress-nginx-controller",
                                        "--ingress-class=nginx",
                                        "--validating-webhook=:8443",
                                        "--validating-webhook-certificate=/usr/local/certificates/cert",
                                        "--validating-webhook-key=/usr/local/certificates/key",
                                    },
                                    SecurityContext = new V1SecurityContext
                                    {
                                        AllowPrivilegeEscalation = true,
                                        Capabilities = new V1Capabilities
                                        {
                                            Drop = new List<string>
                                            {
                                                "ALL"
                                            },
                                            Add = new List<string>
                                            {
                                                "NET_BIND_SERVICE"
                                            }
                                        },
                                        RunAsUser = 101
                                    },
                                    Env = new List<V1EnvVar>
                                    {
                                        new V1EnvVar
                                        {
                                            Name = "POD_NAME",
                                            ValueFrom = new V1EnvVarSource
                                            {
                                                FieldRef = new V1ObjectFieldSelector
                                                {
                                                    FieldPath = "metadata.name"
                                                }
                                            }
                                        },
                                        new V1EnvVar
                                        {
                                            Name = "POD_NAMESPACE",
                                            ValueFrom = new V1EnvVarSource
                                            {
                                                FieldRef = new V1ObjectFieldSelector
                                                {
                                                    FieldPath = "metadata.namespace"
                                                }
                                            }
                                        }
                                    },
                                    Ports = new List<V1ContainerPort>
                                    {
                                        new V1ContainerPort
                                        {
                                            Name = "http",
                                            ContainerPort = 80,
                                            Protocol = "TCP"
                                        },
                                        new V1ContainerPort
                                        {
                                            Name = "https",
                                            ContainerPort = 443,
                                            Protocol = "TCP"
                                        },
                                        new V1ContainerPort
                                        {
                                            Name = "webhook",
                                            ContainerPort = 8443,
                                            Protocol = "TCP"
                                        }
                                    },
                                    LivenessProbe = new V1Probe
                                    {
                                        HttpGet = new V1HTTPGetAction
                                        {
                                            Path = "/healthz",
                                            Port = 10254,
                                            Scheme = "HTTP"
                                        },
                                        FailureThreshold = 5,
                                        InitialDelaySeconds = 10,
                                        PeriodSeconds = 10,
                                        SuccessThreshold = 1,
                                        TimeoutSeconds = 1
                                    },
                                    ReadinessProbe = new V1Probe
                                    {
                                        HttpGet = new V1HTTPGetAction
                                        {
                                            Path = "/healthz",
                                            Port = 10254,
                                            Scheme = "HTTP"
                                        },
                                        FailureThreshold = 3,
                                        InitialDelaySeconds = 10,
                                        PeriodSeconds = 10,
                                        SuccessThreshold = 1,
                                        TimeoutSeconds = 1
                                    },
                                    Lifecycle = new V1Lifecycle
                                    {
                                        PreStop = new V1Handler
                                        {
                                            Exec = new V1ExecAction
                                            {
                                                Command = new List<string>
                                                {
                                                    "/wait-shutdown"
                                                }
                                            }
                                        }
                                    },
                                    VolumeMounts = new List<V1VolumeMount>
                                    {
                                        new V1VolumeMount
                                        {
                                            Name = "webhook-cert",
                                            MountPath = "/usr/local/certificates/",
                                            ReadOnlyProperty = true
                                        }
                                    },
                                    Resources = new V1ResourceRequirements
                                    {
                                        Requests = new Dictionary<string, ResourceQuantity>
                                        {
                                            {"memory", new ResourceQuantity("90Mi") },
                                            {"cpu", new ResourceQuantity("100m") },
                                        }
                                    }
                                }
                            },
                            Volumes = new List<V1Volume>
                            {
                                new V1Volume
                                {
                                    Name = "webhook-cert",
                                    Secret = new V1SecretVolumeSource
                                    {
                                        SecretName = "ingress-nginx-admission"
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public RunResult Run(string dryRun = default)
        {
            var result = new RunResult();
            try
            {
                result.Response = _client.CreateNamespacedDeployment(_deployment, Values.Namespace, dryRun); 
                result.Success = true;
            }
            catch (Microsoft.Rest.RestException ex)
            {
                if (ex is Microsoft.Rest.HttpOperationException e)
                    result.HttpOperationException = e;
                else
                    result.Exception = ex;
                result.Success = false;
            }
            return result;
        }
    }
}
