using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    internal class BrokerRunner
    {
        private BrokerClusterRole _brokerClusterRole;
        private BrokerClusterRoleBinding _brokerClusterRoleBinding;
        private BrokerConfigMap _brokerConfigMap;
        private BrokerPodDisruptionBudget _brokerPodDisruptionBudget;
        private BrokerService _brokerService;
        private BrokerServiceAccount _brokerServiceAccount;
        private BrokerStatefulset _brokerStatefulset;
        private FunctionWorkerConfigMap _function;
        private Dictionary<string, object> _results;
        public BrokerRunner(ConfigMap configMap, PodDisruptionBudget pdb, Service service, ServiceAccount serviceAccount, StatefulSet statefulSet, ClusterRole clusterRole, ClusterRoleBinding clusterRoleBinding)
        {
            _results = new Dictionary<string, object>();
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
        public bool Run(out Dictionary<string, object> results, string dryRun = default)
        {
            if (Values.Broker.Enabled)
            {
                _results = new Dictionary<string, object>();
                var crb = _brokerClusterRoleBinding.Run(dryRun);
                _results.Add("ClusterRoleBinding", crb);

                var cr = _brokerClusterRole.Run(dryRun);
                _results.Add("ClusterRole", cr);

                var conf = _brokerConfigMap.Run(dryRun);
                _results.Add("ConfigMap", conf);

                if (Values.Broker.UsePolicyPodDisruptionBudget)
                {
                    var pdb = _brokerPodDisruptionBudget.Run(dryRun);
                    _results.Add("Pdb", pdb);
                }
                var srv = _brokerService.Run(dryRun);
                _results.Add("Service", srv);

                var srvAcct = _brokerServiceAccount.Run(dryRun);
                _results.Add("ServiceAccount", srvAcct);

                var state = _brokerStatefulset.Run(dryRun);
                _results.Add("Statefulset", state);

                if (Values.Functions.Enabled)
                {
                    var function = _function.Run(dryRun);
                    _results.Add("FunctionConfigMap", function);
                }
                results = _results;
                return true;
            }
            results = _results;
            return false;
        }
    }
}
