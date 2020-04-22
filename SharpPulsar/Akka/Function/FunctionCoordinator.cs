using Akka.Actor;
using Akka.Routing;
namespace SharpPulsar.Akka.Function
{
    public class FunctionCoordinator : ReceiveActor
    {
        public FunctionCoordinator(string server)
        {
            var coordinator = Context.ActorOf(FunctionWorker.Prop(server).WithRouter(new RoundRobinPool(10, new DefaultResizer(2, 10))),
                "FunctionCoordinatorWorkerPool");
            Receive((InternalCommands.Function q) => coordinator.Tell(q));
        }
        public static Props Prop(string server)
        {
            return Props.Create(() => new FunctionCoordinator(server));
        }
    }
}
