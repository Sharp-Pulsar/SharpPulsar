using Akka.Actor;

namespace SharpPulsar.Akka.Transaction
{
    public class TransactionManager:ReceiveActor
    {
        public TransactionManager()
        {
                
        }
        public static Props Prop()
        {
            return Props.Create(() => new TransactionManager());
        }
    }
}
