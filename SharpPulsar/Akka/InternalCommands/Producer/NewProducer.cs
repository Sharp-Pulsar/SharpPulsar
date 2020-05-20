using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.InternalCommands.Producer
{
    public sealed class CreateProducer
    {
        public CreateProducer(ISchema schema, ProducerConfigurationData producerConfiguration)
        {
            Schema = schema;
            ProducerConfiguration = producerConfiguration;
        }
        public ISchema Schema { get; }
        public ProducerConfigurationData ProducerConfiguration { get; }
    }
    internal sealed class NewProducer
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
    internal sealed class NewProducerGroupMember
    {
        public NewProducerGroupMember(ISchema schema, ClientConfigurationData configuration, ProducerConfigurationData producerConfiguration)
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
