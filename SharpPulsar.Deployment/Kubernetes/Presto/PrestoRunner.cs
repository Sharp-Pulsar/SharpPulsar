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
        public IEnumerable<object> Run(string dryRun = default)
        {
            object result;
            if (Values.Settings.PrestoCoord.Enabled)
            {
                result = _prestoCoordinatorConfigMap.Run(dryRun);
                yield return result;

                result = _prestoService.Run(dryRun);
                yield return result;


                result = _prestoCoordinatorStatefulSet.Run(dryRun);
                yield return result;

                if (Values.Settings.PrestoWorker.Replicas > 0)
                {
                    result = _prestoWorkerConfigMap.Run(dryRun);
                    yield return result;

                    result = _prestoWorkerStatefulSet.Run(dryRun);
                    yield return result;
                }
            }
        }
    }
}
