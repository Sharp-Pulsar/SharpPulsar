using System.Collections.Generic;
using System.Collections.Immutable;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Messages.Producer
{
    public sealed class CreateProducerBroadcastGroup
    {
        public CreateProducerBroadcastGroup(ISchema schema, HashSet<ProducerConfigurationData> producerConfigurations, string title)
        {
            Schema = schema;
            ProducerConfigurations = producerConfigurations;
            Title = title;
        }
        /// <summary>
        /// Group name/title
        /// </summary>
        public string Title { get; }
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
    /// <param name="producerConfigurations"></param>
    /// <param name="title">Internally used by PulsarSharp to prevent duplication</param>
        public NewProducerBroadcastGroup(ISchema schema, ClientConfigurationData configuration, ImmutableHashSet<ProducerConfigurationData> producerConfigurations, string title)
        {
            Schema = schema;
            Configuration = configuration;
            ProducerConfigurations = producerConfigurations;
            Title = title;
        }
        public string Title { get; }
        public ISchema Schema { get; }
        public ClientConfigurationData Configuration { get; }
        public ImmutableHashSet<ProducerConfigurationData> ProducerConfigurations { get; }
    }
}
