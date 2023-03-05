namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct HasMessageAvailable
    {
        public static HasMessageAvailable Instance = new HasMessageAvailable();
    }
}
