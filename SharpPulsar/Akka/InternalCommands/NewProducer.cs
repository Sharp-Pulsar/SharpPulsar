using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class NewProducer
    {
        public NewProducer(ISchema schema, ClientConfigurationData configuration, ProducerConfigurationData producerConfiguration)
        {
            Schema = schema;
            Configuration = configuration;
            ProducerConfiguration = producerConfiguration;
        }

        public ISchema Schema { get; }
        public ClientConfigurationData Configuration { get; }
        public ProducerConfigurationData ProducerConfiguration { get; }
    }
}
