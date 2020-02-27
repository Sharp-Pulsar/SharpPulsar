using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Producer
{
    public class ProducerManager:ReceiveActor
    {
        public ProducerManager()
        {
            Receive<object>(create =>
            {

            });
            Receive<Send>(send =>
            {
                //look for the child actor with name equal topic and send
            });
            Receive<Transactional>(send =>
            {
                //create transaction object
                //for each message produce
                //track messageid to commit transaction
            });
        }

        public static Props Prop()
        {
            return Props.Create(()=> new ProducerManager());
        }
    }
}
