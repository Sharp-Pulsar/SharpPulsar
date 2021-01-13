
namespace SharpPulsar.Messages.Consumer
{
    public sealed class ActiveConsumerChanged
    {
        public bool IsActive { get; }
        public ActiveConsumerChanged(bool isactive)
        {
            IsActive = isactive;
        }
    }
}
