using System.Collections.Immutable;

namespace SharpPulsar.Messages.Consumer
{
    public record UpdatePatternTopicsSubscription
    {
        public UpdatePatternTopicsSubscription(ImmutableHashSet<string> topics)
        {
            Topics = topics;
        }
        public ImmutableHashSet<string> Topics { get; }
    }
}
