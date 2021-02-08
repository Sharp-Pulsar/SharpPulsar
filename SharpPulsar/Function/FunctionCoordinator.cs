using Akka.Actor;
using Akka.Routing;

namespace SharpPulsar.Function
{
    public class FunctionCoordinator : ReceiveActor
    {
        public FunctionCoordinator(string server, IActorRef pulsarManager)
        {
            var coordinator = Context.ActorOf(FunctionWorker.Prop(server, pulsarManager).WithRouter(new RoundRobinPool(10, new DefaultResizer(2, 10))),
                "FunctionCoordinatorWorkerPool");
            Receive((Messages.Function q) => coordinator.Tell(q));
        }
        public static Props Prop(string server, IActorRef pulsarManager)
        {
            return Props.Create(() => new FunctionCoordinator(server, pulsarManager));
        }
    }
}
