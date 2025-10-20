namespace SharpPulsar.Messages.Consumer
{
    public record HasMessageAvailable
    {
        public static HasMessageAvailable Instance = new HasMessageAvailable();
    }
}
