namespace SharpPulsar.Impl.Internal.Interface
{
    public interface IConsumerProxy : IEnqueue<MessagePackage>, IDequeue<MessagePackage>
    {
        void Active();
        void Inactive();
        void Disconnected();
        void ReachedEndOfTopic();
    }
}
