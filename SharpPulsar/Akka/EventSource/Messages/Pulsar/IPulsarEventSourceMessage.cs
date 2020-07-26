using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.EventSource.Messages.Pulsar
{
    public interface IPulsarEventSourceMessage: IEventSourceMessage
    {
        public string AdminUrl { get; }
        public ReaderConfigurationData Configuration { get; }
        public ClientConfigurationData ClientConfiguration { get; }
    }
}
