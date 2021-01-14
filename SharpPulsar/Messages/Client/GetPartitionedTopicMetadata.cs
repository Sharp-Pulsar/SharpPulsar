using SharpPulsar.Common.Naming;

namespace SharpPulsar.Messages.Client
{
    public sealed class GetPartitionedTopicMetadata
    {
        public TopicName TopicName { get; }
        public GetPartitionedTopicMetadata(TopicName topicName)
        {
            TopicName = topicName;
        }
    }
}
