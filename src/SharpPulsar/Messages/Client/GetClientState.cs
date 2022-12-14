
using Akka.Actor;

namespace SharpPulsar.Messages.Client
{
    public sealed class GetClientState
    {
        public static GetClientState Instance = new GetClientState();
    }
   
    public record struct SetLookUp(IActorRef Lookup);
    public record struct SetClient(IActorRef Client);
}
