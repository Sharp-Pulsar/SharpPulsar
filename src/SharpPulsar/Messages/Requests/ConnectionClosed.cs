
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
    public sealed class IncomingMessagesCleared
    {
        public int Cleared { get; }
        public IncomingMessagesCleared(int cleared)
        {
            Cleared = cleared;
        }
    }
    public sealed class IncreaseAvailablePermits
    {
        public int Available { get; }
        public IncreaseAvailablePermits(int available)
        {
            Available = available;
        }
    }
    public sealed class ConnectionAlreadySet
    {
        public static ConnectionAlreadySet Instance = new ConnectionAlreadySet();
    }
    public sealed class Connect
    {
        public static Connect Instance = new Connect();
    }
    public sealed class ConnectionOpened
    {
        public IActorRef ClientCnx { get; }
        public long MaxMessageSize { get; }
        public int ProtocolVersion { get; }
        public ConnectionOpened(IActorRef clientCnx, long maxMessageSize, int protocolVersion)
        {
            ClientCnx = clientCnx;
            MaxMessageSize = maxMessageSize;
            ProtocolVersion = protocolVersion;
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
