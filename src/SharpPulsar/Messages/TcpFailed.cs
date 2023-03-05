namespace SharpPulsar.Messages
{
    public readonly record struct TcpFailed
    {
        public string Name { get; }

        public TcpFailed(string name)
        {
            Name = name;    
        }
    }
}
