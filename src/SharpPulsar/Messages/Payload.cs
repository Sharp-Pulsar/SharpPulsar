using SharpPulsar.Shared.Buf;

namespace SharpPulsar.Messages
{
    public readonly record struct Payload
    {
        public ByteBuf Bytes { get; }
        public long RequestId { get; }
        public string Command { get; }
        public Payload(ByteBuf bytes, long requestId, string command)
        {
            Bytes = bytes;
            RequestId = requestId;
            Command = command;
        }
        
    }
}
