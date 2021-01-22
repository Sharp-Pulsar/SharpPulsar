using Akka.Actor;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages.Requests
{
    public sealed class CommandSuccessResponse
    {
        public CommandSuccess Success { get; }
        public CommandSuccessResponse(CommandSuccess success)
        {
            Success = success;
        }
    }
    public sealed class MaxMessageSize
    {
        public static MaxMessageSize Instance = new MaxMessageSize();
    }
    public sealed class RegisterProducer
    {
        public long ProducerId { get; }
        public IActorRef Producer { get; }
        public RegisterProducer(long producerId, IActorRef producer)
        {
            Producer = producer;
            ProducerId = producerId;
        }
    }
    public sealed class RemoveProducer
    {
        public long ProducerId { get; }
        public RemoveProducer(long producerId)
        {
            ProducerId = producerId;
        }
    }
    public sealed class RemoteEndpointProtocolVersion
    {
        public static RemoteEndpointProtocolVersion Instance = new RemoteEndpointProtocolVersion();
    }
    public sealed class SendRequestWithId
    {
        public byte[] Message { get; }
        public long RequestId { get; }
        public SendRequestWithId(byte[] message,  long requestid)
        {
            Message = message;
            RequestId = requestid;
        }
    }
    public sealed class RegisterConsumer
    {
        public long ConsumerId { get; }
        public IActorRef Consumer { get; }
        public RegisterConsumer(long consumerId, IActorRef consumer)
        {
            ConsumerId = consumerId;
            Consumer = consumer;
        }
    }
    public sealed class CleanupConsumer
    {
        public IActorRef Consumer { get; }
        public CleanupConsumer(IActorRef consumer)
        {
            Consumer = consumer;
        }
    }
    public sealed class RemoveConsumer
    {
        public long ConsumerId { get; }
        public RemoveConsumer(long consumerId)
        {
            ConsumerId = consumerId;
        }
    }
}
