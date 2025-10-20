using Akka.Actor;

namespace SharpPulsar.Messages
{
    public record GetTcClient
    {
        public static GetTcClient Instance = new GetTcClient();
    }
    public record TcClientOk
    {
        public static TcClientOk Instance = new TcClientOk();
    }
    public record TcClient
    {
        public IActorRef TCClient { get; }
        public TcClient(IActorRef tcClient)
        {
            TCClient = tcClient;
        }
    }
    public record SetTcClient(IActorRef TCClient);
}
