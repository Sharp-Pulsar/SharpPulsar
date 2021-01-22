
using Akka.Actor;
using SharpPulsar.Exceptions;

namespace SharpPulsar.Messages.Requests
{
    public sealed class ConnectionClosed
    {
        public IActorRef ClientCnx { get; }
        public ConnectionClosed(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
    public sealed class ClearIncomingMessagesAndGetMessageNumber
    {
        public static ClearIncomingMessagesAndGetMessageNumber Instance = new ClearIncomingMessagesAndGetMessageNumber();
    }
    public sealed class IncreaseAvailablePermits
    {
        public static IncreaseAvailablePermits Instance = new IncreaseAvailablePermits();
    }
    public sealed class ConnectionOpened
    {
        public IActorRef ClientCnx { get; }
        public ConnectionOpened(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
    public sealed class ConnectionFailed
    {
        public PulsarClientException Exception { get; }
        public ConnectionFailed(PulsarClientException exception)
        {
            Exception = exception;
        }
    }
}
