namespace SharpPulsar.Messages.Consumer
{
    public record GetAvailablePermits
    {
        public static GetAvailablePermits Instance = new GetAvailablePermits();
    }
}
