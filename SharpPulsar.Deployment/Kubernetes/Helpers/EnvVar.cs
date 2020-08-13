using k8s.Models;
using System;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class EnvVar
    {
        public static List<V1EnvVar> Broker()
        {
            if ((bool)Values.ExtraConfigs.Broker.Holder["AdvertisedPodIP"])
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
            for (var i = 0; i < Values.Settings.ZooKeeper.Replicas; i++)
                zkServers.Add($"{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}-{i}");

            return new List<V1EnvVar>
                {
                    new V1EnvVar
                    {
                        Name = "ZOOKEEPER_SERVERS",
                        Value = $@"
{string.Join(",", zkServers)}".Replace(Environment.NewLine, "")
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
                                Value = $"{Values.ReleaseName}-{Values.Settings.BookKeeper.Name}-journal"
                            },
                            new V1EnvVar
                            {
                                Name = "BOOKIE_PORT",
                                Value = Values.Ports.Bookie[port].ToString()
                            }
            };
            if ((bool)Values.ExtraConfigs.Bookie.Holder["RackAware"])
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
