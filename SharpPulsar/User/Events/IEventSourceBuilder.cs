
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Sql.Client;
using System.Collections.Generic;

namespace SharpPulsar.User.Events
{
    public interface IEventSourceBuilder
    {
       public ISourceBuilder<T> Reader<T>(ClientConfigurationData clientConfiguration, ReaderConfigBuilder<T> readerConfigBuilder, ISchema<T> schema);
       public ISourceBuilder<object> Sql(ClientOptions options, HashSet<string> selectedColumns);
    }
}
