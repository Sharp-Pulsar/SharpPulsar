using Akka.Actor;

namespace SharpPulsar.Messages
{
    public sealed class GetTcClient
    {
        public static GetTcClient Instance = new GetTcClient();
    }
    public sealed class TcClientOk
    {
        public static TcClientOk Instance = new TcClientOk();
    }
    public sealed class TcClient
    {
        public IActorRef TCClient { get; }
        public TcClient(IActorRef tcClient)
        {
            TCClient = tcClient;
        }
    }
}
