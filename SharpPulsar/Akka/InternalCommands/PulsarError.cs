using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class PulsarError
    {
        public PulsarError(string message)
        {
            Message = message;
        }

        public string Message { get; }
        public bool ShouldRetry => Message == "org.apache.zookeeper.KeeperException$BadVersionException: KeeperErrorCode = BadVersion";
    }
}
