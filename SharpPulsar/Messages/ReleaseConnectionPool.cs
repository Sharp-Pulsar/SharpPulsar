
using Akka.Actor;

namespace SharpPulsar.Messages
{    
    public sealed class ReleaseConnectionPool
    {
        public IActorRef ClientCnx { get; }
        public ReleaseConnectionPool(IActorRef cnx)
        {
            ClientCnx = cnx;
        }
    }
}
