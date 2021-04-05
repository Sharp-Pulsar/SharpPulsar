
using SharpPulsar.Configuration;
using SharpPulsar.Sql.Client;
using System.Collections.Generic;

namespace SharpPulsar.User.Events
{
    public interface IEventSourceBuilder
    {
       public ISourceBuilder Reader<T>(ReaderConfigBuilder<T> readerConfigBuilder);
       public ISourceBuilder Sql(ClientOptions options, HashSet<string> selectedColumns);
    }
}
