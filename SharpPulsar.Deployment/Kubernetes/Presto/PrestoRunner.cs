using SharpPulsar.Deployment.Kubernetes.Presto.Coordinator;
using SharpPulsar.Deployment.Kubernetes.Presto.Worker;
using System;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Presto
{
    internal class PrestoRunner
    {
        private readonly PrestoCoordinatorConfigMap _prestoCoordinatorConfigMap;
        private readonly PrestoCoordinatorStatefulSet _prestoCoordinatorStatefulSet;
        private readonly PrestoWorkerConfigMap _prestoWorkerConfigMap;
        private readonly PrestoWorkerStatefulSet _prestoWorkerStatefulSet;
        private readonly PrestoService _prestoService;
        private Dictionary<string, object> _results;
        public PrestoRunner(ConfigMap configMap, StatefulSet stateful, Service service)
        {
            if (configMap == null)
                throw new ArgumentNullException("ConfigMap is null");

            if (service == null)
                throw new ArgumentNullException("Service is null");

            if (stateful == null)
                throw new ArgumentNullException("StatefulSet is null");

            _prestoCoordinatorConfigMap = new PrestoCoordinatorConfigMap(configMap);
            _prestoCoordinatorStatefulSet = new PrestoCoordinatorStatefulSet(stateful);
            _prestoWorkerConfigMap = new PrestoWorkerConfigMap(configMap);
            _prestoWorkerStatefulSet = new PrestoWorkerStatefulSet(stateful);
            _prestoService = new PrestoService(service);
        }
        public bool Run(out Dictionary<string, object> results, string dryRun = default)
        {
            if (Values.PrestoCoordinator.Enabled)
            {
                _results = new Dictionary<string, object>();

                var conf = _prestoCoordinatorConfigMap.Run(dryRun);
                _results.Add("ConfigMap", conf);

                var srv = _prestoService.Run(dryRun);
                _results.Add("Service", srv);


                var state = _prestoCoordinatorStatefulSet.Run(dryRun);
                _results.Add("StatefulSet", state);

                if (Values.PrestoWorker.Replicas > 0)
                {
                    var confw = _prestoWorkerConfigMap.Run(dryRun);
                    _results.Add("ConfigMapWorker", confw);

                    var statew = _prestoWorkerStatefulSet.Run(dryRun);
                    _results.Add("StatefulSetWorker", statew);
                }

                results = _results;
                return true;
            }
            results = _results;
            return false;
        }
    }
}
