namespace SharpPulsar.Akka.InternalCommands
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
