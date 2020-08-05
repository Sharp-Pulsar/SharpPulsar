﻿using k8s.Models;
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
    }
}
