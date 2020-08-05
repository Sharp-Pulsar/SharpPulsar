using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class EnvVar
    {
        public static List<V1EnvVar> Broker(bool advertisedPodIP)
        {
            if (advertisedPodIP)
                return new List<V1EnvVar>
                {
                    new V1EnvVar
                    {
                        Name = "advertisedAddress",
                                ValueFrom = new V1EnvVarSource
                                {
                                    FieldRef = new V1ObjectFieldSelector
                                    {
                                        FieldPath = "status.podIP"
                                    }
                                }
                    }
                };

            return new List<V1EnvVar>();
        }
        public static List<V1EnvVar> ZooKeeper()
        {
            var zkServers = new List<string>();
            for (var i = 0; i < Values.ZooKeeper.Replicas; i++)
                zkServers.Add($"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}-{i}");

            return new List<V1EnvVar>
                {
                    new V1EnvVar
                    {
                        Name = "ZOOKEEPER_SERVERS",
                        Value = string.Join(",", zkServers)
                    }
                };
        }

        public static List<V1EnvVar> BookKeeper(string port = "bookie")
        {
            var bks = new List<V1EnvVar>
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
                            },
                            new V1EnvVar
                            {
                                Name = "VOLUME_NAME",
                                Value = $"{Values.ReleaseName}-{Values.BookKeeper.ComponentName}-journal"
                            },
                            new V1EnvVar
                            {
                                Name = "BOOKIE_PORT",
                                Value = Values.Ports.Bookie[port].ToString()
                            }
            };
            if ((bool)Values.BookKeeper.ExtraConfig.Holder["RackAware"])
            {
                bks.Add(new V1EnvVar
                {
                    Name = "BOOKIE_RACK_AWARE_ENABLED",
                    Value = "true"
                });
            }
            return bks;
        }
    }
}
