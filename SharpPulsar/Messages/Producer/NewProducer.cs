using SharpPulsar.Api;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Producer
{
    public sealed class NewProducerActor<T>
    {
        public ISchema<T> Schema { get; }
        public string Topic { get; }
        public bool EnableBatching { get; }
        public bool BlockIfQueueFull { get; }
        public NewProducerActor(ISchema<T> schema, string topic, bool enableBatching, bool blockIfQueueFull)
        {
            Schema = schema;
            Topic = topic;
            EnableBatching = enableBatching;
            BlockIfQueueFull = blockIfQueueFull;
        }
    }
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
    internal sealed class NewProducer<T>
    {
        public NewProducer(ISchema<T> schema, ClientConfigurationData configuration, ProducerConfigurationData producerConfiguration)
        {
            Schema = schema;
            Configuration = configuration;
            ProducerConfiguration = producerConfiguration;
        }
        
        public ISchema<T> Schema { get; }
        public ClientConfigurationData Configuration { get; }
        public ProducerConfigurationData ProducerConfiguration { get; }
    }
    internal sealed class NewProducerGroupMember
    {
        public NewProducerGroupMember(ISchema schema, ClientConfigurationData configuration, ProducerConfigurationData producerConfiguration, string title)
        {
            Schema = schema;
            Configuration = configuration;
            ProducerConfiguration = producerConfiguration;
            Title = title;
        }

        public ISchema Schema { get; }
        public ClientConfigurationData Configuration { get; }
        public ProducerConfigurationData ProducerConfiguration { get; }
        public string Title { get; }
    }
}
