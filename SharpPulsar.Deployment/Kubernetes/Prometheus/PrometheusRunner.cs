using SharpPulsar.Deployment.Kubernetes.Prometheus.Rbac;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Prometheus
{
    internal class PrometheusRunner
    {
        private readonly PrometheusClusterRole _prometheusClusterRole;
        private readonly PrometheusClusterRoleBinding _prometheusClusterRoleBinding;
        private readonly PrometheusServiceAccount _prometheusServiceAccount;
        private readonly PrometheusConfigmap _prometheusConfigmap;
        private readonly PrometheusService _prometheusService;
        private readonly PrometheusStatefulset _prometheusStatefulset;
        public PrometheusRunner(ClusterRole clusterRole, ClusterRoleBinding clusterRoleBinding, ServiceAccount serviceAccount, Service service, ConfigMap configMap, StatefulSet statefulSet)
        {
            if (clusterRole == null)
                throw new ArgumentNullException("ClusterRole is null");

            if (configMap == null)
                throw new ArgumentNullException("ConfigMap is null");

            if (service == null)
                throw new ArgumentNullException("Service is null");

            if (serviceAccount == null)
                throw new ArgumentNullException("ServiceAccount is null");

            if (statefulSet == null)
                throw new ArgumentNullException("StatefulSet is null");

            if (clusterRoleBinding == null)
                throw new ArgumentNullException("ClusterRoleBinding is null");

            _prometheusClusterRole = new PrometheusClusterRole(clusterRole);
            _prometheusClusterRoleBinding = new PrometheusClusterRoleBinding(clusterRoleBinding);
            _prometheusServiceAccount = new PrometheusServiceAccount(serviceAccount);
            _prometheusConfigmap = new PrometheusConfigmap(configMap);
            _prometheusService = new PrometheusService(service);
            _prometheusStatefulset = new PrometheusStatefulset(statefulSet);
        }
        public IEnumerable<RunResult> Run(string dryRun = default)
        {
            RunResult result;
            if (Values.Monitoring.Prometheus)
            {

                if (Values.Rbac.Enabled)
                {
                    result = _prometheusClusterRole.Run(dryRun);
                    yield return result;

                    result = _prometheusClusterRoleBinding.Run(dryRun);
                    yield return result;

                    result = _prometheusServiceAccount.Run(dryRun);
                    yield return result;
                }
                result = _prometheusConfigmap.Run(dryRun);
                yield return result;

                result = _prometheusService.Run(dryRun);
                yield return result;

                result = _prometheusStatefulset.Run(dryRun);
                yield return result;

            }
        }
    }
}
