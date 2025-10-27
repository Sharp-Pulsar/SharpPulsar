using Pulsar.Proto;

namespace SharpPulsar.Messages
{
    public record GetTopicsOfNamespaceResponse
    {
        public CommandGetTopicsOfNamespaceResponse Response { get; }
        public GetTopicsOfNamespaceResponse(CommandGetTopicsOfNamespaceResponse response)
        {
            Response = response;
        }
    }
}
