using System.Collections.Immutable;
using SharpPulsar.Api;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class Transactional
    {
        public Transactional(ImmutableList<object> messages, IPulsarClient client)
        {
            Messages = messages;
            Client = client;
        }

        public IPulsarClient Client { get; }
        public ImmutableList<object> Messages { get; }
    }
}
