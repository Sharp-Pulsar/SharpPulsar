using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages
{
    public readonly record struct GetTopicsOfNamespaceResponse
    {
        public CommandGetTopicsOfNamespaceResponse Response { get; }
        public GetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse response)
        {
            Response = response;
        }
    }
}
