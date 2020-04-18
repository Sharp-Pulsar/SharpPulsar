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

            };
            return help;
        }
        public static Props Prop(AdminConfiguration configuration)
        {
            return Props.Create(() => new AdminWorker(configuration));
        }
    }
}
