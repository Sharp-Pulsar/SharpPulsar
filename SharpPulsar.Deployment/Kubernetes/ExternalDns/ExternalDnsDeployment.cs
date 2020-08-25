using k8s;
using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.ExternalDns
{
    internal class ExternalDnsDeployment
    {
        private readonly IKubernetes _client;
        private readonly V1Deployment _deployment;
        public ExternalDnsDeployment(IKubernetes client)
        {
            _client = client;
            _deployment = new V1Deployment
            {
                Metadata = new V1ObjectMeta
                {
                    Name = "external-dns",
                    NamespaceProperty = Values.Namespace
                },
                Spec = new V1DeploymentSpec
                {
                    Replicas = 1,
                    Strategy = new V1DeploymentStrategy
                    {
                        Type = "Recreate"
                    },
                    Selector = new V1LabelSelector
                    {
                        MatchLabels = new Dictionary<string, string>
                            {
                                {"app", "external-dns" }
                            }
                    },
                    Template = new V1PodTemplateSpec
                    {
                        Metadata = new V1ObjectMeta
                        {
                            Labels = new Dictionary<string, string>
                            {
                                { "app", "external-dns" }
                            }
                        },
                        Spec = new V1PodSpec
                        {
                            ServiceAccountName = "external-dns",
                            Containers = new List<V1Container>
                            {
                                new V1Container
                                {
                                    Name = "external-dns",
                                    Image = "registry.opensource.zalan.do/teapot/external-dns:latest",
                                    Args = new List<string>
                                    {
                                        "--source=service",
                                        "--source=ingress",
                                        $"--domain-filter={Values.DomainSuffix}", //(optional) change to match the azure dns zone,
                                        "--provider=azure",
                                        "--azure-resource-group=pulsar"//(optional)
                                    }, 
                                    VolumeMounts = new List<V1VolumeMount>
                                    {
                                        new V1VolumeMount
                                        {
                                            Name = "secret-azuredns-config",
                                            MountPath = "/etc/kubernetes",
                                            ReadOnlyProperty = true
                                        }
                                    }
                                }
                            },
                            Volumes = new List<V1Volume>
                            {
                                new V1Volume
                                {
                                    Name = "secret-azuredns-config",
                                    Secret = new V1SecretVolumeSource
                                    {
                                        SecretName = "secret-azuredns-config"
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
