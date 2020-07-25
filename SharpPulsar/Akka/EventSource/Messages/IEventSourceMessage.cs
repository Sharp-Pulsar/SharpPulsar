using SharpPulsar.Akka.InternalCommands.Consumer;

namespace SharpPulsar.Akka.EventSource.Messages
{
    public interface IEventSourceMessage
    {
        public SourceType Source { get; }
        public string Tenant { get; }
        public string Namespace { get; }
        public string Topic { get; }
    }
}
