namespace SharpPulsar.Messages.Producer
{
    public readonly record struct Flush
    {
        public static Flush Instance = new Flush();
    }
}
