namespace SharpPulsar.Messages
{
    public sealed class TcpFailed
    {
        public string Name { get; }

        public TcpFailed(string name)
        {
            Name = name;    
        }
    }
}
