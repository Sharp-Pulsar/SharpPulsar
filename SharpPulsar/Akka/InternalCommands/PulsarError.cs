namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class PulsarError
    {
        public PulsarError(string message, string error)
        {
            Message = message;
            Error = error;
        }
        public string Error { get; }
        public string Message { get; }
        public bool ShouldRetry => Message == "org.apache.zookeeper.KeeperException$BadVersionException: KeeperErrorCode = BadVersion";
    }
}
