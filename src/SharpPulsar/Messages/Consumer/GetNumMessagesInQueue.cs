namespace SharpPulsar.Messages.Consumer
{
    public record GetNumMessagesInQueue
    {
        public static GetNumMessagesInQueue Instance = new GetNumMessagesInQueue();
    }
}
