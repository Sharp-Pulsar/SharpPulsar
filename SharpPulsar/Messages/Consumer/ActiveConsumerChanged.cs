namespace SharpPulsar.Messages.Consumer
{
    public sealed class ActiveConsumerChanged
    {
        public bool IsActive { get; }
        public ActiveConsumerChanged(bool isActive)
        {
            IsActive = isActive;

        }
    }
}
