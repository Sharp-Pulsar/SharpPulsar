using System.Buffers;

namespace SharpPulsar.Messages
{
    public readonly record struct Payload
    {
        public ReadOnlySequence<byte> Bytes { get; }
        public long RequestId { get; }
        public string Command { get; }
        public Payload(ReadOnlySequence<byte> bytes, long requestId, string command)
        {
            Bytes = bytes;
            RequestId = requestId;
            Command = command;
        }
        
    }
}
