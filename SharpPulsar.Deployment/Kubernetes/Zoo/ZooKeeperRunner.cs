using SharpPulsar.Deployment.Kubernetes.Bookie;
using System;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    internal class ZooKeeperRunner
    {
        private readonly ClusterInitializer _clusterInit;
        private readonly ZooKeeperConfigMap _config;
        private readonly ZooKeeperPodDisruptionBudget _pdb;
        private readonly ZooKeeperService _service;
        private readonly ZooKeeperStatefulSet _statefulSet;
        private readonly ZooKeeperStorageClass _storage;
        private readonly ZooKeeperStorageClass _datalog;
        public ZooKeeperRunner(Job job, ConfigMap configMap, PodDisruptionBudget pdb, Service service, StatefulSet statefulSet, StorageClass storageClass)
        {
            if (job == null)
                throw new ArgumentNullException("Job is null");
            
            if (configMap == null)
                throw new ArgumentNullException("ConfigMap is null");
            
            if (pdb == null)
                throw new ArgumentNullException("PodDisruptionBudget is null");

            if (service == null)
                throw new ArgumentNullException("Service is null");

            if (statefulSet == null)
                throw new ArgumentNullException("StatefulSet is null");

            if (storageClass == null)
                throw new ArgumentNullException("StorageClass is null");

            _config = new ZooKeeperConfigMap(configMap);
            _clusterInit = new ClusterInitializer(job);
            _pdb = new ZooKeeperPodDisruptionBudget(pdb);
            _service = new ZooKeeperService(service);
            _statefulSet = new ZooKeeperStatefulSet(statefulSet);
            _storage = new ZooKeeperStorageClass(storageClass);
            _datalog = new ZooKeeperStorageClass(storageClass);
        }

        public IEnumerable<object> Run(string dryRun = default)
        {
            object result;
            if(Values.Settings.ZooKeeper.Enabled)
            {
                result = _config.Run(dryRun);
                yield return result;

                result = _config.GenZkConf(dryRun);
                yield return result;

                if (Values.Settings.ZooKeeper.UsePolicyPodDisruptionBudget)
                {
                    result = _pdb.Run(dryRun);
                    yield return result;
                }

                result =_service.Run(dryRun);
                yield return result;

                result = _statefulSet.Run(dryRun);
                yield return result;

                if (Values.Persistence && Values.Settings.ZooKeeper.Persistence && !Values.LocalStorage)
                {
                    result = _storage.Run(dryRun);
                    yield return result;

                    //result = _datalog.Run(dryRun);
                    //yield return result;

                }
            }
            if (Values.Initialize && Values.Settings.Broker.Enabled)
            {
                result = _clusterInit.Run(dryRun);
                yield return result;
            }
        }
    }
}
