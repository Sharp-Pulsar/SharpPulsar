using Akka.Actor;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Crypto;

namespace SharpPulsar.Batch
{
    public class ProducerContainer
    {
        public ProducerContainer(IActorRef producer, ProducerConfigurationData configuration, int maxMessageSize)
        {
            Producer = producer;
            Configuration = configuration;
            MaxMessageSize = maxMessageSize;
        }
        public long ProducerId { get; set; }
        public MessageCrypto Crypto { get; set; }
        public IActorRef Producer { get; }
        public ProducerConfigurationData Configuration { get; }
        public int MaxMessageSize { get; set; }
    }
}
