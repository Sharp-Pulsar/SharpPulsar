namespace SharpPulsar.Messages
{
    public readonly record struct TcpSuccess
    {
        public string Name { get; }

        public TcpSuccess(string name)
        {
            Name = name;
        }
    }
}
