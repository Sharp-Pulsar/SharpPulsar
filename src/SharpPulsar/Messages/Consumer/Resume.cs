namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct Resume
    {
        public static Resume Instance = new Resume();
    }
}
