using System;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class Create<T>
    {
        public IProducerBuilder<T> ProducerBuilder { get; }
        public ProducerConfigurationData Configuration { get; }

        public Create(IProducerBuilder<T> producerBuilder, ProducerConfigurationData configuration)
        {
            ProducerBuilder = producerBuilder;
            Configuration = configuration;
        }
    }
}
