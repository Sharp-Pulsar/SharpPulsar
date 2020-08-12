using k8s;
using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.NetworkCenter
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
                    Name = $"{Values.ReleaseName}-nginx-ingress-controller",
                    NamespaceProperty = Values.Namespace,
                    Labels = new Dictionary<string, string>
                    {
                        { "app", Values.App },
                        { "cluster", Values.Cluster },
                        { "release", Values.ReleaseName },
                        { "component", "nginx-ingress-controller" }
                    }
                },
                Spec = new V1DeploymentSpec
                {
                    Replicas = Values.Ingress.Replicas,
                    Selector = new V1LabelSelector
                    {
                        MatchLabels = new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component", "nginx-ingress-controller"  }
                            }
                    },
                    Template = new V1PodTemplateSpec
                    {
                        Metadata = new V1ObjectMeta
                        {
                            Labels = new Dictionary<string, string>
                            {
                                { "app", Values.App },
                                { "cluster", Values.Cluster },
                                { "release", Values.ReleaseName },
                                { "component", "nginx-ingress-controller" }
                            },
                            Annotations = new Dictionary<string, string>
                            {
                                { "\"prometheus.io/port\"", "10254"},
                                { "\"prometheus.io/scrape\"", "true" }
                            }
                        },
                        Spec = new V1PodSpec
                        {
                            NodeSelector = new Dictionary<string, string> { },
                            Tolerations = new List<V1Toleration> { },
                            ServiceAccount = Values.Ingress.Rbac? $"{Values.ReleaseName}-nginx-ingress-serviceaccount" :"",
                            TerminationGracePeriodSeconds = Values.Ingress.GracePeriodSeconds,
                            Containers = new List<V1Container>
                            {
                                new V1Container
                                {
                                    Name = "nginx-ingress-controller",
                                    Image = $"{Values.Images.Ingress.Repository}:{Values.Images.Ingress.Tag}",
                                    ImagePullPolicy = Values.Images.Ingress.PullPolicy,
                                    Args = new List<string>
                                    {
                                        $@"/nginx-ingress-controller 
                                            --election-id=ingress-controller-leader
                                            --configmap={Values.Namespace}/{Values.ReleaseName}-nginx-configuration
                                            --tcp-services-configmap={Values.Namespace}/{Values.ReleaseName}-tcp-services
                                            --udp-services-configmap={Values.Namespace}/{Values.ReleaseName}-udp-services
                                            --publish-service={Values.Namespace}/{Values.ReleaseName}-nginx-ingress-controller
                                            --annotations-prefix=nginx.ingress.kubernetes.io"
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
                                        RunAsUser = 33
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
                                        FailureThreshold = 3,
                                        InitialDelaySeconds = 10,
                                        PeriodSeconds = 10,
                                        SuccessThreshold = 1,
                                        TimeoutSeconds = 10
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
                                        TimeoutSeconds = 10
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
