using System.Collections.Generic;
using System.Collections.Immutable;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.InternalCommands.Producer
{
    public sealed class CreateProducerBroadcastGroup
    {
        public CreateProducerBroadcastGroup(ISchema schema, HashSet<ProducerConfigurationData> producerConfigurations)
        {
            Schema = schema;
            ProducerConfigurations = producerConfigurations;
        }
        public ISchema Schema { get; }
        public HashSet<ProducerConfigurationData> ProducerConfigurations { get; }
    }
    public sealed class NewProducerBroadcastGroup
    {
        /// <summary>
    /// Use case: publish single message to multiple topics!
    /// Example: Food Ordered broadcast message to account, audit, chef, driver topics
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="configuration"></param>
    /// <param name="producerConfiguration"></param>
    /// <param name="topics"></param>
        public NewProducerBroadcastGroup(ISchema schema, ClientConfigurationData configuration, ImmutableHashSet<ProducerConfigurationData> producerConfigurations)
        {
            Schema = schema;
            Configuration = configuration;
            ProducerConfigurations = producerConfigurations;
        }

        public ISchema Schema { get; }
        public ClientConfigurationData Configuration { get; }
        public ImmutableHashSet<ProducerConfigurationData> ProducerConfigurations { get; }
    }
}
