using System;
using System.Collections.Generic;
using System.Net.Http;
using Akka.Actor;
using PulsarAdmin;
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
                ["GetMetrics"] = "return IList<Metrics>\nArguments[]"
            };
            return help;
        }
        public static Props Prop(AdminConfiguration configuration)
        {
            return Props.Create(() => new AdminWorker(configuration));
        }
    }
}
