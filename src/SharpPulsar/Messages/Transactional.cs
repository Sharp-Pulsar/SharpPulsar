using System.Collections.Immutable;

namespace SharpPulsar.Messages
{
    public readonly record struct Transactional
    {
        public Transactional(ImmutableList<object> messages)
        {
            Messages = messages;
        }

        public ImmutableList<object> Messages { get; }
    }
}
