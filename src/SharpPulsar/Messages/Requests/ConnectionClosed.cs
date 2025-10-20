
using Akka.Actor;
using SharpPulsar.API;
using SharpPulsar.Shared.Exceptions;

namespace SharpPulsar.Messages.Requests
{
    public record ConnectionClosed
    {
        public IActorRef ClientCnx { get; }
        public ConnectionClosed(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
    public record ClearIncomingMessagesAndGetMessageNumber
    {
        public static ClearIncomingMessagesAndGetMessageNumber Instance = new ClearIncomingMessagesAndGetMessageNumber();
    }
    public record IncomingMessagesCleared
    {
        public int Cleared { get; }
        public IncomingMessagesCleared(int cleared)
        {
            Cleared = cleared;
        }
    }
    public record IncreaseAvailablePermits
    {
        public int Available { get; }
        public IncreaseAvailablePermits(int available)
        {
            Available = available;
        }
    }
    public record IncreaseAvailablePermits<T>
    {
        public IMessage<T> Message { get; }
        public IncreaseAvailablePermits(IMessage<T> message)
        {
           Message = message;
        }
    }
    public record ConnectionAlreadySet(IActorRef ClientCnx);
    public record Connect
    {
        public static Connect Instance = new Connect();
    }
    public record ConnectionOpened
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
    public record ConnectionFailed
    {
        public PulsarClientException Exception { get; }
        public ConnectionFailed(PulsarClientException exception)
        {
            Exception = exception;
        }
    }
}
