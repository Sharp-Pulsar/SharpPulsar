
using Akka.Actor;

namespace SharpPulsar.Messages.Client
{
    public sealed class GetClientState
    {
        public static GetClientState Instance = new GetClientState();
    }
    public sealed class SetLookUp
    {
        public IActorRef Lookup { get; set; }
        public SetLookUp(IActorRef lookup)
        {
            Lookup = lookup;
        }
    }
    public sealed class SetClient
    {
        public IActorRef Client { get; set; }
        public SetClient(IActorRef client)
        {
            Client = client;
        }
    }
}
