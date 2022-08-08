namespace SharpPulsar.Messages
{
    public class TcpReceived
    {
        public byte[] Bytes { get; }

        public TcpReceived(byte[] bytes)
        {
            Bytes = bytes;  
        }
    }
}
