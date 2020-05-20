using Akka.Actor;
using Akka.Routing;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Producer
{
    public class BroadcastRouter : ReceiveActor
    {
        public BroadcastRouter(IActorRef router)
        {
            var router1 = router;
            Receive<Send>(s =>
            {
                router1.Tell(s);
            });

            Receive<BulkSend>(s =>
            {
                foreach (var m in s.Messages)
                {
                    router1.Tell(m);
                }
            });
        }

        public static Props Prop(IActorRef router)
        {
            return Props.Create(()=> new BroadcastRouter(router));
        }
    }
}
