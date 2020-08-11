using SharpPulsar.Deployment.Kubernetes.Bookie.AutoRecovery;
using SharpPulsar.Deployment.Kubernetes.Bookie.Cluster;
using SharpPulsar.Deployment.Kubernetes.Bookie.Storage;
using System;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Bookie
{
    internal class BookieRunner
    {
        private readonly AutoRecoveryConfigMap _autoRecoveryConfigMap;
        private readonly AutoRecoveryService _autoRecoveryService;
        private readonly AutoRecoveryStatefulSet _autoRecoveryStatefulSet;

        private readonly BookieClusterRole _bookieClusterRole;
        private readonly BookieClusterRoleBinding _bookieClusterRoleBinding;
        private readonly ClusterInitialize _clusterInitialize;

        private readonly Journal _journal;
        private readonly Ledger _ledger;

        private readonly BookieConfigMap _bookieConfigMap;
        private readonly BookiePodDisruptionBudget _bookiePodDisruptionBudget;
        private readonly BookieService _bookieService;
        private readonly BookieServiceAccount _bookieServiceAccount;
        private readonly BookieStatefulSet _bookieStatefulSet;
        private readonly Dictionary<string, object> _results;
        public BookieRunner(Job job, ConfigMap configMap, PodDisruptionBudget pdb, Service service, ServiceAccount serviceAccount, StatefulSet statefulSet, ClusterRole clusterRole, ClusterRoleBinding clusterRoleBinding, StorageClass storageClass)
        {
            _results = new Dictionary<string, object>();
            if (job == null)
                throw new ArgumentNullException("Job is null");

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

            if (storageClass == null)
                throw new ArgumentNullException("StorageClass is null");
            _autoRecoveryConfigMap = new AutoRecoveryConfigMap(configMap);
            _autoRecoveryService = new AutoRecoveryService(service);
            _autoRecoveryStatefulSet = new AutoRecoveryStatefulSet(statefulSet);

            _bookieClusterRole = new BookieClusterRole(clusterRole);
            _bookieClusterRoleBinding = new BookieClusterRoleBinding(clusterRoleBinding);
            _bookieConfigMap = new BookieConfigMap(configMap);
            _bookiePodDisruptionBudget = new BookiePodDisruptionBudget(pdb);
            _bookieService = new BookieService(service);
            _bookieServiceAccount = new BookieServiceAccount(serviceAccount);
            _bookieStatefulSet = new BookieStatefulSet(statefulSet);
            _clusterInitialize = new ClusterInitialize(job);
            _journal = new Journal(storageClass);
            _ledger = new Ledger(storageClass);           
        }
        public bool Run(out Dictionary<string, object> results, string dryRun = default)
        {
            if (Values.Settings.Autorecovery.Enabled)
            {
                var autoConf = _autoRecoveryConfigMap.Run(dryRun);
                _results.Add("RecoveryConfigMap", autoConf);

                var reService = _autoRecoveryService.Run(dryRun);
                _results.Add("RecoveryService", reService);

                var reState = _autoRecoveryStatefulSet.Run(dryRun);
                _results.Add("RecoveryStatefulSet", reState);
            }
            if (Values.Settings.BookKeeper.Enabled)
            {
                if (Values.Initialize)
                {
                    var cinit = _clusterInitialize.Run(dryRun);
                    _results.Add("ClusterInitialize", cinit);
                }

                var crb = _bookieClusterRoleBinding.Run(dryRun);
                _results.Add("ClusterRoleBinding", crb);

                var cr = _bookieClusterRole.Run(dryRun);
                _results.Add("ClusterRole", cr);

                var conf = _bookieConfigMap.Run(dryRun);
                _results.Add("ConfigMap", conf);

                if (Values.Settings.BookKeeper.UsePolicyPodDisruptionBudget)
                {
                    var pdb = _bookiePodDisruptionBudget.Run(dryRun);
                    _results.Add("Pdb", pdb);
                }
                var srv = _bookieService.Run(dryRun);
                _results.Add("Service", srv);

                var srvAcct = _bookieServiceAccount.Run(dryRun);
                _results.Add("ServiceAccount", srvAcct);

                var state = _bookieStatefulSet.Run(dryRun);
                _results.Add("Statefulset", state);

                if (Values.Persistence && Values.Settings.BookKeeper.Persistence && !Values.LocalStorage)
                {
                    var journal = _journal.Run(dryRun);
                    _results.Add("JournalStorage", journal);
                    
                    var ledger = _ledger.Run(dryRun);
                    _results.Add("LedgerStorage", ledger);

                }
            }
            results = _results;
            return true;
        }
    }
}
