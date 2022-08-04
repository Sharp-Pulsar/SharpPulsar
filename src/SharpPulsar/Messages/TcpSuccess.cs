namespace SharpPulsar.Messages
{
    public sealed class TcpSuccess
    {
        public string Name { get; }

        public TcpSuccess(string name)
        {
            Name = name;
        }
    }
}
