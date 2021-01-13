using System.Collections.Immutable;

namespace SharpPulsar.Messages
{
    public sealed class Transactional
    {
        public Transactional(ImmutableList<object> messages)
        {
            Messages = messages;
        }

        public ImmutableList<object> Messages { get; }
    }
}
