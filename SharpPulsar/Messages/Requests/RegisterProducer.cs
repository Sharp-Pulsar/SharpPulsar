using System.Buffers;
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
    public sealed class MaxMessageSizeResponse
    {
        public int MessageSize { get; }
        public MaxMessageSizeResponse(int messageSize)
        {
            MessageSize = messageSize;
        }
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
    public sealed class RemoteEndpointProtocolVersionResponse
    {
        public int Version { get; }
        public RemoteEndpointProtocolVersionResponse(int version)
        {
            Version = version;
        }
    }
    public sealed class SendRequestWithId
    {
        public ReadOnlySequence<byte> Message { get; }
        public long RequestId { get; }
        public bool NeedsResponse { get; }
        public SendRequestWithId(ReadOnlySequence<byte> message,  long requestid, bool needsResponse = false)
        {
            Message = message;
            RequestId = requestid;
            NeedsResponse = needsResponse;
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
    public sealed class CleanupProducer
    {
        public IActorRef Producer { get; }
        public CleanupProducer(IActorRef producer)
        {
            Producer = producer;
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
    public sealed class RemoveTopicConsumer
    {
        public string Topic { get; }
        public RemoveTopicConsumer(string topic)
        {
            Topic = topic;
        }
    }
}
