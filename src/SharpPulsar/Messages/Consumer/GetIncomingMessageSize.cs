
namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct GetIncomingMessageSize
    {
        public static GetIncomingMessageSize Instance = new GetIncomingMessageSize();
    }
    public readonly record struct GetIncomingMessageCount
    {
        public static GetIncomingMessageCount Instance = new GetIncomingMessageCount();
    }
}
