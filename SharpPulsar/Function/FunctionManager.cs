using System.Text.RegularExpressions;
using Akka.Actor;

namespace SharpPulsar.Function
{
    public class FunctionManager : ReceiveActor
    {
        public FunctionManager(FunctionConfiguration configuration, IActorRef pulsarManager)
        {
            foreach (var s in configuration.BrokerWebServiceUrl)
            {
                var an = Regex.Replace(s, @"[^\w\d]", "");
                Context.ActorOf(FunctionCoordinator.Prop(s, pulsarManager), an);
            }

            Receive((Messages.Function q) =>
            {
                var an = Regex.Replace(q.BrokerDestinationUrl, @"[^\w\d]", "");
                var actor = Context.Child(an);
                if (actor.IsNobody())
                    q.Log($"{q.BrokerDestinationUrl} not found");
                else
                    actor.Tell(q);
            });
        }
        public static Props Prop(FunctionConfiguration configuration, IActorRef pulsarManager)
        {
            return Props.Create(() => new FunctionManager(configuration, pulsarManager));
        }
    }
}
