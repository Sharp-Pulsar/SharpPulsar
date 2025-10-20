namespace SharpPulsar.Messages.Producer
{
    public record Flush
    {
        public static Flush Instance = new Flush();
    }
}
