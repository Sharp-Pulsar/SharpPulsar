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
        public BookieRunner(Job job, ConfigMap configMap, PodDisruptionBudget pdb, Service service, ServiceAccount serviceAccount, StatefulSet statefulSet, ClusterRole clusterRole, ClusterRoleBinding clusterRoleBinding, StorageClass storageClass)
        {
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
        public IEnumerable<object> Run(string dryRun = default)
        {
            if (Values.Settings.Autorecovery.Enabled)
            {
                var autoConf = _autoRecoveryConfigMap.Run(dryRun);
                yield return autoConf;

                var reService = _autoRecoveryService.Run(dryRun);
                yield return reService;

                var reState = _autoRecoveryStatefulSet.Run(dryRun);
                yield return reState;
            }
            if (Values.Settings.BookKeeper.Enabled)
            {
                if (Values.Initialize)
                {
                    var cinit = _clusterInitialize.Run(dryRun);
                    yield return cinit;
                }

                var crb = _bookieClusterRoleBinding.Run(dryRun);
                yield return crb;

                var cr = _bookieClusterRole.Run(dryRun);
                yield return cr;

                var conf = _bookieConfigMap.Run(dryRun);
                yield return conf;

                if (Values.Settings.BookKeeper.UsePolicyPodDisruptionBudget)
                {
                    var pdb = _bookiePodDisruptionBudget.Run(dryRun);
                    yield return pdb;
                }
                var srv = _bookieService.Run(dryRun);
                yield return srv;

                var srvAcct = _bookieServiceAccount.Run(dryRun);
                yield return srvAcct;

                var state = _bookieStatefulSet.Run(dryRun);
                yield return state;

                if (Values.Persistence && Values.Settings.BookKeeper.Persistence && !Values.LocalStorage)
                {
                    var journal = _journal.Run(dryRun);
                    yield return journal;
                    
                    var ledger = _ledger.Run(dryRun);
                    yield return ledger;

                }
            }
        }
    }
}
