
using Akka.Actor;

namespace SharpPulsar.Messages.Client
{
    public record struct GetClientState
    {
        public static GetClientState Instance = new GetClientState();
    }
   
    public record struct SetLookUp(IActorRef Lookup);
    public record struct SetClient(IActorRef Client);
}
