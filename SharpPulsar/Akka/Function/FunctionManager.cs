using System.Text.RegularExpressions;
using Akka.Actor;

namespace SharpPulsar.Akka.Function
{
    public class FunctionManager : ReceiveActor
    {
        public FunctionManager(FunctionConfiguration configuration)
        {
            foreach (var s in configuration.BrokerWebServiceUrl)
            {
                var an = Regex.Replace(s, @"[^\w\d]", "");
                Context.ActorOf(FunctionCoordinator.Prop(s), an);
            }

            Receive((InternalCommands.Admin q) =>
            {
                var an = Regex.Replace(q.BrokerDestinationUrl, @"[^\w\d]", "");
                var actor = Context.Child(an);
                if (actor.IsNobody())
                    q.Log($"{q.BrokerDestinationUrl} not found");
                else
                    actor.Tell(q);
            });
        }
        public static Props Prop(FunctionConfiguration configuration)
        {
            return Props.Create(() => new FunctionManager(configuration));
        }
    }
}
