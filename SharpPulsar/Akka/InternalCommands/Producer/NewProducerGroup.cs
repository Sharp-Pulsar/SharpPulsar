using System.Collections.Generic;
using System.Collections.Immutable;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.InternalCommands.Producer
{
    public sealed class CreateProducerBroadcastGroup
    {
        public CreateProducerBroadcastGroup(ISchema schema, ProducerConfigurationData producerConfiguration, HashSet<string> topics)
        {
            Schema = schema;
            ProducerConfiguration = producerConfiguration;
            Topics = topics;
        }
        public ISchema Schema { get; }
        public ProducerConfigurationData ProducerConfiguration { get; }
        public HashSet<string> Topics { get; }
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
        public NewProducerBroadcastGroup(ISchema schema, ClientConfigurationData configuration, ProducerConfigurationData producerConfiguration, ImmutableHashSet<string> topics)
        {
            Schema = schema;
            Configuration = configuration;
            ProducerConfiguration = producerConfiguration;
            Topics = topics;
        }

        public ISchema Schema { get; }
        public ClientConfigurationData Configuration { get; }
        public ProducerConfigurationData ProducerConfiguration { get; }
        public ImmutableHashSet<string> Topics { get; }
    }
}
