using System.Text.RegularExpressions;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Admin
{
    public class AdminManager:ReceiveActor
    {
        public AdminManager(AdminConfiguration configuration)
        {
            foreach (var s in configuration.BrokerWebServiceUrl)
            {
                var an = Regex.Replace(s, @"[^\w\d]", "");
                Context.ActorOf(AdminCoordinator.Prop(s), an);
            }

            Receive<QueryAdmin>(q =>
            {
                var an = Regex.Replace(q.BrokerDestinationUrl, @"[^\w\d]", "");
                var actor = Context.Child(an);
                if (actor.IsNobody())
                    q.Log($"{q.BrokerDestinationUrl} not found");
                else
                    actor.Tell(q);
            });
        }
        public static Props Prop(AdminConfiguration configuration)
        {
            return Props.Create(() => new AdminManager(configuration));
        }
    }
}
