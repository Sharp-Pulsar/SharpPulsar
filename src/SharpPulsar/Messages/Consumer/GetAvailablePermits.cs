namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct GetAvailablePermits
    {
        public static GetAvailablePermits Instance = new GetAvailablePermits();
    }
}
