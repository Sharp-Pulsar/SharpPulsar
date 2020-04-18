using System;
using System.Collections.Generic;
using System.Net.Http;
using Akka.Actor;
using PulsarAdmin;
using PulsarAdmin.Models;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Admin
{
    public class AdminWorker:ReceiveActor
    {
        private readonly PulsarAdminRESTAPI _adminRestapi;
        public AdminWorker(AdminConfiguration configuration)
        {
            _adminRestapi = new PulsarAdminRESTAPI(configuration.BrokerWebServiceUrl, new HttpClient(), true);
            Receive<QueryAdmin>(Handle);
        }

        protected override void Unhandled(object message)
        {
            
        }

        private void Handle(QueryAdmin admin)
        {
            try
            {
                switch (admin.Command)
                {
                    case AdminCommands.Help:
                        admin.Handler(Help());
                        break;
                    case AdminCommands.GetBookiesRackInfo:
                        var response = _adminRestapi.GetBookiesRackInfo();
                        admin.Handler(response);
                        break;
                    case AdminCommands.GetBookieRackInfo:
                        var bookie = admin.Arguments[0].ToString();
                        admin.Handler(_adminRestapi.GetBookieRackInfo(bookie));
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
                        admin.Handler(_adminRestapi.GetAllocatorStats(allocator));
                        break;
                    case AdminCommands.GetPendingBookieOpsStats:
                        admin.Handler(_adminRestapi.GetPendingBookieOpsStats());
                        break;
                    case AdminCommands.GetBrokerResourceAvailability:
                        admin.Handler(_adminRestapi.GetBrokerResourceAvailability(admin.Arguments[0].ToString(), admin.Arguments[1].ToString()));
                        break;
                    case AdminCommands.GetLoadReport:
                        admin.Handler(_adminRestapi.GetLoadReport());
                        break;
                    case AdminCommands.GetMBeans:
                        admin.Handler(_adminRestapi.GetMBeans());
                        break;
                    case AdminCommands.GetMetrics:
                        admin.Handler(_adminRestapi.GetMetrics());
                        break;
                    case AdminCommands.GetTopics2:
                        admin.Handler(_adminRestapi.GetTopics2());
                        break;
                    case AdminCommands.GetDynamicConfigurationName:
                        admin.Handler(_adminRestapi.GetDynamicConfigurationName());
                        break;
                    case AdminCommands.GetRuntimeConfiguration:
                        admin.Handler(_adminRestapi.GetRuntimeConfiguration());
                        break;
                    case AdminCommands.GetAllDynamicConfigurations:
                        admin.Handler(_adminRestapi.GetAllDynamicConfigurations());
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
                        admin.Handler(_adminRestapi.GetInternalConfigurationData());
                        break;
                    case AdminCommands.GetOwnedNamespaces:
                        var cluster = admin.Arguments[0].ToString();
                        var service = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetOwnedNamespaces(cluster, service));
                        break;
                    case AdminCommands.GetActiveBrokers:
                        var clustr = admin.Arguments[0].ToString();
                        admin.Handler(_adminRestapi.GetActiveBrokers(clustr));
                        break;
                    case AdminCommands.GetCluster:
                        var clust = admin.Arguments[0].ToString();
                        admin.Handler(_adminRestapi.GetCluster(clust));
                        break;
                    case AdminCommands.GetClusters:
                        admin.Handler(_adminRestapi.GetClusters());
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
                        admin.Handler(_adminRestapi.GetFailureDomains(clr));
                        break;
                    case AdminCommands.GetDomain:
                        var cltr = admin.Arguments[0].ToString();
                        var domain = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetDomain(cltr, domain));
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
                        admin.Handler(_adminRestapi.GetNamespaceIsolationPolicies(decltrN));
                        break;
                    case AdminCommands.GetBrokersWithNamespaceIsolationPolicy:
                        var cname = admin.Arguments[0].ToString();
                        admin.Handler(_adminRestapi.GetBrokersWithNamespaceIsolationPolicy(cname));
                        break;
                    case AdminCommands.GetBrokerWithNamespaceIsolationPolicy:
                        var clname = admin.Arguments[0].ToString();
                        var broker = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetBrokerWithNamespaceIsolationPolicy(clname, broker));
                        break;
                    case AdminCommands.GetNamespaceIsolationPolicy:
                        var cluname = admin.Arguments[0].ToString();
                        var policy = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetNamespaceIsolationPolicy(cluname, policy));
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
                        admin.Handler(_adminRestapi.GetPeerCluster(clustename));
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
                        admin.Handler(_adminRestapi.GetAntiAffinityNamespaces(c_Name, c_Group, t));
                        break;
                    case AdminCommands.GetBookieAffinityGroup:
                        var property = admin.Arguments[0].ToString();
                        var nSpace = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetBookieAffinityGroup(property, nSpace));
                        break;
                    case AdminCommands.DeleteBookieAffinityGroup:
                        var propy = admin.Arguments[0].ToString();
                        var naSpace = admin.Arguments[1].ToString();
                        _adminRestapi.DeleteBookieAffinityGroup(propy, naSpace);
                        admin.Handler("DeleteBookieAffinityGroup");
                        break;
                    case AdminCommands.GetTenantNamespaces:
                        var te = admin.Arguments[0].ToString();
                        admin.Handler(_adminRestapi.GetTenantNamespaces(te));
                        break;
                    case AdminCommands.GetPolicies:
                        var ten = admin.Arguments[0].ToString();
                        var ns = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetPolicies(ten, ns));
                        break;
                    case AdminCommands.CreateNamespace:
                        var tena = admin.Arguments[0].ToString();
                        var nsp = admin.Arguments[1].ToString();
                        _adminRestapi.GetPolicies(tena, nsp);
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
                        admin.Handler(_adminRestapi.GetNamespaceAntiAffinityGroup(tenaNt, nspac));
                        break;
                    case AdminCommands.SetNamespaceAntiAffinityGroup:
                        var _tenaNt = admin.Arguments[0].ToString();
                        var _nspac = admin.Arguments[1].ToString();
                        _adminRestapi.SetNamespaceAntiAffinityGroup(_tenaNt, _nspac);
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
                        admin.Handler(_adminRestapi.GetBacklogQuotaMap(tenant2, nspace2));
                        break;
                    case AdminCommands.GetBundlesData:
                        var tenant3 = admin.Arguments[0].ToString();
                        var nspace3 = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetBundlesData(tenant3, nspace3));
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
                        _adminRestapi.SetCompactionThreshold(tenant6, nspace6);
                        admin.Handler("SetCompactionThreshold");
                        break;
                    case AdminCommands.ModifyDeduplication:
                        var tenant8 = admin.Arguments[0].ToString();
                        var nspace8 = admin.Arguments[1].ToString();
                        _adminRestapi.ModifyDeduplication(tenant8, nspace8);
                        admin.Handler("ModifyDeduplication");
                        break;
                    case AdminCommands.GetCompactionThreshold:
                        var tenant7 = admin.Arguments[0].ToString();
                        var nspace7 = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetCompactionThreshold(tenant7, nspace7));
                        break;
                    case AdminCommands.GetDispatchRate:
                        var tenant9 = admin.Arguments[0].ToString();
                        var nspace9 = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetDispatchRate(tenant9, nspace9));
                        break;
                    case AdminCommands.SetDispatchRate:
                        var tenant10 = admin.Arguments[0].ToString();
                        var nspace10 = admin.Arguments[1].ToString();
                        _adminRestapi.GetDispatchRate(tenant10, nspace10);
                        admin.Handler("SetDispatchRate");
                        break;
                    case AdminCommands.ModifyEncryptionRequired:
                        var tenant11 = admin.Arguments[0].ToString();
                        var nspace11 = admin.Arguments[1].ToString();
                        _adminRestapi.ModifyEncryptionRequired(tenant11, nspace11);
                        admin.Handler("ModifyEncryptionRequired");
                        break;
                    case AdminCommands.GetIsAllowAutoUpdateSchema:
                        var tenant12 = admin.Arguments[0].ToString();
                        var nspace12 = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetIsAllowAutoUpdateSchema(tenant12, nspace12));
                        break;
                    case AdminCommands.SetIsAllowAutoUpdateSchema:
                        var tenant13 = admin.Arguments[0].ToString();
                        var nspace13 = admin.Arguments[1].ToString();
                        _adminRestapi.SetIsAllowAutoUpdateSchema(tenant13, nspace13);
                        admin.Handler("SetIsAllowAutoUpdateSchema");
                        break;
                    case AdminCommands.SetMaxConsumersPerSubscription:
                        var tenant15 = admin.Arguments[0].ToString();
                        var nspace15 = admin.Arguments[1].ToString();
                        _adminRestapi.SetMaxConsumersPerSubscription(tenant15, nspace15);
                        admin.Handler("SetMaxConsumersPerSubscription");
                        break;
                    case AdminCommands.GetMaxConsumersPerSubscription:
                        var tenant14 = admin.Arguments[0].ToString();
                        var nspace14 = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetMaxConsumersPerSubscription(tenant14, nspace14));
                        break;
                    case AdminCommands.GetMaxConsumersPerTopic:
                        var tenant16 = admin.Arguments[0].ToString();
                        var nspace16 = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetMaxConsumersPerTopic(tenant16, nspace16));
                        break;
                    case AdminCommands.SetMaxConsumersPerTopic:
                        var tenant17 = admin.Arguments[0].ToString();
                        var nspace17 = admin.Arguments[1].ToString();
                        _adminRestapi.SetMaxConsumersPerTopic(tenant17, nspace17);
                        admin.Handler("SetMaxConsumersPerTopic");
                        break;
                    case AdminCommands.GetMaxProducersPerTopic:
                        var tenant18 = admin.Arguments[0].ToString();
                        var nspace18 = admin.Arguments[1].ToString();
                        admin.Handler(_adminRestapi.GetMaxProducersPerTopic(tenant18, nspace18));
                        break;
                    case AdminCommands.SetMaxProducersPerTopic:
                        var tenant19 = admin.Arguments[0].ToString();
                        var nspace19 = admin.Arguments[1].ToString();
                        _adminRestapi.SetMaxProducersPerTopic(tenant19, nspace19);
                        admin.Handler("SetMaxProducersPerTopic");
                        break;
                }
            }
            catch (Exception e)
            {
                admin.Exception(e);
            }
        }

        private Dictionary<string, string> Help()
        {
            var help = new Dictionary<string, string>
            {
                ["GetBookiesRackInfo"] = "return IDictionary<string, IDictionary<string, BookieInfo>>\nArguments[]",
                ["GetBookieRackInfo"] = "return BookieInfo\nArguments[string bookie]",
                ["UpdateBookieRackInfo"] = "void\nArguments[string bookie, string group]",
                ["DeleteBookieRackInfo"] = "void\nArguments[string bookie]",
                ["GetAllocatorStats"] = "return AllocatorStats\nArguments[string allocator]",
                ["GetPendingBookieOpsStats"] = "return IDictionary<string, PendingBookieOpsStats>\nArguments[]",
                ["GetBrokerResourceAvailability"] = "return IDictionary<string, ResourceUnit>\nArguments[string tenant, string namespace]",
                ["GetLoadReport"] = "return LoadReport\nArguments[]",
                ["GetMBeans"] = "return IList<Metrics>\nArguments[]",
                ["GetMetrics"] = "return IList<Metrics>\nArguments[]",
                ["GetTopics2"] = "return object\nArguments[]",
                ["GetDynamicConfigurationName"] = "return IList<object>\nArguments[]",
                ["GetRuntimeConfiguration"] = "return IDictionary<string, object>\nArguments[]",
                ["GetAllDynamicConfigurations"] = "return IDictionary<string, object>\nArguments[]",
                ["Healthcheck"] = "void\nArguments[]",
                ["DeleteDynamicConfiguration"] = "void\nArguments[string configName]",
                ["UpdateDynamicConfiguration"] = "void\nArguments[string configName, string configValue]",
                ["GetInternalConfigurationData"] = "return InternalConfigurationData\nArguments[]",
                ["GetOwnedNamespaces"] = "return IDictionary<string, NamespaceOwnershipStatus>\nArguments[string clusterName, string brokerWebserviceurl]",
                ["GetActiveBrokers"] = "return IList<string>\nArguments[string cluster]",
                ["GetCluster"] = "return ClusterData\nArguments[string cluster]",
                ["GetClusters"] = "return IList<string>\nArguments[]",
                ["UpdateCluster"] = "void\nArguments[string cluster, ClusterData body]",
                ["CreateCluster"] = "void\nArguments[string cluster, ClusterData body]",
                ["DeleteCluster"] = "void\nArguments[string cluster]",
                ["GetFailureDomains"] = "return IDictionary<string, FailureDomain>\nArguments[string cluster]",
                ["GetDomain"] = "return FailureDomain\nArguments[string cluster, string domainName]",
                ["SetFailureDomain"] = "void\nArguments[string cluster, string domainName, FailureDomain body]",
                ["DeleteFailureDomain"] = "void\nArguments[string cluster, string domainName]",
                ["GetNamespaceIsolationPolicies"] = "return IDictionary<string, NamespaceIsolationData>\nArguments[string cluster]",
                ["GetBrokersWithNamespaceIsolationPolicy"] = "return IList<BrokerNamespaceIsolationData>\nArguments[string cluster]",
                ["GetBrokerWithNamespaceIsolationPolicy"] = "return BrokerNamespaceIsolationData\nArguments[string cluster, string broker]",
                ["GetNamespaceIsolationPolicy"] = "return NamespaceIsolationData\nArguments[string cluster, string policyName]",
                ["SetNamespaceIsolationPolicy"] = "void\nArguments[string cluster, string policyName, NamespaceIsolationData body]",
                ["DeleteNamespaceIsolationPolicy"] = "void\nArguments[string cluster, string policyName]",
                ["GetPeerCluster"] = "return IList<string>\nArguments[string cluster]",
                ["SetPeerClusterNames"] = "void\nArguments[string cluster, IList<string> body]",
                ["GetAntiAffinityNamespaces"] = "return IList<object>\nArguments[string cluster, string group, string tenant]",
                ["GetBookieAffinityGroup"] = "return BookieAffinityGroupData\nArguments[string property, string namespace]",
                ["DeleteBookieAffinityGroup"] = "void\nArguments[string property, string namespace]",
                ["GetTenantNamespaces"] = "return IList<string>\nArguments[string tenant]",
                ["GetPolicies"] = "return Policies\nArguments[string tenant, string namespace]",
                ["CreateNamespace"] = "void\nArguments[string tenant, string namespace]",
                ["DeleteNamespace"] = "void\nArguments[string tenant, string namespace, bool authoritative]",
                ["GetNamespaceAntiAffinityGroup"] = "return string\nArguments[string tenant, string namespace]",
                ["SetNamespaceAntiAffinityGroup"] = "void\nArguments[string tenant, string namespace]",
                ["RemoveNamespaceAntiAffinityGroup"] = "void\nArguments[string tenant, string namespace]",
                ["SetBacklogQuota"] = "void\nArguments[string tenant, string namespace, string backlogQuotaType]",
                ["RemoveBacklogQuota"] = "void\nArguments[string tenant, string namespace, string backlogQuotaType]",
                ["GetBacklogQuotaMap"] = "return IDictionary<string, object>\nArguments[string tenant, string namespace]",
                ["GetBundlesData"] = "return BundlesData\nArguments[string tenant, string namespace]",
                ["ClearNamespaceBacklog"] = "void\nArguments[string tenant, string namespace, bool authoritative]",
                ["ClearNamespaceBacklogForSubscription"] = "void\nArguments[string tenant, string namespace, string subscription, bool authoritative]",
                ["GetCompactionThreshold"] = "return long\nArguments[string tenant, string namespace]",
                ["SetCompactionThreshold"] = "void\nArguments[string tenant, string namespace]",
                ["ModifyDeduplication"] = "void\nArguments[string tenant, string namespace]",
                ["GetDispatchRate"] = "return DispatchRate\nArguments[string tenant, string namespace]",
                ["SetDispatchRate"] = "void\nArguments[string tenant, string namespace]",
                ["ModifyEncryptionRequired"] = "void\nArguments[string tenant, string namespace]",
                ["GetIsAllowAutoUpdateSchema"] = "return bool\nArguments[string tenant, string namespace]",
                ["SetIsAllowAutoUpdateSchema"] = "void\nArguments[string tenant, string namespace]",
                ["GetMaxConsumersPerSubscription"] = "return int\nArguments[string tenant, string namespace]",
                ["SetMaxConsumersPerSubscription"] = "void\nArguments[string tenant, string namespace]",
                ["GetMaxConsumersPerTopic"] = "return int\nArguments[string tenant, string namespace]",
                ["SetMaxConsumersPerTopic"] = "void\nArguments[string tenant, string namespace]",
                ["GetMaxProducersPerTopic"] = "return int\nArguments[string tenant, string namespace]",
                ["SetMaxProducersPerTopic"] = "void\nArguments[string tenant, string namespace]",
            };
            return help;
        }
        public static Props Prop(AdminConfiguration configuration)
        {
            return Props.Create(() => new AdminWorker(configuration));
        }
    }
}
