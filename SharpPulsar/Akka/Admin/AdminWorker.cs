using System;
using System.Collections.Generic;
using System.Net.Http;
using Akka.Actor;
using PulsarAdmin;
using PulsarAdmin.Models;
using SharpPulsar.Akka.Admin.Api.Models;

namespace SharpPulsar.Akka.Admin
{
    public class AdminWorker:ReceiveActor
    {
        private readonly PulsarAdminRESTAPI _adminRestapi;
        private readonly IActorRef _pulsarManager;
        public AdminWorker(string server, IActorRef pulsarManager)
        {
            _pulsarManager = pulsarManager;
            _adminRestapi = new PulsarAdminRESTAPI(server, new HttpClient(), true);
            //Receive((Action<InternalCommands.Admin>)this.Handle);
            Receive<InternalCommands.Admin>(Handle);
        }

        protected override void Unhandled(object message)
        {
            
        }

        private void Handle(InternalCommands.Admin admin)
        {
            try
            {
                object response = null;
                switch (admin.Command)
                {
                    case AdminCommands.GetBookiesRackInfo:
                        response = _adminRestapi.GetBookiesRackInfo();
                        break;
                    case AdminCommands.GetBookieRackInfo:
                        var bookie = admin.Arguments[0].ToString();
                        response = _adminRestapi.GetBookieRackInfo(bookie);
                        break;
                    case AdminCommands.UpdateBookieRackInfo:
                        _adminRestapi.UpdateBookieRackInfo(admin.Arguments[0].ToString(), admin.Arguments[1].ToString());
                        admin.Handler("UpdateBookieRackInfo");
                        break;
                    case AdminCommands.DeleteBookieRackInfo:
                        _adminRestapi.DeleteBookieRackInfo(admin.Arguments[0].ToString());
                        admin.Handler("DeleteBookieRackInfo");
                        break;
                    case AdminCommands.GetAllocatorStats:
                        var allocator = admin.Arguments[0].ToString();
                        response = _adminRestapi.GetAllocatorStats(allocator);
                        break;
                    case AdminCommands.GetPendingBookieOpsStats:
                        response = _adminRestapi.GetPendingBookieOpsStats();
                        break;
                    case AdminCommands.GetBrokerResourceAvailability:
                        response = _adminRestapi.GetBrokerResourceAvailability(admin.Arguments[0].ToString(), admin.Arguments[1].ToString());
                        break;
                    case AdminCommands.GetLoadReport:
                        response =_adminRestapi.GetLoadReport();
                        break;
                    case AdminCommands.GetMBeans:
                        response = _adminRestapi.GetMBeans();
                        break;
                    case AdminCommands.GetMetrics:
                        response = _adminRestapi.GetMetrics();
                        break;
                    case AdminCommands.GetTopics2:
                        response = _adminRestapi.GetTopics2();
                        break;
                    case AdminCommands.GetDynamicConfigurationName:
                        response = _adminRestapi.GetDynamicConfigurationName();
                        break;
                    case AdminCommands.GetRuntimeConfiguration:
                        response = _adminRestapi.GetRuntimeConfiguration();
                        break;
                    case AdminCommands.GetAllDynamicConfigurations:
                        response = _adminRestapi.GetAllDynamicConfigurations();
                        break;
                    case AdminCommands.DeleteDynamicConfiguration:
                        var config = admin.Arguments[0].ToString();
                        _adminRestapi.DeleteDynamicConfiguration(config);
                        admin.Handler("DeleteDynamicConfiguration");
                        break;
                    case AdminCommands.UpdateDynamicConfiguration:
                        var configN = admin.Arguments[0].ToString();
                        var configV = admin.Arguments[0].ToString();
                        _adminRestapi.UpdateDynamicConfiguration(configN, configV);
                        admin.Handler("UpdateDynamicConfiguration");
                        break;
                    case AdminCommands.Healthcheck:
                        _adminRestapi.Healthcheck();
                        admin.Handler("Healthcheck");
                        break;
                    case AdminCommands.GetInternalConfigurationData:
                        response = _adminRestapi.GetInternalConfigurationData();
                        break;
                    case AdminCommands.GetOwnedNamespaces:
                        var cluster = admin.Arguments[0].ToString();
                        var service = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetOwnedNamespaces(cluster, service);
                        break;
                    case AdminCommands.GetActiveBrokers:
                        var clustr = admin.Arguments[0].ToString();
                        response = _adminRestapi.GetActiveBrokers(clustr);
                        break;
                    case AdminCommands.GetCluster:
                        var clust = admin.Arguments[0].ToString();
                        response = _adminRestapi.GetCluster(clust);
                        break;
                    case AdminCommands.GetClusters:
                        response = _adminRestapi.GetClusters();
                        break;
                    case AdminCommands.UpdateCluster:
                        var clter = admin.Arguments[0].ToString();
                        var clusterData = (ClusterData) admin.Arguments[1];
                        _adminRestapi.UpdateCluster(clter, clusterData);
                        admin.Handler("UpdateCluster");
                        break;
                    case AdminCommands.CreateCluster:
                        var clt = admin.Arguments[0].ToString();
                        var clustData = (ClusterData) admin.Arguments[1];
                        _adminRestapi.CreateCluster(clt, clustData);
                        admin.Handler("CreateCluster");
                        break;
                    case AdminCommands.DeleteCluster:
                        var cl = admin.Arguments[0].ToString();
                        _adminRestapi.DeleteCluster(cl);
                        admin.Handler("DeleteCluster");
                        break;
                    case AdminCommands.GetFailureDomains:
                        var clr = admin.Arguments[0].ToString();
                        response = _adminRestapi.GetFailureDomains(clr);
                        break;
                    case AdminCommands.GetDomain:
                        var cltr = admin.Arguments[0].ToString();
                        var domain = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetDomain(cltr, domain);
                        break;
                    case AdminCommands.SetFailureDomain:
                        var dcltr = admin.Arguments[0].ToString();
                        var domainN = admin.Arguments[1].ToString();
                        var body = (FailureDomain) admin.Arguments[2];
                        _adminRestapi.SetFailureDomain(dcltr, domainN, body);
                        admin.Handler("SetFailureDomain");
                        break;
                    case AdminCommands.DeleteFailureDomain:
                        var decltr = admin.Arguments[0].ToString();
                        var ddomainN = admin.Arguments[1].ToString();
                        _adminRestapi.DeleteFailureDomain(decltr, ddomainN);
                        admin.Handler("DeleteFailureDomain");
                        break;
                    case AdminCommands.GetNamespaceIsolationPolicies:
                        var decltrN = admin.Arguments[0].ToString();
                        response = _adminRestapi.GetNamespaceIsolationPolicies(decltrN);
                        break;
                    case AdminCommands.GetBrokersWithNamespaceIsolationPolicy:
                        var cname = admin.Arguments[0].ToString();
                        response = _adminRestapi.GetBrokersWithNamespaceIsolationPolicy(cname);
                        break;
                    case AdminCommands.GetBrokerWithNamespaceIsolationPolicy:
                        var clname = admin.Arguments[0].ToString();
                        var broker = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetBrokerWithNamespaceIsolationPolicy(clname, broker);
                        break;
                    case AdminCommands.GetNamespaceIsolationPolicy:
                        var cluname = admin.Arguments[0].ToString();
                        var policy = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetNamespaceIsolationPolicy(cluname, policy);
                        break;
                    case AdminCommands.SetNamespaceIsolationPolicy:
                        var clusname = admin.Arguments[0].ToString();
                        var policyName = admin.Arguments[1].ToString();
                        var nbody = (NamespaceIsolationData) admin.Arguments[2];
                        _adminRestapi.SetNamespaceIsolationPolicy(clusname, policyName, nbody);
                        admin.Handler("SetNamespaceIsolationPolicy");
                        break;
                    case AdminCommands.DeleteNamespaceIsolationPolicy:
                        var clustname = admin.Arguments[0].ToString();
                        var poName = admin.Arguments[1].ToString();
                        _adminRestapi.DeleteNamespaceIsolationPolicy(clustname, poName);
                        admin.Handler("DeleteNamespaceIsolationPolicy");
                        break;
                    case AdminCommands.GetPeerCluster:
                        var clustename = admin.Arguments[0].ToString();
                        response = _adminRestapi.GetPeerCluster(clustename);
                        break;
                    case AdminCommands.SetPeerClusterNames:
                        var cName = admin.Arguments[0].ToString();
                        var cBody = (IList<string>) admin.Arguments[1];
                        _adminRestapi.SetPeerClusterNames(cName, cBody);
                        admin.Handler("SetPeerClusterNames");
                        break;
                    case AdminCommands.GetAntiAffinityNamespaces:
                        var c_Name = admin.Arguments[0].ToString();
                        var c_Group = admin.Arguments[1].ToString();
                        var t = admin.Arguments[2].ToString();
                        response = _adminRestapi.GetAntiAffinityNamespaces(c_Name, c_Group, t);
                        break;
                    case AdminCommands.GetBookieAffinityGroup:
                        var property = admin.Arguments[0].ToString();
                        var nSpace = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetBookieAffinityGroup(property, nSpace);
                        break;
                    case AdminCommands.DeleteBookieAffinityGroup:
                        var propy = admin.Arguments[0].ToString();
                        var naSpace = admin.Arguments[1].ToString();
                        _adminRestapi.DeleteBookieAffinityGroup(propy, naSpace);
                        admin.Handler("DeleteBookieAffinityGroup");
                        break;
                    case AdminCommands.GetTenantNamespaces:
                        var te = admin.Arguments[0].ToString();
                        response = _adminRestapi.GetTenantNamespaces(te);
                        break;
                    case AdminCommands.GetPolicies:
                        var ten = admin.Arguments[0].ToString();
                        var ns = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetPolicies(ten, ns);
                        break;
                    case AdminCommands.CreateNamespace:
                        var tena = admin.Arguments[0].ToString();
                        var nsp = admin.Arguments[1].ToString();
                        var poly = (Policies) admin.Arguments[2];
                        _adminRestapi.CreateNamespace(tena, nsp, poly);
                        admin.Handler("CreateNamespace");
                        break;
                    case AdminCommands.DeleteNamespace:
                        var tenat = admin.Arguments[0].ToString();
                        var nspa = admin.Arguments[1].ToString();
                        var auth = (bool)admin.Arguments[2];
                        _adminRestapi.DeleteNamespace(tenat, nspa, auth);
                        admin.Handler("DeleteNamespace");
                        break;
                    case AdminCommands.GetNamespaceAntiAffinityGroup:
                        var tenaNt = admin.Arguments[0].ToString();
                        var nspac = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetNamespaceAntiAffinityGroup(tenaNt, nspac);
                        break;
                    case AdminCommands.SetNamespaceAntiAffinityGroup:
                        var _tenaNt = admin.Arguments[0].ToString();
                        var _nspac = admin.Arguments[1].ToString();
                        var aff = admin.Arguments[2].ToString();
                        _adminRestapi.SetNamespaceAntiAffinityGroup(_tenaNt, _nspac, aff);
                        admin.Handler("SetNamespaceAntiAffinityGroup");
                        break;
                    case AdminCommands.RemoveNamespaceAntiAffinityGroup:
                        var _tenaNt_ = admin.Arguments[0].ToString();
                        var _nspac_ = admin.Arguments[1].ToString();
                        _adminRestapi.RemoveNamespaceAntiAffinityGroup(_tenaNt_, _nspac_);
                        admin.Handler("RemoveNamespaceAntiAffinityGroup");
                        break;
                    case AdminCommands.SetBacklogQuota:
                        var _tenant = admin.Arguments[0].ToString();
                        var _nspace = admin.Arguments[1].ToString();
                        var _type = admin.Arguments[2].ToString();
                        _adminRestapi.SetBacklogQuota(_tenant, _nspace, _type);
                        admin.Handler("SetBacklogQuota");
                        break;
                    case AdminCommands.RemoveBacklogQuota:
                        var _tenant1 = admin.Arguments[0].ToString();
                        var _nspace1 = admin.Arguments[1].ToString();
                        var _type1 = admin.Arguments[2].ToString();
                        _adminRestapi.RemoveBacklogQuota(_tenant1, _nspace1, _type1);
                        admin.Handler("RemoveBacklogQuota");
                        break;
                    case AdminCommands.GetBacklogQuotaMap:
                        var tenant2 = admin.Arguments[0].ToString();
                        var nspace2 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetBacklogQuotaMap(tenant2, nspace2);
                        break;
                    case AdminCommands.GetBundlesData:
                        var tenant3 = admin.Arguments[0].ToString();
                        var nspace3 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetBundlesData(tenant3, nspace3);
                        break;
                    case AdminCommands.ClearNamespaceBacklog:
                        var tenant4 = admin.Arguments[0].ToString();
                        var nspace4 = admin.Arguments[1].ToString();
                        var auth2 = (bool)admin.Arguments[2];
                        _adminRestapi.ClearNamespaceBacklog(tenant4, nspace4, auth2);
                        admin.Handler("ClearNamespaceBacklog");
                        break;
                    case AdminCommands.ClearNamespaceBacklogForSubscription:
                        var tenant5 = admin.Arguments[0].ToString();
                        var nspace5 = admin.Arguments[1].ToString();
                        var sub = admin.Arguments[2].ToString();
                        var auth3 = (bool)admin.Arguments[3];
                        _adminRestapi.ClearNamespaceBacklogForSubscription(tenant5, nspace5, sub, auth3);
                        admin.Handler("ClearNamespaceBacklogForSubscription");
                        break;
                    case AdminCommands.SetCompactionThreshold:
                        var tenant6 = admin.Arguments[0].ToString();
                        var nspace6 = admin.Arguments[1].ToString();
                        var thresh = (long)admin.Arguments[2];
                        _adminRestapi.SetCompactionThreshold(tenant6, nspace6, thresh);
                        admin.Handler("SetCompactionThreshold");
                        break;
                    case AdminCommands.ModifyDeduplication:
                        var tenant8 = admin.Arguments[0].ToString();
                        var nspace8 = admin.Arguments[1].ToString();
                        var enable = (bool) admin.Arguments[2];
                        _adminRestapi.ModifyDeduplication(tenant8, nspace8, enable);
                        admin.Handler("ModifyDeduplication");
                        break;
                    case AdminCommands.GetCompactionThreshold:
                        var tenant7 = admin.Arguments[0].ToString();
                        var nspace7 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetCompactionThreshold(tenant7, nspace7);
                        break;
                    case AdminCommands.GetDispatchRate:
                        var tenant9 = admin.Arguments[0].ToString();
                        var nspace9 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetDispatchRate(tenant9, nspace9);
                        break;
                    case AdminCommands.SetDispatchRate:
                        var tenant10 = admin.Arguments[0].ToString();
                        var nspace10 = admin.Arguments[1].ToString();
                        var rate = (DispatchRate)admin.Arguments[2];
                        _adminRestapi.SetDispatchRate(tenant10, nspace10, rate);
                        admin.Handler("SetDispatchRate");
                        break;
                    case AdminCommands.ModifyEncryptionRequired:
                        var tenant11 = admin.Arguments[0].ToString();
                        var nspace11 = admin.Arguments[1].ToString();
                        var encrpt = (bool)admin.Arguments[2];
                        _adminRestapi.ModifyEncryptionRequired(tenant11, nspace11, encrpt);
                        admin.Handler("ModifyEncryptionRequired");
                        break;
                    case AdminCommands.GetIsAllowAutoUpdateSchema:
                        var tenant12 = admin.Arguments[0].ToString();
                        var nspace12 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetIsAllowAutoUpdateSchema(tenant12, nspace12);
                        break;
                    case AdminCommands.SetIsAllowAutoUpdateSchema:
                        var tenant13 = admin.Arguments[0].ToString();
                        var nspace13 = admin.Arguments[1].ToString();
                        var allow = (bool)admin.Arguments[2];
                        _adminRestapi.SetIsAllowAutoUpdateSchema(tenant13, nspace13, allow);
                        admin.Handler("SetIsAllowAutoUpdateSchema");
                        break;
                    case AdminCommands.SetMaxConsumersPerSubscription:
                        var tenant15 = admin.Arguments[0].ToString();
                        var nspace15 = admin.Arguments[1].ToString();
                        var maxCS = (int) admin.Arguments[2];
                        _adminRestapi.SetMaxConsumersPerSubscription(tenant15, nspace15, maxCS);
                        admin.Handler("SetMaxConsumersPerSubscription");
                        break;
                    case AdminCommands.GetMaxConsumersPerSubscription:
                        var tenant14 = admin.Arguments[0].ToString();
                        var nspace14 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetMaxConsumersPerSubscription(tenant14, nspace14);
                        break;
                    case AdminCommands.GetMaxConsumersPerTopic:
                        var tenant16 = admin.Arguments[0].ToString();
                        var nspace16 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetMaxConsumersPerTopic(tenant16, nspace16);
                        break;
                    case AdminCommands.SetMaxConsumersPerTopic: 
                        var tenant17 = admin.Arguments[0].ToString();
                        var nspace17 = admin.Arguments[1].ToString();
                        var maxCT = (int) admin.Arguments[2];
                        _adminRestapi.SetMaxConsumersPerTopic(tenant17, nspace17, maxCT);
                        admin.Handler("SetMaxConsumersPerTopic");
                        break;
                    case AdminCommands.GetMaxProducersPerTopic:
                        var tenant18 = admin.Arguments[0].ToString();
                        var nspace18 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetMaxProducersPerTopic(tenant18, nspace18);
                        break;
                    case AdminCommands.SetMaxProducersPerTopic:
                        var tenant19 = admin.Arguments[0].ToString();
                        var nspace19 = admin.Arguments[1].ToString();
                        var maxPT = (int)admin.Arguments[2];
                        _adminRestapi.SetMaxProducersPerTopic(tenant19, nspace19, maxPT);
                        admin.Handler("SetMaxProducersPerTopic");
                        break;
                    case AdminCommands.GetNamespaceMessageTTL:
                        var tenant20 = admin.Arguments[0].ToString();
                        var nspace20 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetNamespaceMessageTTL(tenant20, nspace20);
                        break;
                    case AdminCommands.SetNamespaceMessageTTL:
                        var tenant21 = admin.Arguments[0].ToString();
                        var nspace21 = admin.Arguments[1].ToString();
                        var msgTTL = (int)admin.Arguments[2];
                        _adminRestapi.SetNamespaceMessageTTL(tenant21, nspace21, msgTTL);
                        admin.Handler("SetNamespaceMessageTTL");
                        break;
                    case AdminCommands.GetOffloadDeletionLag:
                        var tenant22 = admin.Arguments[0].ToString();
                        var nspace22 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetOffloadDeletionLag(tenant22, nspace22);
                        break;
                    case AdminCommands.SetOffloadDeletionLag:
                        var tenant23 = admin.Arguments[0].ToString();
                        var nspace23 = admin.Arguments[1].ToString();
                        var off = (long)admin.Arguments[2];
                        _adminRestapi.SetOffloadDeletionLag(tenant23, nspace23, off);
                        admin.Handler("SetOffloadDeletionLag");
                        break;
                    case AdminCommands.ClearOffloadDeletionLag:
                        var tenant24 = admin.Arguments[0].ToString();
                        var nspace24 = admin.Arguments[1].ToString();
                        _adminRestapi.ClearOffloadDeletionLag(tenant24, nspace24);
                        admin.Handler("ClearOffloadDeletionLag");
                        break;
                    case AdminCommands.GetOffloadThreshold:
                        var tenant25 = admin.Arguments[0].ToString();
                        var nspace25 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetOffloadThreshold(tenant25, nspace25);
                        break;
                    case AdminCommands.SetOffloadThreshold:
                        var tenant26 = admin.Arguments[0].ToString();
                        var nspace26 = admin.Arguments[1].ToString();
                        var offload = (long)admin.Arguments[2];
                        _adminRestapi.SetOffloadThreshold(tenant26, nspace26, offload);
                        admin.Handler("SetOffloadThreshold");
                        break;
                    case AdminCommands.GetPermissions:
                        var tenant27 = admin.Arguments[0].ToString();
                        var cluster1 = admin.Arguments[1].ToString();
                        var nspace27 = admin.Arguments[2].ToString();
                        response = _adminRestapi.GetPermissions(tenant27, cluster1, nspace27);
                        break;
                    case AdminCommands.GrantPermissionOnNamespace:
                        var tenant28 = admin.Arguments[0].ToString();
                        var nspace28 = admin.Arguments[1].ToString();
                        var role = admin.Arguments[2].ToString();
                        _adminRestapi.GrantPermissionOnNamespace(tenant28, nspace28, role);
                        admin.Handler("GrantPermissionOnNamespace");
                        break;
                    case AdminCommands.RevokePermissionsOnNamespace:
                        var tenant29 = admin.Arguments[0].ToString();
                        var nspace29 = admin.Arguments[1].ToString();
                        var role1 = admin.Arguments[2].ToString();
                        _adminRestapi.RevokePermissionsOnNamespace(tenant29, nspace29, role1);
                        admin.Handler("RevokePermissionsOnNamespace");
                        break;
                    case AdminCommands.GetPersistence:
                        var tenant30 = admin.Arguments[0].ToString();
                        var nspace30 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetPersistence(tenant30, nspace30);
                        break;
                    case AdminCommands.SetPersistence:
                        var tenant31 = admin.Arguments[0].ToString();
                        var nspace31 = admin.Arguments[1].ToString();
                        var persistence = (PersistencePolicies)admin.Arguments[2];
                        _adminRestapi.SetPersistence(tenant31, nspace31, persistence);
                        admin.Handler("SetPersistence");
                        break;
                    case AdminCommands.SetBookieAffinityGroup:
                        var tenant32 = admin.Arguments[0].ToString();
                        var nspace32 = admin.Arguments[1].ToString();
                        var affinity = (BookieAffinityGroupData)admin.Arguments[2];
                        _adminRestapi.SetBookieAffinityGroup(tenant32, nspace32, affinity);
                        admin.Handler("SetBookieAffinityGroup");
                        break;
                    case AdminCommands.GetNamespaceReplicationClusters:
                        var tenant33 = admin.Arguments[0].ToString();
                        var nspace33 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetNamespaceReplicationClusters(tenant33, nspace33);
                        break;
                    case AdminCommands.SetNamespaceReplicationClusters:
                        var tenant34 = admin.Arguments[0].ToString();
                        var nspace34 = admin.Arguments[1].ToString();
                        var ids = (List<string>)admin.Arguments[2];
                        _adminRestapi.SetNamespaceReplicationClusters(tenant34, nspace34, ids);
                        admin.Handler("SetNamespaceReplicationClusters");
                        break;
                    case AdminCommands.GetReplicatorDispatchRate:
                        var tenant35 = admin.Arguments[0].ToString();
                        var nspace35 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetReplicatorDispatchRate(tenant35, nspace35);
                        break;
                    case AdminCommands.SetReplicatorDispatchRate:
                        var tenant36 = admin.Arguments[0].ToString();
                        var nspace36 = admin.Arguments[1].ToString();
                        var dRate = (DispatchRate)admin.Arguments[2];
                        _adminRestapi.SetReplicatorDispatchRate(tenant36, nspace36, dRate);
                        admin.Handler("SetReplicatorDispatchRate");
                        break;
                    case AdminCommands.GetRetention:
                        var tenant37 = admin.Arguments[0].ToString();
                        var nspace37 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetRetention(tenant37, nspace37);
                        break;
                    case AdminCommands.SetRetention:
                        var tenant38 = admin.Arguments[0].ToString();
                        var nspace38 = admin.Arguments[1].ToString();
                        var retention = (RetentionPolicies)admin.Arguments[2];
                        _adminRestapi.SetRetention(tenant38, nspace38, retention);
                        admin.Handler("SetRetention");
                        break;
                    case AdminCommands.GetSchemaAutoUpdateCompatibilityStrategy:
                        var tenant39 = admin.Arguments[0].ToString();
                        var nspace39 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetSchemaAutoUpdateCompatibilityStrategy(tenant39, nspace39);
                        break;
                    case AdminCommands.SetSchemaAutoUpdateCompatibilityStrategy:
                        var tenant40 = admin.Arguments[0].ToString();
                        var nspace40 = admin.Arguments[1].ToString();
                        var strat = (SchemaAutoUpdateCompatibilityStrategy)admin.Arguments[2];
                        _adminRestapi.SetSchemaAutoUpdateCompatibilityStrategy(tenant40, nspace40, strat);
                        admin.Handler("SetSchemaAutoUpdateCompatibilityStrategy");
                        break;
                    case AdminCommands.GetSchemaCompatibilityStrategy:
                        var tenant41 = admin.Arguments[0].ToString();
                        var nspace41 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetSchemaCompatibilityStrategy(tenant41, nspace41);
                        break;
                    case AdminCommands.SetSchemaCompatibilityStrategy:
                        var tenant42 = admin.Arguments[0].ToString();
                        var nspace42 = admin.Arguments[1].ToString();
                        var compact = (SchemaCompatibilityStrategy)admin.Arguments[2];
                        _adminRestapi.SetSchemaCompatibilityStrategy(tenant42, nspace42, compact);
                        admin.Handler("SetSchemaCompatibilityStrategy");
                        break;
                    case AdminCommands.GetSchemaValidtionEnforced:
                        var tenant43 = admin.Arguments[0].ToString();
                        var nspace43 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetSchemaValidtionEnforced(tenant43, nspace43);
                        break;
                    case AdminCommands.SetSchemaValidtionEnforced:
                        var tenant44 = admin.Arguments[0].ToString();
                        var nspace44 = admin.Arguments[1].ToString();
                        var enfo = (bool)admin.Arguments[2];
                        _adminRestapi.SetSchemaValidtionEnforced(tenant44, nspace44, enfo);
                        admin.Handler("SetSchemaValidtionEnforced");
                        break;
                    case AdminCommands.GetSubscribeRate:
                        var tenant45 = admin.Arguments[0].ToString();
                        var nspace45 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetSubscribeRate(tenant45, nspace45);
                        break;
                    case AdminCommands.SetSubscribeRate:
                        var tenant46 = admin.Arguments[0].ToString();
                        var nspace46 = admin.Arguments[1].ToString();
                        var sRate = (SubscribeRate)admin.Arguments[2];
                        _adminRestapi.SetSubscribeRate(tenant46, nspace46, sRate);
                        admin.Handler("SetSubscribeRate");
                        break;
                    case AdminCommands.SetSubscriptionAuthMode:
                        var tenant47 = admin.Arguments[0].ToString();
                        var nspace47 = admin.Arguments[1].ToString();
                        var authMode = (SubscriptionAuthMode)admin.Arguments[2];
                        _adminRestapi.SetSubscriptionAuthMode(tenant47, nspace47, authMode);
                        admin.Handler("SetSubscriptionAuthMode");
                        break;
                    case AdminCommands.GetSubscriptionDispatchRate:
                        var tenant48 = admin.Arguments[0].ToString();
                        var nspace48 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetSubscriptionDispatchRate(tenant48, nspace48);
                        break;
                    case AdminCommands.SetSubscriptionDispatchRate:
                        var tenant49 = admin.Arguments[0].ToString();
                        var nspace49 = admin.Arguments[1].ToString();
                        var sbRate = (DispatchRate)admin.Arguments[2];
                        _adminRestapi.SetSubscriptionDispatchRate(tenant49, nspace49, sbRate);
                        admin.Handler("SetSubscriptionDispatchRate");
                        break;
                    case AdminCommands.GetTopics:
                        var tenant50 = admin.Arguments[0].ToString();
                        var nspace50 = admin.Arguments[1].ToString();
                        var mode = admin.Arguments[2].ToString();
                        response = _adminRestapi.GetTopics(tenant50, nspace50, mode);
                        break;
                    case AdminCommands.UnloadNamespace:
                        var tenant51 = admin.Arguments[0].ToString();
                        var nspace51 = admin.Arguments[1].ToString();
                        _adminRestapi.UnloadNamespace(tenant51, nspace51);
                        admin.Handler("UnloadNamespace");
                        break;
                    case AdminCommands.UnsubscribeNamespace:
                        var tenant52 = admin.Arguments[0].ToString();
                        var cluster3 = admin.Arguments[1].ToString();
                        var nspace52 = admin.Arguments[2].ToString();
                        var sub2 = admin.Arguments[3].ToString();
                        var auth4 = (bool)admin.Arguments[4];
                        _adminRestapi.UnsubscribeNamespace(tenant52, cluster3, nspace52, sub2, auth4);
                        admin.Handler("UnsubscribeNamespace");
                        break;
                    case AdminCommands.DeleteNamespaceBundle:
                        var tenant53 = admin.Arguments[0].ToString();
                        var nspace53 = admin.Arguments[1].ToString();
                        var bndle = admin.Arguments[2].ToString();
                        var auth5 = (bool)admin.Arguments[3];
                        _adminRestapi.DeleteNamespaceBundle(tenant53, nspace53, bndle, auth5);
                        admin.Handler("DeleteNamespaceBundle");
                        break;
                    case AdminCommands.ClearNamespaceBundleBacklog:
                        var tenant54 = admin.Arguments[0].ToString();
                        var nspace54 = admin.Arguments[1].ToString();
                        var bndle1 = admin.Arguments[2].ToString();
                        var auth6 = (bool)admin.Arguments[3];
                        _adminRestapi.ClearNamespaceBundleBacklog(tenant54, nspace54, bndle1, auth6);
                        admin.Handler("ClearNamespaceBundleBacklog");
                        break;
                    case AdminCommands.ClearNamespaceBundleBacklogForSubscription:
                        var tenant55 = admin.Arguments[0].ToString();
                        var nspace55 = admin.Arguments[1].ToString();
                        var sub3 = admin.Arguments[2].ToString();
                        var bndle2 = admin.Arguments[3].ToString();
                        var auth7 = (bool)admin.Arguments[4];
                        _adminRestapi.ClearNamespaceBundleBacklogForSubscription(tenant55, nspace55, sub3, bndle2, auth7);
                        admin.Handler("ClearNamespaceBundleBacklogForSubscription");
                        break;
                    case AdminCommands.SplitNamespaceBundle:
                        var tenant56 = admin.Arguments[0].ToString();
                        var nspace56 = admin.Arguments[1].ToString();
                        var bndle4 = admin.Arguments[2].ToString();
                        var auth8 = (bool)admin.Arguments[3];
                        var unload = (bool)admin.Arguments[4];
                        var algo = admin.Arguments[5].ToString();
                        _adminRestapi.SplitNamespaceBundle(tenant56, nspace56, bndle4, algo, auth8, unload);
                        admin.Handler("SplitNamespaceBundle");
                        break;
                    case AdminCommands.UnloadNamespaceBundle:
                        var tenant57 = admin.Arguments[0].ToString();
                        var nspace57 = admin.Arguments[1].ToString();
                        var bndle5 = admin.Arguments[2].ToString();
                        var auth9 = (bool)admin.Arguments[3];
                        _adminRestapi.UnloadNamespaceBundle(tenant57, nspace57, bndle5, auth9);
                        admin.Handler("UnloadNamespaceBundle");
                        break;
                    case AdminCommands.UnsubscribeNamespaceBundle:
                        var tenant58 = admin.Arguments[0].ToString();
                        var nspace58 = admin.Arguments[1].ToString();
                        var sub4 = admin.Arguments[2].ToString();
                        var bndle6 = admin.Arguments[3].ToString();
                        var auth10 = (bool)admin.Arguments[4];
                        _adminRestapi.UnsubscribeNamespaceBundle(tenant58, nspace58,sub4, bndle6, auth10);
                        admin.Handler("UnsubscribeNamespaceBundle");
                        break;
                    case AdminCommands.GetList:
                        var tenant59 = admin.Arguments[0].ToString();
                        var nspace59 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetList(tenant59, nspace59);
                        break;
                    case AdminCommands.GetListPersistence:
                        var tenant59_ = admin.Arguments[0].ToString();
                        var nspace59_ = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetList1(tenant59_, nspace59_);
                        break;
                    case AdminCommands.GetPartitionedTopicList:
                        var tenant60 = admin.Arguments[0].ToString();
                        var nspace60 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetPartitionedTopicList(tenant60, nspace60);
                        break;
                    case AdminCommands.GetPartitionedTopicListPersistence:
                        var tenant60_ = admin.Arguments[0].ToString();
                        var nspace60_ = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetPartitionedTopicList1(tenant60_, nspace60_);
                        break;
                    case AdminCommands.GetListFromBundle:
                        var tenant61 = admin.Arguments[0].ToString();
                        var nspace61 = admin.Arguments[1].ToString();
                        var bndle7 = admin.Arguments[2].ToString();
                        response = _adminRestapi.GetListFromBundle(tenant61, nspace61, bndle7);
                        break;
                    case AdminCommands.CreateNonPartitionedTopic:
                        var tenant62 = admin.Arguments[0].ToString();
                        var nspace62 = admin.Arguments[1].ToString();
                        var topic = admin.Arguments[2].ToString();
                        var auth11 = (bool)admin.Arguments[3];
                        _adminRestapi.CreateNonPartitionedTopic(tenant62, nspace62, topic, auth11);
                        admin.Handler("CreateNonPartitionedTopic");
                        break;
                    case AdminCommands.CreateNonPartitionedPersistentTopic:
                        var tenant62p = admin.Arguments[0].ToString();
                        var nspace62p = admin.Arguments[1].ToString();
                        var topicp = admin.Arguments[2].ToString();
                        var auth11p = (bool)admin.Arguments[3];
                        _adminRestapi.CreateNonPartitionedTopic1(tenant62p, nspace62p, topicp, auth11p);
                        admin.Handler("CreateNonPartitionedPersistentTopic");
                        break;
                    case AdminCommands.DeleteTopic:
                        var tenant63 = admin.Arguments[0].ToString();
                        var nspace63 = admin.Arguments[1].ToString();
                        var topic1 = admin.Arguments[2].ToString();
                        var force = (bool)admin.Arguments[3];
                        var auth12 = (bool)admin.Arguments[4];
                        _adminRestapi.DeleteTopic(tenant63, nspace63, topic1, force, auth12);
                        admin.Handler("DeleteTopic");
                        break;
                    case AdminCommands.DeletePersistentTopic:
                        var tenant63p = admin.Arguments[0].ToString();
                        var nspace63p = admin.Arguments[1].ToString();
                        var topic1p = admin.Arguments[2].ToString();
                        var forcep = (bool)admin.Arguments[3];
                        var auth12p = (bool)admin.Arguments[4];
                        _adminRestapi.DeleteTopic1(tenant63p, nspace63p, topic1p, forcep, auth12p);
                        admin.Handler("DeletePersistentTopic");
                        break;
                    case AdminCommands.ExpireMessagesForAllSubscriptions:
                        var tenant64 = admin.Arguments[0].ToString();
                        var nspace64 = admin.Arguments[1].ToString();
                        var topic2 = admin.Arguments[2].ToString();
                        var expires = (int)admin.Arguments[3];
                        var auth13 = (bool)admin.Arguments[4];
                        _adminRestapi.ExpireMessagesForAllSubscriptions(tenant64, nspace64, topic2, expires, auth13);
                        admin.Handler("ExpireMessagesForAllSubscriptions");
                        break;
                    case AdminCommands.ExpireMessagesForAllSubscriptionsPersistent:
                        var tenant64p = admin.Arguments[0].ToString();
                        var nspace64p = admin.Arguments[1].ToString();
                        var topic2p = admin.Arguments[2].ToString();
                        var expiresp = (int)admin.Arguments[3];
                        var auth13p = (bool)admin.Arguments[4];
                        _adminRestapi.ExpireMessagesForAllSubscriptions1(tenant64p, nspace64p, topic2p, expiresp, auth13p);
                        admin.Handler("ExpireMessagesForAllSubscriptionsPersistent");
                        break;
                    case AdminCommands.GetBacklog:
                        var tenant65 = admin.Arguments[0].ToString();
                        var nspace65 = admin.Arguments[1].ToString();
                        var topic3 = admin.Arguments[2].ToString();
                        var auth14 = (bool)admin.Arguments[3];
                        response = _adminRestapi.GetBacklog(tenant65, nspace65, topic3, auth14);
                        break;
                    case AdminCommands.GetBacklogPersistent:
                        var tenant65p = admin.Arguments[0].ToString();
                        var nspace65p = admin.Arguments[1].ToString();
                        var topic3p = admin.Arguments[2].ToString();
                        var auth14p = (bool)admin.Arguments[3];
                        response = _adminRestapi.GetBacklog1(tenant65p, nspace65p, topic3p, auth14p);
                        break;
                    case AdminCommands.CompactionStatus:
                        var tenant66 = admin.Arguments[0].ToString();
                        var nspace66 = admin.Arguments[1].ToString();
                        var topic4 = admin.Arguments[2].ToString();
                        var auth15 = (bool)admin.Arguments[3];
                        response = _adminRestapi.CompactionStatus(tenant66, nspace66, topic4, auth15);
                        break;
                    case AdminCommands.CompactionStatusPersistent:
                        var tenant66p = admin.Arguments[0].ToString();
                        var nspace66p = admin.Arguments[1].ToString();
                        var topic4p = admin.Arguments[2].ToString();
                        var auth15p = (bool)admin.Arguments[3];
                        response = _adminRestapi.CompactionStatus1(tenant66p, nspace66p, topic4p, auth15p);
                        break;
                    case AdminCommands.Compact:
                        var tenant67 = admin.Arguments[0].ToString();
                        var nspace67 = admin.Arguments[1].ToString();
                        var topic5 = admin.Arguments[2].ToString();
                        var auth16 = (bool)admin.Arguments[3];
                        _adminRestapi.Compact(tenant67, nspace67, topic5, auth16);
                        admin.Handler("Compact");
                        break;
                    case AdminCommands.CompactPersistent:
                        var tenant67p = admin.Arguments[0].ToString();
                        var nspace67p = admin.Arguments[1].ToString();
                        var topic5p = admin.Arguments[2].ToString();
                        var auth16p = (bool)admin.Arguments[3];
                        _adminRestapi.Compact1(tenant67p, nspace67p, topic5p, auth16p);
                        admin.Handler("CompactPersistent");
                        break;
                    case AdminCommands.GetManagedLedgerInfo:
                        var tenant68 = admin.Arguments[0].ToString();
                        var nspace68 = admin.Arguments[1].ToString();
                        var topic6 = admin.Arguments[2].ToString();
                        _adminRestapi.GetManagedLedgerInfo(tenant68, nspace68, topic6);
                        admin.Handler("GetManagedLedgerInfo");
                        break;
                    case AdminCommands.GetManagedLedgerInfoPersistent:
                        var tenant68p = admin.Arguments[0].ToString();
                        var nspace68p = admin.Arguments[1].ToString();
                        var topic6p = admin.Arguments[2].ToString();
                        _adminRestapi.GetManagedLedgerInfo1(tenant68p, nspace68p, topic6p);
                        admin.Handler("GetManagedLedgerInfoPersistent");
                        break;
                    case AdminCommands.GetInternalStats:
                        var tenant69 = admin.Arguments[0].ToString();
                        var nspace69 = admin.Arguments[1].ToString();
                        var topic7 = admin.Arguments[2].ToString();
                        var auth17 = (bool)admin.Arguments[3];
                        response = _adminRestapi.GetInternalStats(tenant69, nspace69, topic7, auth17);
                        break;
                    case AdminCommands.GetInternalStatsPersistent:
                        var tenant69p = admin.Arguments[0].ToString();
                        var nspace69p = admin.Arguments[1].ToString();
                        var topic7p = admin.Arguments[2].ToString();
                        var auth17p = (bool)admin.Arguments[3];
                        response = _adminRestapi.GetInternalStats1(tenant69p, nspace69p, topic7p, auth17p);
                        break;
                    case AdminCommands.GetLastMessageId:
                        var tenant70 = admin.Arguments[0].ToString();
                        var nspace70 = admin.Arguments[1].ToString();
                        var topic8 = admin.Arguments[2].ToString();
                        var auth18 = (bool)admin.Arguments[3];
                        response = _adminRestapi.GetLastMessageId(tenant70, nspace70, topic8, auth18);
                        break;
                    case AdminCommands.GetLastMessageIdPersistent:
                        var tenant70p = admin.Arguments[0].ToString();
                        var nspace70p = admin.Arguments[1].ToString();
                        var topic8p = admin.Arguments[2].ToString();
                        var auth18p = (bool)admin.Arguments[3];
                        response = _adminRestapi.GetLastMessageId1(tenant70p, nspace70p, topic8p, auth18p);
                        break;
                    case AdminCommands.OffloadStatus:
                        var tenant71 = admin.Arguments[0].ToString();
                        var nspace71 = admin.Arguments[1].ToString();
                        var topic9 = admin.Arguments[2].ToString();
                        var auth19 = (bool)admin.Arguments[3];
                        response = _adminRestapi.OffloadStatus(tenant71, nspace71, topic9, auth19);
                        break;
                    case AdminCommands.OffloadStatusPersistent:
                        var tenant71p = admin.Arguments[0].ToString();
                        var nspace71p = admin.Arguments[1].ToString();
                        var topic9p = admin.Arguments[2].ToString();
                        var auth19p = (bool)admin.Arguments[3];
                        response = _adminRestapi.OffloadStatus1(tenant71p, nspace71p, topic9p, auth19p);
                        break;
                    case AdminCommands.TriggerOffload:
                        var tenant72 = admin.Arguments[0].ToString();
                        var nspace72 = admin.Arguments[1].ToString();
                        var topic10 = admin.Arguments[2].ToString();
                        var auth20 = (bool)admin.Arguments[3];
                        _adminRestapi.TriggerOffload(tenant72, nspace72, topic10, auth20);
                        admin.Handler("TriggerOffload");
                        break;
                    case AdminCommands.TriggerOffloadPersistent:
                        var tenant72p = admin.Arguments[0].ToString();
                        var nspace72p = admin.Arguments[1].ToString();
                        var topic10p = admin.Arguments[2].ToString();
                        var auth20p = (bool)admin.Arguments[3];
                        _adminRestapi.TriggerOffload1(tenant72p, nspace72p, topic10p, auth20p);
                        admin.Handler("TriggerOffloadPersistent");
                        break;
                    case AdminCommands.GetPartitionedStats:
                        var tenant73 = admin.Arguments[0].ToString();
                        var nspace73 = admin.Arguments[1].ToString();
                        var topic11 = admin.Arguments[2].ToString();
                        var per = (bool)admin.Arguments[3];
                        var auth21 = (bool)admin.Arguments[4];
                        _adminRestapi.GetPartitionedStats(tenant73, nspace73, topic11, auth21);
                        admin.Handler("GetPartitionedStats");
                        break;
                    case AdminCommands.GetPartitionedStatsPersistent:
                        var tenant73p = admin.Arguments[0].ToString();
                        var nspace73p = admin.Arguments[1].ToString();
                        var topic11p = admin.Arguments[2].ToString();
                        var perp = (bool)admin.Arguments[3];
                        var auth21p = (bool)admin.Arguments[4];
                        _adminRestapi.GetPartitionedStats1(tenant73p, nspace73p, topic11p, auth21p);
                        admin.Handler("GetPartitionedStatsPersistent");
                        break;
                    case AdminCommands.GetPartitionedMetadata:
                        var tenant74 = admin.Arguments[0].ToString();
                        var nspace74 = admin.Arguments[1].ToString();
                        var topic12 = admin.Arguments[2].ToString();
                        var auth22 = (bool)admin.Arguments[3];
                        var check = (bool)admin.Arguments[4];
                        response = _adminRestapi.GetPartitionedMetadata(tenant74, nspace74, topic12, auth22, check);
                        break;
                    case AdminCommands.GetPartitionedMetadataPersistence:
                        var tenant74p = admin.Arguments[0].ToString();
                        var nspace74p = admin.Arguments[1].ToString();
                        var topic12p = admin.Arguments[2].ToString();
                        var auth22p = (bool)admin.Arguments[3];
                        var checkp = (bool)admin.Arguments[4];
                        response = _adminRestapi.GetPartitionedMetadata1(tenant74p, nspace74p, topic12p, auth22p, checkp);
                        break;
                    case AdminCommands.UpdatePartitionedTopic:
                        var tenant75 = admin.Arguments[0].ToString();
                        var nspace75 = admin.Arguments[1].ToString();
                        var topic13 = admin.Arguments[2].ToString();
                        var body1 = (int)admin.Arguments[3];
                        var local = (bool)admin.Arguments[4];
                        _adminRestapi.UpdatePartitionedTopic(tenant75, nspace75, topic13, body1, local);
                        admin.Handler("UpdatePartitionedTopic");
                        break;
                    case AdminCommands.UpdatePartitionedPersistentTopic:
                        var tenant75p = admin.Arguments[0].ToString();
                        var nspace75p = admin.Arguments[1].ToString();
                        var topic13p = admin.Arguments[2].ToString();
                        var body1p = (int)admin.Arguments[3];
                        var localp = (bool)admin.Arguments[4];
                        _adminRestapi.UpdatePartitionedTopic1(tenant75p, nspace75p, topic13p, body1p, localp);
                        admin.Handler("UpdatePartitionedPersistentTopic");
                        break;
                    case AdminCommands.CreatePartitionedTopic:
                        var tenant76 = admin.Arguments[0].ToString();
                        var nspace76 = admin.Arguments[1].ToString();
                        var topic14 = admin.Arguments[2].ToString();
                        var body2 = (int)admin.Arguments[3];
                        _adminRestapi.CreatePartitionedTopic(tenant76, nspace76, topic14, body2);
                        admin.Handler("CreatePartitionedTopic");
                        break;
                    case AdminCommands.CreatePartitionedPersistentTopic:
                        var tenant76p = admin.Arguments[0].ToString();
                        var nspace76p = admin.Arguments[1].ToString();
                        var topic14p = admin.Arguments[2].ToString();
                        var body2p = (int)admin.Arguments[3];
                        _adminRestapi.CreatePartitionedTopic1(tenant76p, nspace76p, topic14p, body2p);
                        admin.Handler("CreatePartitionedPersistentTopic");
                        break;
                    case AdminCommands.DeletePartitionedTopic:
                        var tenant77 = admin.Arguments[0].ToString();
                        var nspace77 = admin.Arguments[1].ToString();
                        var topic15 = admin.Arguments[2].ToString();
                        var force2 = (bool)admin.Arguments[3];
                        var auth23 = (bool)admin.Arguments[4];
                        _adminRestapi.DeletePartitionedTopic(tenant77, nspace77, topic15, force2, auth23);
                        admin.Handler("DeletePartitionedTopic");
                        break;
                    case AdminCommands.DeletePartitionedPersistentTopic:
                        var tenant77p = admin.Arguments[0].ToString();
                        var nspace77p = admin.Arguments[1].ToString();
                        var topic15p = admin.Arguments[2].ToString();
                        var force2p = (bool)admin.Arguments[3];
                        var auth23p = (bool)admin.Arguments[4];
                        _adminRestapi.DeletePartitionedTopic1(tenant77p, nspace77p, topic15p, force2p, auth23p);
                        admin.Handler("DeletePartitionedPersistentTopic");
                        break;
                    case AdminCommands.GetPermissionsOnTopic:
                        var tenant78 = admin.Arguments[0].ToString();
                        var nspace78 = admin.Arguments[1].ToString();
                        var topic16 = admin.Arguments[2].ToString();
                        response = _adminRestapi.GetPermissionsOnTopic(tenant78, nspace78, topic16);
                        break;
                    case AdminCommands.GetPermissionsOnPersistentTopic:
                        var tenant78p = admin.Arguments[0].ToString();
                        var nspace78p = admin.Arguments[1].ToString();
                        var topic16p = admin.Arguments[2].ToString();
                        response = _adminRestapi.GetPermissionsOnTopic1(tenant78p, nspace78p, topic16p);
                        break;
                    case AdminCommands.GrantPermissionsOnTopic:
                        var tenant79 = admin.Arguments[0].ToString();
                        var nspace79 = admin.Arguments[1].ToString();
                        var topic17 = admin.Arguments[2].ToString();
                        var role3 = admin.Arguments[3].ToString();
                        var actions = (IList<string>)admin.Arguments[4];
                        _adminRestapi.GrantPermissionsOnTopic(tenant79, nspace79, topic17, role3, actions);
                        admin.Handler("GrantPermissionsOnTopic");
                        break;
                    case AdminCommands.GrantPermissionsOnPersistentTopic:
                        var tenant79p = admin.Arguments[0].ToString();
                        var nspace79p = admin.Arguments[1].ToString();
                        var topic17p = admin.Arguments[2].ToString();
                        var role3p = admin.Arguments[3].ToString();
                        var actionsp = (IList<string>)admin.Arguments[4];
                        _adminRestapi.GrantPermissionsOnTopic1(tenant79p, nspace79p, topic17p, role3p, actionsp);
                        admin.Handler("GrantPermissionsOnPersistentTopic");
                        break;
                    case AdminCommands.RevokePermissionsOnTopic:
                        var tenant80 = admin.Arguments[0].ToString();
                        var nspace80 = admin.Arguments[1].ToString();
                        var topic18 = admin.Arguments[2].ToString();
                        var role4 = admin.Arguments[3].ToString();
                        _adminRestapi.RevokePermissionsOnTopic(tenant80, nspace80, topic18, role4);
                        admin.Handler("RevokePermissionsOnTopic");
                        break;
                    case AdminCommands.RevokePermissionsOnPersistentTopic:
                        var tenant80p = admin.Arguments[0].ToString();
                        var nspace80p = admin.Arguments[1].ToString();
                        var topic18p = admin.Arguments[2].ToString();
                        var role4p = admin.Arguments[3].ToString();
                        _adminRestapi.RevokePermissionsOnTopic(tenant80p, nspace80p, topic18p, role4p);
                        admin.Handler("RevokePermissionsOnPersistentTopic");
                        break;
                    case AdminCommands.GetStats:
                        var tenant81 = admin.Arguments[0].ToString();
                        var nspace81 = admin.Arguments[1].ToString();
                        var topic19 = admin.Arguments[2].ToString();
                        var auth24 = (bool)admin.Arguments[3];
                        response = _adminRestapi.GetStats(tenant81, nspace81, topic19, auth24);
                        break;
                    case AdminCommands.GetPersistentStats:
                        var tenant81p = admin.Arguments[0].ToString();
                        var nspace81p = admin.Arguments[1].ToString();
                        var topic19p = admin.Arguments[2].ToString();
                        var auth24p = (bool)admin.Arguments[3];
                        response = _adminRestapi.GetStats1(tenant81p, nspace81p, topic19p, auth24p);
                        break;
                    case AdminCommands.DeleteSubscription:
                        var tenant82 = admin.Arguments[0].ToString();
                        var nspace82 = admin.Arguments[1].ToString();
                        var topic20 = admin.Arguments[2].ToString();
                        var subName = admin.Arguments[3].ToString();
                        var auth25 = (bool)admin.Arguments[4];
                        _adminRestapi.DeleteSubscription(tenant82, nspace82, topic20, subName, auth25);
                        admin.Handler("DeleteSubscription");
                        break;
                    case AdminCommands.DeleteSubscriptionPersistent:
                        var tenant82p = admin.Arguments[0].ToString();
                        var nspace82p = admin.Arguments[1].ToString();
                        var topic20p = admin.Arguments[2].ToString();
                        var subNamep = admin.Arguments[3].ToString();
                        var auth25p = (bool)admin.Arguments[4];
                        _adminRestapi.DeleteSubscription1(tenant82p, nspace82p, topic20p, subNamep, auth25p);
                        admin.Handler("DeleteSubscriptionPersistent");
                        break;
                    case AdminCommands.ExpireTopicMessages:
                        var tenant83 = admin.Arguments[0].ToString();
                        var nspace83 = admin.Arguments[1].ToString();
                        var topic21 = admin.Arguments[2].ToString();
                        var subName1 = admin.Arguments[3].ToString();
                        var expireSec = (int)admin.Arguments[4];
                        var auth26 = (bool)admin.Arguments[5];
                        _adminRestapi.ExpireTopicMessages(tenant83, nspace83, topic21, subName1, expireSec, auth26);
                        admin.Handler("ExpireTopicMessages");
                        break;
                    case AdminCommands.ExpirePersistentTopicMessages:
                        var tenant83p = admin.Arguments[0].ToString();
                        var nspace83p = admin.Arguments[1].ToString();
                        var topic21p = admin.Arguments[2].ToString();
                        var subName1p = admin.Arguments[3].ToString();
                        var expireSecp = (int)admin.Arguments[4];
                        var auth26p = (bool)admin.Arguments[5];
                        _adminRestapi.ExpireTopicMessages1(tenant83p, nspace83p, topic21p, subName1p, expireSecp, auth26p);
                        admin.Handler("ExpirePersistentTopicMessages");
                        break;
                    case AdminCommands.PeekNthMessage:
                        var tenant84 = admin.Arguments[0].ToString();
                        var nspace84 = admin.Arguments[1].ToString();
                        var topic22 = admin.Arguments[2].ToString();
                        var subName2 = admin.Arguments[3].ToString();
                        var position = (int)admin.Arguments[4];
                        var auth27 = (bool)admin.Arguments[5];
                        _adminRestapi.PeekNthMessage(tenant84, nspace84, topic22, subName2, position, auth27);
                        admin.Handler("PeekNthMessage");
                        break;
                    case AdminCommands.PeekNthPersistentMessage:
                        var tenant84p = admin.Arguments[0].ToString();
                        var nspace84p = admin.Arguments[1].ToString();
                        var topic22p = admin.Arguments[2].ToString();
                        var subName2p = admin.Arguments[3].ToString();
                        var positionp = (int)admin.Arguments[4];
                        var auth27p = (bool)admin.Arguments[5];
                        _adminRestapi.PeekNthMessage1(tenant84p, nspace84p, topic22p, subName2p, positionp, auth27p);
                        admin.Handler("PeekNthPersistentMessage");
                        break;
                    case AdminCommands.ResetCursorOnPosition:
                        var tenant85 = admin.Arguments[0].ToString();
                        var nspace85 = admin.Arguments[1].ToString();
                        var topic23 = admin.Arguments[2].ToString();
                        var subName3 = admin.Arguments[3].ToString();
                        var auth28 = (bool)admin.Arguments[4];
                        var id = (MessageIdImpl)admin.Arguments[5];
                        _adminRestapi.ResetCursorOnPosition(tenant85, nspace85, topic23, subName3, auth28, id);
                        admin.Handler("ResetCursorOnPosition");
                        break;
                    case AdminCommands.ResetCursorOnPersistentPosition:
                        var tenant85p = admin.Arguments[0].ToString();
                        var nspace85p = admin.Arguments[1].ToString();
                        var topic23p = admin.Arguments[2].ToString();
                        var subName3p = admin.Arguments[3].ToString();
                        var auth28p = (bool)admin.Arguments[4];
                        var idp = (MessageIdImpl)admin.Arguments[5];
                        _adminRestapi.ResetCursorOnPosition1(tenant85p, nspace85p, topic23p, subName3p, auth28p, idp);
                        admin.Handler("ResetCursorOnPersistentPosition");
                        break;
                    case AdminCommands.ResetCursor:
                        var tenant86 = admin.Arguments[0].ToString();
                        var nspace86 = admin.Arguments[1].ToString();
                        var topic24 = admin.Arguments[2].ToString();
                        var subName4 = admin.Arguments[3].ToString();
                        var time = (long)admin.Arguments[4];
                        var auth29 = (bool)admin.Arguments[5];
                        _adminRestapi.ResetCursor(tenant86, nspace86, topic24, subName4, time, auth29);
                        admin.Handler("ResetCursor");
                        break;
                    case AdminCommands.ResetPersistentCursor:
                        var tenant86p = admin.Arguments[0].ToString();
                        var nspace86p = admin.Arguments[1].ToString();
                        var topic24p = admin.Arguments[2].ToString();
                        var subName4p = admin.Arguments[3].ToString();
                        var timep = (long)admin.Arguments[4];
                        var auth29p = (bool)admin.Arguments[5];
                        _adminRestapi.ResetCursor1(tenant86p, nspace86p, topic24p, subName4p, timep, auth29p);
                        admin.Handler("ResetPersistentCursor");
                        break;
                    case AdminCommands.SkipMessages:
                        var tenant87 = admin.Arguments[0].ToString();
                        var nspace87 = admin.Arguments[1].ToString();
                        var topic25 = admin.Arguments[2].ToString();
                        var subName5 = admin.Arguments[3].ToString();
                        var num = (int)admin.Arguments[4];
                        var auth30 = (bool)admin.Arguments[5];
                        _adminRestapi.SkipMessages(tenant87, nspace87, topic25, subName5, num, auth30);
                        admin.Handler("SkipMessages");
                        break;
                    case AdminCommands.SkipPersistentMessages:
                        var tenant87p = admin.Arguments[0].ToString();
                        var nspace87p = admin.Arguments[1].ToString();
                        var topic25p = admin.Arguments[2].ToString();
                        var subName5p = admin.Arguments[3].ToString();
                        var nump = (int)admin.Arguments[4];
                        var auth30p = (bool)admin.Arguments[5];
                        _adminRestapi.SkipMessages1(tenant87p, nspace87p, topic25p, subName5p, nump, auth30p);
                        admin.Handler("SkipPersistentMessages");
                        break;
                    case AdminCommands.SkipAllMessages:
                        var tenant88 = admin.Arguments[0].ToString();
                        var nspace88 = admin.Arguments[1].ToString();
                        var topic26 = admin.Arguments[2].ToString();
                        var subName6 = admin.Arguments[3].ToString();
                        var auth31 = (bool)admin.Arguments[4];
                        _adminRestapi.SkipAllMessages(tenant88, nspace88, topic26, subName6, auth31);
                        admin.Handler("SkipAllMessages");
                        break;
                    case AdminCommands.SkipAllPersistentMessages:
                        var tenant88p = admin.Arguments[0].ToString();
                        var nspace88p = admin.Arguments[1].ToString();
                        var topic26p = admin.Arguments[2].ToString();
                        var subName6p = admin.Arguments[3].ToString();
                        var auth31p = (bool)admin.Arguments[4];
                        _adminRestapi.SkipAllMessages1(tenant88p, nspace88p, topic26p, subName6p, auth31p);
                        admin.Handler("SkipAllPersistentMessages");
                        break;
                    case AdminCommands.CreateSubscription:
                        var tenant89 = admin.Arguments[0].ToString();
                        var nspace89 = admin.Arguments[1].ToString();
                        var topic27 = admin.Arguments[2].ToString();
                        var subName7 = admin.Arguments[3].ToString();
                        var mId = admin.Arguments[4].ToString();
                        var auth32 = (bool)admin.Arguments[5];
                        _adminRestapi.CreateSubscription(tenant89, nspace89, topic27, subName7, mId, auth32);
                        admin.Handler("CreateSubscription");
                        break;
                    case AdminCommands.CreateSubscriptionPersistent:
                        var tenant89p = admin.Arguments[0].ToString();
                        var nspace89p = admin.Arguments[1].ToString();
                        var topic27p = admin.Arguments[2].ToString();
                        var subName7p = admin.Arguments[3].ToString();
                        var mIdp = admin.Arguments[4].ToString();
                        var auth32p = (bool)admin.Arguments[5];
                        _adminRestapi.CreateSubscription1(tenant89p, nspace89p, topic27p, subName7p, mIdp, auth32p);
                        admin.Handler("CreateSubscriptionPersistent");
                        break;
                    case AdminCommands.GetSubscriptions:
                        var tenant90 = admin.Arguments[0].ToString();
                        var nspace90 = admin.Arguments[1].ToString();
                        var topic28 = admin.Arguments[2].ToString();
                        var auth33 = (bool)admin.Arguments[3];
                        _adminRestapi.GetSubscriptions(tenant90, nspace90, topic28, auth33);
                        admin.Handler("GetSubscriptions");
                        break;
                    case AdminCommands.GetSubscriptionsPersistent:
                        var tenant90p = admin.Arguments[0].ToString();
                        var nspace90p = admin.Arguments[1].ToString();
                        var topic28p = admin.Arguments[2].ToString();
                        var auth33p = (bool)admin.Arguments[3];
                        _adminRestapi.GetSubscriptions1(tenant90p, nspace90p, topic28p, auth33p);
                        admin.Handler("GetSubscriptionsPersistent");
                        break;
                    case AdminCommands.Terminate:
                        var tenant91 = admin.Arguments[0].ToString();
                        var nspace91 = admin.Arguments[1].ToString();
                        var topic29 = admin.Arguments[2].ToString();
                        var auth34 = (bool)admin.Arguments[3];
                        response = _adminRestapi.Terminate(tenant91, nspace91, topic29, auth34);
                        break;
                    case AdminCommands.TerminatePersistent:
                        var tenant91p = admin.Arguments[0].ToString();
                        var nspace91p = admin.Arguments[1].ToString();
                        var topic29p = admin.Arguments[2].ToString();
                        var auth34p = (bool)admin.Arguments[3];
                        response = _adminRestapi.Terminate1(tenant91p, nspace91p, topic29p, auth34p);
                        break;
                    case AdminCommands.UnloadTopic:
                        var tenant92 = admin.Arguments[0].ToString();
                        var nspace92 = admin.Arguments[1].ToString();
                        var topic30 = admin.Arguments[2].ToString();
                        var auth35 = (bool)admin.Arguments[3];
                        _adminRestapi.UnloadTopic(tenant92, nspace92, topic30, auth35);
                        admin.Handler("UnloadTopic");
                        break;
                    case AdminCommands.UnloadPersistentTopic:
                        var tenant92p = admin.Arguments[0].ToString();
                        var nspace92p = admin.Arguments[1].ToString();
                        var topic30p = admin.Arguments[2].ToString();
                        var auth35p = (bool)admin.Arguments[3];
                        _adminRestapi.UnloadTopic1(tenant92p, nspace92p, topic30p, auth35p);
                        admin.Handler("UnloadPersistentTopic");
                        break;
                    case AdminCommands.GetDefaultResourceQuota:
                        response = _adminRestapi.GetDefaultResourceQuota();
                        break;
                    case AdminCommands.SetDefaultResourceQuota:
                        var resource = (ResourceQuota)admin.Arguments[0];
                        response = _adminRestapi.SetDefaultResourceQuota(resource);
                        break;
                    case AdminCommands.GetNamespaceBundleResourceQuota:
                        var tenant93 = admin.Arguments[0].ToString();
                        var nspace93 = admin.Arguments[1].ToString();
                        var bndle8 = admin.Arguments[2].ToString();
                        response = _adminRestapi.GetNamespaceBundleResourceQuota(tenant93, nspace93, bndle8);
                        break;
                    case AdminCommands.SetNamespaceBundleResourceQuota:
                        var tenant94 = admin.Arguments[0].ToString();
                        var nspace94 = admin.Arguments[1].ToString();
                        var bndle9 = admin.Arguments[2].ToString();
                        _adminRestapi.SetNamespaceBundleResourceQuota(tenant94, nspace94, bndle9);
                        admin.Handler("SetNamespaceBundleResourceQuota");
                        break;
                    case AdminCommands.RemoveNamespaceBundleResourceQuota:
                        var tenant95 = admin.Arguments[0].ToString();
                        var nspace95 = admin.Arguments[1].ToString();
                        var bndle10 = admin.Arguments[2].ToString();
                        _adminRestapi.RemoveNamespaceBundleResourceQuota(tenant95, nspace95, bndle10);
                        admin.Handler("RemoveNamespaceBundleResourceQuota");
                        break;
                    case AdminCommands.TestCompatibility:
                        var tenant96 = admin.Arguments[0].ToString();
                        var nspace96 = admin.Arguments[1].ToString();
                        var topic31 = admin.Arguments[2].ToString();
                        var body6 = (PostSchemaPayload)admin.Arguments[3];
                        var auth36 = (bool)admin.Arguments[4];
                        response = _adminRestapi.TestCompatibility(tenant96, nspace96, topic31, body6, auth36);
                        break;
                    case AdminCommands.GetSchema:
                        var tenant97 = admin.Arguments[0].ToString();
                        var nspace97 = admin.Arguments[1].ToString();
                        var topic32 = admin.Arguments[2].ToString();
                        var auth37 = (bool)admin.Arguments[3];
                        response = _adminRestapi.GetSchema(tenant97, nspace97, topic32, auth37);
                        break;
                    case AdminCommands.PostSchema:
                        var tenant98 = admin.Arguments[0].ToString();
                        var nspace98 = admin.Arguments[1].ToString();
                        var topic33 = admin.Arguments[2].ToString();
                        var body7 = (PostSchemaPayload)admin.Arguments[3];
                        var auth38 = (bool)admin.Arguments[4];
                        response = _adminRestapi.PostSchema(tenant98, nspace98, topic33, body7, auth38);
                        break;
                    case AdminCommands.DeleteSchema:
                        var tenant99 = admin.Arguments[0].ToString();
                        var nspace99 = admin.Arguments[1].ToString();
                        var topic34 = admin.Arguments[2].ToString();
                        var auth39 = (bool)admin.Arguments[3];
                        response = _adminRestapi.DeleteSchema(tenant99, nspace99, topic34, auth39);
                        break;
                    case AdminCommands.GetSchemaVersion:
                        var tenant100 = admin.Arguments[0].ToString();
                        var nspace100 = admin.Arguments[1].ToString();
                        var topic35 = admin.Arguments[2].ToString();
                        var version = admin.Arguments[3].ToString();
                        var auth40 = (bool)admin.Arguments[4];
                        response = _adminRestapi.GetSchemaVersion(tenant100, nspace100, topic35, version, auth40);
                        break;
                    case AdminCommands.GetAllSchemas:
                        var tenant101 = admin.Arguments[0].ToString();
                        var nspace101 = admin.Arguments[1].ToString();
                        var topic36 = admin.Arguments[2].ToString();
                        var auth41 = (bool)admin.Arguments[3];
                        response = _adminRestapi.GetAllSchemas(tenant101, nspace101, topic36, auth41);
                        break;
                    case AdminCommands.GetVersionBySchema:
                        var tenant102 = admin.Arguments[0].ToString();
                        var nspace102 = admin.Arguments[1].ToString();
                        var topic37 = admin.Arguments[2].ToString();
                        var body8 = (PostSchemaPayload)admin.Arguments[3];
                        var auth42 = (bool)admin.Arguments[4];
                        response = _adminRestapi.GetVersionBySchema(tenant102, nspace102, topic37, body8, auth42);
                        break;
                    case AdminCommands.GetTenants:
                        response = _adminRestapi.GetTenants();
                        break;
                    case AdminCommands.GetTenantAdmin:
                        var tenant103 = admin.Arguments[0].ToString();
                        response = _adminRestapi.GetTenantAdmin(tenant103);
                        break;
                    case AdminCommands.UpdateTenant:
                        var tenant104 = admin.Arguments[0].ToString();
                        var bdy = (TenantInfo)admin.Arguments[1];
                        _adminRestapi.UpdateTenant(tenant104, bdy);
                        admin.Handler("UpdateTenant");
                        break;
                    case AdminCommands.CreateTenant:
                        var tenant105 = admin.Arguments[0].ToString();
                        var bdy1 = (TenantInfo)admin.Arguments[1];
                        _adminRestapi.CreateTenant(tenant105, bdy1);
                        admin.Handler("CreateTenant");
                        break;
                    case AdminCommands.DeleteTenant:
                        var tenant106 = admin.Arguments[0].ToString();
                        _adminRestapi.DeleteTenant(tenant106);
                        admin.Handler("DeleteTenant");
                        break;
                    case AdminCommands.SetDelayedDeliveryPolicies:
                        var tenant107_ = admin.Arguments[0].ToString();
                        var ns107 = admin.Arguments[1].ToString();
                        var pol = (DelayedDeliveryPolicies)admin.Arguments[2];
                        _adminRestapi.SetDelayedDeliveryPolicies(tenant107_, ns107, pol);
                        admin.Handler("SetDelayedDeliveryPolicies");
                        break;
                    case AdminCommands.GetDelayedDeliveryPolicies:
                        var tenant108_ = admin.Arguments[0].ToString();
                        var ns108 = admin.Arguments[1].ToString();
                        response = _adminRestapi.GetDelayedDeliveryPolicies(tenant108_, ns108);
                        break;
                    case AdminCommands.SetOffloadPolicies:
                        var tenant109 = admin.Arguments[0].ToString();
                        var ns109 = admin.Arguments[1].ToString();
                        var poli = (OffloadPolicies)admin.Arguments[2];
                        _adminRestapi.SetOffloadPolicies(tenant109, ns109, poli);
                        admin.Handler("SetOffloadPolicies");
                        break;
                    case AdminCommands.GetOffloadPolicies:
                        var tenant110 = admin.Arguments[0].ToString();
                        var ns110 = admin.Arguments[1].ToString();
                        response =_adminRestapi.GetOffloadPolicies(tenant110, ns110);
                        break;
                }
                _pulsarManager.Tell(new AdminResponse(response));
            }
            catch (Exception e)
            {
                admin.Exception(e);
                _pulsarManager.Tell(new AdminResponse(e));
            }
        }

        public static Props Prop(string server, IActorRef pulsarManager)
        {
            return Props.Create(() => new AdminWorker(server, pulsarManager));
        }
    }
}
