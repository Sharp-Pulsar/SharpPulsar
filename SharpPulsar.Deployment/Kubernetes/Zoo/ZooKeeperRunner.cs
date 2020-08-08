using SharpPulsar.Deployment.Kubernetes.Bookie;
using System;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    internal class ZooKeeperRunner
    {
        private ClusterInitializer _clusterInit;
        private ZooKeeperConfigMap _config;
        private ZooKeeperPodDisruptionBudget _pdb;
        private ZooKeeperService _service;
        private ZooKeeperStatefulSet _statefulSet;
        private ZooKeeperStorageClass _storage;
        private ZooKeeperStorageClass _datalog;
        private Dictionary<string, object> _results;
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

        public bool Run(out Dictionary<string, object> results, string dryRun = default)
        {
            if(Values.ZooKeeper.Enabled)
            {
                _results = new Dictionary<string, object>();
                var conf = _config.Run(dryRun);
                _results.Add("ConfigMap", conf);

                var confG = _config.GenZkConf(dryRun);
                _results.Add("ConfigMapGen", confG);

                if (Values.ZooKeeper.UsePolicyPodDisruptionBudget)
                {
                    var pdb = _pdb.Run(dryRun);
                    _results.Add("Pdb", pdb);
                }

                var srv =_service.Run(dryRun);
                _results.Add("Service", srv);
                
                var state = _statefulSet.Run(dryRun);
                _results.Add("StatefulSet", state);
                
                if(Values.Persistence && Values.ZooKeeper.Persistence && !Values.LocalStorage)
                {
                    var store = _storage.Run(dryRun);
                    _results.Add("Storage", store);

                    //var log = _datalog.Run(dryRun);
                    //_results.Add("Log", log);

                }
                if (Values.Initialize && Values.Broker.Enabled)
                {
                    var init = _clusterInit.Run(dryRun);
                    _results.Add("ClusterInitializer", init);
                }
                results = _results;
                return true;
            }
            results = _results;
            return false;
        }
    }
}
