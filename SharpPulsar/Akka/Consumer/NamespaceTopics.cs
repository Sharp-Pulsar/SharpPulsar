using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpPulsar.Akka.Consumer
{
    public class NamespaceTopics
    {
        public NamespaceTopics(long requestId, List<string> topics)
        {
            RequestId = requestId;
            Topics = topics.ToImmutableList();
        }

        public long RequestId { get; }
        public ImmutableList<string> Topics { get; }
    }
}
