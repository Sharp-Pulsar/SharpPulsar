using System;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Proxy
{
    internal class ProxyRunner
    {
        private readonly ProxyConfigMap _config;
        private readonly ProxyPodDisruptionBudget _pdb;
        private readonly ProxyService _service;
        private readonly ProxyStatefulset _stateful;
        public ProxyRunner(ConfigMap configMap, PodDisruptionBudget pdb, Service service, StatefulSet statefulSet)
        {
            if (configMap == null)
                throw new ArgumentNullException("ConfigMap is null");

            if (pdb == null)
                throw new ArgumentNullException("PodDisruptionBudget is null");

            if (service == null)
                throw new ArgumentNullException("Service is null");

            if (statefulSet == null)
                throw new ArgumentNullException("StatefulSet is null");

            _config = new ProxyConfigMap(configMap);
            _pdb = new ProxyPodDisruptionBudget(pdb);
            _service = new ProxyService(service);
            _stateful = new ProxyStatefulset(statefulSet);
        }
        public IEnumerable<RunResult> Run(string dryRun = default)
        {
            RunResult result;
            if(Values.Settings.Proxy.Enabled)
            {

                result = _config.Run(dryRun);
                yield return result;

                if (Values.Settings.Proxy.UsePolicyPodDisruptionBudget)
                {
                    result = _pdb.Run(dryRun);
                    yield return result;
                }

                result = _service.Run(dryRun);
                yield return result;

                result = _stateful.Run(dryRun); 
                yield return result;
            }
            
        }
        
    }
}
