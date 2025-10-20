using DotNetty.Buffers;
namespace SharpPulsar.Messages
{
    public record Payload
    {
        public AbstractByteBuffer Bytes { get; }
        public long RequestId { get; }
        public string Command { get; }
        public Payload(AbstractByteBuffer bytes, long requestId, string command)
        {
            Bytes = bytes;
            RequestId = requestId;
            Command = command;
        }
        
    }
}
