
using SharpPulsar.Sql.Client;
using System.Collections.Generic;

namespace SharpPulsar.EventSource.Messages.Presto
{
    public interface IPrestoEventSourceMessage:IEventSourceMessage
    {
        public ClientOptions Options { get; }
        public HashSet<string> Columns { get; }
        public string AdminUrl { get; }
    }
}
