using k8s;
using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.ExternalDns.Rbac;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.ExternalDns
{
    internal class ExternalDnsRunner
    {
        private readonly ExternalDnsRole _externalDnsRole;
        private readonly ExternalDnsRoleBinding _externalDnsRoleBinding;
        private readonly ExternalDnsServiceAccount _externalDnsServiceAccount;
        private readonly ExternalDnsDeployment _externalDnsDeployment;
        public ExternalDnsRunner(IKubernetes client, ClusterRole role, ClusterRoleBinding roleBinding, ServiceAccount serviceAccount)
        {
            if (role == null)
                throw new ArgumentNullException("ClusterRole is null");

            if (roleBinding== null)
                throw new ArgumentNullException("ClusterRoleBinding is null");

            if (serviceAccount == null)
                throw new ArgumentNullException("ServiceAccount is null");
            _externalDnsRole = new ExternalDnsRole(role);
            _externalDnsRoleBinding = new ExternalDnsRoleBinding(roleBinding);
            _externalDnsServiceAccount = new ExternalDnsServiceAccount(serviceAccount);
            _externalDnsDeployment = new ExternalDnsDeployment(client);
        }
        public IEnumerable<RunResult> Run(string dryRun = default)
        {
            if (Values.Ingress.Enabled)
            {
                var acct = _externalDnsServiceAccount.Run(dryRun);
                yield return acct;

                var role = _externalDnsRole.Run(dryRun);
                yield return role;

                var rb = _externalDnsRoleBinding.Run(dryRun);
                yield return rb;

                var dep = _externalDnsDeployment.Run(dryRun);
                yield return dep;
            }
        }
    }
}
