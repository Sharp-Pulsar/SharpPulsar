
using Akka.Actor;

namespace SharpPulsar.Messages
{
    public readonly record struct ReleaseConnectionPool
    {
        public IActorRef ClientCnx { get; }
        public ReleaseConnectionPool(IActorRef cnx)
        {
            ClientCnx = cnx;
        }
    }
}
