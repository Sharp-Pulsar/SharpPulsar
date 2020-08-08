using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Proxy
{
    internal class ProxyRunner
    {
        private ProxyConfigMap _config;
        private ProxyPodDisruptionBudget _pdb;
        private ProxyService _service;
        private ProxyServiceIngress _serviceIngress;
        private ProxyServiceAccount _serviceAcct;
        private ProxyStatefulset _stateful;
        private Dictionary<string, object> _results;
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


                if (Values.Ingress.Proxy.Enabled)
                {
                    var ingress = _serviceIngress.Run(dryRun);
                    _results.Add("ServiceIngress", ingress);
                }

                var srvAcct = _serviceAcct.Run(dryRun);
                _results.Add("ServiceAccount", srvAcct);

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
