namespace SharpPulsar.Messages
{
    public class Payload
    {
        public byte[] Bytes { get; }
        public long RequestId { get; }
        public string Command { get; }
        public Payload(byte[] bytes, long requestId, string command)
        {
            Bytes = bytes;
            RequestId = requestId;
            Command = command;
        }
        
    }
}
