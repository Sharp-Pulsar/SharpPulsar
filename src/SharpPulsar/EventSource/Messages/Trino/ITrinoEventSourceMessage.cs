
using System.Collections.Generic;
using SharpPulsar.Trino;

namespace SharpPulsar.EventSource.Messages.Presto
{
    public interface ITrinoEventSourceMessage:IEventSourceMessage
    {
        public ClientOptions Options { get; }
        public HashSet<string> Columns { get; }
        public string AdminUrl { get; }
    }
}
