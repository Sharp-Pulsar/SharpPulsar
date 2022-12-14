using System.Collections.Immutable;

namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct UpdatePatternTopicsSubscription
    {
        public UpdatePatternTopicsSubscription(ImmutableHashSet<string> topics)
        {
            Topics = topics;
        }
        public ImmutableHashSet<string> Topics { get; }
    }
}
