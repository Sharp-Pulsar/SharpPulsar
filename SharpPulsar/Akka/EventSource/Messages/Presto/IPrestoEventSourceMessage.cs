
using System.Collections.Immutable;
using SharpPulsar.Akka.Sql.Client;

namespace SharpPulsar.Akka.EventSource.Messages.Presto
{
    public interface IPrestoEventSourceMessage:IEventSourceMessage
    {
        public ClientOptions Options { get; }
        public ImmutableHashSet<string> Columns { get; }
    }
}
