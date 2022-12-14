using Akka.Actor;

namespace SharpPulsar.Messages
{
    public readonly record struct GetTcClient
    {
        public static GetTcClient Instance = new GetTcClient();
    }
    public readonly record struct TcClientOk
    {
        public static TcClientOk Instance = new TcClientOk();
    }
    public readonly record struct TcClient
    {
        public IActorRef TCClient { get; }
        public TcClient(IActorRef tcClient)
        {
            TCClient = tcClient;
        }
    }
}
