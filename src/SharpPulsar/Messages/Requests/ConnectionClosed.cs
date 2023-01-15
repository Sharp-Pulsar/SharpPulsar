
using Akka.Actor;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Requests
{
    public readonly record struct ConnectionClosed
    {
        public IActorRef ClientCnx { get; }
        public ConnectionClosed(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
    public readonly record struct ClearIncomingMessagesAndGetMessageNumber
    {
        public static ClearIncomingMessagesAndGetMessageNumber Instance = new ClearIncomingMessagesAndGetMessageNumber();
    }
    public readonly record struct IncomingMessagesCleared
    {
        public int Cleared { get; }
        public IncomingMessagesCleared(int cleared)
        {
            Cleared = cleared;
        }
    }
    public readonly record struct IncreaseAvailablePermits
    {
        public int Available { get; }
        public IncreaseAvailablePermits(int available)
        {
            Available = available;
        }
    }
    public readonly record struct IncreaseAvailablePermits<T>
    {
        public IMessage<T> Message { get; }
        public IncreaseAvailablePermits(IMessage<T> message)
        {
           Message = message;
        }
    }
    public readonly record struct ConnectionAlreadySet
    {
        public static ConnectionAlreadySet Instance = new ConnectionAlreadySet();
    }
    public readonly record struct Connect
    {
        public static Connect Instance = new Connect();
    }
    public readonly record struct ConnectionOpened
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
    public readonly record struct ConnectionFailed
    {
        public PulsarClientException Exception { get; }
        public ConnectionFailed(PulsarClientException exception)
        {
            Exception = exception;
        }
    }
}
