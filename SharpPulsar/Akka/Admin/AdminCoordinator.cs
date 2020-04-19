using Akka.Actor;
using Akka.Routing;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Admin
{
    public class AdminCoordinator:ReceiveActor
    {
        private readonly IActorRef _coordinator;
        public AdminCoordinator(string server)
        {
            _coordinator = Context.ActorOf(AdminWorker.Prop(server).WithRouter(new RoundRobinPool(10, new DefaultResizer(2, 10))),
                "AdminCoordinatorWorkerPool");
            Receive<QueryAdmin>(q => _coordinator.Tell(q));
        }
        public static Props Prop(string server)
        {
            return Props.Create(() => new AdminCoordinator(server));
        }
    }
}
