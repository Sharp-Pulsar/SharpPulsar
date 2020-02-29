using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.InternalCommands.Consumer
{
    public sealed class CreateConsumer
    {
        public CreateConsumer(ISchema schema, ConsumerConfigurationData consumerConfiguration, ConsumerType consumerType)
        {
            Schema = schema;
            ConsumerConfiguration = consumerConfiguration;
            ConsumerType = consumerType;
        }
        public ConsumerType ConsumerType { get; }
        public ISchema Schema { get; }
        public ConsumerConfigurationData ConsumerConfiguration { get; }
    }
    internal sealed class NewConsumer
    {
        public NewConsumer(ISchema schema, ClientConfigurationData configuration, ConsumerConfigurationData consumerConfiguration, ConsumerType consumerType)
        {
            Schema = schema;
            Configuration = configuration;
            ConsumerConfiguration = consumerConfiguration; 
            ConsumerType = consumerType;
        }
        public ConsumerType ConsumerType { get; }
        public ISchema Schema { get; }
        public ClientConfigurationData Configuration { get; }
        public ConsumerConfigurationData ConsumerConfiguration { get; }
    }

    public enum ConsumerType
    {
        Single,
        Multi,
        Partitioned
    }
}
