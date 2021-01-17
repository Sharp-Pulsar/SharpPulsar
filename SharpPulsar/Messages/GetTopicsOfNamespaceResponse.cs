using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages
{
    public sealed class GetTopicsOfNamespaceResponse
    {
        public CommandGetTopicsOfNamespaceResponse Response { get; }
        public GetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse response)
        {
            Response = response;
        }
    }
}
