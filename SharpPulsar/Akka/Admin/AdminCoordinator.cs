using Akka.Actor;
using Akka.Routing;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Admin
{
    public class AdminCoordinator:ReceiveActor
    {
        public AdminCoordinator(string server)
        {
            var coordinator = Context.ActorOf(AdminWorker.Prop(server).WithRouter(new RoundRobinPool(10, new DefaultResizer(2, 10))),
                "AdminCoordinatorWorkerPool");
            Receive((InternalCommands.Admin q) => coordinator.Tell(q));
        }
        public static Props Prop(string server)
        {
            return Props.Create(() => new AdminCoordinator(server));
        }
    }
}
