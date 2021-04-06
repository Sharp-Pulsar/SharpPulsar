
using SharpPulsar.Sql.Client;
using System.Collections.Immutable;

namespace SharpPulsar.EventSource.Messages.Presto
{
    public interface IPrestoEventSourceMessage:IEventSourceMessage
    {
        public ClientOptions Options { get; }
        public ImmutableHashSet<string> Columns { get; }
        public string AdminUrl { get; }
    }
}
