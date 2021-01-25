
namespace SharpPulsar.Messages.Client
{
    public sealed class NewRequestId
    {
        public static NewRequestId Instance = new NewRequestId();
    }
    public sealed class NewRequestIdResponse
    {
        public long Id { get; }
        public NewRequestIdResponse(long requestid)
        {
            Id = requestid;
        }
    }
}
