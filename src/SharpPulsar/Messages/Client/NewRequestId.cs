
namespace SharpPulsar.Messages.Client
{
    public sealed class NewRequestId
    {
        public static NewRequestId Instance = new NewRequestId();
    }
    public record struct NewRequestIdResponse(long Id);
}
