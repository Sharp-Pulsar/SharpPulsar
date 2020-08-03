﻿using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Builders;

namespace SharpPulsar.Deployment.Kubernetes.Bookie
{
    public class BookieKeeperStatefulSet
    {
        private readonly IKubernetes _client;
        public BookieKeeperStatefulSet(IKubernetes client)
        {
            _client = client; 
        }
        public static StatefulSetBuilder Builder()
        {
            return new StatefulSetBuilder();
        }
        public V1StatefulSet Run(string ns, string dryRun = default)
        {
            return _client.CreateNamespacedStatefulSet(Builder().Build(), ns, dryRun);
        }
    }
}
