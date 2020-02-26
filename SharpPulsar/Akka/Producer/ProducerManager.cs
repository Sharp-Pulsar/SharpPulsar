using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Producer
{
    public class ProducerManager<T>:ReceiveActor
    {
        private IProducerBuilder<T> _producerBuilder;
        private ProducerConfigurationData _configuration;

        public ProducerManager(IProducerBuilder<T> producerBuilder, ProducerConfigurationData configuration)
        {
            _producerBuilder = producerBuilder;
            _configuration = configuration;
        }

        public static Props Prop(IProducerBuilder<T> producerBuilder, ProducerConfigurationData configuration)
        {
            return Props.Create(()=> new ProducerManager<T>(producerBuilder, configuration));
        }
    }
}
