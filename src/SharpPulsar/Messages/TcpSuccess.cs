namespace SharpPulsar.Messages
{
    public record TcpSuccess
    {
        public string Name { get; }

        public TcpSuccess(string name)
        {
            Name = name;
        }
    }
}
