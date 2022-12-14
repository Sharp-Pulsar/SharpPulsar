
using SharpPulsar.Common.Naming;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpPulsar.Messages.Client
{
    public record struct GetPartitionedTopicMetadata(TopicName TopicName);
    public record struct GetPartitionsForTopic(string TopicName);
    public record struct PartitionsForTopic(IList<string> topics)
    {
        public ImmutableList<string> Topics => topics.ToImmutableList();
    }
      
}
