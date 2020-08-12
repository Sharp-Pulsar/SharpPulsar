using System;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    internal class BrokerRunner
    {
        private readonly BrokerClusterRole _brokerClusterRole;
        private readonly BrokerClusterRoleBinding _brokerClusterRoleBinding;
        private readonly BrokerConfigMap _brokerConfigMap;
        private readonly BrokerPodDisruptionBudget _brokerPodDisruptionBudget;
        private readonly BrokerService _brokerService;
        private readonly BrokerServiceAccount _brokerServiceAccount;
        private readonly BrokerStatefulset _brokerStatefulset;
        private readonly FunctionWorkerConfigMap _function;
        public BrokerRunner(ConfigMap configMap, PodDisruptionBudget pdb, Service service, ServiceAccount serviceAccount, StatefulSet statefulSet, ClusterRole clusterRole, ClusterRoleBinding clusterRoleBinding)
        {
            if (clusterRole == null)
                throw new ArgumentNullException("ClusterRole is null");

            if (configMap == null)
                throw new ArgumentNullException("ConfigMap is null");

            if (pdb == null)
                throw new ArgumentNullException("PodDisruptionBudget is null");

            if (service == null)
                throw new ArgumentNullException("Service is null");

            if (serviceAccount == null)
                throw new ArgumentNullException("ServiceAccount is null");

            if (statefulSet == null)
                throw new ArgumentNullException("StatefulSet is null");

            if (clusterRoleBinding == null)
                throw new ArgumentNullException("ClusterRoleBinding is null");

            _brokerClusterRole = new BrokerClusterRole(clusterRole);
            _brokerClusterRoleBinding = new BrokerClusterRoleBinding(clusterRoleBinding);
            _brokerConfigMap = new BrokerConfigMap(configMap);
            _brokerPodDisruptionBudget = new BrokerPodDisruptionBudget(pdb);
            _brokerService = new BrokerService(service);
            _brokerServiceAccount = new BrokerServiceAccount(serviceAccount);
            _brokerStatefulset = new BrokerStatefulset(statefulSet);
            _function = new FunctionWorkerConfigMap(configMap);
        }
        public IEnumerable<object> Run(string dryRun = default)
        {
            object result;
            if (Values.Settings.Broker.Enabled)
            {
                result = _brokerClusterRoleBinding.Run(dryRun);
                yield return result;

                result = _brokerClusterRole.Run(dryRun);
                yield return result;

                result = _brokerConfigMap.Run(dryRun);
                yield return result;

                if (Values.Settings.Broker.UsePolicyPodDisruptionBudget)
                {
                    result = _brokerPodDisruptionBudget.Run(dryRun);
                    yield return result;
                }
                result = _brokerService.Run(dryRun);
                yield return result;

                result = _brokerServiceAccount.Run(dryRun);
                yield return result;

                result = _brokerStatefulset.Run(dryRun);
                yield return result;

                if (Values.Settings.Function.Enabled)
                {
                    result = _function.Run(dryRun);
                    yield return result;
                }
            }
        }
    }
}
