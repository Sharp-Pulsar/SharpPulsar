using Akka.Actor;

namespace SharpPulsar.Akka.Producer
{
    public class Producer: ReceiveActor
    {
        private IActorRef _network;

        public Producer()
        {
            
        }
    }
}
