namespace SharpPulsar.Akka.InternalCommands
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
