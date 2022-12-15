
namespace SharpPulsar.Messages.Client
{
    public record struct NewRequestId
    {
        public static NewRequestId Instance = new NewRequestId();
    }
    public record struct NewRequestIdResponse(long Id);
}
