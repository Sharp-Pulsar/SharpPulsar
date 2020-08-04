﻿using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Bookie.Cluster
{
    public class BookieClusterRoleBinding
    {
        private readonly ClusterRoleBinding _config;
        public BookieClusterRoleBinding(ClusterRoleBinding config)
        {
            _config = config;
        }
        public V1ClusterRoleBinding Run(string dryRun = default)
        {
            _config.Builder()
                .Name($"{Values.ReleaseName}-{Values.BookKeeper.ComponentName }-clusterrolebinding")
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .RoleRef("rbac.authorization.k8s.io", "ClusterRole", $"{Values.ReleaseName}-{Values.BookKeeper.ComponentName }-clusterrole")
                .AddSubject(Values.Namespace, "ServiceAccount", $"{Values.ReleaseName}-{Values.BookKeeper.ComponentName }-acct");
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}