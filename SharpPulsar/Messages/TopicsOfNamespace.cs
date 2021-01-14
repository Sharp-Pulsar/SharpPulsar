using System.Collections.Generic;

namespace SharpPulsar.Messages
{
    public sealed class TopicsOfNamespace
    {
        public IList<string> Topics { get; }
        public TopicsOfNamespace(IList<string> topics)
        {
            Topics = topics;
        }
    }
}
