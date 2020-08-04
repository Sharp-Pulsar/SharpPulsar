using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Zoo;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.AutoRecovery
{
    public class AutoRecoveryStatefulSet
    {
        private readonly StatefulSet _set;
        public AutoRecoveryStatefulSet(StatefulSet set)
        {
            _set = set;
        }

        public V1StatefulSet Run(string dryRun = default)
        {
            _set.Builder()
                .Name($"{Values.ReleaseName}-recovery")
                .Namespace(Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component","recovery" }
                            })
                .SpecBuilder()
                .ServiceName($"{Values.ReleaseName}-recovery")
                .Replication(1)
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component","recovery" }
                            })
                .UpdateStrategy("RollingUpdate")
                .PodManagementPolicy("Parallel")
                .VolumeClaimTemplates(new List<V1PersistentVolumeClaim>
                {
                     new V1PersistentVolumeClaim
                     {

                     }
                })
                .TemplateBuilder()
                .Metadata(new Dictionary<string, string>
                {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component","recovery" }
                            }, new Dictionary<string, string>
                            {
                                {"prometheus.io/scrape", "true" },
                                {"prometheus.io/port", "8000" },
                                //{"checksum/config", @"{{ include (print $.Template.BasePath "/bookkeeper/bookkeeper-autorecovery-configmap.yaml") . | sha256sum }}" }
                            }
                 )
                .SpecBuilder()
                .Tolerations(new List<V1Toleration>())
                .PodAntiAffinity(new List<V1PodAffinityTerm>
                {
                    new V1PodAffinityTerm
                    {
                        LabelSelector = new V1LabelSelector
                        {
                            MatchExpressions = new List<V1LabelSelectorRequirement>
                            {
                                new V1LabelSelectorRequirement{ Key = "app", OperatorProperty = "In", Values = new List<string>{$"{Values.ReleaseName}-bookie" } },
                                new V1LabelSelectorRequirement{ Key = "release", OperatorProperty = "In", Values = new List<string>{$"{Values.ReleaseName}" } },
                                new V1LabelSelectorRequirement{ Key = "component", OperatorProperty = "In", Values = new List<string>{ "bookie" }}
                            }
                        },
                        TopologyKey = "kubernetes.io/hostname"
                    }
                })
                .TerminationGracePeriodSeconds(30)
                .InitContainers(new List<V1Container>())///HERE
                .Containers(new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{Values.ReleaseName}-recovery",
                        Image = $"{Values.Images.ZooKeeper.Repository}:{Values.Images.ZooKeeper.Tag}",
                        ImagePullPolicy = Values.Images.ZooKeeper.PullPolicy,
                        Resources = new V1ResourceRequirements{ Requests = new Dictionary<string, ResourceQuantity>{ { "memory", new ResourceQuantity("64Mi") }, { "cpu", new ResourceQuantity("0.05") } } },
                        Command = new []{ "sh", "-c" },
                        Args = new List<string>
                        {
                            "bin/apply-config-from-env.py conf/zookeeper.conf;",
                            "bin/gen-zk-conf.sh conf/zookeeper.conf 0 participant;",
                            "cat conf/zookeeper.conf;",
                            "bin / pulsar zookeeper;"
                        },
                        Ports = new List<V1ContainerPort>
                        {
                            new V1ContainerPort{Name = "metrics", ContainerPort = 8000 },
                            new V1ContainerPort{Name = "client", ContainerPort = 2181 },
                            //new V1ContainerPort{Name = "client-tls", ContainerPort = 2281 },
                            new V1ContainerPort{Name = "follower", ContainerPort = 2888 },
                            new V1ContainerPort{Name = "leader-election", ContainerPort = 3888 }
                        },
                        Env = new List<V1EnvVar>
                        {
                            new V1EnvVar{ Name = "ZOOKEEPER_SERVERS", Value ="pulsar-zookeeper-0,pulsar-zookeeper-1,pulsar-zookeeper-2"}
                        },
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource{ConfigMapRef = new V1ConfigMapEnvSource{ Name = $"{Values.ReleaseName}-zookeeper"}}
                        },
                        ReadinessProbe = new V1Probe
                        {
                            Exec = new V1ExecAction
                            {
                                Command = new List<string>{ "bin/pulsar-zookeeper-ruok.sh" }
                            },
                            InitialDelaySeconds = 10,
                            FailureThreshold = 10,
                            PeriodSeconds = 30
                        },
                        LivenessProbe = new V1Probe
                        {
                            Exec = new V1ExecAction
                            {
                                Command = new List<string>{ "bin/pulsar-zookeeper-ruok.sh" }
                            },
                            InitialDelaySeconds = 10,
                            FailureThreshold = 10,
                            PeriodSeconds = 30
                        }/*,
                        StartupProbe = new V1Probe
                        {
                            Exec = new V1ExecAction
                            {
                                Command = new List<string>{ "bin/pulsar-zookeeper-ruok.sh" }
                            },
                            InitialDelaySeconds = 10,
                            FailureThreshold = 30,
                            PeriodSeconds = 30
                        }*/,
                        VolumeMounts = new List<V1VolumeMount>
                        {
                            new V1VolumeMount{Name = "pulsar-zookeeper-data", MountPath = "/pulsar/data"},
                            new V1VolumeMount{Name = "pulsar-zookeeper-genzkconf", MountPath = "/pulsar/bin/gen-zk-conf.sh", SubPath = "gen-zk-conf.sh"},
                            new V1VolumeMount{Name = "pulsar-zookeeper-log4j2", MountPath = "/pulsar/conf/log4j2.yaml", SubPath = "og4j2.yaml"},
                            /*new V1VolumeMount{Name = "zookeeper-certs", MountPath = "/pulsar/certs/zookeeper", ReadOnlyProperty = true},
                            new V1VolumeMount{Name = "ca", MountPath = "/pulsar/certs/ca", ReadOnlyProperty = true},
                            new V1VolumeMount{Name = "keytool", MountPath = "/pulsar/keytool/keytool.sh", SubPath= "keytool.sh"}*/
                        }
                    }
                })
                .Volumes(new List<V1Volume>
                {
                    /*new V1Volume {Name = "zookeeper-certs", Secret = new V1SecretVolumeSource{SecretName ="{{ .Release.Name }}-{{ .Values.tls.zookeeper.cert_name }}", Items = new List<V1KeyToPath>{ new V1KeyToPath {Key = "tls.crt", Path = "tls.crt" }, new V1KeyToPath { Key = "tls.key", Path = "tls.key" } } }},
                    new V1Volume {Name = "ca", Secret = new V1SecretVolumeSource{SecretName ="{{ .Release.Name }}-ca-tls", Items = new List<V1KeyToPath>{ new V1KeyToPath {Key = "ca.crt", Path = "ca.crt" } } }},
                    new V1Volume{Name = "keytool", ConfigMap = new V1ConfigMapVolumeSource{Name = "{{ template pulsar.fullname . }}-keytool-configmap", DefaultMode = 0755}}*/
                })
                ;

            return _set.Run(_set.Builder(), Values.Namespace, dryRun);
        }
    }
}
