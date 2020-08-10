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
        private Dictionary<string, object> _results;
        public ProxyRunner(ConfigMap configMap, PodDisruptionBudget pdb, Service service, StatefulSet statefulSet)
        {
            _results = new Dictionary<string, object>();
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
        public bool Run(out Dictionary<string, object> results, string dryRun = default)
        {
            if(Values.Proxy.Enabled)
            {
                _results = new Dictionary<string, object>();

                var conf = _config.Run(dryRun);
                _results.Add("ConfigMap", conf);

                if (Values.Proxy.UsePolicyPodDisruptionBudget)
                {
                    var pdb = _pdb.Run(dryRun);
                    _results.Add("Pdb", pdb);
                }

                var srv = _service.Run(dryRun);
                _results.Add("Service", srv);

                var state = _stateful.Run(dryRun);
                _results.Add("StatefulSet", state);
                results = _results;
                return true;
            }
            results = _results;
            return false;
        }
        
    }
}
