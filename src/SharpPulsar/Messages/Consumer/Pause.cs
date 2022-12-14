namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct Pause
    {
        public static Pause Instance = new Pause();
    }
}
