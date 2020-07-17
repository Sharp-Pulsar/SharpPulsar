using Akka.Actor;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Batch
{
    public class ProducerContainer
    {
        public ProducerContainer(IActorRef producer, ProducerConfigurationData configuration, int maxMessageSize, ActorSystem system)
        {
            Producer = producer;
            Configuration = configuration;
            MaxMessageSize = maxMessageSize;
            System = system;
        }
        public string ProducerName { get; set; }
        public ActorSystem System { get;}
        public long ProducerId { get; set; }
        public IMessageCrypto Crypto { get; set; }
        public IActorRef Producer { get; }
        public ProducerConfigurationData Configuration { get; }
        public int MaxMessageSize { get; set; }
    }
}
