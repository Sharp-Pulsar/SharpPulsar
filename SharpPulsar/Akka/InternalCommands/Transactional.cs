using System.Collections.Immutable;
using SharpPulsar.Api;

namespace SharpPulsar.Akka.InternalCommands
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
