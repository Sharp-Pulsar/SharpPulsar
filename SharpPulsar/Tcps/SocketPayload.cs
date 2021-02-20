using System.Buffers;

namespace SharpPulsar.Tcps
{
    public sealed class SocketPayload
    {
        public ReadOnlySequence<byte> Payload { get; }
        public SocketPayload(ReadOnlySequence<byte> payload)
        {
            Payload = payload;
        }
    }
}
