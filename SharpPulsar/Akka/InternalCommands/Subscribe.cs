using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class Subscribe<T>
    {
        public IConsumerBuilder<T> ConsumerBuilder { get; }
        public IMessageReceivedHandler Handler { get; }
        public ConsumerConfigurationData<T> Configurationon { get; }
        public Subscribe(IConsumerBuilder<T> consumerBuilder, ConsumerConfigurationData<T> conf, IMessageReceivedHandler handler)
        {
            ConsumerBuilder = consumerBuilder;
            Handler = handler;
            Configurationon = conf;
        }
    }
}
