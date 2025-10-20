namespace SharpPulsar.Messages
{
    public record TcpFailed
    {
        public string Name { get; }

        public TcpFailed(string name)
        {
            Name = name;    
        }
    }
}
