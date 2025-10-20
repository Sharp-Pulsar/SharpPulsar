namespace SharpPulsar.Messages
{
    public record TcpReceived
    {
        public byte[] Bytes { get; }

        public TcpReceived(byte[] bytes)
        {
            Bytes = bytes;  
        }
    }
}
