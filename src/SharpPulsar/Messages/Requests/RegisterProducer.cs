using System.Buffers;
using System.Collections.Immutable;
using Akka.Actor;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages.Requests
{
    public readonly record struct CommandSuccessResponse
    {
        public CommandSuccess Success { get; }
        public CommandSuccessResponse(CommandSuccess success)
        {
            Success = success;
        }
    }
    public readonly record struct CommandWatchTopicListSuccessResponse
    {
        public long WatcherId { get; }
        public string TopicsHash { get; }
        public ImmutableList<string> Topics { get; }
        public CommandWatchTopicListSuccessResponse(CommandWatchTopicListSuccess success)
        {
            WatcherId = (long)success.WatcherId;
            TopicsHash = success.TopicsHash;
            Topics = success.Topics.ToImmutableList();
        }
    }
    public readonly record struct CommandWatchTopicUpdateResponse
    {
        public long WatcherId { get; }
        public string TopicsHash { get; }
        public ImmutableList<string> NewTopics { get; }
        public ImmutableList<string> DeletedTopics { get; }
        public CommandWatchTopicUpdateResponse(CommandWatchTopicUpdate update)
        {
            WatcherId = (long)update.WatcherId;
            TopicsHash = update.TopicsHash;    
            NewTopics = update.NewTopics.ToImmutableList();
            DeletedTopics = update.DeletedTopics.ToImmutableList();
        }
    }


    public readonly record struct MaxMessageSize
    {
        public static MaxMessageSize Instance = new MaxMessageSize();
    }
    public readonly record struct MaxMessageSizeResponse
    {
        public int MessageSize { get; }
        public MaxMessageSizeResponse(int messageSize)
        {
            MessageSize = messageSize;
        }
    }
    public readonly record struct RegisterProducer
    {
        public long ProducerId { get; }
        public IActorRef Producer { get; }
        public RegisterProducer(long producerId, IActorRef producer)
        {
            Producer = producer;
            ProducerId = producerId;
        }
    }
    public readonly record struct RemoveProducer
    {
        public long ProducerId { get; }
        public RemoveProducer(long producerId)
        {
            ProducerId = producerId;
        }
    }
    public readonly record struct RemoteEndpointProtocolVersion
    {
        public static RemoteEndpointProtocolVersion Instance = new RemoteEndpointProtocolVersion();
    }
    public readonly record struct RemoteEndpointProtocolVersionResponse
    {
        public int Version { get; }
        public RemoteEndpointProtocolVersionResponse(int version)
        {
            Version = version;
        }
    }
    public readonly record struct SendRequestWithId
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
    public readonly record struct RegisterConsumer
    {
        public long ConsumerId { get; }
        public IActorRef Consumer { get; }
        public RegisterConsumer(long consumerId, IActorRef consumer)
        {
            ConsumerId = consumerId;
            Consumer = consumer;
        }
    }
    public readonly record struct CleanupProducer
    {
        public IActorRef Producer { get; }
        public CleanupProducer(IActorRef producer)
        {
            Producer = producer;
        }
    }
    public readonly record struct CleanupConsumer
    {
        public IActorRef Consumer { get; }
        public CleanupConsumer(IActorRef consumer)
        {
            Consumer = consumer;
        }
    }
    public readonly record struct RemoveConsumer
    {
        public long ConsumerId { get; }
        public RemoveConsumer(long consumerId)
        {
            ConsumerId = consumerId;
        }
    }
    public readonly record struct RemoveTopicConsumer
    {
        public string Topic { get; }
        public RemoveTopicConsumer(string topic)
        {
            Topic = topic;
        }
    }
}
