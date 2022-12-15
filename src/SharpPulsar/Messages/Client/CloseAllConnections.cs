
namespace SharpPulsar.Messages.Client
{
    public record struct CloseAllConnections
    {
        public static CloseAllConnections Instance = new CloseAllConnections();
    }

}
