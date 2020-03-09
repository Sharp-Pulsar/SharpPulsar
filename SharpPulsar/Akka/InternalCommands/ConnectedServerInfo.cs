
namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class ConnectedServerInfo
    {
        public ConnectedServerInfo(int maxMessageSize, int protocol, string version, string name)
        {
            MaxMessageSize = maxMessageSize;
            Protocol = protocol;
            Version = version;
            Name = name;
        }

        public int MaxMessageSize { get; }
        public int Protocol { get; }
        public string Version { get; }
        public string Name { get; }//host name
    }
}
