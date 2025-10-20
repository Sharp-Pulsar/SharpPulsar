
namespace SharpPulsar.Messages.Consumer
{
    public record GetIncomingMessageSize
    {
        public static GetIncomingMessageSize Instance = new GetIncomingMessageSize();
    }
    public record GetIncomingMessageCount
    {
        public static GetIncomingMessageCount Instance = new GetIncomingMessageCount();
    }
}
