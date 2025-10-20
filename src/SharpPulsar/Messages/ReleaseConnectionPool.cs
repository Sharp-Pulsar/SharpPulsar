
using Akka.Actor;

namespace SharpPulsar.Messages
{
    public record ReleaseConnectionPool
    {
        public IActorRef ClientCnx { get; }
        public ReleaseConnectionPool(IActorRef cnx)
        {
            ClientCnx = cnx;
        }
    }
}
