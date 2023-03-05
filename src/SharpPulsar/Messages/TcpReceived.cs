namespace SharpPulsar.Messages
{
    public readonly record struct TcpReceived
    {
        public byte[] Bytes { get; }

        public TcpReceived(byte[] bytes)
        {
            Bytes = bytes;  
        }
    }
}
