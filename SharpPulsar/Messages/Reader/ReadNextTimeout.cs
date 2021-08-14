
using System;

namespace SharpPulsar.Messages.Reader
{
    public sealed class ReadNextTimeout
    {
        public long Timeout { get; } 
        public ReadNextTimeout(TimeSpan timeout)
        {
            Timeout = (long)timeout.TotalSeconds;
        }
    }
}
