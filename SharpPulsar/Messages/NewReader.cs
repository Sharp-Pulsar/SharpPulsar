using SharpPulsar.Messages.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Configuration;

namespace SharpPulsar.Messages
{
    public sealed class CreateReader
    {
        public CreateReader(ISchema schema, ReaderConfigurationData readerConfiguration, Seek seek = null)
        {
            Schema = schema;
            ReaderConfiguration = readerConfiguration;
            Seek = seek;
        }
        public Seek Seek { get; }
        public ISchema Schema { get; }
        public ReaderConfigurationData ReaderConfiguration { get; }
    }
    internal sealed class NewReader
    {
        public NewReader(ISchema schema, ClientConfigurationData configuration, ReaderConfigurationData readerConfiguration, Seek seek)
        {
            Schema = schema;
            Configuration = configuration;
            ReaderConfiguration = readerConfiguration;
            Seek = seek;
        }
        public ISchema Schema { get; }
        public Seek Seek { get; }
        public ClientConfigurationData Configuration { get; }
        public ReaderConfigurationData ReaderConfiguration { get; }
    }
}
