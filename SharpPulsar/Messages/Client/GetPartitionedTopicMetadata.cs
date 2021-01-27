using SharpPulsar.Common.Naming;
using System.Collections.Generic;
using System.Collections.Immutable;

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
    public sealed class GetPartitionsForTopic
    {
        public string TopicName { get; }
        public GetPartitionsForTopic(string topicName)
        {
            TopicName = topicName;
        }
    }
    public sealed class PartitionsForTopic
    {
        public ImmutableList<string> Topics { get; }
        public PartitionsForTopic(IList<string> topics)
        {
            Topics = topics.ToImmutableList();
        }
    }
}
