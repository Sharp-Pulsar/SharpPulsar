using k8s;
using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Grafana
{
    internal class GrafanaDeployment
    {
        private readonly IKubernetes _client;
        private readonly V1Deployment _deployment;
        public GrafanaDeployment(IKubernetes client)
        {
            _client = client;
            _deployment = new V1Deployment
            {
                Metadata = new V1ObjectMeta
                {
                    Name = $"{Values.ReleaseName}-{Values.Settings.Grafana.Name}",
                    NamespaceProperty = Values.Namespace,
                    Labels = new Dictionary<string, string>
                    {
                        { "app", Values.App },
                        { "cluster", Values.Cluster },
                        { "release", Values.ReleaseName },
                        { "component", Values.Settings.Grafana.Name }
                    }
                },
                Spec = new V1DeploymentSpec
                {
                    Replicas = Values.Settings.Grafana.Replicas,
                    Selector = new V1LabelSelector
                    {
                        MatchLabels = new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Grafana.Name  }
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
                                { "component", Values.Settings.Grafana.Name }
                            },
                            Annotations = Values.Settings.Grafana.Annotations.Template
                        },
                        Spec = new V1PodSpec
                        {
                            NodeSelector = Values.Settings.Grafana.NodeSelector,
                            Tolerations = Values.Settings.Grafana.Tolerations,
                            TerminationGracePeriodSeconds = Values.Settings.Grafana.GracePeriodSeconds,
                            Containers = new List<V1Container>
                            {
                                new V1Container
                                {
                                    Name = $"{Values.ReleaseName}-{Values.Settings.Grafana.Name}",
                                    Image = $"{Values.Images.Grafana.Repository}:{Values.Images.Grafana.Tag}",
                                    ImagePullPolicy = Values.Images.Grafana.PullPolicy,
                                    Env = new List<V1EnvVar>
                                    {
                                        //for supporting apachepulsar/pulsar-grafana
                                        new V1EnvVar
                                        {
                                            Name = "PROMETHEUS_URL",
                                            Value = $"http://{Values.ReleaseName}-{Values.Settings.Prometheus.Name}:9090/"
                                        },
                                        //for supporting streamnative/apache-pulsar-grafana-dashboard
                                        new V1EnvVar
                                        {
                                            Name = "PULSAR_PROMETHEUS_URL",
                                            Value = $"http://{Values.ReleaseName}-{Values.Settings.Prometheus.Name}:9090/"
                                        },
                                        new V1EnvVar
                                        {
                                            Name = "PULSAR_CLUSTER",
                                            Value = Values.ReleaseName
                                        },
                                        new V1EnvVar
                                        {
                                            Name = "GRAFANA_ADMIN_USER",
                                            ValueFrom = new V1EnvVarSource
                                            {
                                                SecretKeyRef = new V1SecretKeySelector
                                                {
                                                    Name = $"{Values.ReleaseName}-{Values.Settings.Grafana.Name}-secret",
                                                    Key = "GRAFANA_ADMIN_USER"
                                                }
                                            }
                                            
                                        },
                                        new V1EnvVar
                                        {
                                            Name = "GRAFANA_ADMIN_PASSWORD",
                                            ValueFrom = new V1EnvVarSource
                                            {
                                                SecretKeyRef = new V1SecretKeySelector
                                                {
                                                    Name = $"{Values.ReleaseName}-{Values.Settings.Grafana.Name}-secret",
                                                    Key = "GRAFANA_ADMIN_PASSWORD"
                                                }
                                            }
                                            
                                        }
                                    },
                                    Ports = new List<V1ContainerPort>
                                    {
                                        new V1ContainerPort
                                        {
                                            Name = "server",
                                            ContainerPort = Values.Ports.Grafana["targetPort"]
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
