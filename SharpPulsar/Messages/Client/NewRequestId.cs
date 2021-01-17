
namespace SharpPulsar.Messages.Client
{
    public sealed class NewRequestId
    {
        public static NewRequestId Instance = new NewRequestId();
    }
    public sealed class NewRequestIdResponse
    {
        public long RequestId { get; }
        public NewRequestIdResponse(long requestid)
        {
            RequestId = requestid;
        }
    }
}
