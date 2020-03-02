using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class CreateReader
    {
        public CreateReader(ISchema schema, ReaderConfigurationData readerConfiguration)
        {
            Schema = schema;
            ReaderConfiguration = readerConfiguration;
        }
        public ISchema Schema { get; }
        public ReaderConfigurationData ReaderConfiguration { get; }
    }
    internal sealed class NewReader
    {
        public NewReader(ISchema schema, ClientConfigurationData configuration, ReaderConfigurationData readerConfiguration)
        {
            Schema = schema;
            Configuration = configuration;
            ReaderConfiguration = readerConfiguration;
        }
        public ConsumerType ConsumerType { get; }
        public ISchema Schema { get; }
        public ClientConfigurationData Configuration { get; }
        public ReaderConfigurationData ReaderConfiguration { get; }
    }
}
