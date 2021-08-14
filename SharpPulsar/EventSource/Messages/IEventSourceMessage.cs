using SharpPulsar.Messages.Consumer;

namespace SharpPulsar.EventSource.Messages
{
    public interface IEventSourceMessage
    {
        public SourceType Source { get; }
        public string Tenant { get; }
        public string Namespace { get; }
        public string Topic { get; }
        public long FromSequenceId { get; } //Compute ledgerId and entryId for this 
        public long ToSequenceId { get; } //Compute ledgerId and entryId for this 
    }
}
