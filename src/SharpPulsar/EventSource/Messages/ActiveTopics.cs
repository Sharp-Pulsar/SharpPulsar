using System.Collections.Immutable;

namespace SharpPulsar.EventSource.Messages
{
    public sealed class ActiveTopics
    {
        public ActiveTopics(string ns, ImmutableList<string> topics)
        {
            Namespace = ns;
            Topics = topics;
        }

        public string Namespace { get; }
        public ImmutableList<string> Topics { get; }
    }
}
