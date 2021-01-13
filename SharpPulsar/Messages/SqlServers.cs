using System.Collections.Immutable;

namespace SharpPulsar.Messages
{
    public sealed class SqlServers
    {
        public SqlServers(ImmutableList<string> servers)
        {
            Servers = servers;
        }

        public ImmutableList<string> Servers { get; }
    }
}
