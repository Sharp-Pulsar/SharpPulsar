namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct GetNumMessagesInQueue
    {
        public static GetNumMessagesInQueue Instance = new GetNumMessagesInQueue();
    }
}
